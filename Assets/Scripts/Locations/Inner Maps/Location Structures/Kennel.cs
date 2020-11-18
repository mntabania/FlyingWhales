using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class Kennel : DemonicStructure {
        public override Vector2 selectableSize { get; }
        public override string nameplateName => $"{name}";
        private const int BreedingDuration = GameManager.ticksPerHour;
        
        private bool _isCurrentlyBreeding;
        private int _remainingBreedingTicks;
        private RaceClass _currentlyBreeding;
        private LocationGridTile targetTile;

        private readonly HashSet<Summon> _ownedSummons;

        private MarkerDummy _markerDummy;

        public Kennel(Region location) : base(STRUCTURE_TYPE.KENNEL, location){
            selectableSize = new Vector2(10f, 10f);
            _ownedSummons = new HashSet<Summon>();
        }
        public Kennel(Region location, SaveDataDemonicStructure data) : base(location, data) {
            selectableSize = new Vector2(10f, 10f);
            _ownedSummons = new HashSet<Summon>();
        }

        #region Overrides
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            _markerDummy = ObjectPoolManager.Instance
                .InstantiateObjectFromPool("MarkerDummy", Vector3.zero, Quaternion.identity, structureObj.objectsParent)
                .GetComponent<MarkerDummy>();
            _markerDummy.Deactivate();
        }
        protected override void DestroyStructure() {
            base.DestroyStructure();
            //RemoveBreedMonsterAction();
            //Messenger.RemoveListener(Signals.TICK_STARTED, PerSummonTick);
            if (_markerDummy != null) {
                ObjectPoolManager.Instance.DestroyObject(_markerDummy.gameObject);
            }
            //Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        }
        public bool HasReachedKennelCapacity() {
            int numOfSummons = GetNumberOfSummonsHere();
            return numOfSummons >= 3;
        }
        public int GetAvailableCapacity() {
            int numOfSummons = GetNumberOfSummonsHere();
            return 3 - numOfSummons;
        }
        #endregion
    }
}