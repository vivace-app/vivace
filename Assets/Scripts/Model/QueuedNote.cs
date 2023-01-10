using System.Collections.Generic;
using JetBrains.Annotations;
using Project.Scripts.Model;
using Project.Scripts.Tools;
using UnityEngine;

namespace Model
{
    public class QueuedNote
    {
        public QueuedNote(Note note, int bpm, int offset, QueuedNote tailQueuedNote = null)
        {
            timing = 60f * note.num / (bpm * note.LPB) + offset / (1000 * 60f);
            lane = note.block;
            switch (note.type)
            {
                case 1:
                    noteType = NoteType.Normal;
                    break;
                case 2:
                    noteType = NoteType.Long;
                    break;
                case 3:
                case 4:
                case 5:
                case 6:
                    noteType = NoteType.Flick;
                    break;
            }

            TailNote = tailQueuedNote;
        }

        public enum NoteType
        {
            Normal,
            Long,
            Flick
        }

        /* 先頭からの秒数 */
        public float timing { get; }

        /* レーン番号（0~6） */
        public int lane { get; }

        /* ノーツの種類 */
        public NoteType noteType { get; }

        /* 生成されたゲームオブジェクト（未生成時はnull） */
        [CanBeNull] public List<GameObject> gameObjects { get; private set; }

        /* 末尾ノーツ（ロングノーツの場合） */
        [CanBeNull] public QueuedNote TailNote { get; }


        /* 生成されたゲームオブジェクトを登録 */
        public void LinkGameObject(List<GameObject> gameObjectList) => gameObjects = gameObjectList;

        /* 生成されたゲームオブジェクトを破壊（非表示） */
        public void Destroy()
        {
            if (gameObjects == null) return;
            foreach (var gameObject in gameObjects)
                gameObject.SetActive(false);
        }

        public override string ToString() => this.ToStringReflection();
    }
}