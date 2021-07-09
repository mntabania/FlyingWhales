using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class Golem : Summon {
    public override string raceClassName => "Golem";
    public Golem() : base(SUMMON_TYPE.Golem, "Golem", RACE.GOLEM, UtilityScripts.Utilities.GetRandomGender()) {
        //SetMaxHPMod(1000);
        visuals.SetHasBlood(false);
        traitContainer.AddTrait(this, "Indestructible");
        traitContainer.AddTrait(this, "Hibernating");
    }
    public Golem(string className) : base(SUMMON_TYPE.Golem, className, RACE.GOLEM, UtilityScripts.Utilities.GetRandomGender()) {
        //SetMaxHPMod(1000);
        visuals.SetHasBlood(false);
        traitContainer.AddTrait(this, "Indestructible");
        traitContainer.AddTrait(this, "Hibernating");
    }
    public Golem(SaveDataSummon data) : base(data) {
        visuals.SetHasBlood(false);
    }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Golem_Behaviour);
    }
    public override void OnSummonAsPlayerMonster() {
        base.OnSummonAsPlayerMonster();
        traitContainer.RemoveTrait(this, "Indestructible");
        traitContainer.RemoveTrait(this, "Hibernating");
    }
    public override void SubscribeToSignals() {
        if (hasSubscribedToSignals) {
            return;
        }
        base.SubscribeToSignals();
        Messenger.AddListener<Character, Area>(CharacterSignals.CHARACTER_EXITED_AREA, OnCharacterExitedArea);
        Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    }
    public override void UnsubscribeSignals() {
        if (!hasSubscribedToSignals) {
            return;
        }
        base.UnsubscribeSignals();
        Messenger.RemoveListener<Character, Area>(CharacterSignals.CHARACTER_EXITED_AREA, OnCharacterExitedArea);
        Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
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
    protected override void OnTickEnded() {
        base.OnTickEnded();
        PerTickOutsideCombatHPRecovery();
    }
    #endregion

    private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
        if (character != this && combatComponent.isInCombat && homeStructure != null) {
            if (structure != homeStructure) {
                combatComponent.RemoveHostileInRange(character);
            }
        }
    }

    private void OnCharacterExitedArea(Character character, Area p_area) {
        if (character != this && combatComponent.isInCombat) {
            if (HasTerritory()) {
                if (IsTerritory(p_area)) {
                    bool isCharacterInStillInTerritory = character.IsInTerritoryOf(this);
                    if (!isCharacterInStillInTerritory) {
                        combatComponent.RemoveHostileInRange(character);
                    }
                }
            }
        }
    }
}