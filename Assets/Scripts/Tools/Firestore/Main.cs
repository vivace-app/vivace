using System;
using System.Collections;
using Firebase.Auth;

namespace Tools.Firestore
{
    public partial class FirestoreHandler
    {
        public IEnumerator GetIsSupportedVersionCoroutine(string version) => _GetIsSupportedVersion(version);

        public IEnumerator GetMusicListCoroutine() => _GetMusicList();
        
        public IEnumerator UpdateLastLoggedIn(FirebaseUser user) => _UpdateLastLoggedIn(user);

        public event Action<string> OnErrorOccured;
    }
}