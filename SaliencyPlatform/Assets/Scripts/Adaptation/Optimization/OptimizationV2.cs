using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementObjectDirectConnection
{
    public int element;
    public int obj;
    public Transform connection;
    public double weight;
    public bool userDefined = false; 
}

public class ElementObjectSemanticConnection
{
    public string element;
    public string obj;
    public double weight; 
}

public class OptimizationV2 : MonoBehaviour
{
    [Header("User")]
    // Transform of user 
    public Transform _userTransform;
    // Previous position of user 
    private Vector3 _previousUserPosition; 

    [Header("Environment")]
    // Transform containing data for all environments
    public Transform _allEnvironmentData;
    // User environment selection
    public Constants.Environments _userEnvironmentSelection;
    // Current environment transform 
    private Transform _environmentTransform; 
    // Environment identification 
    private string _environmentID;

    [Header("Objects")]
    // Transform containing models of added objects 
    private Transform _addedObjectParentTransform;
    // Transform containing the physical objects in the current environment 
    private Transform _objectsParentTransform;
    // List of object transforms 
    private List<Transform> _objectTransforms = new List<Transform>();
    // List of unique objects (identified using the object name assigned in the hierarchy)
    private List<string> _uniqueObjects = new List<string>();

    [Header("Containers")]
    // Visualize containers 
    public bool _visualizeVoxels; 
    // Multiplier for voxel size 
    public float _voxelSizeMultiplier;
    // Buffer
    public float _elementBuffer; 
    // Voxel size 
    private Vector2 _voxelSize;
    // Transform containing the potential containers in the current environment
    private Transform _containersParentTransform;
    // List of container transforms 
    private List<Transform> _containerTransforms = new List<Transform>();
    private List<ContainerModel[,,]> _placementSlots = new List<ContainerModel[,,]>();
    private List<Transform[,,]> _placementSlotTransforms = new List<Transform[,,]>();
    // Container extents
    private Vector3 _previousContainerExtentsMin, _previousContainerExtentsMax; 

    [Header("Elements")]
    // Transform containing all element GameObjects 
    public Transform _allElementData; 
    // User element set selection 
    public Constants.ElementSet _userElementSetSelection;
    // List of element transforms 
    private List<Transform> _elementTransforms = new List<Transform>();
    // List of element models 
    private List<ElementModel> _elementModels = new List<ElementModel>();
    // List of previous element models 
    private ElementModel[] _previousLayout;

    [Header("Optimization")]
    private bool _optimized; 
    public bool optimized
    {
        get => _optimized;
        set { _optimized = optimized; }
    }
    [Range(1, 7)]
    public int _container2DSupport2D;
    [Range(1, 7)]
    public int _container2DSupport3D;
    [Range(1, 7)]
    public int _container3DSupport2D;
    [Range(1, 7)]
    public int _container3DSupport3D;
    public double _containerTouchThreshold;
    public double _maxUtilityDistanceMean;
    public double _maxUtilityDistanceRange;
    public double _highUtilityHeightCutoff;
    public double _compatibilityWeight;
    public double _compatibilityTypeWeight;
    public double _compatibilityVisibilityWeight;
    public double _compatibilityTouchWeight;
    public double _compatibilityBackgroundWeight;
    public double _utilityWeight;
    public double _utilityCompatibilityWeight;
    public double _utilityMaxWeight;
    public double _anchorWeight;
    public double _anchorThreshold;
    public double _avoidWeight;
    public double _avoidThreshold;
    public double _historyWeight;
    public double _structureWeight;
    public double _userDefinedConnectionsWeight;
    public double _occlusionCostWeight; 

    [Header("Semantic Connections")]
    public float _semanticConnectionThreshold;
    public GameObject _semanticConnectionPrefab; 
    private List<ElementObjectDirectConnection> _directConnections = new List<ElementObjectDirectConnection>();
    private List<ElementObjectSemanticConnection> _semanticConnections = new List<ElementObjectSemanticConnection>();

    [Header("Web Socket (For Gurobi Optimization)")]
    public WebSocketCommunicationV2 _ws;

