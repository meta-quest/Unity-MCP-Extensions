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
using Newtonsoft.Json.Linq;
using Oculus.Interaction.Editor.QuickActions;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;
using UnityEngine;

namespace Meta.XR.MCP.Extension.Editor
{
    /// <summary>
    /// MCP tool that adds telpeort hotspot for locomotion in the scene at the specified world position.
    /// </summary>
    public class AddTeleportHotspot
    {
        private const string ToolName = "meta_add_teleport_hotspot";
        private const string Description = "Place a teleport hotspot in the scene. The player will be able to teleport to that position. Need the position field.";

        [McpTool(ToolName, Description, Groups = new[] { "meta", "quest", "locomotion" })]
        public static object HandleCommand(JObject @params)
        {
            UsageTelemetry.OnToolUsed(ToolName);
            var position = Vector3Helper.ParseVector3(@params["position"] as JArray);

            var targetGo = new GameObject("TeleportHotspot");
            targetGo.transform.position = position;

            var createdObjects = InteractionUtils.AddInteractable<TeleportWizard>(targetGo);

            var data = SetupUtilities.BuildDataForUpdatedGameObjects(targetGo, createdObjects);

            UsageTelemetry.OnToolSuccess(ToolName);
            return Response.Success("Hotspot added", data);
        }

        /// <summary>
        /// Returns the input schema for this tool.
        /// </summary>
        [McpSchema("meta_add_teleport_hotspot")]
        public static object GetInputSchema()
        {
            return new
            {
                type = "object",
                properties = new
                {
                    position = new
                    {
                        type = "array",
                        description = "World position [x,y,z]",
                        items = new { type = "number" },
                        min_items = 3,
                        max_items = 3
                    },
                },
                required = new[] { "position" },
            };
        }
    }
}
#endif
