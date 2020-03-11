using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UnderlineHyperlink : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    protected bool isHovering;
    protected bool hasBeenUnderlined;

    protected TextMeshProUGUI tmPro;
    protected int linkID;

    private void OnEnable() {
        tmPro = this.GetComponent<TextMeshProUGUI>();
        hasBeenUnderlined = false;
    }
    private void OnDisable() {
        isHovering = false;
    }
    public void OnPointerEnter(PointerEventData eventData) {
        if (tmPro != null) {
            isHovering = true;
        }
    }
    public void OnPointerExit(PointerEventData eventData) {
        if (tmPro != null) {
            isHovering = false;
            //RemoveUnderline();
        }
    }

    void Update() {
        if (isHovering) {
            UnderlineTextWithObject();
        }
    }

    private void UnderlineTextWithObject() {
        //if (hasBeenUnderlined) { return; }
        linkID = TMP_TextUtilities.FindIntersectingLink(tmPro, Input.mousePosition, null);
        if (linkID != -1) {
            hasBeenUnderlined = true;
            //this means that the hovered text has an object
            TMP_LinkInfo linkInfo = tmPro.textInfo.linkInfo[linkID];
            //int startIndex = linkInfo.linkTextfirstCharacterIndex;
            //int lastIndex = linkInfo.linkTextfirstCharacterIndex + linkInfo.linkTextLength;
            for (int i = 0; i < linkInfo.linkTextLength; i++) {
                tmPro.textInfo.characterInfo[linkInfo.linkTextfirstCharacterIndex + i].style = FontStyles.Underline;
                Debug.LogWarning(
                    $"CHAR: {tmPro.textInfo.characterInfo[linkInfo.linkTextfirstCharacterIndex + i].character}");
                Debug.LogWarning(
                    $"STYLE: {tmPro.textInfo.characterInfo[linkInfo.linkTextfirstCharacterIndex + i].style}");
            }


            //string result = wordInfo.get.Insert(lastIndex, "</u>");
            //result = result.Insert(startIndex, "<u>");
            //tmPro.text = result;
        }
    }
    private void RemoveUnderline() {
        hasBeenUnderlined = false;
        if (linkID != -1) {
            //this means that the hovered text has an object
            TMP_LinkInfo linkInfo = tmPro.textInfo.linkInfo[linkID];
            int startIndex = linkInfo.linkTextfirstCharacterIndex;
            int lastIndex = linkInfo.linkTextfirstCharacterIndex + linkInfo.linkTextLength;
            string result = tmPro.text.Remove(lastIndex, 4);
            result = result.Remove(startIndex - 3, 3);
            tmPro.text = result;
        }
    }
}
