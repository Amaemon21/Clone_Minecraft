using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Space(5)]
    [Header("Speed Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _maxSpeed = 7f;
    [SerializeField] private float _runMultiplier = 1.3f;

    [Space(5)]
    [Header("Jump Settings")]
    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [Header("Grounded check parameters:")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float rayDistance = 1;
    [SerializeField] private bool _isGrounded;

    private CharacterController _characterController;
    private Vector3 playerVelocity;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, rayDistance, groundMask);
    }

    public void Walk(Vector3 movementInput, bool runningInput)
    {
        Vector3 movementDirection = GetMovementDirection(movementInput);
        float speed = runningInput ? _maxSpeed * _runMultiplier : _moveSpeed;
        _characterController.Move(movementDirection * Time.deltaTime * speed);
    }

    public void HandleGravity(bool isJumping)
    {
        if (_characterController.isGrounded && playerVelocity.y < 0)
            playerVelocity.y = 0f;
        _characterController.Move(playerVelocity * Time.deltaTime);
    }

    private Vector3 GetMovementDirection(Vector3 movementInput)
    {
        return transform.right * movementInput.x + transform.forward * movementInput.z;
    }
}