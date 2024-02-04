using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class ExperimentSceneManager : MonoBehaviour
{
    // Touch Controller Controls 
    private InputDevice m_leftController;
    private InputDevice m_rightController;
    public SelectionManager m_leftSelectionManager;
    public SelectionManager m_rightSelectionManager;

    // Track Y and B Buttons
    private bool m_prevYButton;
    private bool m_YButton;
    private bool m_prevBButton;
    private bool m_BButton;
    private bool m_prevLeftTrigger;
    private bool m_leftTrigger;
    private bool m_prevRightTrigger;
    private bool m_rightTrigger; 

    // Text Display 
    public TextMeshProUGUI m_text;
    public Canvas m_experimentMenu;
    private bool m_experimentMenuActive;

    // Virtual Objects 
    public Transform m_virtualObjects;

    // Logger
    public Logger m_logger;

    // Image of previous environment
    public GameObject m_prevEnvCaptureDisplay;
    public MeshRenderer m_prevEnvCaptureFront, m_prevEnvCaptureLeft, m_prevEnvCaptureRight;
    public Camera m_envCaptureCameraFront, m_envCaptureCameraLeft, m_envCaptureCameraRight;
    private bool m_viewPastActive; 

    // Touch Controller Input Handling
    private void getYButtonValue()
    {
        this.m_leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out this.m_YButton);
    }
    private void getBButtonValue()
    {
        this.m_rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out this.m_BButton);
    }
    private void getLeftTriggerValue()
    {
        this.m_leftController.TryGetFeatureValue(CommonUsages.triggerButton, out this.m_leftTrigger);
    }
    private void getRightTriggerValue()
    {
        this.m_rightController.TryGetFeatureValue(CommonUsages.triggerButton, out this.m_rightTrigger);
    }

    private bool getYPress()
    {
        return this.m_prevYButton != this.m_YButton && this.m_YButton;
    }
    private bool getBPress()
    {
        return this.m_prevBButton != this.m_BButton && this.m_BButton;
    }
    private bool getLeftTriggerPress()
    {
        return this.m_prevLeftTrigger != this.m_leftTrigger && this.m_leftTrigger;
    }
    private bool getRightTriggerPress()
    {
        return this.m_prevRightTrigger != this.m_rightTrigger && this.m_rightTrigger;
    }

    // Position environment display 
    private void positionEnvironmentDisplay()
    {
        
        Vector3 pos = Camera.main.transform.position;
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;
        forward.Normalize();
        pos += (0.75f * forward);
        pos.y = 0;
        this.m_prevEnvCaptureDisplay.transform.position = pos;
        this.m_prevEnvCaptureDisplay.transform.rotation = Quaternion.LookRotation(forward);
    }

    IEnumerator Instruction(string instruction)
    {
        this.m_text.SetText(instruction);
        yield return new WaitForSeconds(0.3f);
        while (true)
        {
            this.getBButtonValue();
            if (this.getBPress())
            {
                break;
            }
            this.m_prevBButton = this.m_BButton;
            yield return null;
        }
    }

    IEnumerator OpenMenu()
    {
        this.m_text.SetText("Open the experiment menu to continue.");
        this.m_leftSelectionManager.setRaycastingUI(true);
        this.m_rightSelectionManager.setRaycastingUI(true);
        yield return new WaitForSeconds(0.3f);
        while (true)
        {
            this.getYButtonValue();
            if (this.getYPress())
            {
                break;
            }
            this.m_prevYButton = this.m_YButton;
            yield return null;
        }
        this.m_experimentMenu.gameObject.SetActive(true);
    }

    IEnumerator CloseMenu()
    {
        this.m_text.SetText("Press Y to close the experiment menu. Close the experiment menu to continue.");
        yield return new WaitForSeconds(0.3f);
        while (true)
        {
            this.getYButtonValue();
            if (this.getYPress())
            {
                break;
            }
            this.m_prevYButton = this.m_YButton;
            yield return null;
        }
        this.m_experimentMenu.gameObject.SetActive(false);
        this.m_leftSelectionManager.setRaycastingUI(false);
        this.m_rightSelectionManager.setRaycastingUI(false);
    }

    IEnumerator FirstSceneInstructionSequence()
    {
        GlobalExperimentState.enableViewPast(false);
        GlobalExperimentState.enableExperimentMenu(false);
        Coroutine c = StartCoroutine(Instruction("Welcome to the first environment for scenario one."));
        yield return c;
        c = StartCoroutine(Instruction("Your first task is to design a MR layout for the given environment."));
        yield return c;
        c = StartCoroutine(Instruction("When you are done with setting up the MR content, notify the researcher."));
        yield return c;
        c = StartCoroutine(Instruction("The researcher will instruct you to then open the experiment menu. Press Y to do so."));
        yield return c;
        c = StartCoroutine(OpenMenu());
        yield return c;
        c = StartCoroutine(Instruction("You can use the button on this menu to proceed to the next scene."));
        yield return c;
        c = StartCoroutine(CloseMenu());
        yield return c;
        c = StartCoroutine(Instruction("Please notify the researcher."));
        yield return c;
        this.m_text.SetText("");
        GlobalExperimentState.enableExperimentMenu(true);
    }

    IEnumerator ShowPreviousSetup()
    {
        this.m_text.SetText("View and then hide your previous layout to continue.");
        yield return new WaitForSeconds(0.3f);
        while (true)
        {
            this.getBButtonValue();
            if (this.getBPress())
            {
                break;
            }
            this.m_prevBButton = this.m_BButton;
            yield return null;
        }
        this.positionEnvironmentDisplay();
        this.m_prevEnvCaptureDisplay.SetActive(true);
    }

    IEnumerator ClosePreviousSetup()
    {
        this.m_text.SetText("");
        yield return new WaitForSeconds(0.3f);
        while (true)
        {
            this.getBButtonValue();
            if (this.getBPress())
            {
                break;
            }
            this.m_prevBButton = this.m_BButton;
            yield return null;
        }
        this.m_prevEnvCaptureDisplay.SetActive(false);
    }

    IEnumerator SecondSceneInstructionSequence()
    {
        GlobalExperimentState.enableViewPast(false);
        GlobalExperimentState.enableExperimentMenu(false);
        Coroutine c = StartCoroutine(Instruction("Welcome to the second environment for scenario one."));
        yield return c;
        c = StartCoroutine(Instruction("Your second task is to adapt your first MR layout for this new environment."));
        yield return c;
        c = StartCoroutine(Instruction("Going forward, to view your layout for the first environment, press B. You can hide the view by pressing B again."));
        yield return c;
        c = StartCoroutine(ShowPreviousSetup());
        yield return c;
        c = StartCoroutine(ClosePreviousSetup());
        yield return c;
        c = StartCoroutine(Instruction("Please notify the researcher."));
        yield return c;
        this.m_text.SetText("");
        yield return new WaitForSeconds(0.3f);
        GlobalExperimentState.enableViewPast(true);
        GlobalExperimentState.enableExperimentMenu(true);
    }

    IEnumerator ThirdSceneInstructionSequence()
    {
        GlobalExperimentState.enableViewPast(false);
        GlobalExperimentState.enableExperimentMenu(false);
        Coroutine c = StartCoroutine(Instruction("Welcome to the first environment for scenario two."));
        yield return c;
        c = StartCoroutine(Instruction("Here, we are repeating task one (design an MR layout) for a new scenario."));
        yield return c;
        c = StartCoroutine(Instruction("Please notify the researcher."));
        yield return c;
        this.m_text.SetText("");
        GlobalExperimentState.enableExperimentMenu(true);
    }

    IEnumerator FourthSceneInstructionSequence()
    {
        GlobalExperimentState.enableViewPast(false);
        GlobalExperimentState.enableExperimentMenu(false);
        Coroutine c = StartCoroutine(Instruction("Welcome to the second environment for scenario two."));
        yield return c;
        c = StartCoroutine(Instruction("Here, we are repeating task two (adapting an MR layout) for a new scenario."));
        yield return c;
        c = StartCoroutine(Instruction("As a reminder, you can press B to view your previous layout."));
        yield return c;
        c = StartCoroutine(Instruction("Please notify the researcher."));
        yield return c;
        this.m_text.SetText("");
        yield return new WaitForSeconds(0.3f);
        GlobalExperimentState.enableViewPast(true);
        GlobalExperimentState.enableExperimentMenu(true);
    }

    IEnumerator NextScene()
    {
        this.m_leftSelectionManager.setRaycastingUI(false);
        this.m_rightSelectionManager.setRaycastingUI(false);
        this.m_logger.logEndScene();
        this.m_experimentMenu.gameObject.SetActive(false);
        
        // Capture setups 
        this.m_leftSelectionManager.gameObject.SetActive(false);
        this.m_rightSelectionManager.gameObject.SetActive(false);
        Camera main = Camera.main;
        main.enabled = false;
        // Front
        this.m_envCaptureCameraFront.enabled = true; 
        GlobalExperimentState.cameraCaptureFront(this.m_envCaptureCameraFront);
        this.m_envCaptureCameraFront.enabled = false;
        // Left
        this.m_envCaptureCameraLeft.enabled = true;
        GlobalExperimentState.cameraCaptureLeft(this.m_envCaptureCameraLeft);
        this.m_envCaptureCameraLeft.enabled = false;
        // Right
        this.m_envCaptureCameraRight.enabled = true;
        GlobalExperimentState.cameraCaptureRight(this.m_envCaptureCameraRight);
        this.m_envCaptureCameraRight.enabled = false;
        main.enabled = true;

        GlobalExperimentState.nextScene();
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(GlobalExperimentState.getCurrScene());
    }

    // Load Previous Scene Captures
    private void loadPrevSceneCaptures()
    {
        int prevSceneIdx = GlobalExperimentState.getCurrSceneIdx() - 1;

        // Front 
        Texture2D texFront = GlobalExperimentState.getCaptureTextureFront(prevSceneIdx);
        this.m_prevEnvCaptureFront.material.mainTexture = texFront; 
                 
        // Left
        Texture2D texLeft = GlobalExperimentState.getCaptureTextureLeft(prevSceneIdx);
        this.m_prevEnvCaptureLeft.material.mainTexture = texLeft; 

        // Right
        Texture2D texRight = GlobalExperimentState.getCaptureTextureRight(prevSceneIdx);
        this.m_prevEnvCaptureRight.material.mainTexture = texRight; 
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize right controller
        List<InputDevice> inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right, inputDevices);
        this.m_rightController = inputDevices[0];
        inputDevices.Clear();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Left, inputDevices);
        this.m_leftController = inputDevices[0];

        
        switch (GlobalExperimentState.getCurrSceneIdx())
        {
            case 0:
                StartCoroutine(FirstSceneInstructionSequence());
                break;
            case 1:
                // Load previous scene capture
                this.loadPrevSceneCaptures();
                StartCoroutine(SecondSceneInstructionSequence());
                break;
            case 2:
                StartCoroutine(ThirdSceneInstructionSequence());
                break;
            case 3:
                // Load previous scene capture
                this.loadPrevSceneCaptures();
                StartCoroutine(FourthSceneInstructionSequence());
                break;

        }
    }

    // Update is called once per frame
    void Update()
    {
        // Experiment menu
        if (GlobalExperimentState.isExperimentMenuEnabled())
        {
            this.getYButtonValue();
            if (this.getYPress())
            {
                this.m_experimentMenuActive = !this.m_experimentMenu.gameObject.activeInHierarchy;
                this.m_experimentMenu.gameObject.SetActive(this.m_experimentMenuActive);
                this.m_leftSelectionManager.setRaycastingUI(this.m_experimentMenuActive);
                this.m_rightSelectionManager.setRaycastingUI(this.m_experimentMenuActive);
                if (GlobalExperimentState.isViewPastEnabled() && this.m_experimentMenuActive && this.m_viewPastActive)
                {
                    this.m_viewPastActive = false;
                    this.m_prevEnvCaptureDisplay.SetActive(this.m_viewPastActive);
                }
            }
            this.m_prevYButton = this.m_YButton;
            if (this.m_experimentMenuActive)
            {
                this.getLeftTriggerValue();
                this.getRightTriggerValue();
                if ((this.getLeftTriggerPress() && this.m_leftSelectionManager.m_selectedUI != null) ||
                    (this.getRightTriggerPress() && this.m_rightSelectionManager.m_selectedUI != null))
                {
                    StartCoroutine(NextScene());
                }
                this.m_prevLeftTrigger = this.m_leftTrigger;
                this.m_prevRightTrigger = this.m_rightTrigger;
            }
        }
        // View Past 
        if (GlobalExperimentState.isViewPastEnabled())
        {
            this.getBButtonValue();
            if (this.getBPress())
            {
                this.m_viewPastActive = !this.m_prevEnvCaptureDisplay.activeInHierarchy;
                this.m_prevEnvCaptureDisplay.SetActive(this.m_viewPastActive);
                if (this.m_viewPastActive)
                {
                    this.positionEnvironmentDisplay();
                    if (GlobalExperimentState.isExperimentMenuEnabled() && this.m_experimentMenuActive)
                    {
                        this.m_experimentMenuActive = false;
                        this.m_experimentMenu.gameObject.SetActive(this.m_experimentMenuActive);
                        this.m_leftSelectionManager.setRaycastingUI(this.m_experimentMenuActive);
                        this.m_rightSelectionManager.setRaycastingUI(this.m_experimentMenuActive);
                    }
                }
            }
            this.m_prevBButton = this.m_BButton;
        }
    }
}
