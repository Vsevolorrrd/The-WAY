using System;

namespace Subtegral.DialogueSystem.DataContainers
{
    [Serializable]
    public class DialogueEvent
    {
        public DialogueEventType EventType;
        public string EventName;
    }

    public enum DialogueEventType
    {
        Custom,
        SetStringCondition,
        SetBooleanCondition,
        PlaySound,
        ScreenShake,
        PostEffect,
        SetPlayerName
    }

}