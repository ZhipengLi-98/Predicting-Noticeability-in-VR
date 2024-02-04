using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalExperimentState
{
    // Experiment scene order
    private static string[][] scenes = new string[][] {
        new string[]{"experiment_scene_leisure_coffeeshop", "experiment_scene_leisure_livingroom"},
        new string[]{ "experiment_scene_workspace_office_public", "experiment_scene_workspace_bedroom" }
    };
    public static string[] sceneOrder = new string[5];
    // Current scene index
    private static int currScene = 0;

    // Experiment Menu enabled 
    private static bool experimentMenuEnabled = false;
    private static bool viewPastEnabled = false; 

    // Capture textures: Views of previous scenes
    private static Texture2D[] captureTexturesFront = new Texture2D[4];
    private static Texture2D[] captureTexturesLeft = new Texture2D[4];
    private static Texture2D[] captureTexturesRight = new Texture2D[4];

    // Set experiment order 
    public static void setSceneOrder()
    {
        currScene = 0; 
        sceneOrder = new string[5];
        int scenario = Random.Range(0, 2);
        int scene = Random.Range(0, 2);
        //sceneOrder[0] = scenes[scenario][scene];
        //sceneOrder[1] = scenes[scenario][(scene + 1) % 2];
        sceneOrder[0] = scenes[scenario][0];
        sceneOrder[1] = scenes[scenario][1];
        scene = Random.Range(0, 2);
        //sceneOrder[2] = scenes[(scenario + 1) % 2][scene];
        //sceneOrder[3] = scenes[(scenario + 1) % 2][(scene + 1) % 2];
        sceneOrder[2] = scenes[(scenario + 1) % 2][0];
        sceneOrder[3] = scenes[(scenario + 1) % 2][1];
        sceneOrder[4] = "experiment_end";
    }

    // Get current scene
    public static string getCurrScene()
    {
        return sceneOrder[currScene];
    }
    public static int getCurrSceneIdx()
    {
        return currScene;
    }

    // Increment scene index
    public static void nextScene()
    {
        currScene++; 
    }

    // Enable experiment menu 
    public static void enableExperimentMenu(bool enabled)
    {
        experimentMenuEnabled = enabled; 
    }
    // Check for experiment menu state
    public static bool isExperimentMenuEnabled()
    {
        return experimentMenuEnabled;
    }

    // Capture environment 
    public static void cameraCaptureFront(Camera camera)
    {
        RenderTexture capture = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 16);
        camera.targetTexture = capture;
        RenderTexture.active = capture;

        camera.Render();
        int width = camera.targetTexture.width;
        int height = camera.targetTexture.height;
        captureTexturesFront[currScene] = new Texture2D(width, height);
        captureTexturesFront[currScene].ReadPixels(new Rect(0, 0, width, height), 0, 0);
        captureTexturesFront[currScene].Apply();

        RenderTexture.active = null;
        camera.targetTexture = null;
    }
    public static void cameraCaptureLeft(Camera camera)
    {
        RenderTexture capture = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 16);
        camera.targetTexture = capture;
        RenderTexture.active = capture;

        camera.Render();
        int width = camera.targetTexture.width;
        int height = camera.targetTexture.height;
        captureTexturesLeft[currScene] = new Texture2D(width, height);
        captureTexturesLeft[currScene].ReadPixels(new Rect(0, 0, width, height), 0, 0);
        captureTexturesLeft[currScene].Apply();

        RenderTexture.active = null;
        camera.targetTexture = null;
    }
    public static void cameraCaptureRight(Camera camera)
    {
        RenderTexture capture = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 16);
        camera.targetTexture = capture;
        RenderTexture.active = capture;

        camera.Render();
        int width = camera.targetTexture.width;
        int height = camera.targetTexture.height;
        captureTexturesRight[currScene] = new Texture2D(width, height);
        captureTexturesRight[currScene].ReadPixels(new Rect(0, 0, width, height), 0, 0);
        captureTexturesRight[currScene].Apply();

        RenderTexture.active = null;
        camera.targetTexture = null;
    }

    // Get capture texture
    public static Texture2D getCaptureTextureFront(int scene)
    {
        return captureTexturesFront[scene];
    }
    public static Texture2D getCaptureTextureLeft(int scene)
    {
        return captureTexturesLeft[scene];
    }
    public static Texture2D getCaptureTextureRight(int scene)
    {
        return captureTexturesRight[scene];
    }

    // Enable viewing of previous set-up
    public static void enableViewPast(bool enabled)
    {
        viewPastEnabled = enabled;
    }
    // Check for experiment menu state
    public static bool isViewPastEnabled()
    {
        return viewPastEnabled;
    }
}
