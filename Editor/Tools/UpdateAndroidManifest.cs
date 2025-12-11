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

namespace Meta.XR.MCP.Extension.Editor
{
    /// <summary>
    /// MCP tool to update the Android Manifest.
    /// </summary>
    public static class UpdateAndroidManifest
    {
        private const string ToolName = "meta_update_android_manifest";
        private const string Description = "After changes to the OculusProjectConfig(Assets/Oculus/OculusProjectConfig.asset) file (Meta XR settings) or the OVRManager script, we need to make sure the manifest is updated by calling this function.";

        [McpTool(ToolName, Description, Groups = new[] { "meta", "quest" })]
        public static object HandleCommand()
        {
            UsageTelemetry.OnToolUsed(ToolName);
            OVRManifestPreprocessor.GenerateOrUpdateAndroidManifest(true);
            UsageTelemetry.OnToolSuccess(ToolName);
            return Response.Success("Android Manifest is updated");
        }
    }
}
#endif
