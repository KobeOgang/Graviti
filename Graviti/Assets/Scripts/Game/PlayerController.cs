using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Transform spawnPoint;


    [Header("Box Interaction Settings")]
    [SerializeField] private float boxStickCooldown = 0.5f; 

    [Header("Gravity Shift Settings")]
    [SerializeField] private float gravityShiftCooldown = 0.5f; 

    private float nextGravityShiftTime = 0f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private bool isGrounded;
    private Vector2 movement;
    private int lives = 3;
    private Vector2 currentGravityDirection = Vector2.down;
    private bool facingRight = true;
    private float nextBoxStickTime;
    private bool isStuckToBox;
    public PushableBox currentBox;
    private Quaternion targetRotation;
    private bool isStuckFromRight;

    // Animation parameter names
    private readonly string IS_RUNNING = "IsRunning";
    private readonly string IS_JUMPING = "IsJumping";
    private readonly string IS_STUCK_TO_BOX = "IsStuckTo_BOX";
    private readonly string IS_PUSH_IDLE = "IsPushIdle";
    private readonly string IS_PUSHING = "IsPushing";
    private readonly string IS_PULLING = "IsPulling";
    private readonly string IS_IDLE = "IsIdle";

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponent<Animator>();
        UpdateLivesDisplay();
        gameOverPanel.SetActive(false);

        rb.freezeRotation = true;
        targetRotation = transform.rotation;
    }

    private void Update()
    {
        HandleInput();
        UpdateAnimations();
        CheckGravityShift();

        if (transform.rotation != targetRotation)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private void FixedUpdate()
    {
        Move();
        CheckGround();
    }

    private void HandleInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isStuckToBox)
            {
                UnstickFromBox();
                return; 
            }
            else if (isGrounded)
            {
                AudioManager.instance.PlaySFX(AudioManager.instance.jump);
                Vector2 jumpDirection = -currentGravityDirection;
                rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);
            }
        }

        float horizontalInput = Input.GetAxisRaw("Horizontal");

        Vector2 rightDirection = GetRightDirectionForGravity();
        movement = rightDirection * horizontalInput;

        if (!isStuckToBox && horizontalInput != 0)
        {
            HandleMovementFacing();
        }
    }

    private Vector2 GetRightDirectionForGravity()
    {
        if (currentGravityDirection == Vector2.down)
            return Vector2.right;
        if (currentGravityDirection == Vector2.up)
            return Vector2.right;
        if (currentGravityDirection == Vector2.left)
            return Vector2.down;
        if (currentGravityDirection == Vector2.right)
            return Vector2.up;

        return Vector2.right;
    }

    private void Move()
    {
        Vector2 gravityVelocity = Vector2.Dot(rb.velocity, currentGravityDirection) * currentGravityDirection;
        Vector2 moveVelocity = movement * moveSpeed;

        Vector2 finalVelocity = moveVelocity + gravityVelocity;
        rb.velocity = finalVelocity;
    }

    private void CheckGravityShift()
    {
        if (isStuckToBox || Time.time < nextGravityShiftTime) return;

        Vector2 newGravityDirection = currentGravityDirection;

        if (Input.GetKeyDown(KeyCode.UpArrow)) newGravityDirection = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) newGravityDirection = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) newGravityDirection = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow)) newGravityDirection = Vector2.right;

        if (newGravityDirection != currentGravityDirection)
        {
            ShiftGravity(newGravityDirection);
            nextGravityShiftTime = Time.time + gravityShiftCooldown;
        }
    }

    private void ShiftGravity(Vector2 newDirection)
    {
        AudioManager.instance.PlaySFX(AudioManager.instance.gravityShift);
        currentGravityDirection = newDirection;
        Physics2D.gravity = currentGravityDirection * 9.81f;

        float angle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;
        angle += 90;
        targetRotation = Quaternion.Euler(0, 0, angle);

        if (newDirection == Vector2.up)
        {
            if (!facingRight) Flip();
        }

        rb.freezeRotation = false;
        StartCoroutine(ReenableRotationFreeze());
    }

    private System.Collections.IEnumerator ReenableRotationFreeze()
    {
        yield return new WaitForSeconds(0.1f);
        rb.freezeRotation = true;
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

   

    private void UpdateAnimations()
    {
        animator.SetBool(IS_JUMPING, !isGrounded && !isStuckToBox);

        if (isStuckToBox && currentBox != null)
        {
            animator.SetBool(IS_RUNNING, false);

            animator.SetBool(IS_STUCK_TO_BOX, true);
            animator.SetBool(IS_PUSH_IDLE, movement.magnitude == 0f); 

            bool isPushing = (isStuckFromRight && movement.x < 0) || (!isStuckFromRight && movement.x > 0) || 
                (isStuckFromRight && movement.y > 0) || (!isStuckFromRight && movement.y < 0);
            bool isPulling = (isStuckFromRight && movement.x > 0) || (!isStuckFromRight && movement.x < 0) ||
                (isStuckFromRight && movement.y < 0) || (!isStuckFromRight && movement.y > 0);

            animator.SetBool(IS_PUSHING, isPushing);
            animator.SetBool(IS_PULLING, isPulling);

            if (isStuckFromRight)
            {
                GetComponent<SpriteRenderer>().flipX = false; 
            }
            else
            {
                GetComponent<SpriteRenderer>().flipX = false; 
            }
        }
        else
        {
            animator.SetBool(IS_RUNNING, Mathf.Abs(movement.magnitude) > 0.1f && isGrounded);

            animator.SetBool(IS_STUCK_TO_BOX, false);
            animator.SetBool(IS_PUSH_IDLE, false);
            animator.SetBool(IS_PUSHING, false);
        }
    }

    private void HandleMovementFacing()
    {
        if (currentGravityDirection == Vector2.up)
        {
            bool shouldFaceRight = movement.x < 0;
            if (facingRight != shouldFaceRight) Flip();
        }
        else if (currentGravityDirection == Vector2.left)
        {
            bool shouldFaceRight = movement.y < 0;
            if (facingRight != shouldFaceRight) Flip();
        }
        else if (currentGravityDirection == Vector2.right)
        {
            bool shouldFaceRight = movement.y > 0;
            if (facingRight != shouldFaceRight) Flip();
        }
        else 
        {
            bool shouldFaceRight = movement.x > 0;
            if (facingRight != shouldFaceRight) Flip();
        }
    }

    private void Flip()
    {
        if (!isStuckToBox)
        {
            facingRight = !facingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    public void StickToBox(PushableBox box)
    {
        if (Time.time >= nextBoxStickTime && !isStuckToBox && box != null && !box.IsLocked() && isGrounded)
        {
            isStuckToBox = true;
            currentBox = box;

            if (currentGravityDirection == Vector2.down || currentGravityDirection == Vector2.up)
            {
                isStuckFromRight = transform.position.x > box.transform.position.x; 
            }
            else if (currentGravityDirection == Vector2.left)
            {
                isStuckFromRight = transform.position.y < box.transform.position.y; 
            }
            else if (currentGravityDirection == Vector2.right)
            {
                isStuckFromRight = transform.position.y < box.transform.position.y;
            }

            currentBox.OnPlayerStick();
        }
    }

    public void UnstickFromBox()
    {
        if (isStuckToBox && currentBox != null)
        {
            isStuckToBox = false;
            currentBox.OnPlayerUnstick();
            currentBox = null;
            nextBoxStickTime = Time.time + boxStickCooldown;

            animator.SetBool(IS_STUCK_TO_BOX, false);
            animator.SetBool(IS_PUSH_IDLE, false);
            animator.SetBool(IS_PUSHING, false);
            animator.SetBool(IS_PULLING, false);
  
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Box") && !isStuckToBox)
        {
            PushableBox box = collision.gameObject.GetComponent<PushableBox>();
            if (box != null && !box.IsLocked())
            {
                StickToBox(box);
            }
        }
        else if (collision.gameObject.CompareTag("Spikes"))
        {
            TakeDamage();
        }
    }

    private void TakeDamage()
    {
        AudioManager.instance.PlaySFX(AudioManager.instance.deathSFX);
        lives--;
        UpdateLivesDisplay();

        if (lives <= 0)
        {
            GameOver();
        }
        else
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        transform.position = spawnPoint.position;
        rb.velocity = Vector2.zero;
        ShiftGravity(Vector2.down); 
        if (isStuckToBox)
        {
            UnstickFromBox();
        }
    }

    private void GameOver()
    {
        AudioManager.instance.StopMusic();
        AudioManager.instance.PlayGameOverSFX();
        gameOverPanel.SetActive(true);
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;
    }

    private void UpdateLivesDisplay()
    {
        if (livesText)
        {
            livesText.text = $"Lives: {lives}";
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    public bool IsStuckToBox()
    {
        return isStuckToBox;
    }
}