    // Set optimization parameters 
    private void setOptimizationParams()
    {
        OptimizationParameters.container2DSupport2D = OptimizationUtils.normalize(_container2DSupport2D);
        OptimizationParameters.container2DSupport3D = OptimizationUtils.normalize(_container2DSupport3D);
        OptimizationParameters.container3DSupport2D = OptimizationUtils.normalize(_container3DSupport2D);
        OptimizationParameters.container3DSupport3D = OptimizationUtils.normalize(_container3DSupport3D);
        OptimizationParameters.containerTouchThreshold = _containerTouchThreshold;
        OptimizationParameters.containerMaxUtilityDistanceMean = _maxUtilityDistanceMean;
        OptimizationParameters.containerMaxUtilityDistanceRange = _maxUtilityDistanceRange;
        OptimizationParameters.containerHighUtilityHeightCutoff = _highUtilityHeightCutoff;
        OptimizationParameters.optimizationCompatibilityWeight = _compatibilityWeight;
        OptimizationParameters.optimizationCompatibilityTypeWeight = _compatibilityTypeWeight;
        OptimizationParameters.optimizationCompatibilityVisibilityWeight = _compatibilityVisibilityWeight;
        OptimizationParameters.optimizationCompatibilityTouchWeight = _compatibilityTouchWeight;
        OptimizationParameters.optimizationCompatibilityBackgroundWeight = _compatibilityBackgroundWeight;
        OptimizationParameters.optimizationUtilityWeight = _utilityWeight;
        OptimizationParameters.optimizationCompatibilityUtilityWeight = _utilityCompatibilityWeight;
        OptimizationParameters.optimizationMaxUtilityWeight = _utilityMaxWeight;
        OptimizationParameters.optimizationAnchoringRewardWeight = _anchorWeight;
        OptimizationParameters.optimizationAnchoringThreshold = _anchorThreshold;
        OptimizationParameters.optimizationAvoidanceCostWeight = _avoidWeight;
        OptimizationParameters.optimizationAvoidanceThreshold = _avoidThreshold;
        OptimizationParameters.optimizationStructureWeight = _structureWeight;
        OptimizationParameters.optimizationUserConnectionWeight = _userDefinedConnectionsWeight;
        OptimizationParameters.optimizationOcclusionCostWeight = _occlusionCostWeight;
        OptimizationParameters.optimizationUseHistory = 0.0;
        if (_previousLayout != null) {
            if (_previousLayout.Length > 0) OptimizationParameters.optimizationUseHistory = _historyWeight;
        }
    }

    // Get Environment 
    private void getSelectedEnvironment()
    {
        switch (_userEnvironmentSelection)
        {
            case Constants.Environments.Bedroom:
                _environmentID = "bedroom";
                break;
            case Constants.Environments.Office:
                _environmentID = "office";
                break;
            case Constants.Environments.Coffeeshop:
                _environmentID = "coffeeshop";
                break;
            case Constants.Environments.Livingroom:
                _environmentID = "livingroom";
                break;
            case Constants.Environments.BedroomAddedObjects:
                _environmentID = "bedroom_addedObjects";
                break;
            case Constants.Environments.OfficeAddedObjects:
                _environmentID = "office_addedObjects";
                break;
            case Constants.Environments.CoffeeshopAddedObjects:
                _environmentID = "coffeeshop_addedObjects";
                break;
            case Constants.Environments.LivingroomAddedObjects:
                _environmentID = "livingroom_addedObjects";
                break;
            case Constants.Environments.Bedroom2:
                _environmentID = "bedroom2";
                break;
            case Constants.Environments.MeetingRoomA:
                _environmentID = "meetingRoomA";
                break;
            case Constants.Environments.MeetingRoomB:
                _environmentID = "meetingRoomB";
                break;
            case Constants.Environments.Empty:
                _environmentID = "empty";
                break;
        }
        // Update environment using _environmentID
        updateEnvironment();
    }
    // Update environment using _environmentID
    private void updateEnvironment()
    {
        Debug.Assert(_allEnvironmentData != null, "updateEnvironment() in OptimizationV2: Ensure the allEnvironmentData is assigned");
        string[] environment = _environmentID.Split('_');
        // Environment
        if (_environmentTransform != null) _environmentTransform.gameObject.SetActive(false);
        _environmentTransform = _allEnvironmentData.Find("Environment_" + environment[0]).transform;
        _environmentTransform.gameObject.SetActive(true);

        // Added objects 
        if (_addedObjectParentTransform != null) _addedObjectParentTransform.gameObject.SetActive(false);
        if (environment.Length > 1)
        {
            _addedObjectParentTransform = _allEnvironmentData.Find("AddedObjects_" + environment[0]).transform;
            _addedObjectParentTransform.gameObject.SetActive(true);
        }

        // Objects 
        if (_objectsParentTransform != null) _objectsParentTransform.gameObject.SetActive(false);
        if (environment.Length > 1)
        {
            _objectsParentTransform = _allEnvironmentData.Find("Objects_" + _environmentID).transform;
        } else
        {
            _objectsParentTransform = _allEnvironmentData.Find("Objects_" + environment[0]).transform;
        }
        _objectsParentTransform.gameObject.SetActive(true);
        getEnvironmentObjects();

        // Containers 
        if (_containersParentTransform != null) _containersParentTransform.gameObject.SetActive(false);
        if (environment.Length > 1)
        {
            _containersParentTransform = _allEnvironmentData.Find("Containers_" + _environmentID).transform;   
        }
        else
        {
            _containersParentTransform = _allEnvironmentData.Find("Containers_" + environment[0]).transform;
        }
        _containersParentTransform.gameObject.SetActive(true);
        getEnvironmentContainers();

        _environmentID = environment[0];
    }
    // Get environment objects 
    private void getEnvironmentObjects()
    {
        _objectTransforms.Clear();
        _uniqueObjects.Clear();
        Debug.Assert(_objectsParentTransform != null, "getEnvironmentObjects() in OptimizationV2: Ensure the objectsParentTransform is assigned");
        foreach (Transform objectTransform in _objectsParentTransform)
        {
            _objectTransforms.Add(objectTransform);
            if (!_uniqueObjects.Contains(objectTransform.name)) _uniqueObjects.Add(objectTransform.name);
        }
    }
    // Get environment containers 
    private void getEnvironmentContainers()
    {
        _containerTransforms.Clear();
        Debug.Assert(_containersParentTransform != null, "getEnvironmentContainers() in OptimizationV2: Ensure the containersParentTransform is assigned");
        foreach (Transform containerTransform in _containersParentTransform) _containerTransforms.Add(containerTransform);
        //reorientContainers();
    }

