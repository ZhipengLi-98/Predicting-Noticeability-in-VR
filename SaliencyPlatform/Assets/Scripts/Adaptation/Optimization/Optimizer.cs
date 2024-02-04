using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gurobi;
using System;

public class Optimizer
{
    private List<ElementModel> _elements;
    private List<ContainerModel> _containers;
    private int _numElements;
    private int _numContainers;
    private List<Tuple<int, int>> _assignments = new List<Tuple<int, int>>();
    private List<Tuple<int, double>> _scales = new List<Tuple<int, double>>();

    public Optimizer(List<ElementModel> elements, List<ContainerModel> containers)
    {
        _elements = elements;
        _containers = containers;
        _numElements = _elements.Count;
        _numContainers = _containers.Count;
    }

    private double calcCompatibility(ElementModel element, ContainerModel container)
    {
        double typeCompatibility2D = (element.type2D * container.support2D);
        double typeCompatibility3D = (element.type3D * container.support3D);
        double utilityCompatibility = element.utility * container.utility;
        double visibilityCompatibility = element.visReq * container.visibility;
        double touchCompatibility = element.touchReq * container.touchSupport;
        double backgroundCompatibility = 1.0 - ((1.0 - element.backgroundTol) * container.backgroundComplexity);
        double compatibility =
            (OptimizationParameters.optimizationCompatibilityTypeWeight * typeCompatibility2D) +
            (OptimizationParameters.optimizationCompatibilityTypeWeight * typeCompatibility3D) +
            (OptimizationParameters.optimizationCompatibilityUtilityWeight * utilityCompatibility) +
            (OptimizationParameters.optimizationCompatibilityVisibilityWeight * visibilityCompatibility) +
            (OptimizationParameters.optimizationCompatibilityTouchWeight * touchCompatibility) +
            (OptimizationParameters.optimizationCompatibilityBackgroundWeight * backgroundCompatibility);
        compatibility /= (
            OptimizationParameters.optimizationCompatibilityTypeWeight +
            OptimizationParameters.optimizationCompatibilityTypeWeight +
            OptimizationParameters.optimizationCompatibilityUtilityWeight +
            OptimizationParameters.optimizationCompatibilityVisibilityWeight +
            OptimizationParameters.optimizationCompatibilityTouchWeight +
            OptimizationParameters.optimizationCompatibilityBackgroundWeight
            );
        return compatibility;
    }

    private double calcAnchoringReward(ElementModel element, ContainerModel container)
    {
        double reward = 0.0;
        double[] elementAnchors = element.anchors;
        double[] containerObjs = container.objects;
        int numObjs = containerObjs.Length;
        for (int oIdx = 0; oIdx < numObjs; oIdx++)
        {
            reward = Mathf.Max((float)reward, (float)(elementAnchors[oIdx] * containerObjs[oIdx]));
        }
        if (reward < OptimizationParameters.optimizationAnchoringThreshold) reward = 0;
        return reward;
    }

    private double calcAvoidanceCost(ElementModel element, ContainerModel container)
    {
        double cost = 0.0;
        double[] elementAvoidances = element.avoidances;
        double[] containerObjs = container.objects;
        int numObjs = containerObjs.Length;
        for (int oIdx = 0; oIdx < numObjs; oIdx++)
        {
            cost = Mathf.Max((float)cost, (float)(elementAvoidances[oIdx] * containerObjs[oIdx]));
        }
        if (cost < OptimizationParameters.optimizationAvoidanceThreshold) cost = 0;
        return cost;
    }

