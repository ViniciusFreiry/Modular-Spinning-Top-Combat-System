using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.SceneManagement;

public class PawnCustomizer : MonoBehaviour {
    public List<PawnPart> topPartsActive = new List<PawnPart>(3);
    public List<PawnPart> attackPartsActive = new List<PawnPart>(3);
    public List<PawnPart> defensePartsActive = new List<PawnPart>(3);
    public List<PawnPart> basePartsActive = new List<PawnPart>(3);

    public List<PawnPart> topParts;
    public List<PawnPart> attackParts;
    public List<PawnPart> defenseParts;
    public List<PawnPart> baseParts;

    public RectTransform inventoryContainer;
    public RectTransform slotsTransform;
    public RectTransform statsPreview;
    public RectTransform confirmSetButton;

    public GameObject draggablePartPrefab;
    public Transform topInventoryContainer;
    public Transform attackInventoryContainer;
    public Transform defenseInventoryContainer;
    public Transform baseInventoryContainer;

    public List<Button> buttons;

    private List<Image> topImage = new List<Image>(3);
    private List<Image> attackImage = new List<Image>(3);
    private List<Image> defenseImage = new List<Image>(3);
    private List<Image> baseImage = new List<Image>(3);
    private List<Image> allImages;

    private bool usingSlot = false;
    private int slotsQtd = 3;
    private int slot = 0;
    private float slotLerp = 0.25f;
    private Vector2 slotOpenPosition;

    private void Start()
    {
        slotsQtd = MapProgressManager.Instance.activePawns.Count;

        topParts = MapProgressManager.Instance.unlockedParts.FindAll(p => p.partType == PartType.Top);
        attackParts = MapProgressManager.Instance.unlockedParts.FindAll(p => p.partType == PartType.Attack);
        defenseParts = MapProgressManager.Instance.unlockedParts.FindAll(p => p.partType == PartType.Defense);
        baseParts = MapProgressManager.Instance.unlockedParts.FindAll(p => p.partType == PartType.Base);

        for (int i = 0; i < slotsQtd; i++)
        {
            topPartsActive[i] = MapProgressManager.Instance.activePawns[i].topPart;
            attackPartsActive[i] = MapProgressManager.Instance.activePawns[i].attackPart;
            defensePartsActive[i] = MapProgressManager.Instance.activePawns[i].defensePart;
            basePartsActive[i] = MapProgressManager.Instance.activePawns[i].basePart;
        }

        PopulateInventoryUI();

        inventoryContainer.localScale = new Vector2(1, 0);
        statsPreview.localScale = new Vector2(0, 1);

        slotOpenPosition = new Vector2(slotsTransform.anchoredPosition.x, slotsTransform.anchoredPosition.y);
        slotsTransform.anchoredPosition = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (slotsTransform.gameObject.activeInHierarchy == true)
        {
            // Animação do slot abrindo

            
            RectTransform usingRect = buttons[slot].GetComponent<RectTransform>();
            RectTransform parentRect = usingRect.parent.GetComponent<RectTransform>();
            RectTransform topImageRect = topImage[slot].GetComponent<RectTransform>();
            RectTransform attackImageRect = attackImage[slot].GetComponent<RectTransform>();
            RectTransform defenseImageRect = defenseImage[slot].GetComponent<RectTransform>();
            RectTransform baseImageRect = baseImage[slot].GetComponent<RectTransform>();

            float imageSize = topImageRect.sizeDelta.x;

            if (usingSlot)
            {
                float containerSize = parentRect.parent.GetComponent<RectTransform>().sizeDelta.x;
                float imageGap = (containerSize - (imageSize * 4)) / 3;
                float lastImageX = (1.5f * imageSize) + (1.5f * imageGap);
                float penultImageX = (0.5f * imageSize) + (0.5f * imageGap);

                if (inventoryContainer.localScale.y < 0.9999f)
                {
                    LerpScaleY(inventoryContainer, 1, slotLerp);
                    LerpScaleX(statsPreview, 1, slotLerp);
                    LerpScaleX(confirmSetButton, 0, slotLerp);

                    LerpSizeX(usingRect, containerSize, slotLerp);

                    LerpPositionY(slotsTransform, slotOpenPosition.y, slotLerp);
                    LerpPositionX(slotsTransform, slotOpenPosition.x, slotLerp);
                    LerpPositionX(parentRect, 0, slotLerp);
                    LerpPositionX(topImageRect, -lastImageX, slotLerp);
                    LerpPositionX(attackImageRect, -penultImageX, slotLerp);
                    LerpPositionX(defenseImageRect, penultImageX, slotLerp);
                    LerpPositionX(baseImageRect, lastImageX, slotLerp);
                }
            }
            else
            {
                if (inventoryContainer.localScale.y > 0.0001f) {
                    float buttonPosition = 0;

                    switch (slot)
                    {
                        case 0: buttonPosition = 0;
                            break;

                        case 1: buttonPosition = -imageSize * 2;
                            break;

                        case 2: buttonPosition = imageSize * 2;
                            break;
                    }

                    LerpScaleY(inventoryContainer, 0, slotLerp);
                    LerpScaleX(statsPreview, 0, slotLerp);
                    LerpScaleX(confirmSetButton, 1, slotLerp);

                    LerpSizeX(usingRect, imageSize, slotLerp);

                    LerpPositionY(slotsTransform, 0, slotLerp);
                    LerpPositionX(slotsTransform, 0, slotLerp);
                    LerpPositionX(parentRect, buttonPosition, slotLerp);
                    LerpPositionX(topImageRect, 0, slotLerp);
                    LerpPositionX(attackImageRect, 0, slotLerp);
                    LerpPositionX(defenseImageRect, 0, slotLerp);
                    LerpPositionX(baseImageRect, 0, slotLerp);
                }
            }
        }
    }

