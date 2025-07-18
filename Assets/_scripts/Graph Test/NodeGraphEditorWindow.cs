using UnityEditor;
    using UnityEditor.Experimental.GraphView;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;
    
    public class NodeGraphEditorWindow : EditorWindow
    {
        private NodeGraphView graphView;
    
        [MenuItem("Window/Tools/Node Graph Editor")]
        public static void OpenGraphWindow()
        {
            var window = GetWindow<NodeGraphEditorWindow>();
            window.titleContent = new GUIContent("Node Graph");
        }
    
        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
        }
    
        private void ConstructGraphView()
        {
            graphView = new NodeGraphView
            {
                name = "Node Graph View"
            };
    
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }
    
        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();
    
            var nodeButton = new ToolbarButton(() => { graphView.CreateNode("New Node"); });
            nodeButton.text = "Create Node"; // Correctly set the button text
            toolbar.Add(nodeButton);
    
            rootVisualElement.Add(toolbar);
        }
    
        private void OnDisable()
        {
            rootVisualElement.Remove(graphView);
        }
    }