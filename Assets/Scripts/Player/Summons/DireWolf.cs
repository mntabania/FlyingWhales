using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DireWolf : Summon {
	
	public override bool defaultDigMode => true;
    public override string raceClassName => "Dire Wolf";
    public DireWolf() : base(SUMMON_TYPE.Dire_Wolf, "Dire", RACE.WOLF, UtilityScripts.Utilities.GetRandomGender()) { }
    public DireWolf(string className) : base(SUMMON_TYPE.Dire_Wolf, className, RACE.WOLF, UtilityScripts.Utilities.GetRandomGender()) { }
    public DireWolf(SaveDataSummon data) : base(data) { }

    #region Overrides
    //public override void OnPlaceSummon(LocationGridTile tile) {
    //    base.OnPlaceSummon(tile);
    //    //CharacterState state = stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, tile.parentAreaMap.npcSettlement);
    //    //state.SetIsUnending(true);
    //    //Messenger.AddListener(Signals.TICK_STARTED, PerTickGoapPlanGeneration);
    //    GoToWorkArea();
    //}
    //protected override void IdlePlans() {
    //    base.IdlePlans();
    //    //CharacterState state = stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, specificLocation);
    //    //state.SetIsUnending(true);
    //    GoToWorkArea();
    //}
    public override void Initialize() {
	    base.Initialize();
	    movementComponent.SetEnableDigging(true);
	    behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Dire_Wolf_Behaviour);
    }
    #endregion
}