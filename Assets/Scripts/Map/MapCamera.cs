using UnityEngine;

public class MapCamera : MonoBehaviour
{
    private Camera cam;
    private RenderTexture tex;
    private Vector2 oldScreenSize;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GetComponent<Camera>();
        tex = cam.targetTexture;
        oldScreenSize = new Vector2(Screen.width, Screen.height);
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("SCREEN IS " + Screen.width + " by " + Screen.height);
        // cam.aspect = Screen.width / Screen.height;
        // tex.width = Screen.width;
        // tex.height = Screen.height;
        // cam.ResetAspect();
        // Debug.Log("CAMERA ASPECT SET TO: " + cam.aspect);
    }
}
