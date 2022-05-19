using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreMountains.TopDownEngine;
using UnityEngine;

public enum Ai
{
    Default,
    GoTarget,
    CanSpawnBomb,
    MoveSavePos,
    NoBomb,
    WaitDestroyBomb,
}

public class AiBomber : MonoBehaviour
{
    public List<Vector2> PatchToTarget;
    private List<Node> CanMovePoints = new List<Node>();
    private List<Node> WaitingNode = new List<Node>();
    private List<Node> ListChecked = new List<Node>();

    [SerializeField]
    private GameObject Target;

    [SerializeField]
    private LayerMask LayerMaskCantMove;

    [SerializeField]
    private LayerMask CanBeDestroy;

    [SerializeField]
    private CharacterGridMovement m_CharacterGridMovement;

    [SerializeField]
    private CharacterHandleWeapon m_CharacterHandleWeapon;

    private Ai State;
    private Vector2 PosSpawnBomb;
    private List<Vector2> ListAnSavePos;
    private Vector2 PosSavePos = new Vector2(int.MinValue, int.MinValue);

    private void Start()
    {
        StartCoroutine(TestCoroutine());
    }

    private IEnumerator TestCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.6f);

            PatchToTarget = GetPathMove(new Vector2(Target.transform.position.x, Target.transform.position.z));

            DestroyBlocksLogic();

            var transformPosition = transform.position;

            Vector2 startPos = new Vector2(Mathf.Round(transformPosition.x), Mathf.Round(transformPosition.z));

            foreach (var node in CanMovePoints)
            {
                if (node.Position == startPos)
                {
                    CanMovePoints.Remove(node);
                    break;
                }
            }

            if (PatchToTarget.Count != 0)
            {
                var move = new Vector2(PatchToTarget[PatchToTarget.Count - 1].x - startPos.x, PatchToTarget[PatchToTarget.Count - 1].y - startPos.y);
                MoveBot(move);
            }
        }
    }

    private void DestroyBlocksLogic()
    {
        var transformPosition = transform.position;

        Vector2 startPos = new Vector2(Mathf.Round(transformPosition.x), Mathf.Round(transformPosition.z));

        if (PatchToTarget.Count == 0 && State != Ai.MoveSavePos)
        {
            var path = GetPathMove(CalculateBestTarget().Position);
            PatchToTarget = path;

            if (PatchToTarget.Count == 0)
            {
                State = Ai.CanSpawnBomb;
            }
        }

        if (PatchToTarget.Count == 0 && State == Ai.CanSpawnBomb && State != Ai.MoveSavePos)
        {
            SpawnBomb();
            State = Ai.MoveSavePos;
        }
        else if (State == Ai.MoveSavePos)
        {
            MoveSavePos();
        }
    }

    private void MoveSavePos()
    {
        foreach (var node in CanMovePoints)
        {
            foreach (var vector2 in ListAnSavePos)
            {
                if (node.Position != vector2)
                {
                    PosSavePos = node.Position;
                    break;
                }
            }
        }

        var savePos = new Vector2(PosSavePos.x, PosSavePos.y);
        PatchToTarget = GetPathMove(savePos);
    }

    private async void SpawnBomb()
    {
        m_CharacterHandleWeapon.ShootStart();
        var transformPosition = transform.position;

        Vector2 startPos = new Vector2(Mathf.Round(transformPosition.x), Mathf.Round(transformPosition.z));

        PosSpawnBomb = startPos;
        ListAnSavePos = new List<Vector2>();

        for (int i = 1; i < 5; i++)
        {
            ListAnSavePos.Add(new Vector2(PosSpawnBomb.x + i, PosSpawnBomb.y));
        }

        for (int i = 1; i < 5; i++)
        {
            ListAnSavePos.Add(new Vector2(PosSpawnBomb.x, PosSpawnBomb.y + i));
        }

        for (int i = 1; i < 5; i++)
        {
            ListAnSavePos.Add(new Vector2(PosSpawnBomb.x - i, PosSpawnBomb.y));
        }

        for (int i = 1; i < 5; i++)
        {
            ListAnSavePos.Add(new Vector2(PosSpawnBomb.x, PosSpawnBomb.y - i));
        }

        await Task.Delay(10000);
        State = Ai.Default;
        PosSavePos = new Vector2(int.MinValue, int.MinValue);
    }

    private Node CalculateBestTarget()
    {
        var nodeStart = CanMovePoints[0];

        var bestMove = new Dictionary<Node, int>();

        foreach (Node node in CanMovePoints)
        {
            var nodes = GetNeighbourNodeCanBeDestroy(node);

            bestMove.Add(node, nodes.Count());
        }

        int valueDestroyBlocks = 0;

        foreach (var keyValuePair in bestMove)
        {
            if (keyValuePair.Value > valueDestroyBlocks)
            {
                valueDestroyBlocks = keyValuePair.Value;
                nodeStart = keyValuePair.Key;
            }
        }

        return nodeStart;
    }

    private void MoveBot(Vector2 directory)
    {
        if (directory == Vector2.down)
        {
            m_CharacterGridMovement.DownOneCell();
        }
        else if (directory == Vector2.up)
        {
            m_CharacterGridMovement.UpOneCell();
        }
        else if (directory == Vector2.left)
        {
            m_CharacterGridMovement.LeftOneCell();
        }
        else if (directory == Vector2.right)
        {
            m_CharacterGridMovement.RightOneCell();
        }
    }

    private void CalculateBestMove()
    {
        ListChecked.Clear();

        foreach (Node variable in CanMovePoints)
        {
            ListChecked.AddRange(GetNeighbourNode(variable));
        }
    }

    private List<Vector2> GetPathMove(Vector2 target)
    {
        CalculateBestMove();
        PatchToTarget.Clear();
        CanMovePoints.Clear();
        WaitingNode.Clear();
        var pathToTarget = new List<Vector2>();
        var transformPosition = transform.position;
        Vector2 startPos = new Vector2(Mathf.Round(transformPosition.x), Mathf.Round(transformPosition.z));
        Vector2 targetPos = new Vector2(Mathf.Round(target.x), Mathf.Round(target.y));

        if (startPos == targetPos)
        {
            return pathToTarget;
        }

        Node startNode = new Node(startPos, targetPos, null, 0);
        CanMovePoints.Add(startNode);
        WaitingNode.AddRange(GetNeighbourNodeEmpty(startNode));

        while (WaitingNode.Count > 0)
        {
            Node nodeToCheck = WaitingNode.FirstOrDefault(x => x.Sum == WaitingNode.Min(y => y.Sum));

            // ReSharper disable once PossibleNullReferenceException
            if (nodeToCheck.Position == targetPos)
            {
                return CalculatePatchFromNode(nodeToCheck);
            }

            var ray = new Ray(new Vector3(nodeToCheck.Position.x, 1f, nodeToCheck.Position.y), Vector3.down);
            var raycast = Physics.Raycast(ray, out var hit, 1f, LayerMaskCantMove);

            if (raycast)
            {
                WaitingNode.Remove(nodeToCheck);
                CanMovePoints.Add(nodeToCheck);
            }
            else
            {
                WaitingNode.Remove(nodeToCheck);

                if (CanMovePoints.All(x => x.Position != nodeToCheck.Position))
                {
                    CanMovePoints.Add(nodeToCheck);
                    WaitingNode.AddRange(GetNeighbourNodeEmpty(nodeToCheck));
                }
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

    private IEnumerable<Node> GetNeighbourNodeEmpty(Node startNode)
    {
        var nodesNeighbour = new List<Node>();

        AddNodeIfCanMove(new Vector2(startNode.Position.x - 1, startNode.Position.y), startNode, nodesNeighbour);

        AddNodeIfCanMove(new Vector2(startNode.Position.x + 1, startNode.Position.y), startNode, nodesNeighbour);

        AddNodeIfCanMove(new Vector2(startNode.Position.x, startNode.Position.y - 1), startNode, nodesNeighbour);

        AddNodeIfCanMove(new Vector2(startNode.Position.x, startNode.Position.y + 1), startNode, nodesNeighbour);

        return nodesNeighbour;
    }

    private IEnumerable<Node> GetNeighbourNode(Node startNode)
    {
        var nodesNeighbour = new List<Node>();

        AddNode(new Vector2(startNode.Position.x - 1, startNode.Position.y), startNode, nodesNeighbour);

        AddNode(new Vector2(startNode.Position.x + 1, startNode.Position.y), startNode, nodesNeighbour);

        AddNode(new Vector2(startNode.Position.x, startNode.Position.y - 1), startNode, nodesNeighbour);

        AddNode(new Vector2(startNode.Position.x, startNode.Position.y + 1), startNode, nodesNeighbour);

        return nodesNeighbour;
    }

    private IEnumerable<Node> GetNeighbourNodeCanBeDestroy(Node startNode)
    {
        var nodesNeighbour = new List<Node>();

        AddNodeIfCanBeDestroy(new Vector2(startNode.Position.x - 1, startNode.Position.y), startNode, nodesNeighbour);

        AddNodeIfCanBeDestroy(new Vector2(startNode.Position.x + 1, startNode.Position.y), startNode, nodesNeighbour);

        AddNodeIfCanBeDestroy(new Vector2(startNode.Position.x, startNode.Position.y - 1), startNode, nodesNeighbour);

        AddNodeIfCanBeDestroy(new Vector2(startNode.Position.x, startNode.Position.y + 1), startNode, nodesNeighbour);

        return nodesNeighbour;
    }

    private void AddNode(Vector2 vector2, Node startNode, ICollection<Node> nodesNeighbour)
    {
        nodesNeighbour.Add(new Node(vector2, startNode.TargetPosition, startNode, startNode.FromStartToTarget));
    }

    private void AddNodeIfCanBeDestroy(Vector2 vector2, Node startNode, ICollection<Node> nodesNeighbour)
    {
        var ray = new Ray(new Vector3(vector2.x, 1f, vector2.y), Vector3.down);
        var raycast = Physics.Raycast(ray, out var hit, 1f, CanBeDestroy);

        if (raycast)
        {
            nodesNeighbour.Add(new Node(vector2, startNode.TargetPosition, startNode, startNode.FromStartToTarget));
        }
    }

    private void AddNodeIfCanMove(Vector2 vector2, Node startNode, ICollection<Node> nodesNeighbour)
    {
        var ray = new Ray(new Vector3(vector2.x, 1f, vector2.y), Vector3.down);
        var raycast = Physics.Raycast(ray, out var hit, 1f, LayerMaskCantMove);

        if (!raycast)
        {
            nodesNeighbour.Add(new Node(vector2, startNode.TargetPosition, startNode, startNode.FromStartToTarget));
        }
    }

    //Use for debug
    private void OnDrawGizmos()
    {
        foreach (var node in ListChecked)
        {
            Gizmos.color = Color.white;

            Gizmos.DrawSphere(new Vector3(node.Position.x, 1.2f, node.Position.y), 0.1f);
        }

        foreach (var item in CanMovePoints)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(new Vector3(item.Position.x, 1.2f, item.Position.y), 0.2f);
        }

        if (PatchToTarget != null)
        {
            foreach (var item in PatchToTarget)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(new Vector3(item.x, 1.2f, item.y), 0.3f);
            }
        }
    }
}