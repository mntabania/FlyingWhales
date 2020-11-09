using Tutorial;
namespace Quests {
    public class ChaosOrbSpawned : QuestCriteria {

        public override void Enable() {
            Messenger.AddListener(PlayerSignals.CHAOS_ORB_SPAWNED, SetCriteriaAsMet);
            Messenger.AddListener(PlayerSignals.CHAOS_ORB_DESPAWNED, CheckIfUnMet);
        }
        public override void Disable() {
            Messenger.RemoveListener(PlayerSignals.CHAOS_ORB_SPAWNED, SetCriteriaAsMet);
            Messenger.RemoveListener(PlayerSignals.CHAOS_ORB_DESPAWNED, CheckIfUnMet);
        }

        private void CheckIfUnMet() {
            if (PlayerManager.Instance.availableChaosOrbs.Count == 0) {
                //no more available chaos orbs.
                SetCriteriaAsUnMet();
            }
        }
    }
}