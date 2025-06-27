using TMPro;
using UnityEngine;

public class D_FloatingText : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2f, 0);
    public TextMeshProUGUI textUI;

    private Camera mainCamera;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        mainCamera = Camera.main;
    }

    public void SetText(string text)
    {
        textUI.text = text;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 worldPos = target.position + offset;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        transform.position = screenPos;
    }
}