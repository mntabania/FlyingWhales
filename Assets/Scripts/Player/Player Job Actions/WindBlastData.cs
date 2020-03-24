using System.Collections.Generic;
using Inner_Maps;
using Traits;

public class WindBlastData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.WIND_BLAST;
    public override string name => "Wind Blast";
    public override string description => "Pushes movable characters and objects outwards and applies a moderate amount of Wind damage.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.DEVASTATION;
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public WindBlastData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        // List<LocationGridTile> tiles = targetTile.GetTilesInRadius(1, includeCenterTile: true, includeTilesInDifferentStructure: false);
        // //create generic tile object and destroy after 3 ticks.
        // targetTile.genericTileObject.GetOrCreateMapVisual();
        // SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(3),
        //     () => targetTile.genericTileObject.TryDestroyMapVisual(), this);
        
        // for (int i = 0; i < tiles.Count; i++) {
        //     LocationGridTile tile = tiles[i];
        //     tile.PerformActionOnTraitables((traitable) => SpookCharacter(traitable, targetTile));
        // }
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Wind_Blast);
    }
    private void SpookCharacter(ITraitable traitable, LocationGridTile targetTile) {
        if (traitable is Character character) {
            character.traitContainer.AddTrait(character, "Spooked");
            character.marker.AddPOIAsInVisionRange(targetTile.genericTileObject);
            character.combatComponent.Flight(targetTile.genericTileObject, "heard a terrifying howl");
        }
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        return targetTile.structure != null;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(1, tile);
    }
}