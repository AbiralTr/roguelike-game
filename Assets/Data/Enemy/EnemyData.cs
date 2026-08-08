using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Health")]
    public int maxHealth = 30;

    [Header("Combat")]
    public int contactDamage = 10;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
}
