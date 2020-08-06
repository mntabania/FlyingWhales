using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Quests;
using UnityEngine.Assertions;
using UtilityScripts;

public class ThreatComponent {
    public Player player { get; private set; }

    public const int MAX_THREAT = 100;
    public int threat { get; private set; }
    // public int threatPerHour { get; private set; }

    private bool isDecreasingThreatPerHour;
    
    public ThreatComponent(Player player) {
        this.player = player;
        isDecreasingThreatPerHour = false;
        // Messenger.AddListener(Signals.HOUR_STARTED, PerHour);
        // Messenger.AddListener(Signals.START_THREAT_EFFECT, OnStartThreatEffect);
        Messenger.AddListener<Quest>(Signals.QUEST_DEACTIVATED, OnQuestDeactivated);
    }
    // private void PerHour() {
    //     AdjustThreat(threatPerHour);
    // }

    public void AdjustThreat(int amount) {
        if(!Tutorial.TutorialManager.Instance.hasCompletedImportantTutorials && 
           WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
            //Threat does not increase until Tutorial is over, and since the last tutorial is Invade a village, it should be the checker
            //https://trello.com/c/WOZJmvzQ/1238-threat-does-not-increase-until-tutorial-is-over
            return;
        }
        if (WorldSettings.Instance.worldSettingsData.noThreatMode) {
            return;
        }

        int previousValue = threat;
        int supposedThreat = threat + amount;
        threat = supposedThreat;
        threat = Mathf.Clamp(threat, 0, 100);

        //check if the player just now, reached max threat
        bool justReachedMax = threat >= MAX_THREAT && previousValue < MAX_THREAT;
        
        if (amount > 0) {
            OnThreatIncreased();
            Messenger.Broadcast(Signals.THREAT_INCREASED, amount);
        } else if (amount < 0) {
            OnThreatDecreased();
        }

        if (justReachedMax) {
            OnMaxThreat();
            Messenger.Broadcast(Signals.THREAT_MAXED_OUT);
        }
        Messenger.Broadcast(Signals.THREAT_UPDATED);
    }
    // public void AdjustThreatPerHour(int amount) {
    //     threatPerHour += amount;
    // }
    // public void SetThreatPerHour(int amount) {
    //     threatPerHour = amount;
    // }
    private void OnMaxThreat() {
        //Counterattack();
        DivineIntervention();
    }
    private void OnThreatIncreased() {
        if (!isDecreasingThreatPerHour) {
            isDecreasingThreatPerHour = true;
            //start decreasing threat per hour
            Messenger.AddListener(Signals.HOUR_STARTED, DecreaseThreatPerHour);
        }
    }
    private void DecreaseThreatPerHour() {
        if (QuestManager.Instance.IsQuestActive<DivineIntervention>()) {
            return; //do not decrease threat per hour if divine intervention quest is active
        }
        AdjustThreat(-1);
    }
    private void OnQuestDeactivated(Quest quest) {
        if (quest is DivineIntervention) {
            if (QuestManager.Instance.IsQuestActive<DivineIntervention>()) {
                return;
            }    
            //if divine intervention quest has finished and there is no other active divine intervention
            //then reset threat to 50.
            threat = 50;
            Messenger.Broadcast(Signals.THREAT_UPDATED);
            Messenger.Broadcast(Signals.STOP_THREAT_EFFECT);
        }
    }
    private void OnThreatDecreased() {
        if (threat <= 0) {
            isDecreasingThreatPerHour = false;
            Messenger.Broadcast(Signals.THREAT_UPDATED);
            // Messenger.Broadcast(Signals.THREAT_RESET);
            Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseThreatPerHour);
        }
        Messenger.Broadcast(Signals.STOP_THREAT_EFFECT);
    }
    // private void ResetThreatAfterHours(int hours) {
    //     GameDate dueDate = GameManager.Instance.Today();
    //     dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(hours));
    //     SchedulingManager.Instance.AddEntry(dueDate, ResetThreat, player);
    // }
    // public void ResetThreat() {
    //     threat = 0;
    //     // SetThreatPerHour(0);
    //     Messenger.Broadcast(Signals.THREAT_UPDATED);
    //     Messenger.Broadcast(Signals.THREAT_RESET);
    //     Messenger.Broadcast(Signals.STOP_THREAT_EFFECT);
    // }
    // private void OnStartThreatEffect() {
    //     ResetThreatAfterHours(2);
    // }

    public void DivineIntervention() {
        string debugLog = GameManager.Instance.TodayLogString() + "Divine Intervention";
        LocationStructure targetDemonicStructure = null;
        if (InnerMapManager.Instance.HasExistingWorldKnownDemonicStructure()) {
            targetDemonicStructure = InnerMapManager.Instance.worldKnownDemonicStructures[UnityEngine.Random.Range(0, InnerMapManager.Instance.worldKnownDemonicStructures.Count)];
        } else {
            targetDemonicStructure = PlayerManager.Instance.player.playerSettlement.GetRandomStructure();
        }
        if (targetDemonicStructure == null) {
            //it is assumed that this only happens if the player casts a spell that is seen by another character,
            //but results in the destruction of the portal
            //attackingCharacters = null;
            return;
        }
        debugLog += "\n-TARGET: " + targetDemonicStructure.name;
        CharacterManager.Instance.SetCurrentDemonicStructureTargetOfAngels(targetDemonicStructure as DemonicStructure);
        Region region = targetDemonicStructure.location;
        HexTile spawnHex = targetDemonicStructure.location.GetRandomUncorruptedPlainHex();
        List<Character> characters = new List<Character>();
        int angelCount = UnityEngine.Random.Range(3, 6);
        for (int i = 0; i < angelCount; i++) {
            SUMMON_TYPE angelType = SUMMON_TYPE.Warrior_Angel;
            if (UnityEngine.Random.Range(0, 2) == 0) { angelType = SUMMON_TYPE.Magical_Angel; }
            LocationGridTile spawnTile = spawnHex.GetRandomTile();
            Summon angel = CharacterManager.Instance.CreateNewSummon(angelType, FactionManager.Instance.vagrantFaction, homeRegion: region);
            CharacterManager.Instance.PlaceSummon(angel, spawnTile);
            angel.behaviourComponent.SetIsAttackingDemonicStructure(true, CharacterManager.Instance.currentDemonicStructureTargetOfAngels);
            characters.Add(angel);
        }
        Messenger.Broadcast(Signals.ANGELS_ATTACKING_DEMONIC_STRUCTURE, characters);
        Messenger.Broadcast(Signals.START_THREAT_EFFECT);
    }
}
