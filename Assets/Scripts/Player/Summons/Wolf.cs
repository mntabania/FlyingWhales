using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf : SkinnableAnimal {
	
	public override bool defaultDigMode => true;
    //public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Passive;
    public override string raceClassName => "Wolf";
    public Wolf() : base(SUMMON_TYPE.Wolf, "Ravager", RACE.WOLF, UtilityScripts.Utilities.GetRandomGender()) { }
    public Wolf(string className) : base(SUMMON_TYPE.Wolf, className, RACE.WOLF, UtilityScripts.Utilities.GetRandomGender()) { }
    public Wolf(SaveDataSkinnableAnimal data) : base(data) { }

    public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.WOLF_HIDE;

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
	    behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Ravager_Behaviour);
    }
    #endregion
}
