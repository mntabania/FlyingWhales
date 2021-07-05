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
        Messenger.AddListener<Quest>(PlayerQuestSignals.QUEST_DEACTIVATED, OnQuestDeactivated);
    }
    public ThreatComponent() {
        isDecreasingThreatPerHour = false;
        Messenger.AddListener<Quest>(PlayerQuestSignals.QUEST_DEACTIVATED, OnQuestDeactivated);
    }
    public void SetPlayer(Player player) {
        this.player = player;
    }
    // private void PerHour() {
    //     AdjustThreat(threatPerHour);
    // }

    public void AdjustThreatAndApplyModification(int amount) {
        // amount = SpellUtilities.GetModifiedSpellCost(amount, WorldSettings.Instance.worldSettingsData.playerSkillSettings.GetThreatModification());
        AdjustThreat(amount);
    }
    public void AdjustThreat(int amount) {
        return; //disabled threat for now.
        if(!Tutorial.TutorialManager.Instance.hasCompletedImportantTutorials && 
           WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
            //Threat does not increase until Tutorial is over, and since the last tutorial is Invade a village, it should be the checker
            //https://trello.com/c/WOZJmvzQ/1238-threat-does-not-increase-until-tutorial-is-over
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
            Messenger.Broadcast(PlayerSignals.THREAT_INCREASED, amount);
        } else if (amount < 0) {
            OnThreatDecreased();
        }

        if (justReachedMax) {
            OnMaxThreat();
            Messenger.Broadcast(PlayerSignals.THREAT_MAXED_OUT);
        }
        Messenger.Broadcast(PlayerSignals.THREAT_UPDATED);
    }
    // public void AdjustThreatPerHour(int amount) {
    //     threatPerHour += amount;
    // }
    // public void SetThreatPerHour(int amount) {
    //     threatPerHour = amount;
    // }
    private void OnMaxThreat() {
        //Counterattack();
        //DivineIntervention();
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
        int hour = GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick);
        if (UtilityScripts.Utilities.IsEven(hour)) {
            //only decrease threat every other hour
            AdjustThreat(-1);    
        }
    }
    private void OnQuestDeactivated(Quest quest) {
        if (quest is DivineIntervention) {
            if (QuestManager.Instance.IsQuestActive<DivineIntervention>()) {
                return;
            }    
            //if divine intervention quest has finished and there is no other active divine intervention
            //then reset threat to 50.
            threat = 50;
            Messenger.Broadcast(PlayerSignals.THREAT_UPDATED);
            Messenger.Broadcast(PlayerSignals.STOP_THREAT_EFFECT);
        }
    }
    private void OnThreatDecreased() {
        if (threat <= 0) {
            isDecreasingThreatPerHour = false;
            Messenger.Broadcast(PlayerSignals.THREAT_UPDATED);
            // Messenger.Broadcast(Signals.THREAT_RESET);
            Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseThreatPerHour);
        }
        Messenger.Broadcast(PlayerSignals.STOP_THREAT_EFFECT);
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

    #region Save
    public void SetThreatFromSave(int amount) {
        threat = amount;
    }
    #endregion
}

[System.Serializable]
public class SaveDataThreatComponent : SaveData<ThreatComponent> {
    public int threat;

    public override void Save(ThreatComponent component) {
        threat = component.threat;
    }
    public override ThreatComponent Load() {
        ThreatComponent component = new ThreatComponent();
        component.SetThreatFromSave(threat);
        return component;
    }
}