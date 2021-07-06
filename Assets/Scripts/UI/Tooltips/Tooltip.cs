using System;
using System.Collections;
using System.Collections.Generic;
using Ruinarch;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour {
    public static Tooltip Instance;
    
    public RectTransform mainRT;
    public Canvas canvas;
    public GameObject smallInfoGO;
    public RectTransform smallInfoRT;
    public HorizontalLayoutGroup smallInfoBGParentLG;
    public VerticalLayoutGroup smallInfoVerticalLG;
    public RectTransform smallInfoBGRT;
    public RuinarchText smallInfoLbl;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
    public void ShowSmallInfo(string info, string header = "", bool autoReplaceText = true) {
        string message = string.Empty;
        if (!string.IsNullOrEmpty(header)) {
            message = $"<font=\"Eczar-Medium\"><line-height=100%><size=18>{header}</font>\n";
        }
        message = $"{message}<line-height=70%><size=16>{info}";

        message = message.Replace("\\n", "\n");

        if (autoReplaceText) {
            smallInfoLbl.SetTextAndReplaceWithIcons(message);    
        } else {
            smallInfoLbl.text = message;
        }
        if (!IsSmallInfoShowing()) {
            smallInfoGO.transform.SetParent(this.transform);
            smallInfoGO.SetActive(true);
            if (gameObject.activeInHierarchy) {
                StartCoroutine(ReLayout(smallInfoBGParentLG));
                StartCoroutine(ReLayout(smallInfoVerticalLG));    
            }
        }
        PositionTooltip(smallInfoGO, smallInfoRT, smallInfoBGRT);
    }
    public void ShowSmallInfo(string info, UIHoverPosition pos, string header = "", bool autoReplaceText = true) {
        string message = string.Empty;
        if (!string.IsNullOrEmpty(header)) {
            message = $"<font=\"Eczar-Medium\"><line-height=100%><size=18>{header}</font>\n";
        }
        message = $"{message}<line-height=70%><size=16>{info}";

        message = message.Replace("\\n", "\n");

        if (autoReplaceText) {
            smallInfoLbl.SetTextAndReplaceWithIcons(message);    
        } else {
            smallInfoLbl.text = message;
        }
        
        PositionTooltip(pos, smallInfoGO, smallInfoRT);
        
        if (!IsSmallInfoShowing()) {
            smallInfoGO.SetActive(true);
            // if (gameObject.activeInHierarchy) {
            //     StartCoroutine(ReLayout(smallInfoBGParentLG));
            //     StartCoroutine(ReLayout(smallInfoVerticalLG));    
            // }
        }
    }
    private IEnumerator ReLayout(LayoutGroup layoutGroup) {
        layoutGroup.enabled = false;
        yield return null;
        layoutGroup.enabled = true;
    }
    private void PositionTooltip(GameObject tooltipParent, RectTransform rtToReposition, RectTransform boundsRT) {
        PositionTooltip(Input.mousePosition, tooltipParent, rtToReposition, boundsRT);
    }
    private void PositionTooltip(Vector3 position, GameObject tooltipParent, RectTransform rtToReposition, RectTransform boundsRT) {
        Vector3 v3 = position;
        
        if (tooltipParent.transform.parent != mainRT) {
            tooltipParent.transform.SetParent(mainRT);    
        }
        if (tooltipParent.transform.localScale != Vector3.one) {
            tooltipParent.transform.localScale = Vector3.one;    
        }

        rtToReposition.pivot = new Vector2(0f, 1f);
        RectTransform tooltipParentRT = tooltipParent.transform as RectTransform;
        tooltipParentRT.pivot = new Vector2(0f, 0f);

        UtilityScripts.Utilities.GetAnchorMinMax(TextAnchor.LowerLeft, out var anchorMin, out var anchorMax);
        tooltipParentRT.anchorMin = anchorMin;
        tooltipParentRT.anchorMax = anchorMax;

        smallInfoBGParentLG.childAlignment = TextAnchor.UpperLeft;

        if (InputManager.Instance != null) {
            if (InputManager.Instance.currentCursorType == InputManager.Cursor_Type.Cross 
                || InputManager.Instance.currentCursorType == InputManager.Cursor_Type.Check 
                || InputManager.Instance.currentCursorType == InputManager.Cursor_Type.Link) {
                v3.x += 100f;
                v3.y -= 32f;
            } else {
                v3.x += 25f;
                v3.y -= 25f;
            }    
        }

        Vector3 clampedPos = KeepFullyOnScreen(smallInfoBGRT, v3, canvas, mainRT);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mainRT, clampedPos, null, out var localPoint);
        
        (tooltipParent.transform as RectTransform).localPosition = localPoint; //clampedPos;
    }
     private Vector3 KeepFullyOnScreen(RectTransform rect, Vector3 newPos, Canvas canvas, RectTransform CanvasRect) {
         float minX = 0f;
         var scaleFactor = canvas.scaleFactor;
         float maxX = ((CanvasRect.sizeDelta.x * scaleFactor) - (rect.sizeDelta.x * scaleFactor));
         float minY = rect.sizeDelta.y * scaleFactor;
         float maxY = CanvasRect.sizeDelta.y * scaleFactor;
        
         newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
         newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
        
         return newPos;
     }
    // private void PositionTooltip(Vector3 position, GameObject tooltipParent, RectTransform rtToReposition, RectTransform boundsRT) {
    //     var v3 = position;
    //
    //     rtToReposition.pivot = new Vector2(0f, 1f);
    //     RectTransform tooltipParentRT = tooltipParent.transform as RectTransform;
    //     tooltipParentRT.pivot = new Vector2(0f, 0f);
    //
    //     UtilityScripts.Utilities.GetAnchorMinMax(TextAnchor.LowerLeft, out var anchorMin, out var anchorMax);
    //     tooltipParentRT.anchorMin = anchorMin;
    //     tooltipParentRT.anchorMax = anchorMax;
    //     
    //     smallInfoBGParentLG.childAlignment = TextAnchor.UpperLeft;
    //
    //     if (InputManager.Instance != null) {
    //         if (InputManager.Instance.currentCursorType == InputManager.Cursor_Type.Cross 
    //             || InputManager.Instance.currentCursorType == InputManager.Cursor_Type.Check 
    //             || InputManager.Instance.currentCursorType == InputManager.Cursor_Type.Link) {
    //             v3.x += 100f;
    //             v3.y -= 32f;
    //         } else {
    //             v3.x += 25f;
    //             v3.y -= 25f;
    //         }    
    //     }
    //
    //
    //     Vector3 clampedPos = KeepFullyOnScreen(smallInfoBGRT, v3, mainRT);
    //     (tooltipParent.transform as RectTransform).anchoredPosition = clampedPos;
    // }
    // private Vector3 KeepFullyOnScreen(RectTransform rect, Vector3 newPos, RectTransform CanvasRect) {
    //     float minX = 0f;
    //     float maxX = (CanvasRect.sizeDelta.x - rect.sizeDelta.x); //* 0.5f;
    //     float minY = rect.sizeDelta.y; //* -0.5f;
    //     float maxY = CanvasRect.sizeDelta.y; //* 0.5f;
    //     
    //     newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
    //     newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
    //     
    //     return newPos;
    // }
    private void PositionTooltip(UIHoverPosition position, GameObject tooltipParent, RectTransform rt) {
        tooltipParent.transform.SetParent(position.transform);
        RectTransform tooltipParentRT = tooltipParent.transform as RectTransform;
        tooltipParentRT.pivot = position.pivot;

        UtilityScripts.Utilities.GetAnchorMinMax(position.anchor, out var anchorMin, out var anchorMax);
        tooltipParentRT.anchorMin = anchorMin;
        tooltipParentRT.anchorMax = anchorMax;
        tooltipParentRT.anchoredPosition = Vector2.zero;

        smallInfoBGParentLG.childAlignment = position.anchor;
        rt.pivot = position.pivot;
    }
    public bool IsSmallInfoShowing() {
        return (smallInfoGO != null && smallInfoGO.activeSelf);
    }
    public void HideSmallInfo() {
        if (IsSmallInfoShowing()) {
            smallInfoGO.SetActive(false);
        }
    }
}
