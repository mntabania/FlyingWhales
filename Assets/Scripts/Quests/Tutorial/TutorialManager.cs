using System;
using System.Collections.Generic;
using System.Linq;
using Settings;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.Video;
using UtilityScripts;
namespace Tutorial {
    public class TutorialManager : MonoBehaviour {

        public static TutorialManager Instance;
        private const int MaxActiveTutorials = 1;
        public enum Tutorial { 
            Basic_Controls = 0, 
            Build_A_Kennel = 1, 
            Defend_A_Structure = 2, 
            Elemental_Interactions = 3, 
            Character_Info = 4,
            Invade_A_Village = 5,
            Regional_Map = 6,
            Share_An_Intel = 9,
            Afflictions = 10,
            Torture_Chambers = 11,
            Threat = 12,
            Counterattack = 13,
            Divine_Intervention = 14,
            Chaos_Orbs_Tutorial = 15,
            Special_Events = 16,
            Griefstricken,
            Killed_By_Monster,
            Booby_Trap,
            Rumor,
            Zombie_Virus,
            Frame_Up,
            Pause_Reminder
        }

        private List<ImportantTutorial> _activeImportantTutorials;
        private List<ImportantTutorial> _waitingImportantTutorials;
        private List<BonusTutorial> _activeBonusTutorials;
        private List<TutorialQuest> _instantiatedTutorials;

        public bool alwaysResetTutorialsOnStart;
        
        //Video Clips
        public VideoClip demonicStructureVideoClip;
        public VideoClip fireDamageVideoClip;
        public VideoClip villageVideoClip;
        public VideoClip storeIntelVideoClip;
        public VideoClip shareIntelVideoClip;
        public VideoClip blessedVideoClip;
        public VideoClip timeControlsVideoClip;
        public VideoClip areaVideoClip;
        public VideoClip spellsVideoClip;
        public VideoClip afflictionsVideoClip;
        public Texture deadCharactersImage;
        public VideoClip afflictButtonVideoClip;
        public VideoClip spellsTabVideoClip;
        public Texture buildStructureButton;
        public VideoClip chambersVideo;
        public Texture tortureButton;
        public Texture threatPicture;
        public Texture counterattackPicture;
        public Texture divineInterventionPicture;
        public Texture seizeImage;
        public VideoClip afflictionDetailsVideoClip;
        public VideoClip breedVideoClip;
        public Texture infoTab;
        public Texture moodTab;
        public Texture relationsTab;
        public Texture logsTab;
        public VideoClip homeStructureVideo;
        public Texture necronomiconPicture;
        public Texture griefstrickenLog;
        public Texture killedByMonsterLog;
        public Texture tileObjectOwner;
        public Texture structureInfoResidents;
        public Texture assumedThief;
        public Texture boobyTrapLog;
        public Texture infectedLog;
        public Texture recipientLog;

        #region Monobehaviours
        private void Awake() {
            Instance = this;
        }
        private void OnDestroy() {
            Messenger.RemoveListener<bool, bool>(Signals.ON_SKIP_TUTORIALS_CHANGED, OnSkipTutorialsChanged);
        }
        private void LateUpdate() {
            if (GameManager.Instance.gameHasStarted) {
                CheckIfNewTutorialCanBeActivated();    
            }
        }
        #endregion

