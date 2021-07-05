using UnityEngine;
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
        Monster_Scent = "Monster_Scent", Fullness_Recovery = "Fullness_Recovery", Taunted = "Taunted", Tanking = "Tanking", Snatch = "Snatch", Music_Hater_Knockout = "Music_Hater_Knockout", 
        Music_Hater_Murder = "Music_Hater_Murder";

    //Hostility reasons
    public const string Raid = "Raid", Warring_Factions = "Warring_Factions",
        Slaying_Monster = "Slaying_Monster", Slaying_Undead = "Slaying_Undead", Slaying_Demon = "Slaying_Demon", Slaying_Villager = "Slaying_Villager",
        Incapacitating_Monster = "Incapacitating_Monster", Incapacitating_Undead = "Incapacitating_Undead", Incapacitating_Demon = "Incapacitating_Demon", Incapacitating_Villager = "Incapacitating_Villager",
        Fighting_Vagrant = "Fighting_Vagrant", Feral_Monster = "Feral_Monster",
        Hostile_Undead = "Hostile_Undead", Defending_Territory = "Defending_Territory", Defending_Home = "Defending_Home";

    //Retaliation reasons
    public const string Resisting_Arrest = "Resisting_Arrest", Resisting_Abduction = "Resisting_Abduction", Defending_Self = "Defending_Self";

    //Flee reasons
    public const string Vulnerable = "Vulnerable when alone", Coward = "character is a coward";

    [SerializeField] private ProjectileDictionary _projectileDictionary;
    [SerializeField] private GameObject _dragonProjectile;

    public delegate void ElementalTraitProcessor(ITraitable target, Trait trait);

    public Dictionary<CHARACTER_COMBAT_BEHAVIOUR, CharacterCombatBehaviour> characterCombatBehaviours { get; private set; }
    public Dictionary<COMBAT_SPECIAL_SKILL, CombatSpecialSkill> combatSpecialSkills { get; private set; }

    private void Awake() {
        Instance = this;
    }
    protected override void OnDestroy() {
        base.OnDestroy();
        Instance = null;
    }
    public void Initialize() {
        ConstructAllCharacterCombatBehaviours();
        ConstructAllCombatSpecialSkills();
    }

    public void ApplyElementalDamage(int damage, ELEMENTAL_TYPE elementalType, ITraitable target, Character characterResponsible = null, ElementalTraitProcessor elementalTraitProcessor = null, bool createHitEffect = true, bool setAsPlayerSource = false) {
#if DEBUG_PROFILER
        Profiler.BeginSample("Apply Elemental Damage - Get Data");
#endif
        ElementalDamageData elementalDamage = ScriptableObjectsManager.Instance.GetElementalDamageData(elementalType);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

#if DEBUG_PROFILER
        Profiler.BeginSample("Apply Elemental Damage - Create Hit Effect");
#endif
        if (target != null && createHitEffect) {
            CreateHitEffectAt(target, elementalType);
        }
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

#if DEBUG_PROFILER
        Profiler.BeginSample("Apply Elemental Damage - Wake Sleeping");
#endif
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
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

        if (!string.IsNullOrEmpty(elementalDamage.addedTraitName)) {
#if DEBUG_PROFILER
            Profiler.BeginSample("Apply Elemental Damage - Add Elemental Trait");
#endif
            bool hasSuccessfullyAdded = target.traitContainer.AddTrait(target, elementalDamage.addedTraitName, 
                out Trait trait, characterResponsible); //, out trait
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
            if (hasSuccessfullyAdded) {
                Trait elementalTrait = target.traitContainer.GetTraitOrStatus<Trait>(elementalDamage.addedTraitName);
                if(elementalTrait is IElementalTrait ielementalTrait && setAsPlayerSource) {
                    ielementalTrait.SetIsPlayerSource(true);
                }
#if DEBUG_PROFILER
                Profiler.BeginSample("Apply Elemental Damage - Chain Electric");
#endif
                if (elementalType == ELEMENTAL_TYPE.Electric) {
                    ChainElectricDamage(target, damage, characterResponsible, target, setAsPlayerSource);
                }
#if DEBUG_PROFILER
                Profiler.EndSample();
#endif

#if DEBUG_PROFILER
                Profiler.BeginSample("Apply Elemental Damage - Elemental Trait Processor");
#endif
                if (elementalTraitProcessor != null) {
                    elementalTraitProcessor.Invoke(target, trait);    
                } else {
                    DefaultElementalTraitProcessor(target, trait);
                }
#if DEBUG_PROFILER
                Profiler.EndSample();
#endif

            }
        }
#if DEBUG_PROFILER
        Profiler.BeginSample("Apply Elemental Damage - General Element Process");
#endif
        GeneralElementProcess(target, characterResponsible);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif

        if (elementalType == ELEMENTAL_TYPE.Earth) {
#if DEBUG_PROFILER
            Profiler.BeginSample("Apply Elemental Damage - Earth Process");
#endif
            EarthElementProcess(target);
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        } else if (elementalType == ELEMENTAL_TYPE.Wind) {
#if DEBUG_PROFILER
            Profiler.BeginSample("Apply Elemental Damage - Wind Process");
#endif
            WindElementProcess(target, characterResponsible);
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        } else if (elementalType == ELEMENTAL_TYPE.Fire) {
#if DEBUG_PROFILER
            Profiler.BeginSample("Apply Elemental Damage - Fire Process");
#endif
            FireElementProcess(target);
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        } else if (elementalType == ELEMENTAL_TYPE.Water) {
#if DEBUG_PROFILER
            Profiler.BeginSample("Apply Elemental Damage - Water Process");
#endif
            WaterElementProcess(target);
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        } else if (elementalType == ELEMENTAL_TYPE.Electric) {
#if DEBUG_PROFILER
            Profiler.BeginSample("Apply Elemental Damage - Electric Process");
#endif
            ElectricElementProcess(target);
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        } else if (elementalType == ELEMENTAL_TYPE.Normal) {
#if DEBUG_PROFILER
            Profiler.BeginSample("Apply Elemental Damage - Normal Process");
#endif
            NormalElementProcess(target);
#if DEBUG_PROFILER
            Profiler.EndSample();
#endif
        }
    }
    public void ModifyDamage(ref int damage, ELEMENTAL_TYPE elementalType, float piercingPower, ITraitable target) {
        if(damage < 0) {
            if (target.traitContainer.HasTrait("Immune")) {
                damage = 0;
                return;
            }
            if (target.traitContainer.HasTrait("Protection")) {
                damage = Mathf.RoundToInt(damage * 0.5f);
                if (damage >= 0) {
                    damage = -1;
                }
            }
            if (HasSpecialImmunityToElement(target, elementalType)) {
                if (target is Vapor) {
                    damage = 0;
                    return;
                } else {
                    //Immunity - less 85% damage
                    damage = Mathf.RoundToInt(damage * 0.15f);
                    if (damage >= 0) {
                        damage = -1;
                    }
                }
            }
            if (elementalType == ELEMENTAL_TYPE.Fire) {
                if (target.traitContainer.HasTrait("Fire Prone")) {
                    damage *= 2;
                }
            }
            if (target is Character targetCharacter) {
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
    public bool HasSpecialImmunityToElement(ITraitable target, ELEMENTAL_TYPE elementalType) {
        if (target is Vapor && elementalType != ELEMENTAL_TYPE.Ice && elementalType != ELEMENTAL_TYPE.Poison && elementalType != ELEMENTAL_TYPE.Fire) {
            //Vapors are immune to all other damage types except Ice
            return true;
        }
        if (elementalType != ELEMENTAL_TYPE.Fire) {
            if (target is WinterRose) {
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
        return false;
    }
    public bool IsImmuneToElement(ITraitable target, ELEMENTAL_TYPE elementalType) {
        if (HasSpecialImmunityToElement(target, elementalType)) {
            return true;
        }
        if (elementalType == ELEMENTAL_TYPE.Fire) {
            if (target.traitContainer.HasTrait("Fire Prone")) {
                return false;
            } else if (target.traitContainer.HasTrait("Fire Resistant")) {
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
    public void PoisonExplosion(IPointOfInterest target, LocationGridTile targetTile, int stacks, Character characterResponsible, int radius, bool isPlayerSource) {
        StartCoroutine(PoisonExplosionCoroutine(target, targetTile, stacks, characterResponsible, radius, isPlayerSource));
        if (characterResponsible == null) {
            Messenger.Broadcast(PlayerSignals.POISON_EXPLOSION_TRIGGERED_BY_PLAYER, target);    
        }
    }
    private IEnumerator PoisonExplosionCoroutine(IPointOfInterest target, LocationGridTile targetTile, int stacks, Character characterResponsible, int radius, bool isPlayerSource) {
        while (GameManager.Instance.isPaused) {
            //Pause coroutine while game is paused
            //Might be performance heavy, needs testing
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        AudioManager.Instance.CreatePoisonExplosionAudio(targetTile);
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Poison_Explosion);
        //List<ITraitable> traitables = new List<ITraitable>();
        List<LocationGridTile> affectedTiles = RuinarchListPool<LocationGridTile>.Claim();
        targetTile.PopulateTilesInRadius(affectedTiles, radius, includeCenterTile: true, includeTilesInDifferentStructure: true);
        float damagePercentage = 0.05f * stacks;
        if (damagePercentage > 1) {
            damagePercentage = 1;
        }
        BurningSource bs = null;
        for (int i = 0; i < affectedTiles.Count; i++) {
            LocationGridTile tile = affectedTiles[i];
            // traitables.AddRange(tile.GetTraitablesOnTile());
            tile.PerformActionOnTraitables((traitable) => PoisonExplosionEffect(traitable, damagePercentage, characterResponsible, ref bs, isPlayerSource));
        }
        RuinarchListPool<LocationGridTile>.Release(affectedTiles);
        // if(!(target is GenericTileObject)) {
        //     Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Poison Explosion", "effect");
        //     log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //     PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        //     log.AddLogToInvolvedObjects();
        // }
    }
    private void PoisonExplosionEffect(ITraitable traitable, float damagePercentage, Character characterResponsible, ref BurningSource bs, bool isPlayerSource) {
        int damage = Mathf.RoundToInt(traitable.maxHP * damagePercentage);
        traitable.AdjustHP(-damage, ELEMENTAL_TYPE.Fire, true, characterResponsible, showHPBar: true, isPlayerSource: isPlayerSource);
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
    public void FrozenExplosion(IPointOfInterest target, LocationGridTile targetTile, int stacks, bool isPlayerSource) {
        StartCoroutine(FrozenExplosionCoroutine(target, targetTile, stacks, isPlayerSource));
    }
    private IEnumerator FrozenExplosionCoroutine(IPointOfInterest target, LocationGridTile targetTile, int stacks, bool isPlayerSource) {
        while (GameManager.Instance.isPaused) {
            //Pause coroutine while game is paused
            //Might be performance heavy, needs testing
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        AudioManager.Instance.CreateFrozenExplosionAudio(targetTile);
        GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Frozen_Explosion);
        //List<ITraitable> traitables = new List<ITraitable>();
        List<LocationGridTile> affectedTiles = RuinarchListPool<LocationGridTile>.Claim();
        targetTile.PopulateTilesInRadius(affectedTiles, 2, includeTilesInDifferentStructure: true);
        float damagePercentage = 0.2f * stacks;
        if (damagePercentage > 1) {
            damagePercentage = 1;
        }
        for (int i = 0; i < affectedTiles.Count; i++) {
            LocationGridTile tile = affectedTiles[i];
            // traitables.AddRange(tile.GetTraitablesOnTile());
            tile.PerformActionOnTraitables((traitable) => FrozenExplosionEffect(traitable, damagePercentage, isPlayerSource));
        }
        RuinarchListPool<LocationGridTile>.Release(affectedTiles);
        // if (!(target is GenericTileObject)) {
        //     Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Frozen Explosion", "effect");
        //     log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //     PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        //     log.AddLogToInvolvedObjects();
        // }
    }
    private void FrozenExplosionEffect(ITraitable traitable, float damagePercentage, bool isPlayerSource) {
        int damage = Mathf.RoundToInt(traitable.maxHP * damagePercentage);
        traitable.AdjustHP(-damage, ELEMENTAL_TYPE.Water, true, showHPBar: true, isPlayerSource: isPlayerSource);
    }
    public void ChainElectricDamage(ITraitable traitable, int damage, Character characterResponsible, ITraitable origin, bool setAsPlayerSource = false) {
        if (characterResponsible == null) {
            Messenger.Broadcast(PlayerSignals.ELECTRIC_CHAIN_TRIGGERED_BY_PLAYER);
        }

        if (traitable.gridTileLocation != null && !traitable.gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.HasTrait("Chained Electric")) {
            traitable.gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.AddTrait(traitable, "Chained Electric", characterResponsible: characterResponsible);
            ChainedElectric chainedElectric = traitable.gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.GetTraitOrStatus<ChainedElectric>("Chained Electric");
            chainedElectric.SetDamage(damage);
            chainedElectric?.SetIsPlayerSource(setAsPlayerSource);
        }

        //if (traitable.gridTileLocation != null && !traitable.gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.HasTrait("Chained Electric")) {
        //    traitable.gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.AddTrait(traitable, "Chained Electric");
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
            if (tile.tileObjectComponent.genericTileObject.traitContainer.HasTrait("Wet") && !tile.tileObjectComponent.genericTileObject.traitContainer.HasTrait("Zapped", "Chained Electric")) {
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
    //    if (traitable.gridTileLocation != null && !traitable.gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.HasTrait("Chained Electric")) {
    //        if (characterResponsible == null) {
    //            Messenger.Broadcast(Signals.ELECTRIC_CHAIN_TRIGGERED_BY_PLAYER);
    //        }
    //        traitable.gridTileLocation.tileObjectComponent.genericTileObject.traitContainer.AddTrait(traitable, "Chained Electric");
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
    //        if (tile.tileObjectComponent.genericTileObject.traitContainer.HasTrait("Wet") && !tile.tileObjectComponent.genericTileObject.traitContainer.HasTrait("Zapped") && !tile.tileObjectComponent.genericTileObject.traitContainer.HasTrait("Chained Electric")) {
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
            //Capped at x1 multiplier
            //The reason is that piercing should not amplify initial damage (which is p_value)
            //Its purpose is only to reduce the resistance so if piercing is higher than the resistance, the affected character will take the full amount of damage (p_value) but it will not be doubled or increased any further
            //This means that its resistance is neglected when piercing is higher
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

#region Combat Behaviour
    private void ConstructAllCharacterCombatBehaviours() {
        CHARACTER_COMBAT_BEHAVIOUR[] behaviourTypes = CollectionUtilities.GetEnumValues<CHARACTER_COMBAT_BEHAVIOUR>();
        characterCombatBehaviours = new Dictionary<CHARACTER_COMBAT_BEHAVIOUR, CharacterCombatBehaviour>();
        for (int i = 0; i < behaviourTypes.Length; i++) {
            CHARACTER_COMBAT_BEHAVIOUR type = behaviourTypes[i];
            if (type != CHARACTER_COMBAT_BEHAVIOUR.None) {
                string typeName = $"{UtilityScripts.Utilities.NotNormalizedConversionEnumToStringNoSpaces(type.ToString())}CombatBehaviour, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
                CharacterCombatBehaviour behaviour = System.Activator.CreateInstance(System.Type.GetType(typeName) ??
                   throw new Exception($"Problem with creating character combat behaviour for {typeName}")) as CharacterCombatBehaviour;
                characterCombatBehaviours.Add(type, behaviour);
            }
        }
    }
    public CharacterCombatBehaviour GetCombatBehaviour(CHARACTER_COMBAT_BEHAVIOUR p_behaviourType) {
        if (characterCombatBehaviours.ContainsKey(p_behaviourType)) {
            return characterCombatBehaviours[p_behaviourType];
        }
        return null;
    }
#endregion

#region Combat Special Skill
    private void ConstructAllCombatSpecialSkills() {
        COMBAT_SPECIAL_SKILL[] skillTypes = CollectionUtilities.GetEnumValues<COMBAT_SPECIAL_SKILL>();
        combatSpecialSkills = new Dictionary<COMBAT_SPECIAL_SKILL, CombatSpecialSkill>();
        for (int i = 0; i < skillTypes.Length; i++) {
            COMBAT_SPECIAL_SKILL type = skillTypes[i];
            if(type != COMBAT_SPECIAL_SKILL.None) {
                string typeName = $"{UtilityScripts.Utilities.NotNormalizedConversionEnumToStringNoSpaces(type.ToString())}SpecialSkill, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
                CombatSpecialSkill skill = System.Activator.CreateInstance(System.Type.GetType(typeName) ??
                   throw new Exception($"Problem with creating combat special skill for {typeName}")) as CombatSpecialSkill;
                combatSpecialSkills.Add(type, skill);
            }
        }
    }
    public CombatSpecialSkill GetCombatSpecialSkill(COMBAT_SPECIAL_SKILL p_skillType) {
        if (combatSpecialSkills.ContainsKey(p_skillType)) {
            return combatSpecialSkills[p_skillType];
        }
        return null;
    }
#endregion

    public bool IsDamageSourceFromPlayerSpell(object source) {
        if (source != null) {
            if (source is SkillData skill && skill.category == PLAYER_SKILL_CATEGORY.SPELL) {
                return true;
            } else if (source is LocustSwarm locustSwarm && locustSwarm.isPlayerSource) {
                return true;
            } else if (source is Tornado tornado && tornado.isPlayerSource) {
                return true;
            }
        }
        return false;
    }
}

