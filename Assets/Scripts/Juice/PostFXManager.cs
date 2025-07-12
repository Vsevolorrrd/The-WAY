using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostFXManager : Singleton<PostFXManager>
{
    [Header("Pain")]
    [SerializeField] Volume painEffect;
    [Header("Dizzyness")]
    [SerializeField] Volume dizzyEffect;
    [SerializeField] float speed;
    [SerializeField] float minIntensity;
    [SerializeField] float maxIntensity;

    private ChromaticAberration chromaticAberration;
    private bool stop = false;

    [Header("Settings")]
    [SerializeField] private float effectDecaySpeed = 1f;
    public void DialoguePostEffect(string effectName, float duration)
    {
        switch (effectName.ToLower())
        {
            case "pain":
                PainEffect(duration);
                break;
            case "dizzy":
                DizzyEffect(duration);
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
        TriggerDizzyness();
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
        if (effect == dizzyEffect)
        StopDizzyness();
    }
    protected override void OnAwake()
    {
        if (!dizzyEffect.profile.TryGet(out chromaticAberration))
        {
            Debug.LogError("Chromatic Aberration not found in dizzyEffect profile.");
        }
        else
        {
            chromaticAberration.intensity.overrideState = true;
        }
    }

    private void StopDizzyness()
    {
        stop = true;
    }

    private void TriggerDizzyness()
    {
        stop = false;
        StartCoroutine(DizzynessCycle());
        IEnumerator DizzynessCycle()
        {
            int direction = 1;

            while (!stop || (stop && chromaticAberration.intensity.value > minIntensity))
            {
                chromaticAberration.intensity.value += direction * speed * Time.deltaTime;
                if (chromaticAberration.intensity.value <= minIntensity)
                {
                    chromaticAberration.intensity.value = minIntensity;
                    direction = 1;
                }
                else if (chromaticAberration.intensity.value >= maxIntensity)
                {
                    chromaticAberration.intensity.value = maxIntensity;
                    direction = -1;
                }
                yield return null;
            }
        }
    }
}