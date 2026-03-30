using System;
using System.Collections;
using UnityEngine;

namespace Checkers
{
    public class Utils
    {
        public static IEnumerator LerpAsync(float a, float b, float time, Action<float> callback)
        {
            float startTime = Time.time;
            float t = 0;

            while (t < 1)
            {
                t = (Time.time - startTime) / time;
                callback(a + t * (b - a));

                yield return null;
            }

            callback(b);
        }

        public static Vector3 ByteToWorldSpace(byte coordinate, float spaceSize, Vector3 offset = new())
        {
            int[] coords = ByteToInts(coordinate);

            Vector3 position = new(
                coords[1] * spaceSize,
                0,
                coords[0] * spaceSize
            );

            return position + offset;
        }

        // Takes row and column as parameters
        public static byte IntsToByte(int r, int c)
        {
            if (r > 0x0F || c > 0x0F)
                throw new InvalidMoveException();

            byte output = 0x00;
            output |= (byte)(r << 4);
            output |= (byte)c;

            return output;
        }

        public static int[] ByteToInts(byte coordinate)
        {
            int[] output = new int[2];
            output[0] = coordinate >> 4;
            output[1] = coordinate & 0x0F;

            return output;
        }

        public static bool IsKing(char piece)
        {
            return (piece & 0b01) != 0;
        }

        public static bool IsOpponent(char a, char b)
        {
            return ((a & 0b10) ^ (b & 0b10)) != 0;
        }

        public static bool IsRed(char piece)
        {
            return (piece & 0b10) != 0;
        }

        public static bool IsBlack(char piece)
        {
            return (piece & 0b10) == 0;
        }

        public static int[] AddAll(int[] a, int[] b)
        {
            int l = Math.Min(a.Length, b.Length);
            int[] output = new int[l];

            for (int i = 0; i < l; i++)
            {
                output[i] = a[i] + b[i];
            }

            return output;
        }
    }
}