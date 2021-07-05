using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Ruinarch;
using UtilityScripts;

public class PlayerRetaliationComponent {
    private const int MAX_RETALIATION_COUNTER = 5;
    public int retaliationCounter { get; private set; }
    public int angelCount { get; private set; }
    public int destroyedStructuresByAngelCounter { get; private set; }
    public List<Character> spawnedAngels { get; private set; }
    public bool isRetaliating { get; private set; }
    public RuinarchBasicProgress retaliationProgress { get; private set; }

    public PlayerRetaliationComponent() {
        spawnedAngels = new List<Character>();
        SetAngelCount(2);
        retaliationProgress = new RuinarchBasicProgress(GetRetaliationBookmarkText(), BOOKMARK_TYPE.Progress_Bar);
        retaliationProgress.SetOnHoverOverAction(OnHoverOverRetaliationBookmark);
        retaliationProgress.SetOnHoverOutAction(OnHoverOutRetaliationBookmark);
        retaliationProgress.SetOnSelectAction(OnClickRetaliationBookmark);
        retaliationProgress.Initialize(0, MAX_RETALIATION_COUNTER);
    }
    public PlayerRetaliationComponent(SaveDataPlayerRetaliationComponent data) {
        retaliationCounter = data.retaliationCounter;
        angelCount = data.angelCount;
        destroyedStructuresByAngelCounter = data.destroyedStructuresByAngelCounter;
        isRetaliating = data.isRetaliating;
        retaliationProgress = data.retaliationProgress;
        retaliationProgress.SetOnHoverOverAction(OnHoverOverRetaliationBookmark);
        retaliationProgress.SetOnHoverOutAction(OnHoverOutRetaliationBookmark);
        retaliationProgress.SetOnSelectAction(OnClickRetaliationBookmark);
    }

    #region Listeners
    public void OnCharacterDeath(Character p_character) {
        if (p_character.race == RACE.ANGEL) {
            if (spawnedAngels.Remove(p_character)) {
                CheckAngels();
            }
        }
    }
    #endregion

    #region Utilities
    public void OnCharacterRestrained(Character p_character) {
        if (p_character.race == RACE.ANGEL) {
            CheckAngels();
        }
    }
    #endregion

    #region Retaliation Counter
    public bool AddRetaliationCounter() {
        if (!WorldSettings.Instance.worldSettingsData.IsRetaliationAllowed()) {
            return false;
        }
        if (isRetaliating) {
            return false;
        }
        retaliationCounter++;
        retaliationProgress.SetName(GetRetaliationBookmarkText());
        retaliationProgress.SetProgress(retaliationCounter);
        if (retaliationCounter >= MAX_RETALIATION_COUNTER) {
            MaxRetaliationCounterReached();
        }
        return true;
    }
    private void StopRetaliation() {
        if (isRetaliating) {
            isRetaliating = false;
            retaliationCounter = 0;
            retaliationProgress.SetName(GetRetaliationBookmarkText());
            retaliationProgress.SetProgress(retaliationCounter);
            DespawnAllAngels();
            ResetAngelDestroyedStructuresCounter();
            Messenger.Broadcast(PlayerSignals.STOP_THREAT_EFFECT);

            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "General", "Player", "stop_retaliation", null, LOG_TAG.Player, LOG_TAG.Major);
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
        }
    }
    private void MaxRetaliationCounterReached() {
        ResetAngelDestroyedStructuresCounter();
        DivineIntervention();
        SetAngelCount(angelCount + 1);
        PlayerManager.Instance.player.ClearCharactersThatHaveReported();
    }
    private void DivineIntervention() {
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
        //CharacterManager.Instance.SetCurrentDemonicStructureTargetOfAngels(targetDemonicStructure as DemonicStructure);
        Region region = targetDemonicStructure.region;
        Area spawnArea = region.GetRandomAreaThatIsNotMountainWaterAndNoCorruption();
        //List<Character> characters = new List<Character>();
        spawnedAngels.Clear();
        for (int i = 0; i < angelCount; i++) {
            SUMMON_TYPE angelType = SUMMON_TYPE.Warrior_Angel;
            if (UnityEngine.Random.Range(0, 2) == 0) { angelType = SUMMON_TYPE.Magical_Angel; }
            LocationGridTile spawnTile = spawnArea.gridTileComponent.GetRandomTile();
            Summon angel = CharacterManager.Instance.CreateNewSummon(angelType, FactionManager.Instance.vagrantFaction, homeRegion: region);
            CharacterManager.Instance.PlaceSummonInitially(angel, spawnTile);
            angel.behaviourComponent.SetIsAttackingDemonicStructure(true, targetDemonicStructure as DemonicStructure);
            angel.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
            angel.SetDestroyMarkerOnDeath(true);
            spawnedAngels.Add(angel);
            //characters.Add(angel);
        }
        //Messenger.Broadcast(PlayerQuestSignals.ANGELS_ATTACKING_DEMONIC_STRUCTURE, characters);
        isRetaliating = true;
        Messenger.Broadcast(PlayerSignals.START_THREAT_EFFECT);

        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "General", "Player", "retaliation", null, LOG_TAG.Player, LOG_TAG.Major);
        log.AddLogToDatabase();
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
    }
    private string GetRetaliationBookmarkText() {
        return "Retaliation: " + retaliationCounter + "/" + MAX_RETALIATION_COUNTER;
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
        if (!HasActiveAngel()) {
            StopRetaliation();
        }
    }
    public bool HasActiveAngel() {
        for (int i = 0; i < spawnedAngels.Count; i++) {
            Character angel = spawnedAngels[i];
            if (!angel.traitContainer.HasTrait("Restrained") && !angel.isDead) {
                return true;
            }
        }
        return false;
    }
