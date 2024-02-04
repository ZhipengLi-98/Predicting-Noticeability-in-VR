using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EvaluationLogging : MonoBehaviour
{
    private StreamWriter m_sw;
    private StreamWriter m_swInterval; 
    private float m_elapsedTime;
    public bool m_logging;

    public void intervalLog(int order, Constants.Environments environmentID, EvaluationExperimentManager.AdaptationMethod type, List<Transform> elements)
    {
        if (m_logging)
        {
            string currentLayoutLog = "INTERVAL " +
            order + " " +
            m_elapsedTime + " " +
            environmentID + " " +
            type;
            m_swInterval.WriteLine(currentLayoutLog);
            m_swInterval.WriteLine("PLACEMENTS " + elements.Count);
            foreach (Transform element in elements)
            {
                string elementLog = element.name + " " + element.position + " " + element.rotation;
                m_swInterval.WriteLine(elementLog);
            }
        }
    }
    
    public void log(string log)
    {
        m_sw.WriteLine(log + " " + m_elapsedTime);
    }

    public void logAdaptedLayout(List<Transform> elements)
    {
        string adaptedLayoutLog = "ADAPTED " +
            m_elapsedTime;
        m_sw.WriteLine(adaptedLayoutLog);
        m_sw.WriteLine("ADJUSTMENTS " + elements.Count);
        foreach (Transform element in elements)
        {
            string elementLog = element.name + " " + element.position + " " + element.rotation;
            m_sw.WriteLine(elementLog);
        }
    }

    public void logCurrentLayout(int order, Constants.Environments environmentID, EvaluationExperimentManager.AdaptationMethod type, List<Transform> elements)
    {
        string currentLayoutLog = "NEW " + 
            order + " " +
            m_elapsedTime + " " +
            environmentID + " " +
            type;
        m_sw.WriteLine(currentLayoutLog);
        m_sw.WriteLine("PLACEMENTS " + elements.Count);
        foreach (Transform element in elements)
        {
            string elementLog = element.name + " " + element.position + " " + element.rotation;
            m_sw.WriteLine(elementLog);
        }
    }

    public void logEnd()
    {
        m_logging = false;
        m_sw.WriteLine("END");
        if (m_sw.BaseStream != null) m_sw.Close();
        m_swInterval.WriteLine("END");
        if (m_swInterval.BaseStream != null) m_swInterval.Close();
    }

    // Start is called before the first frame update
    void Start()
    {
        m_logging = true; 

        string fileName = "Evaluation_" +
            System.DateTime.Now.Month.ToString("D2") + "-" +
            System.DateTime.Now.Day.ToString("D2") + "-" +
            System.DateTime.Now.Hour.ToString("D2") + "-" +
            System.DateTime.Now.Minute.ToString("D2") + "-" +
            System.DateTime.Now.Second.ToString("D2") + ".txt";
        m_sw = new StreamWriter(Application.persistentDataPath + "/" + fileName);
        m_sw.AutoFlush = true;
        m_sw.WriteLine("START");

        string intervalFileName = "Evaluation_IntervalLog_" +
            System.DateTime.Now.Month.ToString("D2") + "-" +
            System.DateTime.Now.Day.ToString("D2") + "-" +
            System.DateTime.Now.Hour.ToString("D2") + "-" +
            System.DateTime.Now.Minute.ToString("D2") + "-" +
            System.DateTime.Now.Second.ToString("D2") + ".txt";
        m_swInterval = new StreamWriter(Application.persistentDataPath + "/" + intervalFileName);
        m_swInterval.AutoFlush = true;
        m_swInterval.WriteLine("START");
    }

    // Update is called once per frame
    void Update()
    {
        m_elapsedTime += Time.deltaTime;
    }

    private void OnApplicationQuit()
    {
        logEnd();
    }
}