        #region Initialization
        public void Initialize() {
            _activeImportantTutorials = new List<ImportantTutorial>();
            _waitingImportantTutorials = new List<ImportantTutorial>();
            _activeBonusTutorials = new List<BonusTutorial>();
            _instantiatedTutorials = new List<TutorialQuest>();
            
            if (SaveManager.Instance.currentSaveDataPlayer.completedTutorials == null || alwaysResetTutorialsOnStart) {
                SaveManager.Instance.currentSaveDataPlayer.InitializeTutorialData();
            }
            if (WorldConfigManager.Instance.isDemoWorld == false) {
                InstantiatePendingTutorials();
            }
            Messenger.AddListener<bool, bool>(Signals.ON_SKIP_TUTORIALS_CHANGED, OnSkipTutorialsChanged);
        }
        public void InstantiatePendingTutorials() {
            if (SettingsManager.Instance.settings.skipTutorials) {
                return;
            }
            //Create instances for all uncompleted tutorials.
            List<Tutorial> completedTutorials = SaveManager.Instance.currentSaveDataPlayer.completedTutorials;
            Tutorial[] allTutorials = CollectionUtilities.GetEnumValues<Tutorial>();
            for (int i = 0; i < allTutorials.Length; i++) {
                Tutorial tutorial = allTutorials[i];
                //only instantiate tutorial if it has not yet been completed and has not yet been instantiated
                bool instantiateTutorial = completedTutorials.Contains(tutorial) == false && _instantiatedTutorials.Count(quest => quest.tutorialType == tutorial) == 0;
                if (WorldConfigManager.Instance.isDemoWorld && instantiateTutorial) {
                    //if is demo world, check if tutorial should be enabled in demo
                    instantiateTutorial = WorldConfigManager.Instance.demoTutorials.Contains(tutorial);
                }
                if (instantiateTutorial) {
                   InstantiateTutorial(tutorial);
                }
            }
        }
        public TutorialQuest InstantiateTutorial(Tutorial tutorial) {
            string noSpacesName = UtilityScripts.Utilities.RemoveAllWhiteSpace(UtilityScripts.Utilities.
                NormalizeStringUpperCaseFirstLettersNoSpace(tutorial.ToString()));
            string typeName = $"Tutorial.{ noSpacesName }, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            Type type = Type.GetType(typeName);
            if (type != null) {
                TutorialQuest tutorialQuest = Activator.CreateInstance(type) as TutorialQuest;
                _instantiatedTutorials.Add(tutorialQuest);
                return tutorialQuest;
            }
            throw new Exception($"Could not instantiate tutorial quest {noSpacesName}");
        }
        #endregion

        #region Inquiry
        public bool HasTutorialBeenCompleted(Tutorial tutorial) {
            return SaveManager.Instance.currentSaveDataPlayer.completedTutorials.Contains(tutorial);
        }
        public bool IsTutorialCurrentlyActive(Tutorial tutorial) {
            return _instantiatedTutorials.Any(t => t.tutorialType == tutorial && t.isActivated);
        }
        private bool IsBonusTutorial(TutorialQuest tutorialQuest) {
            return tutorialQuest is BonusTutorial;
        }
        public bool HasActiveLogQuest() {
            for (int i = 0; i < _activeBonusTutorials.Count; i++) {
                if (_activeBonusTutorials[i] is LogQuest) {
                    return true;
                }
            }
            return false;
        }
        public int GetAllActiveTutorialsCount() {
            return _activeBonusTutorials.Count + _activeImportantTutorials.Count + _waitingImportantTutorials.Count;
        }
        #endregion

        #region Completion
        public void CompleteTutorialQuest(TutorialQuest tutorial) {
            SaveManager.Instance.currentSaveDataPlayer.AddTutorialAsCompleted(tutorial.tutorialType);
            Messenger.Broadcast(Signals.TUTORIAL_QUEST_COMPLETED, tutorial);
            DeactivateTutorial(tutorial);
            if (IsBonusTutorial(tutorial) == false) {
                CheckIfAllTutorialsCompleted();    
            }
        }
        private void CheckIfAllTutorialsCompleted() {
            if (_instantiatedTutorials.Count == 0 || _instantiatedTutorials.Count(x => IsBonusTutorial(x) == false) == 0) {
                //all non-bonus tutorials completed
                PlayerUI.Instance.ShowGeneralConfirmation("Finished Tutorial",
                    "You're done with the Tutorials! " +
                    "Feel free to use the remaining time to play around with the various unlocked options... " +
                    $"or just wipe out all Villagers as soon as possible!");
                SettingsManager.Instance.ManualToggleSkipTutorials(true, false);
                Messenger.Broadcast(Signals.FINISHED_IMPORTANT_TUTORIALS);
            }
        }
        #endregion

        #region Failure
        public void FailTutorialQuest(TutorialQuest tutorial) {
            DeactivateTutorial(tutorial);
        }
        #endregion

        #region Availability
        public void AddTutorialToWaitList(ImportantTutorial tutorialQuest) {
            _waitingImportantTutorials.Add(tutorialQuest);
            _waitingImportantTutorials = _waitingImportantTutorials.OrderBy(q => q.priority).ToList();
            CheckIfNewTutorialCanBeActivated();
        }
        public void RemoveTutorialFromWaitList(ImportantTutorial tutorialQuest) {
            _waitingImportantTutorials.Remove(tutorialQuest);
        }
        #endregion

