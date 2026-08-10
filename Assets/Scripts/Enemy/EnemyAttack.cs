using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Melee Visual")]
    [SerializeField] private GameObject meleeVisual;
    [SerializeField] private float meleeVisualDuration = 0.1f;

    private Transform player;
    private float cooldownTimer;
    private float meleeVisualTimer;
    private float meleeVisualBaseX;

    void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (meleeVisual != null) meleeVisualBaseX = Mathf.Abs(meleeVisual.transform.localPosition.x);
    }

    void Update()
    {
        if (player == null) return;

        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        if (meleeVisual != null)
        {
            var pos = meleeVisual.transform.localPosition;
            pos.x = meleeVisualBaseX * Mathf.Sign(player.position.x - transform.position.x);
            meleeVisual.transform.localPosition = pos;

            if (meleeVisualTimer > 0f)
            {
                meleeVisualTimer -= Time.deltaTime;
                if (meleeVisualTimer <= 0f) meleeVisual.SetActive(false);
            }
        }

        float distance = Mathf.Abs(player.position.x - transform.position.x);
        if (distance > enemyData.attackRange || cooldownTimer > 0f) return;

        if (enemyData.attackType == AttackType.Ranged) Fire();
        else MeleeAttack();

        cooldownTimer = enemyData.attackCooldown;
    }

    private void MeleeAttack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, enemyData.attackRange);
        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            PlayerHealth health = hit.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(enemyData.attackDamage, transform.position);
            }
        }

        if (meleeVisual != null)
        {
            meleeVisual.SetActive(true);
            meleeVisualTimer = meleeVisualDuration;
        }
    }

    private void Fire()
    {
        if (projectilePrefab == null) return;

        Vector2 direction = new Vector2(Mathf.Sign(player.position.x - transform.position.x), 0f);
        GameObject instance = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        instance.GetComponent<EnemyProjectile>().Init(direction, enemyData.attackDamage, enemyData.projectileSpeed, enemyData.projectileRange);
    }

    void OnDrawGizmosSelected()
    {
        if (enemyData == null) return;
        Gizmos.color = enemyData.attackType == AttackType.Ranged ? Color.magenta : Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);
    }
}
