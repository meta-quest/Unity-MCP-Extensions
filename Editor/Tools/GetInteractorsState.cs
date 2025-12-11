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
using System.Collections.Generic;
using Oculus.Interaction.Input;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;
using UnityEngine;

namespace Meta.XR.MCP.Extension.Editor
{
    public class GetInteractorsState
    {
        private const string ToolName = "meta_get_interactors_state";
        private const string Description = "Get all the interactors from the Interaction rig. It will provide the state (enable/disable) of the interactor GameObject. You can enable/disable it by changing the state of the GameObject. When modifying the state, ensure to use search_inactive = true.";

        [McpTool(ToolName, Description, Groups = new[] { "meta", "quest" })]
        public static object HandleCommand()
        {
            UsageTelemetry.OnToolUsed(ToolName);

            if (!CoreUtils.TryGetCameraRig(out var cameraRig))
            {
                UsageTelemetry.OnToolError(ToolName, "No Camera Rig in the scene");
                return Response.Error($"First you need to add a camera rig to the scene, use {AddCameraRig.ToolName} action");
            }

            var interactionRigRef = cameraRig.GetComponentInChildren<OVRCameraRigRef>();
            if (interactionRigRef == null)
            {
                UsageTelemetry.OnToolError(ToolName, "No Interaction Rig in the scene");
                return Response.Error($"First you need to add a an interaction rig to the scene, use {AddInteractionRig.ToolName} action");
            }

            var leftInteractors = GetInteractor(interactionRigRef.transform, "LeftInteractions");
            var rightInteractors = GetInteractor(interactionRigRef.transform, "RightInteractions");

            var locomotor = interactionRigRef.transform.Find("Locomotor");

            if (leftInteractors == null && rightInteractors == null && locomotor == null)
            {
                UsageTelemetry.OnToolError(ToolName, "No interactors were found");
                return Response.Error($"A problem occured the interactors are not found. Make sure the interaction rig is in the scene and is active.");
            }

            var data = new
            {
                leftInteractors = leftInteractors,
                rightInteractors = rightInteractors,
                locomotor = locomotor == null ? null : CreateDataForGameObject(locomotor.gameObject),
            };

            UsageTelemetry.OnToolSuccess(ToolName);
            return Response.Success("List of interactors and their state", data);
        }

        private static Dictionary<string, List<object>> GetInteractor(Transform rig, string sideName)
        {
            var root = rig.FindChildRecursive(sideName);
            if (root == null)
            {
                return null;
            }

            var interactorsTransform = root.Find("Interactors");
            if (interactorsTransform == null)
            {
                return null;
            }

            Dictionary<string, List<object>> interactors = new ();

            for (var i = 0; i < interactorsTransform.childCount; ++i)
            {
                var type = interactorsTransform.GetChild(i);
                var typeName = type.gameObject.name;
                if (typeName == "ActiveStatesTrackers") continue;

                interactors[typeName] = new List<object>();
                for (var y = 0; y < type.childCount; ++y)
                {
                    var interactor = type.GetChild(y);
                    var go = interactor.gameObject;
                    interactors[typeName].Add(CreateDataForGameObject(go));
                }
            }

            return interactors;
        }

        private static object CreateDataForGameObject(GameObject go)
        {
            return new
            {
                name = go.name,
                instanceID = go.GetInstanceID(),
                activeSelf = go.activeSelf,
            };
        }
    }
}
#endif
