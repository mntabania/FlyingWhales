using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Settings;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.Video;
using UtilityScripts;
namespace Tutorial {
    public class TutorialManager : BaseMonoBehaviour {

        public static TutorialManager Instance;
        private const int MaxActiveTutorials = 1;

        public enum Tutorial_Type {
            Unlocking_Bonus_Powers,
            Upgrading_The_Portal,
            Mana,
            Chaotic_Energy,
            Storing_Targets,
            Prism,
            Maraud,
            Intel,
            Spirit_Energy,
            Migration_Controls,
            Base_Building,
            Abilities,
            Resistances,
            Time_Management,
            Target_Menu,
        }
        
        [System.Obsolete("Old Tutorials will be removed")]
        public enum Tutorial {
            Basic_Controls = 0,
            Build_A_Kennel = 1,
            Defend_A_Structure = 2,
            Elemental_Interactions = 3,
            Character_Info = 4,
            Spawn_An_Invader = 5,
            Share_An_Intel = 9,
            Afflictions = 10,
            Prison = 11,
            Chaos_Orbs_Tutorial = 15,
            Griefstricken,
            Killed_By_Monster,
            Booby_Trap,
            Rumor,
            Zombie_Virus,
            Frame_Up,
            Faction_Info,
            Create_A_Cultist,
            Biolab_Tutorial,
            Meddler_Tutorial
        }

        /// <summary>
        /// Tutorial types that are part of the main tutorial.
        /// </summary>
        private readonly Tutorial[] mainTutorialTypes = new[] {
            Tutorial.Basic_Controls,
            Tutorial.Character_Info,
            Tutorial.Afflictions,
            Tutorial.Share_An_Intel,
            Tutorial.Elemental_Interactions,
            Tutorial.Build_A_Kennel,
            Tutorial.Spawn_An_Invader,
        };
        /// <summary>
        /// Tutorial types that are NOT part of the main tutorial.
        /// </summary>
        private readonly Tutorial[] bonusTutorialTypes = new[] {
            Tutorial.Defend_A_Structure,
            Tutorial.Prison,
            Tutorial.Chaos_Orbs_Tutorial,
            Tutorial.Griefstricken,
            Tutorial.Killed_By_Monster,
            Tutorial.Booby_Trap,
            Tutorial.Rumor,
            Tutorial.Zombie_Virus,
            Tutorial.Frame_Up,
            Tutorial.Faction_Info,
            Tutorial.Create_A_Cultist,
            Tutorial.Biolab_Tutorial,
            Tutorial.Meddler_Tutorial,
        };

        private List<ImportantTutorial> _activeImportantTutorials;
        private List<ImportantTutorial> _waitingImportantTutorials;
        private List<Tutorial> _completedImportantTutorials;
        private List<BonusTutorial> _activeBonusTutorials;
        private List<TutorialQuest> _instantiatedTutorials;

        //Video Clips
        public VideoClip demonicStructureVideoClip;
        public VideoClip villageVideoClip;
        public VideoClip storeIntelVideoClip;
        public VideoClip shareIntelVideoClip;
        public VideoClip blessedVideoClip;
        public VideoClip timeControlsVideoClip;
        public VideoClip areaVideoClip;
        public VideoClip spellsVideoClip;
        public VideoClip afflictionsVideoClip;
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
        public Texture brainWashButton;
        public VideoClip defilerChamberVideo;
        public Texture factionInfo;
        
        public bool hasCompletedImportantTutorials { get; private set; }
        

        #region Monobehaviours
        private void Awake() {
            Instance = this;
            _loadedTutorialData = new Dictionary<Tutorial_Type, TutorialScriptableObjectData>();
            _activeImportantTutorials = new List<ImportantTutorial>();
            _waitingImportantTutorials = new List<ImportantTutorial>();
            _activeBonusTutorials = new List<BonusTutorial>();
            _instantiatedTutorials = new List<TutorialQuest>();
            _completedImportantTutorials = new List<Tutorial>();
        }
        protected override void OnDestroy() {
            base.OnDestroy();
            Instance = null;
            _loadedTutorialData = null;
        }
        private void LateUpdate() {
            if (GameManager.Instance != null && GameManager.Instance.gameHasStarted) {
                CheckIfNewTutorialCanBeActivated();    
            }
        }
        #endregion

