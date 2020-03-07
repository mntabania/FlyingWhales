using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;

public class StructureWallObject : MapObject<StructureWallObject>, ITraitable {
    public string name { get; }
    public int maxHP { get; }
    public int currentHP { get; private set; }
    public RESOURCE madeOf { get; private set; }
    public ITraitContainer traitContainer { get; private set; }
    public LocationGridTile gridTileLocation { get; private set; }
    public ProjectileReceiver projectileReceiver => _visual.collisionTrigger.projectileReceiver;
    public TraitProcessor traitProcessor => TraitManager.defaultTraitProcessor;
    public Transform worldObject => _visual.transform;
    public override MapObjectVisual<StructureWallObject> mapVisual => _visual;
    public BaseMapObjectVisual mapObjectVisual => mapVisual;
    private readonly WallVisual _visual;

    public StructureWallObject(LocationStructure structure, WallVisual visual) {
        name = $"Wall of {structure}";
        _visual = visual;
        maxHP = 500;
        currentHP = maxHP;
        madeOf = RESOURCE.WOOD;
        CreateTraitContainer();
        traitContainer.AddTrait(this, "Flammable");
        visual.Initialize(this);
    }

    #region HP
    public void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null) {
        if (currentHP <= 0 && amount < 0) {
            return; //ignore
        }
        CombatManager.Instance.DamageModifierByElements(ref amount, elementalDamageType, this);
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        if (amount <= 0) {
            //ELEMENTAL_TYPE elementalType = ELEMENTAL_TYPE.Normal;
            //if(source != null && source is Character) {
            //    elementalType = (source as Character).combatComponent.elementalDamage.type;
            //}
            // CombatManager.Instance.CreateHitEffectAt(this, elementalDamageType);
            Character responsibleCharacter = null;
            if (source != null && source is Character) {
                responsibleCharacter = source as Character;
            }
            CombatManager.Instance.ApplyElementalDamage(amount, elementalDamageType, this, responsibleCharacter);
        }
        if (currentHP <= 0) {
            //wall has been destroyed
            _visual.UpdateWallState(this);
            Messenger.Broadcast(Signals.WALL_DESTROYED, this);
            gridTileLocation.CreateSeamlessEdgesForSelfAndNeighbours();
        } else if (amount < 0 && currentHP < maxHP) {
            //wall has been damaged
            _visual.UpdateWallAssets(this);
            Messenger.Broadcast(Signals.WALL_DAMAGED, this);
        } else if (amount > 0 && currentHP < maxHP) {
            //wall has been partially repaired
            _visual.UpdateWallState(this);
            //Messenger.Broadcast(Signals.WALL_REPAIRED, this);
        } else if (currentHP == maxHP) {
            //wall has been fully repaired
            _visual.UpdateWallAssets(this);
            _visual.UpdateWallState(this);
            Messenger.Broadcast(Signals.WALL_REPAIRED, this);
        }
    }
    public void OnHitByAttackFrom(Character characterThatAttacked, CombatState state, ref string attackSummary) {
        //GameManager.Instance.CreateHitEffectAt(this, characterThatAttacked.combatComponent.elementalDamage.type);
        AdjustHP(-characterThatAttacked.attackPower, characterThatAttacked.combatComponent.elementalDamage.type, source: characterThatAttacked);
    }
    #endregion

    #region Resource
    internal void ChangeResourceMadeOf(RESOURCE madeOf) {
        this.madeOf = madeOf;
        _visual.UpdateWallAssets(this);
        //TODO: Update HP based on new resource
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

    #region Settlement Map Object
    protected override void CreateMapObjectVisual() {
        mapVisual = _visual;
    }
    protected override void OnMapObjectStateChanged() { }
    public void SetGridTileLocation(LocationGridTile tile) {
        gridTileLocation = tile;
    }
    #endregion

    #region ITraitable
    public void AddAdvertisedAction(INTERACTION_TYPE actionType) {
        //Not Applicable
    }
    public void RemoveAdvertisedAction(INTERACTION_TYPE actionType) {
        //Not Applicable
    }
    #endregion
}
