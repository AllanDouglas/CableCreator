using System;
using UnityEngine;
namespace RopeCreator
{
    public class RopeExample : MonoBehaviour
    {
        [SerializeField] private string layer;
        [SerializeField] private float radius = .5f;
        [SerializeField] private Transform start;
        [SerializeField] private Transform end;
        [SerializeField] private float distanceBetweenNodes = 1;
        [SerializeField] private float mass = 1;
        [SerializeField] private float drag = 0;
        [SerializeField] private float spring = 100;
        [SerializeField] private float damper = 1;
        [SerializeField] private RopeCollisionMode collisionMode = RopeCollisionMode.NONE;
        [Range(2, 25), SerializeField] private int resolution = 10;
        [SerializeField] private Material material;
        [Header("Rigidbody Constraints")]
        [SerializeField] private bool lockXPosition;
        [SerializeField] private bool lockYPosition;
        [SerializeField] private bool lockZPosition;
        [SerializeField] private bool lockXRotation;
        [SerializeField] private bool lockYRotation;
        [SerializeField] private bool lockZRotation;

        private void Start()
        {
            var data = RopeGenerator.Create(
                start: start.position,
                end: end.position,
                resolution: resolution,
                radius: radius,
                material: material,
                distanceBetweenNodes: distanceBetweenNodes,
                mass: mass,
                drag: drag,
                spring: spring,
                damper: damper,
                collisionMode: collisionMode,
                layer: LayerMask.NameToLayer(layer));

            foreach (var piece in data.pieces)
            {
                if (lockXPosition)
                    piece.rigidbody.constraints |= RigidbodyConstraints.FreezePositionX;
                if (lockYPosition)
                    piece.rigidbody.constraints |= RigidbodyConstraints.FreezePositionY;
                if (lockZPosition)
                    piece.rigidbody.constraints |= RigidbodyConstraints.FreezePositionZ;
                if (lockXRotation)
                    piece.rigidbody.constraints |= RigidbodyConstraints.FreezeRotationX;
                if (lockYRotation)
                    piece.rigidbody.constraints |= RigidbodyConstraints.FreezeRotationY;
                if (lockZRotation)
                    piece.rigidbody.constraints |= RigidbodyConstraints.FreezeRotationZ;
            }
        }

    }
}