#endregion

#region Triggers
    public void CharacterDeathRetaliation(Character p_character) {
        string debugLog = string.Empty;
#if DEBUG_LOG
        debugLog = "ADD RETALIATION COUNTER!";
        debugLog += "\nDeath of " + p_character.name;
#endif
        if (ChanceData.RollChance(CHANCE_TYPE.Retaliation_Character_Death, ref debugLog)) {
            if (!p_character.traitContainer.HasTrait("Cultist") && p_character.isNormalCharacter) {
                if (AddRetaliationCounter()) {
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "General", "Player", "retaliation_character_death", null, LOG_TAG.Player, LOG_TAG.Major);
                    log.AddToFillers(p_character, p_character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddLogToDatabase();
                    PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
                }
            }
        }
#if DEBUG_LOG
        Debug.Log(debugLog);
#endif
    }
    public void StructureDestroyedRetaliation(LocationStructure p_structure) {
        if (p_structure.settlementLocation != null && p_structure.settlementLocation.locationType == LOCATION_TYPE.VILLAGE && p_structure.settlementLocation.owner != null && p_structure.settlementLocation.owner.isMajorNonPlayer
            && !p_structure.settlementLocation.owner.IsFriendlyWith(PlayerManager.Instance.player.playerFaction)) {
            string debugLog = string.Empty;
#if DEBUG_LOG
            debugLog = "ADD RETALIATION COUNTER!";
            debugLog += "\nDestruction of " + p_structure.name;
#endif
            if (ChanceData.RollChance(CHANCE_TYPE.Retaliation_Structure_Destroy, ref debugLog)) {
                if (AddRetaliationCounter()) {
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "General", "Player", "retaliation_structure_destroyed", null, LOG_TAG.Player, LOG_TAG.Major);
                    log.AddToFillers(p_structure, p_structure.name, LOG_IDENTIFIER.LANDMARK_1);
                    log.AddLogToDatabase();
                    PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
                }
            }
#if DEBUG_LOG
            Debug.Log(debugLog);
#endif
        }
    }
    public void ResourcePileRetaliation(TileObject p_pile, LocationGridTile removedFrom) {
        if (removedFrom != null && removedFrom.structure.structureType == STRUCTURE_TYPE.CITY_CENTER && removedFrom.structure.settlementLocation != null && 
            removedFrom.structure.settlementLocation.locationType == LOCATION_TYPE.VILLAGE && removedFrom.structure.settlementLocation.owner != null && 
            !removedFrom.structure.settlementLocation.owner.IsFriendlyWith(PlayerManager.Instance.player.playerFaction)) {
            string debugLog = string.Empty;
#if DEBUG_LOG
            debugLog = "ADD RETALIATION COUNTER!";
            debugLog += "\nDestruction/Loss of " + p_pile.name;
#endif
            if (ChanceData.RollChance(CHANCE_TYPE.Retaliation_Resource_Pile, ref debugLog)) {
                if (AddRetaliationCounter()) {
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "General", "Player", "retaliation_pile_loss", null, LOG_TAG.Player, LOG_TAG.Major);
                    log.AddToFillers(p_pile, p_pile.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddLogToDatabase();
                    PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
                }
            }
#if DEBUG_LOG
            Debug.Log(debugLog);
#endif
        }
    }
    public void ReportDemonicStructureRetaliation(Character p_character) {
        if (AddRetaliationCounter()) {
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "General", "Player", "retaliation_report_structure", null, LOG_TAG.Player, LOG_TAG.Major);
            log.AddToFillers(p_character, p_character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
        }
    }
