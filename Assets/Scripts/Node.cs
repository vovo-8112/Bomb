using UnityEngine;

public class Node
{
    public Vector2 Position;
    public Vector2 TargetPosition;
    public Node PreviousNode;
    public int Sum;
    public int FromStartToTarget;
    public int DistanceToTarget;

    public Node(Vector2 position, Vector2 targetPosition, Node previousNode, int fromStartToTarget)
    {
        Position = position;
        TargetPosition = targetPosition;
        PreviousNode = previousNode;
        FromStartToTarget = fromStartToTarget;
        DistanceToTarget = (int)Mathf.Abs(targetPosition.x - position.x) + (int)Mathf.Abs(targetPosition.y - position.y);
        Sum = DistanceToTarget + FromStartToTarget;
    }
}