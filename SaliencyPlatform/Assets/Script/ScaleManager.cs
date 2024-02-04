using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Tobii.XR;
using System.IO;
using Valve.VR.InteractionSystem;
using Valve.VR;
using System;
using RockVR.Video;

public class ScaleManager : MonoBehaviour
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

    public GameObject camera;

    // [Range(1, 3)]
    // public int startLevel = 1;

    private int INIT_FRAMES = 600;

    public int augFrames;
    public int curFrames = 0;

    private float timer = 0f;

    public bool scaleAug = false;
    public Vector3 oriScale;
    public Vector3 tarScale;
    public Vector3 minScale;
    public Vector3 maxScale;
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

    public GameObject curObject;

    public string user = "test.txt";
    private StreamWriter writer;

    public bool ifGaze = false;

    private int augLayer;
    private int norLayer;

    public Material redMaterial;
    private Material oriMaterial;
    private GameObject redObject;

    private string layoutFile = "./layout.txt";
    private List<Dictionary<string, List<Vector3>>> layout;
    private int layoutCnt = 0;

    private bool isAug = false;
    private float augTimer = 0f;

    private bool isWait = false;
    private float waitTimer = 0f;

    private float curHue = 0f;

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

    // Start is called before the first frame update
    void Start()
    {
        INIT_FRAMES = 900;
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

        if (!ifGaze)
        {
            notice.AddOnStateUpListener(TriggerUp, controller);
            notice.AddOnStateDownListener(TriggerDown, controller);
            capture.AddOnStateDownListener(CaptureDown, controller);
        }

        writer = new StreamWriter(user, false);

        augFrames = INIT_FRAMES;

        layout = new List<Dictionary<string, List<Vector3>>>();
        ReadLayout();
        NextLayout();
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
        
        if (curObject != null)
        {
            curObject.transform.localScale = minScale;
            curObject.layer = norLayer;
            // curObject.GetComponent<Renderer>().material = oriMaterial;
            // curObject.GetComponent<Renderer>().material.color = Color.HSVToRGB(curHue, 0f, 1.0f);
        }
        foreach (GameObject ui in userInterefaces)
        {
            ui.transform.position += new Vector3(UnityEngine.Random.Range(-0.02f, 0.02f), UnityEngine.Random.Range(-0.02f, 0.02f), 0f);
            ui.transform.localScale = UnityEngine.Random.Range(0.9f, 1.1f) * ui.transform.localScale;
        }
        curObject = userInterefaces[UnityEngine.Random.Range(0, userInterefaces.Count)];
        curHue = curObject.GetComponent<Renderer>().material.color[0];
        minScale = new Vector3(curObject.transform.localScale.x, curObject.transform.localScale.y, curObject.transform.localScale.z);
        maxScale = new Vector3(1.5f * curObject.transform.localScale.x, 1.5f * curObject.transform.localScale.y, 1.5f * curObject.transform.localScale.z);
        oriScale = minScale;
        tarScale = maxScale;
        oriMaterial = curObject.GetComponent<Renderer>().material;

        if (!isVideo)
        {
            changeText.inputField.Select();
            changeText.inputField.text = "";
            changeText.tmp.text = changeText.sentences[changeText.cnt];
            changeText.cnt += 1;
            changeText.cnt %= changeText.sentences.Count;
        }
        
        if (viewerList.Contains(curObject))
        {
            INIT_FRAMES = 660;
        }
        else if  (iconList.Contains(curObject))
        {
            INIT_FRAMES = 180;
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
        // augFrames = INIT_FRAMES;

        player = videoPlayer.GetComponent<UnityEngine.Video.VideoPlayer>();
        int videoIndex = UnityEngine.Random.Range(1, 20);
        player.url = "./Assets/Videos/" + videoIndex + ".mp4";
    }

    public void TriggerUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        curObject.transform.localScale = minScale;
        curObject.layer = norLayer;
        // curObject.GetComponent<Renderer>().material = oriMaterial;
        curObject.GetComponent<Renderer>().material.color = Color.HSVToRGB(curHue, 0f, 1.0f);
        curObject = userInterefaces[UnityEngine.Random.Range(0, userInterefaces.Count)];
        curHue = curObject.GetComponent<Renderer>().material.color[0];
        print(curObject.transform.name);
        writer.WriteLine("Noticed" + " " + Time.time);
        writer.Flush();
        
        minScale = new Vector3(curObject.transform.localScale.x, curObject.transform.localScale.y, curObject.transform.localScale.z);
        maxScale = new Vector3(1.5f * curObject.transform.localScale.x, 1.5f * curObject.transform.localScale.y, 1.5f * curObject.transform.localScale.z);

        oriScale = minScale;
        tarScale = maxScale;
        
        augTimer = UnityEngine.Random.Range(5, 15);
        isAug = true;
        scaleAug = false;
        curFrames = 0;
        NextLayout();
    }

    public void TriggerDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        // oriMaterial = curObject.GetComponent<Renderer>().material;
        // print(oriMaterial.name);
        // curObject.GetComponent<Renderer>().material = redMaterial;
        curObject.GetComponent<Renderer>().material.color = Color.HSVToRGB(curHue, 1.0f, 0f);
        scaleAug = false;
        augFrames = INIT_FRAMES;
        curFrames = 0;
    }

    private void NoticeDown()
    {
        oriMaterial = curObject.GetComponent<Renderer>().material;
        print(oriMaterial.name);
        curObject.GetComponent<Renderer>().material = redMaterial;
        scaleAug = false;
        augFrames = INIT_FRAMES;
        curFrames = 0;
    }

    private void NoticeUp()
    {
        curObject.transform.localScale = minScale;
        curObject.layer = norLayer;
        curObject.GetComponent<Renderer>().material = oriMaterial;
        curObject = userInterefaces[UnityEngine.Random.Range(0, userInterefaces.Count)];
        curObject.layer = augLayer;
        print(curObject.transform.name);
        writer.WriteLine("Noticed" + " " + Time.time);
        writer.Flush();
        
        minScale = new Vector3(curObject.transform.localScale.x, curObject.transform.localScale.y, curObject.transform.localScale.z);
        maxScale = new Vector3(1.5f * curObject.transform.localScale.x, 1.5f * curObject.transform.localScale.y, 1.5f * curObject.transform.localScale.z);

        oriScale = minScale;
        tarScale = maxScale;
    }

    void OnApplicationQuit()
    {
        writer.Flush();
        writer.Close();
    }

    // Update is called once per frame
    void Update()
    {        
        if (Input.GetKeyDown(KeyCode.A))
        {
            // Random time interval before starting animation
            augTimer = UnityEngine.Random.Range(5, 15);
            isAug = true;
            scaleAug = false;
            curFrames = 0;
            NextLayout();
        }
        if (augTimer > 0)
        {
            augTimer -= Time.deltaTime;
        }
        else if (augTimer <= 0 && isAug)
        {
            isAug = false;
            scaleAug = true;
            isWait = false;
            curFrames = 0;
            writer.WriteLine(curObject.transform.name + " " + Time.time);
            writer.Flush();
        }
        // if (Input.GetKeyDown(KeyCode.S))
        // {
        //     NoticeDown();
        //     NoticeUp();
        // }
        // if (Input.GetKeyDown(KeyCode.Z))
        // {
        //     NextLayout();
        // }
        else if (scaleAug)
        {   
            curObject.layer = augLayer;
            // waitTimer is a random time interval to prevent the sudden change between scaling up and down
            if (waitTimer > 0)
            {
                waitTimer -= Time.deltaTime;
            }
            if (waitTimer <= 0 && isWait)
            {
                isWait = false;
                Vector3 temp = oriScale;
                oriScale = tarScale;
                tarScale = temp;
            }
            if (!isWait)
            {
                string t = "Camera: " + Vector3ToString(camera.transform.position) + " " + QuaternionToString(camera.transform.rotation) + " " + Time.time;
                writer.WriteLine(t);
                writer.Flush();

                curFrames = (curFrames + 1) % (augFrames + 1);
                float interpolationRatio = (float) curFrames / augFrames;
                Vector3 interpolatedScale = Vector3.Lerp(oriScale, tarScale, interpolationRatio);
                curObject.transform.localScale = interpolatedScale;
                // if (interpolationRatio == 1)
                // if (Mathf.Approximately(interpolationRatio, 1f))
                if (Mathf.Approximately(curFrames, augFrames))
                {
                    if (tarScale.x < oriScale.x)
                    {
                        if (augFrames > 3 * INIT_FRAMES / 10)
                        {
                            augFrames = (int) (augFrames - INIT_FRAMES / 5);
                        }
                    }
                    curFrames = -1;
                    isWait = true;
                    waitTimer = UnityEngine.Random.Range(1f, 3f);
                }
            }
        }

        // var eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);
        // // record gaze data
        // if (ifGaze && eyeTrackingData.GazeRay.IsValid)
        // {
        //     string g = "Gaze: " + Vector3ToString(eyeTrackingData.GazeRay.Origin) + " " + Vector3ToString(eyeTrackingData.GazeRay.Direction) + " " + Time.time;
        //     writer.WriteLine(g);
        //     writer.Flush();
        //     int layerMask = 1 << 6 | 1 << 7;
        //     RaycastHit hit;
        //     if (Physics.Raycast(eyeTrackingData.GazeRay.Origin, eyeTrackingData.GazeRay.Direction, out hit, Mathf.Infinity, layerMask))
        //     {
        //         if (Equals(hit.transform, curObject.transform))
        //         {
        //             timer += Time.deltaTime;
        //         }
        //         else
        //         {
        //             timer = 0f;
        //         }
        //     }
        // }
        // if (timer >= 1.5f && scaleAug)
        // {
        //     NoticeDown();
        // }
        // if (timer >= 1.55f)
        // {
        //     NoticeUp();
        //     timer = 0f;
        // }
    }
}
