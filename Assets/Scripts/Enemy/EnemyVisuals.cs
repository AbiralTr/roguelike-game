using UnityEngine;

public class EnemyVisuals : MonoBehaviour
{
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool facingRight = true;
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float facing = enemyAI.FacingDirection;

        if (facing > 0f && !facingRight)
        {
            Flip(true);
        }
        else if (facing < 0f && facingRight)
        {
            Flip(false);
        }

        if (animator != null) animator.SetBool("IsMoving", enemyAI.IsMoving);
    }

    private void Flip(bool faceRight)
    {
        facingRight = faceRight;
        spriteRenderer.flipX = !faceRight;
    }
}
