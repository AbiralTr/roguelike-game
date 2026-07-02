using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private float iframeDuration = 1.5f;
    [SerializeField] private float flashInterval = 0.1f;

    [SerializeField] private LayerMask enemyLayer;
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
        if (isInvincible) return;

        playerData.TakeDamage(amount);
        iframeTimer = iframeDuration;
        
        Physics2D.IgnoreLayerCollision(playerLayer, LayerMask.NameToLayer("Enemy"), true);

        if (playerData.IsDead) Die();
    }

    private void Die()
    {
        // get off my dick twin
        Debug.Log("You Died");
    }
}