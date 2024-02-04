using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Model3D : MonoBehaviour
{
    public GameObject m_modelPrefab;
    public Vector3 m_positionOffset;
    public Vector3 m_scale;

    private GameObject m_model; 

    // Start is called before the first frame update
    void Start()
    {
        this.m_model = GameObject.Instantiate(this.m_modelPrefab, this.transform);
        this.m_model.name = "model";
        this.m_model.transform.localPosition = this.m_positionOffset;
        this.m_model.transform.localScale = this.m_scale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
