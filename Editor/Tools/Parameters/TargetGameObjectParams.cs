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

using Unity.AI.MCP.Editor.ToolRegistry;

namespace Meta.XR.MCP.Extension.Editor
{
    /// <summary>
    /// Params for tools that target a game object for the action
    /// </summary>
    public record TargetGameObjectParams
    {
        [McpDescription("Name, path or ID of the GameObject to target", Required = true)]
        public string NameOrID { get; set; } = string.Empty;
    }
}
