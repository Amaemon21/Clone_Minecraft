using Unity.Android.Gradle.Manifest;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class FPSController : MonoBehaviour
{
    [SerializeField] private Transform _cameraTransform = null;

    [Space]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _runMultiplier = 2.0f;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private float _jumpForce = 10f;

    [Space]
    [SerializeField] private float _mouseSensitivity = 2.0f;

    [Space]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundDistance = 0.4f;
    [SerializeField] private LayerMask _groundMask;
    private bool _isGrounded;

    private float _xRotation = 0.0f;

    private CharacterController _characterController;
    private Animator _animator;
    private Vector3 _velocity;
    private Transform _transform;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        _transform = transform;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        _isGrounded = _characterController.isGrounded;

        if (_isGrounded && _velocity.y < 0)
            _velocity.y = -2f;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = (transform.right * x + transform.forward * z).normalized;

        float currentSpeed = Input.GetKey(ButtonsManager.Instance.runningButton) ? _moveSpeed * _runMultiplier : _moveSpeed;

        if (moveDirection.sqrMagnitude >= 0.1f && _isGrounded)
            _animator.Play("Move");
        else
            _animator.Play("Idle");

        _characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

        if (Input.GetKey(ButtonsManager.Instance.jumpButton) && _isGrounded)
        {
            _velocity.y += Mathf.Sqrt(_jumpForce * -2.0f * _gravity);
            _animator.Play("Idle");
        }

        _velocity.y += _gravity * Time.deltaTime;

        _characterController.Move(_velocity * Time.deltaTime);
    }

    private void LateUpdate()
    {
        Look();
    }

    public void SetSensitivity(float newSensitivity)
    {
        if (newSensitivity >= 0)
            _mouseSensitivity = newSensitivity;
    }

    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        _transform.Rotate(Vector3.up * mouseX);
    }
}