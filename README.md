# MineXR Source Code and Dataset
This repository contains the source code and dataset from the paper [*MineXR: Mining Personzlied Extended Reality Interfaces*](https://augmented-perception.org/publications/2024-minexr.html) published at CHI 2024.

## Source Code
The source code largely consists of four components in each directory:
* [iOS](./iOS/README.md): code for the iOS mobile app for widget creation and placement
* [Unity](./Unity/README.md): code for simultaneous preview on HoloLens 2 and layout reconstruction in Unity Editor
* [MineXR Data Viewer](./MineXR%20Data%20Viewer/README.md): code for MineXR dataset viewer that loads files locally
* [annotator](./annotator/README.md): code for widget annotation tools with web interface
* [Sharing](./Sharing/README.md): Azure Spatial Anchors Sharing service that supports the anchor sharing between `iOS` and `Unity`

Each directory has more detailed instructions in their `README.md`.
>For quick browsing of MineXR data with least configurations, open `MineXR Data Viewer` directory in Unity 2020.3.12f1. Refer to [MineXR Data Viewer README](./MineXR%20Data%20Viewer/README.md) for more details.



## Dataset
The dataset is available as a [zip file](https://drive.google.com/file/d/18NNp5OT3uRggXmFxgpxFju9abXWWeWo6/view?usp=sharing). The file contains the following files:

* `screenshots_widgets/`: This directory contains all image files of screenshots and widgets created by each participant. Private information was redacted during the data collection and through post-processing. 

* `widgets.csv`: This file is the main file for individual widget information.
Each row corresponds to a widget identified by `anchorId`. The `pId`, `envId`, and `taskId` indicate the participant ID, environment, and task from which the widget was created.
It contains annotations of individual widgets collected in the study. 
The annotations include application name (`appName`), screenshot description (`screenDesc`), widget description (`widgetDesc`), excluded parts (`excludedParts`), whether the widget was cropped from a full screenshot (`isCropped`), UI component types included in the widget (`uiTypes`), application category (`appCategory`), etc. 
The `widgetImagePath` and `screenshotImagePath` are the paths inside `screenshots_widgets/` directory. 

* `cluster.csv`: This file contains annotations of widget clusters in every scenario. The annotations include cluster name, task type, cluster description, and list of widgets in the cluster.

* `interaction_history.csv`: This file contains transaction records of all widgets' add/update/delete histories. 
This data informs the temporal sequences of how users compose their XR interface. 
The widgets are identified by `widget_id` that correponds to `anchorId` in `widgets.csv`. 
The widget's translation and orientation are represented as a transform matrix [[m00, m01, m02, m03],...,[m30, m31, m32, m33]].
We provide helper functions in C# to extract translation, rotation, and scale from the transform matrix in [Util.cs]().

* `layout.json`: This file contains the list of widgets per scenario. The file is structured like:
```
"{participantID}_{environment}_{task}": {
  "{widget_id}": [
    [m00, m01, m02, m03],
    ...
    [m30, m31, m32, m33]
  ],
  "{widget_id2}": [...],
  ...
}
```
* `room_scans/`: This directory contains sample 3D scans of rooms where the data were collected. 
