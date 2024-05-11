# MineXR Source Code and Dataset
This repository contains the source code and dataset from the paper [*MineXR: Mining Personalized Extended Reality Interfaces*](https://augmented-perception.org/publications/2024-minexr.html) published at CHI 2024.

![teaser](./readme_imgs/teaser.png)

## Source Code

![architecture](./readme_imgs/architecture.png)

The source code largely consists of four components in each directory:
* [iOS](./iOS/README.md): code for the iOS mobile app for widget creation and placement
* [Unity](./Unity/README.md): code for simultaneous preview on HoloLens 2 and layout reconstruction in Unity Editor
* [MineXR Data Viewer](./MineXR%20Data%20Viewer/README.md): code for MineXR dataset viewer that loads files locally
* [annotator](./annotator/README.md): code for widget annotation tools with web interface
* [Sharing](./Sharing/README.md): Azure Spatial Anchors Sharing service that supports the anchor sharing between `iOS` and `Unity`

Each directory has more detailed instructions in their `README.md`.
>For quick browsing of MineXR data with least configurations, open `MineXR Data Viewer` directory in Unity 2020.3.12f1. Refer to [MineXR Data Viewer README](./MineXR%20Data%20Viewer/README.md) for more details.



## Dataset

<img src="./readme_imgs/example_widgets.png" alt="example widgets" width="400">
<img src="./readme_imgs/example_layouts.png" alt="example layouts" width="400">

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

## Cite
You can find the citation on the project website [*MineXR: Mining Personalized Extended Reality Interfaces*](https://augmented-perception.org/publications/2024-minexr.html) 


<details>
  <summary>Copyright Notice</summary>
The screenshots contained in the MineXR dataset may contain copyrighted work.

By downloading the MineXR dataset (the "Database"), you (the "Researcher") hereby agree to the following terms and conditions:

1. Carnegie Mellon University makes no representations or warranties regarding the Database, including but not limited to warranties of non-infringement or fitness for a particular purpose.

2. Researcher accepts full responsibility for his or her use of the Database and shall defend and indemnify the MineXR team and Carnegie Mellon University, including their employees, Trustees, officers and agents, against any and all claims arising from Researcher's use of the Database, including but not limited to Researcher's use of any copies of copyrighted images that he or she may create from the Database.

3. Researcher may provide research associates and colleagues with access to the Database provided that they first agree to be bound by these terms and conditions.

4. If Researcher is employed by a for-profit, commercial entity, Researcher's employer shall also be bound by these terms and conditions, and Researcher hereby represents that he or she is fully authorized to enter into this agreement on behalf of such employer.
</details>
