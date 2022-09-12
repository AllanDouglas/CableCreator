using System.Collections.Generic;
using UnityEngine;

namespace RopeCreator
{
    public static class RopeGenerator
    {
        public static RopeObjectData Create(
                Vector3 start,
                Vector3 end,
                int resolution,
                float radius,
                Material material,
                float mass,
                float distanceBetweenNodes = 1,
                float drag = 0,
                float angularDrag = 0,
                float spring = 100,
                float damper = 10,
                RopeCollisionMode collisionMode = RopeCollisionMode.COLLIDER,
                int layer = 0)
        {
            var ropeObject = new GameObject("Rope");
            ropeObject.layer = layer;

            var direction = (end - start).normalized;
            var currentPosition = start;
            var pieces = new List<RopePiece>();
            var isKinematic = true;
            RopePiece ropePiece = null;

            var dot = 1f;


            while (Mathf.Approximately(dot, 1))
            {
                ropePiece = CreatePiece(
                    _radius: radius,
                    _mass: mass,
                    _ropeObject: ropeObject,
                    _direction: direction,
                    _currentPosition: ref currentPosition,
                    _pieces: pieces,
                    _lastRopePiece: ropePiece,
                    _isKinematic: isKinematic,
                    _collisionMode: collisionMode,
                    _ropeResolution: distanceBetweenNodes,
                    _layer: layer);

                isKinematic = false;

                dot = Vector3.Dot(
                    (end - currentPosition).normalized,
                    direction
                );
            }

            currentPosition = end;

            CreatePiece(
                _radius: radius,
                _mass: mass,
                _ropeObject: ropeObject,
                _direction: direction,
                _currentPosition: ref currentPosition,
                _pieces: pieces,
                _lastRopePiece: ropePiece,
                _collisionMode: collisionMode,
                _ropeResolution: distanceBetweenNodes,
                _createJoint: false);

            return Create(pieces: pieces.ToArray(),
                ropeObject: ropeObject,
                resolution: resolution,
                radius: radius,
                material: material);

            RopePiece CreatePiece(
                float _radius,
                float _mass,
                GameObject _ropeObject,
                Vector3 _direction,
                ref Vector3 _currentPosition,
                List<RopePiece> _pieces,
                RopePiece _lastRopePiece,
                bool _isKinematic = false,
                RopeCollisionMode _collisionMode = RopeCollisionMode.NONE,
                float _ropeResolution = 2,
                int _layer = 0,
                bool _createJoint = true)
            {
                var piece = new GameObject("Rope Piece");
                SphereCollider collider = null;
                piece.layer = _layer;
                piece.transform.parent = _ropeObject.transform;
                piece.transform.localPosition = _currentPosition;
                _currentPosition += _ropeResolution * _direction;
                var rb = piece.AddComponent<Rigidbody>();
                rb.drag = drag;
                rb.angularDrag = angularDrag;
                rb.isKinematic = _isKinematic;
                rb.mass = _mass;

                var up = Quaternion.LookRotation(direction) * Vector3.up;
                var cross = Vector3.Cross(direction, up);

                if (_collisionMode.HasFlag(RopeCollisionMode.COLLIDER) || _collisionMode.HasFlag(RopeCollisionMode.TRIGGER))
                {
                    collider = piece.AddComponent<SphereCollider>();
                    collider.radius = _radius;
                    collider.isTrigger = _collisionMode.HasFlag(RopeCollisionMode.TRIGGER);
                }

                if (_lastRopePiece != null)
                {
                    _lastRopePiece.joint.connectedBody = rb;

                    AddJoint(rb.gameObject).connectedBody = _lastRopePiece.rigidbody;
                }

                RopePiece ropePiece = new RopePiece(_createJoint ? AddJoint(piece) : null, collider, rb);
                _pieces.Add(ropePiece);
                return ropePiece;

                Joint AddJoint(GameObject go)
                {
                    var joint = go.AddComponent<ConfigurableJoint>();
                    joint.axis = cross;
                    joint.enableCollision = false;
                    joint.enablePreprocessing = false;
                    joint.xMotion = ConfigurableJointMotion.Limited;
                    joint.yMotion = ConfigurableJointMotion.Limited;
                    joint.zMotion = ConfigurableJointMotion.Limited;
                    joint.angularXMotion = ConfigurableJointMotion.Limited;
                    joint.angularYMotion = ConfigurableJointMotion.Limited;
                    joint.angularZMotion = ConfigurableJointMotion.Limited;

                    joint.angularYZDrive = new JointDrive() { maximumForce = 999, positionSpring = spring, positionDamper = damper };
                    joint.angularXDrive = new JointDrive() { maximumForce = 999, positionSpring = spring, positionDamper = damper };

                    return joint;

                }
            }
        }

        public static RopeObjectData Create(
            RopePiece[] pieces,
            GameObject ropeObject,
            int resolution,
            float radius,
            Material material)
        {
            var meshRenderer = ropeObject.AddComponent<SkinnedMeshRenderer>();
            meshRenderer.sharedMaterial = material;

            var mesh = RopeMeshGenerator.Generate(pieces, resolution, radius);

            var ropeData = new RopeObjectData(gameObject: ropeObject, pieces);

            meshRenderer.bones = ropeData.points;
            meshRenderer.sharedMesh = mesh;

            return ropeData;
        }
    }
}