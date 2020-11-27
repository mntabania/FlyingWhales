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
        Profiler.BeginSample("Show Small Info Sample");
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
        Profiler.EndSample();
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
            if (gameObject.activeInHierarchy) {
                StartCoroutine(ReLayout(smallInfoBGParentLG));
                StartCoroutine(ReLayout(smallInfoVerticalLG));    
            }
        }
    }
    private IEnumerator ReLayout(LayoutGroup layoutGroup) {
        layoutGroup.enabled = false;
        yield return null;
        layoutGroup.enabled = true;
    }
    
    public void PositionTooltip(GameObject tooltipParent, RectTransform rtToReposition, RectTransform boundsRT) {
        PositionTooltip(Input.mousePosition, tooltipParent, rtToReposition, boundsRT);
    }
    public void PositionTooltip(Vector3 position, GameObject tooltipParent, RectTransform rtToReposition, RectTransform boundsRT) {
        var v3 = position;

        rtToReposition.pivot = new Vector2(0f, 1f);
        smallInfoBGParentLG.childAlignment = TextAnchor.UpperLeft;

        if (InputManager.Instance.currentCursorType == InputManager.Cursor_Type.Cross 
            || InputManager.Instance.currentCursorType == InputManager.Cursor_Type.Check 
            || InputManager.Instance.currentCursorType == InputManager.Cursor_Type.Link) {
            v3.x += 100f;
            v3.y -= 32f;
        } else {
            v3.x += 25f;
            v3.y -= 25f;
        }
        
        tooltipParent.transform.position = v3;

        if (rtToReposition.sizeDelta.y >= Screen.height) {
            return;
        }

        Vector3[] corners = new Vector3[4]; //bottom-left, top-left, top-right, bottom-right
        List<int> cornersOutside = new List<int>();
        boundsRT.GetWorldCorners(corners);
        for (int i = 0; i < 4; i++) {
            Vector3 localSpacePoint = mainRT.InverseTransformPoint(corners[i]);
            // If parent (canvas) does not contain checked items any point
            if (!mainRT.rect.Contains(localSpacePoint)) {
                cornersOutside.Add(i);
            }
        }

        if (cornersOutside.Count != 0) {
            if (cornersOutside.Contains(2) && cornersOutside.Contains(3)) {
                if (cornersOutside.Contains(0)) {
                    //bottom side and right side are outside, move anchor to bottom right
                    rtToReposition.pivot = new Vector2(1f, 0f);
                    smallInfoBGParentLG.childAlignment = TextAnchor.LowerRight;
                } else {
                    //right side is outside, move anchor to top right side
                    rtToReposition.pivot = new Vector2(1f, 1f);
                    smallInfoBGParentLG.childAlignment = TextAnchor.UpperRight;
                }
            } else if (cornersOutside.Contains(0) && cornersOutside.Contains(3)) {
                //bottom side is outside, move anchor to bottom left
                rtToReposition.pivot = new Vector2(0f, 0f);
                smallInfoBGParentLG.childAlignment = TextAnchor.LowerLeft;
            }
            rtToReposition.localPosition = Vector3.zero;
        }
    }
    public void PositionTooltip(UIHoverPosition position, GameObject tooltipParent, RectTransform rt) {
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
    private bool IsSmallInfoShowing() {
        return (smallInfoGO != null && smallInfoGO.activeSelf);
    }
    public void HideSmallInfo() {
        if (IsSmallInfoShowing()) {
            smallInfoGO.SetActive(false);
        }
    }
}
