using System;
using System.Collections.Generic;
using Traits;

public class IcalawaWinConditionTracker : WinConditionTracker {

    public const int TotalCharactersToKill = 5;
    
    private System.Action<Character, int> _characterEliminatedAction;
    private System.Action<Character> _characterAddedAsTargetAction;
    private System.Action<Character> _characterSuccessChangeTraitToPsychopath;

    #region IListener
    public interface IListenerKillingEvents {
        void OnCharacterEliminated(Character p_character, int p_villagerCount);
        void OnCharacterAddedAsTarget(Character p_character);
    }

    public interface IListenerChangeTraits {
        void OnCharacterGainedPsychopathTrait(Character p_character);
    }
    #endregion

    public Character psychoPath { set; get; }
    public List<Character> villagersToEliminate { get; private set; }
    public int totalCharactersToEliminate { get; private set; }
    public override Type serializedData => typeof(SaveDataIcalawaWinConditionTracker);
    
    public override void Initialize(List<Character> p_allCharacters) {
        base.Initialize(p_allCharacters);

        villagersToEliminate = new List<Character>();
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character>(FactionSignals.FACTION_SET, CheckIfCharacterIsEliminated);
        Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharactertraitGain);
        Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerArrived);

        List<Character> charactersToTrack = GetQuestCharacters(p_allCharacters);
        villagersToEliminate.Clear();
        for (int i = 0; i < charactersToTrack.Count; i++) {
            AddVillagerToEliminate(charactersToTrack[i]);
        }
        totalCharactersToEliminate = 5;
    }
    protected override IBookmarkable[] CreateWinConditionSteps() {
        return null;
    }

    #region Loading
    public override void LoadReferences(SaveDataWinConditionTracker data) {
        base.LoadReferences(data);
        SaveDataIcalawaWinConditionTracker tracker = data as SaveDataIcalawaWinConditionTracker;
        if (!string.IsNullOrEmpty(tracker.psychopath)) {
            psychoPath = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(tracker.psychopath);
        }
        villagersToEliminate = SaveUtilities.ConvertIDListToCharacters(tracker.villagersToEliminate);
        totalCharactersToEliminate = tracker.totalCharactersToEliminate;
    }
    #endregion

    #region List Maintenance
    private void EliminateVillager(Character p_character) {
        if (p_character != psychoPath) {
            if (villagersToEliminate.Remove(p_character)) {
                if (p_character.causeOfDeath == INTERACTION_TYPE.RITUAL_KILLING) {
                    totalCharactersToEliminate--;
                }
            }
        } else if (p_character == psychoPath) {
            PlayerUI.Instance.LoseGameOver("Psychopath Died, Mission Failed");
        }
        _characterEliminatedAction?.Invoke(p_character, totalCharactersToEliminate);
    }
    private void AddVillagerToEliminate(Character p_character) {
        if (!villagersToEliminate.Contains(p_character)) {
            villagersToEliminate.Add(p_character);
            _characterAddedAsTargetAction?.Invoke(p_character);
        }
    }
    #endregion

    public List<Character> GetQuestCharacters(List<Character> p_allCharacters) {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < p_allCharacters.Count; i++) {
            Character character = p_allCharacters[i];
            if (character.isNormalCharacter && character.race.IsSapient() && character.traitContainer.HasTrait("Blessed")) {
                characters.Add(character);
            } else if(character.isNormalCharacter && character.race.IsSapient()) {
                psychoPath = character;
            }
        }
        return characters;
    }

    private void OnCharactertraitGain(Character p_character, Trait p_trait) {
        if (p_character == psychoPath) {
            if (psychoPath.traitContainer.HasTrait("Blessed")) {
                PlayerUI.Instance.LoseGameOver("Psychopath Became blessed, Mission Failed");
            } else if (psychoPath.traitContainer.HasTrait("Psychopath")) {
                _characterSuccessChangeTraitToPsychopath?.Invoke(p_character);
            }
        }
    }

    private void CheckIfCharacterIsEliminated(Character p_character) {
        if (ShouldConsiderCharacterAsEliminated(p_character)) {
            EliminateVillager(p_character);
        }
    }
    private void OnNewVillagerArrived(Character newVillager) {
        AddVillagerToEliminate(newVillager);
    }

    public void SubscribeToKillingEvents(IcalawaWinConditionTracker.IListenerKillingEvents p_listener) {
        _characterEliminatedAction += p_listener.OnCharacterEliminated;
        _characterAddedAsTargetAction += p_listener.OnCharacterAddedAsTarget;
    }
    public void UnsubscribeToKillingEvents(IcalawaWinConditionTracker.IListenerKillingEvents p_listener) {
        _characterEliminatedAction -= p_listener.OnCharacterEliminated;
        _characterAddedAsTargetAction -= p_listener.OnCharacterAddedAsTarget;
    }

    public void SubscribeToChangeTraitEvents(IcalawaWinConditionTracker.IListenerChangeTraits p_listener) {
        _characterSuccessChangeTraitToPsychopath += p_listener.OnCharacterGainedPsychopathTrait;
    }
    public void UnsubscribeToChangeTraitEvents(IcalawaWinConditionTracker.IListenerChangeTraits p_listener) {
        _characterSuccessChangeTraitToPsychopath -= p_listener.OnCharacterGainedPsychopathTrait;
    }
}

public class SaveDataIcalawaWinConditionTracker : SaveDataWinConditionTracker {
    public string psychopath;
    public List<string> villagersToEliminate;
    public int totalCharactersToEliminate;
    public override void Save(WinConditionTracker data) {
        base.Save(data);
        IcalawaWinConditionTracker tracker = data as IcalawaWinConditionTracker;
        if (tracker.psychoPath != null) {
            psychopath = tracker.psychoPath.persistentID;    
        }
        villagersToEliminate = SaveUtilities.ConvertSavableListToIDs(tracker.villagersToEliminate);
        totalCharactersToEliminate = tracker.totalCharactersToEliminate;
    }
}