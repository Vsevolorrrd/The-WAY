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

        // Misc
        public string Actor = "Unknown";
        public string AnimationName;
        public bool LoopAnimation = true;

        // Basic data
        public bool CheckThisNode = false;

        // Timed Choice
        public float FailTime = 5f;

        // Conditions
        public BoolCondition BoolCondition;
        public IntCondition IntCondition;
        public StringCondition StringCondition;
        public RandomCondition RandomCondition;
        public CharacterCondition CharacterCondition;

        // Events
        public DialogueEvent Event;

        // Move Character
        public Vector3 MoveTo = Vector3.zero;

        // Camera
        public DialogueCameraNode Camera;

        // End
        public EndAction EndAction;
    }
}