using UnityEngine;

public class AudioVisualizerController : MonoBehaviour
{
    public AudioSpectrum spectrum;
    public Transform[] cubes;
    public float scale = 100.0f;

    private void Update()
    {
        for ( int i = 0; i < cubes.Length; i++ )
        {
            var cube = cubes[ i ];
            var localScale = cube.localScale;
            localScale.y = spectrum.Levels[ i ] * scale;
            cube.localScale = localScale;
        }
    }
}