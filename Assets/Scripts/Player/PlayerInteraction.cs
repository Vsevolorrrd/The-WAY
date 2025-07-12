using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] LayerMask interactiveLayer;
    private GameObject interactiveObj;
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
            playerMovement.MovePlayerTo(Hit.collider.gameObject);
        }
    }
}