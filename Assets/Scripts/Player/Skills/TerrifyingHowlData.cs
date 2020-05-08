using System.Collections.Generic;
using Inner_Maps;
using Traits;

public class TerrifyingHowlData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.TERRIFYING_HOWL;
    public override string name => "Terrifying Howl";
    public override string description => "This Spell releases a bunch of screaming skulls. Their spine-tingling wails will cause all nearby characters to flee.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public TerrifyingHowlData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(1, includeCenterTile: true, includeTilesInDifferentStructure: false);
        //create generic tile object and destroy after 3 ticks.
        BaseMapObjectVisual visual = targetTile.genericTileObject.GetOrCreateMapVisual();
        visual.visionTrigger.VoteToMakeVisibleToCharacters();
        SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(3),
            () => targetTile.genericTileObject.TryDestroyMapVisual(), this);
        
        // for (int i = 0; i < tiles.Count; i++) {
        //     LocationGridTile tile = tiles[i];
        //     tile.PerformActionOnTraitables((traitable) => SpookCharacter(traitable, targetTile));
        // }
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Terrifying_Howl);
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    private void SpookCharacter(ITraitable traitable, LocationGridTile targetTile) {
        if (traitable is Character character) {
            character.traitContainer.AddTrait(character, "Spooked");
            character.marker.AddPOIAsInVisionRange(targetTile.genericTileObject);
            character.combatComponent.Flight(targetTile.genericTileObject, "heard a terrifying howl");
        }
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile);
        if (canPerform) {
            return targetTile.structure != null;
        }
        return canPerform;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(1, tile);
    }
}