        #region Initialization
        public void Initialize() {
            hasCompletedImportantTutorials = WorldSettings.Instance.worldSettingsData.worldType != WorldSettingsData.World_Type.Tutorial;
            if (WorldSettings.Instance.worldSettingsData.worldType != WorldSettingsData.World_Type.Tutorial) {
                // Instantiate all pending bonus tutorials. NOTE: In tutorial world this is called after Start Popup is hidden
                // InstantiatePendingBonusTutorials();    
            }
        }
        // /// <summary>
        // /// Instantiate all Important tutorials. NOTE: This is called after Start Popup is hidden
        // /// <see cref="PopUpScreensUI.HideStartDemoScreen"/>
        // /// </summary>
        // public void InstantiateImportantTutorials() {
        //     if (SettingsManager.Instance.settings.skipTutorials) {
        //         hasCompletedImportantTutorials = true;
        //         return; //do not create tutorials if skip tutorials switch is on.
        //     } else {
        //         hasCompletedImportantTutorials = false;
        //     }
        //     for (int i = 0; i < mainTutorialTypes.Length; i++) {
        //         Tutorial tutorial = mainTutorialTypes[i];
        //         InstantiateTutorial(tutorial);
        //     }
        // }
        // [System.Obsolete("This should no longer used.")]
        // public void InstantiatePendingBonusTutorials() {
        //     if (SettingsManager.Instance.settings.skipAdvancedTutorials) {
        //         return; //do not create tutorials if skip advanced tutorials switch is on.
        //     }
        //     if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom) {
        //         return; //Tutorials shouldn't show up on Customize Worlds
        //     }
        //     //Create instances for all uncompleted tutorials.
        //     List<Tutorial> completedTutorials = SaveManager.Instance.currentSaveDataPlayer.completedBonusTutorials;
        //     for (int i = 0; i < bonusTutorialTypes.Length; i++) {
        //         Tutorial tutorial = bonusTutorialTypes[i];
        //         //only instantiate tutorial if it has not yet been completed and has not yet been instantiated
        //         bool instantiateTutorial = completedTutorials.Contains(tutorial) == false && _instantiatedTutorials.Count(quest => quest.tutorialType == tutorial) == 0;
        //         if (instantiateTutorial) {
        //            InstantiateTutorial(tutorial);
        //         }
        //     }
        // }
        public void InstantiateTutorial(Tutorial tutorial) {
            string noSpacesName = UtilityScripts.Utilities.RemoveAllWhiteSpace(UtilityScripts.Utilities.
                NormalizeStringUpperCaseFirstLettersNoSpace(tutorial.ToString()));
            string typeName = $"Tutorial.{ noSpacesName }, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            Type type = Type.GetType(typeName);
            if (type != null) {
                TutorialQuest tutorialQuest = Activator.CreateInstance(type) as TutorialQuest;
                _instantiatedTutorials.Add(tutorialQuest);
                return;
            }
            throw new Exception($"Could not instantiate tutorial quest {noSpacesName}");
        }
        #endregion

        #region Inquiry
        // public bool HasTutorialBeenCompleted(Tutorial tutorial) {
        //     if (mainTutorialTypes.Contains(tutorial)) {
        //         if (SettingsManager.Instance.settings.skipTutorials) {
        //             //if tutorials are skipped then when this is used always return true
        //             return true;
        //         }
        //     } else if (bonusTutorialTypes.Contains(tutorial)) {
        //         if (SettingsManager.Instance.settings.skipAdvancedTutorials) {
        //             //if advanced tutorials are skipped then when this is used always return true
        //             return true;
        //         }
        //     }
        //     return SaveManager.Instance.currentSaveDataPlayer.completedBonusTutorials.Contains(tutorial) || _completedImportantTutorials.Contains(tutorial);
        // }
        // public bool HasTutorialBeenCompletedInCurrentPlaythrough(Tutorial tutorial) {
        //     if (mainTutorialTypes.Contains(tutorial)) {
        //         if (SettingsManager.Instance.settings.skipTutorials) {
        //             //if tutorials are skipped then when this is used always return true
        //             return true;
        //         }
        //     } else if (bonusTutorialTypes.Contains(tutorial)) {
        //         if (SettingsManager.Instance.settings.skipAdvancedTutorials) {
        //             //if advanced tutorials are skipped then when this is used always return true
        //             return true;
        //         }
        //     }
        //     return _completedImportantTutorials.Contains(tutorial);
        // }
        public bool IsTutorialCurrentlyActive(Tutorial tutorial) {
            return _instantiatedTutorials.Any(t => t.tutorialType == tutorial && t.isActivated);
        }
        private bool IsBonusTutorial(TutorialQuest tutorialQuest) {
            return tutorialQuest is BonusTutorial;
        }
        public int GetAllActiveTutorialsCount() {
            return _activeBonusTutorials.Count + _activeImportantTutorials.Count + _waitingImportantTutorials.Count;
        }
        #endregion

