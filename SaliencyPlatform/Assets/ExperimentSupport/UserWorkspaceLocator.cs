using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserWorkspaceLocator : MonoBehaviour
{
    private float m_scale;
    public float m_pulsateSpeed;
    public float m_pulsateScaleChange;
    public float m_rotateSpeed;

    // Start is called before the first frame update
    void Start()
    {
        this.m_scale = (this.transform.localScale.x +
            this.transform.localScale.y +
            this.transform.localScale.z) / 3.0f;
    }

    // Update is called once per frame
    void Update()
    {
        float s = this.m_scale;
        s += this.m_pulsateScaleChange * Mathf.Sin(this.m_pulsateSpeed * Time.realtimeSinceStartup);
        this.transform.localScale = new Vector3(s, s, s);
        this.transform.localRotation = Quaternion.Euler(0.0f, this.m_rotateSpeed * Time.deltaTime, 0.0f) * this.transform.localRotation;
    }
}
