# UnityVR-Eye_tracking-Demo
### This repository aims to develop a small VR demo using [Pupil-labs](https://pupil-labs.com/) eye_tracking function. The developing team of the Pupil-lab have already integrated their hardware [(HTC-Vive Add-on)](https://docs.pupil-labs.com/vr-ar/htc-vive/) with [Unity3D](https://unity.com/cn) and realized the eye-tracking function in VR scene. To understand the workflow and implementation of the original Unity3D + Pupil Capture/Service, please refer to their [repo](https://github.com/pupil-labs/hmd-eyes/blob/master/docs/Developer.md) and play with their demos to get your hand dirty. Of course you can directly download the latest release and start.


## Explanation of the Random_Selecting demo:
- ### ✔️Basic Implementation: 
  - At first press **keycode C** to enter calibration process, and 3*6 = 18 points show in the scene to adjust the mapping content between your real gaze point and the point in VR scene.
  - First ten cubes marked with number 0~9 are instantiated randomly in Z-axis fixed 2D surface [*(why 2D?)*]().
