using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DefaultNamespace;
using MoreMountains.TopDownEngine;
using UnityEngine;

public enum Ai
{
    Default = 0,
    MoveSavePos = 2,
    SpawnBomb = 3,
}

public class AiBomber : MonoBehaviour
{
    public List<Vector2> AllBoard = new List<Vector2>();

    public List<Vector2> PatchToTarget = new List<Vector2>();
    private readonly List<Node> CanMovePoints = new List<Node>();
    private readonly List<Node> WaitingNode = new List<Node>();

    public GameObject Target;

    [SerializeField]
    private LayerMask LayerMaskCantMove;

    [SerializeField]
    private LayerMask CanBeDestroy;

    [SerializeField]
    private LayerMask UnDestroy;

    [SerializeField]
    private LayerMask Bomb;

    [SerializeField]
    private CharacterGridMovement m_CharacterGridMovement;

    [SerializeField]
    private CharacterHandleWeapon m_CharacterHandleWeapon;

    private Ai State;
    private List<Vector2> ListAnSavePos = new List<Vector2>();
    private Vector2 PosSavePos = new Vector2(int.MinValue, int.MinValue);

    private List<Vector2> ExplodudesBombs;
    private Vector2 movePosForDestroyPlayer;

    private void Awake()
    {
        for (int i = 0; i < 17; i++)
        {
            for (int j = 0; j < 11; j++)
            {
                AllBoard.Add(new Vector2(-7 + i, 5 - j));
            }
        }
    }

    private void Start()
    {
        StartCoroutine(TestCoroutine());
        StartCoroutine(TestCoroutine2());
    }

