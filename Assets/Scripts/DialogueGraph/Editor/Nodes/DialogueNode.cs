using Subtegral.DialogueSystem.DataContainers;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Subtegral.DialogueSystem.Editor
{
    public class DialogueNode : Node
    {
        public string DialogueText;
        public string GUID;
        public bool EntyPoint = false;

        // Misc
        public string DisplayText;
        public DialogueNodeType NodeType = DialogueNodeType.Basic;
        public string Actor = "Unknown";
        public string AnimationName;
        public bool LoopAnimation = true;
        public Vector3 MoveTo = Vector3.zero;
        public float FailTime = 5f;

        // Conditions
        public BoolCondition BoolCondition;
        public IntCondition IntCondition;
        public StringCondition StringCondition;
        public RandomCondition RandomCondition;

        // Events
        public DialogueEvent Event;

        // Camera
        public DialogueCameraNode Camera;
    }
}