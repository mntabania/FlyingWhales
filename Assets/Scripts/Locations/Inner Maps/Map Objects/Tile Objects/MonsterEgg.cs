using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class MonsterEgg : TileObject {
    public SUMMON_TYPE summonType { get; protected set; }
    public int hatchTime { get; protected set; }

    private bool hasInitiated;
    private bool isSupposedToHatch;
    private bool hasHatched;

    protected MonsterEgg(TILE_OBJECT_TYPE tileObjectType, SUMMON_TYPE summonType, int hatchTime) {
        Initialize(tileObjectType, false);
        this.summonType = summonType;
        this.hatchTime = hatchTime;
    }

    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if (!hasInitiated) {
            hasInitiated = true;
            GameDate hatchDate = GameManager.Instance.Today();
            hatchDate.AddTicks(hatchTime);
            SchedulingManager.Instance.AddEntry(hatchDate, Hatch, this);
        } else {
            if (!hasHatched && isSupposedToHatch) {
                GameDate hatchDate = GameManager.Instance.Today();
                hatchDate.AddTicks(1);
                SchedulingManager.Instance.AddEntry(hatchDate, Hatch, this);
            }
        }
    }

    protected void Hatch() {
        if (!hasHatched) {
            isSupposedToHatch = true;
            if (!isBeingSeized) {
                if (isBeingCarriedBy != null) {
                    isBeingCarriedBy.UncarryPOI(this);
                }
                if (gridTileLocation != null) {
                    Character monster = CharacterManager.Instance.CreateNewSummon(summonType, PlayerManager.Instance.player.playerFaction, homeRegion: gridTileLocation.parentMap.region);
                    monster.CreateMarker();
                    monster.InitialCharacterPlacement(gridTileLocation, true);
                    if (gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                        monster.ClearTerritory();
                        monster.AddTerritory(gridTileLocation.collectionOwner.partOfHextile.hexTileOwner);
                    }
                    gridTileLocation.structure.RemovePOI(this);
                    hasHatched = true;
                }
            }
        }
    }

    #region Overrides
    public override string ToString() {
        return $"Monster Egg {id.ToString()}";
    }
    #endregion
}
