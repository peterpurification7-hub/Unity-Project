using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Interactions")]
		public float interactRange = 4.0f;
		[Header("Tower Abilities")]
		public bool canGlide = false;
		public bool canDoubleJump = false;
		[Header("Combat")]
		public bool canShoot = false;
		public float laserDamage = 1f;
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;
		public float mouseSensitivity = 1.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		private float _footstepTimer = 0f;
		[SerializeField] private float footstepSpeed = 0.5f;

		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;


#if ENABLE_INPUT_SYSTEM
		private PlayerInput _playerInput;
#endif
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;

		private bool IsCurrentDeviceMouse
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
			}
		}

		private void Awake()
		{
			// Reset abilities to 'False' for the new run
			canGlide = false;
			canDoubleJump = false;
			canShoot = false;

			// Ensure the main camera is found again
			if (_mainCamera == null) _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
		}

		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
			_playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError("Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
		}

		private void Update()
		{
			JumpAndGravity();
			GroundedCheck();
			Move();

			// --- RESTORE INTERACTION LOGIC ---
			if (Keyboard.current.eKey.wasPressedThisFrame)
			{
				CheckForInteraction();
			}
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		private void GroundedCheck()
		{
			bool wasGroundedBefore = Grounded;

			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

			if (Grounded)
			{
				// THE FIX: Stop the glide sound the EXACT frame your feet touch the grass
				if (AudioManager.instance != null && AudioManager.instance.glideSource.isPlaying)
				{
					AudioManager.instance.glideSource.Stop();
				}

				if (!wasGroundedBefore && _verticalVelocity < -5.0f)
				{
					if (AudioManager.instance != null)
						AudioManager.instance.PlaySFX(AudioManager.instance.landing);
				}
			}
		}

		private void CameraRotation()
		{
			if (_input.look.sqrMagnitude >= _threshold)
			{
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

				// The Fix: Ensure mouseSensitivity is used as a multiplier
				// If it freezes, check the Console for "Sensitivity is 0" errors!
				if (mouseSensitivity <= 0) mouseSensitivity = 0.1f;

				_cinemachineTargetPitch += _input.look.y * RotationSpeed * mouseSensitivity * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * mouseSensitivity * deltaTimeMultiplier;

				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move()
		{
			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			if (_input.move != Vector2.zero)
			{
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}

			// FIXED: Only call this ONCE per frame
			_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

			// --- FOOTSTEP LOGIC ---
			if (Grounded && _speed > 1.0f)
			{
				_footstepTimer -= Time.deltaTime;
				if (_footstepTimer <= 0)
				{
					float currentInterval = _input.sprint ? footstepSpeed * 0.6f : footstepSpeed;
					if (AudioManager.instance != null)
						AudioManager.instance.PlaySFX(AudioManager.instance.walking);

					_footstepTimer = currentInterval;
				}
			}
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// 1. Calculate the physics
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

					// 2. Play the sound
					if (AudioManager.instance != null)
						AudioManager.instance.PlaySFX(AudioManager.instance.jumping);

					// 3. THE FIX: Immediately set jump to false so it can't fire again next frame
					_input.jump = false;
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				_input.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				bool isSpaceHeld = Keyboard.current.spaceKey.isPressed;

				if (canGlide && !Grounded && _verticalVelocity < 0 && isSpaceHeld)
				{
					_verticalVelocity = -1.5f;

					// START the glide sound if it's not already playing
					if (AudioManager.instance != null && !AudioManager.instance.glideSource.isPlaying)
					{
						AudioManager.instance.glideSource.clip = AudioManager.instance.gliding;
						AudioManager.instance.glideSource.loop = true;
						AudioManager.instance.glideSource.Play();
					}
				}
				else
				{
					// STOP the glide sound if we let go of space or start moving up
					if (AudioManager.instance != null && AudioManager.instance.glideSource.isPlaying)
					{
						AudioManager.instance.glideSource.Stop();
					}
					_verticalVelocity += Gravity * Time.deltaTime;
				}
			}

		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}

		private void CheckForInteraction()
		{
			// Shoot a ray from the center of the camera
			Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
			RaycastHit hit;

			// LayerMask to hit everything EXCEPT the player
			int layerMask = ~LayerMask.GetMask("Player");

			if (Physics.Raycast(ray, out hit, interactRange, layerMask))
			{
				// Check if we hit a tower (Look for the script on the object or its parents)
				Tower tower = hit.collider.GetComponentInParent<Tower>();

				if (tower != null)
				{
					Debug.Log("Interacting with: " + hit.collider.name);
					tower.Interact(); // This triggers the tower activation!
				}
				else
				{
					Debug.Log("Hit " + hit.collider.name + " but it's not a tower.");
				}
			}
		}


	}


}