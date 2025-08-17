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
    [SerializeField] AudioClip choiceSound;

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
        timedChoiceCG.alpha = 0f;
    }

    public void CloseDialogueUI()
    {
        if (textRoutine != null) StopCoroutine(textRoutine);
        if (floatingRoutine != null) StopCoroutine(floatingRoutine);
        if (buttonRoutine != null) StopCoroutine(buttonRoutine);

        dialogueText.text = "";
        dialogueCG.alpha = 0f;
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
            dialogueCG.alpha = 1f;
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
                dialogueCG.alpha = 1f;
                dialogueText.text = "";
                ShowFloatingText(character, nodeData.DialogueText, nodeData.Actor);
            }
            else
            {
                yield return HideFloatingTextCoroutine();
                string text = $"<b>{nodeData.Actor}</b>\n{nodeData.DialogueText}";
                dialogueCG.alpha = 1f;
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
        string formattedText = string.IsNullOrEmpty(actor) ? text : $"<b>{actor}</b>\n{text}";

        currentTarget = character.transform;
        currentBox.SetActive(true);
        floatingCG.alpha = 1f;
        boxIsVisible = true;
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
            button.onClick.AddListener(() =>
            {
                manager.ProceedToNarrative(choice.TargetNodeGUID);

                if (choiceSound)
                AudioManager.Instance.PlaySound(choiceSound, 0.7f);
            });
        }

        if (buttonRoutine != null) StopCoroutine(buttonRoutine);
        buttonRoutine = StartCoroutine(HandleButtonFadeIn());
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

                if(choiceSound)
                AudioManager.Instance.PlaySound(choiceSound, 0.7f);
            });
        }

        if (buttonRoutine != null) StopCoroutine(buttonRoutine);
        buttonRoutine = StartCoroutine(HandleButtonFadeIn());
    }
    private IEnumerator HandleButtonFadeIn()
    {
        yield return FadeOut(dialogueCG);
        dialogueText.text = "";

        if (boxIsVisible && floatingCG != null && floatingCG.alpha > 0f)
        {
            yield return FadeOut(floatingCG);
            currentBox.SetActive(false);
            currentText.text = "";
        }

        yield return FadeIn(buttonCG);
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
    }

    private IEnumerator TypeText(TextMeshProUGUI textMesh, string fullText, float delayPerChar = 0.03f)
    {
        textToType = fullText;
        textMesh.text = "";
        bool insideTag = false;
        string displayedText = "";

        foreach (char c in fullText)
        {
            if (c == '<') insideTag = true;
            displayedText += c;
            if (c == '>') insideTag = false;

            if (!insideTag && c != '<')
            {
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
        //memoryIcon.SetActive(true);
    }
    public void FadeAllText()
    {
        if (textRoutine != null) StopCoroutine(textRoutine);
        if (floatingRoutine != null) StopCoroutine(floatingRoutine);

        StartCoroutine(FadeAllTextCoroutine());
    }

    private IEnumerator FadeAllTextCoroutine()
    {
        if (dialogueCG != null && dialogueCG.alpha > 0f)
        {
            yield return FadeOut(dialogueCG);
            dialogueText.text = "";
        }
        if (boxIsVisible && floatingCG != null && floatingCG.alpha > 0f)
        {
            yield return FadeOut(floatingCG);
            currentBox.SetActive(false);
            currentText.text = "";
            boxIsVisible = false;
            currentTarget = null;
        }
    }

}