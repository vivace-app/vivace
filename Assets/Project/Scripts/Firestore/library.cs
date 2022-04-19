using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Firestore;
using Project.Scripts.Model;
using UnityEngine;

namespace Project.Scripts.Firestore
{
    public partial class Main
    {
        private readonly FirebaseFirestore _db = FirebaseFirestore.DefaultInstance;

        private static IEnumerator ReadDoc(Query query)
        {
            var getTask = query.GetSnapshotAsync();
            yield return new WaitForTaskCompletion(getTask);
            if (getTask.IsFaulted || getTask.IsCanceled)
                Debug.LogError("The data could not be read.");
            else
                yield return getTask.Result;
        }

        private static IEnumerator WriteDoc(DocumentReference doc, IDictionary<string, object> data)
        {
            var setTask = doc.SetAsync(data);
            yield return new WaitForTaskCompletion(setTask);
            if (setTask.IsFaulted || setTask.IsCanceled)
                Debug.LogError("Could not write data.");
        }

        private IEnumerator GetIsValidLicense()
        {
            var startDate = Timestamp.GetCurrentTimestamp();
            var capitalQuery = _db.Collection("licenses")
                .WhereEqualTo("active", true)
                .OrderBy("invalided_at").StartAt(startDate);

            var ie = ReadDoc(capitalQuery);
            yield return ie;

            var isValidLicense = ((QuerySnapshot) ie.Current)!.Documents.Any(documentSnapshot =>
                documentSnapshot.Id == EnvDataStore.ThisVersion);
            yield return isValidLicense;
        }

        private IEnumerator GetMusicList()
        {
            var capitalQuery = _db.Collection("musics").WhereEqualTo("active", true).OrderBy("id");

            var ie = ReadDoc(capitalQuery);
            yield return ie;

            var querySnapshot = (QuerySnapshot) ie.Current;
            var musicList = new Music[querySnapshot?.Count ?? 0];

            if (querySnapshot != null)
                foreach (var documentSnapshot in querySnapshot.Documents.Select((v, i) => new {Value = v, Index = i}))
                    musicList[documentSnapshot.Index] =
                        documentSnapshot.Value.ConvertTo<Music>(ServerTimestampBehavior.Estimate);

            yield return musicList;
        }
    }
}