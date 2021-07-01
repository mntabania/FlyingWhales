using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using UtilityScripts;

[ExecuteInEditMode]
public class CharacterMarkerAnimationListener : MonoBehaviour {
    
    [SerializeField] private CharacterMarker parentMarker;
    private float _timeElapsed;
    private bool isExecutingAttack;
    private const float AttackTime = 0.16f;
    
    public void OnAttackExecuted() {
        if (parentMarker.character.stateComponent.currentState is CombatState combatState && combatState.isExecutingAttack) {
#if DEBUG_LOG
            Debug.Log($"{parentMarker.character.name} executed attack.");
#endif
            if (parentMarker.character.characterClass.rangeType == RANGE_TYPE.RANGED) {
                CreateProjectile(combatState.currentClosestHostile, combatState);
                //combatState.isExecutingAttack = false;
            } else {
                //combatState.isExecutingAttack = false;
                combatState.OnAttackHit(combatState.currentClosestHostile);
                if (parentMarker.character == null) { return; } 
                if (parentMarker.character is Summon) {
                    AudioManager.Instance.TryCreateAudioObject(AudioManager.Instance.GetRandomPunchAudio(),
                        parentMarker.character.gridTileLocation, 1, false);    
                } else {
                    switch (parentMarker.character.characterClass.className) {
                        case "Crafter":
                        case "Miner":
                            AudioManager.Instance.TryCreateAudioObject(AudioManager.Instance.GetRandomBluntWeaponAudio(),
                                parentMarker.character.gridTileLocation, 1, false);
                            break;
                        case "Noble":
                        case "Knight":
                        case "Barbarian":
                        case "Marauder":
                            AudioManager.Instance.TryCreateAudioObject(
                                combatState.currentClosestHostile is Character
                                    ? AudioManager.Instance.GetRandomSwordAgainstFleshAudio()
                                    : AudioManager.Instance.GetRandomSwordAgainstObjectAudio(),
                                parentMarker.character.gridTileLocation, 1, false);
                            break;
                        default:
                            AudioManager.Instance.TryCreateAudioObject(AudioManager.Instance.GetRandomPunchAudio(),
                                parentMarker.character.gridTileLocation, 1, false);
                            break;
                    }
                }
            }
            combatState.isExecutingAttack = false;
        }
    }
    public void StartAttackExecution() {
        isExecutingAttack = true;
    }
    private void Update() {
        if (isExecutingAttack) {
            _timeElapsed += Time.deltaTime;
            if (_timeElapsed >= AttackTime) {
                _timeElapsed = 0f;
                isExecutingAttack = false;
                OnAttackExecuted();
            }
        }
    }
    public void CreateProjectile(IDamageable target, CombatState state, Action<IDamageable, CombatState, Projectile> onHitAction = null) {
        if (target == null || target.currentHP <= 0 || target.projectileReceiver == null) {
            return;
        }
        //Create projectile here and set the on hit action to combat state OnAttackHit
        Projectile projectile = CombatManager.Instance.CreateNewProjectile(parentMarker.character, parentMarker.character.combatComponent.elementalDamage.type, parentMarker.character.currentRegion.innerMap.objectsParent, parentMarker.projectileParent.transform.position);
        projectile.SetTarget(target.projectileReceiver.transform, target, state, parentMarker.character);
        if (onHitAction != null) {
            projectile.onHitAction = onHitAction;
        } else {
            projectile.onHitAction = OnProjectileHit;    
        }
        
        AudioManager.Instance.TryCreateAudioObject(AudioManager.Instance.GetRandomBowAndArrowAudio(),
            parentMarker.character.gridTileLocation, 1, false);
    }
    /// <summary>
    /// Called when an attack that this character does, hits another character.
    /// </summary>
    /// <param name="target">The character that was hit.</param>
    /// <param name="fromState">The projectile was created from this combat state.</param>
    private void OnProjectileHit(IDamageable target, CombatState fromState, Projectile projectile) {
        //fromState.OnAttackHit(character);
        if (parentMarker.character != null) {
            if (target.gridTileLocation != null) {
                AudioManager.Instance.TryCreateAudioObject(AudioManager.Instance.GetRandomArrowImpactAudio(),
                    target.gridTileLocation, 1, false);    
            }
            if (parentMarker.character.stateComponent.currentState is CombatState combatState) {
                if (projectile.isAOE) {
                    List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
                    target.gridTileLocation.PopulateTilesInRadius(tiles, 1, 0, true, true); //radius
                    for (int i = 0; i < tiles.Count; i++) {
                        LocationGridTile tile = tiles[i];
                        tile.PerformActionOnTraitables((traitable) => combatState.OnAttackHit(traitable == parentMarker.character ? null : traitable));
                    }
                    RuinarchListPool<LocationGridTile>.Release(tiles);
                } else {
                    combatState.OnAttackHit(target);
                }
            } else if (target != null) {
                string attackSummary = string.Empty;
#if DEBUG_LOG
                attackSummary = $"{parentMarker.character.name} hit {target.name}, outside of combat state";
#endif
                if (projectile.isAOE) {
                    List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
                    target.gridTileLocation.PopulateTilesInRadius(tiles, 1, 0, true, true); //radius
                    for (int i = 0; i < tiles.Count; i++) {
                        LocationGridTile tile = tiles[i];
                        tile.PerformActionOnTraitables((traitable) => traitable.OnHitByAttackFrom(traitable == parentMarker.character ? null : parentMarker.character, fromState, ref attackSummary));
                    }
                    RuinarchListPool<LocationGridTile>.Release(tiles);
                } else {
                    target.OnHitByAttackFrom(parentMarker.character, fromState, ref attackSummary);
                }
#if DEBUG_LOG
                parentMarker.character.logComponent.PrintLogIfActive(attackSummary);
#endif
            }    
        }
    }
    public void Reset() {
        isExecutingAttack = false;
    }
}
