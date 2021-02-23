using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Components;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class Kennel : DemonicStructure, CharacterEventDispatcher.IDeathListener {
        public override string nameplateName => $"{name}";
        private MarkerDummy _markerDummy;
        private Summon _occupyingSummon;

        public Kennel(Region location) : base(STRUCTURE_TYPE.KENNEL, location){ }
        public Kennel(Region location, SaveDataDemonicStructure data) : base(location, data) { }

        #region Overrides
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            _markerDummy = ObjectPoolManager.Instance.InstantiateObjectFromPool("MarkerDummy", Vector3.zero, Quaternion.identity, structureObj.objectsParent).GetComponent<MarkerDummy>();
            _markerDummy.Deactivate();
        }
        protected override void DestroyStructure() {
            StopDrainingCharactersHere();
            base.DestroyStructure();
            if (_markerDummy != null) {
                ObjectPoolManager.Instance.DestroyObject(_markerDummy.gameObject);
            }
        }
        public bool HasReachedKennelCapacity() {
            int numOfSummons = GetNumberOfSummonsHere();
            return numOfSummons >= 1;
        }
        protected override void AfterCharacterAddedToLocation(Character p_character) {
            //In case there are multiple monsters inside kennel, only the first one will be counted.
            //Reference: https://www.notion.so/ruinarch/f5da33a23d5545298c66be49c3c767fd?v=1ebbd3791a3d477fb7818103643f9a41&p=595c5767c8684d2b91274f304058c4a1
            if (p_character is Summon summon) {
                if (charactersHere.Count(c => c is Summon && !c.isDead) == 1) {
                    OccupyKennel(summon);    
                }
                summon.movementComponent.SetEnableDigging(false);
            }
        }
        protected override void AfterCharacterRemovedFromLocation(Character p_character) {
            if (p_character is Summon summon) {
                if (_occupyingSummon == summon) {
                    UnOccupyKennelAndCheckForNewOccupant();    
                }
                summon.movementComponent.SetEnableDigging(true);
            }
        }
        public override string GetTestingInfo() {
            string info = base.GetTestingInfo();
            if (_occupyingSummon != null) {
                info = $"{info}\nOccupying Summon: {_occupyingSummon.name}";
            }
            return info;
        }
        #endregion

        private void OccupyKennel(Summon p_summon) {
            Assert.IsNotNull(p_summon);
            _occupyingSummon = p_summon;
            _occupyingSummon.eventDispatcher.SubscribeToCharacterDied(this);
            PlayerManager.Instance.player.AdjustMonsterCapacity(p_summon.summonType, p_summon.gainedKennelSummonCapacity);
            PlayerManager.Instance.player.AdjustMonsterCharges(p_summon.summonType, p_summon.gainedKennelSummonCapacity);
            Debug.Log($"Set occupant of {name} to {_occupyingSummon?.name}");
        }
        private void UnOccupyKennelAndCheckForNewOccupant() {
            Assert.IsNotNull(_occupyingSummon, $"Problem un occupying summon at {name}");
            Debug.Log($"Removed {_occupyingSummon.name} as occupant of {name}");
            _occupyingSummon.eventDispatcher.UnsubscribeToCharacterDied(this);
            PlayerManager.Instance.player.AdjustMonsterCapacity(_occupyingSummon.summonType, -_occupyingSummon.gainedKennelSummonCapacity);
            PlayerManager.Instance.player.AdjustMonsterCharges(_occupyingSummon.summonType, -_occupyingSummon.gainedKennelSummonCapacity);
            _occupyingSummon = null;

            //in case there is another monster that is still at this kennel, then set the occupying monster to that monster, also add related charges
            Summon otherSummon = charactersHere.FirstOrDefault(c => c is Summon && !c.isDead) as Summon;
            if (otherSummon != null) {
                OccupyKennel(otherSummon);    
            }
        }
        
        private void StopDrainingCharactersHere() {
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                character.traitContainer.RemoveTrait(character, "Being Drained");
            }
        }
        public void OnCharacterDied(Character p_character) {
            Assert.IsTrue(p_character == _occupyingSummon, $"{name} is subscribed to death event of non occupying summon {p_character?.name}! Occupying summon is {_occupyingSummon?.name}");
            UnOccupyKennelAndCheckForNewOccupant();
        }
    }
}