using UnityEngine;

public class Node
{
    public Vector2 Position;
    public Vector2 TargetPosition;
    public Node PreviousNode;
    public int F;
    public int G;
    public int H;

    public Node(Vector2 position, Vector2 targetPosition, Node previousNode, int g)
    {
        Position = position;
        TargetPosition = targetPosition;
        PreviousNode = previousNode;
        G = g;
        H = (int)Mathf.Abs(targetPosition.x - position.x) + (int)Mathf.Abs(targetPosition.y - position.y);
        F = H + G;
    }
}