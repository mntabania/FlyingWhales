using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Traits;
using Inner_Maps;
using UnityEngine.Assertions;

public class CombatManager : MonoBehaviour {
    public static CombatManager Instance;

    public const int pursueDuration = 5;
    public const string Hostility = "Hostility", Retaliation = "Retaliation", Berserked = "Berserked", Action = "Action",
        Threatened = "Threatened", Anger = "Anger", Join_Combat = "Join Combat", Drunk = "Drunk", Rage = "Rage";

    [SerializeField] private ProjectileDictionary _projectileDictionary;

    public delegate void ElementalTraitProcessor(ITraitable target, Trait trait);
    
    private void Awake() {
        Instance = this;
    }

    public void ApplyElementalDamage(int damage, ELEMENTAL_TYPE elementalType, ITraitable target, 
        Character responsibleCharacter = null, ElementalTraitProcessor elementalTraitProcessor = null) { 
        
        ElementalDamageData elementalDamage = ScriptableObjectsManager.Instance.GetElementalDamageData(elementalType);
        if (target != null) {
            CreateHitEffectAt(target, elementalType);
        }
        if (damage < 0) {
            //Damage should awaken sleeping characters
            if (target.traitContainer.HasTrait("Resting")) {
                if (target is Character character) {
                    character.jobQueue.CancelFirstJob();
                }
            }
        }
        if (!string.IsNullOrEmpty(elementalDamage.addedTraitName)) {
            bool hasSuccessfullyAdded = target.traitContainer.AddTrait(target, elementalDamage.addedTraitName, 
                out Trait trait, responsibleCharacter); //, out trait
            if (hasSuccessfullyAdded) {
                if (elementalType == ELEMENTAL_TYPE.Electric) {
                    ChainElectricDamage(target, damage);
                }
                elementalTraitProcessor?.Invoke(target, trait);
            }
        }
        if(elementalType == ELEMENTAL_TYPE.Earth) {
            EarthElementProcess(target);
        } else if (elementalType == ELEMENTAL_TYPE.Wind) {
            WindElementProcess(target);
        } else if (elementalType == ELEMENTAL_TYPE.Fire) {
            FireElementProcess(target);
        } else if (elementalType == ELEMENTAL_TYPE.Water) {
            WaterElementProcess(target);
        }
    }
    public void DamageModifierByElements(ref int damage, ELEMENTAL_TYPE elementalType, ITraitable target) {
        if(damage < 0) {
            if(IsImmuneToElement(target, elementalType)) {
                if(target is VaporTileObject) {
                    damage = 0;
                } else {
                    //Immunity - less 85% damage
                    damage = Mathf.RoundToInt(damage * 0.15f);
                }
                return;
            }
            if (elementalType == ELEMENTAL_TYPE.Fire) {
                if (target.traitContainer.HasTrait("Fire Prone")) {
                    damage *= 2;
                }
            }
        }
    }
    public bool IsImmuneToElement(ITraitable target, ELEMENTAL_TYPE elementalType) {
        if(target is VaporTileObject && elementalType != ELEMENTAL_TYPE.Ice && elementalType != ELEMENTAL_TYPE.Poison && elementalType != ELEMENTAL_TYPE.Fire) {
            //Vapors are immune to all other damage types except Ice
            return true;
        }
        if(elementalType != ELEMENTAL_TYPE.Fire) {
            if(target is WinterRose) {
                //Immunity - less 85% damage
                return true;
            }
        }
        if (elementalType != ELEMENTAL_TYPE.Water) {
            if (target is DesertRose) {
                //Immunity - less 85% damage
                return true;
            }
        }
        if (elementalType == ELEMENTAL_TYPE.Fire) {
            if (target.traitContainer.HasTrait("Fire Prone")) {
                return false;
            } else if (target.traitContainer.HasTrait("Fireproof")) {
                //Immunity - less 85% damage
                return true;
            }
        } else if (elementalType == ELEMENTAL_TYPE.Electric) {
            if (target.traitContainer.HasTrait("Electric")) {
                //Immunity - less 85% damage
                return true;
            }
        } else if (elementalType == ELEMENTAL_TYPE.Ice) {
            if (target.traitContainer.HasTrait("Cold Blooded")) {
                //Immunity - less 85% damage
                return true;
            }
        }
        return false;
    }
    public void CreateHitEffectAt(IDamageable poi, ELEMENTAL_TYPE elementalType) {
        ElementalDamageData elementalData = ScriptableObjectsManager.Instance.GetElementalDamageData(elementalType);
        if (poi.gridTileLocation == null) {
            return;
        }
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(elementalData.hitEffectPrefab.name, Vector3.zero, Quaternion.identity, poi.gridTileLocation.parentMap.objectsParent);
        if (!poi.mapObjectVisual || !poi.projectileReceiver) {
            go.transform.localPosition = poi.gridTileLocation.centeredLocalLocation;
        } else {
            go.transform.position = poi.projectileReceiver.transform.position;
        }
        // go.transform.position = poi.gridTileLocation.centeredWorldLocation;
        go.SetActive(true);

    }
    
