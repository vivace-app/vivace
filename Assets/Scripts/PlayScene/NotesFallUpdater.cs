using UnityEngine;

namespace Project.Scripts.PlayScene
{
    public class NotesFallUpdater : MonoBehaviour
    {
        public const float Speed = 5f;

        private readonly Vector3 _fallSpeed = new(0, -Speed, 0);

        private void FixedUpdate()
        {
            if (gameObject.activeSelf) transform.position += _fallSpeed * Time.deltaTime;
            if (transform.position.y <= -2f) gameObject.SetActive(false);
        }
    }
}