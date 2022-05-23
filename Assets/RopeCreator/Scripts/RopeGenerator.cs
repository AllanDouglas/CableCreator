using System;
using System.Collections.Generic;
using UnityEngine;

namespace RopeCreator
{
    public class RopePiece
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

    public class RopeObjectData
    {
        public readonly GameObject gameObject, firstPiece, lastPiece;
        public readonly RopePiece[] pieces;
        public Transform[] points { get; private set; }

        public RopeObjectData(GameObject gameObject,
            GameObject firstPiece, GameObject lastPiece,
            RopePiece[] pieces)
        {
            this.gameObject = gameObject;
            this.firstPiece = firstPiece;
            this.lastPiece = lastPiece;
            this.pieces = pieces;

            points = new Transform[pieces.Length];

            for (int i = 0; i < pieces.Length; i++)
            {
                points[i] = pieces[i].transform;
            }
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
                float mass,
                float distanceBetweenNodes = 1,
                float drag = 0,
                float spring = 100,
                float damper = 10,
                bool hasCollision = true,
                int layer = 0)
        {
            var ropeObject = new GameObject("Rope");
            ropeObject.layer = layer;

            var direction = (end - start).normalized;
            var currentPosition = start;
            var pieces = new List<RopePiece>();
            var isKinematic = true;
            HingeJoint lastHinge = null;

            var dot = 1f;

            var ropeResolution = distanceBetweenNodes * 2;

            while (Mathf.Approximately(dot, 1))
            {
                lastHinge = CreatePiece(
                    _radius: radius,
                    _mass: mass,
                    _ropeObject: ropeObject,
                    _direction: direction,
                    _currentPosition: ref currentPosition,
                    _pieces: pieces,
                    _lastHinge: lastHinge,
                    _isKinematic: isKinematic,
                    _createCollider: hasCollision,
                    _ropeResolution: ropeResolution,
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
                _lastHinge: lastHinge,
                _ropeResolution: ropeResolution);

            return Create(pieces: pieces.ToArray(),
                ropeObject: ropeObject,
                resolution: resolution,
                radius: radius,
                material: material);

            HingeJoint CreatePiece(
                float _radius,
                float _mass,
                GameObject _ropeObject,
                Vector3 _direction,
                ref Vector3 _currentPosition,
                List<RopePiece> _pieces,
                HingeJoint _lastHinge,
                bool _isKinematic = false,
                bool _createCollider = false,
                float _ropeResolution = 2,
                int _layer = 0)
            {
                var piece = new GameObject("Rope Piece");
                SphereCollider collider = null;
                piece.layer = _layer;
                piece.transform.parent = _ropeObject.transform;
                piece.transform.localPosition = _currentPosition;
                _currentPosition += _ropeResolution * _direction;
                var rb = piece.AddComponent<Rigidbody>();
                rb.drag = drag;
                rb.isKinematic = _isKinematic;
                rb.mass = _mass;

                var hinge = piece.AddComponent<HingeJoint>();
                var up = Quaternion.LookRotation(direction) * Vector3.up;
                var cross = Vector3.Cross(direction, up);

                hinge.axis = cross;
                hinge.enableCollision = false;
                hinge.enablePreprocessing = false;
                hinge.useLimits = true;
                hinge.useSpring = true;
                hinge.spring = new JointSpring()
                {
                    damper = damper,
                    spring = spring
                };

                if (_createCollider)
                {
                    collider = piece.AddComponent<SphereCollider>();
                    collider.radius = Mathf.Max(_radius, .25f);
                }

                if (_lastHinge)
                {
                    _lastHinge.connectedBody = rb;
                }
                _pieces.Add(new RopePiece(hinge, collider, rb));
                return hinge;
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

            var points = pieces;
            var mesh = RopeMeshGenerator.Generate(points, resolution, radius);

            var ropeData = new RopeObjectData(gameObject: ropeObject,
                                      firstPiece: pieces[0].gameObject,
                                      lastPiece: pieces[pieces.Length - 1].gameObject,
                                      pieces);

            meshRenderer.bones = ropeData.points;
            meshRenderer.sharedMesh = mesh;

            return ropeData;
        }

        public static object Create(Vector3 start, Vector3 end, int resolution, float radius, Material material, float distanceBetweenNodes, object mass, float drag, float spring, float damper, bool hasCollision, int layer)
        {
            throw new NotImplementedException();
        }
    }
}