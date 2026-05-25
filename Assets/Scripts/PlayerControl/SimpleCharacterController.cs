using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

namespace PlayerControl
{
    public class SimpleCharacterController : MonoBehaviour, IPlayerObject
    {
        private readonly int _moveSpeedHash = Animator.StringToHash("MoveSpeed");
        private readonly int _isJumpingAnimHash = Animator.StringToHash("IsJumping");
        private readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");
        private readonly int _currentGaitHash = Animator.StringToHash("CurrentGait");
        private readonly int _strafeDirectionXHash = Animator.StringToHash("StrafeDirectionX");
        private readonly int _strafeDirectionZHash = Animator.StringToHash("StrafeDirectionZ");
        private readonly int _isStrafingHash = Animator.StringToHash("IsStrafing");
        private readonly int _isWalkingHash = Animator.StringToHash("IsWalking");
        private readonly int _isStoppedHash = Animator.StringToHash("IsStopped");
        private readonly int _movementInputHeldHash = Animator.StringToHash("MovementInputHeld");

        [Header("Components")] [SerializeField]
        private InputReader _inputReader;

        [SerializeField] private Animator _animator;
        [SerializeField] private CharacterController _controller;
        [SerializeField] private Transform _modelTransform;

        [Header("Movement Settings")] [SerializeField]
        private float _moveSpeed = 5f;

        [SerializeField] private float _jumpForce = 10f;
        [SerializeField] private float _gravityMultiplier = 2f;
        [SerializeField] private float _rotationSpeed = 10f;

        [Header("Movement Feel")] [SerializeField]
        private float _acceleration = 20f;

        [SerializeField] private float _deceleration = 30f;
        [SerializeField] private float _airAcceleration = 10f;

        [Header("Jump Feel")] [SerializeField] private float _coyoteTime = 0.15f;
        [SerializeField] private float _jumpBufferTime = 0.15f;
        [SerializeField] private float _fallGravityMultiplier = 4f;
        [SerializeField] private float _lowJumpMultiplier = 2f;

        [Header("Ground Check")] [SerializeField]
        private LayerMask _groundLayerMask;

        [SerializeField] private float _groundedOffset = -0.14f;

        [Header("Respawn data")] [SerializeField]
        private Vector3 _initialPosition = new Vector3(0f, 0f, 0f);

        private Vector3 _velocity;
        private bool _isGrounded = true;
        private float _speed2D;
        private Vector3 _moveDirection;
        private int _currentGait;
        private float _strafeDirectionX = 0f;
        private float _strafeDirectionZ = 1f;
        private bool _isWalking = false;
        private bool _isStopped = true;
        private bool _movementInputHeld = false;

        private float _currentHorizontalSpeed;
        private float _lastGroundedTime;
        private float _lastJumpPressedTime;
        private bool _jumpReleased = true;

        private float _lastBonusEffectTriggeredTime;
        private float _radius;


        private void Start()
        {
            _inputReader.onJumpPerformed += OnJump;
            _radius = _controller.radius;
        }

        private void Update()
        {
            GroundedCheck();
            HandleJump();
            CalculateMoveDirection();
            CheckIfStopped();
            FaceMoveDirection();
            ApplyGravity();
            Move();
            UpdateAnimator();
        }

        private void CalculateMoveDirection()
        {
            int inputX = Mathf.RoundToInt(_inputReader._moveComposite.x);
            _moveDirection = new Vector3(inputX, 0f, 0f);
            _movementInputHeld = _moveDirection.magnitude > 0.01f;

            float targetSpeed = _moveDirection.x * _moveSpeed;

            float acceleration = _isGrounded
                ? (_movementInputHeld ? _acceleration : _deceleration)
                : _airAcceleration;

            _currentHorizontalSpeed = Mathf.MoveTowards(
                _currentHorizontalSpeed,
                targetSpeed,
                acceleration * Time.deltaTime);

            _velocity.x = _currentHorizontalSpeed;
            _velocity.z = 0f;

            _speed2D = new Vector3(_velocity.x, 0f, _velocity.z).magnitude;
            _speed2D = Mathf.Round(_speed2D * 1000f) / 1000f;

            CalculateGait();
        }

