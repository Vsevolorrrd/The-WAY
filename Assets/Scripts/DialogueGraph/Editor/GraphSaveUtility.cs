using Subtegral.DialogueSystem.DataContainers;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Overlays;
using static UnityEngine.GraphicsBuffer;

namespace Subtegral.DialogueSystem.Editor
{
    public class GraphSaveUtility
    {
        private List<Edge> Edges => _graphView.edges.ToList();
        private List<DialogueNode> Nodes => _graphView.nodes.ToList().Cast<DialogueNode>().ToList();

        private List<Group> CommentBlocks =>
        _graphView.graphElements.ToList().Where(x => x is Group).Cast<Group>().ToList();

        private DialogueContainer _dialogueContainer;
        private DialogueGraphView _graphView;

        public static GraphSaveUtility GetInstance(DialogueGraphView graphView)
        {
            return new GraphSaveUtility
            {
                _graphView = graphView
            };
        }
        public void SaveGraph(string fileName)
        {
            if (!Edges.Any()) EditorUtility.DisplayDialog("Nodes disconected", "Some of the nodes are unconected", "OK");

            var dialogueContainerObject = ScriptableObject.CreateInstance<DialogueContainer>();
            if (!SaveNodes(fileName, dialogueContainerObject)) return;
            SaveExposedProperties(dialogueContainerObject);
            SaveCommentBlocks(dialogueContainerObject);

            // Ensure folders exist
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

            if (!AssetDatabase.IsValidFolder("Assets/Resources/Dialogues"))
            AssetDatabase.CreateFolder("Assets/Resources", "Dialogues");

            string assetPath = $"Assets/Resources/Dialogues/{fileName}.asset";
            UnityEngine.Object loadedAsset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(DialogueContainer));

            if (loadedAsset == null || !AssetDatabase.Contains(loadedAsset))
            {
                AssetDatabase.CreateAsset(dialogueContainerObject, assetPath);
            }
            else
            {
                DialogueContainer container = loadedAsset as DialogueContainer;
                container.NodeLinks = dialogueContainerObject.NodeLinks;
                container.DialogueNodeData = dialogueContainerObject.DialogueNodeData;
                container.ExposedProperties = dialogueContainerObject.ExposedProperties;
                container.CommentBlockData = dialogueContainerObject.CommentBlockData;
                EditorUtility.SetDirty(container);
            }

            AssetDatabase.SaveAssets();
        }

