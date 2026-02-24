using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewInventory", menuName = "Pawn Inventory")]
public class PawnInventory : ScriptableObject
{
    public List<PawnPart> availableParts;
}