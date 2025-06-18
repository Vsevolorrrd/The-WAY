using UnityEngine;
using System;

namespace Subtegral.DialogueSystem.DataContainers
{
    [Serializable] // stores the data
    public class DialogueNodeData
    {
        public string NodeGUID;
        public string DialogueText;
        public Vector2 Position;

        public DialogueNodeType NodeType;
        public string actor;
        public string DisplayText;

        // String Condition data
        public string StringConditionKey;

        // Bool Condition data
        public string BoolConditionKey;
        public bool BoolConditionExpectedValue;

        // Int Condition data
        public string IntConditionKey;
        public ComparisonType IntConditionComparison;
        public ActionType IntActionType;
        public int IntConditionValue;

        // Event data
        public DialogueEventType EventType;
        public string EventName;
        public float EventValue;

        // Animation data
        public string AnimationName;
        public bool LoopAnimation = true;

        // MoveCharacter data
        public Vector3 MoveTo;

        // Camera data
        public CameraActionType CameraActionType;
        public float CameraActionDuration = 1f;
        public Vector3 CameraActionPosition;
    }
}