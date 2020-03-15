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

    public const int pursueDuration = 10;

    [SerializeField] private ProjectileDictionary _projectileDictionary;

    private void Awake() {
        Instance = this;
    }

    public void ApplyElementalDamage(int damage, ELEMENTAL_TYPE elementalType, ITraitable target, Character responsibleCharacter = null) { //, bool shouldSetBurningSource = true
        ElementalDamageData elementalDamage = ScriptableObjectsManager.Instance.GetElementalDamageData(elementalType);
        if (target != null) {
            CreateHitEffectAt(target, elementalType);
        }
        if (damage < 0) {
            //Damage should awaken sleeping characters
            if (target.traitContainer.HasTrait("Resting")) {
                if (target is Character) {
                    Character character = target as Character;
                    character.jobQueue.CancelFirstJob();
                }
            }
        }
        if (!string.IsNullOrEmpty(elementalDamage.addedTraitName)) {
            //Trait trait = null;
            bool hasSuccessfullyAdded = target.traitContainer.AddTrait(target, elementalDamage.addedTraitName, responsibleCharacter); //, out trait
            if (hasSuccessfullyAdded) {
                if (elementalType == ELEMENTAL_TYPE.Electric) {
                    ChainElectricDamage(target, damage);
                }
            }
        }
        if(elementalType == ELEMENTAL_TYPE.Earth) {
            EarthElementProcess(target);
        }
    }
    public void DamageModifierByElements(ref int damage, ELEMENTAL_TYPE elementalType, ITraitable target) {
        if(damage < 0) {
            if(target is VaporTileObject && elementalType != ELEMENTAL_TYPE.Ice) {
                //Vapors are immune to all other damage types except Ice
                damage = 0;
                return;
            }
            if(elementalType == ELEMENTAL_TYPE.Fire) {
                if (target.traitContainer.HasTrait("Fire Prone")) {
                    damage *= 2;
                }
            } else if (elementalType == ELEMENTAL_TYPE.Electric) {
                if (target.traitContainer.HasTrait("Electric")) {
                    damage = 0;
                }
            }
        }
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
    }
    private IEnumerator PoisonExplosionCoroutine(IPointOfInterest target, LocationGridTile targetTile, int stacks) {
        while (GameManager.Instance.isPaused) {
            //Pause coroutine while game is paused
            //Might be performance heavy, needs testing
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Poison_Explosion);
        List<ITraitable> traitables = new List<ITraitable>();
        List<LocationGridTile> affectedTiles = targetTile.GetTilesInRadius(1, includeTilesInDifferentStructure: true);
        float damagePercentage = 0.1f * stacks;
        if (damagePercentage > 1) {
            damagePercentage = 1;
        }
        for (int i = 0; i < affectedTiles.Count; i++) {
            LocationGridTile tile = affectedTiles[i];
            traitables.AddRange(tile.GetTraitablesOnTile());
        }
        // flammables = flammables.Where(x => !x.traitContainer.HasTrait("Burning", "Burnt", "Wet", "Fireproof")).ToList();
        BurningSource bs = null;
        for (int i = 0; i < traitables.Count; i++) {
            ITraitable traitable = traitables[i];
            int damage = Mathf.RoundToInt(traitable.maxHP * damagePercentage);
            traitable.AdjustHP(-damage, ELEMENTAL_TYPE.Fire);
            Burning burningTrait = traitable.traitContainer.GetNormalTrait<Burning>("Burning");
            if (burningTrait != null && burningTrait.sourceOfBurning == null) {
                if (bs == null) {
                    bs = new BurningSource(traitable.gridTileLocation.parentMap.region);
                }
                burningTrait.SetSourceOfBurning(bs, traitable);
                Assert.IsNotNull(burningTrait.sourceOfBurning, $"Burning source of {traitable.ToString()} was set to null");
            }
        }

        if(!(target is GenericTileObject)) {
            Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Poison Explosion", "effect");
            log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
            log.AddLogToInvolvedObjects();
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
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Frozen_Explosion);
        List<ITraitable> traitables = new List<ITraitable>();
        List<LocationGridTile> affectedTiles = targetTile.GetTilesInRadius(2, includeTilesInDifferentStructure: true);
        float damagePercentage = 0.2f * stacks;
        if (damagePercentage > 1) {
            damagePercentage = 1;
        }
        for (int i = 0; i < affectedTiles.Count; i++) {
            LocationGridTile tile = affectedTiles[i];
            traitables.AddRange(tile.GetTraitablesOnTile());
        }
        // flammables = flammables.Where(x => !x.traitContainer.HasTrait("Burning", "Burnt", "Wet", "Fireproof")).ToList();
        // BurningSource bs = null;
        for (int i = 0; i < traitables.Count; i++) {
            ITraitable traitable = traitables[i];
            int damage = Mathf.RoundToInt(traitable.maxHP * damagePercentage);
            traitable.AdjustHP(-damage, ELEMENTAL_TYPE.Water);
            // Burning burningTrait = traitable.traitContainer.GetNormalTrait<Burning>();
            // if (burningTrait != null && burningTrait.sourceOfBurning == null) {
            //     if (bs == null) {
            //         bs = new BurningSource(InnerMapManager.Instance.currentlyShowingLocation);
            //     }
            //     burningTrait.SetSourceOfBurning(bs, traitable);
            // }
        }

        if (!(target is GenericTileObject)) {
            Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Frozen Explosion", "effect");
            log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
            log.AddLogToInvolvedObjects();
        }
    }
    public void ChainElectricDamage(ITraitable traitable, int damage) {
        List<ITraitable> traitables = new List<ITraitable>();
        if (traitable.gridTileLocation != null) {
            List<LocationGridTile> tiles = traitable.gridTileLocation.GetTilesInRadius(1, includeTilesInDifferentStructure: true);
            traitables.Clear();
            for (int i = 0; i < tiles.Count; i++) {
                if (tiles[i].genericTileObject.traitContainer.HasTrait("Wet")) {
                    traitables.AddRange(tiles[i].GetTraitablesOnTile());
                }
            }
            if (traitables.Count > 0) {
                StartCoroutine(ChainElectricDamageCoroutine(traitables, damage));
            }
        }
    }
    private IEnumerator ChainElectricDamageCoroutine(List<ITraitable> traitables, int damage) {
        for (int i = 0; i < traitables.Count; i++) {
            while (GameManager.Instance.isPaused) {
                //Pause coroutine while game is paused
                //Might be performance heavy, needs testing
                yield return null;
            }
            yield return new WaitForSeconds(0.1f);
            if (!traitables[i].traitContainer.HasTrait("Zapped")) {
                traitables[i].AdjustHP(damage, ELEMENTAL_TYPE.Electric, true);
            }
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