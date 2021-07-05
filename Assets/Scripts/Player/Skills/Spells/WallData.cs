using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;

public class WallData : SkillData {

    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.WALL;
    public override string name => "Wall";
    public override string description => "This Spell spawns a single tile of durable wall. Can be chained together to block someone's path.";
    public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

    public WallData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        TileObject tileObject = targetTile.tileObjectComponent.objHere;
        if (tileObject != null && !tileObject.tileObjectType.IsTileObjectImportant()) {
            targetTile.structure.RemovePOI(tileObject);
        }
        BlockWall wall = InnerMapManager.Instance.CreateNewTileObject<BlockWall>(TILE_OBJECT_TYPE.BLOCK_WALL);
        wall.SetWallType(WALL_TYPE.Demon_Stone);
        GameDate expiryDate = GameManager.Instance.Today();
        int processedTick = (PlayerSkillManager.Instance.GetDurationBonusPerLevel(PLAYER_SKILL_TYPE.WALL));
        expiryDate.AddTicks(processedTick);
        wall.SetExpiry(expiryDate);
        targetTile.structure.AddPOI(wall, targetTile);
        //IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
        if (canPerform) {
            if (targetTile.structure is DemonicStructure) {
                return false;
            }
            TileObject tileObject = targetTile.tileObjectComponent.objHere;
            if (tileObject != null) {
                if (tileObject.tileObjectType.IsTileObjectImportant()) {
                    return false;    
                }
                if (tileObject.tileObjectType.IsDemonicStructureTileObject()) {
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
    public override void ShowValidHighlight(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }    
}
