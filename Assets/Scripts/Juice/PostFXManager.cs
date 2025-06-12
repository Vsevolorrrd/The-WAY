using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class PostFXManager : Singleton<PostFXManager>
{
    [Header("Post Effects")]
    [SerializeField] private Volume painEffect;
    [SerializeField] private Volume dizzyEffect;

    [Header("Settings")]
    [SerializeField] private float effectDecaySpeed = 1f;
    public void DialoguePostEffect(string effectName, float duration)
    {
        switch (effectName)
        {
            case "pain":
                PainEffect(duration);
                break;
            case "dizzy":
                PainEffect(duration);
                break;
            default:
                Debug.LogWarning($"Unknown post effect name: {effectName}");
                break;
        }
    }
    public void PainEffect(float duration)
    {
        StartCoroutine(PlayEffect(painEffect, duration));
    }
    public void DizzyEffect(float duration)
    {
        StartCoroutine(PlayEffect(dizzyEffect, duration));
    }
    private IEnumerator PlayEffect(Volume effect, float duration)
    {
        if (effect == null)
        yield break;

        effect.weight = 1f;

        yield return new WaitForSeconds(duration);

        while (effect.weight > 0)
        {
            effect.weight -= effectDecaySpeed * Time.deltaTime;
            effect.weight = Mathf.Max(effect.weight, 0);
            yield return null;
        }
    }
}