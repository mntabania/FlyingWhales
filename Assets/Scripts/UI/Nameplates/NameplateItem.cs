using System;
using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// This is the base class for all nameplates, regardless of what type of nameplate it is.
/// </summary>
public class NameplateItem<T> : PooledObject, INameplateItem {

    [Header("Main Content")]
    [SerializeField] protected TextMeshProUGUI mainLbl;
    [SerializeField] protected TextMeshProUGUI subLbl;
    [SerializeField] protected TextMeshProUGUI supportingLbl;
    [SerializeField] protected RectTransform supportingLblRT;
    [SerializeField] protected RectTransform supportingLblContainer;
    [SerializeField] protected NameplateButton nameplateButton;

    [Header("Button")]
    [SerializeField] protected Button button;

    [Header("Toggle")]
    [SerializeField] protected Toggle toggle;

    [Header("Cover")]
    [SerializeField] private GameObject coverGO;   

    //Hover Delegates
    public delegate void OnHoverEnterNameplate(T obj);
    public delegate void OnHoverExitNameplate(T obj);
    private OnHoverEnterNameplate onHoverEnterNameplate; //actions to be executed when the nameplate is hovered over.
    private OnHoverExitNameplate onHoverExitNameplate; //actions to be executed when the nameplate is hovered out.

    //Button Click Delegates
    public delegate void OnClickNameplate(T obj);
    private OnClickNameplate onClickNameplate;

    public delegate void OnRightClickNameplate(T obj);
    private OnRightClickNameplate onRightClickNameplate;

    //Toggle Delegates
    public delegate void OnToggleNameplate(T obj, bool isOn);
    private OnToggleNameplate onToggleNameplate;

    //Signals
    private Dictionary<string, System.Delegate> signals; //signals that this nameplate is subscribed to. This is null by default

    public virtual T obj { get; private set; }

    public bool coverState {
        get { return coverGO.activeSelf; }
    }

    #region Virtuals
    public virtual void SetObject(T o) {
        obj = o;
        nameplateButton.SetNameplateItem(this);
    }
    public virtual void UpdateObject(T o) {
        obj = o;
        nameplateButton.SetNameplateItem(this);
    }
    #endregion

    #region Object Pooling
    public override void Reset() {
        base.Reset();
        onHoverEnterNameplate = null;
        onHoverExitNameplate = null;
        onClickNameplate = null;
        onRightClickNameplate = null;
        onToggleNameplate = null;
        toggle.isOn = false;
        obj = default(T);
        SetToggleGroup(null);
        SetInteractableState(true);
    }
    #endregion

    #region Hover Enter Actions
    public void AddHoverEnterAction(OnHoverEnterNameplate action) {
        onHoverEnterNameplate += action;
    }
    public void RemoveHoverEnterAction(OnHoverEnterNameplate action) {
        onHoverEnterNameplate -= action;
    }
    public void ClearAllHoverEnterActions() {
        onHoverEnterNameplate = null;
    }
    /// <summary>
    /// This is called to invoke all hover enter actions.
    /// </summary>
    public virtual void OnHoverEnter() {
        onHoverEnterNameplate?.Invoke(obj);
    }
    #endregion

    #region Hover Exit Actions
    public void AddHoverExitAction(OnHoverExitNameplate action) {
        onHoverExitNameplate += action;
    }
    public void RemoveHoverExitAction(OnHoverExitNameplate action) {
        onHoverExitNameplate -= action;
    }
    public void ClearAllHoverExitActions() {
        onHoverExitNameplate = null;
    }
    /// <summary>
    /// This is called to invoke all hover exit actions.
    /// </summary>
    public virtual void OnHoverExit() {
        onHoverExitNameplate?.Invoke(obj);
    }
    #endregion

    #region Button Functions
    /// <summary>
    /// This will make the nameplate act as a button.
    /// </summary>
    public void SetAsButton() {
        button.gameObject.SetActive(true);
        toggle.gameObject.SetActive(false);
    }
    public void AddOnClickAction(OnClickNameplate action) {
        onClickNameplate += action;
    }
    public void AddOnRightClickAction(OnRightClickNameplate action) {
        onRightClickNameplate += action;
    }
    public void RemoveOnClickAction(OnClickNameplate action) {
        onClickNameplate -= action;
    }
    public void RemoveOnRightClickAction(OnRightClickNameplate action) {
        onRightClickNameplate -= action;
    }
    public void ClearAllOnClickActions() {
        onClickNameplate = null;
        onRightClickNameplate = null;
    }

