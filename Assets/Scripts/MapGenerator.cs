using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("References")]
    public GameObject nodePrefab;
    public GameObject linePrefab;
    public Transform nodeContainer;

    [Header("Map Settings")]
    public int totalLayers = 10;
    public int maxNodesPerLayer = 5;
    public float xSpacing = 250f;
    public float ySpacing = 180f;

    private List<List<MapNode>> layers = new();
    private List<LineRenderer> connectionLines = new();

    void Start()
    {
        if (MapProgressManager.Instance.mapAlreadyGenerated())
        {
            ReconstructMap(MapProgressManager.Instance.GetMapData());
        }
        else
        {
            MapData mapData = GenerateMap();
            MapProgressManager.Instance.SetMapData(mapData);

            MapProgressManager.Instance.StartRewardChoice(true, PartType.Base);
            MapProgressManager.Instance.StartRewardChoice(true, PartType.Defense);
            MapProgressManager.Instance.StartRewardChoice(true, PartType.Attack);
            MapProgressManager.Instance.StartRewardChoice(true, PartType.Top);
        }
    }

    private MapData GenerateMap()
    {
        layers.Clear();
        MapData data = new();

        for (int layer = totalLayers - 1; layer >= 0; layer--)
        {
            int nodeCount;

            if (layer == totalLayers - 1) nodeCount = 1; // Camada inicial (topo)
            else if (layer == totalLayers - 2) nodeCount = Random.Range(2, 4); // Segunda camada
            else if (layer == 0) nodeCount = 1; // Camada final (base)
            else nodeCount = Random.Range(2, maxNodesPerLayer + 1); // Demais camadas

            List<MapNode> currentLayer = new();
            float layerWidth = (nodeCount - 1) * xSpacing;

            for (int i = 0; i < nodeCount; i++)
            {
                Vector2 pos = new(
                    (i * xSpacing) - (layerWidth / 2),
                    (totalLayers - 1 - layer) * ySpacing // de baixo pra cima
                );

                GameObject nodeGO = Instantiate(nodePrefab, nodeContainer);
                RectTransform rt = nodeGO.GetComponent<RectTransform>();
                rt.anchoredPosition = pos;

                var node = nodeGO.GetComponent<MapNode>();
                node.mapGenerator = this;
                node.layerIndex = layer;
                node.nodeIndex = i;
                node.Initialize(GetNodeTypeForLayer(layer));

                currentLayer.Add(node);

                // Armazena no MapData
                data.nodes.Add(new MapNodeData
                {
                    layerIndex = layer,
                    nodeIndex = i,
                    position = pos,
                    nodeType = node.nodeType,
                    connections = new() // Conexões virão depois
                });
            }

            layers.Add(currentLayer);
        }

        // Conectar os nós de cada camada aos nós da camada acima
        for (int layer = 0; layer < layers.Count - 1; layer++)
        {
            List<MapNode> current = layers[layer];
            List<MapNode> next = layers[layer + 1];

            // Marcar nós que já receberam conexões
            HashSet<MapNode> nodesWithIncoming = new();

            // Conexões principais: 1 para 1 com base nos índices
            for (int i = 0; i < current.Count; i++)
            {
                MapNode curr = current[i];

                // Alvo preferencial: mesmo índice ou o mais próximo
                int targetIndex = Mathf.Clamp(i, 0, next.Count - 1);
                MapNode target = next[targetIndex];

                curr.connections.Add(target);
                nodesWithIncoming.Add(target);
                DrawConnection(curr, target);

                // Armazenar conexões no MapData
                var currData = data.nodes.Find(n => n.layerIndex == curr.layerIndex && n.nodeIndex == curr.nodeIndex);
                currData.connections.Add((target.layerIndex, target.nodeIndex));

                // Conexão extra opcional (para dar mais caminhos)
                if (Random.value < 0.5f)
                {
                    int extraIndex = Mathf.Clamp(i + Random.Range(-1, 2), 0, next.Count - 1);
                    MapNode extra = next[extraIndex];
                    if (!curr.connections.Contains(extra))
                    {
                        curr.connections.Add(extra);
                        nodesWithIncoming.Add(extra);
                        DrawConnection(curr, extra);

                        // Armazenar conexões no MapData
                        var currDataExtra = data.nodes.Find(n => n.layerIndex == curr.layerIndex && n.nodeIndex == curr.nodeIndex);
                        currDataExtra.connections.Add((extra.layerIndex, extra.nodeIndex));
                    }
                }
            }

            // Garantir que nenhum nó na próxima camada fique sem entrada
            for (int i = 0; i < next.Count; i++)
            {
                MapNode target = next[i];
                if (!nodesWithIncoming.Contains(target))
                {
                    // Encontre o nó da camada anterior mais próximo em índice
                    int closestIndex = Mathf.Clamp(i, 0, current.Count - 1);
                    MapNode closest = current[closestIndex];

                    if (!closest.connections.Contains(target))
                    {
                        closest.connections.Add(target);
                        DrawConnection(closest, target);

                        // Armazenar conexões no MapData
                        var currData = data.nodes.Find(n => n.layerIndex == closest.layerIndex && n.nodeIndex == closest.nodeIndex);
                        currData.connections.Add((target.layerIndex, target.nodeIndex));
                    }
                }
            }
        }

        MapProgressManager.Instance.InitializeProgress(totalLayers);
        UpdateNodeVisuals();
        return data;
    }

    void ReconstructMap(MapData data)
    {
        layers.Clear();

        Dictionary<(int layer, int index), MapNode> nodeLookup = new();

        foreach (var nodeData in data.nodes)
        {
            // Cria visualmente o nó
            GameObject nodeGO = Instantiate(nodePrefab, nodeContainer);
            RectTransform rt = nodeGO.GetComponent<RectTransform>();
            rt.anchoredPosition = nodeData.position;

            var node = nodeGO.GetComponent<MapNode>();
            node.mapGenerator = this;
            node.layerIndex = nodeData.layerIndex;
            node.nodeIndex = nodeData.nodeIndex;
            node.Initialize(nodeData.nodeType);

            // Adiciona ao dicionário e à camada
            nodeLookup[(node.layerIndex, node.nodeIndex)] = node;

            while (layers.Count <= node.layerIndex)
                layers.Add(new List<MapNode>());
            layers[node.layerIndex].Add(node);
        }

        // Reconectar os nós com base nos dados salvos
        foreach (var nodeData in data.nodes)
        {
            var fromNode = nodeLookup[(nodeData.layerIndex, nodeData.nodeIndex)];
            foreach (var conn in nodeData.connections)
            {
                var toNode = nodeLookup[conn];
                fromNode.connections.Add(toNode);
                DrawConnection(fromNode, toNode);
            }
        }

        MapProgressManager.Instance.AttChosenPath(layers);
        UpdateNodeVisuals();
    }

    NodeType GetNodeTypeForLayer(int layer)
    {
        if (layer == totalLayers - 1) return NodeType.NormalBattle;
        if (layer == 0) return NodeType.Boss;

        int roll = Random.Range(0, 100);
        if (roll < 40) return NodeType.NormalBattle;
        if (roll < 65) return NodeType.EliteBattle;
        if (roll < 80) return NodeType.Shop;
        if (roll < 95) return NodeType.Event;
        return NodeType.EliteBattle;
    }

    public void UpdateNodeVisuals()
    {
        foreach (var layer in layers)
        {
            foreach (var node in layer)
            {
                var img = node.icon;

                if (MapProgressManager.Instance.IsNodeChosen(node))
                    img.color = node.chosenColor;
                else if (MapProgressManager.Instance.WasNodeAvailableButNotChosen(node))
                    img.color = node.skippedColor;
                else
                    img.color = node.defaultColor;

                node.button.interactable = MapProgressManager.Instance.CanSelectNode(node);
            }
        }
    }

    void DrawConnection(MapNode from, MapNode to)
    {
        GameObject lineObj = Instantiate(linePrefab, nodeContainer);
        LineRenderer line = lineObj.GetComponent<LineRenderer>();

        Vector3 start = from.GetComponent<RectTransform>().position;
        Vector3 end = to.GetComponent<RectTransform>().position;

        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        line.sortingOrder = -1; // para ficar por baixo dos ícones

        connectionLines.Add(line);
    }
}