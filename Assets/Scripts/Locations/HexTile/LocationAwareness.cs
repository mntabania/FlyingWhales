using System.Collections.Generic;

namespace Locations {
    public class LocationAwareness : ILocationAwareness {
        #region ILocationAwareness implementation
        public Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness { get; } //main list of awareness
        public List<IPointOfInterest> pendingAwarenessToBeAdded { get; }
        public List<IPointOfInterest> pendingAwarenessToBeRemoved { get; }
        #region add pending list
        public void AddPendingAwarenessToPendingAddList(IPointOfInterest poi) {
            pendingAwarenessToBeAdded.Add(poi);
        }
        public void RemovePendingAwarenessFromPendingAddList(IPointOfInterest poi) {
            pendingAwarenessToBeAdded.Remove(poi);
        }
        #endregion

        #region remove pending list
        public void AddPendingAwarenessToPendingRemoveList(IPointOfInterest poi) {
            pendingAwarenessToBeRemoved.Add(poi);
        }
        public void RemovePendingAwarenessFromPendingRemoveList(IPointOfInterest poi) {
            pendingAwarenessToBeRemoved.Remove(poi);
        }
        #endregion
        public void UpdateAwareness() {
            for (int i = 0; i < pendingAwarenessToBeAdded.Count; i++) {
                AddAwarenessToMainList(pendingAwarenessToBeAdded[i]);
            }
            for (int i = 0; i < pendingAwarenessToBeRemoved.Count; i++) {
                RemoveAwarenessFromMainList(pendingAwarenessToBeRemoved[i]);
            }
            pendingAwarenessToBeAdded.Clear();
            pendingAwarenessToBeRemoved.Clear();
        }

        #region main list
        public bool AddAwarenessToMainList(IPointOfInterest pointOfInterest) {
            if (pointOfInterest == null) {
                return false;
            }
            if (!HasAwareness(pointOfInterest)) {
                if (!awareness.ContainsKey(pointOfInterest.poiType)) {
                    awareness.Add(pointOfInterest.poiType, new List<IPointOfInterest>());
                }
                awareness[pointOfInterest.poiType].Add(pointOfInterest);
                return true;
            }
            return false;
        }
        public void RemoveAwarenessFromMainList(IPointOfInterest pointOfInterest) {
            if (awareness.ContainsKey(pointOfInterest.poiType)) {
                List<IPointOfInterest> awarenesses = awareness[pointOfInterest.poiType];
                for (int i = 0; i < awarenesses.Count; i++) {
                    IPointOfInterest iawareness = awarenesses[i];
                    if (iawareness == pointOfInterest) {
                        awarenesses.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        public void RemoveAwarenessFromMainList(POINT_OF_INTEREST_TYPE poiType) {
            if (awareness.ContainsKey(poiType)) {
                awareness.Remove(poiType);
            }
        }
        #endregion

        public bool HasAwareness(IPointOfInterest poi) {
            if (awareness.ContainsKey(poi.poiType)) {
                List<IPointOfInterest> awarenesses = awareness[poi.poiType];
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