using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CharacterMarkerAnimationListener : MonoBehaviour {

    [SerializeField] private CharacterMarker parentMarker;
    private float _timeElapsed;
    private bool isExecutingAttack;
    private const float AttackTime = 0.16f;
    
    public void OnAttackExecuted() {
        if (parentMarker.character.stateComponent.currentState is CombatState combatState && combatState.isExecutingAttack) {
            Debug.Log($"{parentMarker.character.name} executed attack.");
            if (parentMarker.character.characterClass.rangeType == RANGE_TYPE.RANGED) {
                CreateProjectile(combatState.currentClosestHostile, combatState);
                combatState.isExecutingAttack = false;
            } else {
                combatState.isExecutingAttack = false;
                combatState.OnAttackHit(combatState.currentClosestHostile);
                if (parentMarker.character is Summon) {
                    AudioManager.Instance.CreateAudioObject(AudioManager.Instance.GetRandomPunchAudio(),
                        parentMarker.character.gridTileLocation, 1, false);    
                } else {
                    switch (parentMarker.character.characterClass.className) {
                        case "Craftsman":
                        case "Miner":
                            AudioManager.Instance.CreateAudioObject(AudioManager.Instance.GetRandomBluntWeaponAudio(),
                                parentMarker.character.gridTileLocation, 1, false);
                            break;
                        case "Noble":
                        case "Knight":
                        case "Barbarian":
                        case "Marauder":
                            AudioManager.Instance.CreateAudioObject(
                                combatState.currentClosestHostile is Character
                                    ? AudioManager.Instance.GetRandomSwordAgainstFleshAudio()
                                    : AudioManager.Instance.GetRandomSwordAgainstObjectAudio(),
                                parentMarker.character.gridTileLocation, 1, false);
                            break;
                        default:
                            AudioManager.Instance.CreateAudioObject(AudioManager.Instance.GetRandomPunchAudio(),
                                parentMarker.character.gridTileLocation, 1, false);
                            break;
                    }
                }
            }
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
    public void CreateProjectile(IDamageable target, CombatState state, Action<IDamageable, CombatState> onHitAction = null) {
        if (target == null || target.currentHP <= 0) {
            return;
        }
        //Create projectile here and set the on hit action to combat state OnAttackHit
        Projectile projectile = CombatManager.Instance.CreateNewProjectile(parentMarker.character.combatComponent.elementalDamage.type, parentMarker.character.currentRegion.innerMap.objectsParent, parentMarker.projectileParent.transform.position);
        projectile.SetTarget(target.projectileReceiver.transform, target, state, parentMarker.character);
        if (onHitAction != null) {
            projectile.onHitAction = onHitAction;
        } else {
            projectile.onHitAction = OnProjectileHit;    
        }
        
        AudioManager.Instance.CreateAudioObject(AudioManager.Instance.GetRandomBowAndArrowAudio(),
            parentMarker.character.gridTileLocation, 1, false);
    }
    /// <summary>
    /// Called when an attack that this character does, hits another character.
    /// </summary>
    /// <param name="target">The character that was hit.</param>
    /// <param name="fromState">The projectile was created from this combat state.</param>
    private void OnProjectileHit(IDamageable target, CombatState fromState) {
        //fromState.OnAttackHit(character);
        if (parentMarker.character != null) {
            AudioManager.Instance.CreateAudioObject(AudioManager.Instance.GetRandomArrowImpactAudio(),
                target.gridTileLocation, 1, false);
            if (parentMarker.character.stateComponent.currentState is CombatState combatState) {
                combatState.OnAttackHit(target);
            } else if (target != null) {
                string attackSummary = $"{parentMarker.character.name} hit {target.name}, outside of combat state";
                target.OnHitByAttackFrom(parentMarker.character, fromState, ref attackSummary);
                parentMarker.character.logComponent.PrintLogIfActive(attackSummary);
            }    
        }
    }
}
