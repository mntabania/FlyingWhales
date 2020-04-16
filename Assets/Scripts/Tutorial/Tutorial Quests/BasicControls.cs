using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
namespace Tutorial {
    public class BasicControls : TutorialQuest {

        public BasicControls() : base("Basic Controls", TutorialManager.Tutorial.Basic_Controls) { }
        public override void WaitForAvailability() {
            TutorialManager.Instance.StartCoroutine(WaitForSeconds());
        }
        public override void ConstructSteps() {
            TutorialQuestStep look = new LookAroundStep(tooltip: "Use WASD/Arrow keys/Middle Mouse drag to control the camera.");
            TutorialQuestStep unpause = new UnpauseStep(tooltip: "Unpause the game by pressing the pause button or the Space Bar");
            TutorialQuestStep objectClick = new ClickOnObjectStep();
            TutorialQuestStep characterClick = new ClickOnCharacterStep();
            TutorialQuestStep structureClick = new ClickOnStructureStep();
            TutorialQuestStep hexTileClick = new ClickOnAreaStep();

            steps = new List<TutorialQuestStep>() {
                look, unpause, objectClick, characterClick, structureClick, hexTileClick
            };
        }

        private IEnumerator WaitForSeconds() {
            yield return new WaitForSecondsRealtime(8);
            MakeAvailable();
        }
    }
}