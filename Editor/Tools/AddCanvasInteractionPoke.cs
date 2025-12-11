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

#if META_INTERACTION_SDK
using Oculus.Interaction.Editor.QuickActions;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;
using UnityEngine;

namespace Meta.XR.MCP.Extension.Editor
{
    /// <summary>
    /// MCP tool that adds a Poke interaction to a Canvas.
    /// </summary>
    public class AddCanvasInteractionPoke
    {
        private const string ToolName = "meta_add_canvas_interaction_poke";
        private const string Description = "Specify a gameObject with a canvas to add poke interaction. Canvas must be in world space. The player will be able to poke, using hands and controllers, the interactable on a canvas (like buttons).";

        [McpTool(ToolName, Description, Groups = new[] { "meta", "quest", "interaction", "ui" })]
        public static object HandleCommand(TargetGameObjectParams parameters)
        {
            UsageTelemetry.OnToolUsed(ToolName);
            var targetGo = SetupUtilities.GetTargetGameObject(parameters.NameOrID);
            if (targetGo == null)
            {
                UsageTelemetry.OnToolError(ToolName, "GameObject not found.");
                return Response.Error($"Target GameObject ('{parameters.NameOrID}') not found.");
            }

            if (!targetGo.TryGetComponent<Canvas>(out var canvas))
            {
                UsageTelemetry.OnToolError(ToolName, "GameObject doesn't have a Canvas component.");
                return Response.Error($"Target GameObject ('{parameters.NameOrID}') doesn't have a Canvas component.");
            }
            var updatedObjects = InteractionUtils.AddCanvasInteraction<PokeCanvasWizard>(canvas);

            var data = SetupUtilities.BuildDataForUpdatedGameObjects(targetGo, updatedObjects);

            UsageTelemetry.OnToolSuccess(ToolName);
            return Response.Success($"Poke interactor added to canvas {targetGo.name}", data);
        }
    }
}
#endif
