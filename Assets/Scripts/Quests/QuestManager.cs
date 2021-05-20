using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Quests.Special_Popups;
using Settings;
using Tutorial;
using UnityEngine;
using UtilityScripts;
using Debug = System.Diagnostics.Debug;
namespace Quests {
    public class QuestManager : BaseMonoBehaviour {

        public static QuestManager Instance;

        public WinConditionTracker winConditionTracker { set; get; }
       
        /// <summary>
        /// List of active quests. NOTE: this does not include tutorials.
        /// </summary>
        private List<Quest> _activeQuests;
        
        public enum Special_Popup { 
            Threat, Special_Events, Pause_Reminder,
            Excalibur_Obtained, Disguised_Succubus, Activated_Ankh, Dragon_Left, Dragon_Awakened, Sleeping_Dragon,
            The_Sword, The_Crack, The_Necronomicon, Cult_Leader
        }

        
        #region getters
        /// <summary>
        /// List of active quests. NOTE: this does not include tutorials.
        /// </summary>
        public List<Quest> activeQuests => _activeQuests;
        #endregion
        
        private void Awake() {
            Instance = this; ;
            _activeQuests = new List<Quest>();
        }
        protected override void OnDestroy() {
            base.OnDestroy();
            Instance = null;
            // Messenger.RemoveListener<List<Character>, DemonicStructure>(PartySignals.CHARACTERS_ATTACKING_DEMONIC_STRUCTURE, OnCharactersAttackingDemonicStructure);
            // Messenger.RemoveListener<LocationStructure, Character, GoapPlanJob>(JobSignals.DEMONIC_STRUCTURE_DISCOVERED, OnDemonicStructureDiscovered);
            //Messenger.RemoveListener<List<Character>>(PlayerQuestSignals.ANGELS_ATTACKING_DEMONIC_STRUCTURE, OnAngelsAttackingDemonicStructure);
            Messenger.RemoveListener<Character, DemonicStructure>(CharacterSignals.CHARACTER_HIT_DEMONIC_STRUCTURE, OnSingleCharacterAttackedDemonicStructure);
        }
        
        public void LoadWinConditionTracker(SaveDataWinConditionTracker data) {
            InitializeWinConditionTracker();
            winConditionTracker?.Initialize(CharacterManager.Instance.allCharacters);
            winConditionTracker?.LoadReferences(data);
        }

