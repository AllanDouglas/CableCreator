using UnityEngine;

namespace RopeCreator
{
    public sealed class RopePiece
    {
        public readonly HingeJoint joint;
        public readonly Collider collider;
        public readonly Rigidbody rigidbody;
        public Vector3 position => rigidbody.position;
        public Transform transform => rigidbody.transform;
        public GameObject gameObject => rigidbody.gameObject;

        public RopePiece(HingeJoint joint, Collider collider, Rigidbody rigidbody)
        {
            this.joint = joint;
            this.collider = collider;
            this.rigidbody = rigidbody;
        }
    }
}