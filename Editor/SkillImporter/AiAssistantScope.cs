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

namespace Meta.XR.MCP.Extension.Editor
{
    /// <summary>
    /// Which AI Assistant skills location to use. The AI Assistant scans both the project
    /// (anywhere under <c>Assets/</c>) and a per-user folder outside the project.
    /// </summary>
    internal enum AiAssistantScope
    {
        /// <summary>Project skills, stored under <c>Assets/MetaSkills</c> and shared via the project.</summary>
        Project,

        /// <summary>User skills, stored in the AI Assistant user folder and shared across projects.</summary>
        User,
    }
}
