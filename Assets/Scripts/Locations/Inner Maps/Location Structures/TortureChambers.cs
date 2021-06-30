using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class TortureChambers : PartyStructure {
        public override string scenarioDescription => "This Structure allows the Player to spawn a Snatch Party to abduct and imprison a Villager.\n\nImprisoned Villagers may be tortured, brainwashed or drained to produce Chaos Orbs.";

        private TortureChamberStructureObject _tortureChamberStructureObject;
        public override List<IStoredTarget> allPossibleTargets => PlayerManager.Instance.player.storedTargetsComponent.storedVillagers;
        public override string nameplateName => name;
        public List<LocationGridTile> borderTiles { get; private set; }
        public override Type serializedData => typeof(SaveDataTortureChambers);
        public TortureChambers(Region location) : base(STRUCTURE_TYPE.TORTURE_CHAMBERS, location){
            SetMaxHPAndReset(2500);
            nameWithoutID = "Prison";
            startingSummonCount = 2;
            name = $"{nameWithoutID} {id.ToString()}";
            borderTiles = new List<LocationGridTile>();
        }
        public TortureChambers(Region location, SaveDataTortureChambers data) : base(location, data) { }

        #region Overrides
        public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure) {
            base.LoadReferences(saveDataLocationStructure);
            SaveDataTortureChambers saveDataTortureChambers = saveDataLocationStructure as SaveDataTortureChambers;
            if (saveDataTortureChambers.borderTiles != null) {
                borderTiles = new List<LocationGridTile>();
                for (int i = 0; i < saveDataTortureChambers.borderTiles.Length; i++) {
                    TileLocationSave saveData = saveDataTortureChambers.borderTiles[i];
                    LocationGridTile tile = DatabaseManager.Instance.locationGridTileDatabase.GetTileBySavedData(saveData);
                    borderTiles.Add(tile);
                }
            }
        }
        public override void OnCharacterUnSeizedHere(Character character) {
            if (character.isNormalCharacter) {
                if (character.gridTileLocation != null && IsTilePartOfARoom(character.gridTileLocation, out var room)) {
                    DoorTileObject door = room.GetTileObjectInRoom<DoorTileObject>(); //close door in room
                    door?.Close();
                }
                character.traitContainer.RestrainAndImprison(character, null, PlayerManager.Instance.player.playerFaction);
                if (character.partyComponent.hasParty) {
                    //We remove the character from the party quest if he is put in the defiler so he will not dig out of it and do the quest
                    character.partyComponent.currentParty.RemoveMemberThatJoinedQuest(character);
                }
                if (character.gridTileLocation != null && !character.gridTileLocation.charactersHere.Contains(character)) {
                    character.gridTileLocation.AddCharacterHere(character);
                }
                Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
            }
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
        }
        public override void DeployParty() {
            base.DeployParty();
            party = PartyManager.Instance.CreateNewParty(partyData.deployedMinions[0], PARTY_QUEST_TYPE.Demon_Snatch);
            partyData.deployedMinions[0].combatComponent.SetCombatMode(COMBAT_MODE.Defend);
            partyData.deployedSummons.ForEach((eachSummon) => party.AddMember(eachSummon));
            partyData.deployedSummons.ForEach((eachSummon) => eachSummon.combatComponent.SetCombatMode(COMBAT_MODE.Defend));

            partyData.deployedMinions[0].faction.partyQuestBoard.CreateDemonSnatchPartyQuest(partyData.deployedMinions[0],
                    partyData.deployedMinions[0].homeSettlement, partyData.deployedTargets[0] as Character, this);
            party.TryAcceptQuest();
            partyData.deployedTargets[0].isTargetted = true;
            party.AddMemberThatJoinedQuest(partyData.deployedMinions[0]);
            partyData.deployedSummons.ForEach((eachSummon) => party.AddMemberThatJoinedQuest(eachSummon));
            ListenToParty();
            PlayerManager.Instance.player.bookmarkComponent.AddBookmark(party, BOOKMARK_CATEGORY.Player_Parties);
        }
        // private bool IsPartyStructureOccupied() {
        //     //only check room occupants, since we do not want to count occupants in unpassable parts of the structure
        //     //Reference: https://trello.com/c/EFAyp5Vn/4223-demonic-structure-appears-occupied
        //     if (rooms.Length > 0) {
        //         return rooms[0].charactersInRoom.Count > 0;    
        //     }
        //     return charactersHere.Count > 0;
        // }
        // public override bool IsAvailableForTargeting() {
        //     bool isOccupied = IsPartyStructureOccupied();
        //     var charactersToCheck = rooms.Length > 0 ? rooms[0].charactersInRoom : charactersHere;
        //     
        //     int deadCount = 0;
        //     charactersToCheck.ForEach((eachCharacter) => {
        //         if (eachCharacter.isDead) {
        //             deadCount++;
        //         }
        //     });
        //     if (charactersToCheck.Count > deadCount) {
        //         isOccupied = true;
        //     }
        //     return !isOccupied;
        // }
        public override bool IsAvailableForTargeting() {
            if (rooms.Length > 0 && rooms[0] is PrisonCell prisonCell) {
                //List<Character> charactersInRoom = prisonCell.charactersInRoom;
                //return !charactersInRoom.Any(prisonCell.IsValidOccupant); //can target character for snatch if prison does not currently have a valid occupant
                return !prisonCell.HasValidOccupant();
            }
            return false;
        }
        #endregion

        #region Listeners
        public override void OnCharacterDied(Character p_character) {
            base.OnCharacterDied(p_character);
            Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
        }
        #endregion

        protected override void AfterCharacterAddedToLocation(Character p_character) {
            base.AfterCharacterAddedToLocation(p_character);
            // if (p_character.isNormalCharacter && IsTilePartOfARoom(p_character.gridTileLocation, out var room) && room is PrisonCell prisonCell && prisonCell.skeleton == null) {
            //     DoorTileObject door = room.GetTileObjectInRoom<DoorTileObject>(); //close door in room
            //     door?.Close();
            // }
            // Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
        }
        protected override void AfterCharacterRemovedFromLocation(Character p_character) {
            base.AfterCharacterRemovedFromLocation(p_character);
            Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
        }
        public void OnSnatchedCharacterDroppedHere(Character character) {
            //In case there are multiple monsters inside kennel, only the first one will be counted.
            //Reference: https://www.notion.so/ruinarch/f5da33a23d5545298c66be49c3c767fd?v=1ebbd3791a3d477fb7818103643f9a41&p=595c5767c8684d2b91274f304058c4a1
            if (rooms.Length > 0 && rooms[0] is PrisonCell prisonCell && prisonCell.IsValidOccupant(character)) {
                //automatically restrain and imprison dropped monsters
                //Reference: https://trello.com/c/AlvDm0U6/4251-kennel-and-prison-updates
                character.traitContainer.RestrainAndImprison(character, factionThatImprisoned: PlayerManager.Instance.player.playerFaction);
            }
        }
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.SNATCH_VILLAGER);
            AddPlayerAction(PLAYER_SKILL_TYPE.LET_GO);
            AddPlayerAction(PLAYER_SKILL_TYPE.DRAIN_SPIRIT);
            AddPlayerAction(PLAYER_SKILL_TYPE.BRAINWASH);
            AddPlayerAction(PLAYER_SKILL_TYPE.TORTURE);
            AddPlayerAction(PLAYER_SKILL_TYPE.CREATE_BLACKMAIL);
        }
        public override string GetTestingInfo() {
            string summary = base.GetTestingInfo();
            return $"{summary}\nBorder Tiles({borderTiles.Count.ToString()}): {borderTiles.ComafyList()}";
        }

        #region Structure Object
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            _tortureChamberStructureObject = structureObj as TortureChamberStructureObject;
            Vector3 position = structureObj.transform.position;
            position.x -= 0.5f;
            position.y -= 0.5f;
            worldPosition = position;
        }
        public override void OnBuiltNewStructure() {
            base.OnBuiltNewStructure();
            _tortureChamberStructureObject.SetEntrance(region.innerMap);
            List<Character> characters = RuinarchListPool<Character>.Claim();
            characters.AddRange(charactersHere);
            if (rooms.FirstOrDefault() is PrisonCell prisonCell) {
                for (int i = 0; i < characters.Count; i++) {
                    Character character = characters[i];
                    //teleport character to inside prison cell
                    //automatically restrain and imprison accidentally captured characters
                    //Reference: https://trello.com/c/AlvDm0U6/4251-kennel-and-prison-updates
                    character.traitContainer.RestrainAndImprison(character, factionThatImprisoned: PlayerManager.Instance.player.playerFaction);
                    LocationGridTile targetTile = prisonCell.tilesInRoom.First(t => t.charactersHere.Count <= 0) ?? CollectionUtilities.GetRandomElement(prisonCell.tilesInRoom);
                    if (targetTile != null) {
                        CharacterManager.Instance.Teleport(character, targetTile);
                        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Minion_Dissipate);    
                    }
                }
            }
            RuinarchListPool<Character>.Release(characters);
        }
        public override void OnDoneLoadStructure() {
            _tortureChamberStructureObject.SetEntrance(region.innerMap);
        }
        #endregion

        #region Rooms
        protected override StructureRoom CreteNewRoomForStructure(List<LocationGridTile> tilesInRoom) {
            return new PrisonCell(tilesInRoom);
        }
        #endregion

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
        
        private void StopDrainingCharactersHere() {
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                character.traitContainer.RemoveTrait(character, "Being Drained");
            }
        }
    }
}

#region Save Data
public class SaveDataTortureChambers : SaveDataPartyStructure {
    public TileLocationSave[] borderTiles;
    public override void Save(LocationStructure locationStructure) {
        base.Save(locationStructure);
        TortureChambers tortureChambers = locationStructure as TortureChambers;
        borderTiles = new TileLocationSave[tortureChambers.borderTiles.Count];
        for (int i = 0; i < tortureChambers.borderTiles.Count; i++) {
            LocationGridTile tile = tortureChambers.borderTiles[i];
            borderTiles[i] = new TileLocationSave(tile);
        }

    }
}
#endregion