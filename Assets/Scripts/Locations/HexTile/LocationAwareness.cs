using System.Collections.Generic;

namespace Locations {
    public class LocationAwareness : ILocationAwareness {
        #region ILocationAwareness implementation
        public Dictionary<INTERACTION_TYPE, List<IPointOfInterest>> awareness { get; } //main list of awareness
        //public List<IPointOfInterest> pendingAwarenessToBeAdded { get; }
        //public List<IPointOfInterest> pendingAwarenessToBeRemoved { get; }
        //public List<KeyValuePair<INTERACTION_TYPE, IPointOfInterest>> pendingSpecificAwarenessToBeRemoved { get; }
        //public List<KeyValuePair<INTERACTION_TYPE, IPointOfInterest>> pendingSpecificAwarenessToBeAdded { get; }
        //public bool flaggedForUpdate { get; private set; }

        public LocationAwareness() {
            awareness = new Dictionary<INTERACTION_TYPE, List<IPointOfInterest>>();
            //pendingAwarenessToBeAdded = new List<IPointOfInterest>();
            //pendingAwarenessToBeRemoved = new List<IPointOfInterest>();
            //pendingSpecificAwarenessToBeRemoved = new List<KeyValuePair<INTERACTION_TYPE, IPointOfInterest>>();
            //pendingSpecificAwarenessToBeAdded = new List<KeyValuePair<INTERACTION_TYPE, IPointOfInterest>>();
        }

        #region add pending list
        public void AddSpecificAwarenessToPendingAddList(INTERACTION_TYPE actionType, IPointOfInterest poi) {
            //pendingSpecificAwarenessToBeAdded.Add(new KeyValuePair<INTERACTION_TYPE, IPointOfInterest>(actionType, poi));
        }
        public bool AddAwarenessToPendingAddList(IPointOfInterest poi) {
            //if (!poi.isInPendingAwarenessList) {
                //pendingAwarenessToBeAdded.Add(poi);
            //    poi.SetIsInPendingAwarenessList(true);
            //    return true;
            //}
            return false;
        }
        public bool RemoveAwarenessFromPendingAddList(IPointOfInterest poi) {
            //if (poi.isInPendingAwarenessList) {
            //    poi.SetIsInPendingAwarenessList(false);
                //return pendingAwarenessToBeAdded.Remove(poi);
            //}
            return false;
        }
        #endregion

        #region remove pending list
        public void AddSpecificAwarenessToPendingRemoveList(INTERACTION_TYPE actionType, IPointOfInterest poi) {
            //pendingSpecificAwarenessToBeRemoved.Add(new KeyValuePair<INTERACTION_TYPE, IPointOfInterest>(actionType, poi));
        }
        public bool AddAwarenessToPendingRemoveList(IPointOfInterest poi) {
            //if (!poi.isInPendingAwarenessList) {
                //pendingAwarenessToBeRemoved.Add(poi);
            //    poi.SetIsInPendingAwarenessList(true);
            //    return true;
            //}
            return false;
        }
        public bool RemoveAwarenessFromPendingRemoveList(IPointOfInterest poi) {
            //if (poi.isInPendingAwarenessList) {
            //    poi.SetIsInPendingAwarenessList(false);
                //return pendingAwarenessToBeRemoved.Remove(poi);
            //}
            return false;
        }
        #endregion

        #region Getting Awareness
        public List<IPointOfInterest> GetListOfPOIBasedOnActionType(INTERACTION_TYPE actionType) {
            lock (MultiThreadPool.THREAD_LOCKER) {
                if (awareness.ContainsKey(actionType)) {
                    return awareness[actionType];
                }
                return null;
            }
        }
        #endregion

