using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpdateMaterial : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Transform t in this.transform.GetComponentsInChildren<Transform>())
        {
            Renderer temp = t.gameObject.GetComponent<Renderer>();
            if (temp != null)
            {
                if (t.name.Contains("TMP"))
                {   
                    t.gameObject.GetComponent<TextMeshPro>().color = this.GetComponent<Renderer>().material.color;
                }
                else
                {
                    temp.material.color = this.GetComponent<Renderer>().material.color;   
                }
            }
            t.gameObject.layer = this.transform.gameObject.layer;
        }
    }
}