    private void reorientVolume(Transform containerTransform)
    {
        Transform bounds = containerTransform.Find("bounds");
        Vector3 boundsScale = bounds.localScale;

        Vector3 toContainer = containerTransform.position - _userTransform.position;
        toContainer.y = 0.0f;
        toContainer.Normalize();

        Vector3 containerCurrFor = containerTransform.forward;
        containerCurrFor.y = 0.0f;
        containerCurrFor.Normalize();

        if (Vector3.Dot(containerCurrFor, toContainer) > Mathf.Cos(0.25f * Mathf.PI)) return;

        Quaternion rotation;
        if (Mathf.Abs(toContainer.z) >= Mathf.Abs(toContainer.x))
        {
            if (toContainer.z > 0)
            {
                rotation = Quaternion.FromToRotation(containerCurrFor, Vector3.forward);
            }
            else
            {
                rotation = Quaternion.FromToRotation(containerCurrFor, Vector3.back);
            }
        }
        else
        {
            if (toContainer.x > 0)
            {
                rotation = Quaternion.FromToRotation(containerCurrFor, Vector3.right);
            }
            else
            {
                rotation = Quaternion.FromToRotation(containerCurrFor, Vector3.left);
            }
        }
        boundsScale = Quaternion.Inverse(rotation) * boundsScale;

        containerTransform.rotation = rotation;
        bounds.localScale = new Vector3(Mathf.Abs(boundsScale.x), Mathf.Abs(boundsScale.y), Mathf.Abs(boundsScale.z));
    }

    private void reorientPlane(Transform containerTransform)
    {
        Transform bounds = containerTransform.Find("bounds");
        Vector3 boundsScale = bounds.localScale;

        Vector3 toContainer = containerTransform.position - _userTransform.position;
        toContainer.Normalize();

        Vector3 containerCurrFor = containerTransform.forward;
        containerCurrFor.Normalize();

        if (Vector3.Dot(toContainer, -containerCurrFor) > Vector3.Dot(toContainer, containerCurrFor))
        {
            Quaternion rotation = Quaternion.FromToRotation(containerCurrFor, -containerCurrFor);
            boundsScale = Quaternion.Inverse(rotation) * boundsScale;
            containerTransform.rotation = rotation;
            bounds.localScale = new Vector3(Mathf.Abs(boundsScale.x), Mathf.Abs(boundsScale.y), Mathf.Abs(boundsScale.z));
        }
    }

    // Call before get containers 
    // Reorient so container forwards approximately face in the same direction as the user
    private void reorientContainers()
    {
        foreach (Transform containerTransform in _containerTransforms)
        {
            switch(containerTransform.name)
            {
                case "volume":
                    reorientVolume(containerTransform);
                    break;
                case "plane":
                    reorientPlane(containerTransform);
                    break;
            }
        }
    }

    // Get Elements
    private void getSelectedElements()
    {
        Debug.Assert(_allElementData != null, "getSelectedElements() in OptimizationV2: Ensure the allElementData is assigned");
        foreach (Transform elementTransform in _elementTransforms)
        {
            elementTransform.SetParent(null);
            GameObject.Destroy(elementTransform.gameObject);
        }
        _elementTransforms.Clear();
        string[] elementSet = new string[] { };
        switch(_userElementSetSelection)
        {
            case Constants.ElementSet.Leisure:
                elementSet = Constants.leisureElements;
                break;
            case Constants.ElementSet.Productivity:
                elementSet = Constants.productivityElements;
                break;
            //case Constants.ElementSet.UserStudy:
            //    elementSet = Constants.studyElements;
            //    break;
        }
        foreach (string element in elementSet)
        {
            GameObject elementObject = GameObject.Instantiate(_allElementData.Find(element).gameObject);
            elementObject.name = element;
            _elementTransforms.Add(elementObject.transform);
        }
        foreach (Transform elementTransform in _elementTransforms)
        {
            elementTransform.gameObject.SetActive(false);
        }
    }

    // Get elements 
    public List<Transform> getElements() {
        return _elementTransforms;
    }

    // Define Element Models 
    private void defineElementModels()
    {
        _elementModels.Clear();
        int eNum = _elementTransforms.Count; 
        for (int eIdx = 0; eIdx < eNum; eIdx++)
        {
            _elementModels.Add(OptimizationUtils.defineElementModel(eIdx, _elementTransforms[eIdx], _elementTransforms[eIdx].GetComponent<ElementSettings>(), _semanticConnections, _uniqueObjects));
        }
    }

