using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using TMPro;
using System.IO;
using UnityEngine.UI;

public class ChangeText : MonoBehaviour
{
    public SteamVR_Action_Boolean nextText;

    public SteamVR_Input_Sources controller;
    
    public TextMeshProUGUI tmp;

    public InputField inputField;

    public List<string> sentences;
    private string fileName = "./sentences.txt";
    private StreamReader reader;

    public int cnt = 0;

    // Start is called before the first frame update
    void Start()
    {
        nextText.AddOnStateDownListener(TriggerDown, controller);
        sentences = new List<string>();
        reader = new StreamReader(fileName);
        string temp;
        while ((temp = reader.ReadLine()) != null)
        {
            sentences.Add(temp);
        }
        for (int i = 0; i < sentences.Count; i++) 
        {
            temp = sentences[i];
            int randomIndex = Random.Range(i, sentences.Count);
            sentences[i] = sentences[randomIndex];
            sentences[randomIndex] = temp;
        }
        reader.Close();
    }

    public void TriggerDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        inputField.Select();
        inputField.text = "";
        tmp.text = sentences[cnt];
        cnt += 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
