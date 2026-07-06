using UnityEngine;

public class Projectile : MonoBehaviour
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
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth health = other.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
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
