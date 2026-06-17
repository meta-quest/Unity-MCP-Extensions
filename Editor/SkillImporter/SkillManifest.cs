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

namespace Meta.XR.MCP.Extension.Editor
{
    /// <summary>
    /// Helpers for reading the YAML front matter of a SKILL.md manifest.
    /// </summary>
    internal static class SkillManifest
    {
        public const string FileName = "SKILL.md";

        // Maximum number of leading lines scanned for front-matter keys.
        private const int MaxFrontMatterLines = 40;

        /// <summary>
        /// Reads the <c>name</c> and <c>description</c> fields from a SKILL.md manifest's
        /// front matter. Skills in the source repository wrap their metadata in a leading
        /// block that may contain one or more "---" delimiter lines (e.g.
        /// "---\n---\nname: ...\n---"), so rather than assume an exact delimiter layout this
        /// collects "key: value" pairs from the top of the file until the document body
        /// begins (the first "# " heading). Missing fields are returned as null.
        /// </summary>
        public static void Read(string manifest, out string name, out string description)
        {
            name = null;
            description = null;

            if (string.IsNullOrEmpty(manifest))
            {
                return;
            }

            var lines = manifest.Replace("\r\n", "\n").Split('\n');
            var limit = Math.Min(lines.Length, MaxFrontMatterLines);

            for (var i = 0; i < limit; i++)
            {
                var trimmed = lines[i].Trim();
                if (trimmed.StartsWith("#", StringComparison.Ordinal))
                {
                    // Reached the markdown body; stop scanning.
                    break;
                }

                if (trimmed.Length == 0 || trimmed == "---")
                {
                    continue;
                }

                var colon = trimmed.IndexOf(':');
                if (colon <= 0)
                {
                    continue;
                }

                var key = trimmed.Substring(0, colon).Trim();
                var value = trimmed.Substring(colon + 1).Trim().Trim('"', '\'');
                if (value.Length == 0)
                {
                    continue;
                }

                if (string.Equals(key, "name", StringComparison.OrdinalIgnoreCase))
                {
                    name = value;
                }
                else if (string.Equals(key, "description", StringComparison.OrdinalIgnoreCase))
                {
                    description = value;
                }
            }
        }
    }
}
