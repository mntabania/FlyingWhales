using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tombstone : TileObject {

    public override Character[] users {
        get { return new Character[] { this.character }; }
    }

    public Character character { get; private set; }
    public Tombstone() {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.REMEMBER_FALLEN, INTERACTION_TYPE.SPIT, INTERACTION_TYPE.BUTCHER };
        //Initialize(TILE_OBJECT_TYPE.TOMBSTONE);
    }
    public Tombstone(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.REMEMBER_FALLEN, INTERACTION_TYPE.SPIT, INTERACTION_TYPE.BUTCHER };
        //Initialize(data);
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        character.DisableMarker();
        character.SetGrave(this);
    }
    public virtual void OnClickAction() {
        UIManager.Instance.ShowCharacterInfo(character, true);
    }

    public override string ToString() {
        return $"Tombstone of {character.name}";
    }

    public void SetCharacter(Character character) {
        this.character = character;
        Initialize(TILE_OBJECT_TYPE.TOMBSTONE);
    }
    public void SetCharacter(Character character, SaveDataTileObject data) {
        this.character = character;
        Initialize(data);
    }
}

public class SaveDataTombstone : SaveDataTileObject {
    public int characterID;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        Tombstone obj = tileObject as Tombstone;
        characterID = obj.character.id;
    }

    public override TileObject Load() {
        Tombstone obj = base.Load() as Tombstone;
        obj.SetCharacter(CharacterManager.Instance.GetCharacterByID(characterID), this);
        return obj;
    }
}