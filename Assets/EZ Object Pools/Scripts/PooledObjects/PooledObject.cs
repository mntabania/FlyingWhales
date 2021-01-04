using System;
using UnityEngine;

namespace EZObjectPools
{
    [AddComponentMenu("EZ Object Pools/Pooled Object")]
    public class PooledObject : BaseMonoBehaviour {
        /// <summary>
        /// The object pool this object originated from.
        /// </summary>
        [HideInInspector]
        public EZObjectPool ParentPool;

        /// <summary>
        /// [OBSOLETE] Simply calls gameObject.SetActive(false). No longer needed in your scripts.
        /// </summary>
        public virtual void Disable() {
            gameObject.SetActive(false);
        }

        /*
         * This will return an object to it's respective pool.
         * */
        public void SendObjectBackToPool() {
            gameObject.SetActive(false);
            transform.position = Vector3.zero;
            if (ParentPool) {
                if(ParentPool.transform != transform.parent) {
                    //To avoid duplicates in the available objects list, we must check if the game object is already in the object pool, if it is, do not add to available object anymore
                    transform.SetParent(ParentPool.transform);
                    ParentPool.AddToAvailableObjects(this.gameObject);
                }
            } else {
                throw new Exception(
                    $"PooledObject {gameObject.name} does not have a parent pool. If this occurred during a scene transition, ignore this. Otherwise report to developer.");
            }
        }

        public virtual void Reset() { }
        public virtual void BeforeDestroyActions() { }

        //void OnDisable()
        //{
        //    transform.position = Vector3.zero;

        //    if (ParentPool)
        //        ParentPool.AddToAvailableObjects(this.gameObject);
        //    else
        //        Debug.LogWarning("PooledObject " + gameObject.name + " does not have a parent pool. If this occurred during a scene transition, ignore this. Otherwise reoprt to developer.");
        //}
    }
}