using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using UtilityScripts;
namespace Tutorial {
    public class TutorialManager : MonoBehaviour {

        public static TutorialManager Instance;
        public const string CompletedTutorialsKey = "Completed_Tutorials";
        public enum Tutorial { 
            Basic_Controls, 
            Build_A_Kennel, 
            Defend_A_Structure, 
            Cast_Meteor, 
            Character_Info,
            Harass_A_Village,
            Regional_Map,
            Trigger_Poison_Explosion,
            Survive_Counterattack,
            Share_An_Intel,
            Apply_An_Affliction,
            Win_Condition
        }

        private List<TutorialQuest> _activeTutorials;
        private List<TutorialQuest> _waitingTutorials;
        private List<TutorialQuest> _instantiatedTutorials;
        
        //UI
        public TutorialUI tutorialUI;
        
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

        #region Monobehaviours
        private void Awake() {
            Instance = this;
        }
        private void LateUpdate() {
            if (GameManager.Instance.gameHasStarted) {
                CheckIfNewTutorialCanBeActivated();    
            }
        }
        private void Update() {
            if (GameManager.Instance.gameHasStarted) {
                for (int i = 0; i < _instantiatedTutorials.Count; i++) {
                    TutorialQuest tutorialQuest = _instantiatedTutorials[i];
                    tutorialQuest.PerFrameActions();
                }
            }
        }
        #endregion

        #region Initialization
        public void Initialize() {
            _activeTutorials = new List<TutorialQuest>();
            _waitingTutorials = new List<TutorialQuest>();
            _instantiatedTutorials = new List<TutorialQuest>();
            
            tutorialUI.Initialize();
            
            //Create instances for all uncompleted tutorials.
            string[] completedTutorials = PlayerPrefsX.GetStringArray(CompletedTutorialsKey);
            Tutorial[] allTutorials = CollectionUtilities.GetEnumValues<Tutorial>();
            for (int i = 0; i < allTutorials.Length; i++) {
                Tutorial tutorial = allTutorials[i];
                bool instantiateTutorial = completedTutorials.Contains(tutorial.ToString()) == false;
                if (WorldConfigManager.Instance.isDemoWorld && instantiateTutorial) {
                    //if is demo world, check if tutorial should be enabled in demo
                    instantiateTutorial = WorldConfigManager.Instance.demoTutorials.Contains(tutorial);
                }
                if (instantiateTutorial) {
                    TutorialQuest tutorialQuest = InstantiateTutorial(tutorial);
                    tutorialQuest.WaitForAvailability();
                    _instantiatedTutorials.Add(tutorialQuest);
                }
            }
        }
        public TutorialQuest InstantiateTutorial(Tutorial tutorial) {
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
            string[] completedTutorials = PlayerPrefsX.GetStringArray(CompletedTutorialsKey);
            return completedTutorials.Contains(tutorial.ToString());
        }
        #endregion

        #region Completion
        public void CompleteTutorialQuest(TutorialQuest tutorial) {
            List<string> completedTutorials = PlayerPrefsX.GetStringArray(CompletedTutorialsKey).ToList();
            completedTutorials.Add(tutorial.tutorialType.ToString());
            PlayerPrefsX.SetStringArray(CompletedTutorialsKey, completedTutorials.ToArray());
            DeactivateTutorial(tutorial);
            Messenger.Broadcast(Signals.TUTORIAL_QUEST_COMPLETED, tutorial);
        }
        #endregion

        #region Availability
        public void AddTutorialToWaitList(TutorialQuest tutorialQuest) {
            _waitingTutorials.Add(tutorialQuest);
            _waitingTutorials = _waitingTutorials.OrderByDescending(q => q.priority).ToList();
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
            if (_waitingTutorials.Count > 0 && _activeTutorials.Count < 3) {
                //new tutorial can be shown.
                //check number of tutorials that can be shown. 3 at maximum
                int missingTutorials = 3 - _activeTutorials.Count;
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
            tutorialQuest.Activate();
            RemoveTutorialFromWaitList(tutorialQuest);
            TutorialQuestItem tutorialQuestItem = tutorialUI.ShowTutorialQuest(tutorialQuest);
            tutorialQuest.SetTutorialQuestItem(tutorialQuestItem);
        }
        private void DeactivateTutorial(TutorialQuest tutorialQuest) {
            _activeTutorials.Remove(tutorialQuest);
            _waitingTutorials.Remove(tutorialQuest); //this is for cases when a tutorial is in the waiting list, but has been deactivated.
            _instantiatedTutorials.Remove(tutorialQuest);
            if (tutorialQuest.tutorialQuestItem != null) {
                tutorialUI.HideTutorialQuest(tutorialQuest);
            }
            tutorialQuest.Deactivate();
        }
        public bool HasActiveTutorial() {
            return _activeTutorials.Count > 0;
        }
        #endregion

        #region For Testing
        public void ResetTutorials() {
            string[] completedTutorials = PlayerPrefsX.GetStringArray(CompletedTutorialsKey);
            PlayerPrefs.DeleteKey(CompletedTutorialsKey);
            //respawn previously completed tutorials
            Tutorial[] allTutorials = CollectionUtilities.GetEnumValues<Tutorial>();
            for (int i = 0; i < allTutorials.Length; i++) {
                Tutorial tutorial = allTutorials[i];
                if (completedTutorials.Contains(tutorial.ToString())) {
                    TutorialQuest tutorialQuest = InstantiateTutorial(tutorial);
                    tutorialQuest.WaitForAvailability();
                    _instantiatedTutorials.Add(tutorialQuest);
                }
            }
        }
        #endregion

    }
}