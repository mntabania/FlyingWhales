using System.Collections;
using System.Collections.Generic;
using EZObjectPools;
using Inner_Maps;
using UnityEngine;

public class AreaMapTravelLine : PooledObject {

    [SerializeField] private LineRenderer line;
    [SerializeField] private SpriteRenderer lineSprite;
    [SerializeField] private SpriteRenderer lineSpriteFill;
    [SerializeField] private Transform startTransform;
    [SerializeField] private Transform targetTransform;

    private LocationGridTile start;
    //private LocationGridTile end;
    private AreaInnerTileMap areaMap;

    private float targetXScale; //linespriteFill
    private int ticksLeft;
    private Character owner;

    public void DrawLine(LocationGridTile start, LocationGridTile end, Character owner, AreaInnerTileMap areaMap) {
        this.owner = owner;
        this.start = start;
        //this.end = end;
        this.areaMap = areaMap;
        this.transform.localPosition = Vector3.zero;
        startTransform.localPosition = new Vector3(start.localPlace.x + 0.5f, start.localPlace.y + 0.5f, 0f);
        targetTransform.localPosition = new Vector3(end.localPlace.x + 0.5f, end.localPlace.y + 0.5f, 0f);
        line.positionCount = 2;
        line.SetPosition(0, startTransform.localPosition);
        line.SetPosition(1, targetTransform.localPosition);

        float angle = Mathf.Atan2(targetTransform.localPosition.y - startTransform.localPosition.y,
            targetTransform.localPosition.x - startTransform.localPosition.x) * Mathf.Rad2Deg;

        lineSprite.transform.eulerAngles = new Vector3(line.transform.rotation.x, line.transform.rotation.y, angle);
        lineSpriteFill.transform.eulerAngles = new Vector3(line.transform.rotation.x, line.transform.rotation.y, angle);

        float distance = Vector2.Distance(startTransform.localPosition, targetTransform.localPosition);
        Debug.LogWarning(distance);
        lineSprite.transform.localPosition = new Vector2(startTransform.localPosition.x, startTransform.localPosition.y);
        lineSpriteFill.transform.localPosition = new Vector2(startTransform.localPosition.x, startTransform.localPosition.y);

        lineSprite.size = new Vector2(distance, lineSprite.size.y);
        //lineSprite.transform.localScale = new Vector2(distance/7.04f, 0.03f);
        lineSpriteFill.transform.localScale = new Vector2(0f, 0.03f);

        targetXScale = lineSprite.transform.localScale.x;
        
        ticksLeft = InteractionManager.Character_Action_Delay;
        UpdateVisibility();

        AddListeners();
        //SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddDays(5), () => DestroyLine());
    }
    private void UpdateVisibility() {
        if ((UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter.id == owner.id)) {
            this.gameObject.SetActive(true);
        } else {
            this.gameObject.SetActive(false);
        }
    }
    private void FillProgress() {
        if (ticksLeft == 0) {
            lineSpriteFill.transform.localScale = new Vector2(targetXScale, 0.03f);
            DestroyLine();
        } else {
            lineSpriteFill.transform.localScale = new Vector2(targetXScale/(float)ticksLeft, 0.03f);
        }
        ticksLeft--;        
    }
    private void DestroyLine() {
        RemoveListeners();
        if (start.objHere == owner) {
            //areaMap.objectsTilemap.SetTile(start.localPlace, null);
            //if (start.prefabHere != null) {
            //    ObjectPoolManager.Instance.DestroyObject(start.prefabHere);
            //}
        }
        ObjectPoolManager.Instance.DestroyObject(this);
    }

    #region Listeners
    private void AddListeners() {
        Messenger.AddListener(Signals.INSPECT_ALL, OnInspectAll);
        Messenger.AddListener(Signals.TICK_STARTED, FillProgress);
        Messenger.AddListener<InfoUIBase>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<InfoUIBase>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    }
    private void RemoveListeners() {
        Messenger.RemoveListener(Signals.INSPECT_ALL, OnInspectAll);
        Messenger.RemoveListener(Signals.TICK_STARTED, FillProgress);
        Messenger.RemoveListener<InfoUIBase>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.RemoveListener<InfoUIBase>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    }
    private void OnCharacterDied(Character character) {
        if (owner.id == character.id) {
            DestroyLine();
        }
    }
    private void OnInspectAll() {
        UpdateVisibility();
    }
    private void OnMenuOpened(InfoUIBase @base) {
        if (@base is CharacterInfoUI) {
            UpdateVisibility();
        }
    }
    private void OnMenuClosed(InfoUIBase @base) {
        if (@base is CharacterInfoUI) {
            UpdateVisibility();
        }
    }
    #endregion
}