    //public void OnClick() {
    //    onClickNameplate?.Invoke(obj);
    //    Messenger.Broadcast(Signals.NAMEPLATE_CLICKED, mainLbl.text);
    //}

    /// <summary>
    /// This is called to invoke all click actions.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData) {
        if (button.gameObject.activeSelf) {
            if(eventData.button == PointerEventData.InputButton.Left) {
                onClickNameplate?.Invoke(obj);
                Messenger.Broadcast(Signals.NAMEPLATE_CLICKED, mainLbl.text);
            } else if (eventData.button == PointerEventData.InputButton.Right) {
                onRightClickNameplate?.Invoke(obj);
            }
        }
    }
    #endregion

    #region Toggle Funtions
    /// <summary>
    /// This will make the nameplate act as a toggle.
    /// </summary>
    public void SetAsToggle() {
        button.gameObject.SetActive(false);
        toggle.gameObject.SetActive(true);
        toggle.isOn = false;
    }
    public void AddOnToggleAction(OnToggleNameplate action) {
        onToggleNameplate += action;
    }
    public void RemoveOnToggleAction(OnToggleNameplate action) {
        onToggleNameplate -= action;
    }
    public void ClearAllOnToggleActions() {
        onToggleNameplate = null;
    }
    /// <summary>
    /// This is called to invoke all on toggle actions.
    /// </summary>
    /// <param name="isOn">The state of the nameplate toggle</param>
    public void OnToggle(bool isOn) {
        onToggleNameplate?.Invoke(obj, isOn);
    }
    public void SetToggleState(bool isOn) {
        toggle.isOn = isOn;
    }
    /// <summary>
    /// Set the toggle group of this nameplate.
    /// NOTE: If you want the action of this toggle to execute when it turns on automatically
    /// because of setting it's group, make sure to set the action before calling this.
    /// </summary>
    /// <param name="group">The group that the toggle is in.</param>
    public void SetToggleGroup(ToggleGroup group) {
        ToggleGroup previous = toggle.group;
        toggle.group = group;
        previous?.UnregisterToggle(toggle);
        if (group != null) {
            group.RegisterToggle(toggle);
            if (!group.allowSwitchOff && !group.AnyTogglesOn()) {
                toggle.isOn = true; //if this toggle is set to a group that does not allow all toggles to be switched off, and currently all toggles in the group are switched off, then turn this toggle on.
                //toggle.onValueChanged.Invoke(toggle.isOn);
            }
        }
        
        
    }
    #endregion

    #region Utilities
    /// <summary>
    /// Set this nameplates interactability state. This will also affect the cover of the nameplate. (Setting the nameplate as un-interactable will activate the cover)
    /// </summary>
    /// <param name="state">The state the interactable will be set to.</param>
    public void SetInteractableState(bool state) {
        button.interactable = state;
        toggle.interactable = state;
        coverGO.SetActive(!state);
    }
    #endregion

    #region Supporting Text
    Coroutine scrollRoutine;
    public void ScrollText() {
        if (supportingLblRT.sizeDelta.x < supportingLblContainer.sizeDelta.x || scrollRoutine != null) {
            return;
        }
        scrollRoutine = StartCoroutine(Scroll());
    }
    public void StopScroll() {
        if (scrollRoutine != null) {
            StopCoroutine(scrollRoutine);
            scrollRoutine = null;
        }
        supportingLblRT.anchoredPosition = new Vector3(0f, supportingLblRT.anchoredPosition.y);
    }
    private IEnumerator Scroll() {
        float width = supportingLbl.preferredWidth;
        Vector3 startPosition = supportingLblRT.anchoredPosition;

        float difference = supportingLblContainer.sizeDelta.x - supportingLblRT.sizeDelta.x;

        float scrollDirection = -1f;

        while (true) {
            float newX = supportingLblRT.anchoredPosition.x + (0.5f * scrollDirection);
            supportingLblRT.anchoredPosition = new Vector3(newX, startPosition.y, startPosition.z);
            if (supportingLblRT.anchoredPosition.x < difference) {
                scrollDirection = 1f;
            } else if (supportingLblRT.anchoredPosition.x > 0) {
                scrollDirection = -1f;
            }
            yield return null;
        }
    }
    public void SetSupportingLabelState(bool state) {
        supportingLbl.gameObject.SetActive(state);
    }
    #endregion
}

public interface INameplateItem {
    //This is only for reference, so that we can reference the Generic Class of NameplateItem<T>, since we cannot declare Nameplate<T> in a variable
    void OnPointerClick(PointerEventData eventData);
}