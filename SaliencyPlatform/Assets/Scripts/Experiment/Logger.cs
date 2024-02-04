using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class Logger : MonoBehaviour
{
    // StreamWriter
    private StreamWriter m_sw;

    // File name 
    private string m_currScene;
    private string m_fileName; 

    // HMD 
    public Transform m_hmd;

    // Touch controllers
    public Transform m_lTouch, m_rTouch;

    // Tracked objects 
    public List<Transform> m_trackedObjs; 

    // Time-tracking variables 
    public float m_intervalLength;
    private float m_intervalTime;
    private float m_elapsedTime;

    // Interval logging
    private void logInterval()
    {
        float intervalTime = this.m_elapsedTime;
        Vector3 hmdPos = this.m_hmd.transform.position;
        Vector3 hmdRot = this.m_hmd.rotation.eulerAngles;
        Vector3 lTouchPos = this.m_lTouch.position;
        Vector3 lTouchRot = this.m_lTouch.rotation.eulerAngles;
        Vector3 rTouchPos = this.m_rTouch.position;
        Vector3 rTouchRot = this.m_rTouch.rotation.eulerAngles;
        this.m_sw.WriteLine("Interval " + intervalTime);
        this.m_sw.WriteLine("HMD " +
            hmdPos.x + " " + hmdPos.y + " " + hmdPos.z + " " +
            hmdRot.x + " " + hmdRot.y + " " + hmdPos.z);
        this.m_sw.WriteLine("L_TOUCH " +
            lTouchPos.x + " " + lTouchPos.y + " " + lTouchPos.z + " " +
            lTouchRot.x + " " + lTouchRot.y + " " + lTouchRot.z);
        this.m_sw.WriteLine("R_TOUCH " +
            rTouchPos.x + " " + rTouchPos.y + " " + rTouchPos.z + " " +
            rTouchRot.x + " " + rTouchRot.y + " " + rTouchRot.z);
        this.m_sw.WriteLine(this.m_trackedObjs.Count);
        foreach (Transform trackedObj in this.m_trackedObjs)
        {
            this.m_sw.WriteLine("TRACKED_OBJ " + trackedObj.name + " " +
                trackedObj.position.x + " " + trackedObj.position.y + " " + trackedObj.position.z + " " +
                trackedObj.rotation.eulerAngles.x + " " + trackedObj.rotation.eulerAngles.y + " " + trackedObj.rotation.eulerAngles.z + " " +
                trackedObj.localScale.x + " " + trackedObj.localScale.y + " " + trackedObj.localScale.z);
        }
        
    }

    public void logEndScene()
    {
        this.m_sw.WriteLine("End");
        if (this.m_sw.BaseStream != null) this.m_sw.Close();
        // Log tracked objects final state 
        this.m_sw = new StreamWriter(Application.persistentDataPath + "/" + this.m_fileName + "_finalTrackedObjectState.txt");
        this.m_sw.WriteLine(this.m_trackedObjs.Count);
        foreach (Transform trackedObj in this.m_trackedObjs)
        {
            this.m_sw.WriteLine("TRACKED_OBJ " + trackedObj.name + " " +
                trackedObj.position.x + " " + trackedObj.position.y + " " + trackedObj.position.z + " " +
                trackedObj.rotation.eulerAngles.x + " " + trackedObj.rotation.eulerAngles.y + " " + trackedObj.rotation.eulerAngles.z + " " +
                trackedObj.localScale.x + " " + trackedObj.localScale.y + " " + trackedObj.localScale.z);
        }
        if (this.m_sw.BaseStream != null) this.m_sw.Close();
    }

    private void logStartScene()
    {
        // Define log file name
        this.m_currScene = SceneManager.GetActiveScene().name;
        this.m_fileName = this.m_currScene + "_" +
            System.DateTime.Now.Month.ToString("D2") + "-" +
            System.DateTime.Now.Day.ToString("D2") + "-" +
            System.DateTime.Now.Hour.ToString("D2") + "-" +
            System.DateTime.Now.Minute.ToString("D2") + "-" +
            System.DateTime.Now.Second.ToString("D2");

        // Log tracked objects initial state 
        this.m_sw = new StreamWriter(Application.persistentDataPath + "/" + this.m_fileName + "_initTrackedObjectState.txt");
        this.m_sw.WriteLine(this.m_trackedObjs.Count);
        foreach (Transform trackedObj in this.m_trackedObjs)
        {
            this.m_sw.WriteLine("TRACKED_OBJ " + trackedObj.name + " " +
                trackedObj.position.x + " " + trackedObj.position.y + " " + trackedObj.position.z + " " +
                trackedObj.rotation.eulerAngles.x + " " + trackedObj.rotation.eulerAngles.y + " " + trackedObj.rotation.eulerAngles.z + " " +
                trackedObj.localScale.x + " " + trackedObj.localScale.y + " " + trackedObj.localScale.z);
        }
        if (this.m_sw.BaseStream != null) this.m_sw.Close();

        // Log throughout experiment
        this.m_sw = new StreamWriter(Application.persistentDataPath + "/" + this.m_fileName + "_fullExperiment.txt");
        this.m_sw.WriteLine("Start " + this.m_currScene);

        // Initialize timing variables 
        this.m_intervalTime = 0.0f;
        this.m_elapsedTime = 0.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.logStartScene();
    }

    // Update is called once per frame
    void Update()
    {
        // Interval logging 
        if (this.m_intervalTime >= this.m_intervalLength)
        {
            // Reset timer 
            this.m_intervalTime = 0.0f;

            // Log 
            if (this.m_sw.BaseStream != null) { logInterval(); }
        }

        // Increment timers 
        this.m_intervalTime += Time.deltaTime;
        this.m_elapsedTime += Time.deltaTime;
        
    }

    private void OnApplicationQuit()
    {
        this.logEndScene();
    }
}
