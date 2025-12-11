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

#if META_CORE_SDK
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;
using UnityEditor;

namespace Meta.XR.MCP.Extension.Editor
{
    /// <summary>
    /// MCP tool that adds a Camera rig to the current scene.
    /// If the camera rig already exists it returns the existing one.
    /// </summary>
    public static class AddCameraRig
    {
        public const string ToolName = "meta_add_camerarig";
        private const string CameraRigPrefabPath = "Packages/com.meta.xr.sdk.core/Prefabs/OVRCameraRig.prefab";

        private const string Description = "Adds a camera rig (OVRCameraRig) from Meta XR Core SDK. It is a custom XR rig that replaces Unity's conventional Main Camera. Under the camera rig it contains the anchor objects for eyes and hands/controllers. \nThe camera rig also includes the OVRManager component for all Meta Quest settings (including reflecting the OVRProjectConfig located at Assets/Oculus/OculusProjectConfig.asset).";

        [McpTool(ToolName, Description, Groups = new[] { "meta", "quest" })]
        public static object HandleCommand()
        {
            UsageTelemetry.OnToolUsed(ToolName);
            if (!CoreUtils.TryGetCameraRig(out var cameraRig))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<OVRCameraRig>(CameraRigPrefabPath);
                cameraRig = PrefabUtility.InstantiatePrefab(prefab) as OVRCameraRig;
                if (cameraRig != null)
                {
                    Undo.RegisterCreatedObjectUndo(cameraRig.gameObject, "Create " + cameraRig.gameObject.name);
                    if (cameraRig.TryGetComponent<OVRManager>(out var ovrManager))
                    {
                        ovrManager.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;
                    }

                    // disable other main cameras
                    SetupUtilities.DisableMainCameraNotUnderCameraRig(cameraRig.gameObject);
                }
                else
                {
                    const string errorMsg = "Camera Rig prefab failed to instantiate";
                    UsageTelemetry.OnToolError(ToolName, errorMsg);
                    return Response.Error(errorMsg);
                }
            }

            if (cameraRig == null)
            {
                const string errorMsg = "Camera Rig wasn't added to the scene";
                UsageTelemetry.OnToolError(ToolName, errorMsg);
                return Response.Error(errorMsg);
            }

            UsageTelemetry.OnToolSuccess(ToolName);
            return Response.Success($"Camera Rig is in the scene",
                GameObjectSerializer.GetGameObjectData(cameraRig.gameObject));
        }
    }
}
#endif
