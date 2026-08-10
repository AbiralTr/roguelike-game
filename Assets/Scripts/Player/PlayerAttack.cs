using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private WeaponData equippedWeapon;
    [SerializeField] private SpriteRenderer weaponSpriteRenderer;
    [SerializeField] private GameObject weaponPickupPrefab;

    [Header("Melee Visual")]
    [SerializeField] private GameObject meleeVisual;
    [SerializeField] private float meleeVisualDuration = 0.1f;

    private float attackPointBaseX;
    private float meleeCooldownTimer;
    private float projectileCooldownTimer;
    private float meleeVisualTimer;
    private Animator meleeVisualAnimator;
    private SpriteRenderer meleeVisualSpriteRenderer;

    void Awake()
    {
        attackPointBaseX = Mathf.Abs(attackPoint.localPosition.x);
        if (meleeVisual != null)
        {
            meleeVisualAnimator = meleeVisual.GetComponent<Animator>();
            meleeVisualSpriteRenderer = meleeVisual.GetComponent<SpriteRenderer>();
        }
        if (weaponSpriteRenderer != null) weaponSpriteRenderer.sprite = equippedWeapon != null ? equippedWeapon.icon : null;
    }

    public string EquippedWeaponName => equippedWeapon != null ? equippedWeapon.weaponName : "None";

    public void Equip(WeaponData newWeapon, Vector3 dropPosition)
    {
        if (equippedWeapon != null && weaponPickupPrefab != null)
        {
            GameObject dropped = Instantiate(weaponPickupPrefab, dropPosition, Quaternion.identity);
            dropped.GetComponent<WeaponPickup>().Init(equippedWeapon);
        }

        equippedWeapon = newWeapon;
        if (weaponSpriteRenderer != null) weaponSpriteRenderer.sprite = newWeapon != null ? newWeapon.icon : null;
    }


    void Update()
    {
        var pos = attackPoint.localPosition;
        pos.x = attackPointBaseX * Mathf.Sign(playerMovement.FacingDirection);
        attackPoint.localPosition = pos;
        if (weaponSpriteRenderer != null) weaponSpriteRenderer.flipX = playerMovement.FacingDirection < 0f;

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
            WeaponStats stats = equippedWeapon != null ? equippedWeapon.GetAggregatedStats() : new WeaponStats();
            MeleeAttack(stats);
            meleeCooldownTimer = playerData.meleeCooldown / (1f + stats.attackSpeedBonus);
        }

        if (mouse.rightButton.wasPressedThisFrame && projectileCooldownTimer <= 0f)
        {
            ProjectileAttack();
            projectileCooldownTimer = playerData.projectileCooldown;
        }
    }

    private void MeleeAttack(WeaponStats weaponStats)
    {
        int damage = playerData.meleeDamage + weaponStats.damage;
        if (Random.value < weaponStats.critChance) damage *= 2;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, playerData.meleeRange, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            EnemyHealth health = hit.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);

                if (equippedWeapon != null)
                {
                    foreach (WeaponPassiveEffect passive in equippedWeapon.GetPassives())
                    {
                        passive.OnHit(gameObject, hit.gameObject);
                    }
                }
            }
        }

        if (meleeVisual != null)
        {
            float facing = Mathf.Sign(playerMovement.FacingDirection);

            var visualPos = meleeVisual.transform.localPosition;
            visualPos.x = attackPointBaseX * facing;
            meleeVisual.transform.localPosition = visualPos;

            if (meleeVisualSpriteRenderer != null) meleeVisualSpriteRenderer.flipX = facing < 0f;

            meleeVisual.SetActive(true);
            meleeVisualTimer = meleeVisualDuration;
            if (meleeVisualAnimator != null) meleeVisualAnimator.Play(0, 0, 0f);
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