    // Voxelize Containers 
    private void voxelize()
    {
        _placementSlots.Clear();

        // Define voxel unit size 
        float voxelHSize = Mathf.Infinity;
        float voxelVSize = Mathf.Infinity;
        foreach (ElementModel elementModel in _elementModels)
        {
            voxelHSize = Mathf.Min(voxelHSize, (float)elementModel.hSize);
            voxelVSize = Mathf.Min(voxelVSize, (float)elementModel.vSize);
        }

        _voxelSize = new Vector2(_voxelSizeMultiplier * voxelHSize, _voxelSizeMultiplier * voxelVSize);
        // Assign element sizes in voxels 
        foreach (ElementModel elementModel in _elementModels)
        {
            elementModel.hVoxels = Mathf.Max(Mathf.CeilToInt(((float)elementModel.hSize + _elementBuffer) / _voxelSize.x), 1);
            elementModel.vVoxels = Mathf.Max(Mathf.CeilToInt(((float)elementModel.vSize + _elementBuffer) / _voxelSize.y), 1);
        }
        
        // Voxelize containers 
        int cIdx = 0;
        foreach (Transform containerTransform in _containerTransforms)
        {
            Vector3 bounds = containerTransform.Find("bounds").lossyScale;
            Vector3Int containerVoxels = new Vector3Int(
                Mathf.Max(Mathf.FloorToInt(bounds.x / _voxelSize.x), 1),
                Mathf.Max(Mathf.FloorToInt(bounds.y / _voxelSize.y), 1),
                Mathf.Max(Mathf.FloorToInt(bounds.z / _voxelSize.x), 1)
            );
            ContainerModel[,,] containerPlacementSlots = new ContainerModel[containerVoxels.x, containerVoxels.y, containerVoxels.z];
            float xOffset = (int)(containerVoxels.x / 2) - (0.5f * ((containerVoxels.x + 1) % 2));
            float yOffset = (int)(containerVoxels.y / 2) - (0.5f * ((containerVoxels.y + 1) % 2));
            float zOffset = (int)(containerVoxels.z / 2) - (0.5f * ((containerVoxels.z + 1) % 2));
            Vector3 containerVoxelSize = new Vector3(bounds.x / containerVoxels.x, bounds.y / containerVoxels.y, bounds.z / containerVoxels.z);
            for (int xIdx = 0; xIdx < containerVoxels.x; xIdx++)
            {
                for (int yIdx = 0; yIdx < containerVoxels.y; yIdx++)
                {
                    for (int zIdx = 0; zIdx < containerVoxels.z; zIdx++)
                    {
                        Vector3 voxelPosition = new Vector3(
                            containerVoxelSize.x * (xIdx - xOffset),
                            containerVoxelSize.y * (yIdx - yOffset),
                            containerVoxelSize.x * (zIdx - zOffset));
                        voxelPosition = containerTransform.TransformPoint(voxelPosition); // Voxel position in world space
                        containerPlacementSlots[xIdx, yIdx, zIdx] =
                            OptimizationUtils.defineContainerModel(cIdx++,
                            containerTransform.GetComponent<ContainerSettings>(),
                            voxelPosition,
                            _voxelSize,
                            _userTransform,
                            _environmentID,
                            _uniqueObjects,
                            _objectTransforms);
                    }
                }
            }
            _placementSlots.Add(containerPlacementSlots);
        }
        visualizeVoxels();
    }

    private void visualizeVoxels()
    {
        foreach (Transform[,,] placementSlotTransform in _placementSlotTransforms)
        {
            int xSlots = placementSlotTransform.GetLength(0);
            int ySlots = placementSlotTransform.GetLength(1);
            int zSlots = placementSlotTransform.GetLength(2);
            for (int xIdx = 0; xIdx < xSlots; xIdx++)
            {
                for (int yIdx = 0; yIdx < ySlots; yIdx++)
                {
                    for (int zIdx = 0; zIdx < zSlots; zIdx++)
                    {
                        GameObject.Destroy(placementSlotTransform[xIdx, yIdx, zIdx].gameObject);
                    }
                }
            }
        }
        _placementSlotTransforms.Clear();

        int numPlacementSlots = _placementSlots.Count;
        for (int cIdx = 0; cIdx < numPlacementSlots; cIdx++)
        {
            ContainerModel[,,] containerModel = _placementSlots[cIdx];
            Transform containerTransform = _containerTransforms[cIdx];
            int xVoxels = containerModel.GetLength(0);
            int yVoxels = containerModel.GetLength(1);
            int zVoxels = containerModel.GetLength(2);
            Transform[,,] containerPlacementSlotTransforms = new Transform[xVoxels, yVoxels, zVoxels];
            for (int xIdx = 0; xIdx < xVoxels; xIdx++)
            {
                for (int yIdx = 0; yIdx < yVoxels; yIdx++)
                {
                    for (int zIdx = 0; zIdx < zVoxels; zIdx++)
                    {
                        ContainerModel containerModelVoxel = containerModel[xIdx, yIdx, zIdx];
                        GameObject voxel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        Vector3 voxelScale = new Vector3(_voxelSize.x, _voxelSize.y, 0.01f);
                        if (containerModelVoxel.support3D > 0) voxelScale.z = _voxelSize.x;
                        voxel.transform.localScale = voxelScale;
                        voxel.transform.position = new Vector3((float)containerModelVoxel.x, (float)containerModelVoxel.y, (float)containerModelVoxel.z);
                        voxel.transform.forward = containerTransform.forward;
                        Renderer cubeRenderer = voxel.GetComponent<Renderer>();
                        cubeRenderer.enabled = false;
                        voxel.transform.name = "voxel_" + cIdx + "_" + xIdx + "_" + yIdx + "_" + zIdx;
                        voxel.layer = 12;
                        //float debugcolor = 1.0f - (float)containerModelVoxel.utility;
                        //cubeRenderer.material.SetColor("_Color", new Color(1.0f, debugcolor, debugcolor));
                        containerPlacementSlotTransforms[xIdx, yIdx, zIdx] = voxel.transform;
                    }
                }
            }
            _placementSlotTransforms.Add(containerPlacementSlotTransforms);
        }
    }