    public void optimize()
    {
        GRBEnv env = new GRBEnv();
        GRBModel model = new GRBModel(env);

        // Decision variables
        GRBVar[,] assignments = new GRBVar[_numElements, _numContainers];
        GRBVar[,] scales = new GRBVar[_numElements, _numContainers];
        for (int eIdx = 0; eIdx < _numElements; eIdx++)
        {
            int eId = _elements[eIdx].identifier;
            for (int cIdx = 0; cIdx < _numContainers; cIdx++)
            {
                int cId = _containers[cIdx].identifier;
                GRBVar assignment = model.AddVar(0, 1, 0, GRB.BINARY,
                    "x_" + eId + "_" + cId);
                assignments[eIdx, cIdx] = assignment;
                GRBVar scale = model.AddVar(_elements[eIdx].minScale, _elements[eIdx].maxScale, 0, GRB.SEMIINT,
                    "s_" + eId + "_" + cId);
                scales[eIdx, cIdx] = scale;
            }
        }

        GRBLinExpr lhs = 0.0;
        for (int eIdx = 0; eIdx < _numElements; eIdx++)
        {
            int eId = _elements[eIdx].identifier;

            // Constraints: Each element assigned once 
            lhs = 0.0;
            for (int cIdx = 0; cIdx < _numContainers; cIdx++)
            {
                lhs.AddTerm(1.0, assignments[eIdx, cIdx]);
            }
            model.AddConstr(lhs, GRB.EQUAL, 1.0,
                "constr_" + eId + "assignment");


            for (int cIdx = 0; cIdx < _numContainers; cIdx++)
            {
                int cId = _containers[cIdx].identifier;

                // Constraints: Zero scale when assignment is zero 
                model.AddGenConstrIndicator(assignments[eIdx, cIdx], 0, scales[eIdx, cIdx] == 0, "constr_" + eId + "_" + cId + "_zeroScale");

                // Constraints: scale within possible min/max
                model.AddGenConstrIndicator(assignments[eIdx, cIdx], 1, scales[eIdx, cIdx] >= _elements[eIdx].minScale, "constr_" + eId + "_" + cId + "_minScale");
                model.AddGenConstrIndicator(assignments[eIdx, cIdx], 1, scales[eIdx, cIdx] <= _elements[eIdx].maxScale, "constr_" + eId + "_" + cId + "_maxScale");
            }
        }

        // Constraints: Size
        for (int cIdx = 0; cIdx < _numContainers; cIdx++)
        {
            int cId = _containers[cIdx].identifier;

            // Individual size constraints 
            for (int eIdx = 0; eIdx < _numElements; eIdx++)
            {
                if (((_elements[eIdx].minScale * _elements[eIdx].hSize) > _containers[cIdx].hSize) ||
                    ((_elements[eIdx].minScale * _elements[eIdx].vSize) > _containers[cIdx].vSize))
                {
                    int eId = _elements[eIdx].identifier;
                    model.AddConstr(assignments[eIdx, cIdx], GRB.EQUAL, 0,
                        "constr_e_" + eId + "_c_" + cId + "_size");
                }
            }

            // Sum of horizontal sizes 
            lhs = 0.0;
            for (int eIdx = 0; eIdx < _numElements; eIdx++)
            {
                lhs.AddTerm(_elements[eIdx].hSize, scales[eIdx, cIdx]);
            }
            model.AddConstr(lhs, GRB.LESS_EQUAL, _containers[cIdx].hSize,
                "constr_" + cId + "_hSize");

            // Sum of vertical sizes 
            lhs = 0.0;
            for (int eIdx = 0; eIdx < _numElements; eIdx++)
            {
                lhs.AddTerm(_elements[eIdx].vSize, scales[eIdx, cIdx]);
            }
            model.AddConstr(lhs, GRB.LESS_EQUAL, _containers[cIdx].vSize,
                "constr_" + cId + "_vSize");

        }

        // Constrants: Type mismatch 
        for (int cIdx = 0; cIdx < _numContainers; cIdx++)
        {
            int cId = _containers[cIdx].identifier;
            for (int eIdx = 0; eIdx < _numElements; eIdx++)
            {
                if ((_containers[cIdx].support3D == 0) &&
                    (_elements[eIdx].type3D > 0))
                {
                    int eId = _elements[eIdx].identifier;
                    model.AddConstr(assignments[eIdx, cIdx], GRB.EQUAL, 0,
                        "constr_e_" + eId + "_c_" + cId + "_type");
                }
            }
        }

        // Objectives 
        GRBLinExpr compatibilityTerm = 0.0;
        GRBLinExpr anchorBehaviorTerm = 0.0;
        GRBLinExpr avoidBehaviorTerm = 0.0;

        // Compatibility
        for (int eIdx = 0; eIdx < _numElements; eIdx++)
        {
            for (int cIdx = 0; cIdx < _numContainers; cIdx++)
            {
                compatibilityTerm.AddTerm(calcCompatibility(_elements[eIdx], _containers[cIdx]), scales[eIdx, cIdx]);
            }
        }

        // Anchoring behavior
        for (int eIdx = 0; eIdx < _numElements; eIdx++)
        {
            for (int cIdx = 0; cIdx < _numContainers; cIdx++)
            {
                anchorBehaviorTerm.AddTerm(calcAnchoringReward(_elements[eIdx], _containers[cIdx]), assignments[eIdx, cIdx]);
            }
        }

        // Avoidance behavior
        for (int eIdx = 0; eIdx < _numElements; eIdx++)
        {
            for (int cIdx = 0; cIdx < _numContainers; cIdx++)
            {
                avoidBehaviorTerm.AddTerm(calcAvoidanceCost(_elements[eIdx], _containers[cIdx]), assignments[eIdx, cIdx]);
            }
        }

        // Register objective function terms 
        model.SetObjectiveN(compatibilityTerm, 0, 0, OptimizationParameters.optimizationCompatibilityWeight, 0, 0, "objective_compatibility");
        model.SetObjectiveN(anchorBehaviorTerm, 1, 0, OptimizationParameters.optimizationAnchoringRewardWeight, 0, 0, "objective_anchoring");
        model.SetObjectiveN(avoidBehaviorTerm, 2, 0, -OptimizationParameters.optimizationAvoidanceCostWeight, 0, 0, "objective_avoidance");

        model.ModelSense = GRB.MAXIMIZE;
        model.Update();
        model.Optimize();

        int status = model.Status;
        bool modelFeasible = !(status == GRB.Status.INF_OR_UNBD || status == GRB.Status.INFEASIBLE || status == GRB.Status.UNBOUNDED);
        if (modelFeasible)
        {
            Debug.Log("Model Feasible");
            // Store results 
            _assignments.Clear();
            _scales.Clear();
            for (int eIdx = 0; eIdx < _numElements; eIdx++)
            {
                int eId = _elements[eIdx].identifier;
                for (int cIdx = 0; cIdx < _numContainers; cIdx++)
                {
                    if (assignments[eIdx, cIdx].X > 0)
                    {
                        int cId = _containers[cIdx].identifier;
                        _assignments.Add(new Tuple<int, int>(eId, cId));
                        _scales.Add(new Tuple<int, double>(eId, scales[eIdx, cIdx].X));
                        break;
                    }
                }
            }

            // Print objective 
            //Debug.Log("Solutions: " + model.SolCount);
            model.Parameters.ObjNumber = 0;
            Debug.Log("Compatibility Term: " + model.ObjNVal);
            model.Parameters.ObjNumber = 1;
            Debug.Log("Anchoring Term: " + model.ObjNVal);
            model.Parameters.ObjNumber = 2;
            Debug.Log("Avoidance Term: " + model.ObjNVal);
        }
        else
        {
            Debug.Log("Model Infeasible");
        }
        model.Dispose();
        env.Dispose();
    }

    public void getResults(out List<Tuple<int, int>> assignments, out List<Tuple<int, double>> scales)
    {
        assignments = _assignments;
        scales = _scales;
    }
}