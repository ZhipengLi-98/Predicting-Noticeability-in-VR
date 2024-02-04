using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UtilitySpatialDependencies
{
    [SerializeField] public Constants.SpatialDimensions dimension;

    [Range(-1, 1)]
    [SerializeField] public double weight = 0;
}

public class SemanticConnection
{
    public int element;
    public int obj;
    public Transform connection;
    public double weight;
    public bool overriding = false; 
}

public class SemanticConnectionGeneral
{
    public string element;
    public string obj;
    public double weight;
}

public class Optimization : MonoBehaviour
{
    public bool _optimize;
    // Holds all environment data 
    public Transform _allEnvironmentData;


    [Header("Virtual Elements")]
    public Transform _fullElementSetTransform;
    public Transform _elementSetTransform;
    public Constants.ElementSet _elementSet;
    public string _savedDesignPath; // ExperimentResults/Participant<ID>/<environment>.txt
    private int _numElements;
    private List<ElementModel> _elements = new List<ElementModel>();
    private List<Tuple<double[], double>> _pastSamples = new List<Tuple<double[], double>>();
    private List<Transform> _placements = new List<Transform>(); // A parallel array to _elements tracking transforms 
    private const int _layerMaskElements = 1 << 9;

    [Header("Containers")]
    public Transform _containersTransform;
    private int _numContainers;
    private List<ContainerModel> _containers = new List<ContainerModel>();

    [Header("Objects")]
    public Transform _objectsTransform;
    private int _numObjects;
    private List<Transform> _objects = new List<Transform>();
    private int _numUniqueObjects;
    private List<string> _uniqueObjects = new List<string>();

    [Header("Environment")]
    public Constants.Environments _environmentSelection;
    public Transform _environmentTransform; 
    private string _environment;
    private string _optimizedEnvironment; 

    [Header("User")]
    public Transform _user;

    [Header("Optimization Parameters")]
    [Range(1, 7)]
    public int _container2DSupport2D = 7;
    [Range(1, 7)]
    public int _container2DSupport3D = 1;
    [Range(1, 7)]
    public int _container3DSupport2D = 5;
    [Range(1, 7)]
    public int _container3DSupport3D = 7;
    [Range(0, 1)]
    public double _containerVisibilityThreshold = 0.5;
    [Range(0, 1)]
    public double _containerTouchThreshold = 1.0;
    // Rate at which object influences decrease by distance
    public double _objectInfluenceDecayRate = 5;
    // User defined utility spatial dimension dependencies 
    public UtilitySpatialDependencies[] _utilitySpatialDependencies;
    private List<Constants.SpatialDimensions> _utilitySpatiallyDependentDim = new List<Constants.SpatialDimensions>();
    private List<double> _utilitySpatialDependencyWeights = new List<double>();
    [Range(0, 1)]
    public double _utilityDependencyThreshold = 0.5;
    public bool _useHistory = false; 
    [Range(0, 1)]
    public double _historyBias = 0.5;
    [Range(0, 1)]
    public double _historyDependencyThreshold = 0.1;
    // Maximum number of samples to save 
    public int _maxHistoricalSamples = 100;
    // Compatibility weighting in optimization
    public double _optimizationCompatibilityWeight = 1;
    // Threshold for anchoring 
    public double _optimizationAnchoringThreshold = 0.25;
    // Anchoring behavior weighting in optimization
    public double _optimizationAnchoringRewardWeight = 1;
    // Threshold for avoidance
    public double _optimizationAvoidanceThreshold = 0.25;
    // Avoidance behavior weighting in optimization
    public double _optimizationAvoidanceCostWeight = 1;
    public double _optimizationCompatibilityUtilityWeight = 1;
    public double _optimizationCompatibilityAssociatedObjUtilityWeight = 1; 
    public double _optimizationCompatibilityTypeWeight = 0.5;
    public double _optimizationCompatibilityVisibilityWeight = 0.5;
    public double _optimizationCompatibilityTouchWeight = 0.5;
    public double _optimizationCompatibilityBackgroundWeight = 0.5;
    // User defined connections weight 
    public double _optimizationUserConnectionWeight = 1; 

    [Header("Templating Parameters")]
    // Distance between elements 
    public double _elementSpacing = 0.05;

    // Optimization 
    // private Optimizer _optimizer;
    private List<Tuple<int, int>> _optimizationAssignments;
    private List<Tuple<int, double>> _optimizationScales;
    private bool _optimizationInitialized = false;

    [Header("Semantic Connections")]
    public double _visualizeSemanticConnectionsThreshold;
    public GameObject _semanticConnectionPrefab;
    private List<SemanticConnection> _semanticConnections = new List<SemanticConnection>();
    private List<SemanticConnectionGeneral> _semanticConnectionsGeneral = new List<SemanticConnectionGeneral>(); 
    private List<Tuple<int, int>> _userDefinedConnections = new List<Tuple<int, int>>();

    [Header("Options")]
    public bool _connectWebSocket;
    public bool _useWebSocket;
    // Optimization via web socket 
    public WebSocketCommunication _ws;

