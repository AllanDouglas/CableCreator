using System.Collections.Generic;
using UnityEngine;

namespace RopeCreator
{
    public struct RopeObjectData
    {
        public readonly GameObject gameObject, firstPiece, lastPiece;

        public readonly List<Transform> pieces;

        public RopeObjectData(GameObject gameObject,
            GameObject firstPiece, GameObject lastPiece,
            List<Transform> pieces)
        {
            this.gameObject = gameObject;
            this.firstPiece = firstPiece;
            this.lastPiece = lastPiece;
            this.pieces = pieces;
        }
    }

    public static class RopeGenerator
    {
        public static RopeObjectData Create(
                Vector3 start,
                Vector3 end,
                int resolution,
                float radius,
                Material material,
                float distanceBetweenNodes = 1,
                float drag = 0,
                float spring = 100,
                float damper = 10,
                bool hasCollision = true)
        {
            var ropeObject = new GameObject("Rope");

            var direction = (end - start).normalized;
            var currentPosition = start;
            var pieces = new List<Transform>();
            var isKinematic = true;
            HingeJoint lastHinge = null;

            var dot = 1f;

            var ropeResolution = distanceBetweenNodes * 2;

            while (Mathf.Approximately(dot, 1))
            {
                lastHinge = CreatePiece(
                    _radius: radius,
                    _ropeObject: ropeObject,
                    _direction: direction,
                    _currentPosition: ref currentPosition,
                    _pieces: pieces,
                    _lastHinge: lastHinge,
                    _isKinematic: isKinematic,
                    _createCollider: hasCollision,
                    _ropeResolution: ropeResolution);

                isKinematic = false;

                dot = Vector3.Dot(
                    (end - currentPosition).normalized,
                    direction
                );
            }

            currentPosition = end;

            CreatePiece(
                _radius: radius,
                _ropeObject: ropeObject,
                _direction: direction,
                _currentPosition: ref currentPosition,
                _pieces: pieces,
                _lastHinge: lastHinge,
                _ropeResolution: ropeResolution);

            var meshRenderer = ropeObject.AddComponent<SkinnedMeshRenderer>();
            meshRenderer.sharedMaterial = material;

            var points = pieces.ToArray();
            var mesh = RopeMeshGenerator.Generate(points, resolution, radius);

            meshRenderer.bones = points;
            meshRenderer.sharedMesh = mesh;

            return new RopeObjectData(gameObject: ropeObject,
                                      firstPiece: pieces[0].gameObject,
                                      lastPiece: pieces[pieces.Count - 1].gameObject,
                                      pieces);

            HingeJoint CreatePiece(float _radius, GameObject _ropeObject,
                Vector3 _direction, ref Vector3 _currentPosition,
                List<Transform> _pieces, HingeJoint _lastHinge,
                bool _isKinematic = false, bool _createCollider = false, float _ropeResolution = 2)
            {
                var piece = new GameObject("Rope Piece");
                piece.transform.parent = _ropeObject.transform;
                piece.transform.localPosition = _currentPosition;
                _currentPosition += _radius * _ropeResolution * _direction;
                _pieces.Add(piece.transform);
                var rb = piece.AddComponent<Rigidbody>();
                rb.drag = drag;
                rb.isKinematic = _isKinematic;

                var hinge = piece.AddComponent<HingeJoint>();
                var up = Quaternion.LookRotation(direction) * Vector3.up;
                var cross = Vector3.Cross(direction, up);
                
                hinge.axis = cross;
                hinge.enableCollision = false;
                hinge.enablePreprocessing = false;

                hinge.useSpring = true;
                hinge.spring = new JointSpring()
                {
                    damper = damper,
                    spring = spring
                };

                if (_createCollider)
                {
                    var collider = piece.AddComponent<SphereCollider>();
                    collider.radius = _radius;
                    collider.center += Vector3.forward * _radius;

                    Collider collider2 = default;

                    if (_lastHinge?.TryGetComponent(out collider2) ?? false)
                    {
                        Physics.IgnoreCollision(collider, collider2, true);
                    }
                }

                if (_lastHinge)
                {
                    _lastHinge.connectedBody = rb;
                }

                return hinge;
            }
        }
    }
}