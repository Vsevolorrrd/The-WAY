using Subtegral.DialogueSystem.DataContainers;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq;
using UnityEngine;
using System;
using Button = UnityEngine.UIElements.Button;
using Characters;
using UnityEditor.Overlays;

namespace Subtegral.DialogueSystem.Editor
{
    public class DialogueGraphView : GraphView
    {
        public readonly Vector2 DefaultNodeSize = new Vector2(200, 150);
        public readonly Vector2 DefaultCommentBlockSize = new Vector2(300, 200);
        public DialogueNode EntryPointNode;
        public Blackboard Blackboard = new Blackboard();
        public List<ExposedProperty> ExposedProperties { get; private set; } = new List<ExposedProperty>();

        private NodeSearchWindow searchWindow;

        public DialogueGraphView(DialogueGraph editorWindow)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("NarrativeGraph"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            AddElement(GetEntryPointNodeInstance());

            AddSearchWindow(editorWindow);
        }


        private void AddSearchWindow(DialogueGraph editorWindow)
        {
            searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            searchWindow.Configure(editorWindow, this);
            nodeCreationRequest = context =>
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }


        public void ClearBlackBoardAndExposedProperties()
        {
            ExposedProperties.Clear();
            Blackboard.Clear();
        }

        public Group CreateCommentBlock(Rect rect, CommentBlockData commentBlockData = null)
        {
            if (commentBlockData == null)
                commentBlockData = new CommentBlockData();

            var group = new Group
            {
                autoUpdateGeometry = true,
                title = commentBlockData.Title
            };
            AddElement(group);
            group.SetPosition(rect);
            return group;
        }

