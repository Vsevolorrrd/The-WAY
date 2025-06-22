using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class D_UI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] TextMeshProUGUI dialogueText;

    [Header("Choice")]
    [SerializeField] Transform buttonContainer;
    [SerializeField] Button choicePrefab;

    [Header("Timed Choice")]
    [SerializeField] CanvasGroup timedChoiceCG;
    [SerializeField] Image timedChoiceBar;
    [SerializeField] float BarAppearTime = 1f;

    public void OpenDialogueUI()
    {

    }
    public void CloseDialogueUI()
    {

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
}