using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
            Cast_Meteor = 3, 
            Character_Info = 4,
            Harass_A_Village = 5,
            Regional_Map = 6,
            Trigger_Poison_Explosion = 7,
            Share_An_Intel = 9,
            Apply_An_Affliction = 10,
            Torture_Chambers = 11,
            Threat = 12,
        }

        private List<TutorialQuest> _activeTutorials;
        private List<TutorialQuest> _waitingTutorials;
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

        #region Monobehaviours
        private void Awake() {
            Instance = this;
        }
        private void LateUpdate() {
            if (GameManager.Instance.gameHasStarted) {
                CheckIfNewTutorialCanBeActivated();    
            }
        }
        #endregion

        #region Initialization
        public void Initialize() {
            _activeTutorials = new List<TutorialQuest>();
            _waitingTutorials = new List<TutorialQuest>();
            _instantiatedTutorials = new List<TutorialQuest>();
            
            if (SaveManager.Instance.currentSaveDataPlayer.completedTutorials == null || alwaysResetTutorialsOnStart) {
                SaveManager.Instance.currentSaveDataPlayer.InitializeTutorialData();
            }
            if (WorldConfigManager.Instance.isDemoWorld == false) {
                InstantiatePendingTutorials();
            }
        }
        public void InstantiatePendingTutorials() {
            //Create instances for all uncompleted tutorials.
            List<Tutorial> completedTutorials = SaveManager.Instance.currentSaveDataPlayer.completedTutorials;
            Tutorial[] allTutorials = CollectionUtilities.GetEnumValues<Tutorial>();
            for (int i = 0; i < allTutorials.Length; i++) {
                Tutorial tutorial = allTutorials[i];
                bool instantiateTutorial = completedTutorials.Contains(tutorial) == false;
                if (WorldConfigManager.Instance.isDemoWorld && instantiateTutorial) {
                    //if is demo world, check if tutorial should be enabled in demo
                    instantiateTutorial = WorldConfigManager.Instance.demoTutorials.Contains(tutorial);
                }
                if (instantiateTutorial) {
                    TutorialQuest tutorialQuest = InstantiateTutorial(tutorial);
                    _instantiatedTutorials.Add(tutorialQuest);
                }
            }
        }
        private TutorialQuest InstantiateTutorial(Tutorial tutorial) {
            string noSpacesName = UtilityScripts.Utilities.RemoveAllWhiteSpace(UtilityScripts.Utilities.
                NormalizeStringUpperCaseFirstLettersNoSpace(tutorial.ToString()));
            string typeName = $"Tutorial.{ noSpacesName }";
            Type type = Type.GetType(typeName);
            if (type != null) {
                return Activator.CreateInstance(type) as TutorialQuest;    
            }
            throw new Exception($"Could not instantiate tutorial quest {noSpacesName}");
        }
        #endregion

        #region Inquiry
        public bool HasTutorialBeenCompleted(Tutorial tutorial) {
            return SaveManager.Instance.currentSaveDataPlayer.completedTutorials.Contains(tutorial);
        }
        #endregion

        #region Completion
        public void CompleteTutorialQuest(TutorialQuest tutorial) {
            SaveManager.Instance.currentSaveDataPlayer.AddTutorialAsCompleted(tutorial.tutorialType);
            DeactivateTutorial(tutorial);
            Messenger.Broadcast(Signals.TUTORIAL_QUEST_COMPLETED, tutorial);
        }
        #endregion

        #region Availability
        public void AddTutorialToWaitList(TutorialQuest tutorialQuest) {
            _waitingTutorials.Add(tutorialQuest);
            _waitingTutorials = _waitingTutorials.OrderBy(q => q.priority).ToList();
        }
        public void RemoveTutorialFromWaitList(TutorialQuest tutorialQuest) {
            _waitingTutorials.Remove(tutorialQuest);
        }
        public bool IsInWaitList(TutorialQuest tutorialQuest) {
            return _waitingTutorials.Contains(tutorialQuest);
        }
        #endregion

        #region Presentation
        private void CheckIfNewTutorialCanBeActivated() {
            if (_waitingTutorials.Count > 0 && _activeTutorials.Count < MaxActiveTutorials) {
                //new tutorial can be shown.
                //check number of tutorials that can be shown. 3 at maximum
                int missingTutorials = MaxActiveTutorials - _activeTutorials.Count;
                if (missingTutorials > _waitingTutorials.Count) {
                    //if number of missing tutorials is greater than the available tutorials, then just show the available ones.
                    missingTutorials = _waitingTutorials.Count;
                }
                for (int i = 0; i < missingTutorials; i++) {
                    //get first tutorial in list, since tutorials are sorted by priority beforehand.
                    TutorialQuest availableTutorial = _waitingTutorials[0];
                    ActivateTutorial(availableTutorial);        
                }
            }
        }
        private void ActivateTutorial(TutorialQuest tutorialQuest) {
            _activeTutorials.Add(tutorialQuest);
            RemoveTutorialFromWaitList(tutorialQuest);
            tutorialQuest.Activate();
            QuestItem questItem = UIManager.Instance.questUI.ShowQuest(tutorialQuest);
            tutorialQuest.SetTutorialQuestItem(questItem);
        }
        private void DeactivateTutorial(TutorialQuest tutorialQuest) {
            _activeTutorials.Remove(tutorialQuest);
            _waitingTutorials.Remove(tutorialQuest); //this is for cases when a tutorial is in the waiting list, but has been deactivated.
            _instantiatedTutorials.Remove(tutorialQuest);
            if (tutorialQuest.questItem != null) {
                UIManager.Instance.questUI.HideQuestDelayed(tutorialQuest);
            }
            tutorialQuest.Deactivate();
        }
        public bool HasActiveTutorial() {
            return _activeTutorials.Count > 0;
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
                    TutorialQuest tutorialQuest = InstantiateTutorial(tutorial);
                    _instantiatedTutorials.Add(tutorialQuest);
                }
            }
        }
        #endregion

    }
}