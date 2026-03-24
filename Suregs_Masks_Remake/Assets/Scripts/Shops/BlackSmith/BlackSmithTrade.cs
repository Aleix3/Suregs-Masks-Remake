using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Item;

[System.Serializable]
public class BlackSmithTrade
{
    public ItemType potionResult;
    public int goldCost;               // Oro necesario
    public ItemType requiredItem;      // Material requerido, si aplica
    public int requiredItemQty;        // Cantidad del material
}
