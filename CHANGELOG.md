# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to
[Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2.12.0-pre.2]

### Update

- Update dependencies to Unity AI Assistant 2.12.0-pre.2

### Added

- Skill Importer window (`Meta ▸ MCP Extension ▸ Import Meta Skills...`) to browse and import Meta XR skills from [meta-quest/agentic-tools](https://github.com/meta-quest/agentic-tools); skills with "unity" in the name are shown by default.
- Imports to a selectable AI agent (AI Assistant, Claude, Codex, Copilot, Cursor, Gemini, OpenCode); the AI Assistant supports Project (`Assets/MetaSkills`) or User skills.
- Flags already-installed and outdated skills, shows the AI Assistant allow/deny state, and can auto-open once per Editor session. Settings are stored per project (via EditorUserSettings).

## [2.6.0-pre.1]

### Update

- Update dependencies to Unity AI Assistant 2.6.0-pre.1

## [2.0.0-pre.2]

### Fixed

- Load interaction rig prefab by GUID instead of hardcoded path for compatibility across SDK versions.

## [2.0.0-pre.1] - 2026-03-04

### Update for Unity AI Assistant

Updated the extensions to work with the Unity AI Assistant package.

## [0.1.0-exp.1] - 2025-12-10

### This is the first release of _Meta XR Unity MCP Extension Package_.

First release of the Meta XR Unity MCP Extension Package.
