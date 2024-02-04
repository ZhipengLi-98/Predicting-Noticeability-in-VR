using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OptimizationUtils
{
    public static double normalize(double value)
    {
        return Mathf.Max((float)(value - Constants.MIN_RATING), 0) / (Constants.MAX_RATING - Constants.MIN_RATING);
    }

    // OptimizationV2 version 
    public static ElementModel defineElementModel(
        int identifier, 
        Transform elementTransform, 
        ElementSettings userDefinedProperties, 
        List<ElementObjectSemanticConnection> semanticConnections,
        List<string> uniqueObjects
    ) {
        ElementModel element = new ElementModel();
        element.identifier = identifier;
        Vector3 bounds;
        switch (userDefinedProperties.type)
        {
            case Constants.Dimensionality.TwoDimensional:
                element.type2D = 1;
                bounds = elementTransform.lossyScale;
                element.hSize = Mathf.Max(bounds.x, bounds.z);
                element.vSize = bounds.y;
                break;
            case Constants.Dimensionality.ThreeDimensional:
                element.type3D = 1;
                bounds = elementTransform.Find("bounds").lossyScale;
                element.hSize = Mathf.Max(bounds.x, bounds.z);
                element.vSize = bounds.y;
                break;
        }
        element.utility = normalize(userDefinedProperties.utility);
        element.visReq = normalize(userDefinedProperties.visibilityRequirement);
        element.touchReq = normalize(userDefinedProperties.touchRequirement);
        element.backgroundTol = normalize(userDefinedProperties.backgroundTolerance);

        // Anchoring behaviour
        string name = elementTransform.name;
        int oNum = uniqueObjects.Count;
        double[] anchors = new double[oNum];
        for (int oIdx = 0; oIdx < oNum; oIdx++)
        {
            int entryIdx = ObjectSemantics.objects.IndexOf(uniqueObjects[oIdx]);
            //Debug.Log(uniqueObjects[oIdx]);
            anchors[oIdx] = ObjectSemantics.elementAnchors[name][entryIdx];
        }
        // Overriding values 
        ElementObjectRelationship[] anchorOverrides = userDefinedProperties.anchorObjects;
        int anoNum = anchorOverrides.Length;
        for (int aoIdx = 0; aoIdx < anoNum; aoIdx++)
        {
            ElementObjectRelationship anchorOverride = anchorOverrides[aoIdx];
            int entryIdx = uniqueObjects.IndexOf(anchorOverride.environmentObject);
            if (entryIdx < 0) continue;
            anchors[entryIdx] = normalize(anchorOverride.association);
        }

        // Avoiding behaviour 
        // Using global avoidance list 
        double[] avoidances = new double[oNum];
        int aNum = ObjectSemantics.avoidances.Length;
        for (int aIdx = 0; aIdx < aNum; aIdx++)
        {
            string avoidance = ObjectSemantics.avoidances[aIdx];
            if (!uniqueObjects.Contains(avoidance)) continue;
            int entryIdx = uniqueObjects.IndexOf(avoidance);
            avoidances[entryIdx] = 1;
            anchors[entryIdx] = 0;
        }
        // Overriding values 
        ElementObjectRelationship[] avoidOverrides = userDefinedProperties.avoidObjects;
        int avoNum = avoidOverrides.Length;
        for (int aoIdx = 0; aoIdx < avoNum; aoIdx++)
        {
            ElementObjectRelationship avoidOverride = avoidOverrides[aoIdx];
            int entryIdx = uniqueObjects.IndexOf(avoidOverride.environmentObject);
            if (entryIdx < 0) continue;
            avoidances[entryIdx] = normalize(avoidOverride.association);
            anchors[entryIdx] = 0;
        }

        // Overriding semantic connections 
        foreach (ElementObjectSemanticConnection connection in semanticConnections)
        {
            if (connection.element == elementTransform.name)
            {
                int entry = uniqueObjects.IndexOf(connection.obj);
                if (entry >= 0)
                {
                    anchors[entry] = connection.weight;
                    if (connection.weight > 0) avoidances[entry] = 0;
                }
            }
        }

        element.anchors = anchors;
        element.avoidances = avoidances;

        return element; 
    }

    public static ElementModel defineElementModel(
        int identifier, 
        ElementSettings userDefinedProperties, 
        Transform elementTransform, 
        Transform user, 
        List<string> uniqueObjects,
        List<Transform> objects,
        List<SemanticConnectionGeneral> semanticConnections
    ) {
        // Identifier
        ElementModel element = new ElementModel();
        element.identifier = identifier;

        // Spatial properties 
        Vector3 bounds;
        switch (userDefinedProperties.type)
        {
            case Constants.Dimensionality.TwoDimensional:
                element.type2D = 1;
                bounds = elementTransform.lossyScale;
                element.hSize = Mathf.Max(bounds.x, bounds.z);
                element.vSize = bounds.y;
                element.scale = bounds;
                break;
            case Constants.Dimensionality.ThreeDimensional:
                element.type3D = 1;
                bounds = elementTransform.Find("bounds").lossyScale;
                element.hSize = Mathf.Max(bounds.x, bounds.z);
                element.vSize = bounds.y;
                element.scale = bounds;
                break;
        }

        element.minScale = userDefinedProperties.minimumScale;
        element.maxScale = userDefinedProperties.maximumScale;
        Vector3 relativePosition = elementTransform.position - user.position;
        element.x = relativePosition.x;
        element.y = relativePosition.y;
        element.z = relativePosition.z;
        element.dist = relativePosition.magnitude;
        element.fromForward = Vector3.Dot(user.forward, relativePosition.normalized);

        // Element Properties
        element.utility = normalize(userDefinedProperties.utility);
        element.visReq = normalize(userDefinedProperties.visibilityRequirement);
        element.touchReq = normalize(userDefinedProperties.touchRequirement);
        element.backgroundTol = normalize(userDefinedProperties.backgroundTolerance);

        // Anchoring behaviour
        string name = elementTransform.name;
        int oNum = uniqueObjects.Count;
        double[] anchors = new double[oNum];
        for (int oIdx = 0; oIdx < oNum; oIdx++)
        {
            int entryIdx = ObjectSemantics.objects.IndexOf(uniqueObjects[oIdx]);
            anchors[oIdx] = ObjectSemantics.elementAnchors[name][entryIdx];
        }
        // Overriding values 
        ElementObjectRelationship[] anchorOverrides = userDefinedProperties.anchorObjects;
        int anoNum = anchorOverrides.Length;
        for (int aoIdx = 0; aoIdx < anoNum; aoIdx++)
        {
            ElementObjectRelationship anchorOverride = anchorOverrides[aoIdx];
            if (!uniqueObjects.Contains(anchorOverride.environmentObject)) continue;
            int entryIdx = uniqueObjects.IndexOf(anchorOverride.environmentObject);
            anchors[entryIdx] = normalize(anchorOverride.association);
        }

        // Avoiding behaviour 
        double[] avoidances = new double[oNum];
        int aNum = ObjectSemantics.avoidances.Length; // Global avoidance list
        for (int aIdx = 0; aIdx < aNum; aIdx++)
        {
            string avoidance = ObjectSemantics.avoidances[aIdx];
            if (!uniqueObjects.Contains(avoidance)) continue;
            int entryIdx = uniqueObjects.IndexOf(avoidance);
            avoidances[entryIdx] = 1;
            // Currently setting avoidances to take precedence over anchors 
            // We zero anchors for avoid objects 
            anchors[entryIdx] = 0;
        }
        // Overriding values 
        ElementObjectRelationship[] avoidOverrides = userDefinedProperties.avoidObjects;
        int avoNum = avoidOverrides.Length;
        for (int aoIdx = 0; aoIdx < avoNum; aoIdx++)
        {
            ElementObjectRelationship avoidOverride = avoidOverrides[aoIdx];
            if (!uniqueObjects.Contains(avoidOverride.environmentObject)) continue;
            int entryIdx = uniqueObjects.IndexOf(avoidOverride.environmentObject);
            avoidances[entryIdx] = normalize(avoidOverride.association);
            // Currently setting avoidances to take precedence over anchors 
            // We zero anchors for avoid objects 
            anchors[entryIdx] = 0;
        }

        // Overriding semantic connections 
        foreach (SemanticConnectionGeneral connection in semanticConnections)
        {
            if (connection.element == elementTransform.name)
            {
                int entry = uniqueObjects.IndexOf(connection.obj);
                if (entry >= 0)
                {
                    anchors[entry] = connection.weight;
                    if (connection.weight > 0) avoidances[entry] = 0;
                }
            }
        }

        element.anchors = anchors;
        element.avoidances = avoidances;

        return element;
    }

    public static double smoothstep(float edge0, float edge1, float x)
    {
        float result = Mathf.Clamp((x - edge0) / (edge1 - edge0), 0, 1);
        return result * result * (3 - (2 * result));
    }

    // OptimizationV2 version 
    public static double computeContainerVisibility(Vector3 position, Transform user)
    {
        Vector3 forward = user.forward;
        Vector3 toPosition = (position - user.position).normalized;
        float angle = Vector3.Dot(toPosition, forward);
        return smoothstep(Mathf.Cos(Mathf.Deg2Rad * 60.0f), Mathf.Cos(Mathf.Deg2Rad * 2.0f), angle);
    }

    public static double computeContainerVisibility(double fromForward)
    {
        if (fromForward < OptimizationParameters.containerVisibilityThreshold)
        {
            return (1 + fromForward) / (1 + OptimizationParameters.containerVisibilityThreshold);
        }
        return 1;
    }

    // OptimizationV2 version 
    public static double computeContainerTouchSupport(Vector3 position, Transform user)
    {
        float dist = Vector3.Distance(position, user.position);
        return 1 / (1 + Mathf.Exp(10.0f * (dist - (float)OptimizationParameters.containerTouchThreshold)));
    }

    public static double computeContainerTouchSupport(double dist)
    {
        if (dist > OptimizationParameters.containerTouchThreshold)
        {
            return Mathf.Pow((float)(OptimizationParameters.containerTouchThreshold / dist), 2);
        }
        return 1;
    }

    public static Vector2 computeAngularPosition(Vector3 direction)
    {
        Vector3 xz = new Vector3(direction.x, 0, direction.z);
        Vector3 forward = new Vector3(0, 0, 1);
        float longitude = Quaternion.FromToRotation(forward, xz.normalized).eulerAngles.y;
        Vector3 y = new Vector3(0, direction.y, xz.magnitude);
        Vector3 up = new Vector3(0, 1, 0);
        float latitude = Quaternion.FromToRotation(up, y.normalized).eulerAngles.x - 90;
        return new Vector2(longitude, latitude);
    }

    public static double computeContainerBackgroundComplexity(Vector3 position, Transform user, string environment)
    {
        Vector2 angularPos = computeAngularPosition((position - user.position).normalized);
        int queryLon = (((int)angularPos.x - 10) / 20) * 20 + 10;
        int queryLat = (((int)angularPos.y + 70) / 20) * 20 - 70;
        
        if (EnvironmentBackgrounds.backgroundComplexity.TryGetValue(environment, out Dictionary<Tuple<double, double>, double> backgroundComplexity))
        { 
            backgroundComplexity = EnvironmentBackgrounds.backgroundComplexity[environment];
            if (backgroundComplexity.TryGetValue(new Tuple<double, double>(queryLat, queryLon), out double patchComplexity))
            {
                return patchComplexity;
            }
            else
            {
                Debug.Log("Background complexity calculation failure");
                return 0.0;
            }
        } else
        {
            return 0.0;
        }
    }

    public static double computeContainerBackgroundComplexity(Transform containerTransform, Transform user, string environment)
    {
        Dictionary<Tuple<double, double>, double> backgroundComplexity = EnvironmentBackgrounds.backgroundComplexity[environment];

        Vector2 min = new Vector2(Mathf.Infinity, Mathf.Infinity);
        Vector2 max = new Vector2(Mathf.NegativeInfinity, Mathf.NegativeInfinity);
        // Check extents 
        Vector3 sample;
        Quaternion rotation = containerTransform.rotation;
        Vector3 scale = containerTransform.Find("bounds").lossyScale;
        Vector2 sampleAngularPos;
        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    sample = new Vector3(x, y, z);
                    sample = Vector3.Scale(scale, sample);
                    sample = rotation * sample;
                    sample = containerTransform.position + sample;
                    sampleAngularPos = computeAngularPosition(sample - user.position);
                    min = Vector3.Min(sampleAngularPos, min);
                    max = Vector3.Max(sampleAngularPos, max);
                }
            }
        }
        Vector2 extents = new Vector2(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));
        if (extents.x > 180) extents.x = 360 - extents.x;
        if (extents.y > 180) extents.y = 360 - extents.y;
        Vector3 containerRelativePos = (containerTransform.position - user.position).normalized;
        Vector2 containerAngularPos = computeAngularPosition(containerRelativePos);
        Vector2 buffer = new Vector2(10, 10);
        min = containerAngularPos - (0.5f * extents) - buffer;
        max = containerAngularPos + (0.5f * extents) + buffer;

        // Query background complexity values 
        int queryLon, queryLat;
        double patchComplexity;
        double complexity = 0;
        double patchCount = 0;
        for (double lon = min.x; lon < max.x; lon += 20)
        {
            for (double lat = min.y; lat < max.y; lat += 20)
            {
                queryLon = (int)lon;
                if (queryLon < 0) queryLon = 360 + queryLon;
                if (queryLon > 360) queryLon = queryLon % 360;
                queryLat = (int)lat;
                if (queryLat > 90)
                {
                    queryLat = 180 - queryLat;
                    queryLon = queryLon + 180;
                }
                if (queryLat < -90)
                {
                    queryLat = -180 - queryLat;
                    queryLon = queryLon + 180;
                }
                if (queryLon < 0) queryLon = 360 + queryLon;
                if (queryLon > 360) queryLon = queryLon % 360;

                queryLon = ((queryLon - 10) / 20) * 20 + 10;
                queryLat = ((queryLat + 70) / 20) * 20 - 70;

                if (!backgroundComplexity.TryGetValue(new Tuple<double, double>(queryLat, queryLon), out patchComplexity)) continue;
                complexity += patchComplexity;
                patchCount++;
            }
        }
        if (patchCount <= 0) return 0;
        return complexity / patchCount;
    }

    public static bool containsObject(Transform container, Transform obj)
    {
        float distToObj = Vector3.Distance(container.position, obj.position);
        Vector3 bounds = container.Find("bounds").localScale;
        double max = 0.5 * Mathf.Min(new float[] { bounds.x, bounds.y, bounds.z });
        return distToObj < max;
    }

    public static bool containsPoint(Transform container, Vector3 point)
    {
        float distToObj = Vector3.Distance(container.position, point);
        Vector3 bounds = container.Find("bounds").localScale;
        double max = 0.5 * bounds.magnitude;
        return distToObj < max;
    }

    // OptimizationV2 version 
    public static double computeContainerObjectAssociation(Vector3 position, Transform obj, Vector2 voxelSize)
    {
        Vector3 objPos = obj.position;
        Vector3 containerToObjDir = (objPos - position).normalized;
        Collider objCollider = obj.GetComponent<Collider>();
        RaycastHit objHit;
        Vector3 closestPoint;
        if (objCollider.Raycast(new Ray(position, containerToObjDir), out objHit, Mathf.Infinity))
        {
            closestPoint = objHit.point;
            float toClosestDist = Vector3.Distance(closestPoint, objPos);
            float toContainerDist = Vector3.Distance(position, objPos);
            float diff = toContainerDist - toClosestDist;
            if (diff <= 0) return 1; 
            else return Mathf.Clamp(Mathf.Pow((0.5f * voxelSize.magnitude) / diff, 2), 0, 1);
        }
        else return 1;
    }

    public static double computeContainerObjectAssociation(Transform container, Transform obj, Transform user)
    {
        Vector3 containerPos = container.position;
        Vector3 objPos = obj.position;

        Vector3 containerToObj = (objPos - containerPos).normalized;
        Vector3 objToContainer = -containerToObj;

        Collider containerCollider = container.Find("bounds").GetComponent<Collider>();
        Collider objCollider = obj.GetComponent<Collider>();

        Vector3 containerClosest, objClosest;
        RaycastHit containerHit;

        if (containerToObj.magnitude <= 0) return 1;

        if (containerCollider.Raycast(new Ray(objPos, objToContainer), out containerHit, Mathf.Infinity)) containerClosest = containerHit.point;
        else return 1;

        RaycastHit objHit;
        if (objCollider.Raycast(new Ray(containerPos, containerToObj), out objHit, Mathf.Infinity)) objClosest = objHit.point;
        else return 1;

        if ((objClosest - objPos).magnitude > (containerClosest - objPos).magnitude) return 1;

        double distance = Vector3.Distance(objClosest, containerClosest);
        double weight = 1 / (1 + (OptimizationParameters.containerObjectInfluenceDecayRate * distance));

        Vector3 userPos = user.position;
        Vector3 userToObj = (objPos - userPos).normalized;
        RaycastHit projectionHit;
        if (containerCollider.Raycast(new Ray(userPos, userToObj), out projectionHit, Mathf.Infinity))
        {
            weight = Mathf.Max((float)weight, 0.5f);
        }

        return weight;
    }

    public static double getContainerSpatialDimension(ContainerModel container, Constants.SpatialDimensions dim)
    {
        switch (dim)
        {
            case Constants.SpatialDimensions.x:
                return container.x;
            case Constants.SpatialDimensions.xAbs:
                return container.xAbs;
            case Constants.SpatialDimensions.y:
                return container.y;
            case Constants.SpatialDimensions.yAbs:
                return container.yAbs;
            case Constants.SpatialDimensions.z:
                return container.z;
            case Constants.SpatialDimensions.zAbs:
                return container.zAbs;
            case Constants.SpatialDimensions.dist:
                return container.dist;
            case Constants.SpatialDimensions.fromForward:
                return container.fromForward;
        }
        return 0;
    }

    public static double getElementSpatialDimension(ElementModel element, Constants.SpatialDimensions dim)
    {
        switch (dim)
        {
            case Constants.SpatialDimensions.x:
                return element.x;
            case Constants.SpatialDimensions.xAbs:
                return element.xAbs;
            case Constants.SpatialDimensions.y:
                return element.y;
            case Constants.SpatialDimensions.yAbs:
                return element.yAbs;
            case Constants.SpatialDimensions.z:
                return element.z;
            case Constants.SpatialDimensions.zAbs:
                return element.zAbs;
            case Constants.SpatialDimensions.dist:
                return element.dist;
            case Constants.SpatialDimensions.fromForward:
                return element.fromForward;
        }
        return 0;
    }

    public static double[][] computeNormalizedContainerSpatialDimensions(List<ContainerModel> containers, List<Constants.SpatialDimensions> dependencies)
    {
        int sNum = dependencies.Count;
        int cNum = containers.Count;
        double[][] spatialDimensions = new double[sNum][];
        double min, max, range;
        for (int sIdx = 0; sIdx < sNum; sIdx++)
        {
            spatialDimensions[sIdx] = new double[cNum];
            min = Mathf.Infinity;
            max = Mathf.NegativeInfinity;
            for (int cIdx = 0; cIdx < cNum; cIdx++)
            {
                double val = getContainerSpatialDimension(containers[cIdx], dependencies[sIdx]);
                spatialDimensions[sIdx][cIdx] = val;
                min = Mathf.Min((float)min, (float)val);
                max = Mathf.Max((float)max, (float)val);
            }
            range = max - min;
            for (int cIdx = 0; cIdx < cNum; cIdx++)
            {
                spatialDimensions[sIdx][cIdx] = (spatialDimensions[sIdx][cIdx] - min) / range;
            }
        }

        return spatialDimensions;
    }

    // OptimizationV2 version 
    public static double computeUtility(Vector3 position, Transform user)
    {
        Vector3 toPosition = position - user.position;
        float dist = toPosition.magnitude;
        float distUtility = Mathf.Clamp(Mathf.Exp(-(1.0f/(float)OptimizationParameters.containerMaxUtilityDistanceRange) * 
            Mathf.Pow((dist - (float)OptimizationParameters.containerMaxUtilityDistanceMean), 2)), 0, 1);

        float dirUtility = 0.0f;
        Vector3 toPositionDir = toPosition.normalized;
        Vector3 forward = user.forward;
        Vector3 right = user.right;
        float forwardToPosition = Vector3.Dot(forward, toPositionDir);
        float rightToPosition = Vector3.Dot(right, toPositionDir);
        if (Math.Abs(forwardToPosition) > Mathf.Abs(rightToPosition))
        {
            if (forwardToPosition > 0) dirUtility = 1.0f;
        } else
        {
            dirUtility = 0.5f; 
        }

        float heightDifference = Mathf.Abs(position.y - user.position.y);
        float heightUtility = 1 / (1 + Mathf.Exp(10 * (heightDifference - (float)OptimizationParameters.containerHighUtilityHeightCutoff)));

        return (distUtility + dirUtility + heightUtility) / 3;
    }

    // Compute utility from list of dependencies
    // Produces a spatial heatmap from dependencies and weights 
    // Samples from heatmap for utility values 
    // (No previous placements needed for sampling utility values)
    public static double[] computeUtility(List<ContainerModel> containers, List<Constants.SpatialDimensions> dependencies, List<double> weights)
    {
        // Get utility-dependent utility values 
        int sNum = dependencies.Count;
        int cNum = containers.Count;
        double[][] spatialDimensions = computeNormalizedContainerSpatialDimensions(containers, dependencies);

        // Sum weights 
        double weightsSum = 0;
        for (int sIdx = 0; sIdx < sNum; sIdx++)
        {
            weightsSum += Mathf.Abs((float)weights[sIdx]);
        }

        // Calculate utility
        double[] utilityValues = new double[cNum];
        for (int cIdx = 0; cIdx < cNum; cIdx++)
        {
            double utilityValue = 0;
            for (int sIdx = 0; sIdx < sNum; sIdx++)
            {
                if (weights[sIdx] > 0) utilityValue += weights[sIdx] * spatialDimensions[sIdx][cIdx];
                else utilityValue += Mathf.Abs((float)weights[sIdx]) * (1 - spatialDimensions[sIdx][cIdx]);
            }
            utilityValues[cIdx] = utilityValue / weightsSum;
        }

        return utilityValues;
    }

    public static int findNearestNeighbor(double val, double[] neighs)
    {
        int nIdx = 0;
        double closest = Mathf.Infinity;
        int numNeighs = neighs.Length;
        for (int i = 0; i < numNeighs; i++)
        {
            double diff = Mathf.Abs((float)(neighs[i] - val));
            if (diff < closest)
            {
                closest = diff;
                nIdx = i;
            }
        }
        return nIdx;
    }

    // Compute utility from past samples 
    public static double[] computeUtilityFromPast(List<ContainerModel> containers, List<Constants.SpatialDimensions> dependencies, List<double> weights, List<Tuple<double[], double>> samples)
    {
        // Get utility-dependent utility values 
        int sNum = dependencies.Count;
        int cNum = containers.Count;
        double[][] spatialDimensions = computeNormalizedContainerSpatialDimensions(containers, dependencies);

        int sampleNum = samples.Count;
        double[][] sampleSpatialDimensions = new double[sNum][];
        double min, max, range;
        for (int sIdx = 0; sIdx < sNum; sIdx++)
        {
            sampleSpatialDimensions[sIdx] = new double[sampleNum];
            min = Mathf.Infinity;
            max = Mathf.NegativeInfinity;
            int idx = Constants.allSpatialDimensions.IndexOf(dependencies[sIdx]);
            for (int sampleIdx = 0; sampleIdx < sampleNum; sampleIdx++)
            {
                double val = samples[sampleIdx].Item1[idx];
                sampleSpatialDimensions[sIdx][sampleIdx] = val;
                min = Mathf.Min((float)min, (float)val);
                max = Mathf.Max((float)max, (float)val);
            }
            range = max - min;
            for (int sampleIdx = 0; sampleIdx < sampleNum; sampleIdx++)
            {
                sampleSpatialDimensions[sIdx][sampleIdx] = (sampleSpatialDimensions[sIdx][sampleIdx] - min) / range;
            }
        }

        // Sum weights 
        double weightsSum = 0;
        for (int sIdx = 0; sIdx < sNum; sIdx++) weightsSum += Mathf.Abs((float)weights[sIdx]);

        // Calculate utility
        double[] utilityValues = new double[cNum];
        for (int cIdx = 0; cIdx < cNum; cIdx++)
        {
            double utilityValue = 0;
            double utilityComponent;
            for (int sIdx = 0; sIdx < sNum; sIdx++)
            {
                utilityComponent = samples[findNearestNeighbor(spatialDimensions[sIdx][cIdx], sampleSpatialDimensions[sIdx])].Item2;
                utilityValue += Mathf.Abs((float)weights[sIdx]) * utilityComponent;
            }
            utilityValues[cIdx] = utilityValue / weightsSum;
            Debug.Log(utilityValues[cIdx]);
        }

        return utilityValues;
    }

    public static double computeAvg(double[] values)
    {
        double avg = 0;
        foreach (double val in values)
        {
            avg += val;
        }
        return (avg / values.Length);
    }

    public static double computeCorrelation(double[] v1, double[] v2)
    {
        int sNum = v1.Length;
        double v1Avg = computeAvg(v1);
        double v2Avg = computeAvg(v2);
        double cov = 0;
        double v1Std = 0;
        double v2Std = 0;
        double v1Diff, v2Diff;
        for (int eIdx = 0; eIdx < sNum; eIdx++)
        {
            v1Diff = (v1[eIdx] - v1Avg);
            v2Diff = (v2[eIdx] - v2Avg);
            cov += (v1Diff * v2Diff);
            v1Std += (v1Diff * v1Diff);
            v2Std += (v2Diff * v2Diff);
        }
        return (cov / Mathf.Sqrt((float)(v1Std * v2Std)));
    }

    public static double[][] computeNormalizedElementSpatialDimensions(List<ElementModel> elements)
    {
        int sNum = Constants.allSpatialDimensions.Count;
        int eNum = elements.Count;
        double[][] spatialDimensions = new double[sNum][];
        double min, max, range;
        for (int sIdx = 0; sIdx < sNum; sIdx++)
        {
            spatialDimensions[sIdx] = new double[eNum];
            min = Mathf.Infinity;
            max = Mathf.NegativeInfinity;
            for (int eIdx = 0; eIdx < eNum; eIdx++)
            {
                double val = getElementSpatialDimension(elements[eIdx], Constants.allSpatialDimensions[sIdx]);
                spatialDimensions[sIdx][eIdx] = val;
                min = Mathf.Min((float)min, (float)val);
                max = Mathf.Max((float)max, (float)val);
            }
            range = max - min;
            for (int eIdx = 0; eIdx < eNum; eIdx++)
            {
                spatialDimensions[sIdx][eIdx] = (spatialDimensions[sIdx][eIdx] - min) / range;
            }
        }

        return spatialDimensions;
    }


    // Determine utility spatial dependencies from an element set 
    public static void computeUtilitySpatialDependenciesFromElements(List<ElementModel> elements, out List<Constants.SpatialDimensions> dims, out List<double> weights)
    {
        dims = new List<Constants.SpatialDimensions>();
        weights = new List<double>();

        double[][] spatialDimensions = computeNormalizedElementSpatialDimensions(elements);

        int sNum = Constants.allSpatialDimensions.Count;
        int eNum = elements.Count;

        double[] utility = new double[eNum];
        for (int eIdx = 0; eIdx < eNum; eIdx++)
        {
            utility[eIdx] = elements[eIdx].utility;
        }

        double correlation;
        for (int sIdx = 0; sIdx < sNum; sIdx++)
        {
            correlation = computeCorrelation(spatialDimensions[sIdx], utility);
            if (Mathf.Abs((float)correlation) < OptimizationParameters.utilitySpatialDependencyThreshold) continue;
            dims.Add(Constants.allSpatialDimensions[sIdx]);
            weights.Add(correlation);
        }
    }

    // OptimizationV2 version 
    public static ContainerModel defineContainerModel(
        int identifier, 
        ContainerSettings userDefinedProperties, 
        Vector3 position, 
        Vector2 voxelSize, 
        Transform user, 
        string environment, 
        List<string> uniqueObjects, 
        List<Transform> objects)
    {

        ContainerModel container = new ContainerModel();
        container.identifier = identifier;

        container.x = position.x;
        container.y = position.y;
        container.z = position.z;

        switch (userDefinedProperties.type)
        {
            case Constants.Dimensionality.TwoDimensional:
                container.support2D = OptimizationParameters.container2DSupport2D;
                container.support3D = OptimizationParameters.container2DSupport3D;
                break;
            case Constants.Dimensionality.ThreeDimensional:
                container.support2D = OptimizationParameters.container3DSupport2D;
                container.support3D = OptimizationParameters.container3DSupport3D;
                break;
        }

        container.visibility = computeContainerVisibility(position, user);
        
        container.touchSupport = computeContainerTouchSupport(position, user);
        
        container.backgroundComplexity = computeContainerBackgroundComplexity(position, user, environment);

        container.utility = computeUtility(position, user);
        
        if (userDefinedProperties.overrideUtility) container.utility = normalize(userDefinedProperties.utility);

        int numUniqueObjects = uniqueObjects.Count;
        double[] associations = new double[numUniqueObjects];
        double associatedObjUtility = 0.0; 
        foreach (Transform obj in objects)
        {
            int uoIdx = uniqueObjects.IndexOf(obj.name);
            if (uoIdx < 0) continue;
            double association = computeContainerObjectAssociation(position, obj, voxelSize);
            double objUtility = normalize(obj.gameObject.GetComponent<ObjectSettings>().utility);
            associatedObjUtility = Mathf.Max((float)(association * objUtility), (float)associatedObjUtility);
            associations[uoIdx] = Mathf.Max((float)associations[uoIdx], (float)association);
        }
        container.associatedObjUtility = associatedObjUtility;

        ContainerObjectRelationship[] associationOverrides = userDefinedProperties.associations;
        int aNum = associationOverrides.Length;
        for (int aIdx = 0; aIdx < aNum; aIdx++)
        {
            ContainerObjectRelationship associationOverride = associationOverrides[aIdx];
            if (!uniqueObjects.Contains(associationOverride.environmentObject)) continue;
            int uoIdx = uniqueObjects.IndexOf(associationOverride.environmentObject);
            associations[uoIdx] = normalize(associationOverride.association);
        }
        container.objects = associations;

        return container; 
    }

    // initializes a container model with all properties 
    // except utility and background complexity
    public static ContainerModel defineContainerModel(
        int identifier,
        ContainerSettings userDefinedProperties,
        Transform containerTransform,
        Transform user,
        List<string> uniqueObjects,
        List<Transform> objects)
    {
        // Identifier 
        ContainerModel container = new ContainerModel();
        container.identifier = identifier;

        // Spatial properties 
        switch (userDefinedProperties.type)
        {
            case Constants.Dimensionality.TwoDimensional:
                container.support2D = OptimizationParameters.container2DSupport2D;
                container.support3D = OptimizationParameters.container2DSupport3D;
                break;
            case Constants.Dimensionality.ThreeDimensional:
                container.support2D = OptimizationParameters.container3DSupport2D;
                container.support3D = OptimizationParameters.container3DSupport3D;
                break;
        }
        Vector3 bounds = containerTransform.Find("bounds").lossyScale;
        container.hSize = Mathf.Max(bounds.x, bounds.z);
        container.vSize = bounds.y;
        Vector3 relativePosition = containerTransform.position - user.position;
        container.x = relativePosition.x;
        container.y = relativePosition.y;
        container.z = relativePosition.z;
        container.dist = relativePosition.magnitude;
        container.fromForward = Vector3.Dot(user.forward, relativePosition.normalized);

        // Container Properties
        container.visibility = computeContainerVisibility(container.fromForward);
        container.touchSupport = computeContainerTouchSupport(container.dist);

        // Object associations 
        int uoNum = uniqueObjects.Count;
        int oNum = objects.Count;
        double[] associations = new double[uoNum];
        double associatedObjUtility = 0; 
        for (int oIdx = 0; oIdx < oNum; oIdx++)
        {
            int idx = uniqueObjects.IndexOf(objects[oIdx].name);
            double association = computeContainerObjectAssociation(containerTransform, objects[oIdx], user);
            double objUtility = normalize(objects[oIdx].gameObject.GetComponent<ObjectSettings>().utility);
            associatedObjUtility = Mathf.Max((float)(association * objUtility), (float)associatedObjUtility);
            associations[idx] = Mathf.Max((float)associations[idx], (float)association);
        }
        container.associatedObjUtility = associatedObjUtility;

        // Overriding values 
        // Utility 
        if (userDefinedProperties.overrideUtility)
        {
            container.utility = normalize(userDefinedProperties.utility);
            container.overrideUtility = true; 
        }

        // Object associations 
        ContainerObjectRelationship[] associationOverrides = userDefinedProperties.associations;
        int aNum = associationOverrides.Length;
        for (int aIdx = 0; aIdx < aNum; aIdx++)
        {
            ContainerObjectRelationship associationOverride = associationOverrides[aIdx];
            if (!uniqueObjects.Contains(associationOverride.environmentObject)) continue;
            int idx = uniqueObjects.IndexOf(associationOverride.environmentObject);
            associations[idx] = normalize(associationOverride.association);
        }
        container.objects = associations;

        return container;
    }

    public static List<Tuple<int, int, int, int, int, double>> defineConnectionBiases(List<ElementObjectDirectConnection> connections, List<ContainerModel[,,]> containers, List<Transform> objects, Vector2 voxelSize)
    {
        List<Tuple<int, int, int, int, int, double>> biases = new List<Tuple<int, int, int, int, int, double>>();

        int numContainers = containers.Count;
        foreach (ElementObjectDirectConnection connection in connections)
        {
            if (connection.userDefined)
            {
                Transform obj = objects[connection.obj];
                for (int cIdx = 0; cIdx < numContainers; cIdx++)
                {
                    int xVoxelCount = containers[cIdx].GetLength(0);
                    int yVoxelCount = containers[cIdx].GetLength(1);
                    int zVoxelCount = containers[cIdx].GetLength(2);
                    for (int xIdx = 0; xIdx < xVoxelCount; xIdx++)
                    {
                        for (int yIdx = 0; yIdx < yVoxelCount; yIdx++)
                        {
                            for (int zIdx = 0; zIdx < zVoxelCount; zIdx++)
                            {
                                ContainerModel container = containers[cIdx][xIdx, yIdx, zIdx];
                                double association = computeContainerObjectAssociation(new Vector3((float)container.x, (float)container.y, (float)container.z), obj, voxelSize);
                                if (association > OptimizationParameters.optimizationAnchoringThreshold)
                                {
                                    biases.Add(new Tuple<int, int, int, int, int, double>(connection.element, cIdx, xIdx, yIdx, zIdx, association));
                                }
                            }
                        }
                    }
                }
            }
        }

        return biases;
    }

    //public static List<int[]> defineOcclusions(List<Transform[,,]> containerTransforms, List<ContainerModel[,,]> containerModels, Transform user)
    public static List<int[]> defineOcclusions(List<Transform[,,]> containerTransforms, Transform user)
    {
        List<int[]> occlusions = new List<int[]>();

        Vector3 userPos = user.position;

        int numContainers = containerTransforms.Count;
        for (int cIdx = 0; cIdx < containerTransforms.Count; cIdx++)
        {
            int xVoxelCount = containerTransforms[cIdx].GetLength(0);
            int yVoxelCount = containerTransforms[cIdx].GetLength(1);
            int zVoxelCount = containerTransforms[cIdx].GetLength(2);
            for (int xIdx = 0; xIdx < xVoxelCount; xIdx++)
            {
                for (int yIdx = 0; yIdx < yVoxelCount; yIdx++)
                {
                    for (int zIdx = 0; zIdx < zVoxelCount; zIdx++)
                    {
                        Vector3 voxelPos = containerTransforms[cIdx][xIdx, yIdx, zIdx].position;
                        Vector3 toVoxel = (voxelPos - userPos).normalized;
                        float toVoxelDist = (voxelPos - userPos).magnitude;
                        RaycastHit[] hits;
                        hits = Physics.RaycastAll(userPos, toVoxel, Mathf.Infinity, 1 << 12);
                        foreach (RaycastHit hit in hits)
                        {
                            string[] hitVoxelData = hit.transform.name.Split('_');
                            int hitVoxelContainer = int.Parse(hitVoxelData[1]);
                            int hitVoxelX = int.Parse(hitVoxelData[2]);
                            int hitVoxelY = int.Parse(hitVoxelData[3]);
                            int hitVoxelZ = int.Parse(hitVoxelData[4]);
                            if ((hitVoxelContainer != cIdx) || 
                                (hitVoxelX != xIdx) || 
                                (hitVoxelY != yIdx) || 
                                (hitVoxelZ != zIdx)) {
                                if ((hit.point - userPos).magnitude > toVoxelDist)
                                {
                                    occlusions.Add(new int[] { cIdx, xIdx, yIdx, zIdx, hitVoxelContainer, hitVoxelX, hitVoxelY, hitVoxelZ });
                                }
                            }
                        }
                    }
                }
            }
        }

        return occlusions;
    }
}