using System.Collections;
using Firebase.Storage;

namespace Project.Scripts.Tools.CloudStorage
{
    public partial class CloudStorageHandler
    {
        private readonly FirebaseStorage _cs = FirebaseStorage.DefaultInstance;

        private IEnumerator _GenerateDownloadURL(string path)
        {
            var reference = _cs.GetReference(path);
            var task = reference.GetDownloadUrlAsync();
            yield return new WaitForTaskCompletion(task);
            if (task.IsFaulted || task.IsCanceled)
                OnErrorOccured.Invoke("通信に失敗しました\nインターネットの接続状況を確認してください");
            else
                yield return task.Result;
        }
    }
}