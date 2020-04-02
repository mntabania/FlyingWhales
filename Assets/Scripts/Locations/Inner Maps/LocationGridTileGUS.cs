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
            PathfindingManager.Instance.ApplyGraphUpdateSceneCoroutine(gus);
        }

        public void Destroy() {
            gus.setWalkability = true;
            PathfindingManager.Instance.ApplyGraphUpdateScene(gus);
            ObjectPoolManager.Instance.DestroyObject(this);
        }
    }
}