        private void CalculateGait()
        {
            if (_speed2D < 0.01f)
            {
                _currentGait = 0; // Idle
            }
            else if (_speed2D < _moveSpeed * 0.5f)
            {
                _currentGait = 1; // Walk
            }
            else
            {
                _currentGait = 2; // Run
            }
        }

        private void CheckIfStopped()
        {
            _isStopped = _moveDirection.magnitude == 0 && _speed2D < 0.5f;
            _isWalking = !_isStopped && _isGrounded;
        }

        private void FaceMoveDirection()
        {
            if (_modelTransform == null)
                return;

            if (_moveDirection.magnitude > 0.01f)
            {
                Vector3 faceDirection = new Vector3(_velocity.x, 0f, 0f);
                if (faceDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(faceDirection);
                    _modelTransform.rotation = Quaternion.Slerp(_modelTransform.rotation, targetRotation,
                        _rotationSpeed * Time.deltaTime);
                }
            }
        }

        private void OnJump(bool performed)
        {
            if (Time.time - _lastGroundedTime <= _coyoteTime)
            {
                _lastJumpPressedTime = Time.time;
                _jumpReleased = !performed;
            }
        }

        private void HandleJump()
        {
            bool canJump = Time.time - _lastGroundedTime <= _coyoteTime;
            bool jumpBuffered = Time.time - _lastJumpPressedTime <= _jumpBufferTime;

            if (canJump && jumpBuffered)
            {
                _velocity.y = _jumpForce;
                _lastJumpPressedTime = -999f;
                _animator.SetBool(_isJumpingAnimHash, true);
            }
        }

        private void ApplyGravity()
        {
            if (_isGrounded && _velocity.y < 0f)
            {
                _velocity.y = -2f;
                return;
            }

            if (_velocity.y < 0)
            {
                _velocity.y += Physics.gravity.y * _fallGravityMultiplier * Time.deltaTime;
            }
            else
            {
                if (_jumpReleased) _velocity.y += Physics.gravity.y * _gravityMultiplier * Time.deltaTime;
                else _velocity.y += Physics.gravity.y * _lowJumpMultiplier * Time.deltaTime;
            }

            if (_velocity.y <= 0f)
            {
                _animator.SetBool(_isJumpingAnimHash, false);
            }
        }

        private void Move()
        {
            CollisionFlags flags = _controller.Move(_velocity * Time.deltaTime);

            if (!_isGrounded)
            {
                if ((flags & CollisionFlags.Above) != 0)
                {
                    _velocity.y = 0f;
                    _velocity.x = 0f;
                }

                if ((flags & CollisionFlags.Sides) != 0)
                {
                    _velocity.x = 0f;
                    _currentHorizontalSpeed = 0f;
                }
            }
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(
                _controller.transform.position.x,
                _controller.transform.position.y - _groundedOffset,
                _controller.transform.position.z
            );

            _isGrounded = Physics.CheckSphere(spherePosition, _radius, _groundLayerMask,
                QueryTriggerInteraction.Ignore);

            if (_isGrounded)
            {
                _lastGroundedTime = Time.time;
            }
        }

        private void UpdateAnimator()
        {
            _animator.SetFloat(_moveSpeedHash, _speed2D);
            _animator.SetInteger(_currentGaitHash, _currentGait);
            _animator.SetBool(_isGroundedHash, _isGrounded);
            _animator.SetFloat(_strafeDirectionXHash, _strafeDirectionX);
            _animator.SetFloat(_strafeDirectionZHash, _strafeDirectionZ);
            _animator.SetFloat(_isStrafingHash, 0f);
            _animator.SetBool(_isWalkingHash, _isWalking);
            _animator.SetBool(_isStoppedHash, _isStopped);
            _animator.SetBool(_movementInputHeldHash, _movementInputHeld);
        }

        private void OnDestroy()
        {
            _inputReader.onJumpPerformed -= OnJump;
        }

        public void KillZoneEntered()
        {
            _controller.enabled = false;
            transform.position = _initialPosition;
            _controller.enabled = true;

            _velocity = Vector3.zero;
            _moveDirection = Vector3.zero;
            _currentHorizontalSpeed = 0;
            _speed2D = 0f;
            
        }
    }
}
