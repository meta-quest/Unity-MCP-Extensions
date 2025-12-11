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

using System.Diagnostics;
using UnityEditor;

namespace Meta.XR.MCP.Extension.Editor
{
    [InitializeOnLoad]
    internal static class UsageTelemetry
    {
        private const string SessionTelemetryKey = "Meta_XR_UnityMCP_Extension_Telemetry_Start_Sent";
        static UsageTelemetry()
        {
            if (!SessionState.GetBool(SessionTelemetryKey, false))
            {
                SendEvent("package_in_project");
                SessionState.SetBool(SessionTelemetryKey, true);
            }
        }

        public static void OnToolUsed(string toolName)
        {
            SendEvent("tool_used", toolName);
        }

        public static void OnToolSuccess(string toolName)
        {
            SendEvent("tool_success", toolName);
        }

        public static void OnToolError(string toolName, string errorMsg)
        {
            SendEvent("tool_error", toolName);
        }

        [Conditional("META_CORE_SDK")]
        private static void SendEvent(string eventName, string param = null)
        {
#if META_CORE_SDK
            OVRPlugin.SendEvent(eventName, param, "unity_mcp_extension");
#endif
        }
    }
}
