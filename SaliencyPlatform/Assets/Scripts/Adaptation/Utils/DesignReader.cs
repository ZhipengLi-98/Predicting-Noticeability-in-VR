using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DesignReader : MonoBehaviour
{
    public bool m_load = false; 
    public Transform m_elementSet;
    public string m_path; 

    private bool m_loaded = false;

    public void loadLayout(string path, Transform elementSet)
    {
        if (m_loaded) return;

        string str = File.ReadAllText(path);
        string[] lines = str.Split(
            new[] { "\r\n", "\r", "\n" },
            StringSplitOptions.RemoveEmptyEntries
        );
        bool res = int.TryParse(lines[0], out int numElements);
        Debug.Assert(res);

        int lnum = 1;
        for (int eIdx = 0; eIdx < numElements; eIdx++)
        {
            string[] elementInfo = lines[lnum++].Split(' ');
            Debug.Assert(elementInfo.Length == 11);

            // Retrieve element 
            string elementName = elementInfo[1];
            Transform element = elementSet.Find(elementName);
            Debug.Assert(element);
            GameObject elementObj = GameObject.Instantiate(element.gameObject, this.transform);
            elementObj.name = elementName;
            elementObj.SetActive(true);

            // Position 
            res = float.TryParse(elementInfo[2], out float posX);
            Debug.Assert(res);
            res = float.TryParse(elementInfo[3], out float posY);
            Debug.Assert(res);
            res = float.TryParse(elementInfo[4], out float posZ);
            Debug.Assert(res);
            // Rotation
            res = float.TryParse(elementInfo[5], out float rotX);
            Debug.Assert(res);
            res = float.TryParse(elementInfo[6], out float rotY);
            Debug.Assert(res);
            res = float.TryParse(elementInfo[7], out float rotZ);
            Debug.Assert(res);
            // Scale
            res = float.TryParse(elementInfo[8], out float scaleX);
            Debug.Assert(res);
            res = float.TryParse(elementInfo[9], out float scaleY);
            Debug.Assert(res);
            res = float.TryParse(elementInfo[10], out float scaleZ);
            Debug.Assert(res);

            if (Constants.planarElements.Contains(elementName))
            {
                elementObj.transform.position = new Vector3(posX, posY, posZ);
                elementObj.transform.eulerAngles = new Vector3(rotX, rotY, rotZ);
                //elementObj.transform.localScale = new Vector3(scaleX, scaleZ, scaleY);
            } else
            {
                elementObj.transform.position = new Vector3(posX, posY, posZ);
                elementObj.transform.eulerAngles = new Vector3(rotX, rotY, rotZ);
                elementObj.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
            }
        }
        m_loaded = true;
    }

    private void Start()
    {
        if (m_load && (m_elementSet != null) && (m_path.Length > 0)) loadLayout(m_path, m_elementSet);
    }
}
