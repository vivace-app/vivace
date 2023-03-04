using System;
using Project.Scripts.Tools;

// ReSharper disable InconsistentNaming

namespace Project.Scripts.Model
{
    [Serializable]
    public class Note
    {
        /* LPB */
        public int LPB;

        /* 先頭からの拍数 */
        public int num;

        /* レーン番号（0~6） */
        public int block;

        /*
         * ノーツの種類
         * 1: 通常ノーツ
         * 2: ロングノーツ
         * 5: フリックノーツ
         */
        public int type;

        /* 子ノーツ */
        public Note[] notes;

        public override string ToString() => this.ToStringReflection();
    }
}