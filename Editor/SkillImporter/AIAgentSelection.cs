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

using UnityEngine;

namespace Meta.XR.MCP.Extension.Editor
{
    /// <summary>
    /// The AI agent (tool) a skill is imported to.
    /// </summary>
    internal enum AIAgentSelection
    {
        /// <summary>Unity AI Assistant: project (<c>Assets/MetaSkills</c>) or user skills folder.</summary>
        [InspectorName("AI Assistant")]
        AiAssistant,

        /// <summary>Claude Code: <c>.claude/skills</c>.</summary>
        Claude,

        /// <summary>Codex: <c>.codex/skills</c>.</summary>
        Codex,

        /// <summary>GitHub Copilot: <c>.copilot/skills</c>.</summary>
        Copilot,

        /// <summary>Cursor: <c>.cursor/skills</c>.</summary>
        Cursor,

        /// <summary>Gemini: <c>.gemini/skills</c>.</summary>
        Gemini,

        /// <summary>OpenCode: <c>.config/opencode/skills</c>.</summary>
        [InspectorName("OpenCode")]
        OpenCode,
    }
}
