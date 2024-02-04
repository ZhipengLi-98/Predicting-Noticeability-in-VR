using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Tobii.XR;
using System.IO;
using Valve.VR.InteractionSystem;
using Valve.VR;
using System;
using UnityEngine.UI;
using RockVR.Video;

public class ColorManager : MonoBehaviour
{
    public GameObject VirtualHome;
    public GameObject VirtualLab;
    public GameObject VirtualCafe;
    public GameObject PhysicalHome1;
    public GameObject PhysicalHome2;
    public GameObject PhysicalHome3;

    public enum Background {
        VirtualHome, VirtualLab, VirtualCafe, PhysicalHome1, PhysicalHome2, PhysicalHome3
    }
    public Background curBackground;

    public SteamVR_Action_Boolean notice;
    public SteamVR_Action_Boolean capture;

    public SteamVR_Input_Sources controller;
    public bool isCapture = false;

    public VideoCaptureCtrl captureCtrl;

    public List<GameObject> typingUserInterefaces = new List<GameObject>();
    public List<GameObject> typingIconList = new List<GameObject>();
    public List<GameObject> typingViewerList = new List<GameObject>();
    public List<GameObject> videoUserInterefaces = new List<GameObject>();
    public List<GameObject> videoIconList = new List<GameObject>();
    public List<GameObject> videoViewerList = new List<GameObject>();
    private List<GameObject> userInterefaces;
    private List<GameObject> iconList;
    private List<GameObject> viewerList;
    public GameObject videoPlayer;
    public GameObject keyboard;
    public GameObject pointer;

    public bool isVideo;

    public GameObject camera;

    // [Range(1, 3)]
    // public int startLevel = 1;

    private int INIT_FRAMES = 1200;

    public int augFrames;
    public int curFrames = 0;

    private float timer = 0f;

    public bool colorAug = false;

    public string user = "test.txt";
    private StreamWriter writer;

    public GameObject curObject;

    private int augLayer;
    private int norLayer;

    private string layoutFile = "./layout.txt";
    private List<Dictionary<string, List<Vector3>>> layout;
    private int layoutCnt = 0;

    private bool isAug = false;
    private float augTimer = 0f;

    private bool isWait = false;
    private float waitTimer = 0f;

    private float curHue = 0f;
    private float curSat = 0f;
    private bool satFlag = true;
    private float targetHue = 0f;
    private float error = 1e-6f;

    private UnityEngine.Video.VideoPlayer player;

    public ChangeText changeText;

    string Vector3ToString(Vector3 v)
    {
        string res = v.x + " " + v.y + " " + v.z;
        return res;
    }

    string QuaternionToString(Quaternion q)
    {
        string res = q.x + " " + q.y + " " + q.z + " " + q.w;
        return res;
    }

