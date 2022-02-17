using UnityEngine;

namespace Project.Scripts.PlayScreen
{
    public class KeyboardEvent : MonoBehaviour
    {
        // --- Instance -----------------------------------------------------------------------
        private TouchEvent _touchEvent;
        // ------------------------------------------------------------------------------------

        public static readonly int[] OnTouch = {0, 0, 0, 0, 0};
        private Vector3 _judgePosition;

        private void Start()
        {
            _touchEvent =
                GameObject.Find("LaneD").GetComponent<TouchEvent>(); // Instance <- PlayScreenProcessManager.cs
        }

        private void Update()
        {
            if (!Application.isEditor) return;
            if (Input.GetKeyDown(KeyCode.D))
                _touchEvent.Touch(0);
            else if (Input.GetKeyDown(KeyCode.F))
                _touchEvent.Touch(1);
            else if (Input.GetKeyDown(KeyCode.G))
                _touchEvent.Touch(2);
            else if (Input.GetKeyDown(KeyCode.H))
                _touchEvent.Touch(3);
            else if (Input.GetKeyDown(KeyCode.J))
                _touchEvent.Touch(4);
        }
    }
}
