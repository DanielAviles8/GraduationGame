using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour
{

    [SerializeField] private InputActionsHolder inputActionsHolder;

    [SerializeField] private float _moveSpeed = 150f;
    [SerializeField] private float _gravity = 9.8f;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float climbSpeed = 5f; 
    [SerializeField] private float wallDetectionDistance = 0.5f; 
    [SerializeField] private float grappleRange = 10f; 
    [SerializeField] private float grappleSpeed = 10f;
    [SerializeField] private LineRenderer grappleLine; // Referencia al LineRenderer
    [SerializeField] private Material grappleMaterial; // Material del grapple
    [SerializeField] private float grappleLineWidth = 0.1f; // Ancho de la l�nea


    private GameInputActions _inputActions;
    private CharacterController _characterController;
    private SpriteRenderer _spriteRenderer;

    public float jumpForce = 15.0f;
    private float verticalSpeed;
    private Vector2 _inputVector;
    private Vector2 dashDirection;
    private bool isTouchingWall = false;
    private float dashTimeRemaining = 0f;
    private float cooldownTimeRemaining = 0f;
    private Vector3 lastDirection = Vector3.right;

    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask anchorPointLayer; 


    private CharacterController controller;
    private Vector3 grappleTarget;
    private bool isGrappling = false;
    private Transform lastAnchorPoint;
    private float grappleTimeRemaining;
    [SerializeField] private float grappleMaxDuration = 2f;

    //Buleanos para animaciones
    private bool isClimbing = false; 
    private bool isDashing = false;
    private bool isJumping = false;
    private bool isMoving = false;

    private Animator animator;

    private void OnDestroy()
    {
        _inputActions.Player.Jump.performed -= JumpPlayer;
        _inputActions.Player.Dash.performed -= DashPlayer;
        _inputActions.Player.GrabWall.performed -= GrabWall;
        _inputActions.Player.Grapple.performed -= TryGrapple;
    }

    void Start()
    {
        Prepare();
        if (grappleLine == null)
        {
            animator = gameObject.GetComponent<Animator>();
            GameObject lineObject = new GameObject("GrappleLine");
            grappleLine = lineObject.AddComponent<LineRenderer>();
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            grappleLine.material = grappleMaterial; 
            grappleLine.startWidth = grappleLineWidth;
            grappleLine.endWidth = grappleLineWidth;
            grappleLine.positionCount = 2; 
            grappleLine.enabled = false;  
        }
    }

    void Update()
    {

        if(isClimbing == true)
        {
            _inputVector = _inputActions.Player.ClimbWall.ReadValue<Vector2>();
        }
        else
        {
            _inputVector = _inputActions.Player.Move.ReadValue<Vector2>();
        }

        if (isDashing)
        {
            PerformDash();
        }
        else
        {
            MovePlayer();
            ClimbWall();
            if (cooldownTimeRemaining > 0)
            {
                cooldownTimeRemaining -= Time.deltaTime;
            }
        }

        animator.SetBool("IsRunning", _characterController.velocity.magnitude > 1f && !isDashing);
        animator.SetBool("IsJumping", !_characterController.isGrounded && !isDashing);
        animator.SetBool("IsClimbing", isClimbing);
        animator.SetBool("IsDashing", isDashing);

        if (isGrappling)
        {
            MoveTowardsGrappleTarget();
        }

        if(isDashing == false)
        {
            animator.SetBool("IsDashing", false);
        }

        Vector3 lockZ = new Vector3(transform.position.x, transform.position.y, 0);
        transform.position = lockZ;

        DetectWall();
        ClimbWall();
    }

    private void Prepare()
    {
        _characterController = GetComponent<CharacterController>();
        _inputActions = inputActionsHolder._GameInputActions;
        _inputActions.Player.Jump.performed += JumpPlayer;
        _inputActions.Player.Dash.performed += DashPlayer;
        _inputActions.Player.GrabWall.performed += GrabWall;
        _inputActions.Player.Grapple.performed += TryGrapple;
    }

    private void MovePlayer()
    {
        if (isClimbing)
        {
            Vector3 climbMovement = new Vector3(0, _inputVector.y * climbSpeed, 0);
            _characterController.Move(climbMovement * Time.deltaTime);

            if (_inputVector.y == 0 && !_inputActions.Player.GrabWall.ReadValue<float>().Equals(1))
            {
                ReleaseWall();
            }

            return;
        }

        Vector3 dir = new Vector3(_inputVector.x, 0, _inputVector.y);
        Vector3 move = transform.TransformDirection(dir) * _moveSpeed;

        if (_inputVector.x < 0)
        {
            _spriteRenderer.flipX = true;
        }
        else if (_inputVector.x > 0)
        {
            _spriteRenderer.flipX = false;
        }

        if (_characterController.isGrounded)
        {
            if (verticalSpeed < 0)
            {
                verticalSpeed = -0.5f;
            }

            if (lastAnchorPoint != null)
            {
                lastAnchorPoint = null;
            }
        }
        else
        {
            verticalSpeed -= _gravity * Time.deltaTime;
        }

        move.y = verticalSpeed;
        _characterController.Move(move * Time.deltaTime);  
    }
    private void JumpPlayer(InputAction.CallbackContext ctx)
    {
        if (isClimbing)
        {
            animator.SetBool("IsJumping", true);
            ReleaseWall(); 
            _gravity = 4;
            verticalSpeed = jumpForce;
            _gravity = 20f;
        }
        else if (_characterController.isGrounded)
        {
            animator.SetBool("IsJumping", true);
            isJumping = true;
            _gravity = 4;
            verticalSpeed = jumpForce;
            _gravity = 20f;
        }
    }
    private void DashPlayer(InputAction.CallbackContext ctx)
    {
        if (isClimbing) return;
        if (cooldownTimeRemaining > 0) return;

        dashDirection = _inputVector.normalized;
        if (dashDirection == Vector2.zero) return;

        isDashing = true;
        
        dashTimeRemaining = dashDistance / dashSpeed;
        cooldownTimeRemaining = dashCooldown;
    }

    private void PerformDash()
    {
        if (dashTimeRemaining > 0)
        {
            Vector3 movement = new Vector3(dashDirection.x, 0, dashDirection.y) * dashSpeed * Time.deltaTime;
            _characterController.Move(movement);
            dashTimeRemaining -= Time.deltaTime;
        }
        else
        {
            isDashing = false;
        }
    }

    private void DetectWall()
    {
        if (Mathf.Abs(_inputVector.x) > 0.1f)
            lastDirection = _inputVector.x > 0 ? transform.right : -transform.right;

        Vector3 direction = lastDirection;
        Vector3 rayOrigin = transform.position + Vector3.down * 0.9f;

        if (Physics.Raycast(rayOrigin, direction, out RaycastHit hit, wallDetectionDistance, wallLayer))
        {
            if (!isTouchingWall)

                isTouchingWall = true;
        }
        else
        {
            if (isTouchingWall)

                isTouchingWall = false;
            isClimbing = false;
        }

        Debug.DrawRay(rayOrigin, direction * wallDetectionDistance, Color.yellow);
    }
    private void GrabWall(InputAction.CallbackContext ctx)
    {
        if (isTouchingWall)
        {
            isClimbing = true;
            verticalSpeed = 0;
        }
    }

    private void ReleaseWall()
    {
        isClimbing = false;
        isTouchingWall = false;
    }

    private void ClimbWall()
    {
        if (!isClimbing) return;

        float verticalInput = _inputVector.y;

        Vector3 climbMovement = new Vector3(0, verticalInput * climbSpeed, 0);
        _characterController.Move(climbMovement * Time.deltaTime);

        if (Mathf.Abs(verticalInput) < 0.1f)
        {
            verticalSpeed = 0;
        }
    }

    void TryGrapple(InputAction.CallbackContext ctx)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, grappleRange, anchorPointLayer);
        if (hitColliders.Length > 0)
        {
            Transform closestPoint = hitColliders[0].transform;
            float closestDistance = Vector3.Distance(transform.position, closestPoint.position);

            foreach (Collider2D hit in hitColliders)
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestPoint = hit.transform;
                    closestDistance = distance;
                }
            }

            if (lastAnchorPoint != closestPoint || _characterController.isGrounded)
            {
                grappleTarget = closestPoint.position;
                isGrappling = true;
                grappleTimeRemaining = grappleMaxDuration;
                lastAnchorPoint = closestPoint;

                grappleLine.enabled = true;
                UpdateGrappleLine(transform.position, grappleTarget);
            }
        }
    }

    void MoveTowardsGrappleTarget()
    {
        if (grappleTimeRemaining <= 0)
        {
            RestoreGravity();
            EndGrapple();
            return;
        }

        grappleTimeRemaining -= Time.deltaTime;

        Vector3 direction = (grappleTarget - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, grappleTarget);

        if (distance > 0.5f)
        {
            _gravity = 0;
            Vector3 move = direction * grappleSpeed * Time.deltaTime;
            _characterController.Move(move);
            UpdateGrappleLine(transform.position, grappleTarget);
        }
        else
        {
            Vector3 exitDirection = (grappleTarget - transform.position).normalized;
            ApplyGrappleBoost(exitDirection);
            RestoreGravity();
            EndGrapple();
        }
    }
    void EndGrapple()
    {
        isGrappling = false;
        grappleLine.enabled = false;
    }

    void UpdateGrappleLine(Vector3 start, Vector3 end)
    {
        grappleLine.SetPosition(0, start);
        grappleLine.SetPosition(1, end);
    }
    void ApplyGrappleBoost(Vector3 direction)
    {
        float boostForce = 10f;
        Vector3 velocity = direction * boostForce;
    }
    void RestoreGravity()
    {
        _gravity = 20f;
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, grappleRange);
    }

    public void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("EndGame"))
        {
            SceneManager.LoadScene("Credits");
        }
    }

}