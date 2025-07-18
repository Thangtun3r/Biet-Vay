using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeGraphView : GraphView
{
    public NodeGraphView()
    {
        // Enable Zoom, Dragging, Selecting
        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        // Grid Background
        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        // Style
        styleSheets.Add(Resources.Load<StyleSheet>("NodeGraphStyle"));

        // Allow edge validation
        graphViewChanged = OnGraphViewChanged;
    }

    public void CreateNode(string nodeName)
    {
        var node = new NodeGraphNode
        {
            title = nodeName,
            GUID = System.Guid.NewGuid().ToString(),
            style = { left = Random.Range(100, 400), top = Random.Range(100, 400) }
        };

        AddElement(node);
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange change)
    {
        if (change.edgesToCreate != null)
        {
            foreach (var edge in change.edgesToCreate)
            {
                // You can validate connections here if needed
                Debug.Log($"Connected: {edge.output.node.title} → {edge.input.node.title}");
            }
        }

        return change;
    }
}