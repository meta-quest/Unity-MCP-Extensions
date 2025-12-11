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
using UnityEngine;

namespace Meta.XR.MCP.Extension.Editor
{
    internal static class CoreUtils
    {
        /// <summary>
        /// Try to get the CameraRig in the scene
        /// </summary>
        /// <param name="cameraRig">[out] the found camera rig</param>
        /// <returns>True if found, False otherwise</returns>
        public static bool TryGetCameraRig(out OVRCameraRig cameraRig)
        {
            cameraRig = Object.FindFirstObjectByType<OVRCameraRig>();
            return cameraRig != null;
        }
    }
}
#endif
