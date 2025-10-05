using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : ManagedBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _maxSpeed = 10f;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private float _mouseSensitivity = 100f;
    private CharacterController _characterController;
    private Vector3 _velocity;
    private Transform _cam;
    private bool _isAccelerated = false;
    private bool _isMoving = false;
    private bool _isGrounded = false;
    private float _xRotation = 0f;
    private Animator _animator;
    
    // Input System actions (можешь привязать из Input Actions Asset через инспектор)
    [Header("Input Actions (optional if you have an asset)")]
    [SerializeField] private InputAction _moveAction;
    [SerializeField] private InputAction _lookAction;
    [SerializeField] private InputAction _jumpAction;
    [SerializeField] private InputAction _sprintAction;
    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _cam = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
        _animator = GetComponentInChildren<Animator>();
        EnsureDefaultBindingsIfEmpty();
    }

    void OnEnable()
    {
        if (_moveAction != null) _moveAction.Enable();
        if (_lookAction != null) _lookAction.Enable();
        if (_jumpAction != null) _jumpAction.Enable();
        if (_sprintAction != null) _sprintAction.Enable();
    }

    void OnDisable()
    {
        if (_moveAction != null) _moveAction.Disable();
        if (_lookAction != null) _lookAction.Disable();
        if (_jumpAction != null) _jumpAction.Disable();
        if (_sprintAction != null) _sprintAction.Disable();
    }
    protected override void PausableUpdate()
    {
        // Проверка касания земли
        CheckGround();
        // Движение
        Move();

        // Прыжок
        Jump();
    }
    protected override void PausableLateUpdate()
    {
        // Управление камерой мышкой
        CameraControll();
    }
    private void CheckGround()
    {
        _isGrounded = _characterController.isGrounded;
        if (_isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;

    }
    private void Move()
    {
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        _isMoving = move.sqrMagnitude > 0.0001f;

        // Спринт (Shift / нажатие левого стика)
        _isAccelerated = _sprintAction.IsPressed();
        float speed = _isAccelerated ? _maxSpeed : _moveSpeed;
        _animator.SetFloat("Speed", move.sqrMagnitude*speed/_moveSpeed);
        _characterController.Move(move * speed * Time.deltaTime);
    }
    private void Jump()
    {
        if (_jumpAction.triggered && _isGrounded)
            _velocity.y = Mathf.Sqrt(2f * -_gravity);

        // Гравитация
        _velocity.y += _gravity * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);
    }
    private void CameraControll()
    {

        Vector2 lookInput = _lookAction.ReadValue<Vector2>();
        float mouseX = lookInput.x * _mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * _mouseSensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        _cam.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
    // Создаём дефолтные биндинги, если экшены не заполнены в инспекторе
    private void EnsureDefaultBindingsIfEmpty()
    {
        if (_moveAction == null || _moveAction.bindings.Count == 0)
        {
            _moveAction = new InputAction("Move", InputActionType.Value);
            var comp = _moveAction.AddCompositeBinding("2DVector");
            comp.With("Up", "<Keyboard>/w");
            comp.With("Down", "<Keyboard>/s");
            comp.With("Left", "<Keyboard>/a");
            comp.With("Right", "<Keyboard>/d");
            _moveAction.AddBinding("<Gamepad>/leftStick");
        }

        if (_lookAction == null || _lookAction.bindings.Count == 0)
        {
            _lookAction = new InputAction("Look", InputActionType.Value);
            _lookAction.AddBinding("<Mouse>/delta");
            _lookAction.AddBinding("<Gamepad>/rightStick");
        }

        if (_jumpAction == null || _jumpAction.bindings.Count == 0)
        {
            _jumpAction = new InputAction("Jump", InputActionType.Button);
            _jumpAction.AddBinding("<Keyboard>/space");
            _jumpAction.AddBinding("<Gamepad>/buttonSouth");
        }

        if (_sprintAction == null || _sprintAction.bindings.Count == 0)
        {
            _sprintAction = new InputAction("Sprint", InputActionType.Button);
            _sprintAction.AddBinding("<Keyboard>/leftShift");
            _sprintAction.AddBinding("<Gamepad>/leftStickPress");
        }
    }
    
}
