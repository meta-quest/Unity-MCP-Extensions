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
using Oculus.Interaction;
using Oculus.Interaction.Editor.QuickActions;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace Meta.XR.MCP.Extension.Editor
{
    /// <summary>
    /// MCP tool that setup distance grabbable interaction on a specified game object.
    /// </summary>
    public static class AddInteractionDistanceGrabbable
    {
        private const string ToolName = "meta_add_distance_grabbable";
        private const string Description = "Specify a target gameObject to setup the distance grabbable component for the player to be able to grab it from a distance. Need an interaction rig.";

        [McpTool(ToolName, Description, Groups = new[] { "meta", "quest", "interaction" })]
        public static object HandleCommand(TargetGameObjectParams parameters)
        {
            UsageTelemetry.OnToolUsed(ToolName);
            var targetGo = SetupUtilities.GetTargetGameObject(parameters.NameOrID);
            if (targetGo == null)
            {
                UsageTelemetry.OnToolError(ToolName, "GameObject not found.");
                return Response.Error(
                    $"Target GameObject ('{parameters.NameOrID}') not found."
                );
            }
            var grabIntractable = InteractionUtils.AddInteractable<DistanceGrabInteractable, DistanceGrabWizard>(targetGo,
                (wizard) => wizard.InjectMode(DistanceGrabWizard.Mode.InteractableToHand));
            if (grabIntractable == null)
            {
                UsageTelemetry.OnToolError(ToolName, "Error occured when adding distance grabbable");
                return Response.Error(
                    $"Error occured when adding distance grabbable to GameObject {parameters.NameOrID}"
                );
            }

            UsageTelemetry.OnToolSuccess(ToolName);
            return Response.Success($"Distance Grabbable added to GameObject {parameters.NameOrID}",
                InteractionUtils.BuildReturnDataFromGrabbable(targetGo, grabIntractable.gameObject));
        }
    }
}
#endif
