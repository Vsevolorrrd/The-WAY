using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public void BlockMovement(bool state) { blockMovement = state; }
    [SerializeField] LayerMask interactiveLayer;
    [SerializeField] LayerMask walkableLayer;
    [SerializeField] bool blockMovement = false;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0) || Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D Hit = Physics2D.GetRayIntersection(ray, 100, interactiveLayer);

        if (Hit.collider != null)
        {
            if (blockMovement)
            {
                Hit.collider.gameObject.GetComponent<IInteractable>().Interact();
                return;
            }

            playerMovement.MovePlayerToObject(Hit.collider.gameObject);
            return;
        }

        if (blockMovement) 
        return;

        RaycastHit2D groundHit = Physics2D.GetRayIntersection(ray, 100, walkableLayer);
        if (groundHit.collider != null)
        {
            playerMovement.MovePlayerTo(groundHit.point);
        }
    }
}