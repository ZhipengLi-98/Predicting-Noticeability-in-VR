using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VisualizeEnvironmentDesigns : MonoBehaviour
{
    public GameObject[] m_envs;
    public enum Environment { bedroom, office, livingroom, coffeeshop };
    public Environment m_sceneSelection;
    private GameObject m_selectedEnv;

    public GameObject m_visualizeLayoutsTemplate;
    private VisualizeLayout[] m_layouts;

    public int[] m_participants;
    private int m_numParticipants;
    
    private string m_scene; 

    public Transform m_workspaceLocator;
    public Transform m_camera;

    public string[] m_restrictElements; 

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

    public void capture(string output)
    {
        for (int i = 0; i < 4; i++)
        {
            string cameraDir = "";
            Vector3 cameraOffset = Vector3.zero;
            switch (i)
            {
                case 0:
                    cameraDir = "front";
                    cameraOffset.z = -0.5f;
                    break;
                case 1:
                    cameraDir = "right";
                    cameraOffset.x = -0.5f;
                    break;
                case 2:
                    cameraDir = "back";
                    cameraOffset.z = 0.5f;
                    break;
                case 3:
                    cameraDir = "left";
                    cameraOffset.x = 0.5f;
                    break;
            }

            m_camera.transform.eulerAngles = new Vector3(0, 90.0f * i, 0);
            Vector3 cameraPos = m_camera.transform.position;
            m_camera.transform.position = cameraPos + cameraOffset;

            Camera main = Camera.main;
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
            File.WriteAllBytes("ExperimentResults/P" + m_participants[0].ToString("D2") + "_" + output + "_" + cameraDir + ".png", texBytes);

            RenderTexture.active = null;
            main.targetTexture = null;

            m_camera.transform.position = cameraPos;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_scene = getSceneSelection(m_sceneSelection);
        m_selectedEnv = getEnv(m_scene);
        m_selectedEnv.SetActive(true);
        sceneAdjustments(m_scene);

        m_numParticipants = m_participants.Length;
        m_layouts = new VisualizeLayout[m_numParticipants];
        for (int lIdx = 0; lIdx < m_numParticipants; lIdx++)
        {
            GameObject layout = GameObject.Instantiate(m_visualizeLayoutsTemplate);
            layout.SetActive(true);
            m_layouts[lIdx] = layout.GetComponent<VisualizeLayout>();
            m_layouts[lIdx].m_layoutFilePath = m_scene;
            m_layouts[lIdx].m_participant = m_participants[lIdx];
            if (m_restrictElements.Length <= 0) {
                m_layouts[lIdx].loadLayout();
            } else
            {
                m_layouts[lIdx].loadLayout(m_restrictElements);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            capture(m_scene);
        }
    }
}
