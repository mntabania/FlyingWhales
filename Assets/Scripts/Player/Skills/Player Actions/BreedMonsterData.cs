﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class BreedMonsterData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.BREED_MONSTER;
    public override string name { get { return "Breed Monster"; } }
    public override string description { get { return "This Action adds 1 Charge of the current monster to the player's Minion List."; } }

    public BreedMonsterData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is Summon summon) {
            SummonPlayerSkill summonPlayerSkill = PlayerSkillManager.Instance.GetSummonPlayerSkillData(summon.race, summon.characterClass.className);
            PlayerManager.Instance.player.playerSkillComponent.AddCharges(summonPlayerSkill.type, 1);
            base.ActivateAbility(targetPOI);
        }
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            return (targetCharacter is Summon) && targetCharacter.gridTileLocation != null && targetCharacter.gridTileLocation.structure != null && targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.THE_KENNEL;
        }
        return canPerform;
    }
    #endregion
}