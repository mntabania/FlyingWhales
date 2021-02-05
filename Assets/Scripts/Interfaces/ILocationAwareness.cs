using System.Collections.Generic;

namespace Locations {
    public interface ILocationAwareness { 

        Dictionary<INTERACTION_TYPE, List<IPointOfInterest>> awareness { get; } //main list of awareness
        //List<IPointOfInterest> pendingAwarenessToBeAdded { get; }
        //List<IPointOfInterest> pendingAwarenessToBeRemoved { get; }
        //List<KeyValuePair<INTERACTION_TYPE, IPointOfInterest>> pendingSpecificAwarenessToBeRemoved { get; }
        //List<KeyValuePair<INTERACTION_TYPE, IPointOfInterest>> pendingSpecificAwarenessToBeAdded { get; }
        //bool flaggedForUpdate { get; }

        #region add pending list
        void AddSpecificAwarenessToPendingAddList(INTERACTION_TYPE actionType, IPointOfInterest poi);
        bool AddAwarenessToPendingAddList(IPointOfInterest poi);
        bool RemoveAwarenessFromPendingAddList(IPointOfInterest poi);
        #endregion

        #region remove pending list
        void AddSpecificAwarenessToPendingRemoveList(INTERACTION_TYPE actionType, IPointOfInterest poi);
        bool AddAwarenessToPendingRemoveList(IPointOfInterest poi);
        bool RemoveAwarenessFromPendingRemoveList(IPointOfInterest poi);
        #endregion

        #region Getting Awareness
        List<IPointOfInterest> GetListOfPOIBasedOnActionType(INTERACTION_TYPE actionType);
        #endregion

        void UpdateAwareness();
        //void SetFlaggedForUpdate(bool state);
        //bool HasPendingAddOrRemoveAwareness();

        #region main list
        bool AddAwarenessToMainList(IPointOfInterest poi);
        bool AddAwarenessToMainList(INTERACTION_TYPE actionType, IPointOfInterest pointOfInterest);
        void RemoveAwarenessFromMainList(IPointOfInterest poi);
        void RemoveAwarenessFromMainList(INTERACTION_TYPE actionType, IPointOfInterest pointOfInterest);
        #endregion

        bool HasAwareness(INTERACTION_TYPE actionType, IPointOfInterest poi);
	}
}