    private IEnumerator TestCoroutine2()
    {
        ListAnSavePos = new List<Vector2>();
        ExplodudesBombs = new List<Vector2>();

        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            CheckBomb();

            if (ExplodudesBombs.Count != 0)
            {
                foreach (Vector2 vector2 in ExplodudesBombs)
                {
                    ListAnSavePos.AddRange(AiHelper.AddRangeSpawnBomb(vector2));
                }
            }
        }
    }

    private void CheckBomb()
    {
        ExplodudesBombs = new List<Vector2>();

        foreach (var vector2 in AllBoard)
        {
            var ray = new Ray(new Vector3(vector2.x, 1f, vector2.y), Vector3.down);
            var raycast = Physics.Raycast(ray, out RaycastHit hit, 1f, Bomb);

            if (raycast)
            {
                ExplodudesBombs.Add(new Vector2(vector2.x, vector2.y));
            }
        }
    }

    private IEnumerator TestCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);

            var movePos = new Vector2(Target.transform.position.x, Target.transform.position.z);

            PatchToTarget = GetPathMove(movePos);

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
        var movePos = new Vector2(Target.transform.position.x, Target.transform.position.z);
        Vector2 targetPos = new Vector2(Mathf.Round(movePos.x), Mathf.Round(movePos.y));
        Vector2 currentPos = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.z));

        if (State == Ai.MoveSavePos)
        {
            MoveSavePos();
            return;
        }

        if (PatchToTarget.Count != 0)
        {
            if (movePosForDestroyPlayer == currentPos)
            {
                SpawnBomb();
                return;
            }

            movePosForDestroyPlayer = MovePosForDestroyPlayer(targetPos);

            if (movePosForDestroyPlayer != new Vector2(int.MinValue, int.MinValue))
            {
                var path = GetPathMove(movePosForDestroyPlayer);
                PatchToTarget = path;
            }
        }

        if (PatchToTarget.Count == 0 && State != Ai.MoveSavePos)
        {
            var path = GetPathMove(CalculateBestTarget().Position);
            PatchToTarget = path;

            if (path.Count == 0)
            {
                SpawnBomb();
            }
        }
    }

    private Vector2 MovePosForDestroyPlayer(Vector2 targetPos)
    {
        if (PatchToTarget.Count == 0)
            return new Vector2(int.MinValue, int.MinValue);

        for (int i = PatchToTarget.Count - 1; i > 0; i--)
        {
            var vector2 = PatchToTarget[i];

            var listSpawn = AiHelper.AddRangeSpawnBomb(vector2);

            foreach (var variable in listSpawn)
            {
                if (variable == targetPos)
                {
                    return vector2;
                }
            }
        }

        return new Vector2(int.MinValue, int.MinValue);
    }

    private void MoveSavePos()
    {
        var transformPosition = transform.position;

        Vector2 startPos = new Vector2(Mathf.Round(transformPosition.x), Mathf.Round(transformPosition.z));

        foreach (var vector2 in ExplodudesBombs)
        {
            if (startPos == vector2)
            {
                
            }
        }

        if (PosSavePos == new Vector2(int.MinValue, int.MinValue))
        {
            foreach (var node in CanMovePoints)
            {
                foreach (var vector2 in ListAnSavePos)
                {
                    if (node.Position != vector2)
                    {
                        PosSavePos = node.Position;
                    }
                }
            }
        }

        var savePos = new Vector2(PosSavePos.x, PosSavePos.y);

        if (savePos == startPos)
        {
            return;
        }

        PatchToTarget = GetPathMove(savePos);
    }

    private async void SpawnBomb()
    {
        State = Ai.MoveSavePos;
        m_CharacterHandleWeapon.CurrentWeapon.WeaponInputStart();
        m_CharacterHandleWeapon.CurrentWeapon.WeaponInputStop();

        var transformPosition = transform.position;

        Vector2 startPos = new Vector2(Mathf.Round(transformPosition.x), Mathf.Round(transformPosition.z));

        ListAnSavePos = AiHelper.AddRangeSpawnBomb(startPos);

        await Task.Delay(3500);
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
                var destroyRange = AiHelper.AddRangeSpawnBomb(keyValuePair.Key.Position);

                foreach (var node in CanMovePoints)
                {
                    var isDanger = destroyRange.FindAll(x => x == node.Position).Count > 0;

                    if (!isDanger)
                    {
                        valueDestroyBlocks = keyValuePair.Value;
                        nodeStart = keyValuePair.Key;
                    }
                }
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

    private List<Vector2> GetPathMove(Vector2 target)
    {
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

        for (int i = 1; i < 4; i++)
        {
            var findDestroyBlock = AddNodeIfCanBeDestroy(new Vector2(startNode.Position.x - i, startNode.Position.y), startNode, nodesNeighbour);

            if (findDestroyBlock)
            {
                break;
            }
        }

        for (int i = 1; i < 4; i++)
        {
            var findDestroyBlock = AddNodeIfCanBeDestroy(new Vector2(startNode.Position.x + i, startNode.Position.y), startNode, nodesNeighbour);

            if (findDestroyBlock)
            {
                break;
            }
        }

        for (int i = 1; i < 4; i++)
        {
            var findDestroyBlock = AddNodeIfCanBeDestroy(new Vector2(startNode.Position.x, startNode.Position.y - i), startNode, nodesNeighbour);

            if (findDestroyBlock)
            {
                break;
            }
        }

        for (int i = 1; i < 4; i++)
        {
            var findDestroyBlock = AddNodeIfCanBeDestroy(new Vector2(startNode.Position.x, startNode.Position.y + i), startNode, nodesNeighbour);

            if (findDestroyBlock)
            {
                break;
            }
        }

        return nodesNeighbour;
    }

    private void AddNode(Vector2 vector2, Node startNode, ICollection<Node> nodesNeighbour)
    {
        nodesNeighbour.Add(new Node(vector2, startNode.TargetPosition, startNode, startNode.FromStartToTarget));
    }

    private bool AddNodeIfCanBeDestroy(Vector2 vector2, Node startNode, ICollection<Node> nodesNeighbour)
    {
        var ray = new Ray(new Vector3(vector2.x, 1f, vector2.y), Vector3.down);
        var raycastCanDestroy = Physics.Raycast(ray, out var hit1, 1f, UnDestroy);

        if (raycastCanDestroy)
        {
            return true;
        }

        var raycast = Physics.Raycast(ray, out var hit, 1f, CanBeDestroy);

        if (raycast)
        {
            nodesNeighbour.Add(new Node(vector2, startNode.TargetPosition, startNode, startNode.FromStartToTarget));
            return true;
        }

        return false;
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
        if (CanMovePoints.Count != 0)
            foreach (var item in CanMovePoints)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(new Vector3(item.Position.x, 1.2f, item.Position.y), 0.2f);
            }

        if (ListAnSavePos.Count != 0)
            foreach (var item in ListAnSavePos)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(new Vector3(item.x, 1.2f, item.y), 0.2f);
            }

        if (PatchToTarget.Count != 0)
            foreach (var item in PatchToTarget)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(new Vector3(item.x, 1.2f, item.y), 0.2f);
            }
    }
}