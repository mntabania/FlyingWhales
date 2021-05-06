using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class BreedMonsterData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.BREED_MONSTER;
    public override string name => "Breed Monster";
    public override string description => $"This Action adds 1 Charge of the current monster to the player's Monsters List.";
    public BreedMonsterData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is Summon summon) {
            //PLAYER_SKILL_TYPE summonPlayerSkillType = PLAYER_SKILL_TYPE.NONE;
            //if (summon is Rat) {
            //    summonPlayerSkillType = PLAYER_SKILL_TYPE.PLAGUED_RAT;
            //} else {
            //    SummonPlayerSkill summonPlayerSkill = PlayerSkillManager.Instance.GetSummonPlayerSkillData(summon.race, summon.characterClass.className);
            //    summonPlayerSkillType = summonPlayerSkill.type;
            //}
            //PlayerManager.Instance.player.playerSkillComponent.AddCharges(summonPlayerSkillType, 1);

            GameObject effect = ObjectPoolManager.Instance.InstantiateObjectFromPool("Breed Effect",
                summon.worldPosition, Quaternion.identity, summon.currentRegion.innerMap.objectsParent, true);
            BreedEffect breedEffect = effect.GetComponent<BreedEffect>();
            breedEffect.PlayEffect(summon.marker.usedSprite);
            
            base.ActivateAbility(targetPOI);
        }
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = base.CanPerformAbilityTowards(targetCharacter);
        if (canPerform) {
            return (targetCharacter is Summon) && !targetCharacter.isDead && targetCharacter.gridTileLocation != null && 
                   targetCharacter.gridTileLocation.structure != null && targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.KENNEL;
        }
        return false;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if (target is Summon targetCharacter) {
            bool isValid = base.IsValid(target);
            return isValid && targetCharacter.gridTileLocation != null && targetCharacter.gridTileLocation.structure != null && 
                   targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.KENNEL && !(targetCharacter is Dragon) && 
                   PlayerSkillManager.Instance.GetSummonPlayerSkillData(targetCharacter.race, targetCharacter.characterClass.className) != null;
        }
        return false;
    }
    #endregion
}