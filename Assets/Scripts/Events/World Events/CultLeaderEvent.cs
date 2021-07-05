using System;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Events.World_Events {
    public class CultLeaderEvent : WorldEvent {

        private readonly List<Character> _activeCultists;
        private Character _currentCultLeader;
        private const int Minimum_Cultist_Count = 3;
        
        #region getters
        public List<Character> activeCultists => _activeCultists;
        public Character currentCultLeader => _currentCultLeader;
        private bool hasCultLeader => _currentCultLeader != null;
        #endregion
        
        public CultLeaderEvent() {
            _activeCultists = new List<Character>();
        }
        public CultLeaderEvent(SaveDataCultLeaderEvent data) {
            _activeCultists = SaveUtilities.ConvertIDListToCharacters(data.activeCultists);
            if (!string.IsNullOrEmpty(data.cultLeaderID)) {
                _currentCultLeader = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(data.cultLeaderID);    
            }
        }
        
        public override void InitializeEvent() {
            Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
            Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
            Messenger.AddListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_MARKER_DESTROYED, OnMarkerDestroyed);
            Messenger.AddListener<Character, CharacterClass, CharacterClass>(CharacterSignals.CHARACTER_CLASS_CHANGE, OnCharacterChangedClass);
        }

        #region Listeners
        private void OnHourStarted() {
            var hoursBasedOnTicks = GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick);
            if (hoursBasedOnTicks == 18 && !hasCultLeader) {
                if (_activeCultists.Count >= Minimum_Cultist_Count) {
                    int chance = ChanceData.GetChance(CHANCE_TYPE.Base_Cult_Leader_Spawn_Chance);
                    if (_activeCultists.Count > Minimum_Cultist_Count) {
                        //+2% chance per excess cultist 
                        chance += 2 * (_activeCultists.Count - Minimum_Cultist_Count);
                    }
                    if (GameUtilities.RollChance(chance)) {
                        WeightedDictionary<Character> leaderWeights = GetCultLeaderWeights(_activeCultists);
                        leaderWeights.LogDictionaryValues($"Cult Leader Event weights:");
                        if (leaderWeights.GetTotalOfWeights() > 0) {
                            Character chosenLeader = leaderWeights.PickRandomElementGivenWeights();
                            chosenLeader.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Cult_Leader, chosenLeader);
                            SetCurrentCultLeaderInWorld(chosenLeader);
                        }    
                    }
                }
            }
        }
        private void OnTraitableGainedTrait(ITraitable traitable, Trait trait) {
            if (traitable is Character character && trait is Cultist) {
                _activeCultists.Add(character);
            }
        }
        private void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character removedBy) {
            if (traitable is Character character && trait is Cultist) {
                _activeCultists.Remove(character);
            }
        }
        private void OnMarkerDestroyed(Character character) {
            if (character.isDead) {
                if (_activeCultists.Contains(character)) {
                    _activeCultists.Remove(character);
                }
                //only unassign current cult leader if that characters marker is destroyed
                //this means that that character can no longer return and it is safe for us to spawn a new cult leader.
                if (_currentCultLeader == character) {
                    SetCurrentCultLeaderInWorld(null);
                }    
            }
        }
        private void OnCharacterChangedClass(Character character, CharacterClass previousClass, CharacterClass newClass) {
            //NOTE: Added checker for werewolf class since werewolf class is only temporary.
            //TODO: Improve This
            if (_currentCultLeader == character && newClass.className != "Cult Leader" && newClass.className != "Werewolf") {
                SetCurrentCultLeaderInWorld(null);
            }
        }
        #endregion

        #region Saving
        public override SaveDataWorldEvent Save() {
            SaveDataCultLeaderEvent leaderEvent = new SaveDataCultLeaderEvent();
            leaderEvent.Save(this);
            return leaderEvent;
        }
        #endregion

        #region Utilities
        private WeightedDictionary<Character> GetCultLeaderWeights(List<Character> choices) {
            WeightedDictionary<Character> weights = new WeightedDictionary<Character>();
            for (int i = 0; i < choices.Count; i++) {
                Character character = choices[i];
                if (character.traitContainer.HasTrait("Enslaved", "Necromancer")) {
                    //https://trello.com/c/HfbzyNN1/2987-necromancers-should-no-longer-be-able-to-change-into-cult-leaders
                    continue;
                }
                int weight = 20;
                if (character.traitContainer.HasTrait("Persuasive", "Evil", "Treacherous")) {
                    weight += 100;
                }
                if (character.traitContainer.HasTrait("Unattractive")) {
                    weight -= 10;
                }
                if (weight > 0) {
                    weights.AddElement(character, weight);
                }
            }
            return weights;
        }
        private void SetCurrentCultLeaderInWorld(Character character) {
            _currentCultLeader = character;
#if DEBUG_LOG
            Debug.Log($"{GameManager.Instance.TodayLogString()}Set cult leader in world to {_currentCultLeader?.name ?? "Null"}");
#endif
        }
#endregion
    }

    public class SaveDataCultLeaderEvent : SaveDataWorldEvent {
        public List<string> activeCultists;
        public string cultLeaderID;
        public override void Save(WorldEvent data) {
            base.Save(data);
            CultLeaderEvent cultLeaderEvent = data as CultLeaderEvent;
            activeCultists = SaveUtilities.ConvertSavableListToIDs(cultLeaderEvent.activeCultists);
            cultLeaderID = cultLeaderEvent.currentCultLeader == null ? string.Empty : cultLeaderEvent.currentCultLeader.persistentID;
        }
        public override WorldEvent Load() {
            return new CultLeaderEvent(this);
        }
    }
}