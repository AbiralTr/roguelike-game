using UnityEngine;

[CreateAssetMenu(fileName = "StatPickupData", menuName = "Game/Stat Pickup")]
public class StatPickupData : ScriptableObject
{
    public StatType statType;
    public float amount = 10f;
}
