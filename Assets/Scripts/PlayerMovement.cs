using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts {
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 7;
        [SerializeField] private float _moveAcceleration = 14;
        [SerializeField] private float _moveDeacceleration = 10;
        [SerializeField] private float _airAcceleration = 2;
        [SerializeField] private float _airDeacceleration = 2;
        [SerializeField] private float _jumpSpeed = 8;

        [Header("Camera Settings")]
        [SerializeField] private float _xMouseSensitivity = 30;
        [SerializeField] private float _yMouseSensitivity = 30;
        [SerializeField] private float _upDownClamp = 80;

        [Header("World Settings")]
        [SerializeField] private float _gravity = 20;
        [SerializeField] private float _friction = 6;

        [Header("References")]
        [SerializeField] private TransformVariable _transform = null;
        [SerializeField] private BoolVariable _playerHasControl = null;

        private CharacterController _controller;

        private float _forwardMove;
        private float _rightMove;
        private float _rotX;
        private float _rotY;

        private Vector3 _playerVelocity = Vector3.zero;

        private bool _wishJump;

        private void Awake()
        {
            if (_transform == null) Debug.Log("[" + GetType().Name + "] Transform Variable missing on " + name);
            if (_playerHasControl == null) Debug.Log("[" + GetType().Name + "] Player Has Control Bool Variable missing on " + name);
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            if (_transform != null) _transform.Position = transform.position;
        }

        private void Update()
        {
            if (_playerHasControl != null) {
                if (!_playerHasControl.Value) return;
            }

            _rotX -= Input.GetAxisRaw("Mouse Y") * _xMouseSensitivity * 0.02f;
            _rotY += Input.GetAxisRaw("Mouse X") * _yMouseSensitivity * 0.02f;

            _rotX = Mathf.Clamp(_rotX, -_upDownClamp, _upDownClamp);

            transform.rotation = Quaternion.Euler(0, _rotY, 0);
            if (_transform != null) {
                _transform.Rotation = Quaternion.Euler(_rotX, _rotY, 0);
                _transform.Forward = transform.forward;
            }

            SetMovementDirection();
            QueueJump();
            if (_controller.isGrounded) {
                GroundMove();
            } else if (!_controller.isGrounded) {
                AirMove();
            }
            _controller.Move(_playerVelocity * Time.deltaTime);

            if (_transform != null) _transform.Position = transform.position;
        }

        private void SetMovementDirection()
        {
            _forwardMove = Input.GetAxisRaw("Vertical");
            _rightMove = Input.GetAxisRaw("Horizontal");
        }

        private void QueueJump()
        {
            if (Input.GetButtonDown("Jump") && !_wishJump) _wishJump = true;
            if (Input.GetButtonUp("Jump")) _wishJump = false;
        }

        private void GroundMove()
        {
            ApplyFriction(_wishJump ? 0 : 1.0f);

            var wishDir = new Vector3(_rightMove, 0, _forwardMove);
            wishDir = transform.TransformDirection(wishDir);
            wishDir.Normalize();

            var wishSpeed = wishDir.magnitude;
            wishSpeed *= _moveSpeed;

            Accelerate(wishDir, wishSpeed, _moveAcceleration);

            _playerVelocity.y = -_gravity * Time.deltaTime;

            if (_wishJump) {
                _playerVelocity.y = _jumpSpeed;
                _wishJump = false;
            }
        }

        private void AirMove()
        {
            Vector3 wishDir = new Vector3(0, 0, _forwardMove);
            wishDir = transform.TransformDirection(wishDir);

            float wishSpeed = wishDir.magnitude;
            wishDir.Normalize();

            wishSpeed *= _moveSpeed;

            float accel = Vector3.Dot(_playerVelocity, wishDir) < 0 ? _airDeacceleration : _airAcceleration;

            Accelerate(wishDir, wishSpeed, accel);

            _playerVelocity.y -= _gravity * Time.deltaTime;
        }

        private void ApplyFriction(float t)
        {
            Vector3 vec = _playerVelocity;

            vec.y = 0.0f;
            float speed = vec.magnitude;
            float drop = 0.0f;

            if (_controller.isGrounded) {
                float control = speed < _moveDeacceleration ? _moveDeacceleration : speed;
                drop = control * _friction * Time.deltaTime * t;
            }

            float newSpeed = speed - drop;
            if (newSpeed < 0)
                newSpeed = 0;
            if (speed > 0)
                newSpeed /= speed;

            _playerVelocity.x *= newSpeed;
            _playerVelocity.z *= newSpeed;
        }

        private void Accelerate(Vector3 wishDir, float wishSpeed, float accel)
        {
            float currentSpeed = Vector3.Dot(_playerVelocity, wishDir);
            float addSpeed = wishSpeed - currentSpeed;
            if (addSpeed <= 0) return;
            float accelSpeed = accel * Time.deltaTime * wishSpeed;
            if (accelSpeed > addSpeed) accelSpeed = addSpeed;

            _playerVelocity.x += accelSpeed * wishDir.x;
            _playerVelocity.z += accelSpeed * wishDir.z;

            if (_playerVelocity.magnitude > _moveSpeed + 2) {
                float ySpeed = _playerVelocity.y;
                _playerVelocity.Normalize();
                _playerVelocity.x *= _moveSpeed + 2;
                _playerVelocity.y = ySpeed;
                _playerVelocity.z *= _moveSpeed + 2;
            }
        }
    }
}