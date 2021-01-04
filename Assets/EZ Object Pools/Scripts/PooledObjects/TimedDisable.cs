using UnityEngine;
using EZObjectPools;

[AddComponentMenu("EZ Object Pools/Pooled Objects/Timed Disable")]
public class TimedDisable : PooledObject
{
    float timer = 0;
    public float DisableTime;

    public bool paused;

    void OnEnable() {
        timer = 0;
    }

    void Update() {
        if (!paused) {
            timer += Time.deltaTime;

            if (timer > DisableTime) {
                //transform.SetParent(ParentPool.transform);
                SendObjectBackToPool();
                //gameObject.SetActive(false);
            }
        }
    }
}
