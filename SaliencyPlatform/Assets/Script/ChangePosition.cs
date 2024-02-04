using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ChangePosition : MonoBehaviour
{
    public SteamVR_Action_Boolean grab;
    public SteamVR_Action_Vector2 touchPos;

    public SteamVR_Input_Sources controller;

    public GameObject hand;

    public List<GameObject> userInterefaces = new List<GameObject>();

    private GameObject lineBase;
    private LineRenderer line;

    private RaycastHit hit;
    private bool isGrab;
    private Vector3 hitInitPos;

    public GameObject camera;

    // Start is called before the first frame update
    void Start()
    {
        grab.AddOnStateDownListener(TriggerDown, controller);
        grab.AddOnStateUpListener(TriggerUp, controller);

        isGrab = false;

        // lineBase = new GameObject();
        // lineBase.transform.SetParent(this.transform, false);

        // line = lineBase.AddComponent<LineRenderer>();
        // line.useWorldSpace = true;
        // line.positionCount = 2;

        // line.SetPosition(0, controller.transform.position);
        // line.SetPosition(1, controller.transform.position + controller.transform.rotation * Vector3.forward);

        // line.startWidth = 0.002f;
        // line.endWidth = 0.002f;
    }

    public void TriggerDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        int layerMask = 1 << 6 | 1 << 7;
        if (Physics.Raycast(hand.transform.position, hand.transform.rotation * Vector3.forward, out hit, Mathf.Infinity, layerMask))
        {
            isGrab = true;   
            hitInitPos = hit.transform.position;
        }
    }

    public void TriggerUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        isGrab = false;
    }

    // Update is called once per frame
    void Update()
    {
        // line.SetPosition(0, controller.transform.position);
        // line.SetPosition(1, controller.transform.position + controller.transform.rotation * Vector3.forward);

        if (isGrab)
        {
            hit.transform.position = hand.transform.position + hand.transform.rotation * (((hit.transform.position - hand.transform.position).magnitude + touchPos[SteamVR_Input_Sources.LeftHand].axis[1] / 10f) * Vector3.forward);
            hit.transform.LookAt(camera.transform);
        }
    }
}
