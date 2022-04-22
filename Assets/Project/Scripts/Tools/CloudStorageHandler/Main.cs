using System.Collections;
using Firebase.Storage;

namespace Project.Scripts.Tools.CloudStorageHandler
{
    public partial class CloudStorage
    {
        public CloudStorage() => _storage = FirebaseStorage.DefaultInstance;
            
        public IEnumerator FetchTheDownloadURL(string path) => _FetchTheDownloadURL(path);
    }
}