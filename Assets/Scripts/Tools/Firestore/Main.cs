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

        public IEnumerator AddAchieve(FirebaseUser user, string musicName, Level level, Achieve achieve) =>
            _AddAchieve(user, musicName, level, achieve);

        public IEnumerator GetAchieves(FirebaseUser user) => _GetAchieves(user);

        public IEnumerator GetRankingList(string musicId, Level level) => _GetRankingList(musicId, level);

        public IEnumerator GetUserFromRef(DocumentReference documentReference) => _GetUserFromRef(documentReference);

        public event Action<string> OnErrorOccured;
    }
}