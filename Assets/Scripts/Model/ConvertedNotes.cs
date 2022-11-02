using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Project.Scripts.Tools;
using UnityEngine;

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

        public List<GameObject> gameObjects { get; private set; }

        /* 末尾ノーツ（ロングノーツの場合） */
        [CanBeNull] public ConvertedNotes tailNote { get; }

        public void LinkGameObject(List<GameObject> gameObjects) => this.gameObjects = gameObjects;

        public void Destroy()
        {
            foreach (var gameObject in gameObjects)
                gameObject.SetActive(false);
        }

        public override string ToString() => this.ToStringReflection();
    }
}