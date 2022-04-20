using System.Collections;

namespace Project.Scripts.Tools.Firestore
{
    public partial class Main
    {
        public IEnumerator GetIsValidLicenseCoroutine() => GetIsValidLicense();
        public IEnumerator GetMusicListCoroutine() => GetMusicList();
    }
}