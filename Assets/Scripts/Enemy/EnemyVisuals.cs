using UnityEngine;

public class EnemyVisuals : MonoBehaviour
{
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool facingRight = true;

    void Update()
    {
        float moveX = enemyAI.MoveX;

        if (moveX > 0f && !facingRight)
        {
            Flip(true);
        }
        else if (moveX < 0f && facingRight)
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
