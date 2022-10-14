using Firebase.Firestore;
using JetBrains.Annotations;

namespace Tools.Firestore.Model
{
    [FirestoreData]
    public class User
    {
        [FirestoreDocumentId] [UsedImplicitly] public DocumentReference Document { get; set; }

        [FirestoreProperty(Name = "display_name")]
        [UsedImplicitly]
        public string DisplayName { get; set; }

        [FirestoreProperty(Name = "is_admin")]
        [UsedImplicitly]
        public bool IsAdmin { get; set; }

        [FirestoreProperty(Name = "last_logged_in")]
        [UsedImplicitly]
        public Timestamp LastLoggedIn { get; set; }

        [FirestoreProperty(Name = "play_days")]
        [UsedImplicitly]
        public int PlayDays { get; set; }
    }
}