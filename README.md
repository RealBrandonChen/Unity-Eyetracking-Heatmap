# Unity-Eyetracking-Heatmap
### This repository aims to develop a small VR demo to realize [Pupil-labs](https://pupil-labs.com/) eye_tracking function and postprocessing heatmap. The developing team of the Pupil-lab have already integrated their hardware [(HTC-Vive Add-on)](https://docs.pupil-labs.com/vr-ar/htc-vive/) with [Unity3D](https://unity.com/cn) and realized the eye-tracking function in VR scene. To understand the workflow and implementation of the original Unity3D + Pupil Capture/Service, please refer to their [repo](https://github.com/pupil-labs/hmd-eyes/blob/master/docs/Developer.md) and play with their demos to get your hand dirty. Of course you can directly download the latest [release]() and get started.
|<img src="/Images/demo.gif" width="500" height="350">      |<img src="/Images/heatmap.gif" width="500" height="350">   |
| --------------------------------------                    | -----------                                               |
| *Figure1. Eye tracking demo with ApriTag*                 | *Figure2. Corresponding heatmap*                          |


## Explanation of the Random_Selecting demo:
  - At first press **keycode C** to enter calibration process, and 3*6 = 18 points show in the scene to adjust the mapping content between your real gaze point and the point in VR scene.
  - After calibration, the canvas reminder should notice you that the pupil-lab is connected if you set the equipment right. Ten cubes marked with number 0~9 are instantiated randomly in Z-axis fixed 2D surface [*(why 2D?)*]().
  - Try not to cause relative slippage between your head and the headset as the mapping is crucial for eye_tracking. 
  - Now you can look around and you'll find a red point and a green point also move in the scene. These two points represent your gaze point of your left eye(red) and right eye(green), respectively. When they overlap at the same cube, which also means you stare at the cube, the cube turns blue immediately, and it turns red if you keep looking at it for over 3 seconds.
## Heatmap generation
  - Generate the postprocessing data using pupil-labs service software. The software should automatically open a window that shows the record of your testing.
  - Click the heatmap button on the sidebar, adapt for the ApriTag and tune the smoothing rate, then you'll get the corresponding heatmap like the gif above.
  - The heatmap function is developed by pupil-labs service, but it's never used in Unity or other virtual environment to my knowledge. This function can be applied in many scenarios like [visual disability evaluation](https://github.com/RealBrandonChen/VisualDisabilitySim) for patients. 
