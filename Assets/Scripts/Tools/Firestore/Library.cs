using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Firestore;
using Tools.Firestore.Model;

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

        private IEnumerator _WriteDoc(DocumentReference doc, IDictionary<string, object> data)
        {
            var task = doc.SetAsync(data);
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

            var isSupportedVersion = ((QuerySnapshot)iEnumerator.Current).Documents.Any(documentSnapshot =>
                documentSnapshot.Id == version);
            yield return isSupportedVersion;
        }

        private IEnumerator _GetMusicList()
        {
            var capitalQuery = _fs.Collection("musics").WhereEqualTo("active", true).OrderBy("id");

            var iEnumerator = _ReadDoc(capitalQuery);
            yield return iEnumerator;

            var querySnapshot = (QuerySnapshot)iEnumerator.Current;
            if (querySnapshot == null)
            {
                OnErrorOccured.Invoke("通信に失敗しました\nインターネットの接続状況を確認してください");
                yield break;
            }

            var musicList = new Music[querySnapshot.Count];

            foreach (var documentSnapshot in querySnapshot.Documents.Select((v, i) => new { Value = v, Index = i }))
                musicList[documentSnapshot.Index] =
                    documentSnapshot.Value.ConvertTo<Music>(ServerTimestampBehavior.Estimate);

            yield return musicList;
        }
    }
}