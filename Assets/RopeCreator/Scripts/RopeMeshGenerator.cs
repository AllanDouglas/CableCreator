using System;
using UnityEngine;

namespace RopeCreator
{

    public class RopeMeshGenerator
    {
        public static Mesh Generate(Transform[] points, int resolution, float radius)
        {
            var connectionTriangles = resolution * 6;
            var trianglesByResolution = resolution * 3;
            var trianglesArrayLength = (Mathf.Max(points.Length, 2) * trianglesByResolution) + (points.Length - 1) * connectionTriangles;
            var trianglesArray = new int[trianglesArrayLength];
            var verticesArray = new Vector3[(points.Length * resolution) + points.Length];
            var verticeStartsAt = 0;
            var triangleStartAt = 0;

            Node lastNode = default;

            for (int i = 0; i < points.Length; i++)
            {
                var node = new Node(points[i], verticesArray, resolution, verticeStartsAt);

                FillVertices(resolution, radius, node);
                if (i > 0)
                {
                    if (i == 1)
                    {
                        lastNode.GetTriangles(trianglesArray, ref triangleStartAt);
                    }
                    lastNode.GetTrianglesTo(node, trianglesArray, ref triangleStartAt);
                    if (i == points.Length - 1)
                    {
                        node.GetTriangles(trianglesArray, ref triangleStartAt);
                    }
                }

                verticeStartsAt += node.Size;
                lastNode = node;
            }

            var weights = new BoneWeight[verticesArray.Length];
            var bindPoses = new Matrix4x4[points.Length];

            FillBones(points, resolution, weights, bindPoses);

            var uv = new Vector2[verticesArray.Length];
            for (int i = 0; i < uv.Length; i++)
            {
                uv[i] = verticesArray[i];
            }

            var mesh = new Mesh
            {
                vertices = verticesArray,
                triangles = trianglesArray,
                bindposes = bindPoses,
                boneWeights = weights,
                uv = uv
            };

            mesh.RecalculateNormals();
            return mesh;

            int FillBones(Transform[] _points, int _resolution, BoneWeight[] _weights, Matrix4x4[] _bindPoses)
            {
                var index = 0;
                for (var i = 0; i < _points.Length; i++)
                {
                    _bindPoses[i] = _points[i].worldToLocalMatrix; //* obj.transform.localToWorldMatrix;

                    for (var j = 0; j <= _resolution; j++)
                    {
                        _weights[index].boneIndex0 = i;
                        _weights[index].weight0 = 1;
                        index++;
                    }
                }

                return index;
            }
        }

        static void FillVertices(int resolution, float radius, Node vertice)
        {
            for (int i = 0; i < resolution; i++)
            {
                var currentAngle = 2f * Mathf.PI / resolution * i;

                var cos = Mathf.Cos(currentAngle);
                var sin = Mathf.Sin(currentAngle);

                vertice.Add(new Vector3(
                    cos * radius,
                    sin * radius,
                    0
                ) + vertice.Center);
            }
        }

        private struct Node
        {

            private Vector3[] vertices;

            private int currentPosition;
            public Transform Bone { get; private set; }

            private int amount;

            public int StartAt { get; }
            public Vector3 Center => Bone.position;
            public int Size => amount + 1;

            public Node(Transform bone, Vector3[] vectors, int amount, int startAt) : this(amount, bone, startAt)
            {
                vertices = vectors;
                Add(bone.localPosition);
            }

            public Node(Transform bone, int amount, int startAt) : this(amount, bone, startAt)
            {
                vertices = new Vector3[Size];
                Add(bone.localPosition);
            }

            private Node(int amount, Transform bone, int startAt = 0)
            {
                Bone = bone;
                this.amount = amount;
                currentPosition = startAt;
                StartAt = startAt;
                this.vertices = default;
            }

            public void Add(Vector3 point) => vertices[currentPosition++] = point;
            public int IndexOf(Vector3 point) => Array.IndexOf(vertices, point);
            public Vector3 PointAt(int index) => vertices[index];
            public Triangle GetTriangle(Vector3 point) => GetTriangle(IndexOf(point));
            public Triangle GetTriangle(int index) => new Triangle(StartAt, index, index < StartAt + amount ? index + 1 : StartAt + 1);
            public Vector3[] ToArray() => vertices;
            public int[] GetTriangles(int[] bucket, ref int startAt)
            {

                var triangles = bucket;
                var counter = new Counter(StartAt);

                for (int i = 0; i < amount; i++)
                {
                    var triangle = GetTriangle(counter.Next());

                    triangles[startAt++] = triangle.right;
                    triangles[startAt++] = triangle.left;
                    triangles[startAt++] = triangle.center;
                }

                return triangles;
            }

            public int[] GetTrianglesTo(Node otherNode, int[] trianglesArray, ref int startTrianglesIndex)
            {
                var thisCounter = new Counter(StartAt);
                var otherCounter = new Counter(otherNode.StartAt);

                for (int i = 0; i < this.amount; i++)
                {
                    var thisTriangle = this.GetTriangle(thisCounter.Next());
                    var otherTriangle = otherNode.GetTriangle(otherCounter.Next());

                    trianglesArray[startTrianglesIndex++] = thisTriangle.right;
                    trianglesArray[startTrianglesIndex++] = otherTriangle.right;
                    trianglesArray[startTrianglesIndex++] = thisTriangle.left;

                    trianglesArray[startTrianglesIndex++] = otherTriangle.right;
                    trianglesArray[startTrianglesIndex++] = otherTriangle.left;
                    trianglesArray[startTrianglesIndex++] = thisTriangle.left;
                }

                return trianglesArray;
            }
        }

        private struct Counter
        {
            public readonly int start;
            public int Current { get; private set; }
            public int Iterations { get; private set; }

            public Counter(int start)
            {
                this.start = start;
                Current = start;
                Iterations = 0;
            }

            public int Next()
            {
                Iterations++;
                Current++;
                return Current;
            }
        }

        private struct Triangle
        {
            public readonly int center;
            public readonly int left;
            public readonly int right;

            public Triangle(int center, int left, int right)
            {
                this.center = center;
                this.left = left;
                this.right = right;
            }
        }

    }

}
