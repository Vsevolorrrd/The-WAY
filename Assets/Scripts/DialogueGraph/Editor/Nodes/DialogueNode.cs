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

        public string DisplayText;
        public DialogueNodeType NodeType = DialogueNodeType.Basic;
        public string Actor = "Unknown";
        public string AnimationName;
        public Vector3 MoveTo = Vector3.zero;

        // Conditions
        public BoolCondition BoolCondition;
        public IntCondition IntCondition;
        public StringCondition StringCondition;

        // Events
        public DialogueEvent Event;

        // Camera
        public DialogueCameraNode Camera;
    }
}