﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Traits;
using Inner_Maps;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UtilityScripts;

public class CombatManager : BaseMonoBehaviour {
    public static CombatManager Instance;

    public int searchLength;
    public int spread;
    public int aimStrength;

    public const int pursueDuration = 4;
    public const string Hostility = "Hostility", Retaliation = "Retaliation", Berserked = "Berserked", Action = "Action",
        Threatened = "Threatened", Anger = "Anger", Join_Combat = "Join Combat", Drunk = "Drunk", Rage = "Rage", Demon_Kill = "Demon Kill", Dig = "Dig",
        Avoiding_Witnesses = "Avoiding Witnesses", Encountered_Hostile = "Encountered Hostile", Clear_Demonic_Intrusion = "Clear_Demonic_Intrusion", Abduct = "Abduct", Apprehend = "Apprehend",
        Monster_Scent = "Monster_Scent", Fullness_Recovery = "Fullness_Recovery";

    //Hostility reasons
    public const string Raid = "Raid", Warring_Factions = "Warring_Factions",
        Slaying_Monster = "Slaying_Monster", Slaying_Undead = "Slaying_Undead", Slaying_Demon = "Slaying_Demon", Slaying_Villager = "Slaying_Villager",
        Incapacitating_Monster = "Incapacitating_Monster", Incapacitating_Undead = "Incapacitating_Undead", Incapacitating_Demon = "Incapacitating_Demon", Incapacitating_Villager = "Incapacitating_Villager",
        Fighting_Vagrant = "Fighting_Vagrant", Feral_Monster = "Feral_Monster",
        Hostile_Undead = "Hostile_Undead", Defending_Territory = "Defending_Territory", Defending_Home = "Defending_Home";

    //Retaliation reasons
    public const string Resisting_Arrest = "Resisting_Arrest", Resisting_Abduction = "Resisting_Abduction", Defending_Self = "Defending_Self";




    [SerializeField] private ProjectileDictionary _projectileDictionary;
    [SerializeField] private GameObject _dragonProjectile;


    public delegate void ElementalTraitProcessor(ITraitable target, Trait trait);
    
    private void Awake() {
        Instance = this;
    }
    protected override void OnDestroy() {
        base.OnDestroy();
        Instance = null;
    }

