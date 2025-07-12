using Subtegral.DialogueSystem.DataContainers;
using UnityEngine;
using System;

public class D_EventManager : MonoBehaviour
{
    public void DialogueEvent(DialogueNodeData nodeData, D_conditionManager conditionManager)
    {
        switch (nodeData.EventType)
        {
            case DialogueEventType.Custom:
                // add
                break;

            case DialogueEventType.SetStringCondition:
                conditionManager.AddStringCondition(nodeData.EventName.ToLowerInvariant());
                break;

            case DialogueEventType.SetBooleanCondition:
                if (nodeData.EventValue > 0)
                conditionManager.SetBoolCondition(nodeData.EventName.ToLowerInvariant(), true);
                else
                conditionManager.SetBoolCondition(nodeData.EventName.ToLowerInvariant(), false);
                break;

            case DialogueEventType.ChangeInteger:
                VariablesManager.Instance.ModifyValue(nodeData.EventName.ToLowerInvariant(), Mathf.RoundToInt(nodeData.EventValue));
                break;

            case DialogueEventType.PlaySound:
                AudioClip clip = AudioManager.Instance.GetSoundByName(nodeData.EventName);
                if (clip != null) AudioManager.Instance.PlaySound(clip, nodeData.EventValue);
                else Debug.LogWarning($"Sound '{nodeData.EventName}' not found in AudioManager.");
                break;

            case DialogueEventType.PlayMusic:
                AudioClip music = AudioManager.Instance.GetSoundByName(nodeData.EventName.ToLowerInvariant());
                if (music != null)
                AudioManager.Instance.PlaySound(music, nodeData.EventValue, null, true);
                else Debug.LogWarning($"Music '{nodeData.EventName}' not found in AudioManager.");
                break;

            case DialogueEventType.StopAllMusic:
                AudioManager.Instance.StopAllLoopSources(nodeData.EventValue);
                break;

            case DialogueEventType.ScreenShake:

                switch (nodeData.EventName.ToLowerInvariant())
                {
                    case "light":
                        CameraShake.Instance.ShakeCamera(1f, nodeData.EventValue);
                        break;
                    case "heavy":
                        CameraShake.Instance.ShakeCamera(5f, nodeData.EventValue);
                        break;
                    default:
                        CameraShake.Instance.ShakeCamera(3f, nodeData.EventValue);
                        break;

                }
                break;

            case DialogueEventType.PostEffect:
                PostFXManager.Instance.DialoguePostEffect(nodeData.EventName, nodeData.EventValue);
                break;

            case DialogueEventType.AddMemory:
                conditionManager.AddMemory(nodeData.EventName);
                break;

            case DialogueEventType.SetPlayerName:
                // change player name
                break;

            default:
                Debug.LogWarning($"Invalid event: {nodeData.NodeType}");
                break;
        }
    }
}