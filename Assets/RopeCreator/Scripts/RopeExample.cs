using System;
using UnityEngine;
namespace RopeCreator
{
    public class RopeExample : MonoBehaviour
    {
        [SerializeField] private float radius = .5f;
        [SerializeField] private Transform start;
        [SerializeField] private Transform end;
        [SerializeField] private float distanceBetweenNodes = 1;
        [SerializeField] private float drag = 0;
        [SerializeField] private float spring = 100;
        [SerializeField] private float damper = 1;
        [SerializeField] private bool hasCollision = false;
        [Range(2, 25), SerializeField] private int resolution = 10;
        [SerializeField] private Material material;

        private void Start()
        {
            RopeGenerator.Create(
                start: start.position,
                end: end.position,
                resolution: resolution,
                radius: radius,
                material: material,
                distanceBetweenNodes: distanceBetweenNodes,
                drag: drag,
                spring: spring,
                damper: damper,
                hasCollision: hasCollision);
        }

    }
}