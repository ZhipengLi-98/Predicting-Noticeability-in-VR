using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Tobii.XR;
using System.IO;
using Valve.VR.InteractionSystem;
using Valve.VR;
using System;
using UnityEngine.UI;

public class MotorDelayMeasuring : MonoBehaviour
{
    public GameObject spheres;

    public SteamVR_Action_Boolean notice;
    public SteamVR_Input_Sources controller;

    public List<float> res = new List<float>();
    public float waitTimer = 0f;
    public float changeTime = 0f;
    public int times = 0;
    public GameObject curObject;
    private bool flag = false;

    public void TriggerDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        float t = Time.time - changeTime;
        res.Add(t);
        print("This time: " + t);
        times += 1;
        waitTimer = UnityEngine.Random.Range(5f, 10f);
        curObject.GetComponent<Renderer>().material.color = Color.HSVToRGB(0f, 0f, 1.0f);
        if (times < 6)
        {
            float sum = 0f;
            foreach (float r in res)
            {
                sum += r;
            }
            print("Delay Average is: " + sum / res.Count);
            flag = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        notice.AddOnStateDownListener(TriggerDown, controller);

        waitTimer = UnityEngine.Random.Range(5f, 10f);
    }

    // Update is called once per frame
    void Update()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
        }
        else if (times < 5 && !flag)
        {
            flag = true;
            int temp = UnityEngine.Random.Range(0, 18);
            changeTime = Time.time;
            curObject = spheres.transform.GetChild(temp).gameObject;
            curObject.GetComponent<Renderer>().material.color = Color.HSVToRGB(UnityEngine.Random.Range(0f, 1f), 1.0f, 1.0f);
        }
    }
}
