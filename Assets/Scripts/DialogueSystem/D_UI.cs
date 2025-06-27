using Subtegral.DialogueSystem.DataContainers;
using Subtegral.DialogueSystem.Runtime;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using Characters;
using TMPro;


public class D_UI : MonoBehaviour
{
    [SerializeField] GameObject UI;

    [Header("Text")]
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] GameObject textBoxPrefab;
    [SerializeField] Vector3 offset = new Vector3(0, 4f, 0);

    private GameObject currentBox;
    private Transform currentTarget;
    private TextMeshProUGUI currentText;
    private Camera mainCam;
    private bool boxIsVisible = false;

    [Header("Choice")]
    [SerializeField] Transform buttonContainer;
    [SerializeField] Button choicePrefab;

    [Header("Timed Choice")]
    [SerializeField] CanvasGroup timedChoiceCG;
    [SerializeField] Image timedChoiceBar;
    [SerializeField] float BarAppearTime = 0.8f;

    private void Awake()
    {
        mainCam = Camera.main;

        // Create floating text box once
        currentBox = Instantiate(textBoxPrefab, UI.transform);
        currentText = currentBox.GetComponentInChildren<TextMeshProUGUI>();
        currentBox.SetActive(false);
    }
    public void OpenDialogueUI()
    {
        UI.gameObject.SetActive(true);
        UpdateTimedChoiceBar(0);
        timedChoiceCG.alpha = 0;
    }

    public void CloseDialogueUI()
    {
        dialogueText.text = "";
        ClearButtons();
        HideChoiceBar();
        HideFloatingText();
        UI.gameObject.SetActive(false);
    }

    public void CreateText(DialogueNodeData nodeData)
    {
        if (nodeData.Actor == "narrator_id")
        {
            HideFloatingText();
            dialogueText.text = nodeData.DialogueText;
        }
        else
        {
            GameObject character = CharacterManager.Instance.GetCharacterInScene(nodeData.Actor);
            if (character != null)
            {
                ShowFloatingText(character, nodeData.DialogueText);
                dialogueText.text = ""; // Optional: hide bottom text when bubble is used
            }
            else
            {
                HideFloatingText();
                dialogueText.text = nodeData.DialogueText;
            }
        }
    }
    public void ClearText()
    {
        dialogueText.text = null;
        HideFloatingText();
    }
    public void ShowFloatingText(GameObject character, string text)
    {
        if (character == null) return;

        currentTarget = character.transform;
        currentText.text = text;
        currentBox.SetActive(true);
        boxIsVisible = true;
    }

    public void HideFloatingText()
    {
        currentBox.SetActive(false);
        currentText.text = "";
        boxIsVisible = false;
        currentTarget = null;
    }
    public void ClearButtons()
    {
        var buttons = buttonContainer.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            Destroy(buttons[i].gameObject);
        }
    }
    public void CreateButtons(DialogueContainer container, DialogueNodeData nodeData, D_Manager manager)
    {
        var links = container.NodeLinks.Where(x => x.BaseNodeGUID == nodeData.NodeGUID).ToList();

        foreach (var choice in links)
        {
            var button = Instantiate(choicePrefab, buttonContainer);
            button.GetComponentInChildren<TextMeshProUGUI>().text = choice.DisplayText;
            button.onClick.AddListener(() => manager.ProceedToNarrative(choice.TargetNodeGUID));
        }
    }
    public void CreateTimedButtons(DialogueContainer container, DialogueNodeData nodeData, D_Manager manager)
    {
        UpdateTimedChoiceBar(0);
        ShowChoiceBar();
        var links = container.NodeLinks.Where(x => x.BaseNodeGUID == nodeData.NodeGUID).ToList();

        // Skip "Fail" port
        foreach (var choice in links.Where(x => x.PortName != "Fail"))
        {
            var button = Instantiate(choicePrefab, buttonContainer);
            button.GetComponentInChildren<TextMeshProUGUI>().text = choice.DisplayText;
            button.onClick.AddListener(() =>
            {
                HideChoiceBar();
                manager.StopTimedCountDown();
                manager.ProceedToNarrative(choice.TargetNodeGUID);
            });
        }

    }
    public void UpdateTimedChoiceBar(float progress)
    {
        timedChoiceBar.fillAmount = Mathf.Clamp01(progress);
    }
    public void ShowChoiceBar()
    {
        StartCoroutine(FadeChoiceBar(1f));
    }
    public void HideChoiceBar()
    {
        StartCoroutine(FadeChoiceBar(0f));
    }
    private IEnumerator FadeChoiceBar(float targetAlpha)
    {
        float startAlpha = timedChoiceCG.alpha;
        float timer = 0f;

        while (timer < BarAppearTime)
        {
            timer += Time.deltaTime;
            float progress = timer / BarAppearTime;
            timedChoiceCG.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            yield return null;
        }

        timedChoiceCG.alpha = targetAlpha;
    }
    private void LateUpdate()
    {
        if (!boxIsVisible || currentTarget == null) return;

        Vector3 worldPos = currentTarget.position + offset;
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);
        currentBox.transform.position = screenPos;
    }

}