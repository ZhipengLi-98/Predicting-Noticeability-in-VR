using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class EvaluationVisualization : MonoBehaviour
{
    public Transform _allEnvironmentData;
    public Transform _allElementData; 
    public string _path;
    private Dictionary<string, Transform> _elements;
    private string[] _designInfo;
    private GameObject _environment;
    public double _moveThreshold;
    public double _rotateThreshold; 

    private void parseElement(string elementInfo, out string element, out Vector3 position, out Quaternion rotation)
    {
        element = elementInfo.Substring(0, elementInfo.IndexOf(' '));

        int positionInfoStartIdx = elementInfo.IndexOf('(') + 1;
        int positionInfoEndIdx = elementInfo.IndexOf(')') - positionInfoStartIdx;
        string[] positionInfo = elementInfo.Substring(positionInfoStartIdx, positionInfoEndIdx).Split(',');
        position = new Vector3(float.Parse(positionInfo[0]), float.Parse(positionInfo[1]), float.Parse(positionInfo[2]));

        int rotationInfoStartIdx = elementInfo.LastIndexOf('(') + 1;
        int rotationInfoEndIdx = elementInfo.LastIndexOf(')') - rotationInfoStartIdx;
        string[] rotationInfo = elementInfo.Substring(rotationInfoStartIdx, rotationInfoEndIdx).Split(',');
       rotation = new Quaternion(float.Parse(rotationInfo[0]), float.Parse(rotationInfo[1]), float.Parse(rotationInfo[2]), float.Parse(rotationInfo[3]));
    }
    
    private void loadDesign(int design)
    {
        if (_environment != null) _environment.SetActive(false);

        int designIdx = 11 * design;
        _environment = _allEnvironmentData.Find("Environment_" + _designInfo[designIdx]).gameObject;
        _environment.SetActive(true);
        for (int eIdx = designIdx + 1; eIdx <= designIdx + 10; eIdx++)
        {
            string elementInfo = _designInfo[eIdx];
            parseElement(elementInfo, out string element, out Vector3 position, out Quaternion rotation);
            _elements[element].position = position;
            _elements[element].rotation = rotation;
        }
    }

    private void getAdjustments(int design)
    {
        Debug.Log("Scene: " + design);
        countInteractions(2 * design - 1, 2 * design);
    }

    private void countInteractions(int designA, int designB)
    {
        int numMoves = 0;
        int numRotations = 0;
        int numAdjustments = 0;
        for (int eIdx = 1; eIdx <= 10; eIdx++)
        {
            string elementAInfo = _designInfo[(designA * 11) + eIdx];
            string elementBInfo = _designInfo[(designB * 11) + eIdx];
            parseElement(elementAInfo, out string elementA, out Vector3 posA, out Quaternion rotA);
            parseElement(elementBInfo, out string elementB, out Vector3 posB, out Quaternion rotB);
            float distance = Vector3.Distance(posA, posB);
            bool moved = distance > _moveThreshold;
            float angle = Quaternion.Angle(rotA, rotB);
            bool rotated = angle > _rotateThreshold;
            if (moved) numMoves++;
            if (rotated) numRotations++;
            if (moved || rotated) numAdjustments++;
            //Debug.Log(elementA + ": " + distance + ", " + angle);
        }
        Debug.Log(numMoves + " " + numRotations + " " + numAdjustments);
    }
    
    private void readFromPath()
    {
        _designInfo = File.ReadAllLines("EvaluationResults/" + _path);
        _elements = new Dictionary<string, Transform>();

        // Get elements 
        for (int eIdx = 1; eIdx <= 10;  eIdx++)
        {
            string elementInfo = _designInfo[eIdx];
            string element = elementInfo.Substring(0, elementInfo.IndexOf(' '));
            GameObject elementObject = GameObject.Instantiate(_allElementData.Find(element).gameObject);
            elementObject.name = element;
            _elements.Add(element, elementObject.transform);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        readFromPath();
        loadDesign(0);
        getAdjustments(1);
        getAdjustments(2);
        getAdjustments(3);
        getAdjustments(4);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0)) loadDesign(0);
        if (Input.GetKeyDown(KeyCode.Alpha1)) loadDesign(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) loadDesign(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) loadDesign(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) loadDesign(4);
        if (Input.GetKeyDown(KeyCode.Alpha5)) loadDesign(5);
        if (Input.GetKeyDown(KeyCode.Alpha6)) loadDesign(6);
        if (Input.GetKeyDown(KeyCode.Alpha7)) loadDesign(7);
        if (Input.GetKeyDown(KeyCode.Alpha8)) loadDesign(8);

    }
}
