using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;

public class ThinWall : TileObject {
    //public string name { get; }
    //public int maxHP { get; private set; }
    //public int currentHP { get; private set; }
    public RESOURCE madeOf { get; private set; }
    //public ITraitContainer traitContainer { get; private set; }
    //public LocationGridTile gridTileLocation { get; private set; }
    //public ProjectileReceiver projectileReceiver => _visual.visionTrigger.projectileReceiver;
    //public TraitProcessor traitProcessor => TraitManager.defaultTraitProcessor;
    //public Transform worldObject => _visual.transform;
    public override MapObjectVisual<TileObject> mapVisual => _visual;
    private ThinWallGameObject _visual;
    //public List<INTERACTION_TYPE> advertisedActions => null;

    //public ThinWall(LocationStructure structure, WallVisual visual, RESOURCE madeOf) {
    //    name = $"Wall of {structure}";
    //    _visual = visual;
    //    this.madeOf = madeOf;
    //    UpdateMaxHPBasedOnResource();
    //    currentHP = maxHP;
    //    CreateTraitContainer();
    //    traitContainer.AddTrait(this, "Flammable");
    //    visual.Initialize(this);
    //}
    public ThinWall() : base() {
    }
    public ThinWall(SaveDataTileObject data) : base(data) { }

    #region General
    public void InitializeThinWall() {
        Initialize(TILE_OBJECT_TYPE.THIN_WALL, false);
    }
    protected override void Initialize(TILE_OBJECT_TYPE tileObjectType, bool shouldAddCommonAdvertisements = true) {
        base.Initialize(tileObjectType, shouldAddCommonAdvertisements);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.DIG);
        UpdateMaxHPBasedOnResource();
        currentHP = maxHP;
        InitializeMapObject(this);
    }
    public void SetVisualGO(ThinWallGameObject p_visualGO) {
        _visual = p_visualGO;
    }
    public void SetResourceMadeOf(RESOURCE p_resource) {
        madeOf = p_resource;
    }
    //public bool CanBeDamaged() {
    //    return mapObjectState != MAP_OBJECT_STATE.UNBUILT;
    //}
    #endregion

    #region Events
    private void OnWallDestroyed() {
        gridTileLocation.SetTileType(LocationGridTile.Tile_Type.Empty);
        gridTileLocation.CreateSeamlessEdgesForSelfAndNeighbours();
        traitContainer.RemoveAllTraits(this);
        LocationAwarenessUtility.RemoveFromAwarenessList(this);
    }
    private void OnWallRepaired() {
        if(gridTileLocation != null) {
            LocationAwarenessUtility.AddToAwarenessList(this, gridTileLocation);
            ////Thin walls cannot co-exist with block walls, so if a thin wall is placed, all block walls must be destroyed
            //if (gridTileLocation.tileObjectComponent.objHere is BlockWall) {
            //    gridTileLocation.tileObjectComponent.objHere.AdjustHP(-gridTileLocation.tileObjectComponent.objHere.maxHP, ELEMENTAL_TYPE.Normal, true);
            //}
        }
    }
    #endregion

    #region HP
    private void UpdateMaxHPBasedOnResource() {
        switch (madeOf) {
            case RESOURCE.WOOD:
                maxHP = 250;
                break;
            case RESOURCE.STONE:
                maxHP = 500;
                break;
            case RESOURCE.METAL:
                maxHP = 800;
                break;
        }
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false,
        object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false) {
        bool isWallPreviouslyDestroyed = currentHP <= 0;
        if (isWallPreviouslyDestroyed && amount < 0) {
            return; //ignore
        }
        CombatManager.Instance.ModifyDamage(ref amount, elementalDamageType, piercingPower, this);
        if (amount < 0 && Mathf.Abs(amount) > currentHP) {
            //if the damage amount is greater than this object's hp, set the damage to this object's
            //hp instead, this is so that if this object contributes to a structure's hp, it will not deal the excess damage
            //to the structure
            amount = -currentHP;
        }
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        if (amount <= 0 && currentHP > 0) {
            //ELEMENTAL_TYPE elementalType = ELEMENTAL_TYPE.Normal;
            //if(source != null && source is Character) {
            //    elementalType = (source as Character).combatComponent.elementalDamage.type;
            //}
            // CombatManager.Instance.CreateHitEffectAt(this, elementalDamageType);
            Character responsibleCharacter = null;
            if (source != null && source is Character) {
                responsibleCharacter = source as Character;
            }
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, responsibleCharacter, elementalTraitProcessor, setAsPlayerSource: isPlayerSource);
        }

        if (amount < 0) {
            if (source is Character responsibleCharacter) {
                Messenger.Broadcast(StructureSignals.WALL_DAMAGED_BY, this, amount, responsibleCharacter, isPlayerSource);
            } else {
                Messenger.Broadcast(StructureSignals.WALL_DAMAGED, this, amount, isPlayerSource);
            }
        } else if (amount > 0) {
            Messenger.Broadcast(StructureSignals.WALL_REPAIRED, this, amount);
            if (isWallPreviouslyDestroyed) {
                OnWallRepaired();
            }
        }
        
        _visual.UpdateWallAssets(this);
        _visual.UpdateWallState(this);
        
        if (currentHP <= 0) {
            //wall has been destroyed
            OnWallDestroyed();
        } else if (currentHP >= maxHP) {
            gridTileLocation.SetTileType(LocationGridTile.Tile_Type.Wall);
            gridTileLocation.CreateSeamlessEdgesForSelfAndNeighbours();
        }
    }
    //public void OnHitByAttackFrom(Character characterThatAttacked, CombatState combatStateOfAttacker, ref string attackSummary) {
    //    if (characterThatAttacked == null) {
    //        return;
    //    }
    //    //GameManager.Instance.CreateHitEffectAt(this, characterThatAttacked.combatComponent.elementalDamage.type);
    //    AdjustHP(-characterThatAttacked.combatComponent.attack, characterThatAttacked.combatComponent.elementalDamage.type, source: characterThatAttacked, showHPBar: true);
    //}
    #endregion

    #region Resource
    internal void ChangeResourceMadeOf(RESOURCE madeOf) {
        this.madeOf = madeOf;
        _visual.UpdateWallAssets(this);
        UpdateMaxHPBasedOnResource();
        switch (madeOf) {
            case RESOURCE.WOOD:
                traitContainer.AddTrait(this, "Flammable");
                break;
            case RESOURCE.STONE:
            case RESOURCE.METAL:
                traitContainer.RemoveTrait(this, "Flammable");
                break;
        }
    }
    //public void CreateTraitContainer() {
    //    traitContainer = new TraitContainer();
    //}
    #endregion

    #region NPCSettlement Map Object
    //protected override void CreateMapObjectVisual() {
    //    mapVisual = _visual;
    //}
    protected override void OnMapObjectStateChanged() { }
    //public void SetGridTileLocation(LocationGridTile tile) {
    //    gridTileLocation = tile;
    //}
    public override void InitializeMapObject(TileObject obj) {
        visionTrigger.Initialize(obj);
        visionTrigger.gameObject.SetActive(true);
        _visual.UpdateWallAssets(obj as ThinWall);
        if (mapVisual.selectable is TileObject tileObject) {
            Assert.IsNotNull(tileObject.traitContainer, $"Trait Container of {tileObject.name} {tileObject.id.ToString()} {tileObject.tileObjectType.ToString()} is null!");
            List<Trait> traitOverrideFunctions = tileObject.traitContainer.GetTraitOverrideFunctions(TraitManager.Initiate_Map_Visual_Trait);
            if (traitOverrideFunctions != null) {
                for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                    Trait trait = traitOverrideFunctions[i];
                    trait.OnInitiateMapObjectVisual(tileObject);
                }
            }
        }
    }
    #endregion

    //#region ITraitable
    //public void AddAdvertisedAction(INTERACTION_TYPE actionType, bool allowDuplicates = false) {
    //    //Not Applicable
    //}
    //public void RemoveAdvertisedAction(INTERACTION_TYPE actionType) {
    //    //Not Applicable
    //}
    //#endregion

    #region Loading
    public void LoadDataFromSave(SaveDataTileObject saveDataStructureWallObject) {
        currentHP = saveDataStructureWallObject.currentHP;
        _visual.UpdateWallAssets(this);
        _visual.UpdateWallState(this);
    }
    #endregion
}

//public class SaveDataThinWall : SaveDataTileObject {
//    public RESOURCE madeOf;
//}