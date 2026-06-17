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

using System;
using System.Collections.Generic;

namespace Meta.XR.MCP.Extension.Editor
{
    /// <summary>
    /// Describes a single skill discovered in the GitHub repository, including its downloaded
    /// files keyed by skill-relative path.
    /// </summary>
    internal sealed class SkillCatalogEntry
    {
        /// <summary>The skill folder name (e.g. <c>hz-unity-fbx-import</c>).</summary>
        public string Id { get; }

        /// <summary>Human-readable name parsed from SKILL.md front matter, falling back to <see cref="Id"/>.</summary>
        public string DisplayName { get; set; }

        /// <summary>One-line description parsed from SKILL.md front matter (may be empty).</summary>
        public string Description { get; set; }

        /// <summary>
        /// The skill's files, keyed by skill-relative path (e.g. <c>SKILL.md</c>,
        /// <c>sub/dir/file.txt</c>). Populated when the catalog is fetched, so importing
        /// requires no further network access.
        /// </summary>
        public Dictionary<string, byte[]> Files { get; } =
            new Dictionary<string, byte[]>(StringComparer.Ordinal);

        public SkillCatalogEntry(string id)
        {
            Id = id;
            DisplayName = id;
            Description = string.Empty;
        }

        /// <summary>True when the skill folder name contains "unity" (case-insensitive).</summary>
        public bool IsUnitySkill =>
            Id.IndexOf("unity", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
