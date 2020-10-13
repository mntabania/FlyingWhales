using System;
using System.Collections;
using EZObjectPools;
using Pathfinding;
using UnityEngine;

namespace Inner_Maps {
    public class LocationGridTileGUS : PooledObject {
        [SerializeField] private GraphUpdateScene gus;
        [SerializeField] private BoxCollider2D boxCollider;

        private InnerTileMap _innerTileMap;
        
        public void Initialize(Vector2 offset, Vector2 size, InnerTileMap innerTileMap) {
            boxCollider.offset = offset;
            boxCollider.size = size;
            _innerTileMap = innerTileMap;
            transform.localPosition = Vector3.zero;
            gameObject.SetActive(true);
            DelayedApply();
        }

        public void Destroy() {
            InstantApply();
            ObjectPoolManager.Instance.DestroyObject(this);
        }

        [ContextMenu("Apply")]
        public void InstantApply() {
            // gus.Apply();
            Apply();
        }
        [ContextMenu("Apply Coroutine")]
        private void DelayedApply() {
            StartCoroutine(UpdateGraph());
        }
        private IEnumerator UpdateGraph() {
            yield return new WaitForSeconds(0.5f);
            // gus.Apply();
            Apply();
        }


        private void Apply() {
            // gus.Apply();
            GraphUpdateObject guo = new GraphUpdateObject(boxCollider.bounds) {nnConstraint = _innerTileMap.onlyUnwalkableGraph};
            PathfindingManager.Instance.UpdatePathfindingGraphPartialCoroutine(guo);

            guo = new TagGraphUpdateObject(boxCollider.bounds) {nnConstraint = _innerTileMap.onlyPathfindingGraph, updatePhysics = true, modifyWalkability = false};
            PathfindingManager.Instance.UpdatePathfindingGraphPartialCoroutine(guo);
        }
    }
}
