using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EvaluationEnvironment
{
    [SerializeField] public Constants.Environments environment;
    [SerializeField] public EvaluationExperimentManager.AdaptationMethod adaptationMethod;
    [SerializeField] public Vector3 position; // For manual placement 
    [SerializeField] public Vector3 forward; // For manual placement
}

public class EvaluationExperimentManager : MonoBehaviour
{
    public enum AdaptationMethod { Manual, Fixed, Optimized };
    public List<EvaluationEnvironment> m_environmentOrder;
    public List<int> m_saveLayoutIndices;
    private int m_currEnvironment; 
    public OptimizationV2 m_optimizer;
    public EvaluationLogging m_logger;
    public TextMesh m_text;
    public Transform m_user;

    public float m_logInterval;
    private float m_intervalElapsedTime;

    private void updateInstruction()
    {
        switch(m_currEnvironment)
        {
            case 0:
                m_text.text = "Tutorial Scene";
                break;
            case 1:
                m_text.text = "Design";
                break;
            case 4:
                m_text.text = "Break";
                break;
            case 7:
                m_text.text = "End";
                break;
            default:
                switch(m_environmentOrder[m_currEnvironment].adaptationMethod)
                {
                    case AdaptationMethod.Fixed:
                        m_text.text = "Scaled Adaptation";
                        break;
                    case AdaptationMethod.Optimized:
                        m_text.text = "Semantic Adaptation";
                        break;
                }
                break;
        }
    }

    IEnumerator logCurrentEnvironment()
    {
        yield return new WaitForSeconds(1.0f);
        if (m_logger.m_logging) m_logger.logCurrentLayout(m_currEnvironment,
                m_environmentOrder[m_currEnvironment].environment,
                m_environmentOrder[m_currEnvironment].adaptationMethod,
                m_optimizer.getElements());
    }

    private void loadEnvironment()
    {
        if (m_currEnvironment < m_environmentOrder.Count) {
            m_user.position = Vector3.zero;
            m_user.forward = Vector3.forward;
            updateInstruction();
            m_optimizer.changeEnvironments(m_environmentOrder[m_currEnvironment].environment);
            switch (m_environmentOrder[m_currEnvironment].adaptationMethod)
            {
                case AdaptationMethod.Manual:
                    m_optimizer.initManualLayout(m_environmentOrder[m_currEnvironment].position, m_environmentOrder[m_currEnvironment].forward);
                    break;
                case AdaptationMethod.Fixed:
                    m_optimizer.loadFixedLayout();
                    break;
                case AdaptationMethod.Optimized:
                    m_optimizer.optimize();
                    break; 
            }
            StartCoroutine(logCurrentEnvironment());
        } else
        {
            if (m_logger.m_logging) m_logger.logEnd();
        }
    }

    public void nextEnvironment()
    {
        m_logger.log("Next");
        if (m_logger.m_logging) m_logger.logAdaptedLayout(m_optimizer.getElements());
        if (m_saveLayoutIndices.Contains(m_currEnvironment))
        {
            m_optimizer.saveCurrentLayout();
        }
        m_currEnvironment++;
        loadEnvironment();
    }

    public void previousEnvironment()
    {
        m_logger.log("Previous");
        m_currEnvironment--;
        m_currEnvironment = Mathf.Max(0, m_currEnvironment);
        loadEnvironment();
    }

    public void resetEnvironment()
    {
        m_logger.log("RESET");
        loadEnvironment();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        m_currEnvironment = 0;
        loadEnvironment();
        m_intervalElapsedTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_intervalElapsedTime > m_logInterval)
        {
            m_logger.intervalLog(m_currEnvironment,
                m_environmentOrder[m_currEnvironment].environment,
                m_environmentOrder[m_currEnvironment].adaptationMethod,
                m_optimizer.getElements());
            m_intervalElapsedTime = 0;
        }
        m_intervalElapsedTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.N))
        {
            nextEnvironment(); 
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            previousEnvironment();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            resetEnvironment();
        }
    }
    
}
