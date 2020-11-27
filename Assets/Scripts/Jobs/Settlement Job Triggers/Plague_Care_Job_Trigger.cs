using System.Linq;
using Locations.Settlements.Components;
namespace Jobs {
    public class Plague_Care_Job_Trigger : SettlementJobTrigger, NPCSettlementEventDispatcher.ITimeListener {
        public override void HookTriggerToSettlement(NPCSettlement p_settlement) {
            p_settlement.npcSettlementEventDispatcher.SubscribeToHourStartedEvent(this);
        }
        public override void UnhookTriggerToSettlement(NPCSettlement p_settlement) {
            p_settlement.npcSettlementEventDispatcher.UnsubscribeToHourStartedEvent(this);
        }
        public void OnHourStarted(NPCSettlement p_settlement) {
            var hoursBasedOnTicks = GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick);
            if (hoursBasedOnTicks == 6 && !p_settlement.HasJob(JOB_TYPE.PLAGUE_CARE) && //6 
                p_settlement.residents.Count(r => r.traitContainer.HasTrait("Quarantined") && r.currentStructure.settlementLocation == p_settlement) >= 1) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PLAGUE_CARE, INTERACTION_TYPE.START_PLAGUE_CARE, null, p_settlement);
                p_settlement.AddToAvailableJobs(job);    
            }
        }
    }
}