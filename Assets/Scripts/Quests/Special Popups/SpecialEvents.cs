using System.Collections.Generic;
using JetBrains.Annotations;
using Tutorial;
namespace Quests.Special_Popups {
    [UsedImplicitly]
    public class SpecialEvents : SpecialPopup {
        public SpecialEvents() : base("Special Events", QuestManager.Special_Popup.Special_Events) { }
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>(
                new QuestCriteria[] {
                    new HasFinishedImportantTutorials(), 
                    new IsAtTime(new []{ 
                        GameManager.Instance.GetTicksBasedOnHour(6), 
                        GameManager.Instance.GetTicksBasedOnHour(12),
                        GameManager.Instance.GetTicksBasedOnHour(18)
                    }), 
                }    
            );
            Messenger.AddListener<Character>(Signals.NECROMANCER_SPAWNED, OnNecromancerSpawned);
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetAllCriteria = base.HasMetAllCriteria();
            if (hasMetAllCriteria) {
                //No necromancers have been spawned yet
                return GameManager.Instance.Today().day >= 3 && CharacterManager.Instance.hasSpawnedNecromancerOnce == false;
            }
            return false;
        }
        
        private void OnNecromancerSpawned(Character necromancer) {
            CompleteQuest();
        }
        public override void Activate() {
            StopCheckingCriteria();
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Special Events", 
                "As a procedural game, Ruinarch will eventually be full of special events that have various triggers. " +
                $"In this demo, you can turn an {UtilityScripts.Utilities.ColorizeAction("Evil")} or " +
                $"{UtilityScripts.Utilities.ColorizeAction("Treacherous")} Villager " +
                $"into a Necromancer if you manage to get him to pick up the {UtilityScripts.Utilities.ColorizeAction("Necronomicon")}. Try it!", 
                TutorialManager.Instance.necronomiconPicture);
            CompleteQuest();
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener<Character>(Signals.NECROMANCER_SPAWNED, OnNecromancerSpawned);
        }
    }
}