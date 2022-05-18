using UnityEngine;

public class Node
{
    public Vector2 Position;
    public Vector2 TargetPosition;
    public Node PreviousNode;
    public int Sum;
    public int Startposnode;
    public int Pos;

    public Node(Vector2 position, Vector2 targetPosition, Node previousNode, int startposnode)
    {
        Position = position;
        TargetPosition = targetPosition;
        PreviousNode = previousNode;
        Startposnode = startposnode;
        Pos = (int)Mathf.Abs(targetPosition.x - position.x) + (int)Mathf.Abs(targetPosition.y - position.y);
        Sum = Pos + Startposnode;
    }
}