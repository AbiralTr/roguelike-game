using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Base Health")]
    public int baseMaxHealth = 100;

    [Header("Movement")]
    public float baseMoveSpeed = 5f;
    public float jumpForce = 5f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 3f;

    [Header("Base Melee Attack")]
    public int baseMeleeDamage = 10;
    public float meleeRange = 0.75f;
    public float meleeCooldown = 0.4f;

    [Header("Base Projectile Attack")]
    public int baseProjectileDamage = 5;
    public float projectileSpeed = 12f;
    public float projectileRange = 5f;
    public float projectileCooldown = 0.6f;
    [System.NonSerialized] public int maxHealth;
    [System.NonSerialized] public int currentHealth;
    [System.NonSerialized] public float moveSpeed;
    [System.NonSerialized] public int meleeDamage;
    [System.NonSerialized] public int projectileDamage;

    public void Initialize()
    {
        maxHealth = baseMaxHealth;
        currentHealth = baseMaxHealth;
        moveSpeed = baseMoveSpeed;
        meleeDamage = baseMeleeDamage;
        projectileDamage = baseProjectileDamage;
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(0, currentHealth - amount);
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    public bool IsDead => currentHealth <= 0;

    public void ApplyStatBoost(StatType type, float amount)
    {
        switch (type)
        {
            case StatType.Health:
                maxHealth += Mathf.RoundToInt(amount);
                currentHealth += Mathf.RoundToInt(amount);
                break;
            case StatType.Damage:
                meleeDamage += Mathf.RoundToInt(amount);
                projectileDamage += Mathf.RoundToInt(amount);
                break;
            case StatType.Speed:
                moveSpeed += amount;
                break;
        }
    }
}
