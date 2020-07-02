using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTester : MonoBehaviour {

    [SerializeField] private GameObject[] projectilePrefabs;
    [SerializeField] private Transform target;
    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            CreateNewProjectile();
        }
    }

    private void CreateNewProjectile() {
        Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        position.z = 0f;
        GameObject randomPrefab = UtilityScripts.CollectionUtilities.GetRandomElement(projectilePrefabs);
        // randomPrefab = projectilePrefabs[3];
        GameObject projectileGO = GameObject.Instantiate(randomPrefab, position, Quaternion.identity);
        Projectile projectile = projectileGO.GetComponent<Projectile>();
        projectile.SetTarget(target, null, null, null);
    }
}
