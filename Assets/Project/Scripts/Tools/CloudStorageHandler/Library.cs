using System.Collections;
using Firebase.Storage;
using UnityEngine;

namespace Project.Scripts.Tools.CloudStorageHandler
{
    public partial class CloudStorage
    {
        private readonly FirebaseStorage _storage;

        private IEnumerator _FetchTheDownloadURL(string path)
        {
            var reference = _storage.GetReference(path);
            var task = reference.GetDownloadUrlAsync();
            yield return new WaitForTaskCompletion(task);
            if (task.IsFaulted || task.IsCanceled)
                Debug.LogError("The path could not be found.");
            else
                yield return task.Result;
        }
    }
}