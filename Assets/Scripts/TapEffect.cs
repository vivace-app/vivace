using UnityEngine;

public class TapEffect : MonoBehaviour
{
    public GameObject effectPrefab;
    public float deleteTime = 1.0f;

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        var mousePosition = Input.mousePosition;
        mousePosition.z = 3f;
        var clone = Instantiate(effectPrefab, Camera.main!.ScreenToWorldPoint(mousePosition),
            Quaternion.identity);
        Destroy(clone, deleteTime);
    }
}