using TMPro;
using UnityEngine;

public class ResourceUI : MonoBehaviour
{
    [SerializeField] GameObject resourceUI;
    [SerializeField] TextMeshProUGUI foodText;
    [SerializeField] TextMeshProUGUI junkText;
    [SerializeField] TextMeshProUGUI moneyText;
    [SerializeField] TextMeshProUGUI ammoText;
    private void Start()
    {
        ResourceManager.Instance.OnResourceChanged += UpdateUI;
        resourceUI.SetActive(false);
    }
    public void ShowUI()
    {
        resourceUI.SetActive(true);
    }
    public void HideUI()
    {
        resourceUI.SetActive(false);
    }
    public void UpdateUI(string name, int value)
    {
        switch (name.ToLowerInvariant())
        {
            case "food":
                foodText.text = value.ToString();
                break;
            case "junk":
                foodText.text = value.ToString();
                break;
            case "money":
                foodText.text = value.ToString();
                break;
            case "ammo":
                foodText.text = value.ToString();
                break;

            default:
                Debug.LogWarning("Invalid resource type");
                break;
        }
    }
    private void OnDestroy()
    {
        if (ResourceManager.Instance != null)
        ResourceManager.Instance.OnResourceChanged -= UpdateUI;
    }
}
