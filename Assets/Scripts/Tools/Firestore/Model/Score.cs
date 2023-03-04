using Firebase.Firestore;
using JetBrains.Annotations;

namespace Tools.Firestore.Model
{
    [FirestoreData]
    public class Score
    {
        [FirestoreDocumentId] [UsedImplicitly] public DocumentReference Document { get; set; }

        [FirestoreProperty(Name = "active")]
        [UsedImplicitly]
        public bool Active { get; set; }

        [FirestoreProperty(Name = "created_at")]
        [UsedImplicitly]
        public Timestamp CreatedAt { get; set; }

        [FirestoreProperty(Name = "level")]
        [UsedImplicitly]
        public string Level { get; set; }

        [FirestoreProperty(Name = "music_id")]
        [UsedImplicitly]
        public DocumentReference MusicId { get; set; }

        [FirestoreProperty(Name = "total_score")]
        [UsedImplicitly]
        public int TotalScore { get; set; }

        [FirestoreProperty(Name = "updated_at")]
        [UsedImplicitly]
        public Timestamp UpdatedAt { get; set; }

        [FirestoreProperty(Name = "user_id")]
        [UsedImplicitly]
        public DocumentReference UserId { get; set; }
    }
}