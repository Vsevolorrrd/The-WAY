using UnityEngine;
using System;
using Characters;

namespace Subtegral.DialogueSystem.DataContainers
{
    [Serializable] // stores the data
    public class DialogueNodeData
    {
        public string NodeGUID;
        public string DialogueText;
        public Vector2 Position;

        public DialogueNodeType NodeType;
        public CharacterID actor;
        public string DisplayText;

        // Condition data
        public string StringConditionKey;

        public string BoolConditionKey;
        public bool BoolConditionExpectedValue;

        public string IntConditionKey;
        public ComparisonType IntConditionComparison;
        public ActionType IntActionType;
        public int IntConditionValue;

        // Event data
        public DialogueEventType EventType;
        public string EventName;
    }
}