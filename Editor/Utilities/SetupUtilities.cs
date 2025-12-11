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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Unity.AI.MCP.Editor.Helpers;
using UnityEngine;

namespace Meta.XR.MCP.Extension.Editor
{
    /// <summary>
    /// Utility functions used by the various MCP tools for various setup.
    /// </summary>
    internal static class SetupUtilities
    {
        /// <summary>
        /// Disable the other main cameras that are not under the cameraRig gameObject
        /// </summary>
        /// <param name="cameraRig">The camera rig where camera should not be disabled</param>
        public static void DisableMainCameraNotUnderCameraRig(GameObject cameraRig)
        {
            var cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var audioListeners = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            DisableComponent(cameras, cameraRig, true);
            DisableComponent(audioListeners, cameraRig, true);
        }

        /// <summary>
        /// Disable list of components that are not under the exception parent
        /// </summary>
        /// <param name="components">Array of components to disable</param>
        /// <param name="exceptionParent">Parent under which we don't disable</param>
        /// <param name="mainCameraOnly">Check only if the component gameObject has the MainCamera tag</param>
        public static void DisableComponent<T>(T[] components, GameObject exceptionParent, bool mainCameraOnly) where T : Behaviour
        {
            foreach (var component in components)
            {
                if (mainCameraOnly && !component.gameObject.CompareTag("MainCamera"))
                {
                    continue;
                }
                component.enabled = component.transform.IsChildOf(exceptionParent.transform);
            }
        }

        /// <summary>
        /// Find and return the gameObject based on parameters
        /// </summary>
        /// <param name="params">The parameters that should contain the "target" param</param>
        /// <param name="targetToken">[out] The extracted targetToken from the params</param>
        /// <returns>GameObject found in the scene</returns>
        /// <exception cref="Exception">Error if the target field is not found in the params</exception>
        public static GameObject GetTargetGameObject(JObject @params, out JToken targetToken)
        {
            targetToken = @params["target"]; // Can be string (name/path) or int (instanceID)
            if (targetToken == null)
            {
                throw new Exception("Missing `target` game object field. instanceID, name or scene path");
            }
            return ObjectsHelper.FindObject(targetToken, "by_id_or_name_or_path");
        }

        /// <summary>
        /// Find and return the GameObject based on provided target
        /// </summary>
        /// <param name="target">The name, path or Id of the game object to find in the scene</param>
        /// <returns>GameObject found in the scene</returns>
        public static GameObject GetTargetGameObject(string target)
        {
            return ObjectsHelper.FindObject(target, "by_id_or_name_or_path");
        }

        /// <summary>
        /// Build array of data for all updated GameObjects listed.
        /// </summary>
        /// <param name="targetGo">The main target gameobject that was modified</param>
        /// <param name="updatedGameObjects">List of GameObjects that were modified or created</param>
        /// <returns>object[] that contains the data for each GameObject</returns>
        public static object[] BuildDataForUpdatedGameObjects(GameObject targetGo, List<GameObject> updatedGameObjects)
        {
            var data = new object[updatedGameObjects != null ? updatedGameObjects.Count + 1 : 1];
            data[0] = GameObjectSerializer.GetGameObjectData(targetGo);

            if (updatedGameObjects != null)
            {
                for (var i = 0; i < updatedGameObjects.Count; ++i)
                {
                    data[i + 1] = GameObjectSerializer.GetGameObjectData(updatedGameObjects[i]);
                }
            }

            return data;
        }
    }
}
