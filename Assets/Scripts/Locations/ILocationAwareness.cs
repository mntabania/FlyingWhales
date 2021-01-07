using System.Collections.Generic;

namespace Locations {
    public interface ILocationAwareness { 

        Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness { get; } //main list of awareness
        List<IPointOfInterest> pendingAwarenessToBeAdded { get; }
        List<IPointOfInterest> pendingAwarenessToBeRemoved { get; }

		#region add pending list
		void AddPendingAwarenessToPendingAddList(IPointOfInterest poi);
        void RemovePendingAwarenessFromPendingAddList(IPointOfInterest poi);
		#endregion

		#region remove pending list
		void AddPendingAwarenessToPendingRemoveList(IPointOfInterest poi);
        void RemovePendingAwarenessFromPendingRemoveList(IPointOfInterest poi);
		#endregion
		void UpdateAwareness();

		#region main list
		bool AddAwarenessToMainList(IPointOfInterest poi);
        void RemoveAwarenessFromMainList(IPointOfInterest poi);
		void RemoveAwarenessFromMainList(POINT_OF_INTEREST_TYPE poiType);
		#endregion

		bool HasAwareness(IPointOfInterest poi);
	}
}

