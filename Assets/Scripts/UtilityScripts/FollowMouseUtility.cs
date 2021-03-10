using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouseUtility : MonoBehaviour {

    [SerializeField] private Camera _camera;
    // Update is called once per frame
    void Update() {
        Vector3 pos = Input.mousePosition;
        transform.position = pos;
    }
}