        #region Win Condition
        private void InitializeWinConditionTracker() {
            if (PlayerManager.Instance.player.hasAlreadyWon) {
                //if player has already won the game, do not spawn win condition any more to save processing power.
                return;
            }
            switch (WorldSettings.Instance.worldSettingsData.victoryCondition) {
                case VICTORY_CONDITION.Eliminate_All:
                    winConditionTracker = new WipeOutAllVillagersWinConditionTracker();
                    break;
                case VICTORY_CONDITION.Wipe_Out_Village_On_Day:
                    winConditionTracker = new WipeOutAllUntilDayWinConditionTracker();
                    break;
                case VICTORY_CONDITION.Wipe_Elven_Kingdom_Survive_Humans:
                    winConditionTracker = new HumansSurviveAndElvesWipedOutWinConditionTracker();
                    break;
                case VICTORY_CONDITION.Kill_By_Plague:
                    winConditionTracker = new PlagueDeathWinConditionTracker();
                    break;
                case VICTORY_CONDITION.Create_Demon_Cult:
                    winConditionTracker = new RecruitCultistsWinConditionTracker();
                    break;
                case VICTORY_CONDITION.Summon_Ruinarch:
                    winConditionTracker = new UpgradePortalWinConditionTracker();
                    break;
                case VICTORY_CONDITION.Sandbox:
                    //no win condition.
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
#if DEBUG_LOG
            UnityEngine.Debug.Log($"Set win condition to {winConditionTracker?.ToString()}");
#endif
        }
        public T GetWinConditionTracker<T>() where T : WinConditionTracker {
            if (winConditionTracker is T converted) {
                return converted;
            }
            throw new Exception($"Problem trying to convert Win Condition {winConditionTracker}");
        }
#endregion
        
        #region Initialization
        public void InitializeAfterGameLoaded() {
            // Messenger.AddListener<List<Character>, DemonicStructure>(PartySignals.CHARACTERS_ATTACKING_DEMONIC_STRUCTURE, OnCharactersAttackingDemonicStructure);
            // Messenger.AddListener<LocationStructure, Character, GoapPlanJob>(JobSignals.DEMONIC_STRUCTURE_DISCOVERED, OnDemonicStructureDiscovered);
            //Messenger.AddListener<List<Character>>(PlayerQuestSignals.ANGELS_ATTACKING_DEMONIC_STRUCTURE, OnAngelsAttackingDemonicStructure);
            Messenger.AddListener<Character, DemonicStructure>(CharacterSignals.CHARACTER_HIT_DEMONIC_STRUCTURE, OnSingleCharacterAttackedDemonicStructure);
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "CenterButton");
        }
        public void InitializeAfterLoadoutPicked(){
            if (!SaveManager.Instance.useSaveData) {
                InitializeWinConditionTracker();
                //TODO: Try to remove checking
                winConditionTracker?.Initialize(CharacterManager.Instance.allCharacters);    
            }
            if (WorldSettings.Instance.worldSettingsData.worldType != WorldSettingsData.World_Type.Tutorial) {
                TryCreateWinConditionQuest();
                InstantiatePendingSpecialPopups();    
            }
            if (winConditionTracker != null) {
                winConditionTracker.AddStepsToBookmark();
            }
        }
        public void InitializeAfterStartTutorial(){
            TryCreateWinConditionQuest();
            InstantiatePendingSpecialPopups();    
        }
        private void InstantiatePendingSpecialPopups() {
            if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom) {
                return; //Tutorials shouldn't show up on Customize Worlds
            }
            List<Special_Popup> completedSpecialPopups = SaveManager.Instance.currentSaveDataPlayer.completedSpecialPopups;
            Special_Popup[] specialPopups = CollectionUtilities.GetEnumValues<Special_Popup>();
            for (int i = 0; i < specialPopups.Length; i++) {
                Special_Popup popup = specialPopups[i];
                //only instantiate popup if it has not yet been completed
                bool instantiateTutorial = completedSpecialPopups.Contains(popup) == false;
                if (instantiateTutorial) {
                    SpecialPopup specialPopup = InstantiateSpecialPopup(popup);
                    specialPopup.Initialize();
                }
            }
        }
        private SpecialPopup InstantiateSpecialPopup(Special_Popup popup) {
            string noSpacesName = UtilityScripts.Utilities.RemoveAllWhiteSpace(UtilityScripts.Utilities.
                NormalizeStringUpperCaseFirstLettersNoSpace(popup.ToString()));
            string typeName = $"Quests.Special_Popups.{ noSpacesName }, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            Type type = Type.GetType(typeName);
            if (type != null) {
                SpecialPopup specialPopup = Activator.CreateInstance(type) as SpecialPopup;
                return specialPopup;
            }
            throw new Exception($"Could not instantiate special popup {noSpacesName}");
        }
        #endregion

        #region Inquiry
        public T GetActiveQuest<T>() where T : Quest {
            for (int i = 0; i < activeQuests.Count; i++) {
                Quest quest = activeQuests[i];
                if (quest is T validQuest) {
                    return validQuest;
                }
            }
            return null;
        }
        public bool IsQuestActive<T>() where T : Quest {
            for (int i = 0; i < activeQuests.Count; i++) {
                Quest quest = activeQuests[i];
                if (quest is T) {
                    return true;
                }
            }
            return false;
        }
        public int GetActiveQuestsCount() {
            return activeQuests.Count;
        }
        #endregion
        
