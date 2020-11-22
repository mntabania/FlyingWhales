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
        get { return new[] { character }; }
    }
    public Character character { get; private set; }

    public override System.Type serializedData => typeof(SaveDataTombstone);

    public Tombstone() : base(){
        AddAdvertisedAction(INTERACTION_TYPE.REMEMBER_FALLEN);
        AddAdvertisedAction(INTERACTION_TYPE.SPIT);
        AddAdvertisedAction(INTERACTION_TYPE.RAISE_CORPSE);
        AddAdvertisedAction(INTERACTION_TYPE.CARRY_CORPSE);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_CORPSE);
        _respawnCorpseOnDestroy = true;
    }
    public Tombstone(SaveDataTombstone data) {
        _respawnCorpseOnDestroy = true;
    }
    public override void LoadSecondWave(SaveDataTileObject data) {
        base.LoadSecondWave(data);
        SaveDataTombstone saveDataTombstone = data as SaveDataTombstone;
        Assert.IsNotNull(saveDataTombstone);
        character = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataTombstone.characterID);
    }
    public override void OnLoadPlacePOI() {
        character.marker.PlaceMarkerAt(gridTileLocation);
        character.DisableMarker();
        character.marker.TryCancelExpiry();
        character.SetGrave(this);
        if (character.race.IsSapient()) {
            AddPlayerAction(SPELL_TYPE.RAISE_DEAD);
        }
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        character.marker.PlaceMarkerAt(gridTileLocation);
        character.DisableMarker();
        character.marker.TryCancelExpiry();
        character.SetGrave(this);
        if(character.traitContainer.HasTrait("Plagued")) {
            PlagueDisease.Instance.AddPlaguedStatusOnPOIWithLifespanDuration(this);
        }
        if (character.race.IsSapient()) {
            AddPlayerAction(SPELL_TYPE.RAISE_DEAD);
        }
        Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, character as IPlayerActionTarget);
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        RemovePlayerAction(SPELL_TYPE.RAISE_DEAD);
        if (_respawnCorpseOnDestroy) {
            if(previousTile != null) {
                character.EnableMarker();
                character.marker.ScheduleExpiry();
                character.marker.PlaceMarkerAt(previousTile);
                character.SetGrave(null);
                character.jobComponent.TriggerBuryMe();
            } else {
                if (character.currentRegion != null) {
                    character.currentRegion.RemoveCharacterFromLocation(character);
                }
                if (character.marker) {
                    character.DestroyMarker();
                }
                character.SetGrave(null);
            }
        } else {
            character.SetGrave(null);
            if (character.currentRegion != null) {
                character.currentRegion.RemoveCharacterFromLocation(character);
            }
            character.DestroyMarker();
        }
        Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, character as IPlayerActionTarget);
    }
    public void SetRespawnCorpseOnDestroy(bool state) {
        _respawnCorpseOnDestroy = state;
    }
    public override string ToString() {
        return $"Tombstone of {character?.name}";
    }

    public void SetCharacter(Character character) {
        this.character = character;
        Initialize(TILE_OBJECT_TYPE.TOMBSTONE, false);
    }
    public void SetCharacter(Character character, SaveDataTileObject data) {
        this.character = character;
    }
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