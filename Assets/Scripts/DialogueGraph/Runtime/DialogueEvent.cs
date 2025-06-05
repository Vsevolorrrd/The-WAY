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
        PlaySound,
        ScreenShake,
        PostEffect,
    }

}