        public void AddPropertyToBlackBoard(ExposedProperty property, bool loadMode = false)
        {
            var localPropertyName = property.PropertyName;
            var localPropertyValue = property.PropertyValue;
            if (!loadMode)
            {
                while (ExposedProperties.Any(x => x.PropertyName == localPropertyName))
                    localPropertyName = $"{localPropertyName}(1)";
            }

            var item = ExposedProperty.CreateInstance();
            item.PropertyName = localPropertyName;
            item.PropertyValue = localPropertyValue;
            ExposedProperties.Add(item);

            var container = new VisualElement();
            var field = new BlackboardField { text = localPropertyName, typeText = "string" };
            container.Add(field);

            var propertyValueTextField = new TextField("Value:")
            {
                value = localPropertyValue
            };
            propertyValueTextField.RegisterValueChangedCallback(evt =>
            {
                var index = ExposedProperties.FindIndex(x => x.PropertyName == item.PropertyName);
                ExposedProperties[index].PropertyValue = evt.newValue;
            });
            var sa = new BlackboardRow(field, propertyValueTextField);
            container.Add(sa);
            Blackboard.Add(container);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            var startPortView = startPort;

            ports.ForEach((port) =>
            {
                var portView = port;
                if (startPortView != portView && startPortView.node != portView.node)
                    compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        public void CreateNewDialogueNode(string nodeName, Vector2 position, DialogueNodeType type = DialogueNodeType.Basic)
        {
            AddElement(CreateNode(nodeName, position, type));
        }

        public DialogueNode CreateNode(string nodeText, Vector2 position, DialogueNodeType type = DialogueNodeType.Basic, DialogueNodeData savedData = null)
        {
            var tempDialogueNode = new DialogueNode()
            {
                title = $"{type} Node",
                DialogueText = nodeText,
                GUID = Guid.NewGuid().ToString(),
                NodeType = type
            };

            // styling
            tempDialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

            // Color by type
            switch (type)
            {
                case DialogueNodeType.Basic:
                    tempDialogueNode.style.backgroundColor = new Color(0.7f, 0.1f, 0.1f);
                    break;
                case DialogueNodeType.Choice:
                    tempDialogueNode.style.backgroundColor = new Color(0.1f, 0.1f, 0.7f);
                    break;
                case DialogueNodeType.Event:
                    tempDialogueNode.style.backgroundColor = new Color(0.7f, 0.7f, 0.1f);
                    break;
                case DialogueNodeType.End:
                    tempDialogueNode.style.backgroundColor = new Color(0.7f, 0.7f, 0.7f);
                    break;
                default:
                    tempDialogueNode.style.backgroundColor = new Color(0.1f, 0.5f, 0.2f);
                    break;
            }

            // Input port
            var inputPort = GetPortInstance(tempDialogueNode, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            tempDialogueNode.inputContainer.Add(inputPort);

            // Outputs + UI by type
            switch (type)
            {
                case DialogueNodeType.Basic:
                    CreateBasicNodeUI(tempDialogueNode, savedData);
                    break;
                case DialogueNodeType.Choice:
                    CreateChoiceNodeUI(tempDialogueNode);
                    break;
                case DialogueNodeType.Event:
                    CreateEventNodeUI(tempDialogueNode, savedData);
                    break;
                case DialogueNodeType.BoolCondition:
                    CreateBoolConditionNodeUI(tempDialogueNode, savedData);
                    break;
                case DialogueNodeType.IntCondition:
                    CreateIntConditionNodeUI(tempDialogueNode, savedData);
                    break;
                case DialogueNodeType.StringCondition:
                    CreateStringConditionNodeUI(tempDialogueNode, savedData);
                    break;
            }


            // Title text field
            var textField = new TextField { multiline = true };
            textField.AddToClassList("wrapping-text-field");
            textField.SetValueWithoutNotify(tempDialogueNode.DialogueText);
            textField.RegisterValueChangedCallback(evt =>
            {
                tempDialogueNode.DialogueText = evt.newValue;
            });
            tempDialogueNode.mainContainer.Add(textField);

            tempDialogueNode.RefreshExpandedState();
            tempDialogueNode.RefreshPorts();
            tempDialogueNode.SetPosition(new Rect(position, DefaultNodeSize));

            return tempDialogueNode;
        }

        private void CreateBasicNodeUI(DialogueNode node, DialogueNodeData savedData = null)
        {

            var characterField = new EnumField("Actor", CharacterID.Unknown);
            characterField.value = savedData != null ? savedData.actor : CharacterID.Unknown;

            characterField.RegisterValueChangedCallback(evt =>
            {
                node.Actor = (CharacterID)evt.newValue;
            });

            node.mainContainer.Add(characterField);

            var outputPort = GetPortInstance(node, Direction.Output, Port.Capacity.Single);
            outputPort.portName = "Next";
            node.outputContainer.Add(outputPort);
        }

        private void CreateChoiceNodeUI(DialogueNode node)
        {
            var button = new Button(() => { AddChoicePort(node); })
            {
                text = "Add Choice"
            };
            node.titleButtonContainer.Add(button);
        }
        private void CreateEventNodeUI(DialogueNode node, DialogueNodeData savedData = null)
        {
            var dialogueEvent = new DialogueEvent();

            if (savedData != null) // loading the saved data
            {
                dialogueEvent.EventType = savedData.EventType;
                dialogueEvent.EventName = savedData.EventName;
            }

            node.Event = dialogueEvent;

            var eventTypeField = new EnumField("Event Type", DialogueEventType.Custom)
            {
                value = dialogueEvent.EventType
            };
            eventTypeField.RegisterValueChangedCallback(evt =>
            {
                dialogueEvent.EventType = (DialogueEventType)evt.newValue;
            });

            var eventNameField = new TextField("Event Name")
            {
                value = dialogueEvent.EventName
            };
            eventNameField.RegisterValueChangedCallback(evt =>
            {
                dialogueEvent.EventName = evt.newValue;
            });

            node.mainContainer.Add(eventTypeField);
            node.mainContainer.Add(eventNameField);

            // output port
            var outputPort = GetPortInstance(node, Direction.Output, Port.Capacity.Single);
            outputPort.portName = "Next";
            node.outputContainer.Add(outputPort);
        }

        private void CreateStringConditionNodeUI(DialogueNode node, DialogueNodeData savedData = null)
        {
            var condition = new StringCondition();

            if (savedData != null) // loading the saved data
            {
                condition.Key = savedData.StringConditionKey;
            }

            var truePort = GetPortInstance(node, Direction.Output, Port.Capacity.Single);
            truePort.portName = "True";
            node.outputContainer.Add(truePort);

            var falsePort = GetPortInstance(node, Direction.Output, Port.Capacity.Single);
            falsePort.portName = "False";
            node.outputContainer.Add(falsePort);

            var keyField = new TextField("Key") { value = condition.Key };
            keyField.RegisterValueChangedCallback(evt => condition.Key = evt.newValue);

            node.mainContainer.Add(keyField);
            node.StringCondition = condition;
        }
        private void CreateBoolConditionNodeUI(DialogueNode node, DialogueNodeData savedData = null)
        {
            var condition = new BoolCondition();

            if (savedData != null) // loading the saved data
            {
                condition.Key = savedData.BoolConditionKey;
                condition.ExpectedValue = savedData.BoolConditionExpectedValue;
            }

            var truePort = GetPortInstance(node, Direction.Output, Port.Capacity.Single);
            truePort.portName = "True";
            node.outputContainer.Add(truePort);

            var falsePort = GetPortInstance(node, Direction.Output, Port.Capacity.Single);
            falsePort.portName = "False";
            node.outputContainer.Add(falsePort);

            var keyField = new TextField("Key") { value = condition.Key };
            keyField.RegisterValueChangedCallback(evt => condition.Key = evt.newValue);

            var expectedToggle = new Toggle("Expected Value") { value = condition.ExpectedValue };
            expectedToggle.RegisterValueChangedCallback(evt => condition.ExpectedValue = evt.newValue);

            node.mainContainer.Add(keyField);
            node.mainContainer.Add(expectedToggle);

            node.BoolCondition = condition;
        }

        private void CreateIntConditionNodeUI(DialogueNode node, DialogueNodeData savedData = null)
        {
            var condition = new IntCondition();

            if (savedData != null) // loading the saved data
            {
                condition.Key = savedData.IntConditionKey;
                condition.Comparison = savedData.IntConditionComparison;
                condition.Action = savedData.IntActionType;
                condition.Value = savedData.IntConditionValue;
            }

            var truePort = GetPortInstance(node, Direction.Output, Port.Capacity.Single);
            truePort.portName = "True";
            node.outputContainer.Add(truePort);

            var falsePort = GetPortInstance(node, Direction.Output, Port.Capacity.Single);
            falsePort.portName = "False";
            node.outputContainer.Add(falsePort);

            var keyField = new TextField("Key") { value = condition.Key };
            keyField.RegisterValueChangedCallback(evt => condition.Key = evt.newValue);

            var comparisonField = new EnumField("Comparison", ComparisonType.Equals)
            {
                value = condition.Comparison
            };
            comparisonField.RegisterValueChangedCallback(evt => condition.Comparison = (ComparisonType)evt.newValue);

            var actionField = new EnumField("Action", ActionType.None)
            {
                value = condition.Action
            };
            actionField.RegisterValueChangedCallback(evt => condition.Action = (ActionType)evt.newValue);

            var valueField = new IntegerField("Value") { value = condition.Value };
            valueField.RegisterValueChangedCallback(evt => condition.Value = evt.newValue);

            node.mainContainer.Add(keyField);
            node.mainContainer.Add(comparisonField);
            node.mainContainer.Add(actionField);
            node.mainContainer.Add(valueField);

            node.IntCondition = condition;
        }

        public void AddChoicePort(DialogueNode nodeCache, string overriddenPortName = "", string displayText = "")
        {
            var generatedPort = GetPortInstance(nodeCache, Direction.Output);

            var outputPortCount = nodeCache.outputContainer.Query("connector").ToList().Count();
            var portNumber = outputPortCount + 1;
            var fixedPortLabel = $"Option {portNumber}";
            generatedPort.portName = fixedPortLabel;

            var portRow = new VisualElement();
            portRow.style.flexDirection = FlexDirection.Row;

            var textField = new TextField
            {
                value = string.IsNullOrEmpty(displayText)
                ? (string.IsNullOrEmpty(overriddenPortName) ? fixedPortLabel : overriddenPortName)
                : displayText
            };

            generatedPort.userData = textField;
            textField.name = string.Empty;
            textField.style.flexGrow = 1;

            var deleteButton = new Button(() => RemovePort(nodeCache, generatedPort))
            {
                text = "X"
            };

            portRow.Add(textField);
            portRow.Add(deleteButton);

            var container = new VisualElement();
            container.Add(generatedPort);
            container.Add(portRow);

            nodeCache.outputContainer.Add(container);
            nodeCache.RefreshPorts();
            nodeCache.RefreshExpandedState();
        }

        private void RemovePort(Node node, Port socket)
        {
            var targetEdge = edges.ToList().Where(x => x.output.portName == socket.portName && x.output.node == socket.node);
            if (targetEdge.Any())
            {
                var edge = targetEdge.First();
                edge.input.Disconnect(edge);
                RemoveElement(targetEdge.First());
            }

            node.outputContainer.Remove(socket);
            node.RefreshPorts();
            node.RefreshExpandedState();
        }

        private Port GetPortInstance(DialogueNode node, Direction nodeDirection,
        Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, nodeDirection, capacity, typeof(float));
        }

        private DialogueNode GetEntryPointNodeInstance()
        {
            var nodeCache = new DialogueNode()
            {
                title = "START",
                GUID = Guid.NewGuid().ToString(),
                DialogueText = "ENTRYPOINT",
                EntyPoint = true
            };

            var generatedPort = GetPortInstance(nodeCache, Direction.Output);
            generatedPort.portName = "Next";
            nodeCache.outputContainer.Add(generatedPort);

            nodeCache.capabilities &= ~Capabilities.Movable;
            nodeCache.capabilities &= ~Capabilities.Deletable;

            nodeCache.RefreshExpandedState();
            nodeCache.RefreshPorts();
            nodeCache.SetPosition(new Rect(100, 200, 100, 150));
            return nodeCache;
        }
    }
}