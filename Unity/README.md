# MineXR - Widget Layout Preview & Reconstruction

## HoloLens Preview
1. Download Unity (Editor version: 2020.3.12).
2. Download Visual Studio 2022. Install “Universal Windows Platform Development” and check off the Optional C++ workload.
3. In Unity, check the following configuration in File -> Build Settings

4. Click `Build`.
5. In the Build folder, find `HelloAR U3D.sln` and open it in Visual Studio. 
6. Configure as `Debug | ARM64 | Device` and run.

Reference: [https://learn.microsoft.com/en-us/windows/mixed-reality/develop/advanced-concepts/using-visual-studio?tabs=hl2](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/advanced-concepts/using-visual-studio?tabs=hl2)

## Reconstruction tool
1. In Unity Editor, load the Reconstruction Scene (`Scenes/ReconstructionScene`).
2. Click the GameObject named `AzureSpatialAnchors`.
3. In the Inspector pane on the right side of the Unity Editor, find the `Reconstruction Script` and three fields `Participant Id`, `Env Id`, and `Task Id`.
4. Fill in the three fields o fthe scenario you want to load. Refer to the dataset for correct names.
5. Click the Run button.
6. Click the 'Reconstruct' button to load anchors from the scene. Wait until all anchors are loaded.
7. Once all anchors are loaded, click the 'Show All' button to instantiate all the anchors. The current version (2023.08.28) shows all widgets after all the updates.
8. Click the Scene tab and use the mouse to browse the scene.


Also see Microsoft's [quickstart guide](https://docs.microsoft.com/en-us/azure/spatial-anchors/unity-overview) for sample instructions about Spatial Anchors.