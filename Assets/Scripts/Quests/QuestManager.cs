using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Settings;
using Tutorial;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
namespace Quests {
    public class QuestManager : MonoBehaviour {

        public static QuestManager Instance;

        private List<Quest> _activeQuests;
        
        private void Awake() {
            Instance = this;
        }
        private void OnDestroy() {
            Messenger.RemoveListener<List<Character>, DemonicStructure>(Signals.CHARACTERS_ATTACKING_DEMONIC_STRUCTURE, 
                OnCharactersAttackingDemonicStructure);
            Messenger.RemoveListener<LocationStructure, Character, GoapPlanJob>(Signals.DEMONIC_STRUCTURE_DISCOVERED, OnDemonicStructureDiscovered);
            Messenger.RemoveListener<List<Character>>(Signals.ANGELS_ATTACKING_DEMONIC_STRUCTURE, 
                OnAngelsAttackingDemonicStructure);
        }

        #region Initialization
        public void InitializeAfterGameLoaded() {
            _activeQuests = new List<Quest>();
            CheckEliminateAllVillagersQuest();
            Messenger.AddListener<List<Character>, DemonicStructure>(Signals.CHARACTERS_ATTACKING_DEMONIC_STRUCTURE, 
                OnCharactersAttackingDemonicStructure);
            Messenger.AddListener<LocationStructure, Character, GoapPlanJob>(Signals.DEMONIC_STRUCTURE_DISCOVERED, OnDemonicStructureDiscovered);
            Messenger.AddListener<List<Character>>(Signals.ANGELS_ATTACKING_DEMONIC_STRUCTURE, 
                OnAngelsAttackingDemonicStructure);
        }
        #endregion

        #region Inquiry
        public T GetActiveQuest<T>() where T : Quest {
            for (int i = 0; i < _activeQuests.Count; i++) {
                Quest quest = _activeQuests[i];
                if (quest is T validQuest) {
                    return validQuest;
                }
            }
            return null;
        }
        #endregion
        
        #region Activation
        private void ActivateQuest(Quest quest) {
            _activeQuests.Add(quest);
            quest.Activate();
            QuestItem questItem = UIManager.Instance.questUI.ShowQuest(quest, true);
            quest.SetQuestItem(questItem);
        }
        private void ActivateQuest<T>(params object[] arguments) where T : Quest {
            Quest quest = System.Activator.CreateInstance(typeof(T), arguments) as Quest;
            Debug.Assert(quest != null, nameof(quest) + " != null");
            _activeQuests.Add(quest);
            quest.Activate();
            QuestItem questItem = UIManager.Instance.questUI.ShowQuest(quest, true);
            quest.SetQuestItem(questItem);
            Messenger.Broadcast(Signals.REACTION_QUEST_ACTIVATED, quest);
        }
        private void DeactivateQuest(Quest quest) {
            _activeQuests.Remove(quest);
            if (quest.questItem != null) {
                UIManager.Instance.questUI.HideQuestDelayed(quest);
            }
            quest.Deactivate();
        }
        #endregion
        
        #region Completion
        public void CompleteQuest(Quest quest) {
            DeactivateQuest(quest);
        }
        #endregion

        #region Eliminate All Villagers Quest
        private void CheckEliminateAllVillagersQuest() {
            if (SaveManager.Instance.currentSaveDataPlayer.completedTutorials
                .Contains(TutorialManager.Tutorial.Build_A_Kennel) || SettingsManager.Instance.settings.skipTutorials) {
                CreateEliminateAllVillagersQuest();
            } else {
                Messenger.AddListener<TutorialQuest>(Signals.TUTORIAL_QUEST_COMPLETED, OnTutorialQuestCompleted);
                Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
                Messenger.AddListener<bool>(Signals.ON_SKIP_TUTORIALS_CHANGED, OnSkipTutorialsToggled);
            }
        }
        private void OnTutorialQuestCompleted(TutorialQuest completedQuest) {
            if (completedQuest.tutorialType == TutorialManager.Tutorial.Build_A_Kennel) {
                CreateEliminateAllVillagersQuest();
            }
        }
        private void OnCharacterDied(Character character) {
            if (character.isNormalCharacter) {
                CreateEliminateAllVillagersQuest();
            }
        }
        private void OnSkipTutorialsToggled(bool skipTutorials) {
            if (skipTutorials) {
                CreateEliminateAllVillagersQuest();
            }
        }
        private void CreateEliminateAllVillagersQuest() {
            Messenger.RemoveListener<TutorialQuest>(Signals.TUTORIAL_QUEST_COMPLETED, OnTutorialQuestCompleted);
            Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
            Messenger.RemoveListener<bool>(Signals.ON_SKIP_TUTORIALS_CHANGED, OnSkipTutorialsToggled);
            EliminateAllVillagers eliminateAllVillagers = new EliminateAllVillagers();
            ActivateQuest(eliminateAllVillagers);
        }
        #endregion

        #region Counterattack
        private void OnCharactersAttackingDemonicStructure(List<Character> attackers, DemonicStructure targetStructure) {
            ActivateQuest<Counterattack>(attackers, targetStructure);
        }
        #endregion

        #region Report Demonic Structure
        private void OnDemonicStructureDiscovered(LocationStructure structure, Character reporter, GoapPlanJob job) {
            ActivateQuest<DemonicStructureDiscovered>(structure, reporter, job);
        }
        #endregion

        #region Divine Intervention
        private void OnAngelsAttackingDemonicStructure(List<Character> angels) {
            ActivateQuest<DivineIntervention>(angels);
        }
        #endregion
    }
}