        //public void SetFlaggedForUpdate(bool state) {
            //flaggedForUpdate = state;
        //}
        public void UpdateAwareness() {
            //for (int i = 0; i < pendingAwarenessToBeRemoved.Count; i++) {
            //    IPointOfInterest poi = pendingAwarenessToBeRemoved[i];
            //    RemoveAwarenessFromMainList(poi);
            //    poi.SetIsInPendingAwarenessList(false);
            //}
            //for (int i = 0; i < pendingSpecificAwarenessToBeRemoved.Count; i++) {
            //    RemoveAwarenessFromMainList(pendingSpecificAwarenessToBeRemoved[i].Key, pendingSpecificAwarenessToBeRemoved[i].Value);
            //}
            //for (int i = 0; i < pendingAwarenessToBeAdded.Count; i++) {
            //    IPointOfInterest poi = pendingAwarenessToBeAdded[i];
            //    AddAwarenessToMainList(poi);
            //    poi.SetIsInPendingAwarenessList(false);
            //}
            //for (int i = 0; i < pendingSpecificAwarenessToBeAdded.Count; i++) {
            //    AddAwarenessToMainList(pendingSpecificAwarenessToBeAdded[i].Key, pendingSpecificAwarenessToBeAdded[i].Value);
            //}
            //pendingAwarenessToBeAdded.Clear();
            //pendingAwarenessToBeRemoved.Clear();
            //pendingSpecificAwarenessToBeRemoved.Clear();
            //pendingSpecificAwarenessToBeAdded.Clear();
            //SetFlaggedForUpdate(false);
        }
        //public bool HasPendingAddOrRemoveAwareness() {
        //    return pendingAwarenessToBeAdded.Count > 0 || pendingAwarenessToBeRemoved.Count > 0 || pendingSpecificAwarenessToBeAdded.Count > 0 || pendingSpecificAwarenessToBeRemoved.Count > 0;
        //}

        #region main list
        public bool AddAwarenessToMainList(IPointOfInterest pointOfInterest) {
            if (pointOfInterest == null || pointOfInterest.advertisedActions == null) {
                return false;
            }
            if(pointOfInterest.advertisedActions.Count > 0) {
                for (int i = 0; i < pointOfInterest.advertisedActions.Count; i++) {
                    INTERACTION_TYPE actionType = pointOfInterest.advertisedActions[i];
                    if (!HasAwareness(actionType, pointOfInterest)) {
                        if (!awareness.ContainsKey(actionType)) {
                            awareness.Add(actionType, new List<IPointOfInterest>());
                        }
                        awareness[actionType].Add(pointOfInterest);
                    }
                }
                pointOfInterest.SetCurrentLocationAwareness(this);
                return true;
            }
            return false;
        }
        public void RemoveAwarenessFromMainList(IPointOfInterest pointOfInterest) {
            for (int i = 0; i < pointOfInterest.advertisedActions.Count; i++) {
                INTERACTION_TYPE actionType = pointOfInterest.advertisedActions[i];
                if (awareness.ContainsKey(actionType)) {
                    awareness[actionType].Remove(pointOfInterest);
                }
            }
            if (pointOfInterest.currentLocationAwareness == this) {
                pointOfInterest.SetCurrentLocationAwareness(null);
            }
        }
        public bool AddAwarenessToMainList(INTERACTION_TYPE actionType, IPointOfInterest pointOfInterest) {
            if (!awareness.ContainsKey(actionType)) {
                awareness.Add(actionType, new List<IPointOfInterest>());
            }
            awareness[actionType].Add(pointOfInterest);
            return true;
            //Do not set current location awareness of poi since we only removed 1 action type, we did not add the whole poi to the awareness
        }
        public void RemoveAwarenessFromMainList(INTERACTION_TYPE actionType, IPointOfInterest pointOfInterest) {
            if (awareness.ContainsKey(actionType)) {
                awareness[actionType].Remove(pointOfInterest);
            }
            //Do not remove current location awareness of poi since we only removed 1 action type, we did not remove the whole poi from the awareness
        }
        #endregion

        public bool HasAwareness(INTERACTION_TYPE actionType, IPointOfInterest poi) {
            if (awareness.ContainsKey(actionType)) {
                List<IPointOfInterest> awarenesses = awareness[actionType];
                for (int i = 0; i < awarenesses.Count; i++) {
                    IPointOfInterest currPOI = awarenesses[i];
                    if (currPOI == poi) {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }
        #endregion
    }
}