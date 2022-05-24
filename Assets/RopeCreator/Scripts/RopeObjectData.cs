using UnityEngine;

namespace RopeCreator
{
    public sealed class RopeObjectData
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
}