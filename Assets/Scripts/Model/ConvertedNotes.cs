using System;
using JetBrains.Annotations;
using Project.Scripts.Tools;

// ReSharper disable InconsistentNaming

namespace Project.Scripts.Model
{
    // [Serializable] TODO
    public class ConvertedNotes
    {
        public ConvertedNotes(Note note, int bpm, ConvertedNotes tailGenerateNotes = null)
        {
            timing = 60f * note.num / (bpm * note.LPB);
            block = note.block;
            type = note.type;
            tailNote = tailGenerateNotes;
        }

        /* 先頭からの秒数 */
        public float timing { get; }

        /* レーン番号（0~6） */
        public int block { get; }

        /*
         * ノーツの種類
         * 1: 通常ノーツ
         * 2: ロングノーツ
         * 5: フリックノーツ
         */
        public int type { get; }

        /* 末尾ノーツ（ロングノーツの場合） */
        [CanBeNull] public ConvertedNotes tailNote { get; }

        public override string ToString() => this.ToStringReflection();
    }
}