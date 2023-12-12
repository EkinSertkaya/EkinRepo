using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [Header("Run")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float decceleration;

    [Header("Jump")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float airAcceleration;
    [SerializeField] private float airDecceleration;
    [SerializeField] private float minimumJumpHeight;
    [SerializeField] private float gravityScale;
    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float jumpInputBuffer;
    [SerializeField] private float coyoteTime;

    [Header("Air Jump")]
    [SerializeField] private float airJumpHeight;
    [SerializeField] private int airJumpLimit;

    [Header("Wall Jump")]
    [SerializeField] private float wallJumpHorizontalForce;
    [SerializeField] private float wallJumpVerticalForce;
    [SerializeField] private float wallJumpMinJumpHeight;
    [SerializeField] private float wallSlidingMaxFallSpeed;
    [SerializeField] private float wallSlidingGravityScale;

    [Header("Dash")]
    [SerializeField] private int dashLimit;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration;

    [Header("Required Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private InputActionAsset playerActions;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform rightWallCheck;
    [SerializeField] private LayerMask groundCheckLayers;

    private InputAction horizontalMoveAction;
    private InputAction jumpAction;
    private InputAction dashAction;

    private float horizontalMoveInput;
    private float beingPressed;
    private float wallJumpMinJumpHeightTimer;
    private float jumpInputBufferTimer;
    private float lastOnGroundTime;
    private float minJumpTimer;
    private float currentMaxFallSpeed;
    private float currentAcceleration;
    private float currentDecceleration;
    private float wallJumpTimer = 0f;

    private int currentAirJumpLimit;
    private int currentAirJumpCount;
    private int currentDashLimit;
    private int currentDashCount;
    private int dashForceApplyCounter = 0;
    private int dashForceApplyLimit = 10;

    private bool isGrounded = false;
    private bool isJumping = false;
    private bool isWallJumping = false;
    private bool jumpInput = false;
    private bool reachedMinJumpLimit = false;
    private bool holdingWall = false;
    private bool canHoldWall = false;
    private bool holdingRightWall = false;
    private bool holdingLeftWall = false;
    private bool canWallJumpRight = false;
    private bool canWallJumpLeft = false;
    private bool canAirJump = false;
    private bool hasStartedFalling = false;
    private bool isAirJumping = false;
    private bool isDashing = false;
    private bool isFacingRight = true;
    private bool isWallDashing = false;
    private bool hasStartedWallDash = false;
    private bool hasTouchedWall = false;

    public bool IsGrounded => isGrounded;
    public bool IsJumping => isJumping;
    public bool HoldingWall => holdingWall;
    public bool IsDashing => isDashing;
    public bool IsWallDashing => isWallDashing;

    public float HorizontalMoveInput => horizontalMoveInput;

    public Rigidbody2D Rb => rb;

    private void Awake()
    {
        horizontalMoveAction = playerActions.FindActionMap("Player").FindAction("HorizontalMoveAction");
        jumpAction = playerActions.FindActionMap("Player").FindAction("JumpAction");
        dashAction = playerActions.FindActionMap("Player").FindAction("DashAction");

        jumpInputBufferTimer = jumpInputBuffer;
        lastOnGroundTime = coyoteTime;
        minJumpTimer = minimumJumpHeight;
        currentMaxFallSpeed = maxFallSpeed;
        currentAcceleration = acceleration;
        currentDecceleration = decceleration;
        currentAirJumpLimit = airJumpLimit;
        currentAirJumpCount = currentAirJumpLimit;
        currentDashLimit = dashLimit;
        currentDashCount = currentDashLimit;
    }

    private void FixedUpdate()
    {
        Jump();

        TouchWall();

        WallJump();

        Run();

        Dash();

        LimitFallSpeed();

        AirJump();

        FallCheck();
    }

    private void Update()
    {
        GetInput();

        JumpCheck();

        DashCheck();

        FlipCharacterOnInput(true);

        GroundCheck();

        WallCheck();

        WallJumpCheck();

        AirJumpCheck();
    }

    private void Jump()
    {
        if (jumpInput && isGrounded)
        {
            float force = jumpForce - rb.velocity.y;

            rb.gravityScale = 1f;
            rb.AddForce(force * Vector2.up * rb.mass, ForceMode2D.Impulse);
            isJumping = true;
            jumpInput = false;
            lastOnGroundTime = 0;
        }
    }

    private void TouchWall()
    {
        if (hasTouchedWall)
        {
            hasTouchedWall = false;
            rb.AddForce(Vector2.down * rb.mass * wallSlidingMaxFallSpeed, ForceMode2D.Impulse);
        }
    }

    private void WallJump()
    {
        if (canWallJumpRight)
        {
            StartWallJump(-wallJumpHorizontalForce, ref canWallJumpRight);
        }
        else if (canWallJumpLeft)
        {
            StartWallJump(wallJumpHorizontalForce, ref canWallJumpLeft);
        }
    }

    private void Run()
    {
        float speedDif = horizontalMoveInput * maxSpeed - rb.velocity.x;

        if (horizontalMoveInput != 0)
        {
            rb.AddForce(Vector2.right * rb.mass * currentAcceleration * speedDif);
        }

        if (horizontalMoveInput == 0)
        {
            rb.AddForce(Vector2.right * rb.mass * currentDecceleration * speedDif);
        }
    }

    private void Dash()
    {
        if (isDashing || isWallDashing)
        {
            if (isFacingRight)
            {
                StartWallDash(dashSpeed);
            }
            else if (!isFacingRight)
            {
                StartWallDash(-dashSpeed);
            }
        }
    }

    private void LimitFallSpeed()
    {
        if (rb.velocity.y < -currentMaxFallSpeed)
        {
            rb.AddForce(Vector2.up * rb.mass * (-currentMaxFallSpeed - rb.velocity.y), ForceMode2D.Impulse);
        }
    }

    private void AirJump()
    {
        if (canAirJump)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, 0f);
            rb.gravityScale = 1f;
            rb.AddForce(airJumpHeight * rb.mass * Vector2.up, ForceMode2D.Impulse);
            canAirJump = false;
        }
    }

    private void FallCheck()
    {
        if (hasStartedFalling)
        {
            hasStartedFalling = false;
            rb.AddForce(Vector2.up * -rb.velocity.y, ForceMode2D.Impulse);
        }
    }

    private void GetInput()
    {
        horizontalMoveInput = horizontalMoveAction.ReadValue<float>();

        beingPressed = jumpAction.ReadValue<float>();

        if (jumpAction.triggered) jumpInput = true;
    }

    private void JumpCheck()
    {
        if (!isGrounded && jumpInput)
        {
            jumpInputBufferTimer -= Time.deltaTime;

            if (jumpInputBufferTimer <= 0)
            {
                jumpInput = false;
                jumpInputBufferTimer = jumpInputBuffer;
            }
        }

        if (isJumping)
        {
            minJumpTimer -= Time.deltaTime;

            if (minJumpTimer <= 0)
            {
                reachedMinJumpLimit = true;
            }
        }
        else minJumpTimer = minimumJumpHeight;

        if (isJumping && !isGrounded && beingPressed < 1 && reachedMinJumpLimit)
        {
            isJumping = false;
        }

        if (rb.velocity.y < 0 && !holdingWall)
        {
            if (!canHoldWall) canHoldWall = true;
            rb.gravityScale = gravityScale;
        }

        if (rb.velocity.y > 0 && !isJumping && !isWallJumping && !isAirJumping)
        {
            hasStartedFalling = true;
        }
    }

    private void DashCheck()
    {
        if (dashAction.triggered && currentDashCount > 0)
        {
            horizontalMoveAction.Disable();

            rb.velocity = Vector3.zero;

            if (holdingWall)
            {
                isWallDashing = true;
                hasStartedWallDash = true;
            }
            else isDashing = true;

            currentDashCount--;

            currentAcceleration = airAcceleration;
            currentDecceleration = airDecceleration;

            rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void FlipCharacterOnInput(bool onInput)
    {
        if (onInput)
        {
            if (rb.velocity.x > 0 && !isFacingRight)
            {
                FlipCharacter(true);
            }
            else if (rb.velocity.x < 0 && isFacingRight)
            {
                FlipCharacter(false);
            }
            else if (horizontalMoveInput > 0 && !isFacingRight)
            {
                FlipCharacter(true);
            }
            else if (horizontalMoveInput < 0 && isFacingRight)
            {
                FlipCharacter(false);
            }
        }
        else
        {
            if (!isFacingRight)
            {
                FlipCharacter(true);
            }
            else if (isFacingRight)
            {
                FlipCharacter(false);
            }
        }
    }

    private void GroundCheck()
    {
        if (Physics2D.OverlapCircle(groundCheck.position, .12f, groundCheckLayers) && !isDashing)
        {
            isGrounded = true;
            isWallJumping = false;

            if (canHoldWall) canHoldWall = false;
            if (holdingWall) holdingWall = false;
            if (holdingLeftWall) holdingLeftWall = false;
            if (holdingRightWall) holdingRightWall = false;
            if (currentMaxFallSpeed != maxFallSpeed) currentMaxFallSpeed = maxFallSpeed;
            if (!isJumping) lastOnGroundTime = coyoteTime;
            if (rb.gravityScale != 1) rb.gravityScale = 1;

            jumpInputBufferTimer = jumpInputBuffer;
            currentAirJumpCount = currentAirJumpLimit;
            currentDashCount = currentDashLimit;

            currentAcceleration = acceleration;
            currentDecceleration = decceleration;

            return;
        }

        lastOnGroundTime -= Time.deltaTime;

        if (lastOnGroundTime <= 0)
        {
            isGrounded = false;
            currentAcceleration = airAcceleration;
            currentDecceleration = airDecceleration;
        }
    }

    private void WallCheck()
    {
        if (canHoldWall)
        {
            if (Physics2D.OverlapCircle(rightWallCheck.position, .01f, groundCheckLayers))
            {
                if (!holdingWall)
                {
                    OnTouchWall();
                }

                return;
            }

            if (holdingWall)
            {
                wallJumpTimer += Time.deltaTime;

                if (wallJumpTimer >= 0.03f)
                {
                    holdingWall = false;
                    holdingLeftWall = false;
                    holdingRightWall = false;
                    if (currentMaxFallSpeed != maxFallSpeed) currentMaxFallSpeed = maxFallSpeed;

                    wallJumpTimer -= wallJumpTimer;
                }
            }
        }
    }

    private void WallJumpCheck()
    {
        if (holdingWall && jumpAction.triggered)
        {
            isWallJumping = true;

            if (isFacingRight && isWallJumping)
            {
                SetWallJumpModifiers(ref canWallJumpRight);
            }
            else if (!isFacingRight && isWallJumping)
            {
                SetWallJumpModifiers(ref canWallJumpLeft);
            }
        }

        if (isWallJumping)
        {
            wallJumpMinJumpHeightTimer += Time.deltaTime;

            if (wallJumpMinJumpHeightTimer >= wallJumpMinJumpHeight && beingPressed < 1)
            {
                isWallJumping = false;
            }
        }
        else
        {
            wallJumpMinJumpHeightTimer -= wallJumpMinJumpHeightTimer;
        }
    }

    private void AirJumpCheck()
    {
        if (!isGrounded && !holdingWall && jumpAction.triggered && currentAirJumpCount > 0)
        {
            currentAirJumpCount--;
            canAirJump = true;
            isAirJumping = true;
        }

        if (isAirJumping && beingPressed < 1)
        {
            isAirJumping = false;
        }
    }

    private void StartWallDash(float dashSpeed)
    {
        if (isWallDashing && hasStartedWallDash && dashForceApplyCounter == 0)
        {
            FlipCharacterOnInput(false);
            hasStartedWallDash = false;
        }

        if(dashForceApplyCounter < dashForceApplyLimit)
        {
            rb.AddForce(new Vector3(dashSpeed, 0f, 0f) * rb.mass, ForceMode2D.Impulse);
            ++dashForceApplyCounter;

            if(dashForceApplyCounter >= dashForceApplyLimit)
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                
                if (isDashing) isDashing = false;
                if (isWallDashing) isWallDashing = false;
                

                currentAcceleration = acceleration;
                currentDecceleration = decceleration;

                horizontalMoveAction.Enable();

                dashForceApplyCounter -= dashForceApplyCounter;
            }
        }
    }

    private void OnTouchWall()
    {
        isWallJumping = false;
        holdingWall = true;
        rb.gravityScale = wallSlidingGravityScale;
        rb.velocity = Vector2.zero;
        currentMaxFallSpeed = wallSlidingMaxFallSpeed;
        currentAirJumpCount = currentAirJumpLimit;
        currentDashCount = currentDashLimit;
        wallJumpTimer -= wallJumpTimer;
        hasTouchedWall = true;
    }

    private void SetWallJumpModifiers(ref bool wallJumpSide)
    {
        rb.gravityScale = 1;
        rb.velocity = Vector3.zero;
        wallJumpSide = true;
    }

    private void StartWallJump(float wallJumpHorizontalForce, ref bool canWallJump)
    {
        rb.velocity = Vector3.zero;
        rb.gravityScale = 1f;
        rb.AddForce(new Vector3(wallJumpHorizontalForce, wallJumpVerticalForce, 0f) * rb.mass, ForceMode2D.Impulse);
        canWallJump = false;
    }

    private void LimitValue(ref float value, float limit, float percent)
    {
        if (value < 0f) value = 0f;

        while (limit * value * percent > limit)
        {
            --value;
        }
    }

    private void FlipCharacter(bool facingRight)
    {
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        isFacingRight = facingRight;
    }

    private void OnValidate()
    {
        LimitValue(ref currentAcceleration, maxSpeed, Time.fixedDeltaTime);
        LimitValue(ref currentDecceleration, maxSpeed, Time.fixedDeltaTime);
    }

    private void OnEnable()
    {
        playerActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        playerActions.FindActionMap("Player").Disable();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(groundCheck.position, .12f);
        Gizmos.DrawSphere(rightWallCheck.position, .1f);
    }
}
