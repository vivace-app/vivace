using System;
using System.Collections;

namespace Project.Scripts.Tools.Firestore
{
    public partial class FirestoreHandler
    {
        public IEnumerator GetIsSupportedVersionCoroutine(string version) => _GetIsSupportedVersion(version);

        public IEnumerator GetMusicListCoroutine() => _GetMusicList();

        public event Action<string> OnErrorOccured;
    }
}