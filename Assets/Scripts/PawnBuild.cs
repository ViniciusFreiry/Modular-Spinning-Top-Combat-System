using static MapProgressManager;
using UnityEngine;

public class PawnBuilder : MonoBehaviour
{
    public SpriteRenderer topSprite;
    public SpriteRenderer attackSprite;
    public SpriteRenderer defenseSprite;
    public SpriteRenderer baseSprite;

    public void BuildFromParts(PawnBuild build)
    {
        // Exemplo: mudar sprites e atributos dos componentes
        ApplyPart(build.topPart, PartType.Top);
        ApplyPart(build.attackPart, PartType.Attack);
        ApplyPart(build.defensePart, PartType.Defense);
        ApplyPart(build.basePart, PartType.Base);
    }

    private void ApplyPart(PawnPart part, PartType partType)
    {
        Pawn thisPawn = GetComponent<Pawn>();

        switch(partType)
        {
            case PartType.Top:
                topSprite.sprite = part.sprite;
                thisPawn.top = part;
                break;

            case PartType.Attack:
                attackSprite.sprite = part.sprite;
                thisPawn.attackRing = part;
                break;

            case PartType.Defense:
                defenseSprite.sprite = part.sprite;
                thisPawn.defenseRing = part;
                break;

            case PartType.Base:
                baseSprite.sprite = part.sprite;
                thisPawn.baseRing = part;
                break;
        }
    }
}