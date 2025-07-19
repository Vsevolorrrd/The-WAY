using UnityEngine;

public class MapCamera : MonoBehaviour
{
    [SerializeField] float Speed = 10f;
    [SerializeField] float Boarder = 10f;
    [SerializeField] Camera Cam;

    void Update()
    {
        UpdateCamPos();
        CamZoom();
    }
    private void UpdateCamPos()
    {
        Vector3 pos = transform.position;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y >= Screen.height - Boarder)
        {
            pos.y += Speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y <= Boarder)
        {
            pos.y -= Speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= Screen.width - Boarder)
        {
            pos.x += Speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x <= Boarder)
        {
            pos.x -= Speed * Time.deltaTime;
        }
        transform.position = pos;
    }
    private void CamZoom()
    {
        if (Input.mouseScrollDelta == Vector2.zero)
            return;

        if (Input.mouseScrollDelta.y > 0)
        {
            Cam.orthographicSize = Mathf.Clamp(Cam.orthographicSize - 1, 1, 10);
        }
        else
        {
            Cam.orthographicSize = Mathf.Clamp(Cam.orthographicSize + 1, 1, 10);
        }

    }
}