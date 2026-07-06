using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Melee Visual")]
    [SerializeField] private GameObject meleeVisual;
    [SerializeField] private float meleeVisualDuration = 0.1f;

    private float attackPointBaseX;
    private float meleeCooldownTimer;
    private float projectileCooldownTimer;
    private float meleeVisualTimer;

    void Awake()
    {
        attackPointBaseX = Mathf.Abs(attackPoint.localPosition.x);
    }

    void Update()
    {
        var pos = attackPoint.localPosition;
        pos.x = attackPointBaseX * Mathf.Sign(playerMovement.FacingDirection);
        attackPoint.localPosition = pos;

        if (meleeCooldownTimer > 0f) meleeCooldownTimer -= Time.deltaTime;
        if (projectileCooldownTimer > 0f) projectileCooldownTimer -= Time.deltaTime;

        if (meleeVisualTimer > 0f)
        {
            meleeVisualTimer -= Time.deltaTime;
            if (meleeVisualTimer <= 0f) meleeVisual.SetActive(false);
        }

        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame && meleeCooldownTimer <= 0f)
        {
            MeleeAttack();
            meleeCooldownTimer = playerData.meleeCooldown;
        }

        if (mouse.rightButton.wasPressedThisFrame && projectileCooldownTimer <= 0f)
        {
            ProjectileAttack();
            projectileCooldownTimer = playerData.projectileCooldown;
        }
    }

    private void MeleeAttack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, playerData.meleeRange, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            EnemyHealth health = hit.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(playerData.meleeDamage);
            }
        }

        if (meleeVisual != null)
        {
            meleeVisual.SetActive(true);
            meleeVisualTimer = meleeVisualDuration;
        }
    }

    private void ProjectileAttack()
    {
        if (projectilePrefab == null) return;

        Vector2 direction = new Vector2(Mathf.Sign(playerMovement.FacingDirection), 0f);
        GameObject instance = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);
        instance.GetComponent<Projectile>().Init(direction, playerData.projectileDamage, playerData.projectileSpeed, playerData.projectileRange);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null || playerData == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, playerData.meleeRange);
    }
}
