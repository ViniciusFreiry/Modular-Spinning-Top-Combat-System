using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapProgressManager : MonoBehaviour
{
    public static MapProgressManager Instance;

    public GameObject rewardCanvas;
    public List<MapNode> chosenPath = new();
    public List<(int layer, int index)> chosenPathIndex = new();
    public int currentLayer; // Será setado como totalLayers - 1 no início
    private MapData savedMapData = null;

    [System.Serializable]
    public class PawnBuild
    {
        public PawnPart topPart;
        public PawnPart attackPart;
        public PawnPart defensePart;
        public PawnPart basePart;
    }

    // Lista com todas as peças desbloqueadas pelo jogador
    public List<PawnPart> unlockedParts = new();

    // Lista com até 3 builds de peões
    public List<PawnBuild> activePawns = new List<PawnBuild>(3);

    // Todas as PawnPart
    public List<PawnPart> allParts = new List<PawnPart>();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            allParts = Resources.LoadAll<PawnPart>("PawnParts").ToList();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeProgress(int totalLayers)
    {
        currentLayer = totalLayers - 1; // Começa da camada inferior (visual) do mapa
        chosenPath.Clear();
    }

    public void AdvanceToNode(MapNode node)
    {
        chosenPath.Add(node);
        chosenPathIndex.Add((node.layerIndex, node.nodeIndex));
        currentLayer--; // Vai para a camada acima (mais próxima do topo)
    }

    public MapNode GetLastChosenNode()
    {
        return chosenPath.Count == 0 ? null : chosenPath[0];
    }

    public bool CanSelectNode(MapNode node)
    {
        if (node.layerIndex != currentLayer) return false;

        if (chosenPath.Count == 0) return true; // Primeiro nó sempre pode ser escolhido

        var previous = GetLastChosenNode();
        return previous != null && previous.connections.Contains(node);
    }

    public bool IsNodeChosen(MapNode node) => chosenPath.Contains(node);

    public bool WasNodeAvailableButNotChosen(MapNode node)
    {
        if (node.layerIndex <= currentLayer) return false;
        var previous = chosenPath.Find(n => n.layerIndex == node.layerIndex + 1);
        return previous != null && previous.connections.Contains(node) && !chosenPath.Contains(node);
    }

    public bool mapAlreadyGenerated() { 
        return savedMapData != null;
    }

    public void SetMapData(MapData data)
    {
        savedMapData = data;
    }

    public MapData GetMapData() { 
        return savedMapData; 
    }

    public void AttChosenPath(List<List<MapNode>> layers)
    {
        chosenPath.Clear();

        foreach (List<MapNode> row in layers)
        {
            foreach (MapNode node in row)
            {
                foreach ((int layer, int index) index in chosenPathIndex)
                {
                    if (index.layer == node.layerIndex && index.index == node.nodeIndex)
                    {
                        chosenPath.Add(node);
                    }
                }
            }
        }
    }

    public void StartRewardChoice(bool buildFirtsPawn = false, PartType forcedType = PartType.Top)
    {
        GameObject canvas = Instantiate(rewardCanvas);
        RewardManager rewardManager = canvas.GetComponent<RewardManager>();

        if (buildFirtsPawn)
        {
            switch (forcedType)
            {
                case PartType.Top:
                    rewardManager.Show((PawnPart selectedPart) => activePawns[0].topPart = selectedPart, buildFirtsPawn, forcedType);
                    break;

                case PartType.Attack:
                    rewardManager.Show((PawnPart selectedPart) => activePawns[0].attackPart = selectedPart, buildFirtsPawn, forcedType);
                    break;

                case PartType.Defense:
                    rewardManager.Show((PawnPart selectedPart) => activePawns[0].defensePart = selectedPart, buildFirtsPawn, forcedType);
                    break;

                case PartType.Base:
                    rewardManager.Show((PawnPart selectedPart) => activePawns[0].basePart = selectedPart, buildFirtsPawn, forcedType);
                    break;
            }
        }
        else
        {
            rewardManager.Show((PawnPart selectedPart) => unlockedParts.Add(selectedPart));
        }
    }
}