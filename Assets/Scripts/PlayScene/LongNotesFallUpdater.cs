using UnityEngine;

namespace PlayScene
{
    public class LongNotesFallUpdater : MonoBehaviour
    {
        public const float Speed = 5f;

        private float height;

        private readonly Vector3 _fallSpeed = new(0, -Speed, 0);

        private void Start()
        {
            var varticles = gameObject.GetComponent<MeshFilter>().mesh.vertices;
            height = (varticles[2] - varticles[0]).y;
        }

        private void FixedUpdate()
        {
            if (gameObject.activeSelf) transform.position += _fallSpeed * Time.deltaTime;

            if (!(transform.position.y <= -9f - height * 10)) return;
            Destroy(gameObject);
            ProcessManager.AddScore(3);
        }
    }
}