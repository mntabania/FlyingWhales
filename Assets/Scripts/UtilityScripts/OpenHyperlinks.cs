using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TextMeshProUGUI))]
public class OpenHyperlinks : MonoBehaviour, IPointerClickHandler {

    private TextMeshProUGUI _tmpPro;

    void Start() {
        _tmpPro = gameObject.GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerClick(PointerEventData eventData) {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(_tmpPro, Input.mousePosition, null);
        if (linkIndex != -1) { // was a link clicked?
            TMP_LinkInfo linkInfo = _tmpPro.textInfo.linkInfo[linkIndex];

            // open the link id as a url, which is the metadata we added in the text field
            Debug.Log(linkInfo.GetLinkID());
            Application.OpenURL(linkInfo.GetLinkID());
        }
    }
}