using Tools.Score;
using UnityEngine;

namespace PlayScene
{
    public class NotesFallUpdater : MonoBehaviour
    {
        private readonly Vector3 _fallSpeed = new(0, -ProcessManager.Speed, 0);

        private void FixedUpdate()
        {
            if (gameObject.activeSelf) transform.position += _fallSpeed * Time.deltaTime;

            if (!(transform.position.y <= -2f)) return;
            gameObject.SetActive(false);
            ScoreHandler.AddScore(ScoreHandler.Judge.Miss);
        }
    }
}