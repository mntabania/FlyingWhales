using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Components;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class Kennel : PartyStructure, CharacterEventDispatcher.IDeathListener {
        public override string nameplateName => $"{name}";
        public Summon occupyingSummon => _occupyingSummon;
        public override Type serializedData => typeof(SaveDataKennel);
        public override List<IStoredTarget> allPossibleTargets => PlayerManager.Instance.player.storedTargetsComponent.storedMonsters;
        private MarkerDummy _markerDummy;
        private Summon _occupyingSummon;

        public Kennel(Region location) : base(STRUCTURE_TYPE.KENNEL, location){ }
        public Kennel(Region location, SaveDataKennel data) : base(location, data) { }

        #region Loading
        public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure) {
            allPossibleTargets = PlayerManager.Instance.player.storedTargetsComponent.storedMonsters;
            base.LoadReferences(saveDataLocationStructure);
            SaveDataKennel saveDataKennel = saveDataLocationStructure as SaveDataKennel;
            if (!string.IsNullOrEmpty(saveDataKennel.occupyingSummonID)) {
                _occupyingSummon = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataKennel.occupyingSummonID) as Summon;
                _occupyingSummon?.eventDispatcher.SubscribeToCharacterDied(this);
            }
            
        }
        #endregion
        
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
            if(preOccupiedBy != null) {
                return true;
            }
            int numOfSummons = GetNumberOfSummonsHere();
            return numOfSummons >= 1;
        }
        protected override void AfterCharacterAddedToLocation(Character p_character) {
            //In case there are multiple monsters inside kennel, only the first one will be counted.
            //Reference: https://www.notion.so/ruinarch/f5da33a23d5545298c66be49c3c767fd?v=1ebbd3791a3d477fb7818103643f9a41&p=595c5767c8684d2b91274f304058c4a1
            if (p_character is Summon summon) {
                if (_occupyingSummon == null && !summon.isDead) { //charactersHere.Count(c => c is Summon && !c.isDead) == 1
                    OccupyKennel(summon);    
                }
                summon.movementComponent.SetEnableDigging(false);
            }
        }
        protected override void AfterCharacterRemovedFromLocation(Character p_character) {
            if (p_character is Summon summon) {
                if (occupyingSummon == summon) {
                    UnOccupyKennelAndCheckForNewOccupant();    
                }
                summon.movementComponent.SetEnableDigging(true);
            }
        }
        public override string GetTestingInfo() {
            string info = base.GetTestingInfo();
            if (occupyingSummon != null) {
                info = $"{info}\nOccupying Summon: {occupyingSummon.name}";
            }
            return info;
        }
        public override void DeployParty() {
            base.DeployParty();
            party = PartyManager.Instance.CreateNewParty(partyData.deployedMinions[0]);
            partyData.deployedMinions[0].combatComponent.SetCombatMode(COMBAT_MODE.Defend);
            partyData.deployedSummons.ForEach((eachSummon) => party.AddMember(eachSummon));
            partyData.deployedSummons.ForEach((eachSummon) => eachSummon.combatComponent.SetCombatMode(COMBAT_MODE.Defend));

            partyData.deployedMinions[0].faction.partyQuestBoard.CreateDemonSnatchPartyQuest(partyData.deployedMinions[0],
                    partyData.deployedMinions[0].homeSettlement, partyData.deployedTargets[0] as Character, this);
            partyData.deployedTargets[0].isTargetted = true;
            party.TryAcceptQuest();
            party.AddMemberThatJoinedQuest(partyData.deployedMinions[0]);
            partyData.deployedSummons.ForEach((eachSummon) => party.AddMemberThatJoinedQuest(eachSummon));
            ListenToParty();
            PlayerManager.Instance.player.bookmarkComponent.AddBookmark(party, BOOKMARK_CATEGORY.Player_Parties);
        }
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.SNATCH_MONSTER);
        }
        #endregion

        private void OccupyKennel(Summon p_summon) {
            Assert.IsFalse(p_summon.isDead);
            Assert.IsNotNull(p_summon);
            _occupyingSummon = p_summon;
            occupyingSummon.eventDispatcher.SubscribeToCharacterDied(this);
            PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingMaxCharge(p_summon.summonType, p_summon.gainedKennelSummonCapacity, false);
            PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge(p_summon.summonType, p_summon.gainedKennelSummonCapacity);
            Debug.Log($"Set occupant of {name} to {occupyingSummon?.name}");
        }
        private void UnOccupyKennelAndCheckForNewOccupant() {
            Assert.IsNotNull(occupyingSummon, $"Problem un occupying summon at {name}");
            Debug.Log($"Removed {occupyingSummon.name} as occupant of {name}");
            occupyingSummon.eventDispatcher.UnsubscribeToCharacterDied(this);
            PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingMaxCharge(occupyingSummon.summonType, -occupyingSummon.gainedKennelSummonCapacity, false);
            PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge(occupyingSummon.summonType, -occupyingSummon.gainedKennelSummonCapacity);
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
        public void OnCharacterSubscribedToDied(Character p_character) {
            Assert.IsTrue(p_character == occupyingSummon, $"{name} is subscribed to death event of non occupying summon {p_character?.name}! Occupying summon is {occupyingSummon?.name}");
            UnOccupyKennelAndCheckForNewOccupant();
        }
    }
}

#region Save Data
public class SaveDataKennel : SaveDataPartyStructure {
    public string occupyingSummonID;

    public override void Save(LocationStructure structure) {
        base.Save(structure);
        Kennel kennel = structure as Kennel;
        if (kennel.occupyingSummon != null) {
            occupyingSummonID = kennel.occupyingSummon.persistentID;
        }
    }
}
#endregion