    public void ApplyElementalDamage(int damage, ELEMENTAL_TYPE elementalType, ITraitable target, Character characterResponsible = null, ElementalTraitProcessor elementalTraitProcessor = null, bool createHitEffect = true) {
        Profiler.BeginSample("Apply Elemental Damage - Get Data");
        ElementalDamageData elementalDamage = ScriptableObjectsManager.Instance.GetElementalDamageData(elementalType);
        Profiler.EndSample();
        
        Profiler.BeginSample("Apply Elemental Damage - Create Hit Effect");
        if (target != null && createHitEffect) {
            CreateHitEffectAt(target, elementalType);
        }
        Profiler.EndSample();
        
        Profiler.BeginSample("Apply Elemental Damage - Wake Sleeping");
        if (damage < 0) {
            //Damage should awaken sleeping characters
            if (target.traitContainer.HasTrait("Resting")) {
                if (target is Character character) {
                    character.jobQueue.CancelFirstJob();
                }
            }

            //Damage should remove disguise
            if(target is Character targetCharacter) {
                targetCharacter.reactionComponent.SetDisguisedCharacter(null);
            }
        }
        Profiler.EndSample();
        
        if (!string.IsNullOrEmpty(elementalDamage.addedTraitName)) {
            Profiler.BeginSample("Apply Elemental Damage - Add Elemental Trait");
            bool hasSuccessfullyAdded = target.traitContainer.AddTrait(target, elementalDamage.addedTraitName, 
                out Trait trait, characterResponsible); //, out trait
            Profiler.EndSample();
            if (hasSuccessfullyAdded) {
                Profiler.BeginSample("Apply Elemental Damage - Chain Electric");
                if (elementalType == ELEMENTAL_TYPE.Electric) {
                    ChainElectricDamage(target, damage, characterResponsible, target);
                }
                Profiler.EndSample();
                
                Profiler.BeginSample("Apply Elemental Damage - Elemental Trait Processor");
                if (elementalTraitProcessor != null) {
                    elementalTraitProcessor.Invoke(target, trait);    
                } else {
                    DefaultElementalTraitProcessor(target, trait);
                }
                Profiler.EndSample();
                
            }
        }
        Profiler.BeginSample("Apply Elemental Damage - General Element Process");
        GeneralElementProcess(target, characterResponsible);
        Profiler.EndSample();
        
        if(elementalType == ELEMENTAL_TYPE.Earth) {
            Profiler.BeginSample("Apply Elemental Damage - Earth Process");
            EarthElementProcess(target);
            Profiler.EndSample();
        } else if (elementalType == ELEMENTAL_TYPE.Wind) {
            Profiler.BeginSample("Apply Elemental Damage - Wind Process");
            WindElementProcess(target, characterResponsible);
            Profiler.EndSample();
        } else if (elementalType == ELEMENTAL_TYPE.Fire) {
            Profiler.BeginSample("Apply Elemental Damage - Fire Process");
            FireElementProcess(target);
            Profiler.EndSample();
        } else if (elementalType == ELEMENTAL_TYPE.Water) {
            Profiler.BeginSample("Apply Elemental Damage - Water Process");
            WaterElementProcess(target);
            Profiler.EndSample();
        } else if (elementalType == ELEMENTAL_TYPE.Electric) {
            Profiler.BeginSample("Apply Elemental Damage - Electric Process");
            ElectricElementProcess(target);
            Profiler.EndSample();
        } else if (elementalType == ELEMENTAL_TYPE.Normal) {
            Profiler.BeginSample("Apply Elemental Damage - Normal Process");
            NormalElementProcess(target);
            Profiler.EndSample();
        }
    }
    public void ModifyDamage(ref int damage, ELEMENTAL_TYPE elementalType, float piercingPower, ITraitable target) {
        if(damage < 0) {
            if(target is Character targetCharacter) {
                //Piercing and Resistances
                targetCharacter.piercingAndResistancesComponent.ModifyValueByResistance(ref damage, elementalType, piercingPower);
            } else {
                if (elementalType == ELEMENTAL_TYPE.Electric) {
                    if (target is TileObject && !(target is GenericTileObject)) {
                        damage = Mathf.RoundToInt(damage * 0.25f);
                        if (damage >= 0) {
                            damage = -1;
                        }
                    }
                }
            }
            //if (target.traitContainer.HasTrait("Immune")) {
            //    damage = 0;
            //} else {
            //    if (target.traitContainer.HasTrait("Protection")) {
            //        //Protected - less 85% damage
            //        damage = Mathf.RoundToInt(damage * 0.5f);
            //        if (damage >= 0) {
            //            damage = -1;
            //        }
            //    }
            //    if (IsImmuneToElement(target, elementalType)) {
            //        if (target is Vapor) {
            //            damage = 0;
            //        } else {
            //            //Immunity - less 85% damage
            //            damage = Mathf.RoundToInt(damage * 0.15f);
            //            if (damage >= 0) {
            //                damage = -1;
            //            }
            //        }
            //        return;
            //    }
            //    if (elementalType == ELEMENTAL_TYPE.Fire) {
            //        if (target.traitContainer.HasTrait("Fire Prone")) {
            //            damage *= 2;
            //        }
            //    } else if(elementalType == ELEMENTAL_TYPE.Electric) {
            //        if ((target is TileObject || target is StructureWallObject) && !(target is GenericTileObject)) {
            //            damage = Mathf.RoundToInt(damage * 0.25f);
            //            if (damage >= 0) {
            //                damage = -1;
            //            }
            //        }
            //    }
            //}
        }
    }
    public bool IsImmuneToElement(ITraitable target, ELEMENTAL_TYPE elementalType) {
        if(target is Vapor && elementalType != ELEMENTAL_TYPE.Ice && elementalType != ELEMENTAL_TYPE.Poison && elementalType != ELEMENTAL_TYPE.Fire) {
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
            if (target.traitContainer.HasTrait("Cold Blooded", "Iceproof")) {
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
    public void PoisonExplosion(IPointOfInterest target, LocationGridTile targetTile, int stacks, Character characterResponsible, int radius) {
        StartCoroutine(PoisonExplosionCoroutine(target, targetTile, stacks, characterResponsible, radius));
        if (characterResponsible == null) {
            Messenger.Broadcast(PlayerSignals.POISON_EXPLOSION_TRIGGERED_BY_PLAYER, target);    
        }
    }
    private IEnumerator PoisonExplosionCoroutine(IPointOfInterest target, LocationGridTile targetTile, int stacks, Character characterResponsible, int radius) {
        while (GameManager.Instance.isPaused) {
            //Pause coroutine while game is paused
            //Might be performance heavy, needs testing
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        AudioManager.Instance.CreatePoisonExplosionAudio(targetTile);
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Poison_Explosion);
        List<ITraitable> traitables = new List<ITraitable>();
        List<LocationGridTile> affectedTiles = targetTile.GetTilesInRadius(radius, includeCenterTile: true, includeTilesInDifferentStructure: true);
        float damagePercentage = 0.1f * stacks;
        if (damagePercentage > 1) {
            damagePercentage = 1;
        }
        BurningSource bs = null;
        for (int i = 0; i < affectedTiles.Count; i++) {
            LocationGridTile tile = affectedTiles[i];
            // traitables.AddRange(tile.GetTraitablesOnTile());
            tile.PerformActionOnTraitables((traitable) => PoisonExplosionEffect(traitable, damagePercentage, characterResponsible, ref bs));
        }
        // if(!(target is GenericTileObject)) {
        //     Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Poison Explosion", "effect");
        //     log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //     PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        //     log.AddLogToInvolvedObjects();
        // }
    }
    private void PoisonExplosionEffect(ITraitable traitable, float damagePercentage, Character characterResponsible, ref BurningSource bs) {
        int damage = Mathf.RoundToInt(traitable.maxHP * damagePercentage);
        traitable.AdjustHP(-damage, ELEMENTAL_TYPE.Fire, true, characterResponsible, showHPBar: true);
        if (traitable.traitContainer.HasTrait("Burning")) {
            Burning burningTrait = traitable.traitContainer.GetTraitOrStatus<Burning>("Burning");
            if (burningTrait != null && burningTrait.sourceOfBurning == null) {
                if (bs == null) {
                    bs = new BurningSource();
                }
                burningTrait.SetSourceOfBurning(bs, traitable);
                Assert.IsNotNull(burningTrait.sourceOfBurning, $"Burning source of {traitable.ToString()} was set to null");
            }
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

        // if (!(target is GenericTileObject)) {
        //     Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Frozen Explosion", "effect");
        //     log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //     PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        //     log.AddLogToInvolvedObjects();
        // }
    }
    private void FrozenExplosionEffect(ITraitable traitable, float damagePercentage) {
        int damage = Mathf.RoundToInt(traitable.maxHP * damagePercentage);
        traitable.AdjustHP(-damage, ELEMENTAL_TYPE.Water, true, showHPBar: true);
    }
    public void ChainElectricDamage(ITraitable traitable, int damage, Character characterResponsible, ITraitable origin) {
        if (characterResponsible == null) {
            Messenger.Broadcast(PlayerSignals.ELECTRIC_CHAIN_TRIGGERED_BY_PLAYER);
        }

        if (traitable.gridTileLocation != null && !traitable.gridTileLocation.genericTileObject.traitContainer.HasTrait("Chained Electric")) {
            Trait trait = null;
            traitable.gridTileLocation.genericTileObject.traitContainer.AddTrait(traitable, "Chained Electric", out trait, characterResponsible: characterResponsible);
            ChainedElectric chainedElectric = trait as ChainedElectric;
            chainedElectric.SetDamage(damage);
        }

        //if (traitable.gridTileLocation != null && !traitable.gridTileLocation.genericTileObject.traitContainer.HasTrait("Chained Electric")) {
        //    traitable.gridTileLocation.genericTileObject.traitContainer.AddTrait(traitable, "Chained Electric");
        //    StartCoroutine(ChainElectricDamageCoroutine(traitable.gridTileLocation.neighbourList, damage, characterResponsible, origin));
        //}
    }
    private IEnumerator ChainElectricDamageCoroutine(LocationGridTile tile, int damage, Character characterResponsible, ITraitable origin) {
        yield return new WaitForSeconds(0.1f * GameManager.Instance.progressionSpeed);
        tile.PerformActionOnTraitables((traitable) => ChainElectricEffect(traitable, damage, characterResponsible, origin));
    }
    private IEnumerator ChainElectricDamageCoroutine(List<LocationGridTile> tiles, int damage, Character characterResponsible, ITraitable origin) {
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            if (tile.genericTileObject.traitContainer.HasTrait("Wet") && !tile.genericTileObject.traitContainer.HasTrait("Zapped", "Chained Electric")) {
                while (GameManager.Instance.isPaused) {
                    //Pause coroutine while game is paused
                    //Might be performance heavy, needs testing
                    yield return null;
                }
                yield return new WaitForSeconds(0.1f);
                tile.PerformActionOnTraitables((traitable) => ChainElectricEffect(traitable, damage, characterResponsible, origin));
            }
        }
    }
    private void ChainElectricEffect(ITraitable traitable, int damage, Character responsibleCharacter, ITraitable origin) {
        traitable.AdjustHP(damage, ELEMENTAL_TYPE.Electric, true, source: responsibleCharacter, showHPBar: true);
    }

    //public void StartChainElectricDamage(ITraitable traitable, int damage, Character characterResponsible, ITraitable origin) {
    //    if (traitable.gridTileLocation != null && !traitable.gridTileLocation.genericTileObject.traitContainer.HasTrait("Chained Electric")) {
    //        if (characterResponsible == null) {
    //            Messenger.Broadcast(Signals.ELECTRIC_CHAIN_TRIGGERED_BY_PLAYER);
    //        }
    //        traitable.gridTileLocation.genericTileObject.traitContainer.AddTrait(traitable, "Chained Electric");
    //        List<LocationGridTile> affectedTiles = new List<LocationGridTile>();
    //        List<LocationGridTile> tileHolder = new List<LocationGridTile>();
    //        tileHolder.AddRange(traitable.gridTileLocation.neighbourList);
    //        ChainElectricDamage(affectedTiles, tileHolder, damage, characterResponsible, origin);
    //    }
    //}
    //private void ChainElectricDamage(List<LocationGridTile> affectedTiles, List<LocationGridTile> tileHolder, int damage, Character characterResponsible, ITraitable origin) {
    //    damage = Mathf.RoundToInt(damage * 0.8f);
    //    if (damage >= 0) {
    //        damage = -1;
    //    }
    //    affectedTiles.Clear();
    //    for (int i = 0; i < tileHolder.Count; i++) {
    //        LocationGridTile tile = tileHolder[i];
    //        if (tile.genericTileObject.traitContainer.HasTrait("Wet") && !tile.genericTileObject.traitContainer.HasTrait("Zapped") && !tile.genericTileObject.traitContainer.HasTrait("Chained Electric")) {
    //            affectedTiles.Add(tile);
    //        }
    //    }
    //    if (affectedTiles.Count > 0) {
    //        StartCoroutine(ChainElectricDamageCoroutine(affectedTiles, tileHolder, damage, characterResponsible, origin));
    //    }
    //}
    //private IEnumerator ChainElectricDamageCoroutine(List<LocationGridTile> tiles, List<LocationGridTile> tileHolder, int damage, Character characterResponsible, ITraitable origin) {
    //    //HashSet<ITraitable> completedTiles = new HashSet<ITraitable>();
    //    //yield return new WaitForSeconds(0.5f); // * GameManager.Instance.progressionSpeed
    //    for (int i = 0; i < tiles.Count; i++) {
    //        LocationGridTile tile = tiles[i];
    //        while (GameManager.Instance.isPaused) {
    //            //Pause coroutine while game is paused
    //            //Might be performance heavy, needs testing
    //            yield return null;
    //        }
    //        //yield return null;
    //        yield return new WaitForSeconds(0.1f);
    //        tile.PerformActionOnTraitables((traitable) => ChainElectricEffect(traitable, damage, characterResponsible, origin)); //, ref completedTiles
    //    }
    //    tileHolder.Clear();
    //    for (int i = 0; i < tiles.Count; i++) {
    //        tileHolder.AddRange(tiles[i].neighbourList);
    //        //for (int j = 0; j < tiles[i].neighbourList.Count; j++) {
    //        //    LocationGridTile neighbour = tiles[i].neighbourList[j];
    //        //    if (!tiles.Contains(neighbour)) {
    //        //        tileHolder.Add(neighbour);
    //        //    }
    //        //}
    //    }
    //    ChainElectricDamage(tiles, tileHolder, damage, characterResponsible, origin);
    //}
    //private void ChainElectricEffect(ITraitable traitable, int damage, Character responsibleCharacter, ITraitable origin) { //, ref HashSet<ITraitable> completedObjects
    //    if (/*completedObjects.Contains(traitable) == false && */!traitable.traitContainer.HasTrait("Zapped") ) {
    //        //completedObjects.Add(traitable);

    //        //Add chained electric trait first before applying damage so that it enters the StartChainElectricDamage because of AdjustHP below, it already has the chained electric trait, therefore it will not chain anymore 
    //        if (!traitable.traitContainer.HasTrait("Chained Electric")) {
    //            traitable.traitContainer.AddTrait(traitable, "Chained Electric");
    //            traitable.AdjustHP(damage, ELEMENTAL_TYPE.Electric, true, source: responsibleCharacter, showHPBar: true);
    //        }
    //    }
    //}
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
        if (target is DesertRose desertRose) {
            desertRose.DesertRoseOtherDamageEffect();
        }
    }
    private void WindElementProcess(ITraitable target, Character responsibleCharacter) {
        if (target.traitContainer.HasTrait("Poisoned")) {
            int stacks = target.traitContainer.stacks["Poisoned"];
            target.traitContainer.RemoveStatusAndStacks(target, "Poisoned");
            InnerMapManager.Instance.SpawnPoisonCloud(target.gridTileLocation, stacks, GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(GameUtilities.RandomBetweenTwoNumbers(2, 5))));
        }
        if (target.traitContainer.HasTrait("Wet")) {
            int stacks = target.traitContainer.stacks["Wet"];
            target.traitContainer.RemoveStatusAndStacks(target, "Wet");
            Vapor vapor = new Vapor();
            vapor.SetGridTileLocation(target.gridTileLocation);
            vapor.OnPlacePOI();
            vapor.SetStacks(stacks);
            if (responsibleCharacter == null) {
                Messenger.Broadcast(PlayerSignals.VAPOR_FROM_WIND_TRIGGERED_BY_PLAYER);    
            }
        }
        if (target is DesertRose desertRose) {
            desertRose.DesertRoseOtherDamageEffect();
        }
    }
    private void FireElementProcess(ITraitable target) {
        if (target is WinterRose winterRose) {
            winterRose.WinterRoseEffect();
        } else if (target is PoisonCloud poisonCloudTileObject) {
            poisonCloudTileObject.Explode();
        } else if (target is DesertRose desertRose) {
            desertRose.DesertRoseOtherDamageEffect();
        }
    }
    private void WaterElementProcess(ITraitable target) {
        if (target is DesertRose desertRose) {
            desertRose.DesertRoseWaterEffect();
        }
    }
    private void ElectricElementProcess(ITraitable target) {
        if(target is Golem) {
            if (target.traitContainer.HasTrait("Hibernating")) {
                target.traitContainer.RemoveTrait(target, "Hibernating");
            }
            target.traitContainer.RemoveTrait(target, "Indestructible");
        } else if (target is DesertRose desertRose) {
            desertRose.DesertRoseOtherDamageEffect();
        }
    }
    private void NormalElementProcess(ITraitable target) {
        if (target is DesertRose desertRose) {
            desertRose.DesertRoseOtherDamageEffect();
        }
    }
    private void GeneralElementProcess(ITraitable target, Character source) {
        if(source != null && source.faction != null && source.faction.isPlayerFaction) {
            if(target is Dragon dragon && dragon.isAwakened) {
                dragon.SetIsAttackingPlayer(true);
            }
        }
    }
    public void DefaultElementalTraitProcessor(ITraitable traitable, Trait trait) {
        if (trait is Burning burning) {
            //by default, will create new burning source for every burning trait.
            BurningSource burningSource = new BurningSource();
            burning.SetSourceOfBurning(burningSource, traitable);
        }
    }
    #endregion

    #region Projectiles
    public Projectile CreateNewProjectile(Character actor, ELEMENTAL_TYPE elementalType, Transform parent, Vector3 worldPos) {
        GameObject projectileGO = null;
        if (actor != null && actor is Dragon) {
            projectileGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(_dragonProjectile.name, worldPos, Quaternion.identity, parent, true);
        } else {
            projectileGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(_projectileDictionary[elementalType].name, worldPos, Quaternion.identity, parent, true);
        }
        return projectileGO.GetComponent<Projectile>();
    }
    #endregion

    #region Piercing
    public static void ModifyValueByPiercingAndResistance(ref int p_value, float p_piercingPower, float p_resistance) {
        float percentMultiplier = (100f - (p_resistance - p_piercingPower)) / 100f;
        if(percentMultiplier > 1f) {
            percentMultiplier = 1f;
        } else if (percentMultiplier < 0f) {
            percentMultiplier = 0f;
        }
        float rawComputedValue = p_value * percentMultiplier;
        p_value = Mathf.RoundToInt(rawComputedValue);
    }
    public static void ModifyValueByPiercingAndResistance(ref float p_value, float p_piercingPower, float p_resistance) {
        float percentMultiplier = (100f - (p_resistance - p_piercingPower)) / 100f;
        if (percentMultiplier > 1f) {
            percentMultiplier = 1f;
        } else if (percentMultiplier < 0f) {
            percentMultiplier = 0f;
        }
        float rawComputedValue = p_value * percentMultiplier;
        p_value = rawComputedValue;
    }
    #endregion
}

