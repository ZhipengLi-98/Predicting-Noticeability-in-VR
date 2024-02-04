using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class WebSocketCommunicationV2 : MonoBehaviour
{
    public string m_ipaddr;
    public string m_port;
    private Uri m_uri;
    private ClientWebSocket m_ws;
    private List<Tuple<int, int[]>> m_assignments = new List<Tuple<int, int[]>>();

    public void webSocketConnect()
    {
        Task.Run(async () => {
            await connect();
        });
    }

    private async Task connect()
    {
        m_ws = null;
        m_uri = new Uri("ws://" + m_ipaddr + ":" + m_port + "/");

        try
        {
            Debug.Log(m_uri.ToString());
            m_ws = new ClientWebSocket();
            await m_ws.ConnectAsync(m_uri, CancellationToken.None);
            Debug.Log("ConnectAsync OK");
        }
        catch (Exception ex)
        {
            Debug.Log("Error: " + ex.ToString());
        }
    }

    private Task Send(ClientWebSocket ws, string data, CancellationToken cancellation)
    {
        byte[] encoded = Encoding.UTF8.GetBytes(data);
        ArraySegment<Byte> buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
        return ws.SendAsync(buffer, WebSocketMessageType.Text, true, cancellation);
    }

    public void runOptimization(List<ElementModel> elements, List<ContainerModel[,,]> containers, List<Tuple<int, int, int, int, int, double>> directConnectionBias, List<int[]> occlusions, out List<Tuple<int, int[]>> assignments)
    {
        Task t = Task.Run(async () => 
        {
            await Send(m_ws, parseParameters(), CancellationToken.None);
            await Send(m_ws, parseElements(elements), CancellationToken.None);
            await Send(m_ws, parseContainers(containers), CancellationToken.None);
            await Send(m_ws, parseDirectConnections(directConnectionBias), CancellationToken.None);
            await Send(m_ws, parseOcclusions(occlusions), CancellationToken.None);
            await Send(m_ws, "OPTIMIZE", CancellationToken.None);
            await GetOptimizationResult(m_ws);
        });
        t.Wait(-1, CancellationToken.None);
        assignments = m_assignments;
    }

    private string parseParameters()
    {
        string parameters = "PARAMS\n";
        parameters += OptimizationParameters.optimizationCompatibilityWeight + " ";
        parameters += OptimizationParameters.optimizationCompatibilityTypeWeight + " ";
        parameters += OptimizationParameters.optimizationCompatibilityVisibilityWeight + " ";
        parameters += OptimizationParameters.optimizationCompatibilityTouchWeight + " ";
        parameters += OptimizationParameters.optimizationCompatibilityBackgroundWeight + " ";
        parameters += OptimizationParameters.optimizationUtilityWeight + " ";
        parameters += OptimizationParameters.optimizationCompatibilityUtilityWeight + " ";
        parameters += OptimizationParameters.optimizationMaxUtilityWeight + " ";
        parameters += OptimizationParameters.optimizationAnchoringRewardWeight + " ";
        parameters += OptimizationParameters.optimizationAnchoringThreshold + " ";
        parameters += OptimizationParameters.optimizationAvoidanceCostWeight + " ";
        parameters += OptimizationParameters.optimizationAvoidanceThreshold + " ";
        parameters += OptimizationParameters.optimizationStructureWeight + " ";
        parameters += OptimizationParameters.optimizationUseHistory + " ";
        parameters += OptimizationParameters.optimizationUserConnectionWeight + " ";
        parameters += OptimizationParameters.optimizationOcclusionCostWeight;
        return parameters;
    }

    private string parseElement(ElementModel element)
    {
        string parsed = element.identifier + " ";
        parsed += element.x + " ";
        parsed += element.y + " ";
        parsed += element.z + " ";
        parsed += element.type2D + " ";
        parsed += element.type3D + " ";
        parsed += element.hVoxels + " ";
        parsed += element.vVoxels + " ";
        parsed += element.visReq + " ";
        parsed += element.touchReq + " ";
        parsed += element.backgroundTol + " ";
        parsed += element.utility + "\n";
        parsed += element.anchors.Length + " ";
        foreach (double anchor in element.anchors)
        {
            parsed += anchor + " ";
        }
        parsed += "\n";
        parsed += element.avoidances.Length + " ";
        foreach (double avoidance in element.avoidances)
        {
            parsed += avoidance + " ";
        }
        parsed += "\n";
        return parsed;
    }

    private string parseElements(List<ElementModel> elements) 
    {
        string parsed = "ELEMENTS\n";
        parsed += elements.Count + "\n";
        
        foreach (ElementModel element in elements)
        {
            parsed += parseElement(element);
        }
        
        return parsed; 
    }

    private string parseContainer(ContainerModel container)
    {
        string parsed = container.identifier + " ";
        parsed += container.x + " ";
        parsed += container.y + " ";
        parsed += container.z + " ";
        parsed += container.support2D + " ";
        parsed += container.support3D + " ";
        parsed += container.visibility + " ";
        parsed += container.touchSupport + " ";
        parsed += container.backgroundComplexity + " ";
        parsed += container.utility + " ";
        parsed += container.associatedObjUtility + "\n";
        parsed += container.objects.Length + " ";
        foreach (double obj in container.objects)
        {
            parsed += obj + " ";
        }
        parsed += "\n";
        return parsed; 
    }

    private string parseContainers(List<ContainerModel[,,]> containers)
    {
        string parsed = "CONTAINERS\n";
        parsed += containers.Count + "\n";
        foreach(ContainerModel[,,] container in containers)
        {
            int xVoxels = container.GetLength(0);
            int yVoxels = container.GetLength(1);
            int zVoxels = container.GetLength(2);
            parsed += xVoxels + " " + yVoxels + " " + zVoxels + "\n";
            for (int xIdx = 0; xIdx < xVoxels; xIdx++)
            {
                for (int yIdx = 0; yIdx < yVoxels; yIdx++)
                {
                    for (int zIdx = 0; zIdx < zVoxels; zIdx++)
                    {
                        parsed += parseContainer(container[xIdx, yIdx, zIdx]);
                    }
                }
            }
        }
        return parsed; 
    }

    private string parseDirectConnections(List<Tuple<int, int, int, int, int, double>> directConnectionBias)
    {
        string parsed = "BIASES\n";
        parsed += directConnectionBias.Count + "\n";
        foreach (Tuple<int, int, int, int, int, double> bias in directConnectionBias)
        {
            parsed += bias.Item1 + " " + bias.Item2 + " " + bias.Item3 + " " + bias.Item4 + " " + bias.Item5 + " " + bias.Item6 + "\n";
        }
        return parsed; 
    }

    private string parseOcclusions(List<int[]> occlusions)
    {
        string parsed = "OCCLUSIONS\n";
        parsed += occlusions.Count + "\n";
        foreach (int[] occlusion in occlusions)
        {
            string entry = occlusion[0] + " " + occlusion[1] + " " + occlusion[2] + " " + occlusion[3] + " " + occlusion[4] + " " + occlusion[5] + " " + occlusion[6] + " " + occlusion[7];
            parsed += entry + "\n";
        }

        return parsed; 
    }

    private void parseOptimizationResults(string results)
    {
        m_assignments.Clear();
        string[] data = results.Split('\n');
        int.TryParse(data[1], out int numAssignments);
        for (int aIdx = 2; aIdx < numAssignments + 2; aIdx++)
        {
            string[] entryData = data[aIdx].Split(' ');
            int.TryParse(entryData[0], out int element);
            int.TryParse(entryData[1], out int container);
            int.TryParse(entryData[2], out int xVoxel);
            int.TryParse(entryData[3], out int yVoxel);
            int.TryParse(entryData[4], out int zVoxel);
            m_assignments.Add(new Tuple<int, int[]>(element, new int[] { container, xVoxel, yVoxel, zVoxel }));
        }
    }

    private async Task GetOptimizationResult(ClientWebSocket webSocket)
    {
        byte[] buffer = new byte[512];
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        string optimizationResults = Encoding.UTF8.GetString(buffer);
        parseOptimizationResults(optimizationResults);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
