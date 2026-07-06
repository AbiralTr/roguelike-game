using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;

    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => enemyData.maxHealth;
    public bool IsDead => currentHealth <= 0;

    void Awake()
    {
        currentHealth = enemyData.maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (IsDead) Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
