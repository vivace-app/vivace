using System.Collections.Generic;
using UnityEngine;

namespace PlayScene
{
    public class CoordYPreserver : MonoBehaviour
    {
        private static readonly List<int> EventLine = new();
        private static readonly List<float> Y = new();

        public static void AddCoordY(float positionY, int lineNum)
        {
            Y.Add(positionY);
            EventLine.Add(lineNum);
        }

        public static bool IsFlick(float positionY, int lineNum)
        {
            var coordYIndex = EventLine.FindLastIndex(n => n == lineNum);
            if (coordYIndex == -1) return false;

            return positionY > Y[coordYIndex]; // 上フリック
        }
    }
}