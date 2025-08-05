using Subtegral.DialogueSystem.DataContainers;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Subtegral.DialogueSystem.Editor
{
    public class DialogueGraph : EditorWindow
    {
        private string _fileName = "New Dialogue";

        private DialogueGraphView _graphView;

        [MenuItem("Graph/Dialogue Graph")]
        public static void CreateGraphViewWindow()
        {
            var window = GetWindow<DialogueGraph>();
            window.titleContent = new GUIContent("Dialogue Graph");
        }

        private void ConstructGraphView()
        {
            _graphView = new DialogueGraphView(this)
            {
                name = "Dialogue Graph",
            };
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }
        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            // === SAVE ===
            var saveNameField = new TextField("Save Dialogues:")
            {
                value = _fileName
            };
            saveNameField.RegisterValueChangedCallback(evt =>
            {
                _fileName = evt.newValue;
            });
            toolbar.Add(saveNameField);

            toolbar.Add(new Button(() => RequestDataOperation(true)) { text = "Save Dialogue" });

            // === LOAD ===
            var dialogueFiles = Resources.LoadAll<DialogueContainer>("Dialogues");
            var dialogueNames = dialogueFiles.Select(file => file.name).ToList();

            if (dialogueNames.Count == 0)
            dialogueNames.Add("No Saved Dialogues");

            var loadDropdown = new PopupField<string>("Load Dialogues:", dialogueNames, 0);
            loadDropdown.value = "Select a Dialogue";

            loadDropdown.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue != "Select a Dialogue" && evt.newValue != "No Saved Dialogues")
                {
                    _fileName = evt.newValue;
                    saveNameField.SetValueWithoutNotify(_fileName); // Update the save field
                }
            });
            toolbar.Add(loadDropdown);

            toolbar.Add(new Button(() => RequestDataOperation(false)) { text = "Load Dialogue" });

            toolbar.Add(new Button(() => _graphView.CreateNewDialogueNode("Dialogue Node", Vector2.zero))
            {
                text = "New Node"
            });

            rootVisualElement.Add(toolbar);
        }

        private void RequestDataOperation(bool save)
        {
            if (!string.IsNullOrEmpty(_fileName))
            {
                var saveUtility = GraphSaveUtility.GetInstance(_graphView);
                if (save)
                saveUtility.SaveGraph(_fileName);
                else
                saveUtility.LoadNarrative(_fileName);
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid File name", "Please Enter a valid filename", "OK");
            }
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
            GenerateMiniMap();
        }

        private void GenerateMiniMap()
        {
            var miniMap = new MiniMap { anchored = true };
            var cords = _graphView.contentViewContainer.WorldToLocal(new Vector2(this.maxSize.x - 10, 30));
            miniMap.SetPosition(new Rect(cords.x, cords.y, 200, 140));
            _graphView.Add(miniMap);
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }
    }
}