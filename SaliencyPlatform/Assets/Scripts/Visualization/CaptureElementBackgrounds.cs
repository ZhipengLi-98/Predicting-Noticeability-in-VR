using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CaptureElementBackgrounds : MonoBehaviour
{
    /*
    public GameObject[] m_envs;
    public enum Environment { bedroom, office, livingroom, coffeeshop };
    public Environment m_sceneSelection;
    public string m_scene;
    private GameObject m_selectedEnv;

    [Range(1, 12)]
    public int m_participant;

    public GameObject m_visualizeLayoutsTemplate;
    private VisualizeLayout m_layout;
    private Transform m_layoutTransform; 

    public Transform m_workspaceLocator;
    */

    public string m_scene;
    public Transform m_camera;
    private Camera m_cameraSettings;
    private StreamWriter m_sw; 
    
    /*
    private GameObject getEnv(string scene)
    {
        switch (scene)
        {
            case "coffeeshop":
                return m_envs[0];
            case "livingroom":
                return m_envs[1];
            case "bedroom":
                return m_envs[2];
            case "office":
                return m_envs[3];
        }
        return null;
    }

    private string getSceneSelection(Environment selection)
    {
        switch (selection)
        {
            case Environment.bedroom:
                return "bedroom";
            case Environment.coffeeshop:
                return "coffeeshop";
            case Environment.livingroom:
                return "livingroom";
            case Environment.office:
                return "office";
        }
        return null;
    }

    private void sceneAdjustments(string scene)
    {
        if (scene == "livingroom")
        {
            m_workspaceLocator.position = new Vector3(0, 0.35f, 0);
            m_camera.position = new Vector3(0, 1, 0);
        }
        else
        {
            m_workspaceLocator.position = new Vector3(0, 0.05f, 0);
            m_camera.position = new Vector3(0, 1.3f, 0);
        }
    }
    */

    public void capture()
    {
        // Logging for element presence in each image patch 
        /*
        for (int lat = -70; lat <= 70; lat += 20)
        {
            for (int lon = 10; lon <= 350; lon += 20)
            {
                m_camera.transform.eulerAngles = new Vector3(lat, lon, 0);
                Plane[] planes = GeometryUtility.CalculateFrustumPlanes(m_cameraSettings);

                string log = lat + " " + lon;
                foreach (Transform element in m_layoutTransform)
                {
                    bool elementVisible = false;
                    foreach (Renderer r in element.GetComponentsInChildren<Renderer>())
                    {
                        Bounds bounds = r.bounds;
                        if (GeometryUtility.TestPlanesAABB(planes, bounds))
                        {
                            elementVisible = true;
                            break;
                        }
                    }
                    if (elementVisible)
                    {
                        log += " " + element.name;
                    }
                }
                m_sw.WriteLine(log);
            }
        }
        m_sw.Close();
        */

        /*
        // Hide elements 
        m_layoutTransform.gameObject.SetActive(false);
        */

        // Capture image patches
        for (int lat = -70; lat <= 70; lat += 20)
        {
            for (int lon = 10; lon <= 350; lon += 20)
            {
                m_camera.transform.eulerAngles = new Vector3(lat, lon, 0);

                Camera main = m_cameraSettings;
                RenderTexture captureRT = new RenderTexture(main.pixelWidth, main.pixelHeight, 32);
                main.targetTexture = captureRT;
                RenderTexture.active = captureRT;

                main.Render();
                int width = main.targetTexture.width;
                int height = main.targetTexture.height;
                Texture2D tex = new Texture2D(width, height);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
                byte[] texBytes = tex.EncodeToPNG();
                File.WriteAllBytes("ExperimentResults/" + m_scene + "/" + m_scene + "_" + lat.ToString("D2") + "_" + lon.ToString("D3") + ".png", texBytes);

                RenderTexture.active = null;
                main.targetTexture = null;
            }
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        //m_scene = getSceneSelection(m_sceneSelection);
        //m_selectedEnv = getEnv(m_scene);
        //m_selectedEnv.SetActive(true);
        //sceneAdjustments(m_scene);
        
        //GameObject layout = GameObject.Instantiate(m_visualizeLayoutsTemplate);
        //layout.SetActive(true);
        //m_layoutTransform = layout.transform;
        //m_layout = layout.GetComponent<VisualizeLayout>();
        //m_layout.m_layoutFilePath = m_scene;
        //m_layout.m_participant = m_participant;
        //m_layout.loadLayout();

        m_cameraSettings = m_camera.GetComponent<Camera>();

        //m_sw = new StreamWriter("ExperimentResults/P" + m_participant.ToString("D2") + "_" + m_scene + ".txt");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            capture();
        }
    }
}
