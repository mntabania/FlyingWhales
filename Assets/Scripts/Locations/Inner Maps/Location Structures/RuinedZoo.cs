﻿using System.Collections.Generic;
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
            SUMMON_TYPE.Revenant,
            SUMMON_TYPE.Abomination,
        };
        
        public RuinedZoo(Region location) : base(STRUCTURE_TYPE.RUINED_ZOO, location) {
            SetMaxHPAndReset(6000);
        }
        public RuinedZoo(Region location, SaveDataLocationStructure data) : base(location, data) {
            SetMaxHP(6000);
        }

        #region Overrides
        public override void OnBuiltNewStructure() {
            Assert.IsTrue(orderedMonsters.Length == rooms.Length, $"Ruined zoo rooms are inconsistent with monster list! Monsters: {orderedMonsters}. Rooms: {rooms}");
            for (int i = 0; i < rooms.Length; i++) {
                StructureRoom structureRoom = rooms[i];
                SUMMON_TYPE summonType = orderedMonsters[i];
                Summon newSummon = CharacterManager.Instance.CreateNewSummon(summonType, FactionManager.Instance.neutralFaction, settlementLocation, location, this);
                LocationGridTile targetTile = CollectionUtilities.GetRandomElement(structureRoom.tilesInRoom);
                CharacterManager.Instance.PlaceSummon(newSummon, targetTile);
            }
        }
        protected override StructureRoom CreteNewRoomForStructure(List<LocationGridTile> tilesInRoom) {
            return new ZooCell(tilesInRoom);
        }
        #endregion
    }
}