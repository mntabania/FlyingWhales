public class FireElementalBehaviour : BaseMonsterBehaviour {
	public FireElementalBehaviour() {
		priority = 8;
	}
	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
		return DefaultWildMonsterBehaviour(character, ref log, out producedJob);
	}
    protected override bool TamedBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob) {
	    if (TryTakeSettlementJob(p_character, ref p_log, out p_producedJob)) {
		    return true;
	    }
        return TriggerRoamAroundTerritory(p_character, ref p_log, out p_producedJob);
    }
}
