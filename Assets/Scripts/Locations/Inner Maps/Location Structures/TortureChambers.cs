using System;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class TortureChambers : PartyStructure {
        private TortureChamberStructureObject _tortureChamberStructureObject;
        public override List<IStoredTarget> allPossibleTargets => PlayerManager.Instance.player.storedTargetsComponent.storedVillagers;
        public override string nameplateName => name;
        public List<LocationGridTile> borderTiles { get; private set; }
        public override Type serializedData => typeof(SaveDataTortureChambers);
        public TortureChambers(Region location) : base(STRUCTURE_TYPE.TORTURE_CHAMBERS, location){
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
            }
        }
        protected override void DestroyStructure(Character p_responsibleCharacter = null) {
            StopDrainingCharactersHere();
            base.DestroyStructure(p_responsibleCharacter);
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
        #endregion

        protected override void AfterCharacterAddedToLocation(Character p_character) {
            base.AfterCharacterAddedToLocation(p_character);
            p_character.movementComponent.SetEnableDigging(false);
            // if (p_character.isNormalCharacter && IsTilePartOfARoom(p_character.gridTileLocation, out var room) && room is PrisonCell prisonCell && prisonCell.skeleton == null) {
            //     DoorTileObject door = room.GetTileObjectInRoom<DoorTileObject>(); //close door in room
            //     door?.Close();
            // }
            Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
        }
        protected override void AfterCharacterRemovedFromLocation(Character p_character) {
            base.AfterCharacterRemovedFromLocation(p_character);
            p_character.movementComponent.SetEnableDigging(true);
            Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
        }
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.SNATCH_VILLAGER);
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
            List<Character> charactersToTeleport = RuinarchListPool<Character>.Claim();
            charactersToTeleport.AddRange(charactersHere);
            bool alreadyTeleportedCharacterInside = false;
            for (int i = 0; i < charactersToTeleport.Count; i++) {
                Character character = charactersToTeleport[i];
                if (!alreadyTeleportedCharacterInside && character.gridTileLocation != null && IsTilePartOfARoom(character.gridTileLocation, out var room) && room.parentStructure == this &&
                    room is PrisonCell prisonCell && prisonCell.IsValidOccupant(character)) {
                    //teleport character to center of room
                    LocationGridTile targetTile = room.GetCenterTile();
                    if (targetTile != null) {
                        alreadyTeleportedCharacterInside = true; //if already teleported character inside, then flag as true, so that other characters will be teleported outside
                        CharacterManager.Instance.Teleport(character, targetTile);
                        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Minion_Dissipate);    
                    }
                } else {
                    //teleport character to outside
                    LocationGridTile targetTile = CollectionUtilities.GetRandomElement(borderTiles);
                    if (targetTile != null) {
                        CharacterManager.Instance.Teleport(character, targetTile);
                        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Minion_Dissipate);    
                    }
                }
            }
            RuinarchListPool<Character>.Release(charactersToTeleport);
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