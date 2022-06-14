using System.Collections;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class ExplodudesWeapon : Weapon
    {
        public enum GridSpawnMethods
        {
            NoGrid,
            LastCell,
            NextCell,
            Closest
        }

        [MMInspectorGroup("Explodudes Weapon", true, 23)]
        [Tooltip("the spawn method for this weapon")]
        public GridSpawnMethods GridSpawnMethod;
        [Tooltip("the offset to apply on spawn")]
        public Vector3 BombOffset;
        [Tooltip("the max amount of bombs a character can drop on screen at once")]
        public int MaximumAmountOfBombsAtOnce = 3;
        [Tooltip("the delay before the bomb explodes")]
        public float BombDelayBeforeExplosion = 3f;
        [MMReadOnly]
        [Tooltip("the amount of bombs remaining")]
        public int RemainingBombs = 0;

        protected MMSimpleObjectPooler _objectPool;
        protected Vector3 _newSpawnWorldPosition;
        protected bool _alreadyBombed = false;
        protected Vector3 _lastBombPosition;
        protected ExplodudesBomb _bomb;
        protected WaitForSeconds _addOneRemainingBomb;

        protected Vector3 _closestLast;
        protected Vector3 _closestNext;

        protected Vector3Int _cellPosition;
        public override void Initialization()
        {
            base.Initialization();
            _objectPool = this.gameObject.GetComponent<MMSimpleObjectPooler>();
            RemainingBombs = MaximumAmountOfBombsAtOnce;
            _addOneRemainingBomb = new WaitForSeconds(BombDelayBeforeExplosion);
        }
        public override void ShootRequest()
        {
            if (!m_PhotonView.IsMine && m_PhotonView == null)
            {
                return;
            }

            SpawnBomb();
        }
        protected virtual void SpawnBomb()
        {
            DetermineBombSpawnPosition();
            if (_alreadyBombed)
            {
                if (_lastBombPosition == _newSpawnWorldPosition)
                {
                    return;
                }
            }
            if (RemainingBombs <= 0)
            {
                return;
            }
            GameObject nextGameObject = _objectPool.GetPooledGameObject();

            if (nextGameObject == null)
            {
                return;
            }
            nextGameObject.transform.position = _newSpawnWorldPosition;
            _bomb = nextGameObject.MMGetComponentNoAlloc<ExplodudesBomb>();
            _bomb.Owner = Owner.gameObject;
            _bomb.BombDelayBeforeExplosion = BombDelayBeforeExplosion;
            nextGameObject.gameObject.SetActive(true);
            RemainingBombs--;
            StartCoroutine(AddOneRemainingBombCoroutine());
            WeaponState.ChangeState(WeaponStates.WeaponUse);
            _alreadyBombed = true;
            _lastBombPosition = _newSpawnWorldPosition;
        }
        protected virtual void DetermineBombSpawnPosition()
        {
            _newSpawnWorldPosition = this.transform.position;

            switch (GridSpawnMethod)
            {
                case GridSpawnMethods.NoGrid:
                    _newSpawnWorldPosition = this.transform.position;
                    break;
                case GridSpawnMethods.LastCell:
                    if (GridManager.Instance.LastPositions.ContainsKey(Owner.gameObject))
                    {
                        _cellPosition = GridManager.Instance.LastPositions[Owner.gameObject];
                        _newSpawnWorldPosition = GridManager.Instance.CellToWorldCoordinates(_cellPosition);
                    }

                    break;
                case GridSpawnMethods.NextCell:
                    if (GridManager.Instance.NextPositions.ContainsKey(Owner.gameObject))
                    {
                        _cellPosition = GridManager.Instance.NextPositions[Owner.gameObject];
                        _newSpawnWorldPosition = GridManager.Instance.CellToWorldCoordinates(_cellPosition);
                    }

                    break;
                case GridSpawnMethods.Closest:
                    if (GridManager.Instance.LastPositions.ContainsKey(Owner.gameObject))
                    {
                        _cellPosition = GridManager.Instance.LastPositions[Owner.gameObject];
                        _closestLast = GridManager.Instance.CellToWorldCoordinates(_cellPosition);
                    }

                    if (GridManager.Instance.NextPositions.ContainsKey(Owner.gameObject))
                    {
                        _cellPosition = GridManager.Instance.NextPositions[Owner.gameObject];
                        _closestNext = GridManager.Instance.CellToWorldCoordinates(_cellPosition);
                    }

                    if (Vector3.Distance(_closestLast, this.transform.position) < Vector3.Distance(_closestNext, this.transform.position))
                    {
                        _newSpawnWorldPosition = _closestLast;
                    }
                    else
                    {
                        _newSpawnWorldPosition = _closestNext;
                    }

                    break;
            }

            _newSpawnWorldPosition += BombOffset;
        }
        protected virtual IEnumerator AddOneRemainingBombCoroutine()
        {
            yield return _addOneRemainingBomb;

            RemainingBombs++;
            RemainingBombs = Mathf.Min(RemainingBombs, MaximumAmountOfBombsAtOnce);
        }
    }
}