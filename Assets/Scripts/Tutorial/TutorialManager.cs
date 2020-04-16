using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;
namespace Tutorial {
    public class TutorialManager : MonoBehaviour {

        public static TutorialManager Instance;
        public enum Tutorial { Basic_Controls, Build_A_Kennel,
            Defend_A_Structure,
            Cast_Meteor,
            Character_Info
        }
        
        private List<TutorialQuest> _activeTutorials;
        public const string CompletedTutorialsKey = "Completed_Tutorials";

        private List<TutorialQuest> _waitingTutorials;
        
        //UI
        public TutorialUI tutorialUI;
        
        private void Awake() {
            Instance = this;
        }
        private void LateUpdate() {
            if (GameManager.Instance.gameHasStarted) {
                CheckIfNewTutorialCanBeActivated();    
            }
        }
        public void Initialize() {
            _activeTutorials = new List<TutorialQuest>();
            _waitingTutorials = new List<TutorialQuest>();
            
            tutorialUI.Initialize();
            
            //Create instances for all uncompleted tutorials.
            string[] completedTutorials = PlayerPrefsX.GetStringArray(CompletedTutorialsKey);
            Tutorial[] allTutorials = CollectionUtilities.GetEnumValues<Tutorial>();
            for (int i = 0; i < allTutorials.Length; i++) {
                Tutorial tutorial = allTutorials[i];
                if (completedTutorials.Contains(tutorial.ToString()) == false) {
                    TutorialQuest tutorialQuest = InstantiateTutorial(tutorial);
                    tutorialQuest.WaitForAvailability();
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
            if (tutorialQuest.tutorialQuestItem != null) {
                tutorialUI.HideTutorialQuest(tutorialQuest);
            }
            tutorialQuest.Deactivate();
        }
        #endregion

    }
}