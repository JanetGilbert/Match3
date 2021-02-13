using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Fits orthographic camera to display area within a Bounds.
public class CameraFit : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }


    // Based on https://answers.unity.com/questions/1231701/fitting-bounds-into-orthographic-2d-camera.html
    // User Satchel82
    public void Fit(Bounds bounds)
    {
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = bounds.size.x / bounds.size.y;

        if (screenRatio >= targetRatio)
        {
            Camera.main.orthographicSize = bounds.size.y / 2;
        }
        else
        {
            float differenceInSize = targetRatio / screenRatio;
            Camera.main.orthographicSize = bounds.size.y / 2 * differenceInSize;
        }

        transform.position = new Vector3(bounds.center.x, bounds.center.y, -1f);
    }
}
