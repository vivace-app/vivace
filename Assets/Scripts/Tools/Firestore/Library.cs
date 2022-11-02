using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Auth;
using Firebase.Firestore;
using Tools.Firestore.Model;
using UnityEngine;

namespace Tools.Firestore
{
    public partial class FirestoreHandler
    {
        private readonly FirebaseFirestore _fs = FirebaseFirestore.DefaultInstance;

        private IEnumerator _ReadDoc(Query query)
        {
            var task = query.GetSnapshotAsync();
            yield return new WaitForTaskCompletion(task);
            if (task.IsFaulted || task.IsCanceled)
                OnErrorOccured.Invoke("通信に失敗しました\nインターネットの接続状況を確認してください");
            else
                yield return task.Result;
        }

        private IEnumerator _ReadDoc(DocumentReference query)
        {
            var task = query.GetSnapshotAsync();
            yield return new WaitForTaskCompletion(task);
            if (task.IsFaulted || task.IsCanceled)
                OnErrorOccured.Invoke("通信に失敗しました\nインターネットの接続状況を確認してください");
            else
                yield return task.Result;
        }

        private IEnumerator _WriteDoc(DocumentReference doc, IDictionary<string, object> data)
        {
            var task = doc.SetAsync(data);
            yield return new WaitForTaskCompletion(task);
            if (task.IsFaulted || task.IsCanceled)
                OnErrorOccured.Invoke("通信に失敗しました\nインターネットの接続状況を確認してください");
        }

        private IEnumerator _UpdateDoc(DocumentReference doc, IDictionary<string, object> data)
        {
            var task = doc.UpdateAsync(data);
            yield return new WaitForTaskCompletion(task);
            if (task.IsFaulted || task.IsCanceled)
                OnErrorOccured.Invoke("通信に失敗しました\nインターネットの接続状況を確認してください");
        }

        private IEnumerator _GetIsSupportedVersion(string version)
        {
            var startDate = Timestamp.GetCurrentTimestamp();
            var capitalQuery = _fs.Collection("licenses")
                .WhereEqualTo("active", true)
                .OrderBy("invalided_at").StartAt(startDate);

            var iEnumerator = _ReadDoc(capitalQuery);
            yield return iEnumerator;

            if (iEnumerator.Current == null)
            {
                OnErrorOccured.Invoke("通信に失敗しました\nインターネットの接続状況を確認してください");
                yield break;
            }

            var isSupportedVersion = ((QuerySnapshot) iEnumerator.Current).Documents.Any(documentSnapshot =>
                documentSnapshot.Id == version);
            yield return isSupportedVersion;
        }

        private IEnumerator _GetMusicList()
        {
            var capitalQuery = _fs.Collection("musics").WhereEqualTo("active", true).OrderBy("id");

            var iEnumerator = _ReadDoc(capitalQuery);
            yield return iEnumerator;

            var querySnapshot = (QuerySnapshot) iEnumerator.Current;
            if (querySnapshot == null)
            {
                OnErrorOccured.Invoke("通信に失敗しました\nインターネットの接続状況を確認してください");
                yield break;
            }

            var musicList = new Music[querySnapshot.Count];

            foreach (var documentSnapshot in querySnapshot.Documents.Select((v, i) => new {Value = v, Index = i}))
                musicList[documentSnapshot.Index] =
                    documentSnapshot.Value.ConvertTo<Music>(ServerTimestampBehavior.Estimate);

            yield return musicList;
        }

        private IEnumerator _UpdateLastLoggedIn(FirebaseUser user)
        {
            var documentReference = _fs.Collection("users").Document(user.UserId);

            var iEnumerator = _ReadDoc(documentReference);
            yield return iEnumerator;

            var documentSnapshot = (DocumentSnapshot) iEnumerator.Current;
            if (documentSnapshot == null)
            {
                OnErrorOccured.Invoke("通信に失敗しました\nインターネットの接続状況を確認してください");
                yield break;
            }

            var userMeta = documentSnapshot.ConvertTo<User>();
            var playDays = userMeta.PlayDays;

            // 前回のログインから、日本時間午前4時を跨いでいるか
            if (Timestamp.GetCurrentTimestamp().ToDateTime().AddHours(5).Day >
                userMeta.LastLoggedIn.ToDateTime().AddHours(5).Day)
                playDays += 1;

            var updates = new Dictionary<string, object>
            {
                {"last_logged_in", FieldValue.ServerTimestamp},
                {"play_days", playDays}
            };

            iEnumerator = _UpdateDoc(documentReference, updates);
            yield return iEnumerator;
        }

        private IEnumerator _UpdateDisplayName(FirebaseUser user, string displayName)
        {
            var documentReference = _fs.Collection("users").Document(user.UserId);

            var updates = new Dictionary<string, object>
            {
                {"display_name", displayName},
            };

            var iEnumerator = _UpdateDoc(documentReference, updates);
            yield return iEnumerator;
        }

        private IEnumerator _GetRankingList(string musicId, string level)
        {
            var capitalQuery = _fs.CollectionGroup("scores")
                .WhereEqualTo("music_id", _fs.Collection("musics").Document(musicId))
                .WhereEqualTo("level", level)
                .WhereEqualTo("active", true)
                .OrderByDescending("total_score")
                .Limit(10);

            var iEnumerator = _ReadDoc(capitalQuery);
            yield return iEnumerator;

            var querySnapshot = (QuerySnapshot) iEnumerator.Current;
            if (querySnapshot == null)
            {
                OnErrorOccured.Invoke("通信に失敗しました\nインターネットの接続状況を確認してください");
                yield break;
            }

            var scoreList = new Model.Score[querySnapshot.Count];

            foreach (var documentSnapshot in querySnapshot.Documents.Select((v, i) => new {Value = v, Index = i}))
                scoreList[documentSnapshot.Index] =
                    documentSnapshot.Value.ConvertTo<Model.Score>(ServerTimestampBehavior.Estimate);

            yield return scoreList;
        }

        private IEnumerator _GetUserFromRef(DocumentReference documentReference)
        {
            var iEnumerator = _ReadDoc(documentReference);
            yield return iEnumerator;

            var documentSnapshot = (DocumentSnapshot) iEnumerator.Current;
            if (documentSnapshot == null)
            {
                OnErrorOccured.Invoke("通信に失敗しました\nインターネットの接続状況を確認してください");
                yield break;
            }

            yield return documentSnapshot.ConvertTo<User>();
        }
    }
}