using System.Collections.Generic;
using UnityEngine;

namespace Checkers.Data
{
    public class Move
    {
        // Coordinates are represented as a single byte to save space.
        // The first four bits are the row, the last four bits are the column.
        // i.e. 
        //             row-col-
        // Coordinate: xxxxxxxx
        // coordinate >> 4 = row, coordinate & 0x0F = col
        // This format supports board sizes up to 16x16.
        // The Move class has static methods to convert coordinates to byte representations
        // and byte representation to sets of ints.
        public byte start;
        public byte end;

        public bool isCapture;
        public List<byte> captures = new();

        public Move(byte start, byte end)
        {
            this.start = start;
            this.end = end;
        }

        public GameObject CreateFinalPositionMarker()
        {
            GameObject obj = MonoBehaviour.Instantiate(GameManager.Marker, Utils.ByteToWorldSpace(end, GameManager.Spacing), Quaternion.identity);
            obj.GetComponent<MoveMarker>().move = this;

            return obj;
        }
    }
}