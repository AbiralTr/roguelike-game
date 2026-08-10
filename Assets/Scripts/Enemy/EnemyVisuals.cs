using UnityEngine;

public class EnemyVisuals : MonoBehaviour
{
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool facingRight = true;

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
    }

    private void Flip(bool faceRight)
    {
        facingRight = faceRight;
        spriteRenderer.flipX = !faceRight;
    }
}
