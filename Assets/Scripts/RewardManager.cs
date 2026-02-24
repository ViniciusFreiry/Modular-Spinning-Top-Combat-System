using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RewardManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject selectTypePanel;
    public GameObject selectPartPanel;

    [Header("Type Buttons")]
    public Button buttonTop;
    public Button buttonAttack;
    public Button buttonDefense;
    public Button buttonBase;

    [Header("Part Buttons")]
    public Button[] partButtons; // 3 buttons
    public Image[] partImages;

    [Header("Possible Parts")]
    public List<PawnPart> allParts;

    private PartType currentPartType;
    private System.Action<PawnPart> onPartChosenCallback;

    void Start()
    {
        allParts = MapProgressManager.Instance.allParts;

        // Atribui callbacks dos botões de tipo
        buttonTop.onClick.AddListener(() => OnPartTypeSelected(PartType.Top));
        buttonAttack.onClick.AddListener(() => OnPartTypeSelected(PartType.Attack));
        buttonDefense.onClick.AddListener(() => OnPartTypeSelected(PartType.Defense));
        buttonBase.onClick.AddListener(() => OnPartTypeSelected(PartType.Base));
    }

    public void Show(System.Action<PawnPart> onChosen, bool skipTypeSelection = false, PartType forcedType = PartType.Top)
    {
        this.onPartChosenCallback = onChosen;
        gameObject.SetActive(true);

        if (skipTypeSelection)
        {
            currentPartType = forcedType;
            ShowPartSelection();
        }
        else
        {
            selectTypePanel.SetActive(true);
            selectPartPanel.SetActive(false);
        }
    }

    private void OnPartTypeSelected(PartType type)
    {
        currentPartType = type;
        ShowPartSelection();
    }

    private void ShowPartSelection()
    {
        selectTypePanel.SetActive(false);
        selectPartPanel.SetActive(true);

        List<PawnPart> filteredParts = allParts.FindAll(p => p.partType == currentPartType);
        List<PawnPart> options = GetRandomParts(filteredParts, 3);

        for (int i = 0; i < options.Count; i++)
        {
            int index = i;
            partImages[i].sprite = options[i].sprite;
            partButtons[i].gameObject.SetActive(true);
            partButtons[i].onClick.RemoveAllListeners();
            partButtons[i].onClick.AddListener(() => OnPartChosen(options[index]));
        }
    }

    private void OnPartChosen(PawnPart part)
    {
        onPartChosenCallback?.Invoke(part);
        Destroy(gameObject);
    }

    private List<PawnPart> GetRandomParts(List<PawnPart> list, int count)
    {
        List<PawnPart> result = new List<PawnPart>();
        List<PawnPart> copy = new List<PawnPart>(list);

        for (int i = 0; i < count && copy.Count > 0; i++)
        {
            int r = Random.Range(0, copy.Count);
            result.Add(copy[r]);
            copy.RemoveAt(r);
        }

        return result;
    }
}