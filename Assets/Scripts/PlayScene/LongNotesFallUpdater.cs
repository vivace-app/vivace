using Tools.Score;
using UnityEngine;

namespace PlayScene
{
    public class LongNotesFallUpdater : MonoBehaviour
    {
        private readonly Vector3 _fallSpeed = new(0, -ProcessManager.Speed, 0);
        private float _height;

        private void Start()
        {
            var vertices = gameObject.GetComponent<MeshFilter>().mesh.vertices;
            _height = (vertices[2] - vertices[0]).y;
        }

        private void FixedUpdate()
        {
            if (gameObject.activeSelf) transform.position += _fallSpeed * Time.deltaTime;

            if (!(transform.position.y <= -9f - _height * 10)) return;
            Destroy(gameObject);
            ScoreHandler.AddScore(ScoreHandler.Judge.Miss);
        }
    }
}