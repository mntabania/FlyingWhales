using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UtilityScripts;
public class ProtectionData : SkillData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.PROTECTION;
    public override string name => "Protection";
    public override string description => "This Spell will apply Protection to all non-hostile units within its area of effect - significantly reducing damage they receive.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

    public ProtectionData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }
    public override void ActivateAbility(LocationGridTile targetTile) {
        List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
        targetTile.PopulateTilesInRadius(tiles, 1, includeCenterTile: true, includeTilesInDifferentStructure: true);
        GameManager.Instance.CreateParticleEffectAtWithScale(targetTile, PARTICLE_EFFECT.Protection, 4f);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            for (int j = 0; j < tile.charactersHere.Count; j++) {
                Character character = tile.charactersHere[j];
                if (tile.tileObjectComponent.objHere is Tombstone tombstone && tombstone.character == character) {
                    //NOTE: Skip characters in tombstone when damaging character's here. //TODO: This is a quick fix
                    continue;
                }
                if (character.isNotHostileWithPlayer) {
                    character.traitContainer.AddTrait(character, "Protection");
                }
            }
        }
        RuinarchListPool<LocationGridTile>.Release(tiles);

        //TODO: Create Particle Effect
        //GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Water_Bomb);
        base.ActivateAbility(targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
        if (canPerform) {
            return targetTile.structure != null;
        }
        return canPerform;
    }
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(1, tile);
    }
}

