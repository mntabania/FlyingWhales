using System;
using System.Collections.Generic;
using System.Linq;

public class ZenkoWinConditionTracker : WinConditionTracker {

    public const int ActiveWarRequirement = 3;

    private System.Action<int> _factionRelationshipChanged;
    private System.Action<Faction> _factionDisbanded;

    public List<Faction> remainingFactions { private set; get; }
    public int activeWars { private set; get; }

    public override Type serializedData => typeof(SaveDataZenkoWinConditionTracker);

    public interface IListener {
        void OnFactionRelationshipChanged(int p_activeWars);
        void OnFactionDisbanded(Faction p_faction);
    }

    #region Loading
    public override void LoadReferences(SaveDataWinConditionTracker data) {
        base.LoadReferences(data);
        SaveDataZenkoWinConditionTracker tracker = data as SaveDataZenkoWinConditionTracker;
        remainingFactions = SaveUtilities.ConvertIDListToFactions(tracker.remainingFactions);
        activeWars = tracker.activeWars;
    }
    #endregion

    public override void Initialize(List<Character> p_allCharacters) {
        base.Initialize(p_allCharacters);
        remainingFactions = FactionManager.Instance.allFactions.Where(f => f.isMajorNonPlayer).ToList();;
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<Character>(FactionSignals.FACTION_SET, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_BECOME_CULTIST, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerArrived);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_NO_LONGER_CULTIST, OnCharacterNoLongerCultist);
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
        Messenger.AddListener<Faction, Faction, FACTION_RELATIONSHIP_STATUS, FACTION_RELATIONSHIP_STATUS>(FactionSignals.CHANGE_FACTION_RELATIONSHIP, OnFactionRelationshipChanged);
    }
    protected override IBookmarkable[] CreateWinConditionSteps() {
        throw new NotImplementedException();
    }
    private void OnFactionRelationshipChanged(Faction p_callerFaction, Faction p_subjectFaction, FACTION_RELATIONSHIP_STATUS p_newRelationship, FACTION_RELATIONSHIP_STATUS p_oldRelationship) {
        if (GameManager.Instance.gameHasStarted && p_callerFaction.isMajorNonPlayer && p_subjectFaction.isMajorNonPlayer) {
            if (p_newRelationship == FACTION_RELATIONSHIP_STATUS.Hostile) {
                activeWars++;
            } else {
                activeWars--;
            }
            _factionRelationshipChanged?.Invoke(activeWars);
        }
    }
    

    private void CheckFailCondition() {
        if (remainingFactions.Count < 3) {
            if (PlayerManager.Instance.player.hasAlreadyWon) {
                return;
            }
            PlayerUI.Instance.LoseGameOver("Mission failed, war declaration requirement not met");
        }
    }

    #region List Maintenance
    private void AddVillagerToEliminate(Character p_character) {
        // AddCharacterToTrackList(p_character);
    }
    #endregion

    private void OnCharacterRemovedFromFaction(Character p_character, Faction p_faction) {
        CheckForFactionDisbanded(p_faction);
    }
    private void CheckForFactionDisbanded(Faction p_faction) {
        if (p_faction.isMajorNonPlayer && !p_faction.HasAliveMember() && remainingFactions.Remove(p_faction)) {
            foreach (var relationship in p_faction.relationships) {
                if (relationship.Key.isMajorNonPlayer && relationship.Key.HasAliveMember()) {
                    if (relationship.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile) {
                        activeWars--; //if inactive faction is at war with an active faction. Reduce active war count, since inactive factions war should not count anymore.  
                    }
                }
            }
            _factionDisbanded?.Invoke(p_faction);
            CheckFailCondition();
        }
    }
    private void OnCharacterDied(Character p_character) {
        if (p_character.isNormalCharacter && p_character.faction != null && p_character.faction.isMajorNonPlayer) {
            CheckForFactionDisbanded(p_character.faction);    
        }
        CheckIfCharacterIsEliminated(p_character);
    }
    private void CheckIfCharacterIsEliminated(Character p_character) {
        if (ShouldConsiderCharacterAsEliminated(p_character)) {
            // RemoveCharacterFromTrackList(p_character);
        }
        CheckFailCondition();

    }
    private void OnNewVillagerArrived(Character newVillager) {
        AddVillagerToEliminate(newVillager);
    }
    private void OnCharacterNoLongerCultist(Character p_character) {
        AddVillagerToEliminate(p_character);
    }

    public void Subscribe(IListener p_listener) {
        _factionRelationshipChanged += p_listener.OnFactionRelationshipChanged;
        _factionDisbanded += p_listener.OnFactionDisbanded;

    }
    public void Unsubscribe(IListener p_listener) {
        _factionRelationshipChanged -= p_listener.OnFactionRelationshipChanged;
        _factionDisbanded -= p_listener.OnFactionDisbanded;
    }
}

public class SaveDataZenkoWinConditionTracker : SaveDataWinConditionTracker {
    public List<string> remainingFactions;
    public int activeWars;
    public override void Save(WinConditionTracker data) {
        base.Save(data);
        ZenkoWinConditionTracker tracker = data as ZenkoWinConditionTracker;
        remainingFactions = SaveUtilities.ConvertSavableListToIDs(tracker.remainingFactions);
        activeWars = tracker.activeWars;
    }
}