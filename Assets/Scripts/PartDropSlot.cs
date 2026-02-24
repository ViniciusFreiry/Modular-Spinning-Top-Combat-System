using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PartDropSlot : MonoBehaviour, IDropHandler
{
    public PartType slotType;
    public DraggablePawnPart currentPart;
    public PawnCustomizer customizer;

    public void OnDrop(PointerEventData eventData)
    {
        DraggablePawnPart dropped = eventData.pointerDrag.GetComponent<DraggablePawnPart>();

        if (dropped != null && dropped.partType == slotType)
        {
            // Troca entre os dois slots se for possível
            SwapParts(dropped);
        }
    }

    private void SwapParts(DraggablePawnPart dropped)
    {
        if (currentPart.partData != null)
        {
            switch (dropped.partData.partType)
            {
                case PartType.Top:
                    customizer.topParts.Remove(dropped.partData);
                    customizer.topParts.Add(currentPart.partData);
                    break;

                case PartType.Attack:
                    customizer.attackParts.Remove(dropped.partData);
                    customizer.attackParts.Add(currentPart.partData);
                    break;

                case PartType.Defense:
                    customizer.defenseParts.Remove(dropped.partData);
                    customizer.defenseParts.Add(currentPart.partData);
                    break;

                case PartType.Base:
                    customizer.baseParts.Remove(dropped.partData);
                    customizer.baseParts.Add(currentPart.partData);
                    break;
            }

            PawnPart tempPawn = dropped.partData;

            dropped.partData = currentPart.partData;
            currentPart.partData = tempPawn;

            dropped.GetComponent<Image>().sprite = dropped.partData.sprite;
            GetComponent<Image>().sprite = currentPart.partData.sprite;
        }
        else
        {
            currentPart.partData = dropped.partData;
            currentPart.GetComponent<Image>().sprite = currentPart.partData.sprite;

            switch(dropped.partData.partType)
            {
                case PartType.Top:
                    customizer.topParts.Remove(dropped.partData);
                    break;

                case PartType.Attack:
                    customizer.attackParts.Remove(dropped.partData);
                    break;

                case PartType.Defense:
                    customizer.defenseParts.Remove(dropped.partData);
                    break;

                case PartType.Base:
                    customizer.baseParts.Remove(dropped.partData);
                    break;
            }

            Destroy(dropped.gameObject);
        }

        customizer.slotsAtualization();
    }
}