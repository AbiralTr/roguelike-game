using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;

    public float FacingDirection => facingDirection;
    public bool IsMoving => rb.linearVelocity.x != 0f;

    private Rigidbody2D rb;
    private Transform player;
    private float facingDirection = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float distance = Mathf.Abs(player.position.x - transform.position.x);

        bool holdGround = enemyData.attackType == AttackType.Ranged && distance <= enemyData.attackRange;

        if (distance <= enemyData.detectionRange)
        {
            facingDirection = Mathf.Sign(player.position.x - transform.position.x);

            if (!holdGround)
            {
                rb.linearVelocity = new Vector2(facingDirection * enemyData.moveSpeed, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }
}
