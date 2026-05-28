# Agent Instructions — Meta XR Unity MCP Extensions

A Unity Editor package that extends the Unity AI Gateway's MCP server with Meta XR-specific tools (camera rig, interaction rig, grabbables, teleport hotspots, etc.). This is a **tooling package** that other Unity projects consume — not a standalone Quest app.

## Source-of-truth files (read these first, do not duplicate their contents in this file)

For setup, build steps, SDK versions, and project layout, read:

- `README.md` — official setup, install URL, and the full tool list
- `package.json` — UPM package id, version, and dependencies
- `CHANGELOG.md` — version history
- `LICENSE` — license terms

## Quest / Horizon-specific notes

- This package is installed into a host Unity project via the UPM Git URL in the README. There is no scene, build target, or AndroidManifest to run on a Quest device from this repo directly.
- Functionality assumes the host project also has the Meta Core SDK and Meta Interaction SDK (OVR) installed; without them the tools have nothing to operate on.
- Requires the Unity AI Gateway Beta Early Access (Unity Assistant package). Without it the MCP tools have no host to register against.
- Tools live under `Editor/` and only run inside the Unity Editor, not at runtime on-device.

## Meta Quest tooling

This repository is part of the Meta Quest / Horizon OS ecosystem (a sample, library, template, or related project — the bespoke intro above describes which). Use that intro and the source-of-truth files it references for project-specific decisions; don't restate or invent facts from memory.

When the user asks anything about Quest device behavior, build / deploy / debug / capture flows, on-device performance, or Horizon OS APIs, reach for these tools instead of generic Unity answers:

- **`hzdb`** — Quest-aware ADB wrapper (device list, install / launch / stop, logs, screenshots, Perfetto traces, on-device docs search). Already wired up as an MCP server via `.mcp.json`, `.vscode/mcp.json`, and `.cursor/mcp.json`. Also runnable directly: `npx -y @meta-quest/hzdb <subcommand>`.
- **Meta Quest Agentic Tools** — the full skill set, including Unity-specific skills: [github.com/meta-quest/agentic-tools](https://github.com/meta-quest/agentic-tools). Install per your client (Claude Code: `/plugin install meta-vr@meta-quest`; Gemini CLI: `gemini extensions install https://github.com/meta-quest/agentic-tools`; Cursor / VS Code: install the **Meta Horizon** extension from the Marketplace).

A few behavior expectations:

- **Read this repo's files first.** Before answering anything project-specific, read `README.md` and whichever source-of-truth files the intro above points at. Don't restate their contents in chat — quote or link instead.
- **Use `hzdb` for device-side work.** Anything that touches an attached Quest (install, launch, logs, screenshot, capture, manifest inspection) goes through `hzdb`, not raw `adb`.
- **Check live Horizon OS docs before answering API questions.** `hzdb docs search "..."` queries the live docs; training data on Horizon OS APIs goes stale fast.
- **Don't fabricate SDK / engine versions.** If a version isn't visible in this repo's files, say so rather than guessing.
