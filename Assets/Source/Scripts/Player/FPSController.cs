using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class FPSController : MonoCache
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

    [Space(10)]
    [SerializeField] private GameObject _mobileInput;
    [SerializeField] private FixedJoystick _moveInput;
    [SerializeField] private DynamicJoystick _lookInput;

    private bool _isGrounded;

    private float _xRotation = 0.0f;

    private CharacterController _characterController;
    private Animator _animator;
    private Vector3 _velocity;
    private Transform _transform;
    private float _currentSpeed;

    private Vector2 _lookDirectioin;
    private Vector3 _moveDirection;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        _transform = transform;

        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (Application.isMobilePlatform)
        {
            _mobileInput.SetActive(true);
        }
    }

    private void Update()
    {
        _isGrounded = _characterController.isGrounded;

        if (_isGrounded && _velocity.y < 0)
            _velocity.y = -2f;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        if (Application.isMobilePlatform)
            _moveDirection = (transform.right * _moveInput.Direction.x + transform.forward * _moveInput.Direction.y).normalized;
        else
            _moveDirection = (transform.right * x + transform.forward * z).normalized;

        _currentSpeed = Input.GetKey(ButtonsManager.Instance.runningButton) ? _moveSpeed * _runMultiplier : _moveSpeed;

        if (_moveDirection.sqrMagnitude >= 0.1f && _isGrounded)
            _animator.Play("Move");
        else
            _animator.Play("Idle");

        _characterController.Move(_moveDirection * _currentSpeed * Time.deltaTime);

        if (Input.GetKey(ButtonsManager.Instance.jumpButton) && _isGrounded)
        {
            _velocity.y += Mathf.Sqrt(_jumpForce * -2.0f * _gravity);
            _animator.Play("Idle");
        }

        _velocity.y += _gravity * Time.deltaTime;

        _characterController.Move(_velocity * Time.deltaTime);
    }

    public override void OnLateTick()
    {
        Look();
    }

    public void Jump()
    {
        if (_isGrounded)
        {
            _velocity.y += Mathf.Sqrt(_jumpForce * -2.0f * _gravity);
            _animator.Play("Idle");
        }
    }

    public void Run()
    {
        _currentSpeed = _moveSpeed * _runMultiplier;
    }

    private void Look()
    {
        if (Application.isMobilePlatform)
        {
            _lookDirectioin = new Vector2(_lookInput.Direction.x, _lookInput.Direction.y);
        }
        else
        {
            _lookDirectioin.x = Input.GetAxis("Mouse X") * _mouseSensitivity;
            _lookDirectioin.y = Input.GetAxis("Mouse Y") * _mouseSensitivity;
        }

        _xRotation -= _lookDirectioin.y;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        _transform.Rotate(Vector3.up * _lookDirectioin.x);
    }
}