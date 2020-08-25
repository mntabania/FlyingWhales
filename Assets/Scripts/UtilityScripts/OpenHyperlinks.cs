using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Linq;
using Ruinarch;

[RequireComponent(typeof(TextMeshProUGUI))]
public class OpenHyperlinks : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {

    private TextMeshProUGUI _tmpPro;

    private int lastHoveredLinkIndex;
    //private Color32 originalColor;
    private bool isHovering;

    void Start() {
        _tmpPro = gameObject.GetComponent<TextMeshProUGUI>();
    }
    void Update() {
        if (isHovering) {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(_tmpPro, Input.mousePosition, null);
            if (linkIndex != -1) {
                InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Link);
            } else {
                InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
            }
        }
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

    public void OnPointerEnter(PointerEventData eventData) {
        isHovering = true;
        //int linkIndex = TMP_TextUtilities.FindIntersectingLink(_tmpPro, Input.mousePosition, null);
        //if (linkIndex != -1) { // was a link clicked?
        //    InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Link);

        //    lastHoveredLinkIndex = linkIndex;
        //    //TMP_LinkInfo linkInfo = _tmpPro.textInfo.linkInfo[linkIndex];
        //    //originalColor = SetLinkToColor(linkInfo, Color.green);
        //}
    }
    public void OnPointerExit(PointerEventData eventData) {
        isHovering = false;
        //if (lastHoveredLinkIndex != -1) { // was a link clicked?
        //    lastHoveredLinkIndex = -1;
        //    InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);

        //    //TMP_LinkInfo linkInfo = _tmpPro.textInfo.linkInfo[lastHoveredLinkIndex];
        //    //SetLinkToColor(linkInfo, originalColor);
        //}
    }
    private Color32 SetLinkToColor(TMP_LinkInfo linkInfo, Color32 color) {
        //List<Color32[]> oldVertColors = new List<Color32[]>(); // store the old character colors
        Color32 oldColor = Color.white;
        for (int i = 0; i < linkInfo.linkTextLength; i++) { // for each character in the link string
            int characterIndex = linkInfo.linkTextfirstCharacterIndex + i; // the character index into the entire text
            var charInfo = _tmpPro.textInfo.characterInfo[characterIndex];
            int meshIndex = charInfo.materialReferenceIndex; // Get the index of the material / sub text object used by this character.
            int vertexIndex = charInfo.vertexIndex; // Get the index of the first vertex of this character.

            Color32[] vertexColors = _tmpPro.textInfo.meshInfo[meshIndex].colors32; // the colors for this character
            
            if(oldColor == Color.white) {
                oldColor = vertexColors[0];
            }

            if (charInfo.isVisible) {
                vertexColors[vertexIndex + 0] = color;
                vertexColors[vertexIndex + 1] = color;
                vertexColors[vertexIndex + 2] = color;
                vertexColors[vertexIndex + 3] = color;
            }
        }

        // Update Geometry
        _tmpPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

        return oldColor;
    }
}