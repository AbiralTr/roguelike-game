using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Health")]
    public int maxHealth = 30;

    [Header("Combat")]
    public int contactDamage = 10;
    public AttackType attackType = AttackType.Melee;
    public int attackDamage = 10;
    public float attackRange = 4f;
    public float attackCooldown = 1.5f;

    [Header("Ranged Attack (only used when Attack Type is Ranged)")]
    public float projectileSpeed = 8f;
    public float projectileRange = 6f;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
}
