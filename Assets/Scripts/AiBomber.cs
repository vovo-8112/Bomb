using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AiBomber : MonoBehaviour
{
    public List<Vector2> PatchToTarget;
    public List<Node> CheckedNodes = new List<Node>();
    public List<Node> WaitingNodes = new List<Node>();

    [SerializeField]
    private GameObject Target;

    public LayerMask SolidLayer;

    public List<Vector2> GetPath(Vector2 target)
    {
        PatchToTarget.Clear();
        CheckedNodes.Clear();
        WaitingNodes.Clear();
        var pathToTarget = new List<Vector2>();
        var transformPosition = transform.position;
        Vector2 startPos = new Vector2(Mathf.Round(transformPosition.x), Mathf.Round(transformPosition.z));
        var targetTransform = Target.transform.position;
        Vector2 targetPos = new Vector2(Mathf.Round(target.x), Mathf.Round(target.y));

        if (startPos == targetPos)
        {
            return pathToTarget;
        }

        Node startNode = new Node(startPos, targetPos, null, 0);
        CheckedNodes.Add(startNode);
        WaitingNodes.AddRange(GetNeighbourNode(startNode));

        while (WaitingNodes.Count > 0)
        {
            Node nodeToCheck = WaitingNodes.Where(x => x.F == WaitingNodes.Min(y => y.F)).FirstOrDefault();

            if (nodeToCheck.Position == targetPos)
            {
                return CalculatePatchFromNode(nodeToCheck);
            }

            var col = Physics.OverlapSphere(new Vector3(nodeToCheck.Position.x, 0.5f, nodeToCheck.Position.y), 0, SolidLayer);

            bool walkable = col.Length == 0;

            if (!walkable)
            {
                WaitingNodes.Remove(nodeToCheck);
                CheckedNodes.Add(nodeToCheck);
            }
            else
            {
                WaitingNodes.Remove(nodeToCheck);

                if (!CheckedNodes.Where(x => x.Position == nodeToCheck.Position).Any())
                {
                    CheckedNodes.Add(nodeToCheck);
                    WaitingNodes.AddRange(GetNeighbourNode(nodeToCheck));
                }

                // else
                // {
                //     var sameNode = CheckedNodes.Where(x => x.Position == nodeToCheck.Position).ToList();
                //
                //     for (int i = 0; i < sameNode.Count; i++)
                //     {
                //         if (sameNode[i].F > nodeToCheck.F)
                //         {
                //         }
                //     }
                // }
            }
        }

        return pathToTarget;
    }

    private List<Vector2> CalculatePatchFromNode(Node nodeToCheck)
    {
        var path = new List<Vector2>();
        Node current = nodeToCheck;

        while (current.PreviousNode != null)
        {
            path.Add(new Vector2(current.Position.x, current.Position.y));
            current = current.PreviousNode;
        }

        return path;
    }

    private IEnumerable<Node> GetNeighbourNode(Node startNode)
    {
        List<Node> nodesNeighbour = new List<Node>();

        nodesNeighbour.Add(new Node(new Vector2(startNode.Position.x - 1, startNode.Position.y), startNode.TargetPosition, startNode, startNode.G));

        nodesNeighbour.Add(new Node(new Vector2(startNode.Position.x + 1, startNode.Position.y), startNode.TargetPosition, startNode, startNode.G));

        nodesNeighbour.Add(new Node(new Vector2(startNode.Position.x, startNode.Position.y - 1), startNode.TargetPosition, startNode, startNode.G));

        nodesNeighbour.Add(new Node(new Vector2(startNode.Position.x, startNode.Position.y + 1), startNode.TargetPosition, startNode, startNode.G));

        return nodesNeighbour;
    }

    private void OnDrawGizmos()
    {
        foreach (var item in CheckedNodes)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(new Vector3(item.Position.x, 0, item.Position.y), 0.1f);
        }

        if (PatchToTarget != null)
            foreach (var item in PatchToTarget)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(new Vector3(item.x, 0, item.y), 0.1f);
            }
    }

    private void Update()
    {
        PatchToTarget = GetPath(new Vector2(Target.transform.position.x, Target.transform.position.z));
    }
}