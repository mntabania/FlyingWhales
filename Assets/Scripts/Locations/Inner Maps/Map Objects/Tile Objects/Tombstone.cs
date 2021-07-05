using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;

public class Tombstone : TileObject {
    public override Vector2 selectableSize => new Vector2(1f, 0.8f);
    public override Vector3 worldPosition {
        get {
            Vector3 pos = mapVisual.transform.position;
            pos.y += 0.15f;
            return pos;
        }
    }
    private bool _respawnCorpseOnDestroy; 
    public override Character[] users {
        get {
            _users[0] = character;
            return _users; 
        }
    }
    private Character[] _users;
    public Character character { get; private set; }

    public override System.Type serializedData => typeof(SaveDataTombstone);

    public Tombstone() : base(){
        AddAdvertisedAction(INTERACTION_TYPE.REMEMBER_FALLEN);
        AddAdvertisedAction(INTERACTION_TYPE.SPIT);
        AddAdvertisedAction(INTERACTION_TYPE.RAISE_CORPSE);
        AddAdvertisedAction(INTERACTION_TYPE.CARRY_CORPSE);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_CORPSE);
        _users = new Character[1];
        _respawnCorpseOnDestroy = true;
    }
    public Tombstone(SaveDataTombstone data) : base(data) {
        _users = new Character[1];
        _respawnCorpseOnDestroy = true;
    }
    public override void LoadSecondWave(SaveDataTileObject data) {
        base.LoadSecondWave(data);
        SaveDataTombstone saveDataTombstone = data as SaveDataTombstone;
        Assert.IsNotNull(saveDataTombstone);
        character = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataTombstone.characterID);
        if (character != null && character.race.IsSapient()) {
            AddPlayerAction(PLAYER_SKILL_TYPE.RAISE_DEAD);
        }
    }
    public override void LoadAdditionalInfo(SaveDataTileObject data) {
        base.LoadAdditionalInfo(data);
        if (isBeingCarriedBy != null && character != null && character.hasMarker) {
            character.DisableMarker();
            if (character.marker.nameplate) {
                character.marker.nameplate.UpdateNameActiveState();
            }
        }
    }
    public override void OnLoadPlacePOI() {
        DefaultProcessOnPlacePOI();
        character.marker.PlaceMarkerAt(gridTileLocation);
        character.DisableMarker();
        if (character.marker.nameplate) {
            character.marker.nameplate.UpdateNameActiveState();
        }
        character.marker.TryCancelExpiry();
        character.SetGrave(this);
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        character.marker.PlaceMarkerAt(gridTileLocation);
        character.DisableMarker();
        if (character.marker.nameplate) {
            character.marker.nameplate.UpdateNameActiveState();
        }
        character.marker.TryCancelExpiry();
        character.SetGrave(this);
        if(character.traitContainer.HasTrait("Plagued")) {
            PlagueDisease.Instance.AddPlaguedStatusOnPOIWithLifespanDuration(this);
        }
        if (character.race.IsSapient()) {
            AddPlayerAction(PLAYER_SKILL_TYPE.RAISE_DEAD);
        }
        Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, character as IPlayerActionTarget);
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        // RemovePlayerAction(PLAYER_SKILL_TYPE.RAISE_DEAD);
        if (_respawnCorpseOnDestroy) {
            if(previousTile != null) {
                character.EnableMarker();
                character.marker.ScheduleExpiry();
                character.marker.PlaceMarkerAt(previousTile);
                character.SetGrave(null);
                character.jobComponent.TriggerBuryMe();
            } else {
                if (character.marker) {
                    character.DestroyMarker();
                }
                if (character.currentRegion != null) {
                    character.currentRegion.RemoveCharacterFromLocation(character);
                }
                character.SetGrave(null);
            }
        } else {
            character.SetGrave(null);
            character.DestroyMarker();
            if (character.currentRegion != null) {
                character.currentRegion.RemoveCharacterFromLocation(character);
            }
        }
        Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, character as IPlayerActionTarget);
    }
    public void SetRespawnCorpseOnDestroy(bool state) {
        _respawnCorpseOnDestroy = state;
    }
    public override string ToString() {
        return $"Tombstone of {character?.name}";
    }
    public override void SetInventoryOwner(Character p_newOwner) {
        base.SetInventoryOwner(p_newOwner);
        if (p_newOwner != null) {
            if (character.hasMarker) {
                if (character.marker.nameplate) {
                    character.marker.nameplate.SetNameActiveState(false);
                }
            }
        }
        
    }
    public void SetCharacter(Character character) {
        this.character = character;
        Initialize(TILE_OBJECT_TYPE.TOMBSTONE, false);
    }
    public void SetCharacter(Character character, SaveDataTileObject data) {
        this.character = character;
    }

    #region Reactions
    public override void VillagerReactionToTileObject(Character actor, ref string debugLog) {
        base.VillagerReactionToTileObject(actor, ref debugLog);
        Character targetCharacter = this.character;
        //Dead targetDeadTrait = targetCharacter.traitContainer.GetNormalTrait<Dead>("Dead");
        if (targetCharacter != null && !targetCharacter.reactionComponent.charactersThatSawThisDead.Contains(actor)) { //targetDeadTrait != null && !targetDeadTrait.charactersThatSawThisDead.Contains(owner)
            targetCharacter.reactionComponent.AddCharacterThatSawThisDead(actor);
#if DEBUG_LOG
            debugLog = $"{debugLog}\n-Target saw dead for the first time";
#endif
            if (actor.traitContainer.HasTrait("Psychopath")) {
#if DEBUG_LOG
                debugLog = $"{debugLog}\n-Actor is Psychopath";
#endif
                if (targetCharacter.isNormalCharacter) {
#if DEBUG_LOG
                    debugLog = $"{debugLog}\n-Target is a normal character";
#endif
                    if (UnityEngine.Random.Range(0, 2) == 0) {
#if DEBUG_LOG
                        debugLog = $"{debugLog}\n-Target will Mock";
#endif
                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
                    } else {
#if DEBUG_LOG
                        debugLog = $"{debugLog}\n-Target will Laugh At";
#endif
                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
                    }
                }
            } else {
#if DEBUG_LOG
                debugLog = $"{debugLog}\n-Actor is not Psychopath";
#endif
                string opinionLabel = actor.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
#if DEBUG_LOG
                    debugLog = $"{debugLog}\n-Target is Friend/Close Friend";
#endif
                    if (UnityEngine.Random.Range(0, 2) == 0) {
#if DEBUG_LOG
                        debugLog = $"{debugLog}\n-Target will Cry";
#endif
                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, targetCharacter, $"saw dead {targetCharacter.name}");
                    } else {
#if DEBUG_LOG
                        debugLog = $"{debugLog}\n-Target will Puke";
#endif
                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, targetCharacter, $"saw dead {targetCharacter.name}");
                    }
                } else if ((actor.relationshipContainer.IsFamilyMember(targetCharacter) || actor.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                                && opinionLabel != RelationshipManager.Rival) {
#if DEBUG_LOG
                    debugLog = $"{debugLog}\n-Target is Relative/Lover/Affair and not Rival";
#endif
                    if (UnityEngine.Random.Range(0, 2) == 0) {
#if DEBUG_LOG
                        debugLog = $"{debugLog}\n-Target will Cry";
#endif
                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, targetCharacter, $"saw dead {targetCharacter.name}");
                    } else {
#if DEBUG_LOG
                        debugLog = $"{debugLog}\n-Target will Puke";
#endif
                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, targetCharacter, $"saw dead {targetCharacter.name}");
                    }
                } else if (opinionLabel == RelationshipManager.Enemy) {
#if DEBUG_LOG
                    debugLog = $"{debugLog}\n-Target is Enemy";
#endif
                    if (UnityEngine.Random.Range(0, 100) < 25) {
                        if (UnityEngine.Random.Range(0, 2) == 0) {
#if DEBUG_LOG
                            debugLog = $"{debugLog}\n-Target will Mock";
#endif
                            actor.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
                        } else {
#if DEBUG_LOG
                            debugLog = $"{debugLog}\n-Target will Laugh At";
#endif
                            actor.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
                        }
                    } else {
#if DEBUG_LOG
                        debugLog = $"{debugLog}\n-Shock";
#endif
                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, targetCharacter);
                    }
                } else if (opinionLabel == RelationshipManager.Rival) {
#if DEBUG_LOG
                    debugLog = $"{debugLog}\n-Target is Rival";
#endif
                    if (UnityEngine.Random.Range(0, 2) == 0) {
#if DEBUG_LOG
                        debugLog = $"{debugLog}\n-Target will Mock";
#endif
                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
                    } else {
#if DEBUG_LOG
                        debugLog = $"{debugLog}\n-Target will Laugh At";
#endif
                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
                    }
                } else if (targetCharacter.isNormalCharacter && actor.relationshipContainer.HasRelationshipWith(targetCharacter)) {
#if DEBUG_LOG
                    debugLog = $"{debugLog}\n-Otherwise, Shock";
#endif
                    actor.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, targetCharacter);
                }
            }
        }
    }
    #endregion
}

public class SaveDataTombstone : SaveDataTileObject {
    public string characterID;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        Tombstone obj = tileObject as Tombstone;
        Assert.IsNotNull(obj);
        characterID = obj.character.persistentID;
    }
}