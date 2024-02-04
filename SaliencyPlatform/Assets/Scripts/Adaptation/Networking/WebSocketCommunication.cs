using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class WebSocketCommunication : MonoBehaviour
{
    public string m_ipaddr;
    public string m_port;
    private Uri m_uri;
    private ClientWebSocket m_ws;
    private List<Tuple<int, int>> m_assignments = new List<Tuple<int, int>>();
    private List<Tuple<int, double>> m_scales = new List<Tuple<int, double>>();

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

    private string getOptimizationParameterString()
    {
        string parameters = "PARAMS\n";
        parameters += OptimizationParameters.optimizationCompatibilityWeight + " ";
        parameters += OptimizationParameters.optimizationCompatibilityUtilityWeight + " ";
        parameters += OptimizationParameters.optimizationCompatibilityAssociatedObjUtilityWeight + " ";
        parameters += OptimizationParameters.optimizationCompatibilityTypeWeight + " ";
        parameters += OptimizationParameters.optimizationCompatibilityVisibilityWeight + " ";
        parameters += OptimizationParameters.optimizationCompatibilityTouchWeight + " ";
        parameters += OptimizationParameters.optimizationCompatibilityBackgroundWeight + " ";
        parameters += OptimizationParameters.optimizationAnchoringRewardWeight + " ";
        parameters += OptimizationParameters.optimizationAnchoringThreshold + " ";
        parameters += OptimizationParameters.optimizationAvoidanceCostWeight + " ";
        parameters += OptimizationParameters.optimizationAvoidanceThreshold + " ";
        parameters += OptimizationParameters.optimizationUserConnectionWeight;
        return parameters;
    }

    private string parseElements(List<ElementModel> elements)
    {
        string elementData = "ELEMENTS\n";
        elementData += elements.Count + "\n";
        foreach(ElementModel element in elements)
        {
            elementData += "ELEMENT ";
            elementData += element.identifier + " ";
            elementData += element.type2D + " ";
            elementData += element.type3D + " ";
            elementData += element.utility + " ";
            elementData += element.visReq + " ";
            elementData += element.touchReq + " ";
            elementData += element.backgroundTol + " ";
            elementData += element.hSize + " ";
            elementData += element.vSize + " ";
            elementData += element.minScale + " ";
            elementData += element.maxScale + "\n";
            elementData += "ANCHORS " + element.anchors.Length + " ";
            foreach (double anchor in element.anchors)
            {
                elementData += anchor + " ";
            }
            elementData += "\n";
            elementData += "AVOIDANCES " + element.avoidances.Length + " ";
            foreach (double avoidance in element.avoidances)
            {
                elementData += avoidance + " ";
            }
            elementData += "\n";
        }
        return elementData;
    }

    private string parseContainers(List<ContainerModel> containers)
    {
        string containerData = "CONTAINERS\n";
        containerData += containers.Count + "\n";
        foreach (ContainerModel container in containers)
        {
            containerData += "CONTAINER ";
            containerData += container.identifier + " ";
            containerData += container.support2D + " ";
            containerData += container.support3D + " ";
            containerData += container.utility + " ";
            containerData += container.associatedObjUtility + " ";
            containerData += container.visibility + " ";
            containerData += container.touchSupport + " ";
            containerData += container.backgroundComplexity + " ";
            containerData += container.hSize + " ";
            containerData += container.vSize + "\n";
            containerData += "OBJECTS " + container.objects.Length + " ";
            foreach (double obj in container.objects)
            {
                containerData += obj + " ";
            }
            containerData += "\n";
        }
        return containerData;
    }

    private string parseSemanticConnections(List<Tuple<int, int>> connections)
    {
        int numConnections = connections.Count;

        string connectionData = "CONNECTIONS\n";
        connectionData += numConnections + "\n";
        foreach (Tuple<int, int> connection in connections)
        {
            connectionData += "CONNECTION " + connection.Item1 + " " + connection.Item2 + "\n";
        }
        return connectionData;
    }

    private void parseOptimizationResults(string results)
    {
        m_assignments.Clear();
        m_scales.Clear();
        string[] data = results.Split('\n');
        int.TryParse(data[1], out int numAssignments);
        for (int aIdx = 2; aIdx < numAssignments + 2; aIdx++)
        {
            string[] entryData = data[aIdx].Split(' ');
            int.TryParse(entryData[0], out int element);
            int.TryParse(entryData[1], out int assignment);
            double.TryParse(entryData[2], out double scale);
            m_assignments.Add(new Tuple<int, int>(element, assignment));
            m_scales.Add(new Tuple<int, double>(element, scale));
        }
    }

    private async Task GetOptimizationResult(ClientWebSocket webSocket)
    {
        byte[] buffer = new byte[256];
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        string optimizationResults = Encoding.UTF8.GetString(buffer);
        parseOptimizationResults(optimizationResults);
    }

    public void runOptimization(List<ElementModel> elements, List<ContainerModel> containers, List<Tuple<int, int>> userDefinedConnections, out List<Tuple<int, int>> assignments, out List<Tuple<int, double>> scales)
    {
        Task t = Task.Run(async () =>
        {
            await Send(m_ws, getOptimizationParameterString(), CancellationToken.None);
            await Send(m_ws, parseElements(elements), CancellationToken.None);
            await Send(m_ws, parseContainers(containers), CancellationToken.None);
            await Send(m_ws, parseSemanticConnections(userDefinedConnections), CancellationToken.None);
            await Send(m_ws, "OPTIMIZE", CancellationToken.None);
            await GetOptimizationResult(m_ws);
        });
        t.Wait(-1, CancellationToken.None);
        assignments = m_assignments;
        scales = m_scales;
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
