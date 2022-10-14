using System.Collections;
using System.Linq;
using Tools.Firestore;
using Tools.Firestore.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ResultScene
{
    public class ProcessManager : MonoBehaviour
    {
        private void Start()
        {
            if (Application.isEditor) LocaleSetting.ChangeSelectedLocale("ja");

            View.Instance.RetryButtonAction = () => SceneManager.LoadScene("PlayScene");
            View.Instance.ExitButtonAction = () => SceneManager.LoadScene("SelectScene");

            StartCoroutine(_DisplayRanking());
        }

        private IEnumerator _DisplayRanking()
        {
            var fs = new FirestoreHandler();

            // // エラー発生時に実行する処理を登録
            // fs.OnErrorOccured += error =>
            // {
            //     // TODO: エラーをユーザに伝える
            //     Debug.Log(error);
            // };

            var ie = fs.GetRankingList("spring_visit", "NORMAL");
            yield return StartCoroutine(ie);
            var score = (Score[]) ie.Current;
            if (score == null) yield break;

            var nameList = new string[10];
            foreach (var s in score.Select((v, i) => new {Value = v, Index = i}))
            {
                ie = fs.GetUserFromRef(s.Value.UserId);
                yield return StartCoroutine(ie);
                nameList[s.Index] = ((User) ie.Current)?.DisplayName;
            }

            var scoreList = new string[10];
            foreach (var s in score.Select((v, i) => new {Value = v, Index = i}))
                scoreList[s.Index] = s.Value.TotalScore.ToString();

            View.Instance.NameText = nameList;
            View.Instance.ScoreText = scoreList;
        }
    }
}