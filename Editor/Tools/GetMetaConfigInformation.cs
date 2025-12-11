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

using System.Collections.Generic;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;
using UnityEditor;

namespace Meta.XR.MCP.Extension.Editor
{
    public class GetMetaConfigInformation
    {
        private const string ToolName = "meta_get_config_information";
        private const string Description = "Run this tool at least once before running any `meta_` tools. This will give you system information on the configuration files and additional information to better understand how to modify them.";

        [McpTool(ToolName, Description, Groups = new[] { "meta", "quest" })]
        public static object HandleCommand()
        {
            UsageTelemetry.OnToolUsed(ToolName);
            Dictionary<string, object> configInfos = new Dictionary<string, object>();
            // Info on Configs
            var guids = AssetDatabase.FindAssets("OculusProjectConfig");
            if (guids.Length > 0)
            {
                // take only the first one
                var guid = guids[0];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                configInfos["OculusProjectConfig"] = new
                {
                    path = assetPath,
                    info = "This file is the main configuration for the settings of your Meta Quest application. It's a scriptable object from the OVRProjectConfig class. Getting information directly from the class will help ensure to set the right configuration based on the prompt.",
                    action_information = new
                    {
                        handTrackingSupport = "The `handTrackingSupport` field has different values that can be read from this enum HandTrackingSupport. ControllersOnly = 0, ControllersAndHands = 1, HandsOnly = 2",
                        handTrackingFrequency = "The `handTrackingFrequency` field uses the values from enum HandTrackingFrequency. USe the HIGH value only when using Fast Motion Mode (FMM), previously known as 'High Frequency Hand Tracking'",
                    },
                };
            }

            configInfos["OVRManager"] = new
            {
                path =
                    "This will be found on the CameraRig gameObject in the scene. Search for the OVRManager in the scene.",
                info =
                    "This contains configuration specific to the current camera rig and the current scene. It can be modified by managing the GameObject in the scene. Reading the OVRManager script will give information on each setting.",
                action_information = new
                {
                    SimultaneousHandsAndControllersEnabled =
                        "Allows the application to use simultaneous hands and controllers functionality. This option must be enabled at build time.",
                    wideMotionModeHandPosesEnabled =
                        "Defines if hand poses can leverage algorithms to retrieve hand poses outside of the normal tracking area.",
                }
            };

            UsageTelemetry.OnToolSuccess(ToolName);
            return Response.Success("This is key information on how to modify configurations for the Meta Quest", configInfos);
        }
    }
}
