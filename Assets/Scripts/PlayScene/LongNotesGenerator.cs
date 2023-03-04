using System.Collections.Generic;
using UnityEngine;

namespace PlayScene
{
    public class LongNotesGenerator : MonoBehaviour
    {
        public static LongNotesGenerator instance;

        [SerializeField] private Material longNotesFiller;

        private void Awake()
        {
            if (instance == null) instance = this;
        }

        public List<GameObject> Create(int startLane, int endLane, float startTim, float endTim)
        {
            // Debug.Log(startLane + ", " + endLane + ", " + startTim + ", " + endTim);

            // 曲線の始点、制御点(今回は適当に設定)、終点
            var startPos = new Vector3(-0.9f + ProcessManager.LaneWidth * startLane, 6.4f, -0.005f);
            var controlPos = new Vector3(-0.9f + ProcessManager.LaneWidth * endLane,
                6.4f + ProcessManager.Speed * (endTim - startTim) / 2, -0.005f);
            var endPos = new Vector3(-0.9f + ProcessManager.LaneWidth * endLane,
                6.4f + ProcessManager.Speed * (endTim - startTim), -0.005f);

            // 曲線生成
            var curve = GetCurve(startPos, controlPos, endPos, 10);

            // テスト用のGameObject
            /*
             GameObject curveViewer = new GameObject();
             curveViewer.name = "curveViewer";
             curveViewer.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
             curveViewer.AddComponent<NotesFallUpdater>();
             LineRenderer renderer = curveViewer.AddComponent<LineRenderer>();

             renderer.positionCount = curve.Length;
             renderer.widthMultiplier = 0.1f;
             renderer.alignment = LineAlignment.TransformZ;
             renderer.SetPositions(curve);
            */

            var longNotes = new List<GameObject>();

            // ★追加★
            for (var i = 0; i < curve.Length - 1; i++)
            {
                var longNotesPart = new GameObject
                {
                    name = "longNotesPart" + i
                };
                longNotesPart.AddComponent<MeshFilter>();
                longNotesPart.AddComponent<MeshRenderer>().material = longNotesFiller;
                longNotesPart.AddComponent<Rigidbody>().useGravity = false;
                longNotesPart.AddComponent<LongNotesFallUpdater>();
                longNotesPart.SetActive(true);

                Generate(curve[i], curve[i + 1], longNotesPart);
                longNotes.Add(longNotesPart);
            }

            return longNotes;
        }


        // 曲線生成
        private static Vector3[] GetCurve(Vector3 sPos, Vector3 cPos, Vector3 ePos, int splitNum)
        {
            // 曲線上の点の座標を等間隔に格納する配列
            var curvePoints = new Vector3[splitNum + 1];

            // 曲線生成(正確には点の集まりなので点線に近い)
            for (var i = 0; i <= splitNum; i++)
            {
                var t = (float) i / splitNum;
                var a = Vector3.Lerp(sPos, cPos, t);
                var b = Vector3.Lerp(cPos, ePos, t);

                curvePoints[i] = Vector3.Lerp(a, b, t);
            }

            // 点の座標を返します。
            return curvePoints;
        }

        // ★追加★
        private static void Generate(Vector3 sPos, Vector3 ePos, GameObject notesObj)
        {
            // メッシュを生成し、MeshFilterに渡す。
            // MeshFilterがMeshRendererにメッシュを渡して、メッシュが描画される。
            var mesh = new Mesh();

            var vertices = new Vector3[4];

            vertices[0] = sPos + new Vector3(-ProcessManager.LaneWidth / 2, 0, 0); // 始点の左端
            vertices[1] = sPos + new Vector3(ProcessManager.LaneWidth / 2, 0, 0); // 始点の右端
            vertices[2] = ePos + new Vector3(-ProcessManager.LaneWidth / 2, 0, 0); // 終点の左端
            vertices[3] = ePos + new Vector3(ProcessManager.LaneWidth / 2, 0, 0); // 終点の右端

            var triangles = new[] {0, 2, 1, 3, 1, 2};

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            notesObj.GetComponent<MeshFilter>().mesh = mesh;
        }
    }
}