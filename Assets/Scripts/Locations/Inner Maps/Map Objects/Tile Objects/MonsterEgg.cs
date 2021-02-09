using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class MonsterEgg : TileObject {
    public SUMMON_TYPE summonType { get; protected set; }
    public Character characterThatLay { get; protected set; }
    public GameDate hatchDate { get; private set; }
    public bool isSupposedToHatch { get; private set; }
    public bool hasHatched { get; private set; }
    public bool hasInitiated { get; private set; }

    public override System.Type serializedData => typeof(SaveDataMonsterEgg);

    protected MonsterEgg(TILE_OBJECT_TYPE tileObjectType, SUMMON_TYPE summonType, int hatchTime) {
        Initialize(tileObjectType, false);
        this.summonType = summonType;
        //NOTE: It is assumed that when a new instance of monster egg is created, it will be placed immediately
        //hence the immediate setting of hatch date. 
        hatchDate = GameManager.Instance.Today().AddTicks(hatchTime);
    }
    public MonsterEgg(SaveDataMonsterEgg data) : base(data) {
        //SaveDataMonsterEgg saveDataMonsterEgg  = data as SaveDataMonsterEgg;
        Assert.IsNotNull(data);
        summonType = data.summonType;

        //Only a temp fix so that the old save data of players can still be used
        //Remove this when we do not need this anym
        if (summonType == SUMMON_TYPE.None) {
            if (data is SaveDataSpiderEgg) {
                summonType = SUMMON_TYPE.Giant_Spider;
            } else if (data is SaveDataHarpyEgg) {
                summonType = SUMMON_TYPE.Harpy;
            }
        }
        hatchDate = data.hatchDate;
        hasHatched = data.hasHatched;
        isSupposedToHatch = data.isSupposedToHatch;
    }

    #region Loading
    public override void LoadSecondWave(SaveDataTileObject data) {
        base.LoadSecondWave(data);
        SaveDataMonsterEgg saveData = data as SaveDataMonsterEgg;
        Assert.IsNotNull(saveData);
        characterThatLay = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveData.characterThatLayID);
    }
    #endregion
    
    public void SetCharacterThatLay(Character character) {
        characterThatLay = character;
    }
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if (!hasHatched && isSupposedToHatch) {
            //if the egg was placed and it is supposed to hatch, based on schedule,
            //then hatch it after 1 tick from now. Checked this first instead of hasInitiated
            //because if egg came from load and the hatch date is past due, the egg will not
            //hatch until it is picked up then dropped again
            GameDate nextTick = GameManager.Instance.Today().AddTicks(1);
            SchedulingManager.Instance.AddEntry(nextTick, HatchProcess, this);
        } else if (!hasInitiated) {
            hasInitiated = true;
            SchedulingManager.Instance.AddEntry(hatchDate, HatchProcess, this);
        }
    }
    protected void HatchProcess() {
        if (!hasHatched) {
            isSupposedToHatch = true;
            if (!isBeingSeized) {
                if (isBeingCarriedBy != null) {
                    isBeingCarriedBy.UncarryPOI(this);
                }
                if (gridTileLocation != null) {
                    Hatch();
                    gridTileLocation.structure.RemovePOI(this);
                    hasHatched = true;
                }
            }
        }
    }
    protected virtual void Hatch() {
        if(characterThatLay != null) {
            Summon monster = CharacterManager.Instance.CreateNewSummon(summonType, faction: characterThatLay.faction, homeLocation: characterThatLay.homeSettlement, homeRegion: characterThatLay.homeRegion, homeStructure: characterThatLay.homeStructure, bypassIdeologyChecking: true);
            CharacterManager.Instance.PlaceSummonInitially(monster, gridTileLocation);
            if (!monster.HasHome()) {
                monster.ClearTerritory();
                monster.SetTerritory(gridTileLocation.parentArea);
            }
        }

    }

    #region Overrides
    public override string ToString() {
        return $"Monster Egg {id.ToString()}";
    }
    #endregion
}

public class SaveDataMonsterEgg : SaveDataTileObject {
    public SUMMON_TYPE summonType;
    public string characterThatLayID;
    public GameDate hatchDate;
    public bool isSupposedToHatch;
    public bool hasHatched;
    
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        MonsterEgg monsterEgg = tileObject as MonsterEgg;
        Assert.IsNotNull(monsterEgg);
        summonType = monsterEgg.summonType;
        characterThatLayID = monsterEgg.characterThatLay.persistentID;
        hatchDate = monsterEgg.hatchDate;
        isSupposedToHatch = monsterEgg.isSupposedToHatch;
        hasHatched = monsterEgg.hasHatched;
    }
}