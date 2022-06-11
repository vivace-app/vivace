using UnityEngine;

namespace PlayScreen
{
    public class TouchEvent : MonoBehaviour
    {
        // --- Instance -----------------------------------------------------------------------
        private PlayScreenProcessManager _playScreenProcessManager;
        // ------------------------------------------------------------------------------------

        public static readonly int[] OnTouch = {0, 0, 0, 0, 0};
        private Vector3 _judgePosition;

        private void Start()
        {
            _judgePosition = GameObject.Find("JudgeLineObject").transform.position;
            _playScreenProcessManager =
                GameObject.Find("ProcessManager")
                    .GetComponent<PlayScreenProcessManager>(); // Instance <- PlayScreenProcessManager.cs
        }

        public void Touch(int num) //クリック・タッチ中
        {
            var (targetNote, distance) = num switch
            {
                0 => FetchClosestNoteWithTag("Note1"),
                1 => FetchClosestNoteWithTag("Note2"),
                2 => FetchClosestNoteWithTag("Note3"),
                3 => FetchClosestNoteWithTag("Note4"),
                4 => FetchClosestNoteWithTag("Note5"),
                _ => (null, float.PositiveInfinity)
            };

            if (distance < 1.0)
            {
                _playScreenProcessManager.PerfectTimingFunc();
                Destroy(targetNote);
            }
            else if (distance < 1.3)
            {
                _playScreenProcessManager.GreatTimingFunc();
                Destroy(targetNote);
            }
            else if (distance < 1.5)
            {
                _playScreenProcessManager.GoodTimingFunc();
                Destroy(targetNote);
            }
        }

        private (GameObject, float) FetchClosestNoteWithTag(string tagName)
        {
            // 該当タグが1つしか無い場合はそれを返す
            var targets = GameObject.FindGameObjectsWithTag(tagName);
            if (targets.Length == 1)
                return (targets[0], Vector3.Distance(_judgePosition, targets[0].transform.position));

            GameObject result = null;
            var minTargetDistance = float.MaxValue;
            foreach (var target in targets)
            {
                // 前回計測したオブジェクトよりも近くにあれば記録
                var targetDistance = Vector3.Distance(_judgePosition, target.transform.position);
                if (!(targetDistance < minTargetDistance)) continue;
                minTargetDistance = targetDistance;
                result = target;
            }

            // 最後に記録されたオブジェクトを返す
            return (result, minTargetDistance);
        }
    }
}