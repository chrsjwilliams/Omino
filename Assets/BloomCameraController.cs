using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloomCameraController : MonoBehaviour {

    public Camera mainCamera;
    private Camera thisCamera;

    // Use this for initialization
    void Start()
    {
        thisCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position != mainCamera.transform.position)
        {
            transform.position = mainCamera.transform.position;
        }
        if (thisCamera.orthographicSize != mainCamera.orthographicSize)
        {
            thisCamera.orthographicSize = mainCamera.orthographicSize;
        }
    }
}
