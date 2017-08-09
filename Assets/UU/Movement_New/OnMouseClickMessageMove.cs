using UnityEngine;
using UnityEngine.EventSystems;
using PLib.MouseInput;
using PLib.Rand;
using PLib.Math;

namespace PLib.MouseInput
{
    public class MouseClickData
    {
        public int buttonID;
        public MouseButtonState buttonEvent;
        public Vector2 mousePosition;
        public MouseClickData(int buttonID, MouseButtonState buttonEvent, Vector2 mousePosition)
        {
            this.buttonID = buttonID;
            this.buttonEvent = buttonEvent;
            this.mousePosition = mousePosition;
        }
    }
    /// <summary>
    /// 2016-5-18
    /// Messages that correspond to the interface methods.
    /// Using these prevents typos and hard-coded strings.
    /// </summary>
    public static partial class Message
    {
        public static string INPUT_MOUSE_BUTTON = "OnInputMouseButtonMessage";
    }
    public enum MouseButtonState { NONE, UP, DOWN, HOLD }
    public interface IOnInputMouseButton
    {
        void OnInputMouseButtonMessage(MouseClickData mouseClickEvent);
    }
}
namespace PLib.Movement
{
    public class OnMouseClickMessageMove : MonoBehaviour, IOnInputMouseButton
    {
        #region Inspector Assigned
        [Header("Inspector Assigned")]

        [Tooltip("(deg/sec) How fast to turn")]
        public float turnSpeed = 120;	//	deg/sec

        [Tooltip("(m/s) How fast to move")]
        public float moveSpeed = 5;		//	m/sec

        [Tooltip("(deg) Destination must be within this arc"
            + " before forward movement begins.")]
        public float maxAngleForMove = 60;		//	deg

        [Tooltip("Layer(s) on which destinations can be set")]
        public LayerMask moveLayer = 1;

        [Tooltip("(m) Distance to destination at which to stop.")]
        public float stopDistance = 0.2f;	//	meters

        [Header("Input Message Definition")]

        [Tooltip("Index of the button to respond to. 0 is Left"
            +" button, 1 is Right button, 2 is Middle button.")]
        public int buttonID = 0;

        [Tooltip("Button click event to respond to.")]
        public MouseButtonState moveOnButtonEvent;

        #endregion
        #region Data

        private Vector2 mousePosition;
        private Vector3 destination;
        private float cosAngle;
        private bool updateDestination;

        #endregion
        #region Unity API

        void Start()
        {
            //  Prevent "look direction is zero vector" and similar
            //  nonsense by ensuring the inital destination is 
            //  different from the starting position.
            destination = transform.position + PRand.OnCircle(0.1f).ToVector3();
            
            //  cache the calculation for the forward arc 
            cosAngle = Mathf.Cos(Mathf.Deg2Rad * maxAngleForMove);
        }

        void Update()
        {
            UpdateDestination();

            Vector3 moveDirection = destination - transform.position;
            Quaternion goalRotation = Quaternion.LookRotation(moveDirection);

            //  turn towards the destination
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goalRotation, turnSpeed * Time.deltaTime);

            //  abort move when destination is not within the required forward arc
            if (Vector3.Dot(transform.forward, moveDirection.normalized) < cosAngle) return;
            
            //  abort move when destination is close enough
            if (Vector3.SqrMagnitude(moveDirection) < stopDistance * stopDistance) return;

            //  move
            transform.Translate(Vector3.forward * Mathf.Min(moveSpeed * Time.deltaTime, stopDistance * stopDistance * moveDirection.magnitude));
        }

        #endregion
        #region Internal Methods

        /// <summary>
        /// Raycast from the provided mouse position.
        /// Collide against the required layers.
        /// Update the new destination.
        /// </summary>
        private void UpdateDestination()
        {
            if (!updateDestination) return; 
            RaycastHit info;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(mousePosition),
                                out info, Mathf.Infinity, moveLayer))
            {
                destination = info.point;
            }
        }

        #endregion
        #region Interfaces

        public void OnInputMouseButtonMessage(MouseClickData mouseClickEvent)
        {
            if (mouseClickEvent.buttonID != buttonID) return;
            if (!mouseClickEvent.buttonEvent.Equals(moveOnButtonEvent)) return;

            mousePosition = mouseClickEvent.mousePosition;
            updateDestination = true;
        }

        #endregion
    }
}