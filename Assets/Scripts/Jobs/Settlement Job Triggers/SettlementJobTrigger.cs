namespace Jobs {
    public abstract class SettlementJobTrigger {

        public abstract void HookTriggerToSettlement(NPCSettlement p_settlement);
        public abstract void UnhookTriggerToSettlement(NPCSettlement p_settlement);
    }
}