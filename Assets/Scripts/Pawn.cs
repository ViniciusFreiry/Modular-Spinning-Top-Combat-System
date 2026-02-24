using UnityEngine;

public class Pawn : MonoBehaviour
{
    public PawnPart top;
    public PawnPart attackRing;
    public PawnPart defenseRing;
    public PawnPart baseRing;

    [SerializeField] private int totalAttack;
    [SerializeField] private int totalDefense;
    [SerializeField] private int totalStamina;
    [SerializeField] private int totalBalance;
    [SerializeField] private SpinOrientation spinOrientation;

    private PawnPhysics physics;

    void Start()
    {
        physics = GetComponent<PawnPhysics>();
        CalculateStats();

        if (physics != null)
        {
            physics.SetStatus(totalAttack, totalDefense, totalBalance, totalStamina);
        }
    }

    public void CalculateStats()
    {
        totalAttack = attackRing.attack + defenseRing.attack + baseRing.attack + top.attack;
        totalDefense = attackRing.defense + defenseRing.defense + baseRing.defense + top.defense;
        totalStamina = (attackRing.stamina + defenseRing.stamina + baseRing.stamina + top.stamina) * 30;
        totalBalance = attackRing.balance + defenseRing.balance + baseRing.balance + top.balance;
        spinOrientation = attackRing.spinOrientation;
    }

    public SpecialAbility GetSpecial()
    {
        return top.specialAbility;
    }

    public SpinOrientation GetSpinOrientation()
    {
        return spinOrientation;
    }

    public int GetSpinOrientationInt()
    {
        return (spinOrientation == SpinOrientation.Clockwise ? -1 : 1);
    }
}