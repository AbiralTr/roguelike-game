using UnityEngine;

public class EnemyContact : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(enemyData.contactDamage);
            }
        }
    }
}