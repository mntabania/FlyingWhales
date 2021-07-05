using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class RuinedZoo : ManMadeStructure {

        private SUMMON_TYPE[] orderedMonsters = new[] {
            SUMMON_TYPE.Wolf,
            SUMMON_TYPE.Giant_Spider,
            SUMMON_TYPE.Troll,
            SUMMON_TYPE.Ghost,
            SUMMON_TYPE.Kobold,
            SUMMON_TYPE.Sludge,
            SUMMON_TYPE.Fire_Elemental,
            SUMMON_TYPE.Ice_Nymph,
            SUMMON_TYPE.Incubus,
            SUMMON_TYPE.Golem,
            SUMMON_TYPE.Abomination,
        };
        
        public RuinedZoo(Region location) : base(STRUCTURE_TYPE.RUINED_ZOO, location) {
            SetMaxHPAndReset(6000);
        }
        public RuinedZoo(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(6000);
        }

        #region Overrides
        public override void OnBuiltNewStructure() {
            base.OnBuiltNewStructure();
            Assert.IsTrue(orderedMonsters.Length == rooms.Length, $"Ruined zoo rooms are inconsistent with monster list! Monsters: {orderedMonsters}. Rooms: {rooms}");
            for (int i = 0; i < rooms.Length; i++) {
                StructureRoom structureRoom = rooms[i];
                SUMMON_TYPE summonType = orderedMonsters[i];
                Summon newSummon = CharacterManager.Instance.CreateNewSummon(summonType, FactionManager.Instance.neutralFaction, settlementLocation, region, this);
                List<LocationGridTile> tileChoices = structureRoom.tilesInRoom.Where(x => x.IsPassable()).ToList();
                LocationGridTile targetTile = CollectionUtilities.GetRandomElement(tileChoices);
                CharacterManager.Instance.PlaceSummonInitially(newSummon, targetTile);
                newSummon.combatComponent.SetCombatMode(COMBAT_MODE.Passive);
                newSummon.movementComponent.SetEnableDigging(false);
            }
        }
        protected override StructureRoom CreteNewRoomForStructure(List<LocationGridTile> tilesInRoom) {
            return new ZooCell(tilesInRoom);
        }
        #endregion
        
        #region Listeners
        protected override void SubscribeListeners() {
            base.SubscribeListeners();
            // Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
            Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
        }
        protected override void UnsubscribeListeners() {
            base.UnsubscribeListeners();
            // Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
            Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE, OnCharacterLeftStructure);
        }
        private void OnGameLoaded() {
            Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
            Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE, OnCharacterLeftStructure);
        }
        #endregion
        
        // private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
        //     if (structure == this && character is Summon && IsTilePartOfARoom(character.gridTileLocation, out var room) && room is ZooCell) {
        //         
        //     }
        // }
        protected override void DestroyStructure(Character p_responsibleCharacter = null, bool isPlayerSource = false) {
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                if (character is Summon summon && summon.homeStructure == this) {
                    RevertCombatModeAndDiggingToDefault(summon);
                }
            }
            base.DestroyStructure(p_responsibleCharacter, isPlayerSource);
        }
        private void OnCharacterLeftStructure(Character character, LocationStructure structure) {
            if (structure == this && character is Summon summon) {
                RevertCombatModeAndDiggingToDefault(summon);
            }
        }
        private void RevertCombatModeAndDiggingToDefault(Summon summon) {
            summon.combatComponent.SetCombatMode(summon.defaultCombatMode);
            summon.movementComponent.SetEnableDigging(summon.defaultDigMode);
        }
    }
}