using System.Collections;

namespace Project.Scripts.Firestore
{
    public partial class Main
    {
        public IEnumerator GetIsValidLicenseCoroutine() => GetIsValidLicense();
        public IEnumerator GetMusicListCoroutine() => GetMusicList();
    }
}