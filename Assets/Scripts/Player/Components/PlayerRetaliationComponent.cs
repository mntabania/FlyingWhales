using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class PlayerRetaliationComponent {
    private const int MAX_RETALIATION_COUNTER = 5;
    public int retaliationCounter { get; private set; }
    public int angelCount { get; private set; }
    public int destroyedStructuresByAngelCounter { get; private set; }
    public List<Character> spawnedAngels { get; private set; }
    public bool isRetaliating { get; private set; }

    public PlayerRetaliationComponent() {
        spawnedAngels = new List<Character>();
        SetAngelCount(2);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDeath);
    }
    public PlayerRetaliationComponent(SaveDataPlayerRetaliationComponent data) {
        spawnedAngels = new List<Character>();
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDeath);
    }

    #region Listeners
    private void OnCharacterDeath(Character p_character) {
        if (p_character.race == RACE.ANGEL) {
            if (spawnedAngels.Remove(p_character)) {
                CheckAngels();
            }
        }
    }
    #endregion

    #region Utilities
    public bool AddRetaliationCounter() {
        if (isRetaliating) {
            return false;
        }
        retaliationCounter++;
        if (retaliationCounter >= MAX_RETALIATION_COUNTER) {
            MaxRetaliationCounterReached();
        }
        return true;
    }
    private void StopRetaliation() {
        if (isRetaliating) {
            isRetaliating = false;
            retaliationCounter = 0;
            DespawnAllAngels();
            ResetAngelDestroyedStructuresCounter();
            Messenger.Broadcast(PlayerSignals.STOP_THREAT_EFFECT);
        }
    }
    private void MaxRetaliationCounterReached() {
        ResetAngelDestroyedStructuresCounter();
        DivineIntervention();
        SetAngelCount(angelCount + 1);
    }
    private void DivineIntervention() {
        string debugLog = GameManager.Instance.TodayLogString() + "Divine Intervention";
        LocationStructure targetDemonicStructure = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL);
        //if (InnerMapManager.Instance.HasExistingWorldKnownDemonicStructure()) {
        //    targetDemonicStructure = InnerMapManager.Instance.worldKnownDemonicStructures[UnityEngine.Random.Range(0, InnerMapManager.Instance.worldKnownDemonicStructures.Count)];
        //} else {
        //    targetDemonicStructure = PlayerManager.Instance.player.playerSettlement.GetRandomStructure();
        //}
        if (targetDemonicStructure == null) {
            //it is assumed that this only happens if the player casts a spell that is seen by another character,
            //but results in the destruction of the portal
            //attackingCharacters = null;
            return;
        }
        debugLog += "\n-TARGET: " + targetDemonicStructure.name;
        //CharacterManager.Instance.SetCurrentDemonicStructureTargetOfAngels(targetDemonicStructure as DemonicStructure);
        Region region = targetDemonicStructure.region;
        Area spawnArea = region.GetRandomHexThatMeetCriteria(a => a.elevationType != ELEVATION.WATER && a.elevationType != ELEVATION.MOUNTAIN && !a.gridTileComponent.HasCorruption());
        //List<Character> characters = new List<Character>();
        spawnedAngels.Clear();
        for (int i = 0; i < angelCount; i++) {
            SUMMON_TYPE angelType = SUMMON_TYPE.Warrior_Angel;
            if (UnityEngine.Random.Range(0, 2) == 0) { angelType = SUMMON_TYPE.Magical_Angel; }
            LocationGridTile spawnTile = spawnArea.gridTileComponent.GetRandomTile();
            Summon angel = CharacterManager.Instance.CreateNewSummon(angelType, FactionManager.Instance.vagrantFaction, homeRegion: region);
            CharacterManager.Instance.PlaceSummonInitially(angel, spawnTile);
            angel.behaviourComponent.SetIsAttackingDemonicStructure(true, targetDemonicStructure as DemonicStructure);
            angel.SetDestroyMarkerOnDeath(true);
            spawnedAngels.Add(angel);
            //characters.Add(angel);
        }
        //Messenger.Broadcast(PlayerQuestSignals.ANGELS_ATTACKING_DEMONIC_STRUCTURE, characters);
        isRetaliating = true;
        Messenger.Broadcast(PlayerSignals.START_THREAT_EFFECT);
    }
    #endregion

    #region Angels
    private void SetAngelCount(int amount) {
        angelCount = amount;
    }
    private void ResetAngelDestroyedStructuresCounter() {
        destroyedStructuresByAngelCounter = 0;
    }
    public void AddDestroyedStructureByAngels() {
        destroyedStructuresByAngelCounter++;
        //Always compare it with angelCount - 1 because it is the true current number of angels spawned
        //We subtract 1 because every time divine intervention is triggered the angelCount is incremented so the angel count is no longer the accurate representation of the current angels spawned
        if (destroyedStructuresByAngelCounter >= (angelCount - 1)) {
            StopRetaliation();
        }
    }
    private void DespawnAllAngels() {
        List<Character> angels = RuinarchListPool<Character>.Claim();
        angels.AddRange(spawnedAngels);
        for (int i = 0; i < angels.Count; i++) {
            Character angel = angels[i];
            if (!angel.isDead) {
                angel.behaviourComponent.SetIsAttackingDemonicStructure(false, null);
                angel.Death("disappear");
            }
        }
        RuinarchListPool<Character>.Release(angels);
        spawnedAngels.Clear();
    }
    private void CheckAngels() {
        if (!HasAngels()) {
            StopRetaliation();
        }
    }
    public bool HasAngels() {
        return spawnedAngels.Count > 0;
    }
    #endregion

    #region Triggers
    public void CharacterDeathRetaliation(Character p_character) {
        if (GameUtilities.RollChance(25)) {
            if (!p_character.traitContainer.HasTrait("Cultist") && p_character.isNormalCharacter) {
                if (AddRetaliationCounter()) {
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "General", "Player", "retaliation_character_death", null, LOG_TAG.Player, LOG_TAG.Major);
                    log.AddToFillers(p_character, p_character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    //log.AddLogToDatabase();
                    PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
                }
            }
        }
    }
    public void StructureDestroyedRetaliation(LocationStructure p_structure) {
        if (p_structure.settlementLocation != null && p_structure.settlementLocation.locationType == LOCATION_TYPE.VILLAGE && p_structure.settlementLocation.owner != null && p_structure.settlementLocation.owner.isMajorNonPlayer) {
            if (GameUtilities.RollChance(25)) {
                if (AddRetaliationCounter()) {
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "General", "Player", "retaliation_structure_destroyed", null, LOG_TAG.Player, LOG_TAG.Major);
                    log.AddToFillers(p_structure, p_structure.name, LOG_IDENTIFIER.LANDMARK_1);
                    //log.AddLogToDatabase();
                    PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
                }
            }
        }
    }
    public void ResourcePileRetaliation(TileObject p_pile, LocationGridTile removedFrom) {
        if (removedFrom != null && removedFrom.structure.structureType == STRUCTURE_TYPE.CITY_CENTER) {
            if (GameUtilities.RollChance(25)) {
                if (AddRetaliationCounter()) {
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "General", "Player", "retaliation_pile_loss", null, LOG_TAG.Player, LOG_TAG.Major);
                    log.AddToFillers(p_pile, p_pile.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    //log.AddLogToDatabase();
                    PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
                }
            }
        }
    }
    public void ReportDemonicStructureRetaliation(Character p_character) {
        if (AddRetaliationCounter()) {
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "General", "Player", "retaliation_report_structure", null, LOG_TAG.Player, LOG_TAG.Major);
            log.AddToFillers(p_character, p_character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            //log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
        }
    }
    #endregion
}

public class SaveDataPlayerRetaliationComponent : SaveData<PlayerRetaliationComponent> {
    public override PlayerRetaliationComponent Load() {
        PlayerRetaliationComponent component = new PlayerRetaliationComponent(this);
        return component;
    }
}