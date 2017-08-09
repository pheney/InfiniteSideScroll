using System;
using UnityEngine;
using PLib.Interfaces;

namespace PLib.Interfaces
{
    /// <summary>
    /// 2016-5-18
    /// Messages that correspond to the interface methods.
    /// Using these prevents typos and hard-coded strings.
    /// </summary>
    public static partial class Message
    {
        public static string INPUT_AXIS = "OnInputAxisMessage";
    }
    public interface IOnInputAxis
    {
        void OnInputAxisMessage(Vector2 input);
    }
}
namespace PLib.Movement
{
    public enum Axis { X, Y, Z, XY, XZ, YZ, XYZ };
    public enum InputAxis { Horizontal, Vertical };
    public enum InputSmoothing { Linear, Square, Cube, Log };
    public enum MovementType { Kinematic, Physics };
    public enum UpdateMethod { Update, LateUpdate, FixedUpdate, None };

    public static class MovementExtensions
    {
        public static Vector3 ToVector3 (this Axis axis)
        {
            Vector3 result = Vector3.zero;
            switch (axis)
            {
                case Axis.X:
                    result = Vector3.right;
                    break;
                case Axis.Y:
                    result = Vector3.up;
                    break;
                case Axis.Z:
                    result = Vector3.forward;
                    break;
                case Axis.XY:
                    result = Vector3.right + Vector3.up;
                    break;
                case Axis.XZ:
                    result = Vector3.right + Vector3.forward;
                    break;
                case Axis.YZ:
                    result = Vector3.up + Vector3.forward;
                    break;
                case Axis.XYZ:
                    result = Vector3.one;
                    break;
                default:
                    break;
            }
            return result;
        }
    }

    [System.Serializable]
    public class MovementForce
    {
        public string forceName;
        public float magnitude;
        public ForceMode forceMode;
    }

    /// <summary>
    /// 2016-5-17
    /// Complete rewrite of the original globally namespaced
    /// OnMessageMove class.
    /// Accepts input from OnAxisInput message.
    /// Converts input into a single axis of movement.
    /// Input axis is selectable: horizontal or vertical
    /// Movement axis is selectable: X, Y, or Z
    /// Movement mode is selectable: kinematic or physics-based
    /// Kineamtic mode has input amplification and input smoothing.
    /// Phyics mode has force magnitude and force/acceleration/impulse-mode
    /// </summary>
    class OnInputAxisMessageMove : MonoBehaviour, IOnInputAxis
    {
        #region Inspector Assigned

        [Header("Inspector Assigned")]

        [Tooltip("The movement axis.")]
        public Axis movementAxis = Axis.X;

        [Tooltip("The input axis.")]
        public InputAxis inputAxis = InputAxis.Horizontal;

        [Tooltip("Smoothing to applying to"
            + " the input value.")]
        public InputSmoothing smoothing = InputSmoothing.Square;

        [Tooltip("Movement method")]
        public MovementType movementType = MovementType.Kinematic;

        [Tooltip("Required for Physics movement only.")]
        public MovementForce movementForce;

        [Tooltip("Required for Kinematic movement only.")]
        public MovementForce kinematicForce;

        [Tooltip("Update mode")]
        public UpdateMethod updateMethod = UpdateMethod.Update;


        #endregion
        #region Data

        private Transform _transform;
        private Rigidbody _rigidbody;
        private Vector3 movementVector;
        private float smoothExponent;
        private Vector2 inputVector;

        #endregion
        #region Unity API

        void Start()
        {
            _transform = GetComponent<Transform>();
            _rigidbody = GetComponent<Rigidbody>();

            if (movementType.Equals(MovementType.Physics)
                && _rigidbody == null)
            {
                string message = "Missing rigidbody for physics movement";
                throw new SystemException(message);
            }

            switch (movementAxis)
            {
                case Axis.X:
                    movementVector = Vector3.right;
                    break;
                case Axis.Y:
                    movementVector = Vector3.up;
                    break;
                case Axis.Z:
                    movementVector = Vector3.forward;
                    break;
                default:
                    string message = "Invalid Movement Axis " + movementAxis.ToString() + " for linear movement";
                    throw new SystemException(message);
            }

            switch (smoothing)
            {
                case InputSmoothing.Linear:
                    smoothExponent = 1;
                    break;
                case InputSmoothing.Square:
                    smoothExponent = 2;
                    break;
                case InputSmoothing.Cube:
                    smoothExponent = 3;
                    break;
                case InputSmoothing.Log:
                    smoothExponent = 0.5f;
                    break;
            }
        }

        void Update()
        {
            if (updateMethod.Equals(UpdateMethod.Update)) UpdateMovement();
        }

        void LateUpdate()
        {
            if (updateMethod.Equals(UpdateMethod.LateUpdate)) UpdateMovement();
        }

        void FixedUpdate()
        {
            if (updateMethod.Equals(UpdateMethod.FixedUpdate)
                || movementType.Equals(MovementType.Physics))
                UpdateMovement();
        }

        #endregion
        #region Internal Methods

        void UpdateMovement()
        {
            float raw = 0;
            switch (inputAxis)
            {
                case InputAxis.Horizontal:
                    raw = inputVector.x;
                    break;
                case InputAxis.Vertical:
                    raw = inputVector.y;
                    break;
            }

            float smooth = Mathf.Pow(raw, smoothExponent);
            switch (movementType)
            {
                case MovementType.Kinematic:
                    UpdateKinematic(smooth);
                    break;
                case MovementType.Physics:
                    UpdatePhysics(smooth, movementForce);
                    break;
            }
        }

        private void UpdateKinematic(float inputValue)
        {
            transform.Translate(movementVector * inputValue * Time.deltaTime, Space.Self);
        }

        private void UpdatePhysics(float inputValue, MovementForce force)
        {
            Vector3 direction = transform.TransformDirection(movementVector);
            _rigidbody.AddForce(inputValue * force.magnitude * direction * Time.fixedDeltaTime, force.forceMode);
        }

        #endregion
        #region Interfaces

        public void OnInputAxisMessage(Vector2 input)
        {
            this.inputVector = input;
        }

        #endregion
    }
}
