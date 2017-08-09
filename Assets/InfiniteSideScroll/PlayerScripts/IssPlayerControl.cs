using UnityEngine;
using System.Collections;
using PLib;

namespace InfiniteSideScroll
{
    /// <summary>
    /// 2016-5-11
    ///	Infinite Side Scroll (ISS) player control.
    ///	Player controls:
    ///	    Left/Right -- passes input on, player doesn't actually move
    ///	    jump -- anytime player is on ground, has short (100 ms) cooldown
    ///	    double jump -- anytime player is in the air from a jump, limit 1
    ///	    glide -- anytime player is in the air (at all)
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    class IssPlayerControl : MonoBehaviour
    {
        #region Inspector Assigned

        [Tooltip("(ms) Duration of landing. During this time, another " +
            "Jump action cannot be initiated.")]
        public float landingTimeout = 150;

        [Tooltip("Use this key to jump, double jump and glide.")]
        public KeyCode jumpKey = KeyCode.Space;

        [Tooltip("(m/s) Jump force")]
        public float jumpForce = 60;
        public ForceMode jumpMode = ForceMode.Impulse;

        [Tooltip("Drag value when FALLING")]
        public float fallDrag = 0;

        [Tooltip("Drag value when GLIDING")]
        public float glideDrag = 8f;

        [Tooltip("m, drop distance at which state becomes FALLING")]
        public float dropDistance = 0.1f;

        #endregion
        #region Data

        private enum MoveState
        {
            GROUNDED, JUMPING, DOUBLE_JUMPING,
            FALLING, GLIDING, LANDING
        }

        //	serialized for debugging
        [SerializeField]
        private MoveState moveState;

        //	serialied for debugging
        [SerializeField]
        private float landingCompleteTime;
        private Rigidbody _rigidbody;
        private float defaultDrag;	//	15 is a good number

        #endregion
        #region Unity API

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            defaultDrag = _rigidbody.drag;
        }

        void Update()
        {
            UpdateInput();

            //  When left/right pushed: pass on command
        }

        void FixedUpdate()
        {
            UpdateStateRecovery();
            switch (moveState)
            {
                case MoveState.FALLING:
                case MoveState.LANDING:
                case MoveState.GROUNDED:
                    _rigidbody.AddForce(Vector3.down * jumpForce * Time.deltaTime, jumpMode);
                    break;
            }
        }

        #endregion
        #region Internal methods

        private void JumpImmediate()
        {
            Vector3 velocity = _rigidbody.velocity;
            velocity.y = 0;
            _rigidbody.velocity = velocity;
            _rigidbody.AddForce(Vector3.up * jumpForce, jumpMode);
        }

        /// <summary>
        /// User input can put the player into Jump, Double Jump,
        /// and Glide states.
        /// </summary>
        private void UpdateInput()
        {
            if (Input.GetKeyDown(jumpKey))
            {
                if (CheckEnterJumpState())
                {
                    moveState = MoveState.JUMPING;
                    JumpImmediate();
                    return;
                }

                if (CheckEnterDoubleJumpState())
                {
                    moveState = MoveState.DOUBLE_JUMPING;
                    JumpImmediate();
                    return;
                }
            }

            if (Input.GetKey(jumpKey))
            {
                if (CheckEnterGlideState())
                {
                    moveState = MoveState.GLIDING;
                    _rigidbody.drag = glideDrag;
                    return;
                }
            }

            if (Input.GetKeyUp(jumpKey))
            {
                if (moveState.Equals(MoveState.GLIDING))
                {
                    moveState = MoveState.FALLING;
                    _rigidbody.drag = fallDrag;
                    return;
                }
            }
        }

        /// <summary>
        /// The player mvoes into Falling, Landing, and Grounded
        /// states automatically when certain conditions are met.
        /// </summary>
        private void UpdateStateRecovery()
        {
            if (CheckEnterFallState())
            {
                moveState = MoveState.FALLING;
                _rigidbody.drag = fallDrag;
                return;
            }

            if (CheckEnterLandState())
            {
                moveState = MoveState.LANDING;
                landingCompleteTime = Time.time + landingTimeout / 1000;
                _rigidbody.drag = defaultDrag;
                return;
            }

            if (CheckEnterGroundState())
            {
                moveState = MoveState.GROUNDED;
                return;
            }
        }

        /// <summary>
        /// Player can enter Jump state any time he is in
        /// Grounded state.
        /// </summary>
        private bool CheckEnterJumpState()
        {
            return moveState.Equals(MoveState.GROUNDED);
        }

        /// <summary>
        /// Player can enter Double Jump anytime he is in
        /// the Jump state.
        /// </summary>
        private bool CheckEnterDoubleJumpState()
        {
            return moveState.Equals(MoveState.JUMPING);
        }

        /// <summary>
        /// Player can enter Glide state anytime during
        /// the Falling state.
        /// </summary>
        private bool CheckEnterGlideState()
        {
            return moveState.Equals(MoveState.FALLING);
        }

        /// <summary>
        /// Player enters Landing state anytime the player is
        /// in Falling or Gliding state and velocity becomes zero.
        /// </summary>
        private bool CheckEnterLandState()
        {
            bool isFalling = moveState.Equals(MoveState.FALLING)
                || moveState.Equals(MoveState.GLIDING);

            bool acceptableVelocity = Mathf.Approximately(_rigidbody.velocity.y, 0)
                || _rigidbody.velocity.y > 0;

            return isFalling && acceptableVelocity;
        }

        /// <summary>
        /// Enter Grounded state anytime the player is
        /// in Landing state and the jumpReadyTimer elapses.
        /// </summary>
        private bool CheckEnterGroundState()
        {
            return moveState.Equals(MoveState.LANDING)
                && Time.time > landingCompleteTime;
        }

        /// <summary>
        /// Enter Falling state anytime velocity is negative
        /// and player is not already gliding
        /// </summary>
        private bool CheckEnterFallState()
        {
            if (moveState.Equals(MoveState.GLIDING)) return false;

            float distance = Mathf.Ceil(transform.position.y) - transform.position.y;
            bool acceptableFallDistance = distance > dropDistance;
            bool acceptableVelocity = _rigidbody.velocity.y < 0;

            return acceptableFallDistance && acceptableVelocity;
        }

        #endregion
        #region Debugging / development



        #endregion
    }
}
