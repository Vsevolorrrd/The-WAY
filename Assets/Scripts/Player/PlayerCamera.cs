using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] Camera Cam;
    [SerializeField] float threshhold = 5f;
    [SerializeField] float speed = 5f;
    [SerializeField] Vector3 direction;
    private Transform player;

    private void Start()
    {
        player = PlayerManager.Instance.transform;
    }
    void Update()
    {
        // Get the mouse position in the world
        Vector3 mousePos = Cam.ScreenToWorldPoint(Input.mousePosition);
        direction = (mousePos - player.position);
        if (direction.magnitude > threshhold)
        {
            direction = direction.normalized * threshhold;
        }
    }
    private void FixedUpdate()
    {
        Vector3 targetPosition = player.position + direction;
        targetPosition.z = transform.position.z; // Lock Z-axis
        transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
    }
}