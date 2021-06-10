using System;
using System.Linq;
using DG.Tweening;
using Inner_Maps;
using Ruinarch.Custom_UI;
using UnityEngine;

public class StoreTargetButton : MonoBehaviour {
    [SerializeField] private RuinarchButton btn;
    [SerializeField] private HoverHandler hoverHandler;
    //[SerializeField] private GameObject goCover;
    [SerializeField] private GameObject effectPrefab;
    
    private IStoredTarget _target;
    
    private void Awake() {
        btn.onClick.AddListener(OnClick);
        hoverHandler.AddOnHoverOverAction(OnHoverOver);
        hoverHandler.AddOnHoverOutAction(OnHoverOut);
        Messenger.AddListener(Signals.GAME_STARTED, OnGameStarted);
    }
    private void OnGameStarted() {
        Messenger.RemoveListener(Signals.GAME_STARTED, OnGameStarted);
        UpdateInteractableState();
    }
    private void OnEnable() {
        UpdateInteractableState();
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<IStoredTarget>(PlayerSignals.PLAYER_STORED_TARGET, OnPlayerStoredTarget);
    }
    private void OnDisable() {
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.RemoveListener<IStoredTarget>(PlayerSignals.PLAYER_STORED_TARGET, OnPlayerStoredTarget);
    }

    public void SetTarget(IStoredTarget p_target) {
        _target = p_target;
        UpdateInteractableState();
    }
    
    private void OnClick() {
        PlayStoreAnimation();
        PlayerManager.Instance.player.storedTargetsComponent.Store(_target);
        UpdateInteractableState();
    }
    private void PlayStoreAnimation() {
        Vector3 pos = btn.transform.position;
        pos.z = 0f;
        GameObject effectGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(effectPrefab.name, pos, Quaternion.identity, UIManager.Instance.transform);
        effectGO.transform.position = pos;

        Vector3 intelTabPos = PlayerUI.Instance.targetsToggle.transform.position;
        
        Vector3 controlPointA = effectGO.transform.position;
        controlPointA.x += 500f;
        controlPointA.z = 0f;
		      
        Vector3 controlPointB = intelTabPos;
        controlPointB.y -= 5f;
        controlPointB.z = 0f;

        effectGO.transform.DOPath(new[] {intelTabPos, controlPointA, controlPointB}, 0.7f, PathType.CubicBezier).SetEase(Ease.InSine).OnComplete(() => OnEffectCompleted(effectGO));
    }
    private void OnEffectCompleted(GameObject p_effect) {
        PlayerUI.Instance.DoTargetTabPunchEffect();
        StoreTargetEffect storeTargetEffect = p_effect.GetComponent<StoreTargetEffect>();
        storeTargetEffect.trailParticles.Stop();
        storeTargetEffect.SetImageState(false);
    }
    
    public void UpdateInteractableState() {
        if (_target == null) { return; }
        if (!_target.CanBeStoredAsTarget() || !GameManager.Instance.gameHasStarted) {
            //cannot store this target. hide button
            gameObject.SetActive(false);
        } else {
            gameObject.SetActive(true);
            bool isAlreadyStored = PlayerManager.Instance.player.storedTargetsComponent.IsAlreadyStored(_target);
            btn.interactable = !isAlreadyStored;
            //goCover.SetActive(isAlreadyStored);
        }
    }
    private void OnHoverOver() {
        if (!btn.interactable) {
            UIManager.Instance.ShowSmallInfo($"Target {_target.iconRichText} {_target.name} is already stored.", autoReplaceText: false);    
        } else {
            if (PlayerManager.Instance.player.storedTargetsComponent.HasStoredMaxCapacity()) {
                IStoredTarget first = PlayerManager.Instance.player.storedTargetsComponent.allStoredTargets.First();
                UIManager.Instance.ShowSmallInfo($"Store target. \n{UtilityScripts.Utilities.ColorizeInvalidText($"Warning! Storing this will overwrite oldest stored target : {first.iconRichText} {first.name}")}", autoReplaceText: false);
            } else {
                UIManager.Instance.ShowSmallInfo("Store target", autoReplaceText: false);    
            }
        }
        
    }
    private void OnHoverOut() {
        UIManager.Instance.HideSmallInfo();
    }
    private void OnDestroy() {
        _target = null;
    }

    #region Listeners
    private void OnCharacterDied(Character p_character) {
        if (_target == p_character) {
            UpdateInteractableState();
        }
    }
    private void OnPlayerStoredTarget(IStoredTarget p_target) {
        if (_target == p_target) {
            UpdateInteractableState();
        }
    }
    #endregion
}
