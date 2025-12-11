# Meta XR Unity MCP Extension

This package extends the Unity MCP package (`com.unity.ai.mcp`), which is part
of the Unity AI Gateway
([Beta Early Access](https://create.unity.com/UnityAIGatewayBeta)), with
additional Meta-specific tools.

With this package, you can use features specific to Meta XR development directly
from your AI Agent. These include adding a camera rig, adding an interaction rig
and interactors, and more. See the [Tools](#tools) section below for a
comprehensive list.

# Installation

Get access to the Unity AI Gateway Beta Early Access by following the
instructions on the
[Unity AI Gateway](https://create.unity.com/UnityAIGatewayBeta) page.

In Unity, add the following packages using the Package Manager:

- [Meta Core SDK](https://assetstore.unity.com/packages/tools/integration/meta-xr-core-sdk-269169)
  (`com.meta.xr.sdk.core`): v78 or later
- [Meta Interaction SDK](https://assetstore.unity.com/packages/tools/integration/meta-xr-interaction-sdk-265014)
  (`com.meta.xr.sdk.interaction.ovr`): v78 or later
- Unity MCP (`com.unity.ai.mcp`): Access to the
  [Unity AI Gateway](https://create.unity.com/UnityAIGatewayBeta) is required

Then, [install this package](https://docs.unity3d.com/Manual/upm-ui-giturl.html)
in your project by using the following Git URL:

```txt
https://github.com/meta-quest/Unity-Mcp-Extensions.git
```

After installing this package, follow the instructions in the Unity MCP
documentation on setup and usage.

# Tools

This package contains the following MCP tools that perform specific actions or
services:

## Core SDK

- `meta_add_camerarig`: adds the camera rig required for VR and MR development
- `meta_get_config_information`: get information on location of configuration
  files and specific details on how to use them. This will help the AI agent
  better understand how to manipulate them.
- `meta_update_android_manifest`: updates the Android manifest, which is
  necessary after modifying certain settings in the configuration

## Interaction SDK

- `meta_add_canvas_interaction_poke`: adds a poke interaction to a Canvas for UI
  interactions
- `meta_add_canvas_interaction_ray`: adds a ray interaction to a Canvas for
  distance UI interactions
- `meta_add_distance_grabbable`: sets up a GameObject with a distance grab
  interaction
- `meta_add_grabbable`: sets up a GameObject with a grab interaction
- `meta_add_interactionrig`: adds the comprehensive interaction rig required for
  Interaction SDK features
- `meta_add_teleport_hotspot`: places a teleport hotspot at the given world
  position
- `meta_get_interactors_state`: get the list of interactors from the interaction
  rigs with their instance ID and the current state enabled/disabled. With that
  information it's then possible for the AI agent to modify the state of the
  interactors.

# License

This package is licensed under the
[Meta Platform Technologies SDK license](./LICENSE).

# Contribution

See [CONTRIBUTING.md](./CONTRIBUTING.md) for information about how to
contribute.
