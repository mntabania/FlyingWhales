using System;
using System.Collections;
using EZObjectPools;
using Pathfinding;
using UnityEngine;

namespace Inner_Maps {
    public class LocationGridTileGUS : PooledObject {
        [SerializeField] private GraphUpdateScene gus;
        [SerializeField] private BoxCollider2D boxCollider;
        
        public void Initialize(Vector2 offset, Vector2 size) {
            boxCollider.offset = offset;
            boxCollider.size = size;
            gus.setWalkability = false;
            transform.localPosition = Vector3.zero;
            gameObject.SetActive(true);
            ApplyCoroutine();
        }

        public void Destroy() {
            gus.setWalkability = true;
            Apply();
            ObjectPoolManager.Instance.DestroyObject(this);
        }

        [ContextMenu("Apply")]
        public void Apply() {
            PathfindingManager.Instance.ApplyGraphUpdateScene(gus);
        }
        [ContextMenu("Apply Coroutine")]
        private void ApplyCoroutine() {
            StartCoroutine(UpdateGraph());
        }
        
        private IEnumerator UpdateGraph() {
            yield return new WaitForSeconds(0.5f);
            gus.Apply();
        }
    }
}
