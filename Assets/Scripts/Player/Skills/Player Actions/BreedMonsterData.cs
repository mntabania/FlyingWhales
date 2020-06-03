using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class BreedMonsterData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.BREED_MONSTER;
    public override string name => "Breed Monster";
    public override string description => $"This Action adds 1 Charge of the current {UtilityScripts.Utilities.MonsterIcon()}monster to the player's Monsters List.";
    public BreedMonsterData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if(targetPOI is Summon summon) {
            SummonPlayerSkill summonPlayerSkill = PlayerSkillManager.Instance.GetSummonPlayerSkillData(summon.race, summon.characterClass.className);
            PlayerManager.Instance.player.playerSkillComponent.AddCharges(summonPlayerSkill.type, 1);

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
            return (targetCharacter is Summon) && targetCharacter.gridTileLocation != null && targetCharacter.gridTileLocation.structure != null && targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.THE_KENNEL;
        }
        return canPerform;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        if(target is Summon targetCharacter) {
            return (targetCharacter is Summon) && targetCharacter.gridTileLocation != null && targetCharacter.gridTileLocation.structure != null && targetCharacter.gridTileLocation.structure.structureType == STRUCTURE_TYPE.THE_KENNEL;
        }
        return false;
    }
    #endregion
}