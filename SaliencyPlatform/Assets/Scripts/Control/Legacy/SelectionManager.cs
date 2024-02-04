using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class SelectionManager : MonoBehaviour
{
    #region States

    // Laser (visualized with a line renderer)
    public LineRenderer m_lr;
    private Vector3 m_lrDir;
    private Vector3[] m_lrPos;
    public GameObject m_lrStart;
    public GameObject m_lrEnd;
    public float m_lrFar;
    public float m_lrNear;

    // Raycasting
    private int m_raycastInteractablesLayerMask;
    private int m_raycastUILayerMask;
    private RaycastHit m_raycastHit;
    public float m_raycastFar;
    public GameObject m_raycastSelector;
    public GameObject m_selectedUI;
    private bool m_raycastUI = false;

    // Selection 
    // Outline effect on interactable objects 
    // Updated field in shader
    private string m_colorField = "_OutlineColor";
    // Color for deselected objects
    public Color m_deselectedColor;
    // Color for hover objects 
    public Color m_hoverColor;
    // Hover object 
    private GameObject m_hoverObj;
    // Reference to selection manager 
    public SelectedObjectManager m_selectedObjectManager;
    
    // Select device for range adjustment
    public bool m_enableRangeAdjustment = false;
    public float m_rangeAdjustmentSpeed;

    // Select device for handling scaling 
    public bool m_handleScaleAdjustment = false;

    // Device behaviour 
    public InputDeviceCharacteristics m_deviceChar;
    private InputDevice m_device;
    private bool m_prevTriggerButton;
    private bool m_triggerButton;
    private Vector2 m_joystick;
    public float m_joystickThreshold;
    private bool m_prevGripButton;
    private bool m_gripButton;
    private bool m_prevPrimaryButton;
    private bool m_primaryButton;

    // Other controller reference 
    public SelectionManager m_other;

    #endregion

    #region Line Renderer Functions 

    // Function for calculating line renderer parameters 
    private void calcLR()
    {
        this.m_lrDir = (this.m_lrEnd.transform.position - this.m_lrStart.transform.position).normalized;
        this.m_lrPos[0] = this.m_lrStart.transform.position + this.m_lrNear * this.m_lrDir;
        this.m_lrPos[1] = this.m_lrStart.transform.position + this.m_lrFar * this.m_lrDir;
    }

    #endregion

    #region Selection Functions

    // Function for raycasting against Interactables
    private void raycastInteractables()
    {
        GameObject raycastObj; 
        if (Physics.Raycast(this.m_lrPos[0], this.m_lrDir, out this.m_raycastHit, this.m_raycastFar, this.m_raycastInteractablesLayerMask))
        {
            this.m_raycastSelector.SetActive(true);
            this.m_raycastSelector.transform.position = this.m_raycastHit.point;
            raycastObj = this.m_raycastHit.transform.gameObject;
        }
        else
        {
            this.m_raycastSelector.SetActive(false);
            raycastObj = null;
        }
        
        this.updateHoverObj(raycastObj);
    }

    // Function for raycasting against UI
    private void raycastUI()
    {
        if (Physics.Raycast(this.m_lrPos[0], this.m_lrDir, out this.m_raycastHit, this.m_raycastFar, this.m_raycastUILayerMask))
        {
            this.m_raycastSelector.SetActive(true);
            this.m_raycastSelector.transform.position = this.m_raycastHit.point;
            this.m_selectedUI = this.m_raycastHit.transform.gameObject;
            // Update Button Color 
            ColorBlock buttonColor = this.m_selectedUI.GetComponent<Button>().colors;
            buttonColor.normalColor = new Color(1.0f, 1.0f, 1.0f);
            this.m_selectedUI.GetComponent<Button>().colors = buttonColor;
        }
        else
        {
            this.m_raycastSelector.SetActive(false);
            if (this.m_selectedUI != null)
            {
                ColorBlock buttonColor = this.m_selectedUI.GetComponent<Button>().colors;
                buttonColor.normalColor = new Color(0.9f, 0.9f, 0.9f);
                this.m_selectedUI.GetComponent<Button>().colors = buttonColor;
            }
            this.m_selectedUI = null;
        }
    }

    public void setRaycastingUI(bool raycastUI)
    {
        this.m_raycastUI = raycastUI;
    }

    // Update object outline color
    private void updateOutlineColor(GameObject obj, Color c)
    {
        if (obj == this.m_selectedObjectManager.getSelectedObject())
            return;

        Renderer r = obj.GetComponent<Renderer>();
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);
        mpb.SetColor(this.m_colorField, c);
        r.SetPropertyBlock(mpb);
    }

    // Update object the TouchController is hovering over
    private void updateHoverObj(GameObject newHoverObj)
    {
        if (newHoverObj != null)
        {
            if (this.m_hoverObj != null)
            {
                if (this.m_hoverObj != newHoverObj)
                {
                    this.updateOutlineColor(this.m_hoverObj, this.m_deselectedColor);
                    this.m_hoverObj = null;
                }
            }
            this.m_hoverObj = newHoverObj;
            this.updateOutlineColor(this.m_hoverObj, this.m_hoverColor);
        } else
        {
            if (this.m_hoverObj != null)
            {
                this.updateOutlineColor(this.m_hoverObj, this.m_deselectedColor);
                this.m_hoverObj = null;
            }
        }
    }

    #endregion

    #region Controller Input Functions 

    private bool getTriggerButtonValue()
    {
        return this.m_device.TryGetFeatureValue(CommonUsages.triggerButton, out this.m_triggerButton);
    }
    private bool getJoystickValue()
    {
        return this.m_device.TryGetFeatureValue(CommonUsages.primary2DAxis, out this.m_joystick);
    }
    private bool getGripButtonValue()
    {
        return this.m_device.TryGetFeatureValue(CommonUsages.gripButton, out this.m_gripButton);
    }
    private bool getPrimaryButtonValue()
    {
        return this.m_device.TryGetFeatureValue(CommonUsages.primaryButton, out this.m_primaryButton);
    }
 
    private bool getInputDeviceStates()
    {
        bool valid = getTriggerButtonValue() && getGripButtonValue() && getPrimaryButtonValue();
        if (this.m_enableRangeAdjustment) valid &= getJoystickValue();
        return valid; 
    }

    private void updatePreviousInputDeviceStates()
    {
        this.m_prevTriggerButton = this.m_triggerButton;
        this.m_prevGripButton = this.m_gripButton;
        this.m_prevPrimaryButton = this.m_primaryButton;
    }

    private bool getTriggerPress()
    {
        return this.m_prevTriggerButton != this.m_triggerButton && this.m_triggerButton;
    }
    private bool getTriggerRelease()
    {
        return this.m_prevTriggerButton != this.m_triggerButton && !this.m_triggerButton;
    }
    private bool getTriggerDown()
    {
        return this.m_triggerButton;
    }

    public bool getGripPress()
    {
        return this.m_prevGripButton != this.m_gripButton && this.m_gripButton;
    }
    public bool getGripRelease()
    {
        return this.m_prevGripButton != this.m_gripButton && !this.m_gripButton;
    }
    private bool getGripDown()
    {
        return this.m_gripButton;
    }

    public bool getPrimaryPress()
    {
        return this.m_prevPrimaryButton != this.m_primaryButton && this.m_primaryButton;
    }
    public bool getPrimaryRelease()
    {
        return this.m_prevPrimaryButton != this.m_primaryButton && !this.m_primaryButton;
    }
    private bool getPrimaryDown()
    {
        return this.m_primaryButton;
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {

        #region Controller Set-up

        // Get controller 
        List<InputDevice> inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(this.m_deviceChar, inputDevices);
        this.m_device = inputDevices[0];
        this.m_prevTriggerButton = false; 

        #endregion

        #region Laser Set-up

        // Initialize line renderer
        this.m_lrPos = new Vector3[2];
        this.m_lr.startWidth = 0.01f;
        this.m_lr.endWidth = 0.005f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.5f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        this.m_lr.colorGradient = gradient;

        #endregion

        #region Selection Set-up 

        // Define layer mask for raycasting 
        // 8: Interactable
        this.m_raycastInteractablesLayerMask = 1 << 8;
        // 9: ARUI
        this.m_raycastUILayerMask = 1 << 9;
        // Set hover object initially to null
        this.m_hoverObj = null;


        #endregion
    }


    // Update is called once per frame
    void Update()
    {
        #region Laser Update 
         
        // Update laser
        this.calcLR();
        this.m_lr.SetPositions(this.m_lrPos);

        #endregion

        #region Raycasting Update

        // Raycast 
        if (this.m_raycastUI) this.raycastUI();
        else this.raycastInteractables();

        #endregion

        #region Selection 
        
        if (this.getInputDeviceStates()) {

            if (this.getTriggerPress() && !this.m_selectedObjectManager.m_isRotating) // Disable while rotating
            {
                this.m_selectedObjectManager.updateSelectedObj(this.m_hoverObj, this.gameObject);
            }

            if (this.m_selectedObjectManager.isObjectSelected())
            {
                if (!this.m_selectedObjectManager.m_isRotating) // Disable while rotating
                {
                    // Translation
                    if (this.gameObject == this.m_selectedObjectManager.getSelectionController())
                    {
                        // Object Selected 
                        if (this.getTriggerPress())
                        {
                            // Start Move
                            this.m_selectedObjectManager.setSelectedDistance(this.m_lrPos[0]);
                            this.m_selectedObjectManager.setObjectMoving(true);
                        }
                        if (this.getTriggerDown())
                        {
                            // Moving
                            this.m_selectedObjectManager.setSelectedObjectPosition(this.m_lrPos[0] + this.m_selectedObjectManager.getSelectedDistance() * this.m_lrDir);
                        }
                        if (this.getTriggerRelease())
                        {
                            // Stop Move
                            this.m_selectedObjectManager.setObjectMoving(false);
                        }
                    }
                    // Enable range adjustment during move 
                    if (this.m_enableRangeAdjustment && this.m_selectedObjectManager.isObjectMoving() && Math.Abs(this.m_joystick.y) > this.m_joystickThreshold)
                    {
                        this.m_selectedObjectManager.updateSelectedDistance(this.m_rangeAdjustmentSpeed * Time.deltaTime * this.m_joystick.y);
                    }
                    // Scale
                    if (this.getPrimaryPress() && this.m_other.getPrimaryDown())
                    {
                        // Start Scale
                        this.m_selectedObjectManager.startScale(this.transform.position, this.m_other.transform.position);
                    }
                    if (this.m_handleScaleAdjustment && this.getPrimaryDown() && this.m_other.getPrimaryDown())
                    {
                        // Scaling 
                        this.m_selectedObjectManager.scaleObject(this.transform.position, this.m_other.transform.position);
                    }
                }

                if (this.getGripPress())
                {
                    this.m_selectedObjectManager.setRotationStartOrientation(this.gameObject);
                }
                if (this.getGripDown())
                {
                    this.m_selectedObjectManager.rotateObject(this.gameObject);
                }
                if (this.getGripRelease())
                {
                    this.m_selectedObjectManager.endRotation();
                }

                
            }
            
            this.updatePreviousInputDeviceStates();
        }

        #endregion
    }
}
