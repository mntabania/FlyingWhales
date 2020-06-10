using System;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using UnityEngine.Assertions;
namespace Quests.Steps {
    public class DropPOIAtStructureStep : QuestStep {
        
        private readonly Func<LocationStructure, IPointOfInterest, bool> _structureValidityChecker;
        private readonly Func<IPointOfInterest, bool> _poiValidityChecker;
        
        public DropPOIAtStructureStep([NotNull]System.Func<LocationStructure, IPointOfInterest, bool> structureValidityChecker,
            [NotNull]System.Func<IPointOfInterest, bool> poiValidityChecker, string stepDescription = "Drop character at structure") 
            : base(stepDescription) {
            _structureValidityChecker = structureValidityChecker;
            _poiValidityChecker = poiValidityChecker;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<IPointOfInterest>(Signals.ON_UNSEIZE_POI, CheckCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<IPointOfInterest>(Signals.ON_UNSEIZE_POI, CheckCompletion);
        }

        #region Listeners
        private void CheckCompletion(IPointOfInterest poi) {
            if (_poiValidityChecker.Invoke(poi)) {
                Assert.IsNotNull(poi.gridTileLocation, $"Dropped poi: {poi} has null gridTileLocation!");
                LocationStructure droppedAt = poi.gridTileLocation.structure;
                if (_structureValidityChecker.Invoke(droppedAt, poi)) {
                    Complete();
                }

            }
        }
        #endregion
    }
}