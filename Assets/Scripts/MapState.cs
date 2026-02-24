using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapData
{
    public List<MapNodeData> nodes = new();
}

[System.Serializable]
public class MapNodeData
{
    public int layerIndex;
    public int nodeIndex;
    public Vector2 position;
    public NodeType nodeType;
    public List<(int layer, int index)> connections = new();
}