        private bool SaveNodes(string fileName, DialogueContainer dialogueContainerObject)
        {
            var connectedSockets = Edges.Where(x => x.input.node != null).ToArray();
            for (var i = 0; i < connectedSockets.Count(); i++)
            {
                var outputNode = (connectedSockets[i].output.node as DialogueNode);
                var inputNode = (connectedSockets[i].input.node as DialogueNode);
                var outputPort = connectedSockets[i].output;
                var textField = outputPort.userData as TextField;
                string displayText = textField != null ? textField.text : outputPort.portName;

                dialogueContainerObject.NodeLinks.Add(new NodeLinkData
                {
                    BaseNodeGUID = outputNode.GUID,
                    PortName = outputPort.portName,
                    TargetNodeGUID = inputNode.GUID,
                    DisplayText = displayText
                });
            }

            foreach (var node in Nodes.Where(node => !node.EntyPoint))
            {
                var nodeData = new DialogueNodeData
                {
                    NodeGUID = node.GUID,
                    DialogueText = node.DialogueText,
                    Position = node.GetPosition().position,
                    NodeType = node.NodeType,
                    DisplayText = node.DisplayText
                };

                // Save condition UI data for condition and event nodes
                switch (node.NodeType)
                {
                    case DialogueNodeType.Basic:
                        nodeData.Actor = node.Actor;
                        break;

                    case DialogueNodeType.TimedChoice:
                        nodeData.FailTime = node.FailTime;
                        break;

                    case DialogueNodeType.Event:
                        nodeData.EventType = node.Event?.EventType ?? DialogueEventType.Custom;
                        nodeData.EventName = node.Event?.EventName ?? "";
                        nodeData.EventValue = node.Event?.EventValue ?? 0;
                        break;

                    case DialogueNodeType.StringCondition:
                        nodeData.StringConditionKey = node.StringCondition?.Key ?? "";
                        break;

                    case DialogueNodeType.BoolCondition:
                        nodeData.BoolConditionKey = node.BoolCondition?.Key ?? "";
                        nodeData.BoolConditionExpectedValue = node.BoolCondition?.ExpectedValue ?? false;
                        break;

                    case DialogueNodeType.IntCondition:
                        nodeData.IntConditionKey = node.IntCondition?.Key ?? "";
                        nodeData.IntConditionComparison = node.IntCondition?.Comparison ?? ComparisonType.Equals;
                        nodeData.IntActionType = node.IntCondition?.Action ?? ActionType.None;
                        nodeData.IntConditionValue = node.IntCondition?.Value ?? 0;
                        break;

                    case DialogueNodeType.RandomCondition:
                        nodeData.RandomConditionValue = node.RandomCondition?.Value ?? 0;
                        break;

                    case DialogueNodeType.CharacterCondition:
                        nodeData.Actor = node.Actor;
                        nodeData.CharacterAttribute = node.CharacterCondition?.Attribute ?? CharacterAttribute.Relations;
                        nodeData.CharacterTarget = node.CharacterCondition?.Target ?? CharacterTarget.Player;
                        nodeData.CharacterComparisonValue = node.CharacterCondition?.ComparisonValue ?? 0;
                        nodeData.CharacterAction = node.CharacterCondition?.Action ?? CharacterAction.None;
                        break;

                    case DialogueNodeType.Animation:
                        nodeData.Actor = node.Actor;
                        nodeData.AnimationName = node.AnimationName;
                        nodeData.LoopAnimation = node.LoopAnimation;
                        break;

                    case DialogueNodeType.MoveCharacter:
                        nodeData.Actor = node.Actor;
                        nodeData.MoveTo = node.MoveTo;
                        break;

                    case DialogueNodeType.Camera:
                        nodeData.CameraActionType = node.Camera?.CameraActionType ?? CameraActionType.MoveBy;
                        nodeData.CameraActionDuration = node.Camera?.CameraActionDuration ?? 0f;
                        nodeData.CameraActionPosition = node.Camera?.CameraActionPosition ?? Vector3.zero;
                        break;

                }

                dialogueContainerObject.DialogueNodeData.Add(nodeData);
            }

            return true;
        }

        private void SaveExposedProperties(DialogueContainer dialogueContainer)
        {
            dialogueContainer.ExposedProperties.Clear();
            dialogueContainer.ExposedProperties.AddRange(_graphView.ExposedProperties);
        }

        private void SaveCommentBlocks(DialogueContainer dialogueContainer)
        {
            foreach (var block in CommentBlocks)
            {
                var nodes = block.containedElements.Where(x => x is DialogueNode).Cast<DialogueNode>().Select(x => x.GUID)
                    .ToList();

                dialogueContainer.CommentBlockData.Add(new CommentBlockData
                {
                    ChildNodes = nodes,
                    Title = block.title,
                    Position = block.GetPosition().position
                });
            }
        }

        public void LoadNarrative(string fileName)
        {
            if (fileName == "No Saved Dialogues")
            {
                EditorUtility.DisplayDialog("Invalid Selection", "There is no saved dialogues", "OK");
                return;
            }
            if (fileName == "New Dialogue")
            {
                EditorUtility.DisplayDialog("Invalid Selection", "You need to select a dialogue to load", "OK");
                return;
            }
            _dialogueContainer = Resources.Load<DialogueContainer>($"Dialogues/{fileName}");
            if (_dialogueContainer == null)
            {
                EditorUtility.DisplayDialog("File Not Found", $"Target Narrative Data '{fileName}' does not exist!", "OK");
                return;
            }

            ClearGraph();
            GenerateDialogueNodes();
            ConnectDialogueNodes();
            AddExposedProperties();
            GenerateCommentBlocks();
        }

