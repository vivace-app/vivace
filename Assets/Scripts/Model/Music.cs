using System;
using Project.Scripts.Tools;

// ReSharper disable InconsistentNaming

namespace Project.Scripts.Model
{
    [Serializable]
    public class Music
    {
        /* 曲名 */
        public string name;
        /* レーン数 */
        public int maxBlock;
        /* BPM */
        public int BPM;
        /* 譜面始点のオフセット */
        public int offset;
        /* ノーツデータ */
        public Note[] notes;

        public override string ToString() => this.ToStringReflection();
    }
}