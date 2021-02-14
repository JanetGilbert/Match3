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
    public void Fit(Rect rect)
    {
  
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = rect.width / rect.height;

        if (screenRatio >= targetRatio)
        {
            Camera.main.orthographicSize = rect.height / 2;
        }
        else
        {
            float differenceInSize = targetRatio / screenRatio;
            Camera.main.orthographicSize = rect.height / 2 * differenceInSize;
        }

        transform.position = new Vector3(rect.x + (rect.width/2.0f), rect.y + (rect.height / 2.0f), -1f);
    }
}
