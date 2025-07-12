using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float interactionRange = 0.1f;

    private GameObject targetObject;
    private IInteractable targetInteractable;
    private bool movingToObject = false;

    private void Update()
    {
        if (movingToObject && targetObject != null)
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
    }

    public void MovePlayerTo(GameObject obj)
    {
        if (obj == null) return;

        targetObject = obj;
        targetInteractable = obj.GetComponent<IInteractable>();
        movingToObject = true;
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