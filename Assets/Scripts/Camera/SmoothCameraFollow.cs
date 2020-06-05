using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour {
    public float moveSpeed;
    public bool allowSmoothCameraFollow;

    private Vector3 _followedPosition;

    public void SetFollowedPosition(Vector3 pos) {
        _followedPosition = pos;
    }

    private void Update() {
        if (!allowSmoothCameraFollow) {
            return;
        }

        _followedPosition.z = transform.position.z;

        Vector3 direction = (_followedPosition - transform.position).normalized;
        float distance = Vector3.Distance(_followedPosition, transform.position);

        if(distance > 0f) {
            Vector3 newPos = transform.position + direction * distance * moveSpeed * Time.deltaTime;
            float distanceAfterMovement = Vector3.Distance(newPos, _followedPosition);
            if(distanceAfterMovement > distance) {
                newPos = _followedPosition;
            }
            transform.position = newPos;
        }
    }
}
