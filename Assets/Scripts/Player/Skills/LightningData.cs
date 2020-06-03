﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Scriptable_Object_Scripts;
using Traits;
using UtilityScripts;

public class LightningData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.LIGHTNING;
    public override string name => "Lightning";
    public override string description => "This Spell triggers a lightning strike at the target spot. Deals major Electric damage.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public LightningData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        AudioManager.Instance.CreateAudioObject(
            CollectionUtilities.GetRandomElement(PlayerSkillManager.Instance.GetPlayerSkillAsset<LightningAssets>(SPELL_TYPE.LIGHTNING).thunderAudioClips), 
            targetTile, 1, false
        );
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Lightning_Strike);
        // List<IPointOfInterest> pois = targetTile.GetPOIsOnTile();
        // for (int i = 0; i < pois.Count; i++) {
        //     pois[i].AdjustHP(-350, ELEMENTAL_TYPE.Electric, showHPBar: true);
        // }
        targetTile.PerformActionOnTraitables(LightningDamage);
        targetTile.genericTileObject.traitContainer.AddTrait(targetTile.genericTileObject, "Danger Remnant");
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    private void LightningDamage(ITraitable traitable) {
        if (traitable is IPointOfInterest poi) {
            poi.AdjustHP(-500, ELEMENTAL_TYPE.Electric, triggerDeath: true, showHPBar: true);
        }
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
}
