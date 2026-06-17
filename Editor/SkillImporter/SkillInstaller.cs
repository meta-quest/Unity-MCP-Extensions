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
using UnityEditor;
using UnityEngine;

namespace Meta.XR.MCP.Extension.Editor
{
    /// <summary>
    /// Writes downloaded skills to disk in the layout expected by each AI agent, and detects
    /// whether a skill is already present in an AI agent's skills location.
    /// </summary>
    internal static class SkillInstaller
    {
        private const string ClaudeSkillsFolder = ".claude/skills";

        // The AI Assistant scans all of Assets/ for SKILL.md; we write project skills into
        // this dedicated subfolder, but installed-detection looks across the whole location.
        private const string AiAssistantProjectInstallFolder = "MetaSkills"; // relative to Assets/

        // EditorPrefs key scheme the Unity AI Assistant uses to persist per-skill allow state.
        // Mirrors AssistantEditorPreferences in com.unity.ai.assistant: the key is
        // "AIAssistant.SkillAllowed." + <frontmatter name> + ":" + <normalized SKILL.md path>,
        // where the path is the absolute SKILL.md path lower-cased with forward slashes.
        // New skills default to denied (false), which is why the importer surfaces the state
        // and offers a shortcut to the AI ▸ Skills preferences.
        private const string AiAssistantAllowedKeyPrefix = "AIAssistant.SkillAllowed.";

        // Scope passed to AssetDatabase.FindAssets when resolving project skills.
        private static readonly string[] AssetsSearchFolders = { "Assets" };

        /// <summary>A human-readable label for the AI agent, including its skills folder where relevant.</summary>
        public static string DisplayName(AIAgentSelection aiAgent)
        {
            return aiAgent switch
            {
                AIAgentSelection.AiAssistant => "AI Assistant",
                AIAgentSelection.Claude => "Claude (.claude/skills)",
                AIAgentSelection.Codex => "Codex (.codex/skills)",
                AIAgentSelection.Copilot => "Copilot (.copilot/skills)",
                AIAgentSelection.Cursor => "Cursor (.cursor/skills)",
                AIAgentSelection.Gemini => "Gemini (.gemini/skills)",
                AIAgentSelection.OpenCode => "OpenCode (.config/opencode/skills)",
                _ => aiAgent.ToString(),
            };
        }

        /// <summary>
        /// The AI Assistant user skills folder, computed the same way as com.unity.ai.assistant
        /// (SkillsScanner.UserAppDataFolder). Skills here are shared across all Unity projects.
        /// </summary>
        public static string UserSkillsFolder
        {
            get
            {
                var appDataRoot = Application.platform == RuntimePlatform.OSXEditor
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support")
                    : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(appDataRoot, "Unity", "AIAssistantSkills");
            }
        }

        /// <summary>True if the AI Assistant user skills folder exists (i.e. the location is set).</summary>
        public static bool IsUserSkillsLocationSet => Directory.Exists(UserSkillsFolder);

        /// <summary>Absolute path of the folder new skills are written to for the AI agent/scope.</summary>
        public static string GetInstallRoot(AIAgentSelection aiAgent, AiAssistantScope scope = AiAssistantScope.Project)
        {
            // Application.dataPath points at the project's Assets folder.
            var assets = Application.dataPath;
            var projectRoot = Directory.GetParent(assets)!.FullName;

            return aiAgent switch
            {
                AIAgentSelection.Claude => Path.Combine(projectRoot, ".claude", "skills"),
                AIAgentSelection.Codex => Path.Combine(projectRoot, ".codex", "skills"),
                AIAgentSelection.Copilot => Path.Combine(projectRoot, ".copilot", "skills"),
                AIAgentSelection.Cursor => Path.Combine(projectRoot, ".cursor", "skills"),
                AIAgentSelection.Gemini => Path.Combine(projectRoot, ".gemini", "skills"),
                AIAgentSelection.OpenCode => Path.Combine(projectRoot, ".config", "opencode", "skills"),
                AIAgentSelection.AiAssistant => scope == AiAssistantScope.User
                    ? UserSkillsFolder
                    : Path.Combine(assets, AiAssistantProjectInstallFolder),
                _ => Path.Combine(projectRoot, ClaudeSkillsFolder),
            };
        }

        /// <summary>The folder a freshly imported skill would be written to.</summary>
        public static string GetInstallFolder(
            SkillCatalogEntry entry, AIAgentSelection aiAgent, AiAssistantScope scope = AiAssistantScope.Project)
        {
            return Path.Combine(GetInstallRoot(aiAgent, scope), entry.Id);
        }

