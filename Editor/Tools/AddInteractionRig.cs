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
using Oculus.Interaction.Editor;
using Oculus.Interaction.Input;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;
using UnityEditor;
using UnityEngine;
#if META_INTERACTION_SDK_QUICK_ACTIONS_API
using Oculus.Interaction.OVR.Editor.QuickActions;
#endif


namespace Meta.XR.MCP.Extension.Editor
{
    /// <summary>
    /// MCP tool that adds the comprehensive interaction rig to the scene.
    /// It requires the camera rig to be added first.
    /// </summary>
    public static class AddInteractionRig
    {
        public const string ToolName = "meta_add_interactionrig";

#if !META_INTERACTION_SDK_QUICK_ACTIONS_API
        // Stable GUID for the comprehensive interaction rig prefab.
        // Using GUID lookup instead of a hardcoded path so the tool
        // continues to work if the prefab is renamed in future versions.
        private const string InteractionRigPrefabGuid = "0a7d2469f24041c4284c66706f84c45e";
#endif

        private const string Description = "Adds the comprehensive interaction rig from Interaction SDK as a child of the Camera Rig. This rig contains all interactors required for the user to interact in the scene with hands or controllers, like grab, distance grab, teleport, locomotion, etc.\nIt requires the Camera Rig to be added first.";

        [McpTool(ToolName, Description, Groups = new[] { "meta", "quest", "interaction" }, EnabledByDefault = true)]
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
#if META_INTERACTION_SDK_QUICK_ACTIONS_API
                // The public API finds the existing OVRCameraRig (preserved by the
                // precheck above) and parents the comprehensive interaction rig under it.
                OVRQuickActionsAPI.AddOVRInteractionRig();
                interactionRigRef = cameraRig.GetComponentInChildren<OVRCameraRigRef>();
                if (interactionRigRef == null)
                {
                    const string errorMsg = "Comprehensive interaction rig failed to instantiate";
                    UsageTelemetry.OnToolError(ToolName, errorMsg);
                    return Response.Error(errorMsg);
                }
#else
                var prefabPath = AssetDatabase.GUIDToAssetPath(InteractionRigPrefabGuid);
                if (string.IsNullOrEmpty(prefabPath))
                {
                    const string errorMsg = "Comprehensive interaction rig prefab not found. Ensure Interaction SDK is installed.";
                    UsageTelemetry.OnToolError(ToolName, errorMsg);
                    return Response.Error(errorMsg);
                }

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                var interactionRig = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

                if (interactionRig != null)
                {
                    interactionRig.transform.SetParent(cameraRig.transform, false);
                    UnityObjectAddedBroadcaster.HandleObjectWasAdded(interactionRig);
                    Selection.activeObject = interactionRig;

                    interactionRigRef = interactionRig.GetComponent<OVRCameraRigRef>();
                }
                else
                {
                    const string errorMsg = "Comprehensive interaction rig prefab failed to instantiate";
                    UsageTelemetry.OnToolError(ToolName, errorMsg);
                    return Response.Error(errorMsg);
                }
#endif
            }

            UsageTelemetry.OnToolSuccess(ToolName);
            return Response.Success("Interaction Rig is in the scene", GameObjectSerializer.GetGameObjectData(interactionRigRef.gameObject));
        }
    }
}
#endif
