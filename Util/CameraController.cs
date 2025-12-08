using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera mainCam => GameUtil.mainCamera;
    
    private Vector2 movedic;
    private Vector2 currentPosition = Vector2.zero;

    [SerializeField] float zoomSpeed = 1f;
    [SerializeField] float moveSpeed = 1f;

    [SerializeField] float zoomMin = 5f, zoomMax = 20f;

    private bool cameraMoveModeOn = false;

    public void SetMoveMode()
    {
        cameraMoveModeOn = !cameraMoveModeOn;
    }

    public void Update()
    {
        if (cameraMoveModeOn) 
            return;

        if (Input.GetMouseButton(0))
        {
            movedic = (currentPosition - ConvartVector(Input.mousePosition)).normalized;
            Vector3 vec = movedic;
            mainCam.transform.position += vec * moveSpeed;
            currentPosition = ConvartVector(Input.mousePosition);
        }
        Zoom();
    }

    private Vector2 ConvartVector(Vector3 vec)
    {
        return new Vector2 (vec.x, vec.y);
    }

    private void Zoom()
    {
        float zoomValue = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        mainCam.orthographicSize = Math.Clamp(mainCam.orthographicSize - zoomValue, zoomMin, zoomMax);
    }
}
