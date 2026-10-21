# GPU occlusion culling

Licensed under MPL-2.0. See `package.json` for version and engine compatibility.

An experimental optional Brick for 3D views. CPU frustum culling stays in the renderer; this Brick removes
submeshes hidden behind opaque geometry by writing their indirect instance counts on the GPU.

## Enable

Install `org.mass4.turian.hzb` (`turian-cli brick add org.mass4.turian.hzb` or the Bricks panel), create **Settings / GPU Occlusion Culling** under `Assets/Settings`,
and enable its `Enabled` property. Reopen the view after changing settings. Views with no 3D candidates allocate
no HZB resources. Games without the Brick and servers without rendering run no HZB work.

For an explicit comparison, set `SceneViewerService.UseOcclusionCulling` or
`RendererManager.UseOcclusionCulling`. `OcclusionStats` reports the last completed use of the frame slot, so the
counts lag the current submission. CPU `CullingStats.Submitted` remains the number of candidates sent to rendering.
Resources are retained for reuse until the view is closed or resized; disabling culling skips its passes.

## Rendering

The current frame's opaque submeshes first fill a depth-only target with the same sample count and transforms as
the color pass. Compute takes the maximum depth across every MSAA sample, pads the base level to powers of two
with clear depth, then builds a maximum-depth pyramid. Another compute dispatch projects each world-space AABB
and writes one indirect command in the existing sorted order. The color pass consumes these commands without a
CPU visibility readback.

Near-plane intersections, unknown bounds, uncovered samples and uncertain depth comparisons remain visible.
The depth prepass matches the engine's current opaque PBR renderer. A material path that discards fragments,
deforms vertices or disables depth writes needs a matching depth path before it can participate as an occluder.

This first version draws all candidates in the depth prepass. It incurs an extra full-size MSAA depth target,
an R32F pyramid and compute work; fewer color draws do not guarantee a faster frame. Previous-frame depth,
camera reprojection and prepass culling are subsequent work. The feature is disabled by default.

## Build

Shader sources and embedded SPIR-V are under `Runtime/Shaders~`. The project rebuilds changed
shaders with `glslc` (skipped with `-p:SkipShaders=true`) and copies the library into `Brick~/Precast/lib`. It builds against a Turian
checkout beside this repository (or `-p:TurianDir=<path>`):

```sh
dotnet test --project Brick~/Tests/Turian.Hzb.Tests
```

## Source and baked packages

The regular brick contains editable code under `Runtime/` and/or `Editor/`; its assembly definitions let any Turian client compile it without an engine checkout. Build and test projects live under the opaque `Brick~/` roles. Keep committed `.meta` identities when editing or publishing.

`turian-cli brick pack . --precast --twin --out .bricks` compiles the code, imports the assets and writes the regular brick plus its `.bake` twin. The source archive omits generated payloads; the twin includes the assemblies and verified import cache, with no C# source. `brick publish . --store <store> --key <key> --precast --twin` performs the same producer steps before upload; protected production CI signs and uploads the qualified archive bytes.

Consumers can enable `brick twins on --project <project>`, then `brick swap org.mass4.turian.hzb source --project <project>` to download and embed writable originals at the same version. Swapping back to `twin` preserves the edited fork in the project trash. GLSL edits in HZB require rebuilding its shader payload with the build project before packing.