        /// <summary>
        /// The root folder the AI agent actually scans for skills. For the AI Assistant project
        /// scope this is the entire <c>Assets/</c> folder (the AI Assistant discovers SKILL.md
        /// anywhere under it), not just the install subfolder. For other AI agents it is the same
        /// as the install root.
        /// </summary>
        public static string GetSkillsLocationRoot(AIAgentSelection aiAgent, AiAssistantScope scope = AiAssistantScope.Project)
        {
            if (aiAgent == AIAgentSelection.AiAssistant && scope == AiAssistantScope.Project)
            {
                return Application.dataPath;
            }

            return GetInstallRoot(aiAgent, scope);
        }

        /// <summary>
        /// Scans the AI agent's skills location and returns every installed skill keyed by its
        /// SKILL.md <c>name</c> (falling back to the folder name), mapped to the absolute SKILL.md
        /// path. This reads the actual content of the location so a skill counts as installed
        /// wherever it sits within the scanned area.
        ///
        /// AI Assistant project skills (anywhere under <c>Assets/</c>) are resolved via the
        /// AssetDatabase index rather than a recursive directory walk, so this is cheap to call
        /// from the UI thread even in projects with a large <c>Assets/</c> tree. The user folder
        /// and the per-tool folders at the project root live outside the AssetDatabase, so those
        /// fall back to a recursive filesystem scan (their contents are small).
        /// </summary>
        public static Dictionary<string, string> ScanInstalledManifests(
            AIAgentSelection aiAgent, AiAssistantScope scope = AiAssistantScope.Project)
        {
            var installed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (aiAgent == AIAgentSelection.AiAssistant && scope == AiAssistantScope.Project)
            {
                // Indexed query — Unity already tracks every SKILL.md TextAsset under Assets/.
                var nameFilter = $"t:TextAsset {Path.GetFileNameWithoutExtension(SkillManifest.FileName)}";
                foreach (var guid in AssetDatabase.FindAssets(nameFilter, AssetsSearchFolders))
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetFileName(assetPath) == SkillManifest.FileName)
                    {
                        AddInstalledManifest(installed, Path.GetFullPath(assetPath));
                    }
                }

                return installed;
            }

