using System.Collections.Generic;
using System.Linq;
using Ruinarch;
using UnityEngine;
using UtilityScripts;

public class ZenkoWinConditionTracker : WinconditionTracker {

    private System.Action<int> _factionRelationshipChanged;

    public int RemainingWarDeclaration { private set; get; }
    public int RemainingFactions { private set; get; }

    private int PossibleWarDeclaration { set; get; }

    private Dictionary<string, List<string>> m_factionsWarDeclarationHistory = new Dictionary<string, List<string>>();

    public interface Listener {
        void OnFactionRelationshipChanged(int p_remainingWarDeclaration);
    }

    public override void Initialize(List<Character> p_allCharacters) {
        base.Initialize(p_allCharacters);
        RemainingWarDeclaration = 3;
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(FactionSignals.FACTION_SET, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_BECOME_CULTIST, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerArrived);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_NO_LONGER_CULTIST, OnCharacterNoLongerCultist);
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);
        Messenger.AddListener<Faction, Faction, FactionRelationship, FactionRelationship>(FactionSignals.CHANGE_FACTION_RELATIONSHIP, OnFactionRelationshipChanged);
    
    }

    private void OnFactionRelationshipChanged(Faction p_callerFaction, Faction p_subjectFaction, FactionRelationship p_newRelationship, FactionRelationship p_oldRelationship) {
        if (!m_factionsWarDeclarationHistory[p_callerFaction.name].Contains(p_subjectFaction.name)) {
            m_factionsWarDeclarationHistory[p_callerFaction.name].Add(p_subjectFaction.name);
            m_factionsWarDeclarationHistory[p_subjectFaction.name].Add(p_callerFaction.name);
            RemainingWarDeclaration--;
        }
        _factionRelationshipChanged?.Invoke(RemainingWarDeclaration);
    }

    private void UpdateFactionInfo() {
        PossibleWarDeclaration = 0;
        FactionManager.Instance.allFactions.ForEach((eachFaction) => {
            if (eachFaction.isMajorNonPlayer) {
                if (eachFaction.characters.Count > 0) {
                    RemainingFactions++;
                }
                FactionManager.Instance.allFactions.ForEach((compareFaction) => {
                    if (eachFaction != compareFaction) {
                        if (!m_factionsWarDeclarationHistory[compareFaction.name].Contains(compareFaction.name)) {
                            PossibleWarDeclaration++;
                        }
                    }
                });
            }
        });
    }

    private void CheckFailCondition() {
        if (RemainingFactions - 1 < RemainingWarDeclaration) {
            PlayerUI.Instance.LoseGameOver("Mission failed, war declaration requirement not met");
        }
        if (PossibleWarDeclaration < RemainingWarDeclaration) {
            PlayerUI.Instance.LoseGameOver("Mission failed, war declaration requirement not met");
        }
    }

    #region List Maintenance
    private void AddVillagerToEliminate(Character p_character) {
        AddCharacterToTrackList(p_character);
    }
    #endregion

    public void OnCharacterRemovedFromFaction(Character p_character, Faction p_faction) {
        UpdateFactionInfo();
        CheckFailCondition();
    }

    public void OnCharacterAddedToFaction(Character p_character, Faction p_faction) {
        UpdateFactionInfo();
    }

    private void CheckIfCharacterIsEliminated(Character p_character) {
        if (ShouldConsiderCharacterAsEliminated(p_character)) {
            RemoveCharacterFromTrackList(p_character);
        }
        UpdateFactionInfo();
        CheckFailCondition();

    }
    private void OnNewVillagerArrived(Character newVillager) {
        AddVillagerToEliminate(newVillager);
    }
    private void OnCharacterNoLongerCultist(Character p_character) {
        AddVillagerToEliminate(p_character);
    }

    public void Subscribe(Listener p_listener) {
        _factionRelationshipChanged += p_listener.OnFactionRelationshipChanged;
        
    }
    public void Unsubscribe(Listener p_listener) {
        _factionRelationshipChanged -= p_listener.OnFactionRelationshipChanged;
        
    }
}