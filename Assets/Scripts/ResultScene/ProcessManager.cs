using System.Collections;
using System.Linq;
using Tools.AssetBundle;
using Tools.Firestore;
using Tools.Firestore.Model;
using Tools.PlayStatus;
using Tools.Score;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ResultScene
{
    public class ProcessManager : MonoBehaviour
    {
        private void Awake() => Application.targetFrameRate = 60;

        private void Start()
        {
            if (Application.isEditor) LocaleSetting.ChangeSelectedLocale("ja");

            View.Instance.RetryButtonAction = () => SceneManager.LoadScene("PlayScene");
            View.Instance.ExitButtonAction = () => SceneManager.LoadScene("SelectScene");

            StartCoroutine(_DisplayRanking());
        }

        private IEnumerator _DisplayRanking()
        {
            var index = PlayStatusHandler.GetSelectedMusic();
            var level = PlayStatusHandler.GetSelectedLevel();
            var assetBundle = AssetBundleHandler.GetAssetBundle(index);
            var musicName = assetBundle.name;

            var fs = new FirestoreHandler();

            // // エラー発生時に実行する処理を登録
            // fs.OnErrorOccured += error =>
            // {
            //     // TODO: エラーをユーザに伝える
            //     Debug.Log(error);
            // };

            var ie = fs.GetRankingList(musicName, level.ToString().ToUpper());
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

            View.Instance.PerfectScoreText = ScoreHandler.GetPerfect().ToString();
            View.Instance.GreatScoreText = ScoreHandler.GetGreat().ToString();
            View.Instance.GoodScoreText = ScoreHandler.GetGood().ToString();
            View.Instance.MissScoreText = ScoreHandler.GetMiss().ToString();
            View.Instance.TotalScoreText = ScoreHandler.GetTotalScore().ToString();
        }
    }
}