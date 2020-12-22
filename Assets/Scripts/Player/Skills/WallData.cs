using System.Collections.Generic;
using Inner_Maps;
using Traits;

public class WallData : SpellData {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.WALL;
    public override string name => "Wall";
    public override string description => "This Spell spawns a single tile of durable wall. Can be chained together to block someone's path. Wall degrades and disappears after 5 hours.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

    public WallData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        if(targetTile.objHere != null && targetTile.objHere is TileObject tileObject && !tileObject.tileObjectType.IsTileObjectImportant()) {
            targetTile.structure.RemovePOI(targetTile.objHere);
        }
        BlockWall wall = InnerMapManager.Instance.CreateNewTileObject<BlockWall>(TILE_OBJECT_TYPE.BLOCK_WALL);
        wall.SetWallType(WALL_TYPE.Demon_Stone);
        GameDate expiryDate = GameManager.Instance.Today();
        expiryDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(5));
        wall.SetExpiry(expiryDate);
        targetTile.structure.AddPOI(wall, targetTile);
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile);
        if (canPerform) {
            if (targetTile.objHere is TileObject tileObject) {
                if (tileObject.tileObjectType.IsTileObjectImportant()) {
                    return false;    
                }
                if (tileObject.tileObjectType == TILE_OBJECT_TYPE.DOOR_TILE_OBJECT) {
                    return false;    
                }
                if (tileObject.traitContainer.HasTrait("Indestructible")) {
                    return false;
                }
            }
            return targetTile.structure != null && targetTile.structure.structureType != STRUCTURE_TYPE.OCEAN;
        }
        return false;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }    
}
