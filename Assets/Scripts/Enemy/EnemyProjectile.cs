using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private Vector2 direction;
    private int damage;
    private float speed;
    private float maxDistance;
    private Vector3 startPosition;

    public void Init(Vector2 direction, int damage, float speed, float maxDistance)
    {
        this.direction = direction.normalized;
        this.damage = damage;
        this.speed = speed;
        this.maxDistance = maxDistance;
        startPosition = transform.position;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.flipX = this.direction.x < 0f;
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage, transform.position);
            }
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
