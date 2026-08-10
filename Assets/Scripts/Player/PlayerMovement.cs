using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{   
    [SerializeField] private PlayerData playerData;
    [SerializeField] private InputActionsHub inputActionsHub;
    [SerializeField] private float jumpForce;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Dash")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration;
    [SerializeField] private float dashCooldown;

    public bool IsGrounded => isGrounded;
    private Rigidbody2D rb;
    private float moveX;
    public float MoveX => moveX;
    public float FacingDirection => lastMoveX;
    private bool isGrounded;
    private bool jumpPressed;

    private bool isDashing;
    private bool dashPressed;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector2 dashDirection;
    private float lastMoveX = 1f;

    private bool isKnockedBack;
    private float knockbackTimer;
    private Vector2 knockbackVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerData.Initialize();
        jumpForce = playerData.jumpForce;
        dashSpeed = playerData.dashSpeed;
        dashDuration = playerData.dashDuration;
        dashCooldown = playerData.dashCooldown;
    }

    void Update()
    {
        var player = inputActionsHub.Actions.Player;

        moveX = player.Move.ReadValue<float>();

        if (moveX != 0f) lastMoveX = moveX;

        if (player.Jump.WasPressedThisFrame())
        {
            jumpPressed = true;
        }

        if (player.Dash.WasPressedThisFrame() && dashCooldownTimer <= 0f && !isDashing)
        {
            dashPressed = true;
        }

        // Debug-only shortcuts, deliberately left on raw keyboard polling rather
        // than formal Input Actions — meant to be stripped before a real build.
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.hKey.wasPressedThisFrame)
            {
                playerData.TakeDamage(10);
            }

            if (keyboard.jKey.wasPressedThisFrame)
            {
                playerData.Heal(10);
            }
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (dashCooldownTimer > 0f) dashCooldownTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (dashPressed)
        {
            StartDash();
            dashPressed = false;
        }

        if (isDashing)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
            return;
        }

        if (isKnockedBack)
        {
            rb.linearVelocity = knockbackVelocity;
            knockbackTimer -= Time.fixedDeltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockedBack = false;
            }
            return;
        }

        rb.linearVelocity = new Vector2(moveX * playerData.moveSpeed, rb.linearVelocity.y);

        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        jumpPressed = false;
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        dashDirection = new Vector2(lastMoveX, 0f).normalized;
    }

    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        isKnockedBack = true;
        knockbackTimer = duration;
        knockbackVelocity = direction.normalized * force;
    }
}
