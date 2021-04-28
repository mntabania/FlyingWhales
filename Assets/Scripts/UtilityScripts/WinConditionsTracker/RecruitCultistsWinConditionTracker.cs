using System;
using System.Collections.Generic;
using Traits;

public class RecruitCultistsWinConditionTracker : WinConditionTracker {

    private const int NeededCultists = 12;
    
    public List<Character> cultists = new List<Character>();
    public Faction createdFaction;

    #region getters
    public override Type serializedData => typeof(SaveDataRecruitCultistsWinConditionTracker);
    #endregion

    public override void Initialize(List<Character> p_allCharacters) {
        base.Initialize(p_allCharacters);
                
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(FactionSignals.FACTION_SET, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, CharacterJoinedFaction);
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, CharacterRemovedFromFaction);
        Messenger.AddListener<Faction>(FactionSignals.FACTION_CREATED, OnFactionCreated);
        Messenger.AddListener<Faction>(FactionSignals.FACTION_DISBANDED, OnFactionDisbanded);
    }
    
    #region Loading
    public override void LoadReferences(SaveDataWinConditionTracker data) {
        base.LoadReferences(data);
        SaveDataRecruitCultistsWinConditionTracker tracker = data as SaveDataRecruitCultistsWinConditionTracker;
        cultists = SaveUtilities.ConvertIDListToCharacters(tracker.cultists);
        createdFaction = !string.IsNullOrEmpty(tracker.createdFaction) ? DatabaseManager.Instance.factionDatabase.GetFactionBasedOnPersistentID(tracker.createdFaction) : null;
    }
    #endregion

    #region Listeners
    private void OnFactionCreated(Faction p_createdFaction) {
        if (p_createdFaction.factionType.type == FACTION_TYPE.Demon_Cult && createdFaction == null) {
            createdFaction = p_createdFaction;
        }
    }
    private void OnFactionDisbanded(Faction p_disbandedFaction) {
        if (createdFaction != null && createdFaction == p_disbandedFaction) {
            if (PlayerManager.Instance.player.hasAlreadyWon) {
                return;
            }
            PlayerUI.Instance.LoseGameOver("Your demon cult has been wiped out. Mission Failed");
        }
    }
    private void CharacterRemovedFromFaction(Character p_newMember, Faction p_faction) {
        if (createdFaction != null) {
            if (createdFaction == p_faction && !cultists.Contains(p_newMember)) {
                cultists.Remove(p_newMember);
                UpdateStepsChangedNameEvent();
            }
        }
    }

    private void CharacterJoinedFaction(Character p_newMember, Faction p_faction) {
        if (createdFaction != null) {
            if (createdFaction == p_faction && !cultists.Contains(p_newMember)) {
                cultists.Add(p_newMember);
                OnCharacterAddedToMainCultFaction(p_newMember);
            }
        }
    }
    #endregion

    #region List Maintenance
    private void EliminateVillager(Character p_character) {
        if (cultists.Contains(p_character)) {
            cultists.Remove(p_character);
            UpdateStepsChangedNameEvent();
        }
    }
    #endregion

    private int GetFactionCount() {
        int count = 0;
        FactionManager.Instance.allFactions.ForEach((eachFaction) => {
            if (eachFaction.isMajorNonPlayer && eachFaction.characters.Count > 0) {
                count++;
            }
        });
        return count;
    }
    private void CheckIfCharacterIsEliminated(Character p_character) {
        if (ShouldConsiderCharacterAsEliminated(p_character)) {
            EliminateVillager(p_character);
        }
        if (GetFactionCount() <= 0) {
            if (PlayerManager.Instance.player.hasAlreadyWon) {
                return;
            }
            PlayerUI.Instance.LoseGameOver("You fail to recruit 15 cultists. Mission Failed");
        }
    }
    private void OnCharacterAddedToMainCultFaction(Character p_character) {
        UpdateStepsChangedNameEvent();
        if (cultists.Count >= NeededCultists){
            Messenger.Broadcast(PlayerSignals.WIN_GAME, "Your Cultists performed the dark ritual, tainting the divine energy for your own consumption!");
        }
    }

    #region Win Conditions Steps
    protected override IBookmarkable[] CreateWinConditionSteps() {
        GenericTextBookmarkable startDemonCult = new GenericTextBookmarkable(GetStartDemonCultText, () => BOOKMARK_TYPE.Text, OnSelectCreateCultFaction, null, OnHoverOverStartDemonCult, UIManager.Instance.HideSmallInfo);
        IBookmarkable[] bookmarkables = new[] {
            startDemonCult
        };
        return bookmarkables;
    }
    private string GetStartDemonCultText() {
        return $"Start a Demon Cult. Members: {cultists.Count.ToString()}/{NeededCultists.ToString()}";
    }
    #endregion

    #region Tooltips
    private void OnHoverOverStartDemonCult(UIHoverPosition position) {
        UIManager.Instance.ShowSmallInfo(
            "HINT: After recruiting enough cultists, one may eventually become a Cult Leader. You can directly order a Cult Leader to start its own Demon Cult faction. Transform villagers into cultists by brainwashing them in your Prison.", 
            pos: position
        );
    }
    private void OnSelectCreateCultFaction() {
        if (createdFaction != null) {
            UIManager.Instance.ShowFactionInfo(createdFaction);    
        }
    }
    #endregion
}

public class SaveDataRecruitCultistsWinConditionTracker : SaveDataWinConditionTracker {
    public List<string> cultists;
    public string createdFaction;
    public override void Save(WinConditionTracker data) {
        base.Save(data);
        RecruitCultistsWinConditionTracker tracker = data as RecruitCultistsWinConditionTracker;
        cultists = SaveUtilities.ConvertSavableListToIDs(tracker.cultists);
        if (tracker.createdFaction != null) {
            createdFaction = tracker.createdFaction.persistentID;
        } else {
            createdFaction = String.Empty;
        }
        
    }
}