        #region Completion
        public void CompleteTutorialQuest(TutorialQuest tutorial) {
            if (tutorial is ImportantTutorial) {
                _completedImportantTutorials.Add(tutorial.tutorialType);
            } else {
                SaveManager.Instance.currentSaveDataPlayer.AddBonusTutorialAsCompleted(tutorial.tutorialType);    
            }
            Messenger.Broadcast(PlayerQuestSignals.TUTORIAL_QUEST_COMPLETED, tutorial);
            DeactivateTutorial(tutorial);
            if (IsBonusTutorial(tutorial) == false) {
                CheckIfAllTutorialsCompleted();    
            }
        }
        private void CheckIfAllTutorialsCompleted() {
            if (_instantiatedTutorials.Count == 0 || _instantiatedTutorials.Count(x => IsBonusTutorial(x) == false) == 0) {
                PlayerUI.Instance.ShowGeneralConfirmation("Finished Tutorial",
                    "You're done with the Main Tutorial. Killing Villagers may be quite easy if you simply use spells. " +
                    "The real fun is when you get creative with it! Try to start a Zombie Apocalypse, or let the " +
                    "Villagers fight amongst each other by making them do various crimes, " +
                    "or figure out how to turn someone into a Necromancer. Good luck!");    
                hasCompletedImportantTutorials = true;
                Messenger.Broadcast(PlayerQuestSignals.FINISHED_IMPORTANT_TUTORIALS);
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
        public void ActivateTutorial(LogQuest logQuest) {
            _activeBonusTutorials.Add(logQuest);
            logQuest.Activate();
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

        #region Tutorial Data
        private Dictionary<Tutorial_Type, TutorialScriptableObjectData> _loadedTutorialData;
        public TutorialScriptableObjectData GetTutorialData(Tutorial_Type p_type) {
            if (_loadedTutorialData.ContainsKey(p_type)) {
                return _loadedTutorialData[p_type];
            }
            TutorialScriptableObjectData loadedData = Resources.Load<TutorialScriptableObjectData>($"Tutorial Data/{p_type.ToString()}");
            _loadedTutorialData.Add(p_type, loadedData);
            return loadedData;
        }
        public void UnloadTutorialAssets() {
            foreach (var kvp in _loadedTutorialData) {
                for (int i = 0; i < kvp.Value.pages.Count; i++) {
                    TutorialPage page = kvp.Value.pages[i];
                    Resources.UnloadAsset(page.imgTutorial);       
                }
            }
        }
        public void UnlockTutorial(Tutorial_Type p_type) {
            SaveManager.Instance.currentSaveDataPlayer.UnlockTutorial(p_type);
            SaveManager.Instance.savePlayerManager.SavePlayerData();
        }
        #endregion

        // #region For Testing
        // public void ResetTutorials() {
        //     List<Tutorial> completedTutorials = new List<Tutorial>(SaveManager.Instance.currentSaveDataPlayer.completedTutorials);
        //     SaveManager.Instance.currentSaveDataPlayer.ResetTutorialProgress();
        //     //respawn previously completed tutorials
        //     Tutorial[] allTutorials = CollectionUtilities.GetEnumValues<Tutorial>();
        //     for (int i = 0; i < allTutorials.Length; i++) {
        //         Tutorial tutorial = allTutorials[i];
        //         if (completedTutorials.Contains(tutorial)) {
        //            InstantiateTutorial(tutorial);
        //         }
        //     }
        // }
        // #endregion


    }
}