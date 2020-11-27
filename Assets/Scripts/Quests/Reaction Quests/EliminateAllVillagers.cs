using System;
using System.Collections.Generic;
using System.Linq;
using Quests.Steps;
using UnityEngine;
namespace Quests {
    public class EliminateAllVillagers : ReactionQuest {


        private EliminateVillagerStep _eliminateVillagerStep;

        #region getters
        public override Type serializedData => typeof(SaveDataEliminateAllVillagers);
        #endregion
        
        public EliminateAllVillagers() : base($"Eliminate All Villagers") { }
        protected override void ConstructSteps() {
            List<Character> villagers = GetAllCharactersToBeEliminated();
            _eliminateVillagerStep = new EliminateVillagerStep(GetEliminateAllVillagersDescription, villagers);
            _eliminateVillagerStep.SetObjectsToCenter(villagers.Where(x => !ShouldConsiderCharacterAsEliminated(x)).Select(x => x as ISelectable).ToList());
            
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(_eliminateVillagerStep),
            };
            
            Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN, OnKeyPressed);
        }
        private void OnKeyPressed(KeyCode keyCode) {
            if (keyCode == KeyCode.Tab) {
                if (!_eliminateVillagerStep.isCompleted) {
                    _eliminateVillagerStep.CenterCycle();    
                }
            }
        }

        #region Step Helpers
        private string GetEliminateAllVillagersDescription(List<Character> remainingTargets, int totalCharactersToEliminate) {
            return $"Villagers Remaining: {remainingTargets.Count.ToString()}"; // /{totalCharactersToEliminate.ToString()}
        }
        #endregion

        #region Utilities
        public static bool ShouldConsiderCharacterAsEliminated(Character character) {
            if (character.isDead) {
                return true;
            }
            if (character.traitContainer.HasTrait("Cultist")) {
                return true;
            }
            if (character.faction != null) {
                if (!character.faction.isMajorNonPlayerOrVagrant && character.faction.factionType.type != FACTION_TYPE.Ratmen) {
                    return true;
                }
            }
            return false;
        }
        private List<Character> GetAllCharactersToBeEliminated() {
            List<Character> characters = new List<Character>();
            for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                Character character = CharacterManager.Instance.allCharacters[i];
                if (character.isNormalCharacter && character.race.IsSapient()) { 
                    characters.Add(character);
                }
            }
            return characters;
        }
        #endregion
    }
    
    public class SaveDataEliminateAllVillagers : SaveDataReactionQuest {
        public override ReactionQuest Load() {
            return new EliminateAllVillagers();
        }
    }
}