            var root = GetSkillsLocationRoot(aiAgent, scope);
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                return installed;
            }

            foreach (var manifestPath in Directory.EnumerateFiles(root, SkillManifest.FileName, SearchOption.AllDirectories))
            {
                AddInstalledManifest(installed, manifestPath);
            }

            return installed;
        }

        private static void AddInstalledManifest(Dictionary<string, string> installed, string manifestPath)
        {
            string content;
            try
            {
                content = File.ReadAllText(manifestPath);
            }
            catch
            {
                return;
            }

            SkillManifest.Read(content, out var name, out _);
            if (string.IsNullOrEmpty(name))
            {
                name = Path.GetFileName(Path.GetDirectoryName(manifestPath));
            }

            if (!string.IsNullOrEmpty(name) && !installed.ContainsKey(name))
            {
                installed[name] = manifestPath;
            }
        }

        /// <summary>
        /// True if the installed skill at <paramref name="manifestPath"/> matches the catalog
        /// entry's freshly downloaded files exactly. Returns false when any file is missing,
        /// differs, or was added/removed — i.e. an update is available (or the skill was
        /// edited locally). Unity-generated <c>.meta</c> files are ignored.
        /// </summary>
        public static bool IsInstalledUpToDate(SkillCatalogEntry entry, string manifestPath)
        {
            if (string.IsNullOrEmpty(manifestPath))
            {
                return false;
            }

            var skillFolder = Path.GetDirectoryName(manifestPath);
            if (string.IsNullOrEmpty(skillFolder) || !Directory.Exists(skillFolder))
            {
                return false;
            }

            // Every catalog file must exist on disk with identical bytes.
            foreach (var pair in entry.Files)
            {
                var path = Path.Combine(skillFolder, pair.Key);
                if (!File.Exists(path))
                {
                    return false;
                }

                byte[] installed;
                try
                {
                    installed = File.ReadAllBytes(path);
                }
                catch
                {
                    return false;
                }

                if (!BytesEqual(installed, pair.Value))
                {
                    return false;
                }
            }

            // There must be no extra skill files beyond the catalog's (ignoring .meta companions).
            var installedFileCount = 0;
            foreach (var file in Directory.EnumerateFiles(skillFolder, "*", SearchOption.AllDirectories))
            {
                if (!file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    installedFileCount++;
                }
            }

            return installedFileCount == entry.Files.Count;
        }

        private static bool BytesEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            for (var i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// True if any installed skill (in any AI agent/scope) differs from the catalog (an
        /// update is available). Used by the auto-launch "only on updates" mode.
        /// </summary>
        public static bool HasOutdatedSkills(IReadOnlyList<SkillCatalogEntry> catalog)
        {
            if (catalog == null || catalog.Count == 0)
            {
                return false;
            }

            foreach (AIAgentSelection aiAgent in Enum.GetValues(typeof(AIAgentSelection)))
            {
                var scopes = aiAgent == AIAgentSelection.AiAssistant
                    ? new[] { AiAssistantScope.Project, AiAssistantScope.User }
                    : new[] { AiAssistantScope.Project };

                foreach (var scope in scopes)
                {
                    var installed = ScanInstalledManifests(aiAgent, scope);
                    if (installed.Count == 0)
                    {
                        continue;
                    }

                    foreach (var entry in catalog)
                    {
                        if (installed.TryGetValue(entry.DisplayName, out var manifestPath)
                            && !IsInstalledUpToDate(entry, manifestPath))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// True if the AI agent has a per-skill allow/deny concept. Only the Unity AI Assistant
        /// gates skills behind an allow list; the other tools have no equivalent.
        /// </summary>
        public static bool SupportsAllowState(AIAgentSelection aiAgent)
        {
            return aiAgent == AIAgentSelection.AiAssistant;
        }

        /// <summary>
        /// Reads whether the AI Assistant currently allows the skill whose manifest lives at
        /// <paramref name="manifestPath"/> (from its EditorPrefs allow list). Returns false
        /// when the path is unknown or the skill is denied.
        /// </summary>
        public static bool IsSkillAllowedAtManifest(string skillName, string manifestPath)
        {
            if (string.IsNullOrEmpty(manifestPath))
            {
                return false;
            }

            var normalized = Path.GetFullPath(manifestPath).ToLowerInvariant().Replace('\\', '/');
            var key = AiAssistantAllowedKeyPrefix + skillName + ":" + normalized;
            return EditorPrefs.GetBool(key, false);
        }

        /// <summary>
        /// Writes the downloaded files for a skill to disk, replacing any existing contents.
        /// When the skill is already installed somewhere in the AI agent's location,
        /// <paramref name="existingManifestPath"/> should be supplied so the existing copy is
        /// overwritten in place (avoiding duplicates); otherwise it is written to the default
        /// install folder. The caller refreshes the asset database after project installs.
        /// </summary>
        public static void Install(
            SkillCatalogEntry entry,
            IReadOnlyDictionary<string, byte[]> files,
            AIAgentSelection aiAgent,
            AiAssistantScope scope = AiAssistantScope.Project,
            string existingManifestPath = null)
        {
            var skillFolder = !string.IsNullOrEmpty(existingManifestPath)
                ? Path.GetDirectoryName(existingManifestPath)
                : GetInstallFolder(entry, aiAgent, scope);

            // Guard against deleting the scan/install root itself. A SKILL.md sitting directly at
            // the root (e.g. Assets/SKILL.md) would make skillFolder the root, and the recursive
            // delete below would wipe the entire location. Require a real subfolder.
            var locationRoot = GetSkillsLocationRoot(aiAgent, scope);
            if (!IsStrictlyInside(locationRoot, skillFolder))
            {
                throw new InvalidOperationException(
                    $"Refusing to install skill '{entry.Id}': resolved folder '{skillFolder}' is not " +
                    $"safely inside the skills location '{locationRoot}'.");
            }

            if (Directory.Exists(skillFolder))
            {
                Directory.Delete(skillFolder, recursive: true);
            }

            Directory.CreateDirectory(skillFolder);

            var skillFolderFull = Path.GetFullPath(skillFolder);
            foreach (var pair in files)
            {
                var targetPath = Path.Combine(skillFolder, pair.Key);

                // Guard against Zip Slip: a malicious/corrupt archive entry whose relative path
                // contains ".." segments could otherwise resolve outside the skill folder.
                var targetFull = Path.GetFullPath(targetPath);
                if (!IsStrictlyInside(skillFolderFull, targetFull))
                {
                    throw new InvalidOperationException(
                        $"Refusing to write skill file '{pair.Key}': it resolves outside the skill " +
                        $"folder '{skillFolder}'.");
                }

                var targetDir = Path.GetDirectoryName(targetFull);
                if (!string.IsNullOrEmpty(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                File.WriteAllBytes(targetFull, pair.Value);
            }
        }

        // True if <paramref name="candidate"/> is a strict subpath of <paramref name="root"/>
        // (not equal to it). Both are normalized to full paths before comparison.
        private static bool IsStrictlyInside(string root, string candidate)
        {
            if (string.IsNullOrEmpty(root) || string.IsNullOrEmpty(candidate))
            {
                return false;
            }

            var rootFull = Path.GetFullPath(root)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            var candidateFull = Path.GetFullPath(candidate)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            return candidateFull.Length > rootFull.Length
                && candidateFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Opens the Unity AI Assistant skills preferences pane.</summary>
        public static void OpenAiAssistantSkillsPreferences()
        {
            // The Unity AI Assistant registers its skills settings under Preferences/AI/Skills.
            // If the exact leaf path changes in a future package version, this still
            // opens the closest matching Preferences pane (or the Preferences root).
            SettingsService.OpenUserPreferences("Preferences/AI/Skills");
        }
    }
}
