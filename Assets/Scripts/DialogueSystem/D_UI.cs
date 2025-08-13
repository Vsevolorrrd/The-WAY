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
    [SerializeField] GameObject memoryIcon;
    [SerializeField] GameObject UI;

    [Header("Text")]
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] CanvasGroup dialogueCG;
    [SerializeField] GameObject textBoxPrefab;
    [SerializeField] Vector3 offset = new Vector3(0, 4f, 0);
    [SerializeField] float fadeDuration = 0.2f;
    [SerializeField] float textDelayPerCharacter = 0.04f;

    private GameObject currentBox;
    private Transform currentTarget;
    private TextMeshProUGUI currentText;
    private CanvasGroup floatingCG;
    private Camera mainCam;
    private bool boxIsVisible = false;
    private string textToType;

    [Header("Choice")]
    [SerializeField] Transform buttonContainer;
    [SerializeField] Button choicePrefab;
    [SerializeField] CanvasGroup buttonCG;

    [Header("Timed Choice")]
    [SerializeField] CanvasGroup timedChoiceCG;
    [SerializeField] Image timedChoiceBar;
    [SerializeField] float BarAppearTime = 0.8f;

    private Coroutine textRoutine;
    private Coroutine floatingRoutine;
    private Coroutine buttonRoutine;
    private Coroutine typewriterRoutine;

    private void Awake()
    {
        mainCam = Camera.main;
        currentBox = Instantiate(textBoxPrefab, UI.transform);
        currentText = currentBox.GetComponentInChildren<TextMeshProUGUI>();
        floatingCG = currentBox.GetComponent<CanvasGroup>();
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
        if (textRoutine != null) StopCoroutine(textRoutine);
        if (floatingRoutine != null) StopCoroutine(floatingRoutine);
        if (buttonRoutine != null) StopCoroutine(buttonRoutine);

        dialogueText.text = "";
        dialogueCG.alpha = 0;
        ClearButtons();
        HideChoiceBar();
        HideFloatingTextImmediate();
        UI.gameObject.SetActive(false);
    }

    public void CreateText(DialogueNodeData nodeData)
    {
        if (textRoutine != null) StopCoroutine(textRoutine);
        textRoutine = StartCoroutine(HandleTextFade(nodeData));
    }

    private IEnumerator HandleTextFade(DialogueNodeData nodeData)
    {
        yield return FadeOut(dialogueCG);

        if (nodeData.Actor == "narrator_id")
        {
            HideFloatingText();
            dialogueText.text = "";

            if (typewriterRoutine != null)
            {
                StopCoroutine(typewriterRoutine);
                typewriterRoutine = null;
            }
            typewriterRoutine = StartCoroutine(TypeText(dialogueText, nodeData.DialogueText, textDelayPerCharacter));
        }
        else
        {
            GameObject character = CharacterManager.Instance.GetCharacterInScene(nodeData.Actor);
            if (character != null)
            {
                yield return HideFloatingTextCoroutine();
                dialogueText.text = "";
                ShowFloatingText(character, nodeData.DialogueText, nodeData.Actor);
            }
            else
            {
                yield return HideFloatingTextCoroutine();
                string text = $"<b>{nodeData.Actor}</b>\n{nodeData.DialogueText}";
                dialogueText.text = "";

                if (typewriterRoutine != null)
                {
                    StopCoroutine(typewriterRoutine);
                    typewriterRoutine = null;
                }
                typewriterRoutine = StartCoroutine(TypeText(dialogueText, text, textDelayPerCharacter));
            }
        }
    }

    public void ClearText()
    {
        if (textRoutine != null) StopCoroutine(textRoutine);
        dialogueText.text = "";
        dialogueCG.alpha = 0;
        HideFloatingTextImmediate();
    }

    public void ShowFloatingText(GameObject character, string text, string actor = null)
    {
        SetFloatingText(character, text, actor);
    }

    public void HideFloatingText()
    {
        if (floatingRoutine != null) StopCoroutine(floatingRoutine);
        floatingRoutine = StartCoroutine(FadeOutFloatingText());
    }

    private void SetFloatingText(GameObject character, string text, string actor = null)
    {
        string formattedText = string.IsNullOrEmpty(actor) ? text : $"<b>{actor}</b>\n{text}"; //< b > to bold the actor's name


        currentTarget = character.transform;
        currentText.text = formattedText;
        currentBox.SetActive(true);
        boxIsVisible = true;
        floatingCG.alpha = 0f;
        currentText.text = "";

        if (typewriterRoutine != null)
        {
            StopCoroutine(typewriterRoutine);
            typewriterRoutine = null;
        }
        typewriterRoutine = StartCoroutine(TypeText(currentText, formattedText, textDelayPerCharacter));
    }

    private IEnumerator FadeOutFloatingText()
    {
        yield return FadeOut(floatingCG);
        currentBox.SetActive(false);
        currentText.text = "";
        boxIsVisible = false;
        currentTarget = null;
    }

    private IEnumerator HideFloatingTextCoroutine()
    {
        if (!boxIsVisible) yield break;
        yield return FadeOutFloatingText();
    }

    private void HideFloatingTextImmediate()
    {
        if (floatingRoutine != null) StopCoroutine(floatingRoutine);
        floatingCG.alpha = 0f;
        currentBox.SetActive(false);
        currentText.text = "";
        boxIsVisible = false;
        currentTarget = null;
    }

    public void CreateButtons(DialogueContainer container, DialogueNodeData nodeData, D_Manager manager)
    {
        ClearButtons();
        var links = container.NodeLinks.Where(x => x.BaseNodeGUID == nodeData.NodeGUID).ToList();

        foreach (var choice in links)
        {
            var button = Instantiate(choicePrefab, buttonContainer);
            button.GetComponentInChildren<TextMeshProUGUI>().text = choice.DisplayText;
            button.onClick.AddListener(() => manager.ProceedToNarrative(choice.TargetNodeGUID));
        }

        if (buttonRoutine != null) StopCoroutine(buttonRoutine);
        buttonRoutine = StartCoroutine(FadeIn(buttonCG));
    }

    public void ClearButtons()
    {
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
        if (buttonRoutine != null) StopCoroutine(buttonRoutine);
        buttonCG.alpha = 0f;
    }

    public void CreateTimedButtons(DialogueContainer container, DialogueNodeData nodeData, D_Manager manager)
    {
        UpdateTimedChoiceBar(0);
        ShowChoiceBar();

        var links = container.NodeLinks.Where(x => x.BaseNodeGUID == nodeData.NodeGUID && x.PortName != "Fail").ToList();

        foreach (var choice in links)
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

        if (buttonRoutine != null) StopCoroutine(buttonRoutine);
        buttonRoutine = StartCoroutine(FadeIn(buttonCG));
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

    private IEnumerator FadeIn(CanvasGroup cg)
    {
        cg.gameObject.SetActive(true);
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    private IEnumerator FadeOut(CanvasGroup cg)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 0f;
        cg.gameObject.SetActive(false);
    }
    private IEnumerator TypeText(TextMeshProUGUI textMesh, string fullText, float delayPerChar = 0.03f)
    {
        textToType = fullText;
        textMesh.text = "";
        int visibleCount = 0;
        bool insideTag = false;
        string displayedText = "";

        foreach (char c in fullText)
        {
            if (c == '<') insideTag = true;
            displayedText += c;
            if (c == '>') insideTag = false;

            if (!insideTag && c != '<')
            {
                visibleCount++;
                textMesh.text = displayedText;
                yield return new WaitForSeconds(delayPerChar);
            }
            else
            {
                textMesh.text = displayedText;
            }
        }

        textMesh.text = fullText;
        typewriterRoutine = null;
    }
    public bool SkipTypewriterIfActive()
    {
        if (typewriterRoutine != null)
        {
            StopCoroutine(typewriterRoutine);
            typewriterRoutine = null;

            // Immediately show full text
            if (boxIsVisible)
            {
                currentText.text = textToType;
                textToType = null;
            }
            else
            {
                dialogueText.text = textToType;
                textToType = null;
            }
            return true;
        }
        return false;
    }
    public void AddedMemory()
    {
        memoryIcon.SetActive(true);
    }
}