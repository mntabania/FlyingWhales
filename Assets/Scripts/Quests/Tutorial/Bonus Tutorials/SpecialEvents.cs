using System.Collections.Generic;
using JetBrains.Annotations;
using Quests;
namespace Tutorial {
    [UsedImplicitly]
    public class SpecialEvents : BonusTutorial {
        public SpecialEvents() : base("Special Events", TutorialManager.Tutorial.Special_Events) { }
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
        protected override void MakeAvailable() {
            base.MakeAvailable();
            TutorialManager.Instance.ActivateTutorialButDoNotShow(this);
        }
        public override void Activate() {
            StopCheckingCriteria();
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Special Events", 
                "As a procedural game, Ruinarch will eventually be full of special " +
                "events that can be triggered by certain criteria. " +
                "As an example in this demo, you can turn an Evil or " +
                $"Treacherous {UtilityScripts.Utilities.VillagerIcon()}Villager into a " +
                "Necromancer if you manage to get him to pick up the Necronomicon. Can you do it?", 
                TutorialManager.Instance.necronomiconPicture);
            CompleteQuest();
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener<Character>(Signals.NECROMANCER_SPAWNED, OnNecromancerSpawned);
        }
    }
}