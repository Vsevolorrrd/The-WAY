using UnityEngine;

public class MapInteraction : MonoBehaviour
{
    [SerializeField] LayerMask interactiveLayer;

    void Update()
    {
        if (!Input.GetMouseButtonDown(0) || Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D Hit = Physics2D.GetRayIntersection(ray, 100, interactiveLayer);

        if (Hit.collider != null)
        {
            Hit.collider.gameObject.GetComponent<IInteractable>().Interact();
        }
    }
}