    #region Explosion
    public void PoisonExplosion(IPointOfInterest target, LocationGridTile targetTile, int stacks) {
        StartCoroutine(PoisonExplosionCoroutine(target, targetTile, stacks));
        Messenger.Broadcast(Signals.POISON_EXPLOSION_TRIGGERED, target);
    }
    private IEnumerator PoisonExplosionCoroutine(IPointOfInterest target, LocationGridTile targetTile, int stacks) {
        while (GameManager.Instance.isPaused) {
            //Pause coroutine while game is paused
            //Might be performance heavy, needs testing
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        AudioManager.Instance.CreatePoisonExplosionAudio(targetTile);
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Poison_Explosion);
        List<ITraitable> traitables = new List<ITraitable>();
        List<LocationGridTile> affectedTiles = targetTile.GetTilesInRadius(1, includeTilesInDifferentStructure: true);
        float damagePercentage = 0.1f * stacks;
        if (damagePercentage > 1) {
            damagePercentage = 1;
        }
        BurningSource bs = null;
        for (int i = 0; i < affectedTiles.Count; i++) {
            LocationGridTile tile = affectedTiles[i];
            // traitables.AddRange(tile.GetTraitablesOnTile());
            tile.PerformActionOnTraitables((traitable) => PoisonExplosionEffect(traitable, damagePercentage, ref bs));
        }
        if(!(target is GenericTileObject)) {
            Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Poison Explosion", "effect");
            log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
            log.AddLogToInvolvedObjects();
        }
    }
    private void PoisonExplosionEffect(ITraitable traitable, float damagePercentage, ref BurningSource bs) {
        int damage = Mathf.RoundToInt(traitable.maxHP * damagePercentage);
        traitable.AdjustHP(-damage, ELEMENTAL_TYPE.Fire, showHPBar: true);
        Burning burningTrait = traitable.traitContainer.GetNormalTrait<Burning>("Burning");
        if (burningTrait != null && burningTrait.sourceOfBurning == null) {
            if (bs == null) {
                bs = new BurningSource(traitable.gridTileLocation.parentMap.region);
            }
            burningTrait.SetSourceOfBurning(bs, traitable);
            Assert.IsNotNull(burningTrait.sourceOfBurning, $"Burning source of {traitable.ToString()} was set to null");
        }
    }
    public void FrozenExplosion(IPointOfInterest target, LocationGridTile targetTile, int stacks) {
        StartCoroutine(FrozenExplosionCoroutine(target, targetTile, stacks));
    }
    private IEnumerator FrozenExplosionCoroutine(IPointOfInterest target, LocationGridTile targetTile, int stacks) {
        while (GameManager.Instance.isPaused) {
            //Pause coroutine while game is paused
            //Might be performance heavy, needs testing
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        AudioManager.Instance.CreateFrozenExplosionAudio(targetTile);
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Frozen_Explosion);
        List<ITraitable> traitables = new List<ITraitable>();
        List<LocationGridTile> affectedTiles = targetTile.GetTilesInRadius(2, includeTilesInDifferentStructure: true);
        float damagePercentage = 0.2f * stacks;
        if (damagePercentage > 1) {
            damagePercentage = 1;
        }
        for (int i = 0; i < affectedTiles.Count; i++) {
            LocationGridTile tile = affectedTiles[i];
            // traitables.AddRange(tile.GetTraitablesOnTile());
            tile.PerformActionOnTraitables((traitable) => FrozenExplosionEffect(traitable, damagePercentage));
        }

        if (!(target is GenericTileObject)) {
            Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Frozen Explosion", "effect");
            log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
            log.AddLogToInvolvedObjects();
        }
    }
    private void FrozenExplosionEffect(ITraitable traitable, float damagePercentage) {
        int damage = Mathf.RoundToInt(traitable.maxHP * damagePercentage);
        traitable.AdjustHP(-damage, ELEMENTAL_TYPE.Water, showHPBar: true);
    }
    public void ChainElectricDamage(ITraitable traitable, int damage) {
        damage = Mathf.RoundToInt(damage * 0.2f);
        if(damage >= 0) {
            damage = -1;
        }
        Messenger.Broadcast(Signals.ELECTRIC_CHAIN_TRIGGERED);
        List<ITraitable> traitables = new List<ITraitable>();
        if (traitable.gridTileLocation != null) {
            List<LocationGridTile> tiles = traitable.gridTileLocation.GetTilesInRadius(1, includeTilesInDifferentStructure: true);
            traitables.Clear();
            List<LocationGridTile> affectedTiles = new List<LocationGridTile>();
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile tile = tiles[i];
                if (tile.genericTileObject.traitContainer.HasTrait("Wet")) {
                    // traitables.AddRange(tile.GetTraitablesOnTile());
                    affectedTiles.Add(tile);
                }
            }
            if (affectedTiles.Count > 0) {
                StartCoroutine(ChainElectricDamageCoroutine(affectedTiles, damage));
            }
        }
    }
    private IEnumerator ChainElectricDamageCoroutine(List<LocationGridTile> tiles, int damage) {
        HashSet<ITraitable> completedTiles = new HashSet<ITraitable>();
        for (int i = 0; i < tiles.Count; i++) {
            while (GameManager.Instance.isPaused) {
                //Pause coroutine while game is paused
                //Might be performance heavy, needs testing
                yield return null;
            }
            yield return new WaitForSeconds(0.1f);
            LocationGridTile tile = tiles[i];
            tile.PerformActionOnTraitables((traitable) => ChainElectricEffect(traitable, damage, ref completedTiles));
        }
    }
    private void ChainElectricEffect(ITraitable traitable, int damage, ref HashSet<ITraitable> completedObjects) {
        if (completedObjects.Contains(traitable) == false) { //!traitable.traitContainer.HasTrait("Zapped")
            completedObjects.Add(traitable);
            traitable.AdjustHP(damage, ELEMENTAL_TYPE.Electric, true, showHPBar: true);
        }
    }
    #endregion

    #region Elemental Type Processes
    private void EarthElementProcess(ITraitable target) {
        string elements = string.Empty;
        if (target.traitContainer.HasTrait("Zapped")) {
            elements += " Zapped";
        }
        if (target.traitContainer.HasTrait("Burning")) {
            elements += " Burning";
        }
        if (target.traitContainer.HasTrait("Poisoned")) {
            elements += " Poisoned";
        }
        if (target.traitContainer.HasTrait("Wet")) {
            elements += " Wet";
        }
        if (target.traitContainer.HasTrait("Freezing")) {
            elements += " Freezing";
        }
        if(elements != string.Empty) {
            elements = elements.TrimStart(' ');
            string[] elementsArray = elements.Split(' ');
            target.traitContainer.RemoveTrait(target, elementsArray[UnityEngine.Random.Range(0, elementsArray.Length)]);
        }
    }
    private void WindElementProcess(ITraitable target) {
        if (target.traitContainer.HasTrait("Poisoned")) {
            int stacks = target.traitContainer.stacks["Poisoned"];
            target.traitContainer.RemoveStatusAndStacks(target, "Poisoned");
            PoisonCloudTileObject poisonCloudTileObject = new PoisonCloudTileObject();
            poisonCloudTileObject.SetDurationInTicks(GameManager.Instance.GetTicksBasedOnHour(UnityEngine.Random.Range(2, 6)));
            poisonCloudTileObject.SetGridTileLocation(target.gridTileLocation);
            poisonCloudTileObject.OnPlacePOI();
            poisonCloudTileObject.SetStacks(stacks);
        }
        if (target.traitContainer.HasTrait("Wet")) {
            int stacks = target.traitContainer.stacks["Wet"];
            target.traitContainer.RemoveStatusAndStacks(target, "Wet");
            VaporTileObject vaporTileObject = new VaporTileObject();
            vaporTileObject.SetGridTileLocation(target.gridTileLocation);
            vaporTileObject.OnPlacePOI();
            vaporTileObject.SetStacks(stacks);
        }
    }
    private void FireElementProcess(ITraitable target) {
        if (target is WinterRose winterRose) {
            winterRose.WinterRoseEffect();
        } else if (target is PoisonCloudTileObject poisonCloudTileObject) {
            poisonCloudTileObject.Explode();
        }
    }
    private void WaterElementProcess(ITraitable target) {
        if (target is DesertRose desertRose) {
            desertRose.DesertRoseEffect();
        }
    }
    public void DefaultElementalTraitProcessor(ITraitable traitable, Trait trait) {
        if (trait is Burning burning) {
            //by default, will create new burning source for every burning trait.
            BurningSource burningSource = new BurningSource(traitable.gridTileLocation.parentMap.region);
            burning.SetSourceOfBurning(burningSource, traitable);
        }
    }
    #endregion

    #region Projectiles
    public Projectile CreateNewProjectile(ELEMENTAL_TYPE elementalType, Transform parent, Vector3 worldPos) {
        GameObject projectileGO =
            ObjectPoolManager.Instance.InstantiateObjectFromPool(_projectileDictionary[elementalType].name,
                worldPos, Quaternion.identity, parent, true);
        return projectileGO.GetComponent<Projectile>();
    }
    #endregion
}

