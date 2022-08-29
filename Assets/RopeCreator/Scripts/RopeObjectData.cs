using UnityEngine;

namespace RopeCreator
{
    public sealed class RopeObjectData
    {
        public readonly GameObject gameObject;
        public readonly RopePiece[] pieces;
        public readonly Transform[] points;
        public RopePiece FirstPiece => pieces[0];
        public RopePiece LastPiece => pieces[pieces.Length - 1];

        public RopeObjectData(GameObject gameObject,
            RopePiece[] pieces)
        {
            this.gameObject = gameObject;
            this.pieces = pieces;

            points = new Transform[pieces.Length];

            for (int i = 0; i < pieces.Length; i++)
            {
                points[i] = pieces[i].transform;
            }
        }
    }
}