﻿using System;
using System.Collections.Generic;using Characters.Components;
using Inner_Maps.Location_Structures;

public class StoredTargetsComponent : CharacterEventDispatcher.IDeathListener, TileObjectEventDispatcher.IDestroyedListener, LocationStructureEventDispatcher.IDestroyedListener {

    public const int MaxCapacity = 8;
    public List<IStoredTarget> allStoredTargets { get; }
    public List<IStoredTarget> storedVillagers { get; }
    public List<IStoredTarget> storedMonsters { get; }
    public List<IStoredTarget> storedTileObjects { get; }
    public List<IStoredTarget> storedStructures { get; }

    public StoredTargetsComponent() {
        allStoredTargets = new List<IStoredTarget>();
        storedVillagers = new List<IStoredTarget>();
        storedMonsters = new List<IStoredTarget>();
        storedTileObjects = new List<IStoredTarget>();
        storedStructures = new List<IStoredTarget>();
    }
    
    public void Store(IStoredTarget p_target) {
        allStoredTargets.Add(p_target);
        switch (p_target.storedTargetType) {
            case STORED_TARGET_TYPE.Monster:
                Store(p_target as Summon);
                break;
            case STORED_TARGET_TYPE.Character:
                Store(p_target as Character);
                break;
            case STORED_TARGET_TYPE.Tile_Objects:
                Store(p_target as TileObject);
                break;
            case STORED_TARGET_TYPE.Structures:
                Store(p_target as LocationStructure);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        Messenger.Broadcast(PlayerSignals.PLAYER_STORED_TARGET, p_target);
        if (allStoredTargets.Count > MaxCapacity) {
            //Remove oldest stored target
            IStoredTarget oldestTarget = allStoredTargets[0];
            Remove(oldestTarget);
        }
    }
    public bool HasStoredMaxCapacity() {
        return allStoredTargets.Count >= MaxCapacity;
    }
    private void Store(Summon p_monster) {
        storedMonsters.Add(p_monster);
        p_monster.eventDispatcher.SubscribeToCharacterDied(this);
    }
    private void Store(Character p_character) {
        storedVillagers.Add(p_character);
        p_character.eventDispatcher.SubscribeToCharacterDied(this);
    }
    private void Store(TileObject p_tileObject) {
        storedTileObjects.Add(p_tileObject);
        p_tileObject.eventDispatcher.SubscribeToTileObjectDestroyed(this);
    }
    private void Store(LocationStructure p_structure) {
        storedStructures.Add(p_structure);
        p_structure.eventDispatcher.SubscribeToStructureDestroyed(this);
    }
    public void Remove(IStoredTarget p_target) {
        allStoredTargets.Remove(p_target);
        switch (p_target.storedTargetType) {
            case STORED_TARGET_TYPE.Monster:
                Remove(p_target as Summon);
                break;
            case STORED_TARGET_TYPE.Character:
                Remove(p_target as Character);
                break;
            case STORED_TARGET_TYPE.Tile_Objects:
                Remove(p_target as TileObject);
                break;
            case STORED_TARGET_TYPE.Structures:
                Remove(p_target as LocationStructure);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        Messenger.Broadcast(PlayerSignals.PLAYER_REMOVED_STORED_TARGET, p_target);
    }
    private void Remove(Summon p_monster) {
        storedMonsters.Remove(p_monster);
        p_monster.eventDispatcher.UnsubscribeToCharacterDied(this);
    }
    private void Remove(Character p_character) {
        storedVillagers.Remove(p_character);
        p_character.eventDispatcher.UnsubscribeToCharacterDied(this);
    }
    private void Remove(TileObject p_tileObject) {
        storedTileObjects.Remove(p_tileObject);
        p_tileObject.eventDispatcher.UnsubscribeToTileObjectDestroyed(this);
    }
    private void Remove(LocationStructure p_structure) {
        storedStructures.Remove(p_structure);
        p_structure.eventDispatcher.UnsubscribeToStructureDestroyed(this);
    }
    public bool IsAlreadyStored(IStoredTarget p_target) {
        switch (p_target.storedTargetType) {
            case STORED_TARGET_TYPE.Monster:
                return IsAlreadyStored(p_target as Summon);
            case STORED_TARGET_TYPE.Character:
                return IsAlreadyStored(p_target as Character);
            case STORED_TARGET_TYPE.Tile_Objects:
                return IsAlreadyStored(p_target as TileObject);
            case STORED_TARGET_TYPE.Structures:
                return IsAlreadyStored(p_target as LocationStructure);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    private bool IsAlreadyStored(Character p_character) {
        return storedVillagers.Contains(p_character);
    }
    private bool IsAlreadyStored(Summon p_monster) {
        return storedMonsters.Contains(p_monster);
    }
    private bool IsAlreadyStored(TileObject p_tileObject) {
        return storedTileObjects.Contains(p_tileObject);
    }
    private bool IsAlreadyStored(LocationStructure p_structure) {
        return storedStructures.Contains(p_structure);
    }

    #region Loading
    public void LoadReferences(SaveDataStoredTargetsComponent data) {
        for (int i = 0; i < data.allStoredTargets.Count; i++) {
            string persistentID = data.allStoredTargets[i];
            STORED_TARGET_TYPE type = data.allStoredTargetTypes[i];
            IStoredTarget storedTarget;
            switch (type) {
                case STORED_TARGET_TYPE.Character:
                case STORED_TARGET_TYPE.Monster:
                    storedTarget = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(persistentID);
                    break;
                case STORED_TARGET_TYPE.Tile_Objects:
                    storedTarget = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(persistentID);
                    break;
                case STORED_TARGET_TYPE.Structures:
                    storedTarget = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(persistentID);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Store(storedTarget);
        }
    }
    #endregion

    public void OnCharacterSubscribedToDied(Character p_character) {
        Remove(p_character as IStoredTarget);
    }
    public void OnTileObjectDestroyed(TileObject p_tileObject) {
        Remove(p_tileObject as IStoredTarget);
    }
    public void OnStructureDestroyed(LocationStructure p_structure) {
        Remove(p_structure as IStoredTarget);
    }
}

public interface IStoredTarget {
    string persistentID { get; }
    STORED_TARGET_TYPE storedTargetType { get; }
    string name { get; }
    string iconRichText { get; }

    bool isTargetted { get; set; }

    bool CanBeStoredAsTarget();
}

[Serializable]
public class SaveDataStoredTargetsComponent : SaveData<StoredTargetsComponent> {
    public List<string> allStoredTargets;
    public List<STORED_TARGET_TYPE> allStoredTargetTypes; //NOTE: Order of this and allStoredTargets should match!
    
    public override void Save(StoredTargetsComponent component) {
        allStoredTargets = new List<string>();
        allStoredTargetTypes = new List<STORED_TARGET_TYPE>();
        for (int i = 0; i < component.allStoredTargets.Count; i++) {
            IStoredTarget storedTarget = component.allStoredTargets[i];
            allStoredTargets.Add(storedTarget.persistentID);
            allStoredTargetTypes.Add(storedTarget.storedTargetType);
        }
    }
    public override StoredTargetsComponent Load() {
        StoredTargetsComponent component = new StoredTargetsComponent();
        return component;
    }
}