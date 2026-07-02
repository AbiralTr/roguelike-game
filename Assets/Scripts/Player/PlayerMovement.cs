using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{   
    [SerializeField] private PlayerData playerData;
    [SerializeField] private float moveSpeed;
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
    private bool isGrounded;
    private bool jumpPressed;

    private bool isDashing;
    private bool dashPressed;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector2 dashDirection;
    private float lastMoveX = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerData.Initialize();
        moveSpeed = playerData.moveSpeed;
        jumpForce = playerData.jumpForce;
        dashSpeed = playerData.dashSpeed;
        dashDuration = playerData.dashDuration;
        dashCooldown = playerData.dashCooldown;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        moveX = 0f;
        if (keyboard.aKey.isPressed) moveX -= 1f;
        if (keyboard.dKey.isPressed) moveX += 1f;

        if (moveX != 0f) lastMoveX = moveX;

        if (keyboard.wKey.wasPressedThisFrame)
        {
            jumpPressed = true;
        }

        if (keyboard.spaceKey.wasPressedThisFrame && dashCooldownTimer <= 0f && !isDashing)
        {
            dashPressed = true;
        }

        if (keyboard.hKey.wasPressedThisFrame)
        {
            playerData.TakeDamage(10);
        }

        if (keyboard.jKey.wasPressedThisFrame)
        {
            playerData.Heal(10);
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

        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

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
}