        #region Activation
        public void ActivateQuest(Quest quest) {
            activeQuests.Add(quest);
            quest.Activate();
            if (quest is SteppedQuest steppedQuest) {
                QuestItem questItem = UIManager.Instance.questUI.ShowQuest(steppedQuest, true);
                steppedQuest.SetQuestItem(questItem);
                steppedQuest.CheckForAlreadyCompletedSteps();
            }
            Messenger.Broadcast(PlayerQuestSignals.QUEST_ACTIVATED, quest);
        }
        private void ActivateQuest<T>(params object[] arguments) where T : Quest {
            Quest quest = System.Activator.CreateInstance(typeof(T), arguments) as Quest;
            Debug.Assert(quest != null, nameof(quest) + " != null");
            ActivateQuest(quest);
        }
        private void DeactivateQuest(Quest quest) {
            activeQuests.Remove(quest);
            if (quest is SteppedQuest steppedQuest && steppedQuest.questItem != null) {
                UIManager.Instance.questUI.HideQuestDelayed(steppedQuest);
            }
            quest.Deactivate();
        }
        #endregion
        
        #region Completion
        public void CompleteQuest(Quest quest) {
            DeactivateQuest(quest);
            if (quest is SpecialPopup specialPopup) {
                if (specialPopup.isRepeatable) {
                    //spawn popup again.
                    InstantiateSpecialPopup(specialPopup.specialPopupType).Initialize();
                } else {
                    SaveManager.Instance.currentSaveDataPlayer.AddSpecialPopupAsCompleted(specialPopup.specialPopupType);    
                }
            }
        }
        #endregion

