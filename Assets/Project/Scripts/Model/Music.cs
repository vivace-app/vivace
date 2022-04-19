using Firebase.Firestore;
using JetBrains.Annotations;

namespace Project.Scripts.Model
{
    [FirestoreData]
    public class Music
    {
        [FirestoreDocumentId] [UsedImplicitly] public DocumentReference Document { get; set; }

        [FirestoreProperty(Name = "active")]
        [UsedImplicitly]
        public bool Active { get; set; }

        [FirestoreProperty(Name = "artist")]
        [UsedImplicitly]
        public string Artist { get; set; }

        [FirestoreProperty(Name = "artwork")]
        [UsedImplicitly]
        public string Artwork { get; set; }

        [FirestoreProperty(Name = "asset_bundle_android")]
        [UsedImplicitly]
        public string AssetBundleAndroid { get; set; }

        [FirestoreProperty(Name = "asset_bundle_ios")]
        [UsedImplicitly]
        public string AssetBundleIos { get; set; }

        [FirestoreProperty(Name = "asset_bundle_standalone_osx_universal")]
        [UsedImplicitly]
        public string AssetBundleStandaloneOsxUniversal { get; set; }

        [FirestoreProperty(Name = "asset_bundle_standalone_windows64")]
        [UsedImplicitly]
        public string AssetBundleStandaloneWindows64 { get; set; }

        [FirestoreProperty(Name = "created_at")]
        [UsedImplicitly]
        public Timestamp CreatedAt { get; set; }

        [FirestoreProperty(Name = "id")]
        [UsedImplicitly]
        public int Id { get; set; }

        [FirestoreProperty(Name = "name")]
        [UsedImplicitly]
        public string Name { get; set; }

        [FirestoreProperty(Name = "title")]
        [UsedImplicitly]
        public string Title { get; set; }

        [FirestoreProperty(Name = "updated_at")]
        [UsedImplicitly]
        public Timestamp UpdatedAt { get; set; }

        [FirestoreProperty(Name = "version")]
        [UsedImplicitly]
        public int Version { get; set; }
    }
}