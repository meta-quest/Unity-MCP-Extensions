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
using UnityEngine.EventSystems;
using UnityEngine;
using Unity.AI.MCP.Editor.Helpers;
using System;
using System.Collections.Generic;

namespace Meta.XR.MCP.Extension.Editor
{
    /// <summary>
    /// Utility functions for the Meta Interaction SDK
    /// </summary>
    internal static class InteractionUtils
    {
        /// <summary>
        /// Builds the return data for the grabbable actions.
        /// It will include the updated gameObject data and the new grabbable child gameObject data
        /// </summary>
        /// <param name="targetGo">The targeted gameObject we modified</param>
        /// <param name="addedGrabbableGo">The new grabbable interaction gameObject</param>
        /// <returns>object data with the updated_gameobject and grabbable_child gameObject data</returns>
        public static object BuildReturnDataFromGrabbable(GameObject targetGo, GameObject addedGrabbableGo)
        {
            return new
            {
                updated_gameobject = GameObjectSerializer.GetGameObjectData(targetGo),
                grabbable_child = GameObjectSerializer.GetGameObjectData(addedGrabbableGo),
            };
        }

        /// <summary>
        /// Add interactable to a GameObject. Apply the default setup and skip the optional fixes.
        /// </summary>
        /// <typeparam name="TInteractable">The interactable type component that will be added to the gameobject</typeparam>
        /// <typeparam name="TWizard">The interaction SDK wizard used to setup the interaction</typeparam>
        /// <param name="targetGo">The GameObject to target</param>
        /// <param name="injections">Any actions to be done in the wizard when the interaction is added</param>
        /// <returns>The added TInteractable component</returns>
        public static TInteractable AddInteractable<TInteractable, TWizard>(GameObject targetGo, Action<TWizard> injections = null)
            where TInteractable : MonoBehaviour, IInteractable
            where TWizard : QuickActionsWizard
        {
            QuickActionsWizard.CreateWithDefaults(targetGo, false, injections);

            return targetGo.GetComponentInChildren<TInteractable>();
        }

        /// <summary>
        /// Add interactable to a GameObject. Apply the default setup and skip the optional fixes.
        /// </summary>
        /// <typeparam name="TWizard">The interaction SDK wizard used to setup the interaction</typeparam>
        /// <param name="targetGo">The GameObject to target</param>
        /// <param name="injections">Any actions to be done in the wizard when the interaction is added</param>
        /// <returns>List of GameObjects that were created or modified</returns>
        public static List<GameObject> AddInteractable<TWizard>(GameObject targetGo, Action<TWizard> injections = null)
            where TWizard : QuickActionsWizard
        {
            return QuickActionsWizard.CreateWithDefaults(targetGo, false, injections) as List<GameObject>;
        }

        /// <summary>
        /// Add Canvas interaction to a canvas GameObject. Apply the default setup and skip the optional fixes.
        /// </summary>
        /// <typeparam name="TWizard">The interaction SDK wizard used to setup the canvas interaction</typeparam>
        /// <param name="canvas">Canvas component to target</param>
        /// <returns>List of GameObjects that were created or modified</returns>
        /// <exception cref="Exception">Failure to setup the interaction</exception>
        public static List<GameObject> AddCanvasInteraction<TWizard>(Canvas canvas)
            where TWizard : QuickActionsWizard
        {
            List<GameObject> updatedObjs = null;
            try
            {
                FixPointableCanvasModule(); // Make sure there is a PointableCanvasModule
                canvas.renderMode = RenderMode.WorldSpace; // Make sure the render mode is WorldSpace
                updatedObjs = AddInteractable<TWizard>(canvas.gameObject) as List<GameObject> ??
                              new List<GameObject>();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                throw new Exception($"Failed to setup Poke Interactor on canvas {canvas.name}");
            }

            return updatedObjs;
        }

        /// <summary>
        /// Make sure we have a PointableCanvasModule.
        /// Copied from com.meta.xr.sdk.interaction /Editor/QuickActions/Scripts/QuickActionsWizard.cs
        /// </summary>
        private static void FixPointableCanvasModule()
        {
            if (GameObject.FindAnyObjectByType<PointableCanvasModule>() != null)
            {
                return;
            }

            var eventSystemGO =
                GameObject.FindFirstObjectByType<EventSystem>()?.gameObject;

            UnityEngine.Object newObj;
            if (eventSystemGO != null)
            {
                newObj = eventSystemGO.AddComponent<PointableCanvasModule>();
            }
            else
            {
                newObj = new GameObject("Pointable Canvas Module",
                    typeof(EventSystem), typeof(PointableCanvasModule));
            }

            Debug.Log($"{nameof(PointableCanvasModule)} Added to Scene.", newObj);
        }
    }
}
#endif
