using UnityEngine;

[CreateAssetMenu(fileName = "WeaponPartData", menuName = "Game/Weapons/Weapon Part")]
public class WeaponPartData : ScriptableObject
{
    public string partName;
    public PartSlot slot;
    public WeaponStats statContribution;
    public WeaponPassiveEffect passive;
}
