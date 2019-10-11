﻿using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This is the base class for all nameplates, regardless of what type of nameplate it is.
/// </summary>
public class NameplateItem<T> : PooledObject {

    [Header("Main Content")]
    [SerializeField] protected TextMeshProUGUI mainLbl;
    [SerializeField] protected TextMeshProUGUI subLbl;
    [SerializeField] protected TextMeshProUGUI supportingLbl;
    [SerializeField] protected RectTransform supportingLblRT;
    [SerializeField] protected RectTransform supportingLblContainer;

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

    //Toggle Delegates
    public delegate void OnToggleNameplate(T obj, bool isOn);
    private OnToggleNameplate onToggleNameplate;

    //Signals
    private Dictionary<string, System.Delegate> signals; //signals that this nameplate is subscribed to. This is null by default

    public virtual T obj { get; private set; }

    #region Virtuals
    public virtual void SetObject(T o) {
        obj = o;
    }
    #endregion

    #region Object Pooling
    public override void Reset() {
        base.Reset();
        onHoverEnterNameplate = null;
        onHoverExitNameplate = null;
        onClickNameplate = null;
        onToggleNameplate = null;
        toggle.isOn = false;
        obj = default(T);
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
    public void OnHoverEnter() {
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
    public void OnHoverExit() {
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
    public void RemoveOnClickAction(OnClickNameplate action) {
        onClickNameplate -= action;
    }
    public void ClearAllOnClickActions() {
        onClickNameplate = null;
    }
    /// <summary>
    /// This is called to invoke all click actions.
    /// </summary>
    public void OnClick() {
        onClickNameplate?.Invoke(obj);
    }
    #endregion

    #region Toggle Funtions
    /// <summary>
    /// This will make the nameplate act as a toggle.
    /// </summary>
    public void SetAsToggle() {
        button.gameObject.SetActive(false);
        toggle.gameObject.SetActive(true);
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
    public void SetToggleGroup(ToggleGroup group) {
        toggle.group = group;
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
        supportingLblContainer.gameObject.SetActive(state);
    }
    #endregion
}