        #region Win Condition
        private void TryCreateWinConditionQuest() {
            if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
                if (SettingsManager.Instance.settings.skipTutorials) {
                    // SpawnWinConditionQuest();
                } else {
                    Messenger.AddListener(PlayerQuestSignals.FINISHED_IMPORTANT_TUTORIALS, OnImportantTutorialsFinished);    
                }
            } else {
                // SpawnWinConditionQuest();
            }
        }
        private void OnImportantTutorialsFinished() {
            // SpawnWinConditionQuest();
        }
        // private void SpawnWinConditionQuest() {
        //            if (PlayerManager.Instance.player.hasAlreadyWon) {
        //        return; //we hage a return here
        //    }
        //     Messenger.RemoveListener(PlayerQuestSignals.FINISHED_IMPORTANT_TUTORIALS, OnImportantTutorialsFinished);
        //     switch (WorldSettings.Instance.worldSettingsData.victoryCondition) {
        //         case VICTORY_CONDITION.Eliminate_All:
        //         CreateEliminateAllVillagersQuest();
        //         break;
        //         case VICTORY_CONDITION.Kill_By_Psychopath_Ritual:
        //         CreateKillVillagersByPsychopathQuest();
        //         break;
        //         case VICTORY_CONDITION.Wiped_Village_On_Day8:
        //         CreateEliminateAllVillagersOnGivenDateQuest();
        //         break;
        //         case VICTORY_CONDITION.Wipe_Elven_Kingdom_Survive_Humans:
        //         CreateWipeElvenKingdomAndSurviveHumans();
        //         break;
        //         case VICTORY_CONDITION.Declare_3_Wars:
        //         CreateDeclareWar();
        //         break;
        //         case VICTORY_CONDITION.Kill_By_Plague:
        //         CreateKillByPlague();
        //         break;
        //         case VICTORY_CONDITION.Create_Demon_Cult:
        //         CreateDemonCult();
        //         break;
        //         case VICTORY_CONDITION.Summon_Ruinarch:
        //         CreateSummonTheDemon();
        //         break;
        //         case VICTORY_CONDITION.Sandbox:
        //         //no win condition quest
        //         break;
        //         default:
        //         throw new ArgumentOutOfRangeException();
        //     }
        // }
        private void CreateEliminateAllVillagersQuest() {
            if (!IsQuestActive<EliminateAllVillagers>()) {
                EliminateAllVillagers eliminateAllVillagers = new EliminateAllVillagers();
                ActivateQuest(eliminateAllVillagers);
            }
        }

        private void CreateKillVillagersByPsychopathQuest() {
            if (!IsQuestActive<KillVillagersByPsychopath>()) {
                KillVillagersByPsychopath killVillagersByPsychopath = new KillVillagersByPsychopath();
                ActivateQuest(killVillagersByPsychopath);
            }
        }

        private void CreateEliminateAllVillagersOnGivenDateQuest() {
            if (!IsQuestActive<EliminateAllVillagersOnGivenDate>()) {
                EliminateAllVillagersOnGivenDate eliminateAllVillagersOnGivenDate = new EliminateAllVillagersOnGivenDate();
                ActivateQuest(eliminateAllVillagersOnGivenDate);
            }
        }

        private void CreateWipeElvenKingdomAndSurviveHumans() {
            if (!IsQuestActive<WipeElvenKingdomAndSurviveHuman>()) {
                WipeElvenKingdomAndSurviveHuman eliminateAllVillagersOnGivenDate = new WipeElvenKingdomAndSurviveHuman();
                ActivateQuest(eliminateAllVillagersOnGivenDate);
            }
        }

        private void CreateDeclareWar() {
            if (!IsQuestActive<DeclareWarQuest>()) {
                DeclareWarQuest eliminateAllVillagersOnGivenDate = new DeclareWarQuest();
                ActivateQuest(eliminateAllVillagersOnGivenDate);
            }
        }

        private void CreateKillByPlague() {
            if (!IsQuestActive<EliminateNumberOfVillagersUsingPlague>()) {
                EliminateNumberOfVillagersUsingPlague eliminateAllVillagersOnGivenDate = new EliminateNumberOfVillagersUsingPlague();
                ActivateQuest(eliminateAllVillagersOnGivenDate);
            }
        }

        private void CreateDemonCult() {
            if (!IsQuestActive<CreateDemonCultFaction>()) {
                CreateDemonCultFaction killVillagersByPsychopath = new CreateDemonCultFaction();
                ActivateQuest(killVillagersByPsychopath);
            }
        }

        private void CreateSummonTheDemon() {
            if (!IsQuestActive<SummonTheDemon>()) {
                SummonTheDemon SummonTheDemon = new SummonTheDemon();
                ActivateQuest(SummonTheDemon);
            }
        }
        #endregion

        // #region Counterattack
        // private void OnCharactersAttackingDemonicStructure(List<Character> attackers, DemonicStructure targetStructure) {
        //     ActivateQuest<Counterattack>(attackers, targetStructure);
        // }
        // #endregion

        // #region Report Demonic Structure
        // private void OnDemonicStructureDiscovered(LocationStructure structure, Character reporter, GoapPlanJob job) {
        //     ActivateQuest<DemonicStructureDiscovered>(structure, reporter, job);
        // }
        // #endregion

        #region Divine Intervention
        private void OnAngelsAttackingDemonicStructure(List<Character> angels) {
            ActivateQuest<DivineIntervention>(angels);
        }
        #endregion

        #region Center Button
        public void OnClickCenterButton() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "CenterButton");
        }
        #endregion

        #region Under Attack
        private void OnSingleCharacterAttackedDemonicStructure(Character character, DemonicStructure demonicStructure) {
            if (demonicStructure.currentAttackers.Count == 1 && !InnerMapCameraMove.Instance.CanSee(demonicStructure)) {
                PlayerUI.Instance.ShowGeneralConfirmation("Under Attack", $"Your {demonicStructure.name} is under attack!", 
                    onClickCenter: demonicStructure.CenterOnStructure);
            }
        }
        #endregion
    }
}