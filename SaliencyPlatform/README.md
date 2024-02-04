# SaliencyPlatform

## File structure

 - ./Assets/Scenes
    - SampleScene: the main scene

 - ./Assets/Script
    - ChangePosition: enable moving elements in the restoring UI layout task
    - ChangeText: enable change the sentences in the typing task
    - ColorManager: enable color animation and noticing logistic
    - GetGaze: get the gaze data and render a gaze cursor
    - ScaleManager: enable scale animation and noticing logistic
    - PositionManager: enable position moving animation and noticing logistic

## Requirement

 - RockVR: the package to record multiple camera's view. It is in the Assets.
 - SteamVR Input: I use this to detect the controller event. I save the controller setting as ./export_application_generated_unity_saliencyplatform_exe_vive_controller_______ Vive Controller _________ [Testing] SaliencyPlatform ______.json

## Operation

1. Primary task
 - if typing task: enable Canvas, enable Manager.ChangeText, disable VideoPlayer
 - if watching video task: enable Videoplayer, disable Manager.ChangeText, disable Canvas
 - if restoring layout task: enable Manager.ChangePosition, disable Canvas, VideoPlayer, and Manager.ChangeText

2. Animation type
 - if scaled animation, enable Manager.ScaleManager, disable Manager.ColorManager, Manager.PositionManager
 - if color animation, enable Manager.ColorManager, disable Manager.ScaleManager, Manager.PositionManager
 - if moving animation, enable Manager.PositionManager, disable Manager.ScaleManager, Manager.ColorManager

3. Background
 - select CurBackground of Manager.

4. Conduct the eye calibration in Vive, then play the scene 

5. Record video
 - there is a "start record" button in Unity at the left down corner, press it and it will record the user's view, the animation and the gaze map.

6. Change the layout and start a trial
 - press "A" on your keyboard to change the layout and start a trial, the animation will start after a random interal (5s-15s).

7. Controller operation
 - typing task: use right controller to type, first click the white input box and then start to type with the index trigger, press the middle finger trigger to change the target sentence
 - restoring task: use right controller to select an element with the index trigger to then move it
 - watching video task: no operation
 - noticing: press the left controller's index trigger, the augmented object will turn to red in the scaled animation and turn to black in the color animation

