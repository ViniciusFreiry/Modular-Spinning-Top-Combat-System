using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggablePawnPart : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public PawnPart partData;
    public PartType partType;
    public Image image;

    private Transform originalParent;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        image = GetComponent<Image>();
    }

    public void Initialize(PawnPart part, PartType partType)
    {
        if (part != null)
        {
            partData = part;
            this.partType = part.partType;
            image.sprite = part.sprite;
        }
        else
        {
            partData = null;
            this.partType = partType;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = GetComponent<RectTransform>().anchoredPosition;
        // originalParent = transform.parent;
        // transform.SetParent(canvas.transform);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        // transform.SetParent(originalParent);
        // transform.localPosition = Vector3.zero;
        GetComponent<RectTransform>().anchoredPosition = originalPosition;
    }
}