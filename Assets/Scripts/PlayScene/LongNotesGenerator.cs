using System.Collections;
using System.Collections.Generic;
using PlayScene;
using UnityEngine;

public class LongNotesGenerator : MonoBehaviour
{
    public static LongNotesGenerator instance;

    [SerializeField] Material longNotesFiller;
    static float laneWidth = 0.3f; //レーンの太さ( = ノーツの太さ )
    static float _offset = 9f; // 3秒分のオフセット

    public void Awake() => instance ??= this;

    void Start()
    {
        //Create(0, 2, 0f, 3.0f);
        //Create(6, 5, 0f, 5.0f);
    }
    public List<GameObject> Create(int startLane, int endLane, float startTim, float endTim)
    {
        Debug.Log(startLane + ", " + endLane + ", " + startTim + ", " + endTim);
        //曲線の始点、制御点(今回は適当に設定)、終点
        Vector3 startPos = new Vector3(-0.9f + laneWidth * startLane, 6.4f, -0.005f);
        Vector3 controlPos = new Vector3(-0.9f + laneWidth * endLane, 6.4f + NotesFallUpdater.Speed * (endTim - startTim) / 2, -0.005f);
        Vector3 endPos = new Vector3(-0.9f + laneWidth * endLane, 6.4f + NotesFallUpdater.Speed * (endTim - startTim), -0.005f);

        //曲線生成
        Vector3[] curve = GetCurve(startPos, controlPos, endPos, 10);

        //テスト用のGameObject
        /*GameObject curveViewer = new GameObject();
        curveViewer.name = "curveViewer";
        curveViewer.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
        curveViewer.AddComponent<NotesFallUpdater>();
        LineRenderer renderer = curveViewer.AddComponent<LineRenderer>();

        //描画(テスト用)
        renderer.positionCount = curve.Length;
        renderer.widthMultiplier = 0.1f;
        renderer.alignment = LineAlignment.TransformZ;
        renderer.SetPositions(curve);*/

        List<GameObject> longNotes = new List<GameObject>();

        //★追加★
        for (int i = 0; i < curve.Length - 1; i++)
        {
            GameObject longNotesPart = new GameObject();
            longNotesPart.name = "longNotesPart" + i;
            longNotesPart.AddComponent<MeshFilter>();
            longNotesPart.AddComponent<MeshRenderer>().material = longNotesFiller;
            longNotesPart.AddComponent<Rigidbody>().useGravity = false;
            longNotesPart.AddComponent<NotesFallUpdater>();
            longNotesPart.SetActive(true);

            Generate(curve[i], curve[i + 1], longNotesPart);
            longNotes.Add(longNotesPart);
        }

        return longNotes;
    }


    //曲線生成
    public Vector3[] GetCurve(Vector3 sPos, Vector3 cPos, Vector3 ePos, int splitNum)
    {
        float t = 0;

        //曲線上の点の座標を等間隔に格納する配列
        Vector3[] CurvePoints_ = new Vector3[splitNum + 1];

        //曲線生成(正確には点の集まりなので点線に近い)
        for (int i = 0; i <= splitNum; i++)
        {
            t = (float)i / splitNum;
            var a = Vector3.Lerp(sPos, cPos, t);
            var b = Vector3.Lerp(cPos, ePos, t);

            CurvePoints_[i] = Vector3.Lerp(a, b, t);
        }

        //点の座標を返します。
        return CurvePoints_;
    }

    //★追加★
    void Generate(Vector3 sPos, Vector3 ePos, GameObject notesObj)
    {
        //メッシュを生成し、MeshFilterに渡す。
        //MeshFilterがMeshRendererにメッシュを渡して、メッシュが描画される。
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4];
        int[] triangles = new int[6];

        float lane1Pos = -2 * laneWidth;  //左端のx座標

        vertices[0] = sPos + new Vector3(-laneWidth / 2, 0, 0);//始点の左端
        vertices[1] = sPos + new Vector3(laneWidth / 2, 0, 0); //始点の右端
        vertices[2] = ePos + new Vector3(-laneWidth / 2, 0, 0); //終点の左端
        vertices[3] = ePos + new Vector3(laneWidth / 2, 0, 0); //終点の右端

        triangles = new int[6] { 0, 2, 1, 3, 1, 2 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        notesObj.GetComponent<MeshFilter>().mesh = mesh;
    }
}
