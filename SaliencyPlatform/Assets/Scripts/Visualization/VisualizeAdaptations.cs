using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VisualizeAdaptations : MonoBehaviour
{
    public GameObject[] m_envs;
    public VisualizeAdaptation.Environment m_designSceneSelection, m_adaptSceneSelection;
    private GameObject m_designEnv, m_adaptEnv;

    public GameObject m_visualizeAdaptationTemplate;
    private VisualizeAdaptation[] m_adaptations;

    public int[] m_participants;
    private int m_numParticipants;

    private string m_designScene, m_adaptScene;

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

    private string getSceneSelection(VisualizeAdaptation.Environment selection)
    {
        switch (selection)
        {
            case VisualizeAdaptation.Environment.bedroom:
                return "bedroom";
            case VisualizeAdaptation.Environment.coffeeshop:
                return "coffeeshop";
            case VisualizeAdaptation.Environment.livingroom:
                return "livingroom";
            case VisualizeAdaptation.Environment.office:
                return "office";
        }
        return null;
    }

    private void sceneAdjustments(string scene)
    {
        if (scene == "livingroom")
        {
            m_workspaceLocator.position = new Vector3(0, 0.35f, 0);
            m_camera.position = new Vector3(0, 1.85f, -1);
        }
        else
        {
            m_workspaceLocator.position = new Vector3(0, 0.05f, 0);
            m_camera.position = new Vector3(0, 1.5f, -1);
        }
    }

    public void capture()
    {
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
        string currScene = ""; 
        if (m_designEnv.activeInHierarchy)
        {
            currScene = "_designEnv";
        } else if (m_adaptEnv.activeInHierarchy)
        {
            currScene = "_adaptEnv";
        }
        File.WriteAllBytes("ExperimentResults/" + m_designScene + m_adaptScene + currScene + ".png", texBytes);

        RenderTexture.active = null;
        main.targetTexture = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_designScene = getSceneSelection(m_designSceneSelection);
        m_designEnv = getEnv(m_designScene);

        m_adaptScene = getSceneSelection(m_adaptSceneSelection);
        m_adaptEnv = getEnv(m_adaptScene);

        m_numParticipants = m_participants.Length;
        m_adaptations = new VisualizeAdaptation[m_numParticipants];
        for (int aIdx = 0; aIdx < m_numParticipants; aIdx++)
        {
            GameObject adaptation = GameObject.Instantiate(m_visualizeAdaptationTemplate);
            adaptation.SetActive(true);
            m_adaptations[aIdx] = adaptation.GetComponent<VisualizeAdaptation>();
            m_adaptations[aIdx].m_participant = m_participants[aIdx];
            m_adaptations[aIdx].m_designSceneSelection = m_designSceneSelection;
            m_adaptations[aIdx].m_adaptSceneSelection = m_adaptSceneSelection;
            if (m_restrictElements.Length <= 0)
            {
                m_adaptations[aIdx].loadAdaptation();
            } else
            {
                m_adaptations[aIdx].loadAdaptation(m_restrictElements);
            }
        }
        m_designEnv.SetActive(true);
        m_adaptEnv.SetActive(false);
        sceneAdjustments(m_designScene);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            m_designEnv.SetActive(true);
            m_adaptEnv.SetActive(false);
            sceneAdjustments(m_designScene);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            m_designEnv.SetActive(false);
            m_adaptEnv.SetActive(true);
            sceneAdjustments(m_adaptScene);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            capture();
        }
    }
}
