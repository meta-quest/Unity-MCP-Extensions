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
using System.Globalization;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Meta.XR.MCP.Extension.Editor
{
    /// <summary>
    /// Editor window that lists Meta XR skills from the meta-quest/agentic-tools repository and
    /// imports the selected ones into a chosen AI agent (the Unity AI Assistant, Claude, Cursor,
    /// and others).
    /// </summary>
    internal sealed class SkillImporterWindow : EditorWindow
    {
        private const string MenuPath = "Meta/MCP Extension/Import Meta Skills...";
        private const string WindowTitle = "Import Meta Skills";
        private const string AutoLaunchPrefKey = "Meta.XR.MCP.Extension.SkillImporter.AutoLaunch";
        private const string AutoLaunchedSessionKey = "Meta.XR.MCP.Extension.SkillImporter.AutoLaunchedThisSession";

        private const string AIAgentSelectionPrefKey = "Meta.XR.MCP.Extension.SkillImporter.AIAgentSelection";
        private const string ScopePrefKey = "Meta.XR.MCP.Extension.SkillImporter.AiScope";

        private const float ScopeButtonWidth = 110f;

        private readonly HashSet<string> _selected = new HashSet<string>(StringComparer.Ordinal);
        private List<SkillCatalogEntry> _catalog = new List<SkillCatalogEntry>();

        // Installed skills in the current AI-agent/scope location, keyed by skill name ->
        // absolute SKILL.md path. Null when the cache needs rebuilding (see InvalidateInstalled).
        private Dictionary<string, string> _installedManifests;

        // Names of installed skills whose on-disk files differ from the freshly downloaded
        // catalog version (an update is available, or the skill was edited locally).
        private HashSet<string> _outdatedSkills;

        private AIAgentSelection _aiAgentSelection = AIAgentSelection.AiAssistant;
        private AiAssistantScope _aiScope = AiAssistantScope.Project;
        private bool _showAllSkills;
        private bool _busy;
        private string _status = "Click Refresh to load skills from GitHub.";
        private Vector2 _scroll;

        private static GUIStyle _badgeStyle;
        private static Texture2D _greenBadgeTexture;
        private static Texture2D _redBadgeTexture;
        private static Texture2D _orangeBadgeTexture;
        private static Texture2D _grayBadgeTexture;

        [MenuItem(MenuPath)]
        private static void ShowWindowFromMenu()
        {
            UsageTelemetry.OnSkillImporterOpened("menu");
            ShowWindow();
        }

        public static void ShowWindow()
        {
            var window = GetWindow<SkillImporterWindow>(utility: false, title: WindowTitle, focus: true);
            window.minSize = new Vector2(420f, 320f);
            window.Show();
        }

        private static SkillImporterAutoLaunch AutoLaunchMode
        {
            get => (SkillImporterAutoLaunch)GetProjectInt(
                AutoLaunchPrefKey, (int)SkillImporterAutoLaunch.Always);
            set => SetProjectInt(AutoLaunchPrefKey, (int)value);
        }

        // Settings are stored per project (for this user) via EditorUserSettings, which writes
        // to ProjectSettings/EditorUserSettings.asset, so each project keeps its own values.
        private static int GetProjectInt(string key, int defaultValue)
        {
            var raw = EditorUserSettings.GetConfigValue(key);
            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                ? value
                : defaultValue;
        }

        private static void SetProjectInt(string key, int value)
        {
            EditorUserSettings.SetConfigValue(key, value.ToString(CultureInfo.InvariantCulture));
        }

        // Runs on every domain reload, but the SessionState guard ensures the window is only
        // auto-opened once per Editor session (SessionState clears on Editor restart).
        [InitializeOnLoadMethod]
        private static void AutoLaunchOnEditorStart()
        {
            if (SessionState.GetBool(AutoLaunchedSessionKey, false))
            {
                return;
            }

            SessionState.SetBool(AutoLaunchedSessionKey, true);

            switch (AutoLaunchMode)
            {
                case SkillImporterAutoLaunch.Never:
                    return;
                case SkillImporterAutoLaunch.Always:
                    EditorApplication.delayCall += ShowWindow;
                    break;
                case SkillImporterAutoLaunch.OnUpdates:
                    EditorApplication.delayCall += () => _ = ShowIfUpdatesAvailableAsync();
                    break;
            }
        }

        private static async Task ShowIfUpdatesAvailableAsync()
        {
            try
            {
                var catalog = await GitHubSkillCatalog.FetchCatalogAsync();
                if (SkillInstaller.HasOutdatedSkills(catalog))
                {
                    ShowWindow();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SkillImporter] Auto-launch update check failed: {e.Message}");
            }
        }

        private void OnEnable()
        {
            _aiAgentSelection = (AIAgentSelection)GetProjectInt(
                AIAgentSelectionPrefKey, (int)AIAgentSelection.AiAssistant);
            _aiScope = (AiAssistantScope)GetProjectInt(
                ScopePrefKey, (int)AiAssistantScope.Project);

            if (_catalog.Count == 0)
            {
                _ = RefreshAsync();
            }
        }

        private void OnFocus()
        {
            // External changes (manual edits, the AI Assistant, other tools) may have altered
            // what is installed; rebuild the installed cache the next time the list draws.
            InvalidateInstalled();
        }

        private void EnsureInstalledManifests()
        {
            if (_installedManifests != null)
            {
                return;
            }

            _installedManifests = SkillInstaller.ScanInstalledManifests(_aiAgentSelection, _aiScope);

            // Flag installed skills whose files differ from the freshly downloaded catalog.
            _outdatedSkills = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in _catalog)
            {
                if (_installedManifests.TryGetValue(entry.DisplayName, out var manifestPath)
                    && !SkillInstaller.IsInstalledUpToDate(entry, manifestPath))
                {
                    _outdatedSkills.Add(entry.DisplayName);
                }
            }
        }

        private void InvalidateInstalled()
        {
            _installedManifests = null;
            _outdatedSkills = null;
        }

        private void OnGUI()
        {
            using (new EditorGUI.DisabledScope(_busy))
            {
                DrawToolbar();
            }

            EditorGUILayout.HelpBox(_status, _busy ? MessageType.Info : MessageType.None);

            using (new EditorGUI.DisabledScope(_busy))
            {
                if (_aiAgentSelection == AIAgentSelection.AiAssistant)
                {
                    DrawAiAssistantScope();
                }

                DrawSkillList();
                DrawFooter();
            }
        }

        private void DrawAiAssistantScope()
        {
            var userLocationSet = SkillInstaller.IsUserSkillsLocationSet;

            // Without a user skills folder there is nowhere to put User skills, so fall back to Project.
            if (!userLocationSet && _aiScope == AiAssistantScope.User)
            {
                SetScope(AiAssistantScope.Project);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Skills type:", GUILayout.Width(70f));

                if (GUILayout.Toggle(
                        _aiScope == AiAssistantScope.Project, "Project Skills",
                        EditorStyles.miniButtonLeft, GUILayout.Width(ScopeButtonWidth))
                    && _aiScope != AiAssistantScope.Project)
                {
                    SetScope(AiAssistantScope.Project);
                }

                using (new EditorGUI.DisabledScope(!userLocationSet))
                {
                    if (GUILayout.Toggle(
                            _aiScope == AiAssistantScope.User, "User Skills",
                            EditorStyles.miniButtonRight, GUILayout.Width(ScopeButtonWidth))
                        && _aiScope != AiAssistantScope.User)
                    {
                        SetScope(AiAssistantScope.User);
                    }
                }

                GUILayout.FlexibleSpace();
            }

            if (!userLocationSet)
            {
                EditorGUILayout.HelpBox(
                    "User Skills location is not set. Open Preferences ▸ AI ▸ Skills to create the user skills folder, then it can be selected here.",
                    MessageType.Info);
            }
        }

        private void SetScope(AiAssistantScope scope)
        {
            _aiScope = scope;
            SetProjectInt(ScopePrefKey, (int)_aiScope);
            InvalidateInstalled();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                {
                    _ = RefreshAsync();
                }

                GUILayout.Space(8f);
                GUILayout.Label("Import to:", GUILayout.Width(60f));
                var newAiAgentSelection = (AIAgentSelection)EditorGUILayout.EnumPopup(
                    _aiAgentSelection, EditorStyles.toolbarPopup, GUILayout.Width(180f));
                if (newAiAgentSelection != _aiAgentSelection)
                {
                    _aiAgentSelection = newAiAgentSelection;
                    SetProjectInt(AIAgentSelectionPrefKey, (int)_aiAgentSelection);
                    UsageTelemetry.OnSkillImporterAgentSelected(_aiAgentSelection.ToString());
                    InvalidateInstalled();
                }

                GUILayout.FlexibleSpace();
                _showAllSkills = GUILayout.Toggle(
                    _showAllSkills, "Show all skills", EditorStyles.toolbarButton, GUILayout.Width(110f));
            }
        }

        private void DrawSkillList()
        {
            EnsureInstalledManifests();
            var visible = GetVisibleEntries();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            if (visible.Count == 0)
            {
                EditorGUILayout.LabelField(
                    _catalog.Count == 0
                        ? "No skills loaded."
                        : "No matching skills. Try 'Show all skills'.",
                    EditorStyles.centeredGreyMiniLabel);
            }

            foreach (var entry in visible)
            {
                DrawSkillRow(entry);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSkillRow(SkillCatalogEntry entry)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var wasSelected = _selected.Contains(entry.Id);
                    var isSelected = EditorGUILayout.ToggleLeft(
                        entry.DisplayName, wasSelected, EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
                    if (isSelected != wasSelected)
                    {
                        if (isSelected)
                        {
                            _selected.Add(entry.Id);
                        }
                        else
                        {
                            _selected.Remove(entry.Id);
                        }
                    }

                    DrawSkillStatus(entry);
                }

                if (!string.IsNullOrEmpty(entry.Description))
                {
                    EditorGUILayout.LabelField(entry.Description, EditorStyles.wordWrappedMiniLabel);
                }
            }
        }

        private void DrawSkillStatus(SkillCatalogEntry entry)
        {
            if (_installedManifests == null ||
                !_installedManifests.TryGetValue(entry.DisplayName, out var manifestPath))
            {
                return;
            }

            if (_outdatedSkills != null && _outdatedSkills.Contains(entry.DisplayName))
            {
                DrawStatusBadge("Update", BadgeColor.Orange);
            }

            if (SkillInstaller.SupportsAllowState(_aiAgentSelection))
            {
                var allowed = SkillInstaller.IsSkillAllowedAtManifest(entry.DisplayName, manifestPath);
                DrawStatusBadge(allowed ? "Allowed" : "Denied", allowed ? BadgeColor.Green : BadgeColor.Red);
            }
            else
            {
                DrawStatusBadge("Installed", BadgeColor.Gray);
            }
        }

        private enum BadgeColor
        {
            Green,
            Red,
            Orange,
            Gray,
        }

        // Draws a small colored "status button" badge.
        private static void DrawStatusBadge(string text, BadgeColor color)
        {
            var style = BadgeStyle;
            var previousBackground = style.normal.background;
            style.normal.background = color switch
            {
                BadgeColor.Green => GreenBadgeTexture,
                BadgeColor.Red => RedBadgeTexture,
                BadgeColor.Orange => OrangeBadgeTexture,
                BadgeColor.Gray => GrayBadgeTexture,
                _ => GreenBadgeTexture,
            };
            GUILayout.Label(text, style, GUILayout.Width(72f), GUILayout.Height(18f));
            style.normal.background = previousBackground;
        }

        private static GUIStyle BadgeStyle
        {
            get
            {
                if (_badgeStyle == null)
                {
                    _badgeStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold,
                        padding = new RectOffset(8, 8, 2, 2),
                    };
                    _badgeStyle.normal.textColor = Color.white;
                }

                return _badgeStyle;
            }
        }

        private static Texture2D GreenBadgeTexture =>
            _greenBadgeTexture != null ? _greenBadgeTexture : (_greenBadgeTexture = MakeBadgeTexture(new Color(0.22f, 0.55f, 0.27f)));

        private static Texture2D RedBadgeTexture =>
            _redBadgeTexture != null ? _redBadgeTexture : (_redBadgeTexture = MakeBadgeTexture(new Color(0.62f, 0.21f, 0.21f)));

        private static Texture2D OrangeBadgeTexture =>
            _orangeBadgeTexture != null ? _orangeBadgeTexture : (_orangeBadgeTexture = MakeBadgeTexture(new Color(0.72f, 0.45f, 0.13f)));

        private static Texture2D GrayBadgeTexture =>
            _grayBadgeTexture != null ? _grayBadgeTexture : (_grayBadgeTexture = MakeBadgeTexture(new Color(0.35f, 0.38f, 0.42f)));

        private static Texture2D MakeBadgeTexture(Color color)
        {
            var texture = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private void DrawFooter()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Select All", GUILayout.Width(90f)))
                {
                    foreach (var entry in GetVisibleEntries())
                    {
                        _selected.Add(entry.Id);
                    }
                }

                if (GUILayout.Button("Select None", GUILayout.Width(90f)))
                {
                    _selected.Clear();
                }

                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(_selected.Count == 0))
                {
                    if (GUILayout.Button($"Import Selected ({_selected.Count})", GUILayout.Width(160f)))
                    {
                        ImportSelected();
                    }
                }
            }

            if (_aiAgentSelection == AIAgentSelection.AiAssistant)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(
                    "Skills imported to the AI Assistant are denied by default. Use the button below to open the preferences and allow them.",
                    MessageType.Info);
                if (GUILayout.Button("Open Preferences ▸ AI ▸ Skills"))
                {
                    UsageTelemetry.OnSkillImporterPreferencesOpened();
                    SkillInstaller.OpenAiAssistantSkillsPreferences();
                }
            }

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Auto-open on Editor launch:", GUILayout.Width(160f));
                var current = AutoLaunchMode;
                var updated = (SkillImporterAutoLaunch)EditorGUILayout.EnumPopup(current, GUILayout.Width(220f));
                if (updated != current)
                {
                    AutoLaunchMode = updated;
                    UsageTelemetry.OnSkillImporterAutoLaunchChanged(updated.ToString());
                }

                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.LabelField(
                $"You can reopen this window anytime from the menu: {MenuPath.Replace("/", " ▸ ")}",
                EditorStyles.wordWrappedMiniLabel);
        }

        private List<SkillCatalogEntry> GetVisibleEntries()
        {
            var visible = new List<SkillCatalogEntry>();
            foreach (var entry in _catalog)
            {
                if (_showAllSkills || entry.IsUnitySkill)
                {
                    visible.Add(entry);
                }
            }

            return visible;
        }

        private async Task RefreshAsync()
        {
            _busy = true;
            _status = "Loading skills from GitHub...";
            Repaint();

            try
            {
                _catalog = await GitHubSkillCatalog.FetchCatalogAsync();

                // Drop selections that no longer exist.
                _selected.RemoveWhere(id => !_catalog.Exists(e => e.Id == id));

                // Recompute installed/outdated state against the freshly downloaded catalog.
                InvalidateInstalled();

                var unityCount = _catalog.FindAll(e => e.IsUnitySkill).Count;
                _status = $"Loaded {_catalog.Count} skills ({unityCount} contain \"unity\").";
            }
            catch (Exception e)
            {
                _status = $"Failed to load skills: {e.Message}";
                Debug.LogException(e);
            }
            finally
            {
                _busy = false;
                Repaint();
            }
        }

        private void ImportSelected()
        {
            var toImport = new List<SkillCatalogEntry>();
            foreach (var entry in _catalog)
            {
                if (_selected.Contains(entry.Id))
                {
                    toImport.Add(entry);
                }
            }

            if (toImport.Count == 0)
            {
                return;
            }

            EnsureInstalledManifests();

            // Overwrite-after-confirm for any skills already present anywhere in the location.
            var existing = toImport.FindAll(e => _installedManifests.ContainsKey(e.DisplayName));
            if (existing.Count > 0)
            {
                var names = string.Join("\n  • ", existing.ConvertAll(e => e.Id));
                var overwrite = EditorUtility.DisplayDialog(
                    "Overwrite existing skills?",
                    $"These skills already exist in {SkillInstaller.DisplayName(_aiAgentSelection)} and will be replaced:\n\n  • {names}",
                    "Overwrite",
                    "Cancel");
                if (!overwrite)
                {
                    return;
                }
            }

            var succeeded = 0;
            try
            {
                for (var i = 0; i < toImport.Count; i++)
                {
                    var entry = toImport[i];
                    EditorUtility.DisplayProgressBar(
                        WindowTitle, $"Importing '{entry.Id}' ({i + 1}/{toImport.Count})...",
                        (float)i / toImport.Count);

                    var target = _aiAgentSelection == AIAgentSelection.AiAssistant
                        ? $"AiAssistant:{_aiScope}"
                        : _aiAgentSelection.ToString();

                    try
                    {
                        // Files were cached when the catalog was fetched; no network needed.
                        // If the skill already exists in the location, overwrite it in place to
                        // avoid leaving a duplicate; otherwise it goes to the default install folder.
                        _installedManifests.TryGetValue(entry.DisplayName, out var existingManifest);
                        SkillInstaller.Install(entry, entry.Files, _aiAgentSelection, _aiScope, existingManifest);
                        UsageTelemetry.OnSkillImported(entry.Id, target);
                        succeeded++;
                    }
                    catch (Exception e)
                    {
                        UsageTelemetry.OnSkillImportFailed(entry.Id, target);
                        Debug.LogError($"[SkillImporter] Failed to import '{entry.Id}': {e.Message}");
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                // Rebuild installed state so badges reflect the just-imported skills.
                InvalidateInstalled();

                // Only project (Assets/) imports need an asset refresh; user-folder imports
                // are picked up by the AI Assistant's own file watcher.
                if (_aiAgentSelection == AIAgentSelection.AiAssistant && _aiScope == AiAssistantScope.Project)
                {
                    AssetDatabase.Refresh();
                }

                _status = $"Imported {succeeded}/{toImport.Count} skills to {SkillInstaller.DisplayName(_aiAgentSelection)}.";
                Repaint();
            }
        }
    }
}
