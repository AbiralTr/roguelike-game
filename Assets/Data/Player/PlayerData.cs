using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth = 100;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 3f;

    [Header("Melee Attack")]
    public int meleeDamage = 10;
    public float meleeRange = 0.75f;
    public float meleeCooldown = 0.4f;

    [Header("Projectile Attack")]
    public int projectileDamage = 5;
    public float projectileSpeed = 12f;
    public float projectileRange = 5f;
    public float projectileCooldown = 0.6f;

    public void Initialize()
    {
        currentHealth = maxHealth;
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
}