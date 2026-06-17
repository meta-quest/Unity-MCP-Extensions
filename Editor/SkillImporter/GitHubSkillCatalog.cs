/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Meta.XR.MCP.Extension.Editor
{
    /// <summary>
    /// Discovers and downloads skills from the meta-quest/agentic-tools GitHub
    /// repository.
    ///
    /// The catalog is built from the repository's source archive (a single zip
    /// downloaded from codeload.github.com). This needs only one request, is not
    /// subject to the GitHub REST API rate limit, and does not depend on
    /// api.github.com (which is unreachable from some corporate networks). The
    /// archive contains every skill file, so importing later requires no further
    /// network access.
    /// </summary>
    internal static class GitHubSkillCatalog
    {
        private const string Owner = "meta-quest";
        private const string Repo = "agentic-tools";
        private const string Branch = "main";

        // Folder inside the repository that holds the skills.
        private const string SkillsFolder = "skills";

        private const string UserAgent = "Meta-XR-Unity-MCP-Extension";

        // Abort the archive download if the server stops responding, so the importer never
        // hangs indefinitely (which would otherwise leave the window stuck in its busy state).
        private const int DownloadTimeoutSeconds = 30;

        private static string ArchiveUrl =>
            $"https://codeload.github.com/{Owner}/{Repo}/zip/refs/heads/{Branch}";

        /// <summary>
        /// Downloads the repository archive and returns every skill it contains, with
        /// each skill's files (keyed by skill-relative path) and the name/description
        /// parsed from its SKILL.md manifest.
        /// </summary>
        public static async Task<List<SkillCatalogEntry>> FetchCatalogAsync()
        {
            var archive = await DownloadArchiveAsync();

            var entries = new Dictionary<string, SkillCatalogEntry>(StringComparer.Ordinal);

            using (var stream = new MemoryStream(archive))
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                foreach (var zipEntry in zip.Entries)
                {
                    // Directory entries have an empty Name; skip them.
                    if (string.IsNullOrEmpty(zipEntry.Name))
                    {
                        continue;
                    }

                    // Archive paths look like "<repo>-<branch>/skills/<skillId>/<relative...>".
                    var parts = zipEntry.FullName.Split('/');
                    if (parts.Length < 4 || !string.Equals(parts[1], SkillsFolder, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var skillId = parts[2];
                    var relativePath = string.Join("/", parts, 3, parts.Length - 3);
                    if (relativePath.Length == 0)
                    {
                        continue;
                    }

                    if (!entries.TryGetValue(skillId, out var entry))
                    {
                        entry = new SkillCatalogEntry(skillId);
                        entries[skillId] = entry;
                    }

                    entry.Files[relativePath] = ReadAllBytes(zipEntry);
                }
            }

            var result = new List<SkillCatalogEntry>(entries.Values);
            result.Sort((a, b) => string.Compare(a.Id, b.Id, StringComparison.OrdinalIgnoreCase));

            foreach (var entry in result)
            {
                if (entry.Files.TryGetValue(SkillManifest.FileName, out var manifestBytes))
                {
                    SkillManifest.Read(
                        Encoding.UTF8.GetString(manifestBytes),
                        out var name,
                        out var description);
                    if (!string.IsNullOrEmpty(name))
                    {
                        entry.DisplayName = name;
                    }

                    if (!string.IsNullOrEmpty(description))
                    {
                        entry.Description = description;
                    }
                }
            }

            return result;
        }

        private static byte[] ReadAllBytes(ZipArchiveEntry zipEntry)
        {
            using var entryStream = zipEntry.Open();
            using var buffer = new MemoryStream();
            entryStream.CopyTo(buffer);
            return buffer.ToArray();
        }

        private static async Task<byte[]> DownloadArchiveAsync()
        {
            using var request = new UnityWebRequest(ArchiveUrl, UnityWebRequest.kHttpVerbGET)
            {
                downloadHandler = new DownloadHandlerBuffer(),
                timeout = DownloadTimeoutSeconds,
            };
            request.SetRequestHeader("User-Agent", UserAgent);

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new Exception(
                    $"Failed to download skills archive from '{ArchiveUrl}' " +
                    $"({request.responseCode}): {request.error}");
            }

            return request.downloadHandler.data;
        }
    }
}
