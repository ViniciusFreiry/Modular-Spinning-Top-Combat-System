using UnityEngine;

[CreateAssetMenu(fileName = "NewPart", menuName = "Pawn Part")]
public class PawnPart : ScriptableObject
{
    public string partName;
    public Sprite sprite;
    public int attack;
    public int defense;
    public int stamina;
    public int balance;

    public SpecialAbility specialAbility; // Apenas relevante para Top Layer
    public PartType partType; // Enum: Top, Attack, Defense, Base
    public SpinOrientation spinOrientation; // Apenas relevante para Attack Layer
    public Rarity rarity; // Raridade da peça
}

public enum PartType { Top, Attack, Defense, Base }

public enum SpecialAbility
{
    None,
    ExplosionSpin,
    ShieldBurst,
    StaminaDrain,
    MagneticPull
}

public enum SpinOrientation { Clockwise, Anticlockwise, None }

public enum Rarity { Common, Rare, Epic, Legendary }