    // Get environment 
    // Defined in component 
    private void getEnvironment()
    {
        switch (_environmentSelection)
        {
            case Constants.Environments.Bedroom:
                _environment = "bedroom";
                break;
            case Constants.Environments.Office:
                _environment = "office";
                break;
            case Constants.Environments.Coffeeshop:
                _environment = "coffeeshop";
                break;
            case Constants.Environments.Livingroom:
                _environment = "livingroom";
                break;
        }
    }

    // Get environment objects 
    // Stores objects in a list
    private void getObjects()
    {
        _objects.Clear();
        _uniqueObjects.Clear();

        foreach (Transform obj in _objectsTransform)
        {
            _objects.Add(obj);
            if (!_uniqueObjects.Contains(obj.name))
            {
                _uniqueObjects.Add(obj.name);
            }
        }

        _numObjects = _objects.Count;
        _numUniqueObjects = _uniqueObjects.Count;
    }

    // Load elements 
    // Load a new set of elements 
    private void loadElementsNew()
    {
        int numElements = _elementSetTransform.childCount;
        for (int eIdx = numElements-1; eIdx >= 0; eIdx--)
        {
            Transform child = _elementSetTransform.GetChild(eIdx);
            child.SetParent(null);
            GameObject.Destroy(child.gameObject);
        }

        string[] elementSet = new string[] { };
        switch (_elementSet)
        {
            case Constants.ElementSet.Leisure:
                elementSet = Constants.leisureElements;
                break;
            case Constants.ElementSet.Productivity:
                elementSet = Constants.productivityElements;
                break;
        }
        foreach (string element in elementSet)
        {
            GameObject elementObj = GameObject.Instantiate(_fullElementSetTransform.Find(element).gameObject, _elementSetTransform);
            elementObj.name = element;
        }
    }
    // Load a design from file 
    private void loadElementsSaved()
    {
        _elementSetTransform.GetComponent<DesignReader>().loadLayout(_savedDesignPath, _fullElementSetTransform);
    }

    // Get elements
    // This method assumes we have the target element set added to the 
    // _elementSetTransform Transform and that we 
    // have already retrieved the objects present in the environment
    // by calling the getObjects() function because 
    // we use the _uniqueObjects list to build the anchor and avoid vectors 
    private void getElements()
    {
        _elements.Clear();

        int numElements = _elementSetTransform.childCount;
        for (int eIdx = 0; eIdx < numElements; eIdx++)
        {
            Transform elementTransform = _elementSetTransform.GetChild(eIdx);
            ElementSettings userDefinedProperties = elementTransform.GetComponent<ElementSettings>();
            _elements.Add(OptimizationUtils.defineElementModel(eIdx, userDefinedProperties, elementTransform, _user, _uniqueObjects, _objects, _semanticConnectionsGeneral));
        }

        _numElements = _elements.Count;
    }

    // Define utility spatial dependencies 
    // Assumes _utilitySpatialDependencies is defined 
    private void defineUtilitySpatialDependencies()
    {
        _utilitySpatiallyDependentDim.Clear();
        _utilitySpatialDependencyWeights.Clear();

        foreach (UtilitySpatialDependencies dependency in _utilitySpatialDependencies)
        {
            _utilitySpatiallyDependentDim.Add(dependency.dimension);
            _utilitySpatialDependencyWeights.Add(dependency.weight);
        }
    }

    // Compute utility spatial dependencies from elements 
    // A weighted update of the dependency list
    // Assumes _elements list is populated (by calling getElements)
    private void calcUtilitySpatialDependenciesFromElements()
    {
        // Dependencies from element set 
        OptimizationParameters.utilitySpatialDependencyThreshold = _utilityDependencyThreshold;
        OptimizationParameters.historyBias = _historyBias;
        OptimizationParameters.historyDependencyThreshold = _historyDependencyThreshold;
        List<Constants.SpatialDimensions> dims;
        List<double> weights;
        OptimizationUtils.computeUtilitySpatialDependenciesFromElements(_elements, out dims, out weights);

        // Weighted update
        int histDimNum = _utilitySpatialDependencyWeights.Count;
        for (int hIdx = 0; hIdx < histDimNum; hIdx++)
        {
            _utilitySpatialDependencyWeights[hIdx] = OptimizationParameters.historyBias * _utilitySpatialDependencyWeights[hIdx];
        }
        int newDimNum = dims.Count;
        for (int dIdx = 0; dIdx < newDimNum; dIdx++)
        {
            Constants.SpatialDimensions dim = dims[dIdx];
            double weight = (1 - OptimizationParameters.historyBias) * weights[dIdx];
            if (_utilitySpatiallyDependentDim.Contains(dim))
            {
                int idx = _utilitySpatiallyDependentDim.IndexOf(dim);
                _utilitySpatialDependencyWeights[idx] += weight;
            }
            else
            {
                _utilitySpatiallyDependentDim.Add(dim);
                _utilitySpatialDependencyWeights.Add(weight);
            }
        }

        // Remove correlations below a threshold
        /*
        List<Constants.SpatialDimensions> remove = new List<Constants.SpatialDimensions>();
        int hNum = _utilitySpatiallyDependentDim.Count;
        for (int hIdx = 0; hIdx < hNum; hIdx++)
        {
            if (Mathf.Abs((float)_utilitySpatialDependencyWeights[hIdx]) < OptimizationParameters.historyDependencyThreshold)
                remove.Add(_utilitySpatiallyDependentDim[hIdx]);
        }
        int rNum = remove.Count;
        for (int rIdx = 0; rIdx < rNum; rIdx++)
        {
            int idx = _utilitySpatiallyDependentDim.IndexOf(remove[rIdx]);
            _utilitySpatiallyDependentDim.RemoveAt(idx);
            _utilitySpatialDependencyWeights.RemoveAt(idx);
        }
        */
        
        for (int dIdx = 0; dIdx < _utilitySpatiallyDependentDim.Count; dIdx++)
        {
            Debug.Log(_utilitySpatiallyDependentDim[dIdx]);
            Debug.Log(_utilitySpatialDependencyWeights[dIdx]);
        }

    }