        /// <summary>
        /// Set Entry point GUID then Get All Nodes, remove all and their edges. Leave only the entrypoint node. (Remove its edge too)
        /// </summary>
        private void ClearGraph()
        {
            Nodes.Find(x => x.EntyPoint).GUID = _dialogueContainer.NodeLinks[0].BaseNodeGUID;
            foreach (var perNode in Nodes)
            {
                if (perNode.EntyPoint) continue;
                Edges.Where(x => x.input.node == perNode).ToList()
                .ForEach(edge => _graphView.RemoveElement(edge));
                _graphView.RemoveElement(perNode);
            }
        }

        /// <summary>
        /// Create All serialized nodes and assign their guid and dialogue text to them
        /// </summary>

        private void GenerateDialogueNodes()
        {
            foreach (var perNode in _dialogueContainer.DialogueNodeData)
            {
                var tempNode = _graphView.CreateNode(perNode.DialogueText, perNode.Position, perNode.NodeType, perNode);
                tempNode.GUID = perNode.NodeGUID;
                tempNode.NodeType = perNode.NodeType;
                tempNode.DisplayText = perNode.DisplayText;

                _graphView.AddElement(tempNode);

                // Add choice ports if needed
                if (tempNode.NodeType == DialogueNodeType.Choice || tempNode.NodeType == DialogueNodeType.TimedChoice)
                {
                    var nodePorts = _dialogueContainer.NodeLinks
                    .Where(x => x.BaseNodeGUID == perNode.NodeGUID).ToList();

                    nodePorts.ForEach(x =>
                    { if (x.PortName != "Fail") 
                    _graphView.AddChoicePort(tempNode, x.PortName, x.DisplayText); });
                }
            }
        }

        private void ConnectDialogueNodes()
        {
            for (var i = 0; i < Nodes.Count; i++)
            {
                var k = i; //Prevent access to modified closure
                var connections = _dialogueContainer.NodeLinks.Where(x => x.BaseNodeGUID == Nodes[k].GUID).ToList();
                for (var j = 0; j < connections.Count(); j++)
                {
                    var targetNodeGUID = connections[j].TargetNodeGUID;
                    var targetNode = Nodes.First(x => x.GUID == targetNodeGUID);
                    LinkNodesTogether(Nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

                    targetNode.SetPosition(new Rect(
                    _dialogueContainer.DialogueNodeData.First(x => x.NodeGUID == targetNodeGUID).Position,
                    _graphView.DefaultNodeSize));
                }
            }
        }

        private void LinkNodesTogether(Port outputSocket, Port inputSocket)
        {
            var tempEdge = new Edge()
            {
                output = outputSocket,
                input = inputSocket
            };
            tempEdge?.input.Connect(tempEdge);
            tempEdge?.output.Connect(tempEdge);
            _graphView.Add(tempEdge);
        }

        private void AddExposedProperties()
        {
            _graphView.ClearBlackBoardAndExposedProperties();
            foreach (var exposedProperty in _dialogueContainer.ExposedProperties)
            {
                _graphView.AddPropertyToBlackBoard(exposedProperty);
            }
        }

        private void GenerateCommentBlocks()
        {
            foreach (var commentBlock in CommentBlocks)
            {
                _graphView.RemoveElement(commentBlock);
            }

            foreach (var commentBlockData in _dialogueContainer.CommentBlockData)
            {
                var block = _graphView.CreateCommentBlock
                (new Rect(commentBlockData.Position, _graphView.DefaultCommentBlockSize), commentBlockData);
                block.AddElements(Nodes.Where(x => commentBlockData.ChildNodes.Contains(x.GUID)));
            }
        }
    }
}