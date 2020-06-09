using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;

public class StructureWallObject : MapObject<StructureWallObject>, ITraitable {
    public string name { get; }
    public int maxHP { get; private set; }
    public int currentHP { get; private set; }
    public RESOURCE madeOf { get; private set; }
    public ITraitContainer traitContainer { get; private set; }
    public LocationGridTile gridTileLocation { get; private set; }
    public ProjectileReceiver projectileReceiver => _visual.visionTrigger.projectileReceiver;
    public TraitProcessor traitProcessor => TraitManager.defaultTraitProcessor;
    public Transform worldObject => _visual.transform;
    public override MapObjectVisual<StructureWallObject> mapVisual => _visual;
    public BaseMapObjectVisual mapObjectVisual => mapVisual;
    private readonly WallVisual _visual;
    public List<INTERACTION_TYPE> advertisedActions => null;

    public StructureWallObject(LocationStructure structure, WallVisual visual, RESOURCE madeOf) {
        name = $"Wall of {structure}";
        _visual = visual;
        this.madeOf = madeOf;
        UpdateMaxHPBasedOnResource();
        currentHP = maxHP;
        CreateTraitContainer();
        traitContainer.AddTrait(this, "Flammable");
        visual.Initialize(this);
    }

    #region Events
    private void OnWallDestroyed() {
        gridTileLocation.SetTileType(LocationGridTile.Tile_Type.Empty);
        gridTileLocation.CreateSeamlessEdgesForSelfAndNeighbours();
        traitContainer.RemoveAllTraits(this);
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
    public void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false,
        object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false) {
        if (currentHP <= 0 && amount < 0) {
            return; //ignore
        }
        CombatManager.Instance.DamageModifierByElements(ref amount, elementalDamageType, this);
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
            CombatManager.ElementalTraitProcessor etp = elementalTraitProcessor ?? 
                                                        CombatManager.Instance.DefaultElementalTraitProcessor;
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, 
                responsibleCharacter, etp);
        }

        if (amount < 0) {
            Messenger.Broadcast(Signals.WALL_DAMAGED, this, amount);
        } else if (amount > 0) {
            Messenger.Broadcast(Signals.WALL_REPAIRED, this, amount);
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
    public void OnHitByAttackFrom(Character characterThatAttacked, CombatState state, ref string attackSummary) {
        //GameManager.Instance.CreateHitEffectAt(this, characterThatAttacked.combatComponent.elementalDamage.type);
        AdjustHP(-characterThatAttacked.combatComponent.attack, characterThatAttacked.combatComponent.elementalDamage.type, source: characterThatAttacked, showHPBar: true);
    }
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
    public void CreateTraitContainer() {
        traitContainer = new TraitContainer();
    }
    #endregion

    #region General
    public bool CanBeDamaged() {
        return mapObjectState != MAP_OBJECT_STATE.UNBUILT;
    }
    #endregion

    #region NPCSettlement Map Object
    protected override void CreateMapObjectVisual() {
        mapVisual = _visual;
    }
    protected override void OnMapObjectStateChanged() { }
    public void SetGridTileLocation(LocationGridTile tile) {
        gridTileLocation = tile;
    }
    #endregion

    #region ITraitable
    public void AddAdvertisedAction(INTERACTION_TYPE actionType, bool allowDuplicates = false) {
        //Not Applicable
    }
    public void RemoveAdvertisedAction(INTERACTION_TYPE actionType) {
        //Not Applicable
    }
    #endregion
}
