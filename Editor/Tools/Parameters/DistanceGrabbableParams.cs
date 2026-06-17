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
using Oculus.Interaction.Editor.QuickActions;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace Meta.XR.MCP.Extension.Editor
{
    /// <summary>
    /// Params for the distance-grabbable tool. Adds a Mode field so the LLM
    /// can choose the distance grab behavior from the user's prompt.
    /// </summary>
    public record DistanceGrabbableParams : TargetGameObjectParams
    {
#if META_INTERACTION_SDK_QUICK_ACTIONS_API
        [McpDescription(
            "Distance grab behavior. PullToHand brings the object to the hand (default); "
          + "AnchorAtHand keeps it at distance and rotates/scales relative to the hand; "
          + "ManipulateInPlace lets the user rotate/scale the object remotely without moving it.",
            EnumType = typeof(DistanceGrabMode))]
        public DistanceGrabMode Mode { get; set; } = DistanceGrabMode.PullToHand;
#else
        // DistanceGrabWizard.Mode is internal in pre-v205 ISDK; expose as string with
        // EnumType so the schema still constrains LLM-supplied values to valid names.
        // Parsed back to the enum at the call site.
        [McpDescription(
            "Distance grab behavior. InteractableToHand brings the object to the hand (default); "
          + "AnchorAtHand keeps it at distance and rotates/scales relative to the hand; "
          + "HandToInteractable lets the user rotate/scale the object remotely without moving it.",
            EnumType = typeof(DistanceGrabWizard.Mode))]
        public string Mode { get; set; } = nameof(DistanceGrabWizard.Mode.InteractableToHand);
#endif
    }
}
#endif
