using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Game/Weapons/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public Sprite icon;
    public WeaponPartData offensePart;
    public WeaponPartData defensePart;
    public WeaponPartData movementPart;

    private IEnumerable<WeaponPartData> Parts
    {
        get
        {
            if (offensePart != null) yield return offensePart;
            if (defensePart != null) yield return defensePart;
            if (movementPart != null) yield return movementPart;
        }
    }

    public WeaponStats GetAggregatedStats()
    {
        var stats = new WeaponStats();
        foreach (var part in Parts)
        {
            stats = stats.Add(part.statContribution);
        }
        return stats;
    }

    public IEnumerable<WeaponPassiveEffect> GetPassives()
    {
        foreach (var part in Parts)
        {
            if (part.passive != null) yield return part.passive;
        }
    }
}
