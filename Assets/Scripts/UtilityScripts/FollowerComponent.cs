using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowerComponent : MonoBehaviour {

    private Transform targetTransform;
    private System.Action onReachTarget;
    private float speed = 5f;


    public void SetTarget(Transform target, System.Action onReachTarget) {
        Vector3 diff = target.position - transform.position;
        diff.Normalize();
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
        this.targetTransform = target;
        this.onReachTarget = onReachTarget;
        Messenger.AddListener<GameObject>(Signals.POOLED_OBJECT_DESTROYED, OnPooledObjectDestroyed);
    }

    private void FixedUpdate() {
        if (GameManager.Instance.isPaused) {
            return;
        }
        if (targetTransform == null) {
            return;
        }
        float step = speed * Time.deltaTime;
        // move sprite towards the target location
        transform.position = Vector2.MoveTowards(transform.position, targetTransform.position, step);

        Vector3 distance = transform.position - targetTransform.position;
        if (distance.magnitude < 1f) {
            //reached target
            OnReachTarget();
        }
    }

    private void OnReachTarget() {
        Debug.Log($"{this.name} has reached target");
        onReachTarget?.Invoke();
        targetTransform = null;
        GameObject.Destroy(this);
    }

    private void OnPooledObjectDestroyed(GameObject go) {
        if (go.transform == targetTransform) {
            targetTransform = null;
            GameObject.Destroy(this);
        }
    }
    
}