        #region Presentation
        private void CheckIfNewTutorialCanBeActivated() {
            if (_waitingImportantTutorials.Count > 0 && _activeImportantTutorials.Count < MaxActiveTutorials) {
                //new tutorial can be shown.
                //check number of tutorials that can be shown. 3 at maximum
                int missingTutorials = MaxActiveTutorials - _activeImportantTutorials.Count;
                if (missingTutorials > _waitingImportantTutorials.Count) {
                    //if number of missing tutorials is greater than the available tutorials, then just show the available ones.
                    missingTutorials = _waitingImportantTutorials.Count;
                }
                for (int i = 0; i < missingTutorials; i++) {
                    //get first tutorial in list, since tutorials are sorted by priority beforehand.
                    ImportantTutorial availableTutorial = _waitingImportantTutorials[0];
                    ActivateTutorial(availableTutorial);        
                }
            }
        }
        private void ActivateTutorial(ImportantTutorial tutorialQuest) {
            _activeImportantTutorials.Add(tutorialQuest);    
            RemoveTutorialFromWaitList(tutorialQuest);
            tutorialQuest.Activate();
            ShowTutorial(tutorialQuest);
        }
        public void ActivateTutorial(BonusTutorial bonusTutorial) {
            _activeBonusTutorials.Add(bonusTutorial);
            bonusTutorial.Activate();
            ShowTutorial(bonusTutorial);
        }
        public void ActivateTutorialButDoNotShow(BonusTutorial bonusTutorial) {
            _activeBonusTutorials.Add(bonusTutorial);
            bonusTutorial.Activate();
        }
        public void ShowTutorial(TutorialQuest tutorialQuest) {
            Assert.IsTrue(tutorialQuest.isActivated, $"{tutorialQuest.questName} is being shown, but has not yet been activated.");
            QuestItem questItem = UIManager.Instance.questUI.ShowQuest(tutorialQuest);
            tutorialQuest.SetQuestItem(questItem);
        }
        private void DeactivateTutorial(TutorialQuest tutorialQuest) {
            if (tutorialQuest is ImportantTutorial importantTutorial) {
                _activeImportantTutorials.Remove(importantTutorial);
                _waitingImportantTutorials.Remove(importantTutorial); //this is for cases when a tutorial is in the waiting list, but has been deactivated.    
            } else if (tutorialQuest is BonusTutorial bonusTutorial) {
                _activeBonusTutorials.Remove(bonusTutorial);
            }
            _instantiatedTutorials.Remove(tutorialQuest);
            if (tutorialQuest.questItem != null) {
                UIManager.Instance.questUI.HideQuestDelayed(tutorialQuest);
            }
            tutorialQuest.Deactivate();
        }
        #endregion

        #region For Testing
        public void ResetTutorials() {
            List<Tutorial> completedTutorials = new List<Tutorial>(SaveManager.Instance.currentSaveDataPlayer.completedTutorials);
            SaveManager.Instance.currentSaveDataPlayer.ResetTutorialProgress();
            //respawn previously completed tutorials
            Tutorial[] allTutorials = CollectionUtilities.GetEnumValues<Tutorial>();
            for (int i = 0; i < allTutorials.Length; i++) {
                Tutorial tutorial = allTutorials[i];
                if (completedTutorials.Contains(tutorial)) {
                   InstantiateTutorial(tutorial);
                }
            }
        }
        #endregion

        #region Listeners
        private void OnSkipTutorialsChanged(bool skipTutorials, bool deSpawnExisting) {
            if (skipTutorials) {
                if (deSpawnExisting) {
                    //remove all showing tutorials
                    List<TutorialQuest> tutorialsToDeactivate = new List<TutorialQuest>(_instantiatedTutorials);
                    for (int i = 0; i < tutorialsToDeactivate.Count; i++) {
                        TutorialQuest tutorialQuest = tutorialsToDeactivate[i];
                        DeactivateTutorial(tutorialQuest);    
                    }    
                }
            } else {
                //instantiate incomplete tutorials
                InstantiatePendingTutorials();
            }
        }
        #endregion


    }
}