    public void saveCurrentLayout()
    {
        if (_elementModels.Count <= 0) return;

        // Element positions and forwards
        int numElements = _elementModels.Count;
        for (int eIdx = 0; eIdx < numElements; eIdx++)
        {
            Vector3 elementPosition = _elementTransforms[eIdx].transform.position;
            _elementModels[eIdx].x = elementPosition.x;
            _elementModels[eIdx].y = elementPosition.y;
            _elementModels[eIdx].z = elementPosition.z;
            _elementModels[eIdx].forward = _elementTransforms[eIdx].transform.forward;
        }

        _previousLayout = _elementModels.ToArray();
        
        // Container extents
        _previousContainerExtentsMax = Vector3.negativeInfinity;
        _previousContainerExtentsMin = Vector3.positiveInfinity;
        foreach (ContainerModel[,,] containerModel in _placementSlots)
        {
            int xVoxels = containerModel.GetLength(0);
            int yVoxels = containerModel.GetLength(1);
            int zVoxels = containerModel.GetLength(2);
            for (int xIdx = 0; xIdx < xVoxels; xIdx++)
            {
                for (int yIdx = 0; yIdx < yVoxels; yIdx++)
                {
                    for (int zIdx = 0; zIdx < zVoxels; zIdx++)
                    {
                        ContainerModel containerModelVoxel = containerModel[xIdx, yIdx, zIdx];
                        Vector3 voxelPosition = new Vector3((float)containerModelVoxel.x, (float)containerModelVoxel.y, (float)containerModelVoxel.z);
                        _previousContainerExtentsMax = Vector3.Max(_previousContainerExtentsMax, voxelPosition);
                        _previousContainerExtentsMin = Vector3.Min(_previousContainerExtentsMin, voxelPosition);
                    }
                }
            }
        }
        // User position 
        _previousUserPosition = _userTransform.position;
    }

    private void loadPreviousLayout()
    {
        if (_previousLayout == null) return;
        if (_previousLayout.Length <= 0) return; 

        Vector3 currExtentsMax = Vector3.negativeInfinity;
        Vector3 currExtentsMin = Vector3.positiveInfinity;
        foreach (ContainerModel[,,] containerModel in _placementSlots)
        {
            int xVoxels = containerModel.GetLength(0);
            int yVoxels = containerModel.GetLength(1);
            int zVoxels = containerModel.GetLength(2);
            for (int xIdx = 0; xIdx < xVoxels; xIdx++)
            {
                for (int yIdx = 0; yIdx < yVoxels; yIdx++)
                {
                    for (int zIdx = 0; zIdx < zVoxels; zIdx++)
                    {
                        ContainerModel containerModelVoxel = containerModel[xIdx, yIdx, zIdx];
                        Vector3 voxelPosition = new Vector3((float)containerModelVoxel.x, (float)containerModelVoxel.y, (float)containerModelVoxel.z);
                        currExtentsMax = Vector3.Max(currExtentsMax, voxelPosition);
                        currExtentsMin = Vector3.Min(currExtentsMin, voxelPosition);
                    }
                }
            }
        }
        Vector3 currToMax = currExtentsMax - _userTransform.position;
        Vector3 currToMin = currExtentsMin - _userTransform.position;
        Vector3 prevToMax = _previousContainerExtentsMax - _previousUserPosition;
        Vector3 prevToMin = _previousContainerExtentsMin - _previousUserPosition;
        Vector3 scalePositive = Vector3.one;
        if (Mathf.Abs(currToMax.x) < Mathf.Abs(prevToMax.x)) scalePositive.x = Mathf.Abs(currToMax.x / prevToMax.x);
        if (Mathf.Abs(currToMax.y) < Mathf.Abs(prevToMax.y)) scalePositive.y = Mathf.Abs(currToMax.y / prevToMax.y);
        if (Mathf.Abs(currToMax.z) < Mathf.Abs(prevToMax.z)) scalePositive.z = Mathf.Abs(currToMax.z / prevToMax.z);
        Vector3 scaleNegative = Vector3.one;
        if (Mathf.Abs(currToMin.x) < Mathf.Abs(prevToMin.x)) scaleNegative.x = Mathf.Abs(currToMin.x / prevToMin.x);
        if (Mathf.Abs(currToMin.y) < Mathf.Abs(prevToMin.y)) scaleNegative.y = Mathf.Abs(currToMin.y / prevToMin.y);
        if (Mathf.Abs(currToMin.z) < Mathf.Abs(prevToMin.z)) scaleNegative.z = Mathf.Abs(currToMin.z / prevToMin.z);
        int numElements = _elementTransforms.Count;
        for (int eIdx = 0; eIdx < numElements; eIdx++)
        {
            Vector3 scaledPosition = new Vector3((float)_previousLayout[eIdx].x, (float)_previousLayout[eIdx].y, (float)_previousLayout[eIdx].z);
            scaledPosition -= _previousUserPosition;
            if (scaledPosition.x > 0) scaledPosition.x *= scalePositive.x;
            else scaledPosition.x *= scaleNegative.x;
            if (scaledPosition.y > 0) scaledPosition.y *= scalePositive.y;
            else scaledPosition.y *= scaleNegative.y;
            if (scaledPosition.z > 0) scaledPosition.z *= scalePositive.z;
            else scaledPosition.z *= scaleNegative.z;
            scaledPosition += _userTransform.position;
            _elementModels[eIdx].x = scaledPosition.x;
            _elementModels[eIdx].y = scaledPosition.y;
            _elementModels[eIdx].z = scaledPosition.z;
            _elementModels[eIdx].forward = _previousLayout[eIdx].forward;
        }
    }

