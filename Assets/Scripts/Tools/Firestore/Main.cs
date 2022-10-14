using System;
using System.Collections;
using Firebase.Auth;
using Firebase.Firestore;

namespace Tools.Firestore
{
    public partial class FirestoreHandler
    {
        public IEnumerator GetIsSupportedVersionCoroutine(string version) => _GetIsSupportedVersion(version);

        public IEnumerator GetMusicListCoroutine() => _GetMusicList();

        public IEnumerator UpdateLastLoggedIn(FirebaseUser user) => _UpdateLastLoggedIn(user);

        public IEnumerator GetRankingList(string musicId, string level) => _GetRankingList(musicId, level);
        
        public IEnumerator GetUserFromRef(DocumentReference documentReference) => _GetUserFromRef(documentReference);

        public event Action<string> OnErrorOccured;
    }
}