using System;

namespace Subtegral.DialogueSystem.DataContainers
{
    [Serializable]
    public class DialogueEvent
    {
        public DialogueEventType EventType;
        public string EventName;
        public float EventValue;
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