    private int getDirectConnection(int element, int obj)
    {
        int numConnections = _directConnections.Count;
        for (int cIdx = 0; cIdx < numConnections; cIdx++)
        {
            if (_directConnections[cIdx].element == element && _directConnections[cIdx].obj == obj) return cIdx;
        }
        return -1; 
    }

    private int getDirectConnection(Transform connection)
    {
        int numConnections = _directConnections.Count;
        for (int cIdx = 0; cIdx < numConnections; cIdx++)
        {
            if (_directConnections[cIdx].connection == connection) return cIdx;
        }
        return -1;
    }

    private int getElement(string element)
    {
        int numElements = _elementTransforms.Count;
        for (int eIdx = 0; eIdx < numElements; eIdx++)
        {
            if (_elementTransforms[eIdx].name == element) return eIdx;
        }
        return -1; 
    }

    private int getSemanticConnection(string element, string obj)
    {
        int numConnections = _semanticConnections.Count;
        for (int cIdx = 0; cIdx < numConnections; cIdx++)
        {
            if (_semanticConnections[cIdx].element == element && _semanticConnections[cIdx].obj == obj) return cIdx;
        }
        return -1;
    }

    private Transform addConnectionTransform(Transform elementTransform, Transform objTransform)
    {
        GameObject connection = GameObject.Instantiate(_semanticConnectionPrefab);
        connection.name = "Connection";
        Vector3 elementPos = elementTransform.position;
        Vector3 objPos = objTransform.position;
        Vector3 objToElement = elementPos - objPos;
        connection.transform.position = 0.5f * (elementPos + objPos);
        connection.transform.rotation = Quaternion.FromToRotation(Vector3.forward, objToElement);
        connection.transform.localScale = new Vector3(0.01f, 0.01f, objToElement.magnitude);
        return connection.transform;
    }

    private void removeConnectionTransform(ElementObjectDirectConnection connection)
    {
        if (connection.connection != null)
        {
            GameObject.Destroy(connection.connection.gameObject);
            connection.connection = null;
        }
    }

    public void clearConnectionTransforms()
    {
        foreach (ElementObjectDirectConnection connection in _directConnections)
        {
            removeConnectionTransform(connection);
        }
    }

    public void addConnection(Transform elementTransform, Transform objTransform, double weight, bool userDefined)
    {
        // Add as direct connection 
        int eIdx = _elementTransforms.IndexOf(elementTransform);
        int oIdx = _objectTransforms.IndexOf(objTransform);
        int cIdx = getDirectConnection(eIdx, oIdx);

        if (cIdx >= 0)
        {
            removeConnectionTransform(_directConnections[cIdx]);
            _directConnections[cIdx].connection = addConnectionTransform(elementTransform, objTransform);
            if (userDefined)
            {
                _directConnections[cIdx].userDefined = userDefined;
                _directConnections[cIdx].weight = weight;
            }
        } else
        {
            ElementObjectDirectConnection connection = new ElementObjectDirectConnection();
            connection.element = eIdx;
            connection.obj = oIdx;
            connection.userDefined = userDefined;
            connection.weight = weight;
            connection.connection = addConnectionTransform(elementTransform, objTransform);
            _directConnections.Add(connection);
        }

        // Add as semantic connection 
        if (userDefined)
        {
            string element = elementTransform.name;
            string obj = objTransform.name;
            int scIdx = getSemanticConnection(element, obj);
            if (scIdx >= 0)
            {
                _semanticConnections[scIdx].weight = weight;
            } else
            {
                ElementObjectSemanticConnection semanticConnection = new ElementObjectSemanticConnection();
                semanticConnection.element = element;
                semanticConnection.obj = obj;
                semanticConnection.weight = weight;
                _semanticConnections.Add(semanticConnection);
            }
        }
    }

    public void removeConnection(Transform connection)
    {
        int cIdx = getDirectConnection(connection);
        removeConnectionTransform(_directConnections[cIdx]);
        _directConnections[cIdx].userDefined = true;
        _directConnections[cIdx].weight = 0.0;

        string element = _elementTransforms[_directConnections[cIdx].element].name;
        string obj = _objectTransforms[_directConnections[cIdx].obj].name;
        int scIdx = getSemanticConnection(element, obj);
        _semanticConnections[scIdx].weight = 0.0;
    }

