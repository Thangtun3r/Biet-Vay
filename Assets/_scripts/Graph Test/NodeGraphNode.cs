using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public class NodeGraphNode : Node
{
    public string GUID;

    public NodeGraphNode()
    {
        // Create Input Port
        var inputPort = GeneratePort(Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "In";
        inputContainer.Add(inputPort);

        // Create Output Port
        var outputPort = GeneratePort(Direction.Output, Port.Capacity.Multi);
        outputPort.portName = "Out";
        outputContainer.Add(outputPort);

        // Title editing
        var textField = new TextField("Node Name");
        textField.RegisterValueChangedCallback(evt => title = evt.newValue);
        mainContainer.Add(textField);

        RefreshExpandedState();
        RefreshPorts();
    }

    private Port GeneratePort(Direction portDirection, Port.Capacity capacity)
    {
        return InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
    }
}