    public void CaptureDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        isCapture = !isCapture;
        if (isCapture)
        {
            captureCtrl.StartCapture();
        }
        else
        {
            captureCtrl.StopCapture();
        }
    }
    
    public void TriggerUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        // Set as white
        curObject.layer = norLayer;
        curObject.GetComponent<Renderer>().material.color = Color.HSVToRGB(curHue, 0f, 1.0f);
        writer.WriteLine("Noticed" + " " + Time.time);
        writer.Flush();
        
        augTimer = UnityEngine.Random.Range(5, 15);
        isAug = true;
        colorAug = false;
        NextLayout();
    }

    public void TriggerDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        // Set as black
        curObject.GetComponent<Renderer>().material.color = Color.HSVToRGB(curHue, 1f, 0f);
        colorAug = false;
        augFrames = INIT_FRAMES;
        curFrames = 0;
        print(Time.time);
    }
    
    void ReadLayout()
    {
        StreamReader reader = new StreamReader(layoutFile);
        string[] content = reader.ReadToEnd().Split(new string[] { "Start" }, StringSplitOptions.None);
        for (int i = 1; i < content.Length; i++)
        {
            string[] line = content[i].Split('\n');
            Dictionary<string, List<Vector3>> cur = new Dictionary<string, List<Vector3>>();
            for (int j = 1; j < line.Length - 1; j++)
            {
                string[] temp = line[j].Split(new string[] { ", " }, StringSplitOptions.None);
                if (!cur.ContainsKey(temp[0]))
                {
                    List<Vector3> n = new List<Vector3>();
                    cur.Add(temp[0], n);
                }
                cur[temp[0]].Add(new Vector3(float.Parse(temp[1]), float.Parse(temp[2]), float.Parse(temp[3])));
            }
            layout.Add(cur);
        }
    }

    private void NextLayout()
    {
        List<int> list = new List<int>();
        for (int n = 0; n < iconList.Count; n++) 
        {
            list.Add(n);
        }
        foreach (GameObject icon in iconList)
        {
            int index = UnityEngine.Random.Range(0, list.Count - 1);
            int i = list[index];
            list.RemoveAt(index);
            // icon.transform.position = camera.transform.position + camera.transform.rotation * (layout[layoutCnt]["Icon"][i] - new Vector3(0f, 1.4f, 0f));
            icon.transform.position = layout[layoutCnt]["Icon"][i] - new Vector3(0f, 0f, 0.3f);
            icon.transform.LookAt(camera.transform);
            if (icon.transform.name == "HMDModel")
            {
                icon.transform.rotation = icon.transform.rotation * Quaternion.Euler(0, 180, 0);
            }
            if (icon.transform.name == "TimeWidget")
            {
                icon.transform.rotation = icon.transform.rotation * Quaternion.Euler(0, 180, 0);
            }
            if (icon.transform.name == "10621_CoastGuardHelicopter")
            {
                icon.transform.rotation = icon.transform.rotation * Quaternion.Euler(-90, -45, 0);
            }
            if (icon.transform.name == "Controller")
            {
                icon.transform.rotation = icon.transform.rotation * Quaternion.Euler(-90, 180, 0);
            }
            // icon.GetComponent<Renderer>().material.color = Color.HSVToRGB(UnityEngine.Random.Range(0f, 1f), 1.0f, 1.0f);
        }
        list.Clear();
        for (int n = 0; n < viewerList.Count; n++) 
        {
            list.Add(n);
        }
        foreach (GameObject viewer in viewerList)
        {
            int index = UnityEngine.Random.Range(0, list.Count - 1);
            int i = list[index];
            list.RemoveAt(index);
            // viewer.transform.position = camera.transform.position + camera.transform.rotation * (layout[layoutCnt]["Viewer"][i] - new Vector3(0f, 1.4f, 0f));
            viewer.transform.position = layout[layoutCnt]["Viewer"][i] - new Vector3(0f, 0f, 0.3f);
            viewer.transform.LookAt(camera.transform);
            // viewer.GetComponent<Renderer>().material.color = Color.HSVToRGB(UnityEngine.Random.Range(0f, 1f), 1.0f, 1.0f);
        }
        // keyboard.transform.position = camera.transform.position + camera.transform.rotation * (layout[layoutCnt]["Keyboard"][0] - new Vector3(0f, 1.4f, 0f));
        keyboard.transform.position = layout[layoutCnt]["Keyboard"][0] - new Vector3(0f, 0f, 0.3f);
        keyboard.transform.LookAt(camera.transform);
        keyboard.transform.rotation = keyboard.transform.rotation * Quaternion.Euler(0, 180, 0);
        // videoPlayer.transform.position = camera.transform.position + camera.transform.rotation * (layout[layoutCnt]["VideoPlayer"][0] - new Vector3(0f, 1.4f, 0f));
        videoPlayer.transform.position = layout[layoutCnt]["VideoPlayer"][0] - new Vector3(0f, 0f, 0.3f);
        videoPlayer.transform.LookAt(camera.transform);
        int next = UnityEngine.Random.Range(0, layout.Count);
        while (next == layoutCnt)
        {
            next = UnityEngine.Random.Range(0, layout.Count);
        }
        layoutCnt = next;
        
        curObject.layer = norLayer;
        curObject.GetComponent<Renderer>().material.color = Color.HSVToRGB(curHue, 0f, 1.0f);
        curObject = userInterefaces[UnityEngine.Random.Range(0, userInterefaces.Count)];
        curHue = UnityEngine.Random.Range(0f, 1f);
        while (Math.Abs(curHue - 0f) < 0.1f || Math.Abs(curHue - 0.66f) < 0.1f || Math.Abs(curHue - 0.25f) < 0.1f)
        {
            curHue = UnityEngine.Random.Range(0f, 1f);
        }
        satFlag = true;
        curSat = 0f;
        curObject.GetComponent<Renderer>().material.color = Color.HSVToRGB(curHue, curSat, 1.0f);
        print(curObject.transform.name);
        
        if (!isVideo)
        {
            changeText.inputField.Select();
            changeText.inputField.text = "";
            changeText.tmp.text = changeText.sentences[changeText.cnt];
            changeText.cnt += 1;
            changeText.cnt %= changeText.sentences.Count;
        }
        foreach (GameObject ui in userInterefaces)
        {
            ui.transform.position += new Vector3(UnityEngine.Random.Range(-0.02f, 0.02f), UnityEngine.Random.Range(-0.02f, 0.02f), 0f);
            ui.transform.localScale = UnityEngine.Random.Range(0.9f, 1.1f) * ui.transform.localScale;
        }
        if (viewerList.Contains(curObject))
        {
            INIT_FRAMES = 780;
        }
        else if  (iconList.Contains(curObject))
        {
            INIT_FRAMES = 300;
        }
        // int t = UnityEngine.Random.Range(1, 4);
        // startLevel = t;
        // if (startLevel == 1)
        // {
        //     INIT_FRAMES = 600;
        // }
        // else if (startLevel == 2)
        // {
        //     INIT_FRAMES = 450;
        // }
        // else if (startLevel == 3)
        // {
        //     INIT_FRAMES = 300;
        // }
        augFrames = UnityEngine.Random.Range(INIT_FRAMES * 2 / 3, INIT_FRAMES * 5 / 4);

        player = videoPlayer.GetComponent<UnityEngine.Video.VideoPlayer>();
        int videoIndex = UnityEngine.Random.Range(1, 20);
        player.url = "./Assets/Videos/" + videoIndex + ".mp4";
    }

    // Start is called before the first frame update
    void Start()
    {
        INIT_FRAMES = 1200;
        // if (startLevel == 1)
        // {
        //     INIT_FRAMES = 1200;
        // }
        // else if (startLevel == 2)
        // {
        //     INIT_FRAMES = 900;
        // }
        // else if (startLevel == 3)
        // {
        //     INIT_FRAMES = 600;
        // }
        augLayer = LayerMask.NameToLayer("AugObj");
        norLayer = LayerMask.NameToLayer("NorObj");

        if (isVideo)
        {
            userInterefaces = videoUserInterefaces;
            iconList = videoIconList;
            viewerList = videoViewerList;
            foreach (GameObject g in typingUserInterefaces)
            {
                g.SetActive(false);
            }
            foreach (GameObject g in videoUserInterefaces)
            {
                g.SetActive(true);
            }
            videoPlayer.SetActive(true);
            keyboard.SetActive(false);
            pointer.SetActive(false);
        }
        else
        {
            userInterefaces = typingUserInterefaces;
            iconList = typingIconList;
            viewerList = typingViewerList;
            foreach (GameObject g in videoUserInterefaces)
            {
                g.SetActive(false);
            }
            foreach (GameObject g in typingUserInterefaces)
            {
                g.SetActive(true);
            }
            videoPlayer.SetActive(false);
            keyboard.SetActive(true);
            pointer.SetActive(true);
        }

        VirtualHome.SetActive(false);
        VirtualLab.SetActive(false);
        VirtualCafe.SetActive(false);
        PhysicalHome1.SetActive(false);
        PhysicalHome2.SetActive(false);
        PhysicalHome3.SetActive(false);
        switch (curBackground)
        {
            case (Background.VirtualHome):
                VirtualHome.SetActive(true);
                break;
            case (Background.VirtualLab):
                VirtualLab.SetActive(true);
                break;
            case (Background.VirtualCafe):
                VirtualCafe.SetActive(true);
                break;
            case (Background.PhysicalHome1):
                PhysicalHome1.SetActive(true);
                break;
            case (Background.PhysicalHome2):
                PhysicalHome2.SetActive(true);
                break;
            case (Background.PhysicalHome3):
                PhysicalHome3.SetActive(true);
                break;
            default:
                break;
        }
        
        notice.AddOnStateUpListener(TriggerUp, controller);
        notice.AddOnStateDownListener(TriggerDown, controller);
        capture.AddOnStateDownListener(CaptureDown, controller);
        
        writer = new StreamWriter(user, false);
        
        augFrames = INIT_FRAMES;
        curObject = userInterefaces[UnityEngine.Random.Range(0, userInterefaces.Count)];
        curHue = UnityEngine.Random.Range(0f, 1f);
        satFlag = true;
        curSat = 0f;
        curObject.GetComponent<Renderer>().material.color = Color.HSVToRGB(curHue, curSat, 1.0f);
        print(curObject.transform.name);

        layout = new List<Dictionary<string, List<Vector3>>>();
        ReadLayout();
        NextLayout();
    }

    // Update is called once per frame
    void Update()
    {
        // var eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);
        if (Input.GetKeyDown(KeyCode.A))
        {
            augTimer = UnityEngine.Random.Range(5, 15);
            isAug = true;
            colorAug = false;
            NextLayout();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            curObject.layer = norLayer;
            curObject.GetComponent<Renderer>().material.color = Color.HSVToRGB(curHue, 0f, 1.0f);
            curObject = userInterefaces[UnityEngine.Random.Range(0, userInterefaces.Count)];
            curHue = UnityEngine.Random.Range(0f, 1f);
            satFlag = true;
            curSat = 0f;
            curObject.GetComponent<Renderer>().material.color = Color.HSVToRGB(curHue, curSat, 1.0f);
            print(Time.time);
        }
        if (augTimer > 0)
        {
            augTimer -= Time.deltaTime;
        }
        if (augTimer <= 0 && isAug)
        {
            isAug = false;
            colorAug = true;
            writer.WriteLine(curObject.transform.name + " " + Time.time);
            writer.Flush();
        }
        if (colorAug)
        {   
            curObject.layer = augLayer;
            string t = "Camera: " + Vector3ToString(camera.transform.position) + " " + QuaternionToString(camera.transform.rotation) + " " + Time.time;
            writer.WriteLine(t);
            writer.Flush();

            curFrames = (curFrames + 1) % (augFrames);
            if (satFlag)
            {
                curSat += (1f / augFrames);
                if (curSat > 1f)
                {
                    satFlag = false;
                }
            }
            else
            {
                curSat -= (1f / augFrames);
                if (curSat < 0f)
                {
                    satFlag = true;
                }            
                if (curFrames == 0)
                {
                    if (augFrames > 3 * INIT_FRAMES / 10)
                    {
                        augFrames = (int) (augFrames - INIT_FRAMES / 5);
                    }
                }
            }
            // curHue += (1.0f / augFrames);
            // if (curHue - 1.0f > error)
            // {
            //     curHue -= 1.0f;
            // }
            curObject.GetComponent<Renderer>().material.color = Color.HSVToRGB(curHue, curSat, 1.0f);
        }
    }

    // void OnApplicationQuit()
    // {
    //     writer.Flush();
    //     writer.Close();
    // }
}
