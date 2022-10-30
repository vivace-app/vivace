using System.Collections.Generic;
using UnityEngine;

namespace PlayScene
{
    public class CoordYPresever : MonoBehaviour
    {
        static List<int> eventLine = new List<int>();
        static List<float> y = new List<float>();

        public static void AddCoordY(float t, int lineNum)
        {
            y.Add(t);
            eventLine.Add(lineNum);
        }

        public static int isFlick(float t, int lineNum) // -1:フリック判定できない 1:上 2:下
        {
            int coordYIndex = eventLine.FindLastIndex(n => n == lineNum);
            if (coordYIndex == -1) return -1;
            if (t > y[coordYIndex]) return 1;
            else if (t < y[coordYIndex]) return 2;
            else return -1;
        }
    }
}