    // Calculate container utility values 
    // Use user defined spatial dependencies
    // Assumes the _containers list is populated (by calling getContainers())
    // and _utilitySpatiallyDependentDim and _utilitySpatialDependencyWeights are defined
    private void setContainerUtilityValues()
    {
        int numDim = _utilitySpatiallyDependentDim.Count;
        double[] utilityValues = OptimizationUtils.computeUtility(_containers, _utilitySpatiallyDependentDim, _utilitySpatialDependencyWeights);
        for (int cIdx = 0; cIdx < _numContainers; cIdx++)
        {
            if (!_containers[cIdx].overrideUtility) _containers[cIdx].utility = utilityValues[cIdx];
        }

    }

    // Calculate container utility values 
    // Use past samples (nearest-neighbor search) 
    private void setContainerUtilityValuesFromPast()
    {
        double[] utilityValues =
            OptimizationUtils.computeUtilityFromPast(
            _containers,
            _utilitySpatiallyDependentDim,
            _utilitySpatialDependencyWeights,
            _pastSamples);
        for (int cIdx = 0; cIdx < _numContainers; cIdx++)
        {
            _containers[cIdx].utility = utilityValues[cIdx];
        }
    }

    // Save current elements for sampling in future environments 
    // Assumes _placements is populated
    private void saveElementSamples()
    {
        if (_placements.Count <= 0 || !_useHistory)
            return;

        // Update element model spatial parameters
        for (int eIdx = 0; eIdx < _numElements; eIdx++)
        {
            Vector3 relativePosition = _placements[eIdx].position - _user.position;
            _elements[eIdx].x = relativePosition.x;
            _elements[eIdx].y = relativePosition.y;
            _elements[eIdx].z = relativePosition.z;
            _elements[eIdx].dist = relativePosition.magnitude;
            _elements[eIdx].fromForward = Vector3.Dot(_user.forward, relativePosition.normalized);
        }

        double[][] normalizedSpatialDims = OptimizationUtils.computeNormalizedElementSpatialDimensions(_elements);

        // Ensure we have fewer than _maxHistoricalSamples samples 
        int sampleNum = _pastSamples.Count + _numElements;
        if (sampleNum > _maxHistoricalSamples)
        {
            int removeNum = sampleNum - _maxHistoricalSamples;
            _pastSamples.RemoveRange(0, removeNum);
        }

        // adding samples
        for (int eIdx = 0; eIdx < _numElements; eIdx++)
        {
            int sNum = Constants.allSpatialDimensions.Count;
            double[] spatialDims = new double[sNum];
            for (int sIdx = 0; sIdx < sNum; sIdx++)
            {
                spatialDims[sIdx] = normalizedSpatialDims[sIdx][eIdx];
            }
            _pastSamples.Add(new Tuple<double[], double>(spatialDims, _elements[eIdx].utility));
        }

        // Update spatial dependencies 
        calcUtilitySpatialDependenciesFromElements();
    }

