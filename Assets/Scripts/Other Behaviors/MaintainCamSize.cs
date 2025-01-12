using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaintainCamSize : MonoBehaviour
{
    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();

        //This is to force a resolution
        cam.aspect = 1.778f;
        if(Screen.currentResolution.width > 1920 || Screen.currentResolution.height > 1080) Camera.main.pixelRect = new Rect(0, 0, Screen.currentResolution.width, Screen.currentResolution.height);
        else Camera.main.pixelRect = new Rect(0, 0, 1920, 1080);

        /*
        pos = transform.position;
        width = cam.orthographicSize * cam.aspect;
        height = cam.orthographicSize;   
    }

    private void Update()
    {
        cam.orthographicSize = width / cam.aspect;
        transform.position = new Vector3(pos.x,-1 * (height - cam.orthographicSize),pos.z);
    }
        */
    }
}
