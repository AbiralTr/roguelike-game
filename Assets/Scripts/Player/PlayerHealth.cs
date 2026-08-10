using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private float iframeDuration = 1.5f;
    [SerializeField] private float flashInterval = 0.1f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float knockbackDuration = 0.15f;

    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private DeathScreenManager deathScreenManager;
    private int playerLayer;

    private float iframeTimer;
    private SpriteRenderer spriteRenderer;
    private bool isInvincible => iframeTimer > 0f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerLayer = gameObject.layer;
    }

    void Update()
    {
        if (iframeTimer > 0f)
        {
            iframeTimer -= Time.deltaTime;
            spriteRenderer.enabled = Mathf.Sin(iframeTimer / flashInterval) > 0;

            if (iframeTimer <= 0f)
            {
                spriteRenderer.enabled = true;
                Physics2D.IgnoreLayerCollision(playerLayer, LayerMask.NameToLayer("Enemy"), false);
            }
        }
    }

    public void TakeDamage(int amount)
    {
        TakeDamage(amount, (Vector2?)null);
    }

    public void TakeDamage(int amount, Vector2 sourcePosition)
    {
        TakeDamage(amount, (Vector2?)sourcePosition);
    }

    private void TakeDamage(int amount, Vector2? sourcePosition)
    {
        if (isInvincible) return;

        playerData.TakeDamage(amount);
        iframeTimer = iframeDuration;

        Physics2D.IgnoreLayerCollision(playerLayer, LayerMask.NameToLayer("Enemy"), true);

        if (sourcePosition.HasValue && playerMovement != null)
        {
            Vector2 direction = new Vector2(Mathf.Sign(transform.position.x - sourcePosition.Value.x), 0f);
            playerMovement.ApplyKnockback(direction, knockbackForce, knockbackDuration);
        }

        if (playerData.IsDead) Die();
    }

    private void Die()
    {
        Physics2D.IgnoreLayerCollision(playerLayer, LayerMask.NameToLayer("Enemy"), false);
        deathScreenManager.Show();
    }
}