    private void reorientVolume(Transform containerTransform)
    {
        Transform bounds = containerTransform.Find("bounds");
        Vector3 boundsScale = bounds.localScale;

        Vector3 toContainer = containerTransform.position - _user.position;
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

        Vector3 toContainer = containerTransform.position - _user.position;
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
        int numContainers = _containersTransform.childCount;
        for (int cIdx = 0; cIdx < numContainers; cIdx++)
        {
            Transform containerTransform = _containersTransform.GetChild(cIdx);
            switch (containerTransform.name)
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

    // Get containers 
    // This method assumes _containersTransform contains the relevant containers, 
    // that we have already retrieved the objects present in the environment
    // with the getObjects() function because
    // we use the _uniqueObjects list to build the objects vector,
    // and that the _environment variable is set to something available in the 
    // EnvironmentBackgrounds file
    // We set all container values with the exception of utility 
    // which is dependent on whether we have prior data 
    private void getContainers()
    {
        _containers.Clear();

        // Set parameters 
        OptimizationParameters.container2DSupport2D = OptimizationUtils.normalize(_container2DSupport2D);
        OptimizationParameters.container2DSupport3D = OptimizationUtils.normalize(_container2DSupport3D);
        OptimizationParameters.container3DSupport2D = OptimizationUtils.normalize(_container3DSupport2D);
        OptimizationParameters.container3DSupport3D = OptimizationUtils.normalize(_container3DSupport3D);
        OptimizationParameters.containerVisibilityThreshold = _containerVisibilityThreshold;
        OptimizationParameters.containerTouchThreshold = _containerTouchThreshold;
        OptimizationParameters.containerObjectInfluenceDecayRate = _objectInfluenceDecayRate;
        int numContainers = _containersTransform.childCount;
        for (int cIdx = 0; cIdx < numContainers; cIdx++)
        {
            Transform containerTransform = _containersTransform.GetChild(cIdx);
            ContainerSettings userDefinedProperties = containerTransform.GetComponent<ContainerSettings>();
            _containers.Add(OptimizationUtils.defineContainerModel(cIdx, userDefinedProperties, containerTransform, _user, _uniqueObjects, _objects));
            _containers[cIdx].backgroundComplexity = OptimizationUtils.computeContainerBackgroundComplexity(_containersTransform.GetChild(cIdx), _user, _environment);
        }
        _numContainers = _containers.Count;
    }

    // Helper function for getting the closest container to an object
    private int getClosestContainer(Transform obj)
    {
        float closestDist = Mathf.Infinity;
        float dist;
        int closest = -1;
        for (int cIdx = 0; cIdx < _numContainers; cIdx++)
        {
            dist = Vector3.Distance(obj.position, _containersTransform.GetChild(cIdx).position);
            if (dist < closestDist)
            {
                closest = cIdx;
                closestDist = dist;
            }
        }
        return closest;
    }

    // User defined constraints (with connections)
    private void getUserDefinedConnections()
    {
        _userDefinedConnections.Clear();
        foreach(SemanticConnection connection in _semanticConnections)
        {
            if (connection.overriding && (connection.weight > 0))
            {
                int closest = getClosestContainer(_objects[connection.obj]);
                if (closest >= 0) _userDefinedConnections.Add(new Tuple<int, int>(connection.element, closest));
            }
        }
    }

    // Perform optimization 
    // Assumes _elements and _containers 
    // are both populated and all values set 
    private void performOptimization()
    {
        OptimizationParameters.optimizationCompatibilityWeight = _optimizationCompatibilityWeight;
        OptimizationParameters.optimizationAnchoringRewardWeight = _optimizationAnchoringRewardWeight;
        OptimizationParameters.optimizationAnchoringThreshold = _optimizationAnchoringThreshold;
        OptimizationParameters.optimizationAvoidanceCostWeight = _optimizationAvoidanceCostWeight;
        OptimizationParameters.optimizationAvoidanceThreshold = _optimizationAvoidanceThreshold;
        OptimizationParameters.optimizationCompatibilityUtilityWeight = _optimizationCompatibilityUtilityWeight;
        OptimizationParameters.optimizationCompatibilityAssociatedObjUtilityWeight = _optimizationCompatibilityAssociatedObjUtilityWeight;
        OptimizationParameters.optimizationCompatibilityTypeWeight = _optimizationCompatibilityTypeWeight;
        OptimizationParameters.optimizationCompatibilityVisibilityWeight = _optimizationCompatibilityVisibilityWeight;
        OptimizationParameters.optimizationCompatibilityTouchWeight = _optimizationCompatibilityTouchWeight;
        OptimizationParameters.optimizationCompatibilityBackgroundWeight = _optimizationCompatibilityBackgroundWeight;
        OptimizationParameters.optimizationUserConnectionWeight = _optimizationUserConnectionWeight;
        
        _ws.runOptimization(_elements, _containers, _userDefinedConnections, out _optimizationAssignments, out _optimizationScales);
        
        /*
        if (_useWebSocket)
        {
            _ws.runOptimization(_elements, _containers, out _optimizationAssignments, out _optimizationScales);
        }
        else
        {
            _optimizer = new Optimizer(_elements, _containers);
            _optimizer.optimize();
        }
        */

        _optimizedEnvironment = _environment;
    }

    // Get optimization results 
    // Assumes we have previously successfully called performOptimization()
    private void displayOptimizationResults()
    {
        //if (!_useWebSocket) _optimizer.getResults(out _optimizationAssignments, out _optimizationScales);

        _placements.Clear();

        for (int eIdx = 0; eIdx < _numElements; eIdx++)
        {
            int eId = _optimizationAssignments[eIdx].Item1;
            int cId = _optimizationAssignments[eIdx].Item2;
            double optimizedScale = _optimizationScales[eId].Item2;
            _elements[eId].optimizedScale = optimizedScale;
            Transform element = _elementSetTransform.GetChild(eId);
            Transform container = _containersTransform.GetChild(cId);
            GameObject placement = GameObject.Instantiate(element.gameObject, container);
            placement.name = element.name;
            placement.transform.localScale = (float)_elements[eId].optimizedScale * placement.transform.localScale;
            placement.transform.localPosition = Vector3.zero;
            placement.transform.localRotation = Quaternion.identity;
            _placements.Add(placement.transform);
        }
        _elementSetTransform.gameObject.SetActive(false);
    }

    private List<Transform> getContainerElements(Transform container)
    {
        List<Transform> elements = new List<Transform>();
        foreach (Transform element in container)
        {
            if ((1 << element.gameObject.layer) == _layerMaskElements) elements.Add(element);
        }

        return elements;
    }

    private double getContainerElementWidth(List<Transform> elements)
    {
        double width = 0;
        foreach (Transform element in elements)
        {
            int eIdx = _placements.IndexOf(element);
            width += _elements[eIdx].optimizedScale * _elements[eIdx].hSize;
        }
        int numElements = elements.Count;

        return width;
    }

    private double getContainerElementHeight(List<Transform> elements)
    {
        double height = 0;
        foreach (Transform element in elements)
        {
            int eIdx = _placements.IndexOf(element);
            height += _elements[eIdx].optimizedScale * _elements[eIdx].vSize;
        }
        int numElements = elements.Count;

        return height;
    }

    // Row template
    // _placements, _elements should be populated 
    private void rowPlacement(Transform container)
    {
        List<Transform> elements = getContainerElements(container);

        double width = getContainerElementWidth(elements);
        width += (elements.Count - 1) * _elementSpacing;

        double x = -0.5 * width;
        foreach (Transform element in elements)
        {
            int eIdx = _placements.IndexOf(element);
            double eSize = _elements[eIdx].optimizedScale * _elements[eIdx].hSize;
            x += 0.5 * eSize;
            Vector3 pos = element.transform.localPosition;
            pos.x = (float)x;
            element.transform.localPosition = pos;
            x += (0.5 * eSize) + _elementSpacing;
        }
    }

    // Column template
    // _placements, _elements should be populated 
    private void columnPlacement(Transform container)
    {
        List<Transform> elements = getContainerElements(container);

        double height = getContainerElementHeight(elements);
        height += (elements.Count - 1) * _elementSpacing;

        double y = -0.5 * height;
        foreach (Transform element in elements)
        {
            int eIdx = _placements.IndexOf(element);
            double eSize = _elements[eIdx].optimizedScale * _elements[eIdx].vSize;
            y += 0.5 * eSize;
            Vector3 pos = element.transform.localPosition;
            pos.y = (float)y;
            element.transform.localPosition = pos;
            y += (0.5 * eSize) + _elementSpacing;
        }
    }

    // Grid template 
    // _placements, _elements should be populated 
    private void gridPlacement2xN(Transform container)
    {
        List<Transform> elements = getContainerElements(container);
        double maxHeight = 0, maxWidth = 0;
        foreach (Transform element in elements)
        {
            int eIdx = _placements.IndexOf(element);
            maxHeight = Mathf.Max((float)maxHeight, (float)(_elements[eIdx].optimizedScale * _elements[eIdx].vSize));
            maxWidth = Mathf.Max((float)maxWidth, (float)(_elements[eIdx].optimizedScale * _elements[eIdx].hSize));
        }
        int colCount = Mathf.CeilToInt((float)(elements.Count / 2.0));
        double y = 0.5 * ((2 * maxHeight) + _elementSpacing);
        double x;
        int numElements = elements.Count;
        for (int row = 0; row < 2; row++)
        {
            y -= 0.5 * maxHeight;
            x = -0.5 * ((maxWidth * colCount) + _elementSpacing * (colCount - 1));
            for (int col = 0; col < colCount; col++)
            {
                x += 0.5 * maxWidth;
                Vector3 pos = new Vector3((float)x, (float)y, 0);
                int eIdx = (row * colCount) + col;
                if (eIdx >= numElements) break;
                elements[eIdx].localPosition = pos;
                x += (0.5 * maxWidth) + _elementSpacing;
            }
            y -= (0.5 * maxHeight) + _elementSpacing;
        }
    }

    private bool placementAroundAnchor(Transform container)
    {
        int containsIdx = -1;
        for (int oIdx = 0; oIdx < _numObjects; oIdx++)
        {
            if (OptimizationUtils.containsObject(container, _objects[oIdx]))
            {
                containsIdx = oIdx;
                break;
            }
        }
        if (containsIdx == -1)
        {
            return false;
        }

        Transform obj = _objects[containsIdx];
        Vector3 objExtent = 0.5f * obj.localScale;

        List<Transform> elements = getContainerElements(container);
        double maxHeight = 0, maxWidth = 0;
        foreach (Transform element in elements)
        {
            int eIdx = _placements.IndexOf(element);
            maxHeight = Mathf.Max((float)maxHeight, (float)(_elements[eIdx].optimizedScale * _elements[eIdx].vSize));
            maxWidth = Mathf.Max((float)maxWidth, (float)(_elements[eIdx].optimizedScale * _elements[eIdx].hSize));
        }

        // Placements
        List<Vector3> positions = new List<Vector3>();
        double distX = (0.5 * objExtent.x) + maxWidth;
        double distY = (0.5 * objExtent.y) + maxHeight;
        Vector3 potentialPos = obj.position + new Vector3(-(float)distX, 0, 0);
        if (OptimizationUtils.containsPoint(container, potentialPos)) positions.Add(potentialPos);
        potentialPos = obj.position + new Vector3((float)distX, 0, 0);
        if (OptimizationUtils.containsPoint(container, potentialPos)) positions.Add(potentialPos);
        potentialPos = obj.position + new Vector3(0, (float)distY, 0);
        if (OptimizationUtils.containsPoint(container, potentialPos)) positions.Add(potentialPos);
        potentialPos = obj.position + new Vector3(-(float)distX, (float)distY, 0);
        if (OptimizationUtils.containsPoint(container, potentialPos)) positions.Add(potentialPos);
        potentialPos = obj.position + new Vector3((float)distX, (float)distY, 0);
        if (OptimizationUtils.containsPoint(container, potentialPos)) positions.Add(potentialPos);

        int numElements = elements.Count;
        int numPositions = positions.Count;
        if (numElements > numPositions) return false;

        for (int eIdx = 0; eIdx < numElements && eIdx < numPositions; eIdx++)
        {
            elements[eIdx].position = positions[eIdx];
        }

        return true;
    }


    // Templating within containers 
    private void templatePlacement()
    {
        for (int cIdx = 0; cIdx < _numContainers; cIdx++)
        {
            Transform container = _containersTransform.GetChild(cIdx);
            List<Transform> elements = getContainerElements(container);
            int numElements = elements.Count;
            if (numElements <= 0) continue;

            float width = 0.0f, height = 0.0f;
            foreach (Transform element in elements)
            {
                int eIdx = _placements.IndexOf(element);
                width = Mathf.Max((float)(_elements[eIdx].optimizedScale * _elements[eIdx].hSize), width);
                height = Mathf.Max((float)(_elements[eIdx].optimizedScale * _elements[eIdx].vSize), height);
            }
            width += 0.1f;
            height += 0.1f;
            int xSlots = Mathf.Max(Mathf.FloorToInt((float)(_containersTransform.GetChild(cIdx).Find("bounds").localScale.x / width)), 1);
            int zSlots = Mathf.Max(Mathf.FloorToInt((float)(_containersTransform.GetChild(cIdx).Find("bounds").localScale.z / width)), 1);
            int ySlots = Mathf.Max(Mathf.FloorToInt((float)(_containersTransform.GetChild(cIdx).Find("bounds").localScale.y / height)), 1);
            float xOffset = (int)(xSlots / 2) - (0.5f * ((xSlots + 1) % 2));
            float yOffset = (int)(ySlots / 2) - (0.5f * ((ySlots + 1) % 2));
            float zOffset = (int)(zSlots / 2) - (0.5f * ((zSlots + 1) % 2));

            // Placement slots
            List<Vector3> slots = new List<Vector3>();
            List<Vector3Int> slotIndexes = new List<Vector3Int>();
            List<float> slotDists = new List<float>();
            for (int zSlot = 0; zSlot < zSlots; zSlot++)
            {
                for (int ySlot = 0; ySlot < ySlots; ySlot++)
                {
                    for (int xSlot = 0; xSlot < xSlots; xSlot++)
                    {
                        Vector3 slot = new Vector3(width * (xSlot - xOffset),
                                height * (ySlot - yOffset),
                                width * (zSlot - zOffset));
                        slotDists.Add((container.TransformPoint(slot) - _user.transform.position).magnitude);
                        Vector3Int slotIndex = new Vector3Int(xSlot, ySlot, zSlot);
                        slots.Add(slot);
                        slotIndexes.Add(slotIndex);
                    }
                }
            }
            int numSlots = slots.Count;

            /*
            // Objects 
            List<int> objectIndexes = new List<int>();
            for (int oIdx = 0; oIdx < _numObjects; oIdx++)
            {
                if (OptimizationUtils.containsObject(container, _objects[oIdx])) objectIndexes.Add(oIdx);
            }
            int numObjects = objectIndexes.Count;

            // Anchor elements to objects accordingly 
            List<int> objectSlotIndexes = new List<int>();
            for (int oIdx = 0; oIdx < numObjects; oIdx++)
            {
                Transform obj = _objects[objectIndexes[oIdx]];
                float closest = Mathf.Infinity;
                int closestIdx = 0; 
                for (int sIdx = 0; sIdx < numSlots; sIdx++)
                {
                    float dist = (obj.position - container.TransformPoint(slots[sIdx])).magnitude;
                    if (dist < closest)
                    {
                        closest = dist;
                        closestIdx = sIdx;
                    }
                }
                objectSlotIndexes.Add(closestIdx);
            }
            for (int oIdx = 0; oIdx < numObjects; oIdx++)
            {

            }
            */
            
            Vector3[] slotsArray = slots.ToArray();
            float[] slotDistsArray = slotDists.ToArray();
            // Sort by distance from user 
            Array.Sort(slotDistsArray, slotsArray);

            // Sort elements by utility 
            List<float> utility = new List<float>();
            foreach (Transform element in elements)
            {
                int eIdx = _placements.IndexOf(element);
                utility.Add((float)_elements[eIdx].utility);
            }
            float[] utilityArray = utility.ToArray();
            Transform[] elementsArray = elements.ToArray();
            Array.Sort(utilityArray, elementsArray);

            int etIdx = 0; 
            for (int slot = 0; slot < numSlots; slot++)
            {
                elementsArray[etIdx++].localPosition = slotsArray[slot];
                if (etIdx >= numElements) break; 
            }

        }
    }

    // Helper function for getting the closest object
    private int getClosestObj(Transform element, string obj)
    {
        float closestDist = Mathf.Infinity;
        float dist;
        int closest = -1;
        for (int oIdx = 0; oIdx < _numObjects; oIdx++)
        {
            if (_objects[oIdx].name == obj)
            {
                dist = Vector3.Distance(element.position, _objects[oIdx].position);
                if (dist < closestDist)
                {
                    closest = oIdx;
                    closestDist = dist;
                }
            }
        }
        return closest;
    }

    private Transform visualizeSemanticConnection(Transform element, Transform obj)
    {
        GameObject connection = GameObject.Instantiate(_semanticConnectionPrefab, element.parent);
        connection.name = "SemanticConnection";
        Vector3 elementPos = element.position;
        Vector3 objPos = obj.position;
        Vector3 objToElement = elementPos - objPos;
        connection.transform.position = 0.5f * (elementPos + objPos);
        connection.transform.rotation = Quaternion.FromToRotation(Vector3.forward, objToElement);
        connection.transform.localScale = new Vector3(0.01f, 0.01f, objToElement.magnitude);
        return connection.transform;
    }

    private int getSemanticConnectionGeneral(string element, string obj)
    {
        int sNum = _semanticConnectionsGeneral.Count;
        for (int sIdx = 0; sIdx < sNum; sIdx++)
        {
            SemanticConnectionGeneral connection = _semanticConnectionsGeneral[sIdx];
            if (connection.element == element && connection.obj == obj)
            {
                return sIdx;
            }
        }
        return -1; 
    }

    private int getSemanticConnection(int element, int obj)
    {
        int returnIdx = -1;
        int sNum = _semanticConnections.Count;
        for (int sIdx = 0; sIdx < sNum; sIdx++)
        {
            SemanticConnection connection = _semanticConnections[sIdx];
            if (connection.element == element && connection.obj == obj)
            {
                returnIdx = sIdx;
                break;
            }
        }
        return returnIdx;
    }

    private int getSemanticConnection(Transform connection)
    {
        int returnIdx = -1;
        int sNum = _semanticConnections.Count;
        for (int sIdx = 0; sIdx < sNum; sIdx++)
        {
            if (_semanticConnections[sIdx].connection == connection)
            {
                returnIdx = sIdx;
                break;
            }
        }
        return returnIdx;
    }

    public void addSemanticConnection(Transform element, Transform obj, double weight, bool overriding)
    {
        int eIdx = _placements.IndexOf(element);
        int oIdx = _objects.IndexOf(obj);
        int sIdx = getSemanticConnection(eIdx, oIdx);
        if (sIdx >= 0)
        {
            if (overriding)
            {
                _semanticConnections[sIdx].overriding = overriding;
                _semanticConnections[sIdx].weight = weight;
            }
            if (_semanticConnections[sIdx].connection != null)
            {
                _semanticConnections[sIdx].connection.SetParent(null);
                GameObject.Destroy(_semanticConnections[sIdx].connection.gameObject);
            }
            _semanticConnections[sIdx].connection = visualizeSemanticConnection(element, obj);
        } else
        {
            SemanticConnection newConnection = new SemanticConnection();
            newConnection.overriding = overriding;
            newConnection.element = eIdx;
            newConnection.obj = oIdx;
            newConnection.weight = weight; 
            newConnection.connection = visualizeSemanticConnection(element, obj);
            _semanticConnections.Add(newConnection);
        }

        // General semantic connection 
        if (overriding)
        {
            int gsIdx = getSemanticConnectionGeneral(element.name, obj.name);
            if (gsIdx >= 0) _semanticConnectionsGeneral[gsIdx].weight = weight;
            else {
                SemanticConnectionGeneral newConnection = new SemanticConnectionGeneral();
                newConnection.element = element.name;
                newConnection.obj = obj.name;
                newConnection.weight = weight;
                _semanticConnectionsGeneral.Add(newConnection);
            }
        }
    }

    public void removeSemanticConnection(Transform connection)
    {
        int sIdx = getSemanticConnection(connection);
        
        if (sIdx >= 0)
        {
            SemanticConnection remove = _semanticConnections[sIdx];
            // General semantic connection 
            string element = _placements[remove.element].name;
            string obj = _objects[remove.obj].name;
            int gsIdx = getSemanticConnectionGeneral(element, obj);
            if (gsIdx >= 0) _semanticConnectionsGeneral[gsIdx].weight = 0;
            else
            {
                SemanticConnectionGeneral newConnection = new SemanticConnectionGeneral();
                newConnection.element = element;
                newConnection.obj = element;
                newConnection.weight = 0;
                _semanticConnectionsGeneral.Add(newConnection);
            }

            // Specific semantic connection
            remove.connection.SetParent(null);
            GameObject.Destroy(remove.connection.gameObject);
            remove.overriding = true;
            remove.weight = 0; 
        }
        
    }

    public void clearSemanticConnectionTransforms()
    {
        foreach (SemanticConnection connection in _semanticConnections)
        {
            if (connection.connection != null)
            {
                connection.connection.SetParent(null);
                GameObject.Destroy(connection.connection.gameObject);
            }
        }
    }

    public void clearSemanticConnections()
    {
        // Specific semantic connections
        clearSemanticConnectionTransforms();
        _semanticConnections.Clear();
        // General semantic connections 
        _semanticConnectionsGeneral.Clear();
    }

    private void clearOptimizationResults()
    {
        // Clear elements for containers
        for (int cIdx = 0; cIdx < _numContainers; cIdx++)
        {
            Transform container = _containersTransform.GetChild(cIdx);
            List<Transform> elements = getContainerElements(container);
            int eNum = elements.Count;
            for (int eIdx = 0; eIdx < eNum; eIdx++)
            {
                elements[eIdx].SetParent(null);
                GameObject.Destroy(elements[eIdx].gameObject);
            }
        }

        // Clear semantic connection transforms
        clearSemanticConnectionTransforms();
        // Clear non-overriding semantic connections 
        int numConnections = _semanticConnections.Count;
        for (int sIdx = numConnections - 1; sIdx >= 0; sIdx--)
        {
            if (!_semanticConnections[sIdx].overriding) {
                if (_semanticConnections[sIdx].connection != null)
                {
                    _semanticConnections[sIdx].connection.SetParent(null);
                    GameObject.Destroy(_semanticConnections[sIdx].connection.gameObject);
                    _semanticConnections[sIdx].connection = null;
                }
                _semanticConnections.RemoveAt(sIdx);
            }
        }
    }

    private void visualizeSemanticConnections()
    {
        // New connections by proximity 
        for (int cIdx = 0; cIdx < _numContainers; cIdx++)
        {
            Transform container = _containersTransform.GetChild(cIdx);
            List<Transform> elements = getContainerElements(container);
            double[] containerObjectAssociations = _containers[cIdx].objects;
            foreach (Transform element in elements)
            {
                int eIdx = _placements.IndexOf(element);
                double[] elementObjectAssociations = _elements[eIdx].anchors;
                for (int oIdx = 0; oIdx < _numUniqueObjects; oIdx++)
                {
                    if (elementObjectAssociations[oIdx] * containerObjectAssociations[oIdx] > _visualizeSemanticConnectionsThreshold)
                    {
                        string obj = _uniqueObjects[oIdx];
                        int closest = getClosestObj(element, obj);
                        if (closest >= 0) addSemanticConnection(element, _objects[closest], elementObjectAssociations[oIdx], false);
                    }
                }
            }
        }
        // Existing semantic associations
        foreach (SemanticConnection connection in _semanticConnections)
        {
            if (connection.overriding && (connection.weight > 0))
            {
                connection.connection.SetParent(null);
                GameObject.Destroy(connection.connection.gameObject);
                connection.connection = visualizeSemanticConnection(_placements[connection.element], _objects[connection.obj]);
            }
        }
        foreach (SemanticConnectionGeneral connection in _semanticConnectionsGeneral)
        {
            int element = -1;
            for (int eIdx = 0; eIdx < _numElements; eIdx++)
            {
                if (_placements[eIdx].name == connection.element)
                {
                    element = eIdx;
                    break;
                }
            }
            if (element >= 0) {
                int obj = getClosestObj(_placements[element], connection.obj);
                if (obj >= 0 && connection.weight > 0)
                {
                    addSemanticConnection(_placements[element], _objects[obj], connection.weight, false);
                }
            }
        }
    }

    public void optimize()
    {
        // Save previous element placements
        saveElementSamples();

        // Containers set-up
        reorientContainers();
        getContainers();

        // Compute 
        if (_pastSamples.Count <= 0)
        {
            Debug.Log("Utility from User Input");
            defineUtilitySpatialDependencies();
            //setContainerUtilityValues();
        } else
        {
            Debug.Log("Utility from Past Placements");
            //setContainerUtilityValuesFromPast();
        }
        setContainerUtilityValues();

        // Elements set-up
        getElements();

        getUserDefinedConnections();
        
        clearOptimizationResults();
        performOptimization();

        displayOptimizationResults();
        templatePlacement();

        visualizeSemanticConnections();
    }

    public void initOptimization()
    {

        if (_optimizationInitialized) return;

        getEnvironment();
        getObjects();
        loadElementsNew();
        //loadElementsSaved();
    }

    public void resetOptimization()
    {
        clearOptimizationResults();
        _optimizationInitialized = false;
        initOptimization();
    }

    public void changeEnvironments(Constants.Environments env)
    {
        // Save environment placements for sampling in the future
        saveElementSamples();

        clearOptimizationResults();

        // Update environment 
        _environmentSelection = env;
        getEnvironment();
        _environmentTransform.gameObject.SetActive(false);
        _environmentTransform = _allEnvironmentData.Find("Environment_" + _environment).transform;
        _environmentTransform.gameObject.SetActive(true);
        // Update objects 
        _objectsTransform.gameObject.SetActive(false);
        _objectsTransform = _allEnvironmentData.Find("Objects_" + _environment).transform;
        _objectsTransform.gameObject.SetActive(true);
        // Update containers
        _containersTransform.gameObject.SetActive(false);
        _containersTransform = _allEnvironmentData.Find("Containers_" + _environment).transform;
        _containersTransform.gameObject.SetActive(true);

        // Clear prior environment-specific semantic connections 
        if (_optimizedEnvironment != _environment) _semanticConnections.Clear();

        // Initialize optimization 
        _optimizationInitialized = false;
        initOptimization();
        
    }

    private void Start()
    {
        if (_connectWebSocket) _ws.webSocketConnect();
        initOptimization();

        //if (_optimize) optimize();
    }

    private void Update()
    {
        if (_optimize && Input.GetKeyDown(KeyCode.O))
        {
            optimize();
            changeEnvironments(Constants.Environments.Office);
            optimize();
        }
    }
}
 