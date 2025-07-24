using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float interactionRange = 0.1f;

    private GameObject targetObject;
    private IInteractable targetInteractable;
    private bool movingToObject = false;

    private bool movingToPosition = false;
    private Vector3 targetPosition;

    private void Update()
    {
        if (movingToObject && !movingToPosition && targetObject != null)
        {
            float targetX = targetObject.transform.position.x;
            float currentX = transform.position.x;
            float distance = Mathf.Abs(currentX - targetX);

            if (distance > interactionRange)
            {
                float direction = Mathf.Sign(targetX - currentX);
                transform.position += new Vector3(direction * moveSpeed * Time.deltaTime, 0f, 0f);
            }
            else
            {
                InteractWithTarget();
            }
        }

        if (movingToPosition && !movingToObject)
        {
            float distance = Mathf.Abs(transform.position.x - targetPosition.x);

            if (distance > 0.07f)
            {
                float direction = Mathf.Sign(targetPosition.x - transform.position.x);
                transform.position += new Vector3(direction * moveSpeed * Time.deltaTime, 0f, 0f);
            }
            else
            {
                movingToPosition = false;
            }
        }
    }

    public void MovePlayerTo(Vector3 position)
    {
        movingToObject = false;
        targetObject = null;
        targetInteractable = null;

        targetPosition = new Vector3(position.x, transform.position.y, transform.position.z);
        movingToPosition = true;
    }

    public void MovePlayerToObject(GameObject obj)
    {
        if (obj == null) return;

        targetObject = obj;
        targetInteractable = obj.GetComponent<IInteractable>();
        movingToObject = true;
        movingToPosition = false;
    }

    private void InteractWithTarget()
    {
        movingToObject = false;

        if (targetInteractable != null)
        {
            targetInteractable.Interact();
        }

        targetObject = null;
        targetInteractable = null;
    }
}