    private void clearDirectConnections()
    {
        clearConnectionTransforms();
        _directConnections.Clear();
    }

    public void clearConnections()
    {
        clearDirectConnections();
        _semanticConnections.Clear();
    }

    private void visualizeConnectionsByProximity()
    {
        int numElements = _elementModels.Count;
        int numObjects = _uniqueObjects.Count;
        for (int eIdx = 0; eIdx < numElements; eIdx++)
        {
            for (int oIdx = 0; oIdx < numObjects; oIdx++)
            {
                int closestObjIdx = getClosestObj(_elementTransforms[eIdx], _uniqueObjects[oIdx]);
                if (closestObjIdx >= 0)
                {
                    double positionAssociation = OptimizationUtils.computeContainerObjectAssociation(_elementTransforms[eIdx].position, _objectTransforms[closestObjIdx], _voxelSize);
                    double association = _elementModels[eIdx].anchors[oIdx] * positionAssociation;
                    if (association > _semanticConnectionThreshold)
                    {
                        addConnection(_elementTransforms[eIdx], _objectTransforms[getClosestObj(_elementTransforms[eIdx], _uniqueObjects[oIdx])], _elementModels[eIdx].anchors[oIdx], false);
                    }
                } 
            }
        }
    }

    private void visualizeUserDefinedConnections()
    {
        // User defined direct connections
        int numConnections = _directConnections.Count;
        for (int cIdx = 0; cIdx < numConnections; cIdx++)
        {
            if (_directConnections[cIdx].userDefined && _directConnections[cIdx].weight > 0)
            {
                removeConnectionTransform(_directConnections[cIdx]);
                _directConnections[cIdx].connection = addConnectionTransform(_elementTransforms[_directConnections[cIdx].element], _objectTransforms[_directConnections[cIdx].obj]);
            }
        }

        // User defined semantic connections 
        int numSemanticConnections = _semanticConnections.Count;
        for (int scIdx = 0; scIdx < numSemanticConnections; scIdx++)
        {
            int eIdx = getElement(_semanticConnections[scIdx].element); 
            if (eIdx >= 0) {
                int oIdx = getClosestObj(_elementTransforms[eIdx], _semanticConnections[scIdx].obj);
                if (oIdx >= 0)
                {
                    addConnection(_elementTransforms[eIdx], _objectTransforms[oIdx], _semanticConnections[scIdx].weight, false);
                }
            }
        }
    }

    public void visualizeConnections()
    {
        visualizeConnectionsByProximity();
        visualizeUserDefinedConnections();
    }

    private int getClosestObj(Transform element, string obj)
    {
        float closestDist = Mathf.Infinity;
        float dist;
        int closest = -1;
        int numObjects = _objectTransforms.Count;
        for (int oIdx = 0; oIdx < numObjects; oIdx++)
        {
            if (_objectTransforms[oIdx].name == obj)
            {
                dist = Vector3.Distance(element.position, _objectTransforms[oIdx].position);
                if (dist < closestDist)
                {
                    closest = oIdx;
                    closestDist = dist;
                }
            }
        }
        return closest;
    }

    private void initOptimization()
    {
        _optimized = false;
        clearConnectionTransforms();
        getSelectedEnvironment();
        getSelectedElements();

        setOptimizationParams();
        defineElementModels();
        voxelize();
    }

    public void resetOptimization()
    {
        _previousLayout = null;
        clearConnections();
        initOptimization();
    }

    public void initManualLayout(Vector3 position, Vector3 forward)
    {
        resetOptimization();
        List<int> placementIndexes = new List<int>();
        int numElements = _elementTransforms.Count;
        for (int eIdx = 0; eIdx < numElements; eIdx++) placementIndexes.Add(eIdx);
        int randomIdx, placementIdx;
        Vector3 offset;
        Vector3 right = Vector3.Cross(forward, Vector3.up);
        Vector3 initPos; 
        foreach (Transform elementTransform in _elementTransforms)
        {
            randomIdx = UnityEngine.Random.Range(0, placementIndexes.Count);
            placementIdx = placementIndexes[randomIdx];
            offset = (Constants.manualLayoutInitPlacement[placementIdx].x * right) + (Constants.manualLayoutInitPlacement[placementIdx].y * Vector3.up);
            initPos = position + 0.5f * offset;
            switch (elementTransform.name)
            {
                case "Time":
                    initPos += 0.15f * right;
                    break;
                case "ScatterPlot":
                    initPos += 0.125f * right;
                    break;
            }
            elementTransform.position = initPos;
            elementTransform.forward = forward;
            elementTransform.gameObject.SetActive(true);
            placementIndexes.RemoveAt(randomIdx);
        }
        
        _optimized = true;
    }

    public void loadFixedLayout()
    {
        clearConnectionTransforms();

        setOptimizationParams();
        defineElementModels();
        voxelize();

        loadPreviousLayout();

        int numElements = _elementModels.Count;
        for (int eIdx = 0; eIdx < numElements; eIdx++) {
            _elementTransforms[eIdx].position = new Vector3((float)_elementModels[eIdx].x, (float)_elementModels[eIdx].y, (float)_elementModels[eIdx].z);
            _elementTransforms[eIdx].forward = _elementModels[eIdx].forward;
            _elementTransforms[eIdx].gameObject.SetActive(true);
        }
        
        _optimized = true;
    }

