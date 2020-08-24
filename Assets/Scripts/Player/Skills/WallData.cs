using System.Collections.Generic;
using Inner_Maps;
using Traits;

public class WallData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.WALL;
    public override string name => "Wall";
    public override string description => "This Spell spawns a single tile of durable wall. Can be chained together to block someone's path. Wall slowly degrades and disappears after 5 hours.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;

    public WallData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        if(targetTile.objHere != null) {
            targetTile.structure.RemovePOI(targetTile.objHere);
        }
        BlockWall wall = InnerMapManager.Instance.CreateNewTileObject<BlockWall>(TILE_OBJECT_TYPE.BLOCK_WALL);
        wall.SetWallType(WALL_TYPE.Demon_Stone);
        GameDate expiryDate = GameManager.Instance.Today();
        expiryDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(1));
        wall.SetExpiry(expiryDate);
        targetTile.structure.AddPOI(wall, targetTile);
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile);
        if (canPerform) {
            return targetTile.structure != null/*&& targetTile.objHere == null*/;
        }
        return canPerform;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }    
}
