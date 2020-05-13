using System.Collections.Generic;
namespace Tutorial {
    public class Threat : PopupTutorial {
        public Threat() : base("Threat", TutorialManager.Tutorial.Threat) { }
        protected override void ConstructCriteria() {
            _activationCriteria = new List<TutorialQuestCriteria>(
                new [] {
                    new ThreatIncreased(), 
                }    
            );
        }
        public override void Activate() {
            base.Activate();
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Threat", 
                "Careful, your Threat Level has increased! Many of the things you do generate Threat. " +
                "Some generate a large fixed amount once you perform it, while some generate small amounts each hour afterwards." +
                "Something interesting will happen once your Threat Level has reached 100. Be ready!", 
                TutorialManager.Instance.threatPicture);
            CompleteQuest();
        }
    }
}