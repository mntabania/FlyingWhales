using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public class FearSpellAbility : CombatAbility {

    private int _fearDurationInMinutes;
    public FearSpellAbility() : base(COMBAT_ABILITY.FEAR_SPELL) {
        abilityTags.Add(ABILITY_TAG.MAGIC);
        abilityTags.Add(ABILITY_TAG.DEBUFF);
        cooldown = 10;
        _currentCooldown = 10;
    }

    #region Overrides
    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            Character character = targetPOI as Character;
            if (!character.faction.isPlayerFaction) {
                return true;
            }
        }
        return false;
    }
    protected override void OnLevelUp() {
        base.OnLevelUp();
        if (lvl == 1) {
            _fearDurationInMinutes = 30;
        } else if (lvl == 2) {
            _fearDurationInMinutes = 60;
        } else if (lvl == 3) {
            _fearDurationInMinutes = 90;
        }
    }
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            Character character = targetPOI as Character;
            Spooked spooked = character.traitContainer.GetNormalTrait<Spooked>("Spooked");
            if(spooked == null) {
                //Spooked newTrait = new Spooked();
                //newTrait.OverrideDuration(GameManager.Instance.GetTicksBasedOnMinutes(_fearDurationInMinutes));
                character.traitContainer.AddTrait(character, "Spooked");
            }
        }
        base.ActivateAbility(targetPOI);
    }
    #endregion
}
