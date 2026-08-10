using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool facingRight = true;
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float moveX = playerMovement.MoveX;

        if (moveX > 0f && !facingRight)
        {
            Flip(true);
        }
        else if (moveX < 0f && facingRight)
        {
            Flip(false);
        }

        if (animator != null) animator.SetBool("IsWalking", moveX != 0f);
    }

    private void Flip(bool faceRight)
    {
        facingRight = faceRight;
        spriteRenderer.flipX = !faceRight;
    }
}