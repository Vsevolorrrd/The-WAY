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
        public string Actor;
        public string DisplayText;

        // Basic data
        public bool CheckThisNode = false;

        // Timed Choice data
        public float FailTime = 5f;

        // String Condition data
        public string StringConditionKey;
        public StringConditionType ConditionType;

        // Bool Condition data
        public string BoolConditionKey;
        public bool BoolConditionExpectedValue;

        // Int Condition data
        public string IntConditionKey;
        public ComparisonType IntConditionComparison;
        public ActionType IntActionType;
        public int IntConditionValue;

        // Character Condition data
        public CharacterAttribute CharacterAttribute;
        public CharacterTarget CharacterTarget;
        public int CharacterComparisonValue;
        public CharacterAction CharacterAction;

        // Random Condition data
        public int RandomConditionValue;

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

        // End data
        public EndAction EndAction;
    }
}