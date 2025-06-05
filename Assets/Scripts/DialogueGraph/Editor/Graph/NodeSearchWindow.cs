using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;

namespace Subtegral.DialogueSystem.Editor
{
    public class NodeSearchWindow : ScriptableObject,ISearchWindowProvider
    {
        private EditorWindow window;
        private DialogueGraphView graphView;
        private Texture2D indentationIcon;
        
        public void Configure(EditorWindow window, DialogueGraphView graphView)
        {
            this.window = window;
            this.graphView = graphView;
            
            //Transparent 1px indentation icon as a hack
            indentationIcon = new Texture2D(1,1);
            indentationIcon.SetPixel(0,0,new Color(0,0,0,0));
            indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
                new SearchTreeGroupEntry(new GUIContent("Dialogue"), 1),

                new SearchTreeEntry(new GUIContent("Basic Node", indentationIcon))
                {
                    level = 2,
                    userData = DialogueNodeType.Basic
                },

                new SearchTreeEntry(new GUIContent("Choice Node", indentationIcon))
                {
                    level = 2,
                    userData = DialogueNodeType.Choice
                },

                new SearchTreeEntry(new GUIContent("Event Node", indentationIcon))
                {
                    level = 2,
                    userData = DialogueNodeType.Event
                },

                new SearchTreeEntry(new GUIContent("String Condition Node", indentationIcon))
                {
                    level = 2,
                    userData = DialogueNodeType.StringCondition
                },

                new SearchTreeEntry(new GUIContent("Bool Condition Node", indentationIcon))
                {
                    level = 2,
                    userData = DialogueNodeType.BoolCondition
                },

                new SearchTreeEntry(new GUIContent("Int Condition Node", indentationIcon))
                {
                    level = 2,
                    userData = DialogueNodeType.IntCondition
                },

                new SearchTreeEntry(new GUIContent("End Node", indentationIcon))
                {
                    level = 2,
                    userData = DialogueNodeType.End
                },

                new SearchTreeEntry(new GUIContent("Comment Block", indentationIcon))
                {
                    level = 1,
                    userData = new Group()
                }
            };
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            var mousePosition = window.rootVisualElement.ChangeCoordinatesTo(
                window.rootVisualElement.parent,
                context.screenMousePosition - window.position.position
            );
            var graphMousePosition = graphView.contentViewContainer.WorldToLocal(mousePosition);

            switch (entry.userData)
            {
                case DialogueNodeType nodeType:
                    graphView.CreateNewDialogueNode($"{nodeType} Node", graphMousePosition, nodeType);
                    return true;
                case Group group:
                    var rect = new Rect(graphMousePosition, graphView.DefaultCommentBlockSize);
                    graphView.CreateCommentBlock(rect);
                    return true;
            }

            return false;
        }

    }
}