#endregion

#region Loading
    public void LoadReferences(SaveDataPlayerRetaliationComponent data) {
        spawnedAngels = SaveUtilities.ConvertIDListToCharacters(data.spawnedAngels);
    }
#endregion

#region UI
    private void OnHoverOverRetaliationBookmark(UIHoverPosition position) {
        UIManager.Instance.ShowSmallInfo("Angels will spawn and head towards your Portal once the Retaliation Counter reaches Max Count. " +
                                         "The Retaliation Counter may rise whenever you kill Villagers, ruin their structures or destroy their resources.", 
            pos: position);
    }
    private void OnHoverOutRetaliationBookmark() {
        UIManager.Instance.HideSmallInfo();
    }
    private void OnClickRetaliationBookmark() {
        if (spawnedAngels.Count > 0) {
            CharacterCenterCycle(spawnedAngels);
            // Character angel = CollectionUtilities.GetRandomElement(spawnedAngels); //spawnedAngels[GameUtilities.RandomBetweenTwoNumbers(0, spawnedAngels.Count)];
            // UIManager.Instance.ShowCharacterInfo(angel, true);
        }
    }
    private void CharacterCenterCycle(List<Character> characters) {
        if (characters != null && characters.Count > 0) {
            //normal objects to center
            ISelectable objToSelect = GetNextCharacterToCenter(characters);
            if (objToSelect != null) {
                InputManager.Instance.Select(objToSelect);
            }
        }
    }
    private Character GetNextCharacterToCenter(List<Character> selectables) {
        Character objToSelect = null;
        for (int i = 0; i < selectables.Count; i++) {
            Character currentSelectable = selectables[i];
            if (currentSelectable.IsCurrentlySelected()) {
                //set next selectable in list to be selected.
                objToSelect = CollectionUtilities.GetNextElementCyclic(selectables, i);
                break;
            }
        }
        if (objToSelect == null) {
            objToSelect = selectables[0];
        }
        return objToSelect;
    }
#endregion
}

public class SaveDataPlayerRetaliationComponent : SaveData<PlayerRetaliationComponent> {
    public int retaliationCounter;
    public int angelCount;
    public int destroyedStructuresByAngelCounter;
    public List<string> spawnedAngels;
    public bool isRetaliating;
    public RuinarchBasicProgress retaliationProgress;

    public override void Save(PlayerRetaliationComponent data) {
        base.Save(data);
        retaliationCounter = data.retaliationCounter;
        angelCount = data.angelCount;
        destroyedStructuresByAngelCounter = data.destroyedStructuresByAngelCounter;
        spawnedAngels = SaveUtilities.ConvertSavableListToIDs(data.spawnedAngels);
        isRetaliating = data.isRetaliating;
        retaliationProgress = data.retaliationProgress;
    }
    public override PlayerRetaliationComponent Load() {
        PlayerRetaliationComponent component = new PlayerRetaliationComponent(this);
        return component;
    }
}