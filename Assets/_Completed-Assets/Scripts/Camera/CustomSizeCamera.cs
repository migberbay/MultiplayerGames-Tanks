using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomSizeCamera : MonoBehaviour
{
    public Camera minimapCamera;
    public int MaxSize = 45, MinSize = 30;
    float currentAspect = (float)Screen.width/(float)Screen.height;
    private void Update() {
        
        if(Input.GetKey(KeyCode.KeypadPlus))
        {
            minimapCamera.orthographicSize = minimapCamera.orthographicSize + 4*Time.deltaTime;
            if(minimapCamera.orthographicSize > MaxSize)
            {
                minimapCamera.orthographicSize = MaxSize; // Max size
            }
        }

        if(Input.GetKey(KeyCode.KeypadMinus))
        {
            minimapCamera.orthographicSize = minimapCamera.orthographicSize - 4*Time.deltaTime;
            if(minimapCamera.orthographicSize < MinSize)
            {
                minimapCamera.orthographicSize = MinSize; // Min size 
            }
        }

    }
}
