using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
public class Golem : Summon {
    public override string raceClassName => "Golem";
    public Golem() : base(SUMMON_TYPE.Golem, "Golem", RACE.GOLEM, UtilityScripts.Utilities.GetRandomGender()) {
        SetMaxHPMod(1000);
        visuals.SetHasBlood(false);
    }
    public Golem(string className) : base(SUMMON_TYPE.Golem, className, RACE.GOLEM, UtilityScripts.Utilities.GetRandomGender()) {
        SetMaxHPMod(1000);
        visuals.SetHasBlood(false);
    }
    public Golem(SaveDataCharacter data) : base(data) { }

    #region Overrides
    public override string GetClassForRole(CharacterRole role) {
        return "Barbarian"; //all golems are barbarians
    }
    //public override void OnPlaceSummon(LocationGridTile tile) {
    //    base.OnPlaceSummon(tile);
    //    //CharacterState state = stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, tile.parentAreaMap.npcSettlement);
    //    //state.SetIsUnending(true);
    //    GoToWorkArea();
    //}
    //protected override void IdlePlans() {
    //    base.IdlePlans();
    //    //CharacterState state = stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, specificLocation);
    //    //state.SetIsUnending(true);
    //    GoToWorkArea();
    //}
    //protected override void OnSeenBy(Character character) {
    //    base.OnSeenBy(character);
    //    if (traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
    //        return;
    //    }
    //    if (!character.IsHostileWith(this)) {
    //        return;
    //    }
    //    //add taunted trait to the character
    //    character.traitContainer.AddTrait(character, new Taunted(), this);
    //}
    #endregion
}