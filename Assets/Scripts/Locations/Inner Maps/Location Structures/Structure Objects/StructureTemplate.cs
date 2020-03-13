using EZObjectPools;
using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class StructureTemplate : PooledObject{
        public LocationStructureObject[] structureObjects;

        public void CheckForDestroy() {
            bool shouldBeDestroyed = true;
            for (int i = 0; i < structureObjects.Length; i++) {
                LocationStructureObject structureObject = structureObjects[i];
                if (structureObject.gameObject.activeSelf) {
                    shouldBeDestroyed = false;
                    break;
                }
            }
            if (shouldBeDestroyed) {
                ObjectPoolManager.Instance.DestroyObject(this);
            }
        }
        public override void Reset() {
            base.Reset();
            for (int i = 0; i < structureObjects.Length; i++) {
                LocationStructureObject structureObject = structureObjects[i];
                structureObject.Reset();
            }
        }
    }
}