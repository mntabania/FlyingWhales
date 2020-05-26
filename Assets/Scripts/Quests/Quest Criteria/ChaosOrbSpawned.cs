using Tutorial;
namespace Quests {
    public class ChaosOrbSpawned : QuestCriteria {

        public override void Enable() {
            Messenger.AddListener(Signals.CHAOS_ORB_SPAWNED, SetCriteriaAsMet);
            Messenger.AddListener(Signals.CHAOS_ORB_DESPAWNED, CheckIfUnMet);
        }
        public override void Disable() {
            Messenger.RemoveListener(Signals.CHAOS_ORB_SPAWNED, SetCriteriaAsMet);
            Messenger.RemoveListener(Signals.CHAOS_ORB_DESPAWNED, CheckIfUnMet);
        }

        private void CheckIfUnMet() {
            if (PlayerManager.Instance.availableChaosOrbs.Count == 0) {
                //no more available chaos orbs.
                SetCriteriaAsUnMet();
            }
        }
    }
}