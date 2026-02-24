using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public enum NodeType { NormalBattle, EliteBattle, Shop, Event, Boss }

public class MapNode : MonoBehaviour
{
    public NodeType nodeType;
    public Image icon;
    public MapGenerator mapGenerator;
    public List<MapNode> connections = new();

    public Sprite normalSprite;
    public Sprite eliteSprite;
    public Sprite shopSprite;
    public Sprite eventSprite;
    public Sprite bossSprite;

    public Color defaultColor = Color.white;
    public Color chosenColor = Color.green;
    public Color skippedColor = Color.gray;
    public Button button;

    public int layerIndex;
    public int nodeIndex;

    public void Initialize(NodeType type)
    {
        nodeType = type;
        icon.sprite = GetIconForType(type);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnNodeClicked);
    }

    private Sprite GetIconForType(NodeType type)
    {
        return type switch
        {
            NodeType.NormalBattle => normalSprite,
            NodeType.EliteBattle => eliteSprite,
            NodeType.Shop => shopSprite,
            NodeType.Event => eventSprite,
            NodeType.Boss => bossSprite,
            _ => null,
        };
    }

    private void OnNodeClicked()
    {
        if (!MapProgressManager.Instance.CanSelectNode(this))
        {
            Debug.Log("Este nó não pode ser selecionado no momento.");
            return;
        }

        MapProgressManager.Instance.AdvanceToNode(this);
        mapGenerator.UpdateNodeVisuals();

        switch (nodeType)
        {
            case NodeType.NormalBattle:
            case NodeType.EliteBattle:
            case NodeType.Boss:
                SceneManager.LoadScene("BattleScene");
                break;
            case NodeType.Shop:
                // abrir canvas
                break;
            case NodeType.Event:
                // abrir canvas
                break;
        }
    }
}