    private void LerpPositionX(RectTransform rectTransform, float newPosition, float lerpValue) 
    {
        rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(rectTransform.anchoredPosition.x, newPosition, lerpValue), rectTransform.anchoredPosition.y);
    }

    private void LerpPositionY(RectTransform rectTransform, float newPosition, float lerpValue)
    {
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, Mathf.Lerp(rectTransform.anchoredPosition.y, newPosition, lerpValue));
    }

    private void LerpSizeX(RectTransform rectTransform, float newSize, float lerpValue)
    {
        rectTransform.sizeDelta = new Vector2(Mathf.Lerp(rectTransform.sizeDelta.x, newSize, lerpValue), rectTransform.sizeDelta.y);
    }

    private void LerpSizeY(RectTransform rectTransform, float newSize, float lerpValue)
    {
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, Mathf.Lerp(rectTransform.sizeDelta.y, newSize, lerpValue));
    }

    private void LerpScaleX(RectTransform rectTransform, float newScale, float lerpValue)
    {
        newScale = Mathf.Lerp(rectTransform.localScale.x, newScale, slotLerp);

        rectTransform.localScale = new Vector2(newScale, rectTransform.localScale.y);
    }

    private void LerpScaleY(RectTransform rectTransform, float newScale, float lerpValue)
    {
        newScale = Mathf.Lerp(rectTransform.localScale.y, newScale, slotLerp);

        rectTransform.localScale = new Vector2(rectTransform.localScale.x, newScale);
    }

    public void SelectSlot(int slot)
    {
        usingSlot = true;
        ActiveSlotDraggble(true);
        this.slot = slot;
        buttons[slot].GetComponent<RectTransform>().parent.transform.SetAsLastSibling();
    }

    private void PopulateInventoryUI()
    {
        CreateSlotUI(basePartsActive, buttons, PartType.Base);
        CreateSlotUI(defensePartsActive, buttons, PartType.Defense);
        CreateSlotUI(attackPartsActive, buttons, PartType.Attack);
        CreateSlotUI(topPartsActive, buttons, PartType.Top);

        allImages = topImage.Concat(attackImage).Concat(defenseImage).Concat(baseImage).ToList();

        CreateInventoryUI(topParts, topInventoryContainer);
        CreateInventoryUI(attackParts, attackInventoryContainer);
        CreateInventoryUI(defenseParts, defenseInventoryContainer);
        CreateInventoryUI(baseParts, baseInventoryContainer);

        ActiveSlotDraggble(false);
    }

    private void CreateInventoryUI(List<PawnPart> partList, Transform container)
    {
        foreach (PawnPart part in partList)
        {
            GameObject item = Instantiate(draggablePartPrefab, container);
            DraggablePawnPart draggable = item.GetComponent<DraggablePawnPart>();
            draggable.Initialize(part, part.partType);

            PartDropSlot dropSlot = item.GetComponent<PartDropSlot>();
            dropSlot.currentPart = draggable;
            dropSlot.slotType = part.partType;
            dropSlot.customizer = this;
        }
    }

    private void CreateSlotUI(List<PawnPart> partList, List<Button> container, PartType partType)
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject item = Instantiate(draggablePartPrefab, container[i].transform);
            DraggablePawnPart draggable = item.GetComponent<DraggablePawnPart>();
            draggable.Initialize(partList[i], partType);

            PartDropSlot dropSlot = item.GetComponent<PartDropSlot>();
            dropSlot.currentPart = draggable;
            dropSlot.slotType = partType;
            dropSlot.customizer = this;

            if (partList == topPartsActive)
            {
                topImage.Add(item.GetComponent<Image>());
            }
            else if (partList == attackPartsActive)
            {
                attackImage.Add(item.GetComponent<Image>());
            }
            else if (partList == defensePartsActive)
            {
                defenseImage.Add(item.GetComponent<Image>());
            }
            else if (partList == basePartsActive)
            {
                baseImage.Add(item.GetComponent<Image>());
            }
        }
    }

    private void ActiveSlotDraggble(bool activationState)
    {
        foreach (Image image in allImages)
        {
            image.GetComponent<DraggablePawnPart>().enabled = activationState;
            image.GetComponent<EventTrigger>().enabled = activationState;
        }
    }

    public void slotsAtualization()
    {
        for (int i = 0; i < topImage.Count; i++)
        {
            topPartsActive[i] = topImage[i].GetComponent<DraggablePawnPart>().partData;
            attackPartsActive[i] = attackImage[i].GetComponent<DraggablePawnPart>().partData;
            defensePartsActive[i] = defenseImage[i].GetComponent<DraggablePawnPart>().partData;
            basePartsActive[i] = baseImage[i].GetComponent<DraggablePawnPart>().partData;
        }
    }

    public void ConfirmPawnSetup()
    {
        usingSlot = false;
        ActiveSlotDraggble(false);

        MapProgressManager.Instance.unlockedParts = topParts.Concat(attackParts).Concat(defenseParts).Concat(baseParts).ToList();

        for (int i = 0; i < slotsQtd; i++)
        {
            MapProgressManager.Instance.activePawns[i].topPart = topPartsActive[i];
            MapProgressManager.Instance.activePawns[i].attackPart = attackPartsActive[i];
            MapProgressManager.Instance.activePawns[i].defensePart = defensePartsActive[i];
            MapProgressManager.Instance.activePawns[i].basePart = basePartsActive[i];
        }
    }

    public void GoToMap()
    {
        SceneManager.LoadScene("MapScene");
    }
}