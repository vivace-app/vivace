using System;
using System.Collections;
using Firebase.Auth;
using Firebase.Firestore;
using Tools.PlayStatus;

namespace Tools.Firestore
{
    public partial class FirestoreHandler
    {
        public IEnumerator GetIsSupportedVersionCoroutine(string version) => _GetIsSupportedVersion(version);

        public IEnumerator GetMusicListCoroutine() => _GetMusicList();

        public IEnumerator UpdateLastLoggedIn(FirebaseUser user) => _UpdateLastLoggedIn(user);

        public IEnumerator UpdateDisplayName(FirebaseUser user, string displayName) =>
            _UpdateDisplayName(user, displayName);

        public IEnumerator AddScore(FirebaseUser user, string musicName, int totalScore, Level level) =>
            _AddScore(user, musicName, totalScore, level);

        public IEnumerator AddArchive(FirebaseUser user, string musicName, Level level, Archive archive) =>
            _AddArchive(user, musicName, level, archive);

        public IEnumerator GetRankingList(string musicId, string level) => _GetRankingList(musicId, level);

        public IEnumerator GetUserFromRef(DocumentReference documentReference) => _GetUserFromRef(documentReference);

        public event Action<string> OnErrorOccured;
    }
}