using Subtegral.DialogueSystem.DataContainers;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq;
using UnityEngine;
using System;
using Button = UnityEngine.UIElements.Button;
using Characters;

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
                    tempDialogueNode.style.backgroundColor = new Color(0.5f, 0.1f, 0.8f);
                    break;
                case DialogueNodeType.TimedChoice:
                    tempDialogueNode.style.backgroundColor = new Color(0.6f, 0.1f, 0.7f);
                    break;
                case DialogueNodeType.Event:
                    tempDialogueNode.style.backgroundColor = new Color(0.7f, 0.6f, 0f);
                    break;
                case DialogueNodeType.Animation:
                    tempDialogueNode.style.backgroundColor = new Color(0.1f, 0.1f, 0.7f);
                    break;
                case DialogueNodeType.MoveCharacter:
                    tempDialogueNode.style.backgroundColor = new Color(0.1f, 0.1f, 0.4f);
                    break;
                case DialogueNodeType.Camera:
                    tempDialogueNode.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
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
                case DialogueNodeType.TimedChoice:
                    CreateTimedChoiceNodeUI(tempDialogueNode, savedData);
                    break;
                case DialogueNodeType.Event:
                    CreateEventNodeUI(tempDialogueNode, savedData);
                    break;
                case DialogueNodeType.StringCondition:
                    CreateStringConditionNodeUI(tempDialogueNode, savedData);
                    break;
                case DialogueNodeType.BoolCondition:
                    CreateBoolConditionNodeUI(tempDialogueNode, savedData);
                    break;
                case DialogueNodeType.IntCondition:
                    CreateIntConditionNodeUI(tempDialogueNode, savedData);
                    break;
                case DialogueNodeType.RandomCondition:
                    CreateRandomConditionNodeUI(tempDialogueNode, savedData);
                    break;
                case DialogueNodeType.CharacterCondition:
                    CreateCharacterConditionNodeUI(tempDialogueNode, savedData);
                    break;
                case DialogueNodeType.Animation:
                    CreateAnimationNodeUI(tempDialogueNode, savedData);
                    break;
                case DialogueNodeType.MoveCharacter:
                    CreateMoveCharacterNodeUI(tempDialogueNode, savedData);
                    break;
                case DialogueNodeType.Camera:
                    CreateCameraNodeUI(tempDialogueNode, savedData);
                    break;
                case DialogueNodeType.End:
                    CreateEndNodeUI(tempDialogueNode, savedData);
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

            if (savedData != null) // loading the saved data
            {
                node.CheckThisNode = savedData.CheckThisNode;
            }

            #region characters

            var characterNames = CharacterDatabase.GetCharacterNames();
            var characterIDs = CharacterDatabase.GetCharacterIDs();

            // addin Narrator
            const string narratorName = "Narrator";
            const string narratorID = "narrator_id";

            if (!characterNames.Contains(narratorName))
            {
                characterNames.Insert(0, narratorName);
                characterIDs.Insert(0, narratorID);
            }

            string selectedID = savedData != null ? savedData.Actor : characterIDs.FirstOrDefault();
            string selectedName = CharacterDatabase.GetCharacterNameFromID(selectedID);
            int selectedIndex = characterNames.IndexOf(selectedName);
            if (selectedIndex < 0) selectedIndex = 0;

            var characterDropdown = new PopupField<string>("Actor", characterNames, selectedIndex);
            characterDropdown.RegisterValueChangedCallback(evt =>
            {
                node.Actor = CharacterDatabase.GetCharacterIDFromName(evt.newValue);
            });

            node.Actor = CharacterDatabase.GetCharacterIDFromName(characterDropdown.value);
            node.mainContainer.Add(characterDropdown);

            #endregion

            var Toggle = new Toggle("Check This Node") { value = node.CheckThisNode };
            Toggle.RegisterValueChangedCallback(evt => node.CheckThisNode = evt.newValue);
            node.mainContainer.Add(Toggle);

            // Output port
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

        private void CreateTimedChoiceNodeUI(DialogueNode node, DialogueNodeData savedData = null)
        {
            var button = new Button(() => { AddChoicePort(node); })
            {
                text = "Add Choice"
            };

            if (savedData != null)
            node.FailTime = savedData.FailTime;

            var failPort = GetPortInstance(node, Direction.Output);
            failPort.portName = "Fail";
            node.outputContainer.Add(failPort);

            var timeField = new FloatField("Time Limit") { value = savedData != null ? savedData.FailTime : 5f };
            timeField.RegisterValueChangedCallback(evt =>
            {
                // Clamp the value between 0 and 120
                node.FailTime = Mathf.Clamp(evt.newValue, 0, 120);
                timeField.SetValueWithoutNotify(node.FailTime);
            });
            timeField.style.marginTop = 5;
            timeField.style.marginBottom = 5;

            node.mainContainer.Add(timeField); 
            node.titleButtonContainer.Add(button);
            node.outputContainer.Add(failPort);
            node.RefreshPorts();
            node.RefreshExpandedState();
        }
        private void CreateEventNodeUI(DialogueNode node, DialogueNodeData savedData = null)
        {
            var dialogueEvent = new DialogueEvent();

            if (savedData != null) // loading the saved data
            {
                dialogueEvent.EventType = savedData.EventType;
                dialogueEvent.EventName = savedData.EventName;
                dialogueEvent.EventValue = savedData.EventValue;
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

            var valueField = new FloatField("Value") { value = dialogueEvent.EventValue };
            valueField.RegisterValueChangedCallback(evt => dialogueEvent.EventValue = evt.newValue);

            node.mainContainer.Add(eventTypeField);
            node.mainContainer.Add(eventNameField);
            node.mainContainer.Add(valueField);

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
                condition.ConditionType = savedData.ConditionType;
            }

            var truePort = GetPortInstance(node, Direction.Output, Port.Capacity.Single);
            truePort.portName = "True";
            node.outputContainer.Add(truePort);

            var falsePort = GetPortInstance(node, Direction.Output, Port.Capacity.Single);
            falsePort.portName = "False";
            node.outputContainer.Add(falsePort);

            var conditionType = new EnumField("Comparison", StringConditionType.Default)
            {
                value = condition.ConditionType
            };
            conditionType.RegisterValueChangedCallback(evt => condition.ConditionType = (StringConditionType)evt.newValue);

            var keyField = new TextField("Key") { value = condition.Key };
            keyField.RegisterValueChangedCallback(evt => condition.Key = evt.newValue);

            node.mainContainer.Add(conditionType);
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

        private void CreateRandomConditionNodeUI(DialogueNode node, DialogueNodeData savedData = null)
        {
            var condition = new RandomCondition();

            if (savedData != null) // loading the saved data
            {
                condition.Value = savedData.RandomConditionValue;
            }

            var truePort = GetPortInstance(node, Direction.Output, Port.Capacity.Single);
            truePort.portName = "True";
            node.outputContainer.Add(truePort);

            var falsePort = GetPortInstance(node, Direction.Output, Port.Capacity.Single);
            falsePort.portName = "False";
            node.outputContainer.Add(falsePort);

            var valueField = new IntegerField("Value %") { value = condition.Value };
            valueField.RegisterValueChangedCallback(evt =>
            {
                // Clamp the value between 0% and 100%
                condition.Value = Mathf.Clamp(evt.newValue, 0, 100);
                valueField.SetValueWithoutNotify(condition.Value);
            });

            node.mainContainer.Add(valueField);

            node.RandomCondition = condition;
        }

        private void CreateCharacterConditionNodeUI(DialogueNode node, DialogueNodeData savedData = null)
        {
            var condition = new CharacterCondition();

            if (savedData != null)
            {
                condition.Attribute = savedData.CharacterAttribute;
                condition.Target = savedData.CharacterTarget;
                condition.ComparisonValue = savedData.CharacterComparisonValue;
                condition.Action = savedData.CharacterAction;
            }

            var attributeField = new EnumField("Attribute", CharacterAttribute.Relations)
            {
                value = condition.Attribute
            };
            attributeField.RegisterValueChangedCallback(evt => condition.Attribute = (CharacterAttribute)evt.newValue);

            var targetField = new EnumField("Target", CharacterTarget.Player)
            {
                value = condition.Target
            };
            targetField.RegisterValueChangedCallback(evt => condition.Target = (CharacterTarget)evt.newValue);

            var valueField = new IntegerField("Value") { value = condition.ComparisonValue };
            valueField.RegisterValueChangedCallback(evt =>
            {
                condition.ComparisonValue = evt.newValue;
                valueField.SetValueWithoutNotify(condition.ComparisonValue);
            });

            var actionField = new EnumField("Action", CharacterAction.None)
            {
                value = condition.Action
            };
            actionField.RegisterValueChangedCallback(evt => condition.Action = (CharacterAction)evt.newValue);

            node.mainContainer.Add(CreateCharacterDropdown(node, savedData));
            node.mainContainer.Add(attributeField);
            node.mainContainer.Add(targetField);
            node.mainContainer.Add(valueField);
            node.mainContainer.Add(actionField);

            node.CharacterCondition = condition;

            // Add ports
            var truePort = GetPortInstance(node, Direction.Output, Port.Capacity.Single);
            truePort.portName = "True";
            node.outputContainer.Add(truePort);

            var falsePort = GetPortInstance(node, Direction.Output, Port.Capacity.Single);
            falsePort.portName = "False";
            node.outputContainer.Add(falsePort);
        }

        private void CreateAnimationNodeUI(DialogueNode node, DialogueNodeData savedData = null)
        {
            var animationDropdown = new PopupField<string>("Animation", new List<string> { "None" }, 0);

            // characters

            var characterNames = CharacterDatabase.GetCharacterNames();
            var characterIDs = CharacterDatabase.GetCharacterIDs();

            string selectedID = savedData != null ? savedData.Actor : characterIDs.FirstOrDefault();
            string selectedName = CharacterDatabase.GetCharacterNameFromID(selectedID);
            int selectedIndex = characterNames.IndexOf(selectedName);
            if (selectedIndex < 0) selectedIndex = 0;

            var characterDropdown = new PopupField<string>("Actor", characterNames, selectedIndex);
            characterDropdown.RegisterValueChangedCallback(evt =>
            {
                node.Actor = CharacterDatabase.GetCharacterIDFromName(evt.newValue);
                UpdateAnimationDropdown(node, animationDropdown);
            });

            node.Actor = selectedID;

            // animations

            animationDropdown.RegisterValueChangedCallback(evt =>
            {
                node.AnimationName = evt.newValue;
            });

            // Set saved animation if available
            node.AnimationName = savedData?.AnimationName ?? null;
            UpdateAnimationDropdown(node, animationDropdown, node.AnimationName);

            // loop bool
            node.LoopAnimation = savedData?.LoopAnimation ?? true;
            var loop = new Toggle("Loop animation") { value = node.LoopAnimation };
            loop.RegisterValueChangedCallback(evt => node.LoopAnimation = evt.newValue);

            node.mainContainer.Add(characterDropdown);
            node.mainContainer.Add(animationDropdown);
            node.mainContainer.Add(loop);

            // Output port
            var outputPort = GetPortInstance(node, Direction.Output, Port.Capacity.Single);
            outputPort.portName = "Next";
            node.outputContainer.Add(outputPort);
        }

        private void CreateMoveCharacterNodeUI(DialogueNode node, DialogueNodeData savedData = null)
        {

            if (savedData != null) // loading the saved data
            {
                node.MoveTo = savedData.MoveTo;
            }

            // Vector3 Position Fields
            var positionX = new FloatField("Position X") { value = node.MoveTo.x };
            var positionY = new FloatField("Position Y") { value = node.MoveTo.y };
            var positionZ = new FloatField("Position Z") { value = node.MoveTo.z };

            positionX.RegisterValueChangedCallback(evt =>
            {
                node.MoveTo.x = evt.newValue;
            });

            positionY.RegisterValueChangedCallback(evt =>
            {
                node.MoveTo.y = evt.newValue;
            });

            positionZ.RegisterValueChangedCallback(evt =>
            {
                node.MoveTo.z = evt.newValue;
            });

            // put them all in a container
            var vectorContainer = new VisualElement();
            vectorContainer.Add(positionX);
            vectorContainer.Add(positionY);
            vectorContainer.Add(positionZ);

            node.mainContainer.Add(CreateCharacterDropdown(node, savedData));
            node.mainContainer.Add(vectorContainer);

            // Output port
            var outputPort = GetPortInstance(node, Direction.Output, Port.Capacity.Single);
            outputPort.portName = "Next";
            node.outputContainer.Add(outputPort);
        }

        private void CreateCameraNodeUI(DialogueNode node, DialogueNodeData savedData = null)
        {
            var cameraNode = new DialogueCameraNode();

            if (savedData != null) // loading the saved data
            {
                cameraNode.CameraActionType = savedData.CameraActionType;
                cameraNode.CameraActionDuration = savedData.CameraActionDuration;
                cameraNode.CameraActionPosition = savedData.CameraActionPosition;
            }

            var actionField = new EnumField("Action", CameraActionType.MoveBy)
            {
                value = cameraNode.CameraActionType
            };
            actionField.RegisterValueChangedCallback(evt => cameraNode.CameraActionType = (CameraActionType)evt.newValue);


            var valueField = new FloatField("Duration") { value = cameraNode.CameraActionDuration };
            valueField.RegisterValueChangedCallback(evt => cameraNode.CameraActionDuration = evt.newValue);

            // Vector3 Position Fields
            var positionX = new FloatField("Position X") { value = cameraNode.CameraActionPosition.x };
            var positionY = new FloatField("Position Y") { value = cameraNode.CameraActionPosition.y };
            var positionZ = new FloatField("Position Z") { value = cameraNode.CameraActionPosition.z };

            positionX.RegisterValueChangedCallback(evt =>
            {
                cameraNode.CameraActionPosition.x = evt.newValue;
            });

            positionY.RegisterValueChangedCallback(evt =>
            {
                cameraNode.CameraActionPosition.y = evt.newValue;
            });

            positionZ.RegisterValueChangedCallback(evt =>
            {
                cameraNode.CameraActionPosition.z = evt.newValue;
            });

            // put them all in a container
            var vectorContainer = new VisualElement();
            vectorContainer.Add(positionX);
            vectorContainer.Add(positionY);
            vectorContainer.Add(positionZ);

            node.mainContainer.Add(CreateCharacterDropdown(node, savedData));
            node.mainContainer.Add(vectorContainer);
            node.mainContainer.Add(actionField);
            node.mainContainer.Add(valueField);

            node.Camera = cameraNode;

            // Output port
            var outputPort = GetPortInstance(node, Direction.Output, Port.Capacity.Single);
            outputPort.portName = "Next";
            node.outputContainer.Add(outputPort);
        }

        private void CreateEndNodeUI(DialogueNode node, DialogueNodeData savedData = null)
        {
            if (savedData != null) // loading the saved data
            {
                node.EndAction = savedData.EndAction;
            }

            var actionField = new EnumField("End Action", EndAction.LoadScene)
            {
                value = node.EndAction
            };
            actionField.RegisterValueChangedCallback(evt => node.EndAction = (EndAction)evt.newValue);

            node.mainContainer.Add(actionField);
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

            var container = new VisualElement();
            container.Add(generatedPort);
            container.Add(portRow);

            var deleteButton = new Button(() => RemovePort(nodeCache, container))
            {
                text = "X"
            };

            portRow.Add(textField);
            portRow.Add(deleteButton);

            nodeCache.outputContainer.Add(container);
            nodeCache.RefreshPorts();
            nodeCache.RefreshExpandedState();
        }

        private void RemovePort(Node node, VisualElement portContainer)
        {
            var port = portContainer.Q<Port>();
            var targetEdge = edges.ToList().FirstOrDefault(x => x.output == port);
            if (targetEdge != null)
            {
                targetEdge.input.Disconnect(targetEdge);
                RemoveElement(targetEdge);
            }

            node.outputContainer.Remove(portContainer);
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
        private PopupField<string> CreateCharacterDropdown(DialogueNode node, DialogueNodeData savedData, string label = "Actor")
        {
            var characterNames = CharacterDatabase.GetCharacterNames();
            var characterIDs = CharacterDatabase.GetCharacterIDs();

            string selectedID = savedData != null ? savedData.Actor : characterIDs.FirstOrDefault();
            string selectedName = CharacterDatabase.GetCharacterNameFromID(selectedID);
            int selectedIndex = characterNames.IndexOf(selectedName);
            if (selectedIndex < 0) selectedIndex = 0;

            var dropdown = new PopupField<string>(label, characterNames, selectedIndex);
            dropdown.RegisterValueChangedCallback(evt =>
            {
                node.Actor = CharacterDatabase.GetCharacterIDFromName(evt.newValue);
            });

            node.Actor = selectedID;
            return dropdown;
        }

        private void UpdateAnimationDropdown(DialogueNode node, PopupField<string> dropdown, string defaultSelection = null)
        {
            if (string.IsNullOrEmpty(node.Actor))
            return;

            var animations = CharacterDatabase.GetAnimationsForCharacter(
            CharacterDatabase.GetCharacterNameFromID(node.Actor));

            if (animations == null || animations.Count == 0)
            animations = new List<string> { "None" };

            dropdown.choices = animations;

            string selection = animations.Contains(defaultSelection) ? defaultSelection : animations[0];
            dropdown.SetValueWithoutNotify(selection);
            dropdown.index = animations.IndexOf(selection);
            node.AnimationName = selection;
        }
    }
}