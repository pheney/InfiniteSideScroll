using UnityEngine;

namespace PLib.Movement
{
    public class MatchAxis : MonoBehaviour
    {
        public Transform matchTarget;
        public Axis matchAxis;
        private Vector3 _matchAxis;
        private Vector3 _maskAxis;
        private Transform _transform;

        void Awake ()
        {
            _transform = transform;
            _matchAxis = matchAxis.ToVector3();
            _maskAxis = Vector3.one - _matchAxis;
        }

        void LateUpdate ()
        {
            if (!matchTarget) return;
            Vector3 targetPosition = matchTarget.position;
            Vector3 sourcePosition = _transform.position;
            targetPosition.Scale(_matchAxis);
            sourcePosition.Scale(_maskAxis);
            sourcePosition += targetPosition;
            _transform.position = sourcePosition;
        }
    }
}