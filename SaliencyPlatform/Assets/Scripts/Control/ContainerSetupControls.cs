using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ContainerSetupControls : MonoBehaviour
{
    // Laser 
    public LineRenderer m_lr;
    private Vector3 m_lrDir;
    private Vector3[] m_lrPos = new Vector3[2];
    public Transform m_lrStart;
    public Transform m_lrEnd;
    public float m_lrNear;
    public float m_lrFar;

    // Raycasting 
    public float m_selectionRange; 
    private int m_layerMaskAll, m_layerMaskUI, m_layerMaskElements, m_layerMaskObjects, m_layerMaskConnection;
    private GameObject m_laserPointer;

    // Controls 
    public InputDeviceCharacteristics m_deviceCharacteristics;
    private InputDevice m_device;
    public bool m_handleContainerSetup; 
    private enum DeviceState { ADDING_CONTAINERS, DEFINING_PLANE, DEFINING_VOLUME, DEFINING_CONNECTION };
    private DeviceState m_state;
    private bool m_prevTrigger;
    private bool m_trigger;
    public float m_joystickThreshold;
    public float m_rangeAdjustmentSpeed;
    private Vector2 m_joystick; 

    // Defining containers 
    public Transform m_containers;
    public GameObject m_containerPrefab;
    public GameObject m_controlPointPrefab;
    private GameObject m_currControlPoint;
    private float m_selectDistance;

    // Semantic connections 
    public GameObject m_semanticConnectionPrefab;

    // Optimization
    public Optimization m_optimizer;
    private bool m_isOptimized = false;

    // Other controller reference 
    public GameObject m_other;
    private ContainerSetupControls m_otherOptimizationControls;
    private ManualAdjustmentControls m_thisManualAdjustmentControls, m_otherManualAdjustmentControls; 

    // Menus 
    public GameObject m_menuHMD;
    public GameObject m_menuOptimization;
    public GameObject m_menuManualAdjustment;

    private void initControllers()
    {
        List<InputDevice> inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(m_deviceCharacteristics, inputDevices);
        m_device = inputDevices[0];
        m_prevTrigger = false;
        m_otherManualAdjustmentControls = m_other.GetComponent<ManualAdjustmentControls>();
        m_thisManualAdjustmentControls = gameObject.GetComponent<ManualAdjustmentControls>();
        m_otherOptimizationControls = m_other.GetComponent<ContainerSetupControls>();
    }

    private bool getTriggerValue()
    {
        return this.m_device.TryGetFeatureValue(CommonUsages.triggerButton, out m_trigger);
    }
    private bool getTriggerPress()
    {
        return (m_prevTrigger != m_trigger) && m_trigger;
    }
    private bool getTriggerRelease()
    {
        return (m_prevTrigger != m_trigger) && !m_trigger;
    }
    private bool getTriggerDown()
    {
        return m_trigger;
    }

    private bool getJoystickValue()
    {
        return this.m_device.TryGetFeatureValue(CommonUsages.primary2DAxis, out m_joystick);
    }

    private bool getInputDeviceStates()
    {
        bool valid = getTriggerValue();
        valid &= getJoystickValue();
        return valid; 
    }
    private void updatePreviousInputDeviceStates()
    {
        m_prevTrigger = m_trigger;
    }

    private void initLR()
    {
        m_lr.startWidth = 0.01f;
        m_lr.endWidth = 0.005f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.5f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        m_lr.colorGradient = gradient;
    }

    private void calcLR()
    {
        m_lrDir = (m_lrEnd.position - m_lrStart.position).normalized;
        m_lrPos[0] = m_lrStart.position + m_lrNear * m_lrDir;
        m_lrPos[1] = m_lrStart.position + m_lrFar * m_lrDir;
    }

    private void updateLR()
    {
        m_lr.SetPositions(m_lrPos);
    }

    private void initRaycast()
    {
        m_layerMaskUI = 1 << 8;
        m_layerMaskElements = 1 << 9;
        m_layerMaskObjects = 1 << 10;
        m_layerMaskConnection = 1 << 11; 
        m_layerMaskAll = m_layerMaskUI | m_layerMaskElements | m_layerMaskObjects | m_layerMaskConnection;
        m_laserPointer = transform.Find("laser_pointer").gameObject;
    }

    private Transform raycast(int layer)
    {
        RaycastHit hit; 
        if (Physics.Raycast(m_lrPos[0], m_lrDir, out hit, m_selectionRange, layer))
        {
            // visual feedback
            m_laserPointer.SetActive(true);
            m_laserPointer.transform.position = hit.point;

            // return
            return hit.transform;
        } else
        {
            // visual feedback
            m_laserPointer.SetActive(false);

            // return 
            return null;
        }

    }

    private void moveControlPoint()
    {
        m_currControlPoint.transform.position = m_lrStart.position + (m_selectDistance * m_lrDir);
    }

    private GameObject addPlane()
    {
        GameObject container = GameObject.Instantiate(m_containerPrefab, m_containers);
        container.name = "plane";
        container.GetComponent<ContainerSettings>().type = Constants.Dimensionality.TwoDimensional;
        Transform bounds = container.transform.Find("bounds");
        bounds.localScale = new Vector3(0.50f, 0.01f, 0.01f);
        return container;
    }

    private void initDefinePlane()
    {
        m_state = DeviceState.DEFINING_PLANE;

        m_currControlPoint = GameObject.Instantiate(m_controlPointPrefab);
        m_currControlPoint.name = "controlPoint";
        PlaneSetupHelper.init(m_currControlPoint.transform);

        m_selectDistance = 0.2f;
        moveControlPoint();
    }
    private void addPlaneControlPoint()
    {
        m_currControlPoint = GameObject.Instantiate(m_controlPointPrefab);
        m_currControlPoint.name = "controlPoint";
        PlaneSetupHelper.addControlPoint(m_currControlPoint.transform);
        moveControlPoint();
    }

    private GameObject addVolume()
    {
        GameObject container = GameObject.Instantiate(m_containerPrefab, m_containers);
        container.name = "volume";
        container.GetComponent<ContainerSettings>().type = Constants.Dimensionality.ThreeDimensional;
        Transform bounds = container.transform.Find("bounds");
        bounds.localScale = new Vector3(0.50f, 0.50f, 0.50f);
        return container;
    }

    private void initDefineVolume()
    {
        m_state = DeviceState.DEFINING_VOLUME;

        m_currControlPoint = GameObject.Instantiate(m_controlPointPrefab);
        m_currControlPoint.name = "controlPoint";
        VolumeSetupHelper.init(m_currControlPoint.transform);

        m_selectDistance = 0.2f;
        moveControlPoint();
    }

    private void addVolumeControlPoint()
    {
        m_currControlPoint = GameObject.Instantiate(m_controlPointPrefab);
        m_currControlPoint.name = "controlPoint";
        VolumeSetupHelper.addControlPoint(m_currControlPoint.transform);
        moveControlPoint();
    }

    private void initDefineConnection()
    {
        m_state = DeviceState.DEFINING_CONNECTION;
        SemanticConnectionSetupHelper.init(); 
    }

    private void toggleMenu()
    {
        m_menuHMD.SetActive(!m_menuHMD.activeInHierarchy);
    }

    private void toggleManualControl()
    {
        m_menuManualAdjustment.SetActive(true);
        m_menuOptimization.SetActive(false);
        m_otherManualAdjustmentControls.enabled = true;
        m_thisManualAdjustmentControls.enabled = true;
        m_otherOptimizationControls.enabled = false; 
        this.enabled = false;
    }

    private void triggerPressOnMenu(string menuItem)
    {
        switch (menuItem)
        {
            case "Optimize":
                m_optimizer.optimize();
                m_isOptimized = true; 
                break;
            case "AddPlane":
                initDefinePlane();
                break;
            case "AddVolume":
                initDefineVolume();
                break;
            case "AddConnection":
                if (m_isOptimized) initDefineConnection();
                break;
            case "ClearConnections":
                m_optimizer.clearSemanticConnections();
                break;
            case "ManualAdjustment":
                toggleManualControl();
                break;
            case "Menu":
                toggleMenu();
                break;
            case "Reset":
                m_optimizer.resetOptimization();
                break;
            case "EnvOffice":
                m_optimizer.changeEnvironments(Constants.Environments.Office);
                break;
            case "EnvBedroom":
                m_optimizer.changeEnvironments(Constants.Environments.Bedroom);
                break;
        }
    } 

    // Start is called before the first frame update
    void Start()
    {
        initControllers();
        initLR();
        initRaycast();
        m_state = DeviceState.ADDING_CONTAINERS;
    }

    // Update is called once per frame
    void Update()
    {
        calcLR();
        updateLR();
        raycast(m_layerMaskAll);

        if (getInputDeviceStates() && m_handleContainerSetup)
        {
            switch (m_state)
            {
                case DeviceState.ADDING_CONTAINERS:
                    if (getTriggerPress())
                    {
                        // UI Selection
                        Transform selection = raycast(m_layerMaskUI);
                        if (selection != null)
                        {
                            triggerPressOnMenu(selection.name);
                        } else
                        {
                            // Breaking Connections 
                            selection = raycast(m_layerMaskConnection);
                            if (selection != null)
                            {
                                m_optimizer.removeSemanticConnection(selection);
                            }
                        }
                    }
                    break;
                case DeviceState.DEFINING_VOLUME:
                    if (getTriggerPress())
                    {
                        if (VolumeSetupHelper.m_numConfirmedPoints < 1)
                        {
                            VolumeSetupHelper.m_numConfirmedPoints++;
                            addVolumeControlPoint();
                            VolumeSetupHelper.addVolume(addVolume());
                            VolumeSetupHelper.updateVolume();
                        } else
                        {
                            VolumeSetupHelper.confirmVolume();
                            m_state = DeviceState.ADDING_CONTAINERS;
                        }
                    }
                    if (Mathf.Abs(m_joystick.y) > m_joystickThreshold)
                    {
                        m_selectDistance += m_rangeAdjustmentSpeed * m_joystick.y * Time.deltaTime;
                    }
                    break;
                case DeviceState.DEFINING_PLANE:
                    if (getTriggerPress())
                    {
                        if (PlaneSetupHelper.m_numConfirmedPoints < 2)
                        {
                            addPlaneControlPoint();
                            if (PlaneSetupHelper.m_numConfirmedPoints < 1)
                            {
                                PlaneSetupHelper.addPlane(addPlane());
                                PlaneSetupHelper.updatePlane();
                            }

                            PlaneSetupHelper.m_numConfirmedPoints++;
                        }
                        else
                        {
                            PlaneSetupHelper.confirmPlane();
                            m_state = DeviceState.ADDING_CONTAINERS;
                        }
                    }
                    if (Mathf.Abs(m_joystick.y) > m_joystickThreshold)
                    {
                        m_selectDistance += m_rangeAdjustmentSpeed * m_joystick.y * Time.deltaTime;
                    }
                    break;
                case DeviceState.DEFINING_CONNECTION:
                    if (getTriggerPress())
                    {
                        int raycastLayer = m_layerMaskObjects | m_layerMaskElements; 
                        if (SemanticConnectionSetupHelper.m_object == null && SemanticConnectionSetupHelper.m_element != null)
                        {
                            raycastLayer = m_layerMaskObjects;
                        } else if (SemanticConnectionSetupHelper.m_object != null && SemanticConnectionSetupHelper.m_element == null)
                        {
                            raycastLayer = m_layerMaskElements;
                        }
                        Transform selection = raycast(raycastLayer);
                        if (selection != null)
                        {
                            int selectionLayer = 1 << selection.gameObject.layer;
                            bool initConnection = false;
                            if (selectionLayer == m_layerMaskElements)
                            {
                                initConnection = SemanticConnectionSetupHelper.addElement(selection);
                            }
                            else if (selectionLayer == m_layerMaskObjects)
                            {
                                initConnection = SemanticConnectionSetupHelper.addObject(selection);
                            }
                            if (initConnection)
                            {
                                m_optimizer.addSemanticConnection(SemanticConnectionSetupHelper.m_element, SemanticConnectionSetupHelper.m_object, 1.0, true);
                                m_state = DeviceState.ADDING_CONTAINERS;
                            }
                        } 
                    }
                    break;
            }

            updatePreviousInputDeviceStates();
        }

        switch (m_state)
        {
            case DeviceState.DEFINING_VOLUME:
                if (m_currControlPoint != null)
                {
                    moveControlPoint();
                }
                if (VolumeSetupHelper.m_volume != null)
                {
                    VolumeSetupHelper.updateVolume();
                }
                break;
            case DeviceState.DEFINING_PLANE:
                if (m_currControlPoint != null)
                {
                    moveControlPoint();
                }
                if (PlaneSetupHelper.m_plane != null)
                {
                    PlaneSetupHelper.updatePlane();
                }
                break;
        }
    }
}