    IEnumerator runOptimization()
    {
        yield return new WaitForSeconds(0.1f);

        List<Tuple<int, int, int, int, int, double>> directConnectionBiases = OptimizationUtils.defineConnectionBiases(_directConnections, _placementSlots, _objectTransforms, _voxelSize);
        List<int[]> occlusions = OptimizationUtils.defineOcclusions(_placementSlotTransforms, _userTransform);

        _ws.runOptimization(_elementModels, _placementSlots, directConnectionBiases, occlusions, out List<Tuple<int, int[]>> assignments);
        foreach (Tuple<int, int[]> assignment in assignments)
        {
            int eIdx = assignment.Item1;
            int[] voxelInfo = assignment.Item2;
            ElementModel elementModel = _elementModels[eIdx];
            Vector3 elementVoxelSize = new Vector3(elementModel.hVoxels, elementModel.vVoxels, 1);
            if (elementModel.type3D > 0)
            {
                elementVoxelSize.z = elementModel.hVoxels;
            }
            Vector3 placementPosiiton = Vector3.zero;
            for (int xIdx = 0; xIdx < elementVoxelSize.x; xIdx++)
            {
                for (int yIdx = 0; yIdx < elementVoxelSize.y; yIdx++)
                {
                    for (int zIdx = 0; zIdx < elementVoxelSize.z; zIdx++)
                    {
                        ContainerModel placement = _placementSlots[voxelInfo[0]][voxelInfo[1] + xIdx, voxelInfo[2] + yIdx, voxelInfo[3] + zIdx];
                        placementPosiiton += new Vector3((float)placement.x, (float)placement.y, (float)placement.z);
                    }
                }
            }
            placementPosiiton /= elementVoxelSize.x * elementVoxelSize.y * elementVoxelSize.z;

            // Manually overriding for select elements 
            if (_elementTransforms[eIdx].name == "ParallelCoordinates" || _elementTransforms[eIdx].name == "ScatterPlot" || _elementTransforms[eIdx].name == "Time")
            {
                ContainerModel overridePlacement = _placementSlots[voxelInfo[0]][voxelInfo[1], voxelInfo[2], voxelInfo[3]];
                placementPosiiton = new Vector3((float)overridePlacement.x, (float)overridePlacement.y, (float)overridePlacement.z);
            }

            _elementTransforms[eIdx].position = placementPosiiton;
            _elementTransforms[eIdx].forward = _containerTransforms[voxelInfo[0]].forward;
            _elementTransforms[eIdx].gameObject.SetActive(true);

            /*
            // Proximity based connections connections
            int numObjects = _uniqueObjects.Count;
            double[] placementObjAssociations = new double[numObjects];
            for (int xIdx = 0; xIdx < elementVoxelSize.x; xIdx++)
            {
                for (int yIdx = 0; yIdx < elementVoxelSize.y; yIdx++)
                {
                    for (int zIdx = 0; zIdx < elementVoxelSize.z; zIdx++)
                    {
                        ContainerModel placement = _placementSlots[voxelInfo[0]][voxelInfo[1] + xIdx, voxelInfo[2] + yIdx, voxelInfo[3] + zIdx];
                        for (int oIdx = 0; oIdx < numObjects; oIdx++)
                        {
                            placementObjAssociations[oIdx] += placement.objects[oIdx];
                        }
                    }
                }
            }
            for (int oIdx = 0; oIdx < numObjects; oIdx++)
            {
                placementObjAssociations[oIdx] /= elementVoxelSize.x * elementVoxelSize.y * elementVoxelSize.z;
                double association = _elementModels[eIdx].anchors[oIdx] * placementObjAssociations[oIdx];
                if (association > _semanticConnectionThreshold)
                {
                    addConnection(_elementTransforms[eIdx], _objectTransforms[getClosestObj(_elementTransforms[eIdx], _uniqueObjects[oIdx])], _elementModels[eIdx].anchors[oIdx], false);
                }
            }
            */
        }

        //visualizeConnectionsByProximity();
        //visualizeUserDefinedConnections();
        visualizeConnections();

        _optimized = true;
    }

    // DEBUGGING
    /*
    public void addConnectionTest()
    {
        addConnection(_elementTransforms[0], _objectTransforms[0], 1, true);
    }
    */

    public void optimize()
    {
        Debug.Log("Optimizing");
        clearConnectionTransforms();

        // TODO: Uncomment for actual use (currently commented for user study logic)
        //if (_optimized) saveCurrentLayout();

        setOptimizationParams();
        defineElementModels();
        voxelize();
        
        loadPreviousLayout();

        StartCoroutine(runOptimization());
        
    }

    public void changeEnvironments(Constants.Environments environment)
    {
        // Clear environment specific connections 
        clearDirectConnections();

        // TODO: Uncomment for actual use (currently commented for user study logic)
        //if (_optimized) saveCurrentLayout();
        _userEnvironmentSelection = environment;
        initOptimization();
    }

    // Start is called before the first frame update
    void Start()
    {
        _ws.webSocketConnect();
        //initOptimization();
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.O))
        {
            //optimize();
        }
        */
    }
}
