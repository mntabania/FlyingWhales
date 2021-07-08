using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Components;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class Kennel : PartyStructure, CharacterEventDispatcher.IDeathListener {

        public override string scenarioDescription => "This Structure allows the Player to imprison a monster. The Kennel will slowly breed it. Each Kennel can breed up to 3 monsters. You may use these to spawn monster parties using your Maraud, Prism, Kennel or Prison.\n\nImprisoned monsters may also be drained to produce Chaos Orbs.";
        public override string nameplateName => $"{name}";
        public Summon occupyingSummon => _occupyingSummon;
        public override Type serializedData => typeof(SaveDataKennel);
        public override List<IStoredTarget> allPossibleTargets => PlayerManager.Instance.player.storedTargetsComponent.storedMonsters;
        public List<LocationGridTile> borderTiles { get; private set; }

        public override SUMMON_TYPE housedMonsterType => occupyingSummon != null ? occupyingSummon.summonType : SUMMON_TYPE.None;

        private MarkerDummy _markerDummy;
        private Summon _occupyingSummon;

        public Kennel(Region location) : base(STRUCTURE_TYPE.KENNEL, location) {
            SetMaxHPAndReset(2500);
            borderTiles = new List<LocationGridTile>();
        }
        public Kennel(Region location, SaveDataKennel data) : base(location, data) { }

        #region Loading
        public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure) {
            base.LoadReferences(saveDataLocationStructure);
            SaveDataKennel saveDataKennel = saveDataLocationStructure as SaveDataKennel;
            if (!string.IsNullOrEmpty(saveDataKennel.occupyingSummonID)) {
                _occupyingSummon = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataKennel.occupyingSummonID) as Summon;
                _occupyingSummon?.eventDispatcher.SubscribeToCharacterDied(this);
            }
            if (saveDataKennel.borderTiles != null) {
                borderTiles = new List<LocationGridTile>();
                for (int i = 0; i < saveDataKennel.borderTiles.Length; i++) {
                    TileLocationSave saveData = saveDataKennel.borderTiles[i];
                    LocationGridTile tile = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(saveData);
                    borderTiles.Add(tile);
                }
            }
        }
        #endregion
        
        #region Overrides
        protected override void AfterCharacterAddedToLocation(Character p_character) {
            base.AfterCharacterAddedToLocation(p_character);
            Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
        }
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            _markerDummy = ObjectPoolManager.Instance.InstantiateObjectFromPool("MarkerDummy", Vector3.zero, Quaternion.identity, structureObj.objectsParent).GetComponent<MarkerDummy>();
            _markerDummy.Deactivate();
            Vector3 position = structureObj.transform.position;
            position.x -= 0.5f;
            position.y -= 0.5f;
            worldPosition = position;
        }
        protected override void DestroyStructure(Character p_responsibleCharacter = null, bool isPlayerSource = false) {
            StopDrainingCharactersHere();
            List<Character> characters = RuinarchListPool<Character>.Claim();
            characters.AddRange(charactersHere);
            for (int i = 0; i < characters.Count; i++) {
                Character character = characters[i];
                character.traitContainer.RemoveRestrainAndImprison(character);
            }
            base.DestroyStructure(p_responsibleCharacter, isPlayerSource);
            if (_markerDummy != null) {
                ObjectPoolManager.Instance.DestroyObject(_markerDummy.gameObject);
            }
        }
        public bool HasReachedKennelCapacity() {
            if(preOccupiedBy != null) {
                return true;
            }
            // int numOfSummons = GetNumberOfSummonsHere();
            // return numOfSummons >= 1;
            
            //checked characters instead because of this issue
            //https://trello.com/c/AKcwTMkY/5383-drop-lycanwerewolf-form-kennel-bug
            return HasCharactersInside();
        }
        public override void OnCharacterUnSeizedHere(Character character) {
            base.OnCharacterUnSeizedHere(character);
            //In case there are multiple monsters inside kennel, only the first one will be counted.
            //Reference: https://www.notion.so/ruinarch/f5da33a23d5545298c66be49c3c767fd?v=1ebbd3791a3d477fb7818103643f9a41&p=595c5767c8684d2b91274f304058c4a1
            if (character is Summon summon) {
                //automatically restrain and imprison dropped monsters
                //Reference: https://trello.com/c/AlvDm0U6/4251-kennel-and-prison-updates
                summon.traitContainer.RestrainAndImprison(summon, factionThatImprisoned: PlayerManager.Instance.player.playerFaction);
                if (_occupyingSummon == null && IsValidOccupant(summon)) { //charactersHere.Count(c => c is Summon && !c.isDead) == 1
                    OccupyKennel(summon);    
                }
            }
        }
        public void OnSnatchedCharacterDroppedHere(Character character) {
            //In case there are multiple monsters inside kennel, only the first one will be counted.
            //Reference: https://www.notion.so/ruinarch/f5da33a23d5545298c66be49c3c767fd?v=1ebbd3791a3d477fb7818103643f9a41&p=595c5767c8684d2b91274f304058c4a1
            if (character is Summon summon) {
                //automatically restrain and imprison dropped monsters
                //Reference: https://trello.com/c/AlvDm0U6/4251-kennel-and-prison-updates
                summon.traitContainer.RestrainAndImprison(summon, factionThatImprisoned: PlayerManager.Instance.player.playerFaction);
                if (_occupyingSummon == null && IsValidOccupant(summon)) { //charactersHere.Count(c => c is Summon && !c.isDead) == 1
                    OccupyKennel(summon);    
                }
            }
        }
        public void OnHarpyDroppedCharacterHere(Character character) {
            //In case there are multiple monsters inside kennel, only the first one will be counted.
            //Reference: https://www.notion.so/ruinarch/f5da33a23d5545298c66be49c3c767fd?v=1ebbd3791a3d477fb7818103643f9a41&p=595c5767c8684d2b91274f304058c4a1
            if (character is Summon summon) {
                //automatically restrain and imprison dropped monsters
                //Reference: https://trello.com/c/AlvDm0U6/4251-kennel-and-prison-updates
                summon.traitContainer.RestrainAndImprison(summon, factionThatImprisoned: PlayerManager.Instance.player.playerFaction);
                if (_occupyingSummon == null && IsValidOccupant(summon)) { //charactersHere.Count(c => c is Summon && !c.isDead) == 1
                    OccupyKennel(summon);    
                }
            }
        }
        protected override void AfterCharacterRemovedFromLocation(Character p_character) {
            if (p_character is Summon summon) {
                if (occupyingSummon == summon) {
                    UnOccupyKennelAndCheckForNewOccupant();    
                }
            }
        }
        public override string GetTestingInfo() {
            string info = base.GetTestingInfo();
            if (occupyingSummon != null) {
                info = $"{info}\nOccupying Summon: {occupyingSummon.name}";
            }
            info = $"{info}\nBorder Tiles({borderTiles.Count.ToString()}): {borderTiles.ComafyList()}";
            return info;
        }
        public override void DeployParty() {
            base.DeployParty();
            party = PartyManager.Instance.CreateNewParty(partyData.deployedMinions[0], PARTY_QUEST_TYPE.Demon_Snatch);
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
            AddPlayerAction(PLAYER_SKILL_TYPE.LET_GO);
            AddPlayerAction(PLAYER_SKILL_TYPE.DRAIN_SPIRIT);
        }
        public override void OnBuiltNewStructure() {
            base.OnBuiltNewStructure();
            //automatically teleport characters caught inside the kennel upon building
            //this is to prevent complications with occupying summon.
            List<Character> characters = RuinarchListPool<Character>.Claim();
            characters.AddRange(charactersHere);
            for (int i = 0; i < characters.Count; i++) {
                Character character = characters[i];
                //automatically restrain and imprison accidentally captured characters
                //Reference: https://trello.com/c/AlvDm0U6/4251-kennel-and-prison-updates
                character.traitContainer.RestrainAndImprison(character, factionThatImprisoned: PlayerManager.Instance.player.playerFaction);
                //teleport monster to inside of kennel
                LocationGridTile chosenTile = passableTiles.FirstOrDefault(t => t.charactersHere.Count <= 0) ?? GetCenterTile();
                if (chosenTile != null) {
                    CharacterManager.Instance.Teleport(character, chosenTile);
                    GameManager.Instance.CreateParticleEffectAt(chosenTile, PARTICLE_EFFECT.Minion_Dissipate);    
                }
                
                if (_occupyingSummon == null && character is Summon summon && IsValidOccupant(summon)) {
                    OccupyKennel(summon);
                }
            }
            RuinarchListPool<Character>.Release(characters);
            // if (_occupyingSummon != null) {
            //     Character characterToTeleport = _occupyingSummon;
            //     if (!IsValidOccupant(_occupyingSummon)) {
            //         UnOccupyKennelAndCheckForNewOccupant();
            //         LocationGridTile targetTile = CollectionUtilities.GetRandomElement(borderTiles);
            //         if (targetTile != null) {
            //             CharacterManager.Instance.Teleport(characterToTeleport, targetTile);
            //             GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Minion_Dissipate);    
            //         }
            //     }
            // }
        }
        public LocationGridTile GetCenterTile() {
            return GameUtilities.GetCenterTile(tiles.ToList(), tiles.ElementAt(0).parentMap.map);
        }
        public override bool IsAvailableForTargeting() {
            // return _occupyingSummon == null;
            
            //checked characters instead because of this issue
            //https://trello.com/c/AKcwTMkY/5383-drop-lycanwerewolf-form-kennel-bug
            return !HasCharactersInside();
        }
        public bool HasCharactersInside() {
            int charactersHereCount = charactersHere.Count; 
            return charactersHereCount > 0;
        }
        #endregion

        private void OccupyKennel(Summon p_summon) {
            Assert.IsFalse(p_summon.isDead);
            Assert.IsNotNull(p_summon);
            _occupyingSummon = p_summon;
            occupyingSummon.eventDispatcher.SubscribeToCharacterDied(this);
            startingSummonCount = 2;
            PlayerManager.Instance.player.underlingsComponent.GainMonsterUnderlingMaxChargesFromKennel(p_summon.summonType, p_summon.gainedKennelSummonCapacity);
            // PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge(p_summon.summonType, p_summon.gainedKennelSummonCapacity);
#if DEBUG_LOG
            Debug.Log($"Set occupant of {name} to {occupyingSummon?.name}");
#endif
            Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
        }
        private void UnOccupyKennelAndCheckForNewOccupant() {
            Assert.IsNotNull(occupyingSummon, $"Problem un occupying summon at {name}");
#if DEBUG_LOG
            Debug.Log($"Removed {occupyingSummon.name} as occupant of {name}");
#endif
            occupyingSummon.eventDispatcher.UnsubscribeToCharacterDied(this);
            PlayerManager.Instance.player.underlingsComponent.LoseMonsterUnderlingMaxChargesFromKennel(occupyingSummon.summonType, -occupyingSummon.gainedKennelSummonCapacity);
            // PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingCharge(occupyingSummon.summonType, -occupyingSummon.gainedKennelSummonCapacity);
            _occupyingSummon = null;

            //in case there is another monster that is still at this kennel, then set the occupying monster to that monster, also add related charges
            Summon otherSummon = charactersHere.FirstOrDefault(IsValidOccupant) as Summon;
            if (otherSummon != null) {
                OccupyKennel(otherSummon);    
            } else {
                Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
            }
        }
        private bool IsValidOccupant(Character p_character) {
            if (p_character is Summon summon) {
                if (summon.isDead) {
                    return false;
                }
                if (summon.faction != null && summon.faction.isPlayerFaction) {
                    return false;
                }
                if (!summon.traitContainer.HasTrait("Restrained")) {
                    return false;
                }
                // if (!summon.isBeingSeized && summon.gridTileLocation != null && !summon.gridTileLocation.IsPassable()) {
                //     //needed to check for impassable tile placements
                //     //Reference: https://trello.com/c/EFAyp5Vn/4223-demonic-structure-appears-occupied
                //     return false;
                // }
                return true;
            }
            return false;
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
            Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
        }
        
