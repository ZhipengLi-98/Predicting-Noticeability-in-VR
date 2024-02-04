using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DesignAdaptationPair
{
    private Transform designTransform;
    private Transform adaptTransform;
    private LineRenderer connection; 

    public DesignAdaptationPair(Transform designTransform, Transform adaptTransform, Color connectionColor, Transform connectionParent)
    {
        this.designTransform = designTransform;
        this.adaptTransform = adaptTransform;
        GameObject connectionObj = new GameObject(this.designTransform.name + "_designAdaptionPairConnection");
        connectionObj.transform.SetParent(connectionParent);
        this.connection = connectionObj.AddComponent<LineRenderer>();
        this.connection.SetPositions(new Vector3[] { this.designTransform.position, this.adaptTransform.position });
        this.connection.startWidth = 0.01f;
        this.connection.endWidth = 0.01f;
        this.connection.material = new Material(Shader.Find("Unlit/Texture"));
        this.connection.startColor = connectionColor;
        this.connection.endColor = connectionColor;
    }
}

public class VisualizeAdaptation : MonoBehaviour
{
    public GameObject m_visualizeLayoutsTemplate;
    private VisualizeLayout m_design, m_adapt;
    public Color m_designColor, m_adaptColor, m_connectionColor; 
    public int m_participant;
    public enum Environment {bedroom, office, livingroom, coffeeshop};
    public Environment m_designSceneSelection, m_adaptSceneSelection;
    private string m_designScene, m_adaptScene;
    private List<DesignAdaptationPair> m_connections = new List<DesignAdaptationPair>();
    private bool m_loaded = false; 

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

    private void displayDesign()
    {
        m_design.gameObject.SetActive(true);
        m_adapt.gameObject.SetActive(false);
    }

    private void displayAdapt()
    {
        m_design.gameObject.SetActive(false);
        m_adapt.gameObject.SetActive(true);
    }

    private void displayOverlay()
    {
        m_design.gameObject.SetActive(true);
        m_adapt.gameObject.SetActive(true);
    }

    public void loadAdaptation(string[] restrictedElements)
    {
        if (!m_loaded)
        {
            m_designScene = getSceneSelection(m_designSceneSelection);
            m_adaptScene = getSceneSelection(m_adaptSceneSelection);

            m_design = GameObject.Instantiate(m_visualizeLayoutsTemplate).GetComponent<VisualizeLayout>();
            m_design.gameObject.SetActive(true);
            m_design.m_layoutFilePath = m_designScene;
            m_design.m_participant = m_participant;
            m_design.m_color = m_designColor;
            m_design.loadLayout(restrictedElements);

            m_adapt = GameObject.Instantiate(m_visualizeLayoutsTemplate).GetComponent<VisualizeLayout>();
            m_adapt.gameObject.SetActive(true);
            m_adapt.m_layoutFilePath = m_adaptScene;
            m_adapt.m_participant = m_participant;
            m_adapt.m_color = m_adaptColor;
            m_adapt.loadLayout(restrictedElements);

            // Draw connections 
            foreach (Transform designTransform in this.m_design.transform)
            {
                Transform correspondingAdaptTransform = null;
                foreach (Transform adaptTransform in this.m_adapt.transform)
                {
                    if (adaptTransform.name == designTransform.name)
                    {
                        correspondingAdaptTransform = adaptTransform;
                        break;
                    }
                }
                if (correspondingAdaptTransform == null) continue;
                m_connections.Add(new DesignAdaptationPair(designTransform, correspondingAdaptTransform, m_connectionColor, this.transform));
            }

            displayOverlay();
        }
    }

    public void loadAdaptation()
    {
        if (!m_loaded)
        {
            m_designScene = getSceneSelection(m_designSceneSelection);
            m_adaptScene = getSceneSelection(m_adaptSceneSelection);

            m_design = GameObject.Instantiate(m_visualizeLayoutsTemplate).GetComponent<VisualizeLayout>();
            m_design.gameObject.SetActive(true);
            m_design.m_layoutFilePath = m_designScene;
            m_design.m_participant = m_participant;
            m_design.m_color = m_designColor;
            m_design.loadLayout();

            m_adapt = GameObject.Instantiate(m_visualizeLayoutsTemplate).GetComponent<VisualizeLayout>();
            m_adapt.gameObject.SetActive(true);
            m_adapt.m_layoutFilePath = m_adaptScene;
            m_adapt.m_participant = m_participant;
            m_adapt.m_color = m_adaptColor;
            m_adapt.loadLayout();

            // Draw connections 
            foreach (Transform designTransform in this.m_design.transform)
            {
                Transform correspondingAdaptTransform = null;
                foreach (Transform adaptTransform in this.m_adapt.transform)
                {
                    if (adaptTransform.name == designTransform.name)
                    {
                        correspondingAdaptTransform = adaptTransform;
                        break;
                    }
                }
                if (correspondingAdaptTransform == null) continue;
                m_connections.Add(new DesignAdaptationPair(designTransform, correspondingAdaptTransform, m_connectionColor, this.transform));
            }

            displayOverlay();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        /*
        if ((m_participant > 0) && (m_designSceneSelection != m_adaptSceneSelection))
        {
            loadAdaptation();
        }
        */
    }

    // Update is called once per frame
    void Update()
    {
    }
}
