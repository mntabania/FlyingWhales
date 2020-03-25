using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class LightningData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.LIGHTNING;
    public override string name { get { return "Lightning"; } }
    public override string description { get { return "A lightning bolt will hit the designated spot, causing Electric damage."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SPELL; } }
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public LightningData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Lightning_Strike);
        List<IPointOfInterest> pois = targetTile.GetPOIsOnTile();
        for (int i = 0; i < pois.Count; i++) {
            pois[i].AdjustHP(-100, ELEMENTAL_TYPE.Electric, showHPBar: true);
        }
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
}