#region Border Tiles
        public void AddBorderTile(LocationGridTile p_tile) {
            if (!borderTiles.Contains(p_tile)) {
                borderTiles.Add(p_tile);    
            }
        }
        public void RemoveBorderTile(LocationGridTile p_tile) {
            borderTiles.Remove(p_tile);
        }
        public LocationGridTile GetRandomPassableBorderTile() {
            List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
            for (int i = 0; i < borderTiles.Count; i++) {
                LocationGridTile border = borderTiles[i];
                if (border.IsPassable()) {
                    tiles.Add(border);
                }
            }
            LocationGridTile chosenTile = null;
            if (tiles.Count > 0) {
                chosenTile = tiles[GameUtilities.RandomBetweenTwoNumbers(0, tiles.Count - 1)];
            }
            RuinarchListPool<LocationGridTile>.Release(tiles);
            return chosenTile;
        }
        public LocationGridTile GetRandomBorderTile() {
            return CollectionUtilities.GetRandomElement(borderTiles);
        }
#endregion

    }
}

#region Save Data
public class SaveDataKennel : SaveDataPartyStructure {
    public string occupyingSummonID;
    public TileLocationSave[] borderTiles;
    public override void Save(LocationStructure structure) {
        base.Save(structure);
        Kennel kennel = structure as Kennel;
        if (kennel.occupyingSummon != null) {
            occupyingSummonID = kennel.occupyingSummon.persistentID;
        }
        borderTiles = new TileLocationSave[kennel.borderTiles.Count];
        for (int i = 0; i < kennel.borderTiles.Count; i++) {
            LocationGridTile tile = kennel.borderTiles[i];
            borderTiles[i] = new TileLocationSave(tile);
        }
    }
}
#endregion