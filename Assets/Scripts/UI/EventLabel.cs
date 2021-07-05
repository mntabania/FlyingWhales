using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Ruinarch;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events;

public class EventLabel : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler{

	[SerializeField] private TextMeshProUGUI text;
    [SerializeField] private bool allowClickAction = true;
    [SerializeField] private EventLabelHoverAction hoverAction;
    [SerializeField] private UnityEvent hoverOutAction;
    private Log log;

    private int lastHoveredLinkIndex = -1;
    private bool isHighlighting;
    private bool _shouldColorHighlight = true;

    public System.Func<object, bool> shouldBeHighlightedChecker;
    public System.Action<object> onLeftClickAction;
    public System.Action<object> onRightClickAction;

    [SerializeField] protected bool isHovering;
    [SerializeField] private bool wasHoveringPreviousFrame = false;

    //cached this so as not to create a new array everytime this is hovered/clicked. This is used for splitting words in linkText
    private static char[] linkTextSeparators = new[] {'|'}; 
    
    private void Awake() {
        if (text == null) {
            text = gameObject.GetComponent<TextMeshProUGUI>();
        }
    }
    void Update() {
        wasHoveringPreviousFrame = isHovering;
        if (isHovering) {
            HoveringAction();
        } else {
            if (wasHoveringPreviousFrame) {
                OnPointerExit(null);
            }
        }
    }
    void OnDisable() {
        ResetHighlightValues();
        wasHoveringPreviousFrame = false;
    }

    public void ResetHighlightValues() {
        if (lastHoveredLinkIndex != -1 && isHighlighting) {
            UnhighlightLink(text.textInfo.linkInfo[lastHoveredLinkIndex]);
        }
        //else {
        //    CursorManager.Instance.RevertToPreviousCursor();
        //}
        lastHoveredLinkIndex = -1;
        isHighlighting = false;
    }
    public void OnPointerClick(PointerEventData eventData) {
        if (!allowClickAction) {
            return;
        }
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
        if (linkIndex != -1) {
            TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
            object obj = null;
            string linkText = linkInfo.GetLinkID();
            //check first if linkText is just a number, if it is then assume that the object is an integer.
            if (int.TryParse(linkText, out var integer)) {
                obj = linkText;
            } else {
                //linkText is NOT an integer, check if it follows the logFiller format (Type_persistentID)
                if (linkText.Contains("|")) {
                    string[] words = linkText.Split(linkTextSeparators);
                    if (words.Length == 2) {
                        System.Type type = System.Type.GetType(words[0]);
                        string persistentID = words[1];
                        if (type != null && !string.IsNullOrEmpty(persistentID)) {
                            //linkText follows required format for logFiller, info can be used to determine object.
                            obj = DatabaseManager.Instance.GetObjectFromDatabase(type, persistentID);
                        }
                    }
                } else { 
                    //link text does not follow log filler format, just assign the given text
                    obj = linkText;
                }
            }
            if (eventData.button == PointerEventData.InputButton.Left) {
                if (onLeftClickAction != null) {
                    if (obj != null) {
                        onLeftClickAction.Invoke(obj);
                        Messenger.Broadcast(UISignals.EVENT_LABEL_LINK_CLICKED, this);
                    }
                } else {
                    if(obj != null) {
                        UIManager.Instance.OpenObjectUI(obj);
                    }
                }    
            } else if (eventData.button == PointerEventData.InputButton.Right) {
                if (obj != null) {
                    onRightClickAction?.Invoke(obj);
                }
            }
            
            ResetHighlightValues();
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData) {
        if (!allowClickAction) {
            return;
        }
        isHovering = true;
    }
    public void OnPointerExit(PointerEventData eventData) {
        if (!allowClickAction) {
            return;
        }
        isHovering = false;
        HoverOutAction();
    }
    public void SetHighlightChecker(System.Func<object, bool> shouldBeHighlightedChecker) {
        this.shouldBeHighlightedChecker = shouldBeHighlightedChecker;
    }
    public void SetShouldColorHighlight(bool state) {
        _shouldColorHighlight = state;
    }
    private bool ShouldBeHighlighted(object obj) {
        if (shouldBeHighlightedChecker != null) {
            return shouldBeHighlightedChecker.Invoke(obj);
        }
        return true; //default is highlighted
    }
    private void HoveringAction() {
        bool executeHoverOutAction = true;
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
        if (lastHoveredLinkIndex != -1 && lastHoveredLinkIndex != linkIndex && isHighlighting) {
            UnhighlightLink(text.textInfo.linkInfo[lastHoveredLinkIndex]);
        }
        
        if (linkIndex != -1) {
            TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];

            string linkText = linkInfo.GetLinkID();
            object obj = null;
            //check first if linkText is just a number, if it is then assume that the object is an integer.
            if (int.TryParse(linkText, out var integer)) {
                obj = linkText;
            } else {
                //linkText is NOT an integer, check if it follows the logFiller format (Type_persistentID)
                if (linkText.Contains("|")) {
                    string[] words = linkText.Split(linkTextSeparators);
                    if (words.Length == 2) {
                        System.Type type = System.Type.GetType(words[0]);
                        string persistentID = words[1];
                        if (type != null && !string.IsNullOrEmpty(persistentID)) {
                            //linkText follows required format for logFiller, info can be used to determine object.
                            obj = DatabaseManager.Instance.GetObjectFromDatabase(type, persistentID);
                        }
                    }
                } else {
                    //link text does not follow log filler format, just assign the given text
                    obj = linkText;
                }
            }
            if (obj != null) {
                if (ShouldBeHighlighted(obj)) {
                    if (lastHoveredLinkIndex != linkIndex) {
                        //only highlight if last index is different
                        HighlightLink(linkInfo);
                        isHighlighting = true;
                    }
                } else {
                    isHighlighting = false;
                }
                hoverAction?.Invoke(obj);
                executeHoverOutAction = false;
            }
        } 
        lastHoveredLinkIndex = linkIndex;
        if (hoverOutAction != null && executeHoverOutAction) {
            hoverOutAction?.Invoke();
        }
    }

    private Color32 originalColor;
    private void HighlightLink(TMP_LinkInfo linkInfo) {
        // string oldText = $"{linkInfo.GetLinkText()}";
        // string newText = $"<u>{oldText}</u>";
        // text.SetText(text.text.Replace(oldText, newText));

        if (_shouldColorHighlight) {
            // Iterate through each of the characters of the word.
            for (int i = 0; i < linkInfo.linkTextLength; i++) {
                int characterIndex = linkInfo.linkTextfirstCharacterIndex + i;

                // Get the index of the material / sub text object used by this character.
                TMP_CharacterInfo characterInfo = text.textInfo.characterInfo[characterIndex];
                int meshIndex = characterInfo.materialReferenceIndex;
                int vertexIndex = characterInfo.vertexIndex;

                if (!char.IsWhiteSpace(characterInfo.character)) {
                    // Get a reference to the vertex color
                    Color32[] vertexColors = text.textInfo.meshInfo[meshIndex].colors32;
                    if (vertexColors.Length > vertexIndex + 3) {
                        originalColor = vertexColors[vertexIndex];
                        vertexColors[vertexIndex] = Color.white;
                        vertexColors[vertexIndex + 1] = Color.white;
                        vertexColors[vertexIndex + 2] = Color.white;
                        vertexColors[vertexIndex + 3] = Color.white;
                    }
                }
            }

            // Update Geometry
            text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);    
        }

        InputManager.Instance?.SetCursorTo(InputManager.Cursor_Type.Link);
    }
    private void UnhighlightLink(TMP_LinkInfo linkInfo) {
        // string oldText = $"{linkInfo.GetLinkText()}";
        // string newText = $"<u>{oldText}</u>";
        // text.SetText(text.text.Replace(newText, oldText));

        if (_shouldColorHighlight) {
            // Iterate through each of the characters of the word.
            for (int i = 0; i < linkInfo.linkTextLength; i++) {
                int characterIndex = linkInfo.linkTextfirstCharacterIndex + i;

                // Get the index of the material / sub text object used by this character.
                if (text.textInfo.characterInfo.IsIndexInArray(characterIndex)) {
                    TMP_CharacterInfo characterInfo = text.textInfo.characterInfo[characterIndex];
                    int meshIndex = characterInfo.materialReferenceIndex;
                    int vertexIndex = characterInfo.vertexIndex;

                    if (!char.IsWhiteSpace(characterInfo.character)) {
                        // Get a reference to the vertex color
                        Color32[] vertexColors = text.textInfo.meshInfo[meshIndex].colors32;
                        if (vertexColors.Length >  vertexIndex + 3) {
                            Color32 c = originalColor;
                            if (vertexColors.IsIndexInArray(vertexIndex)) {
                                vertexColors[vertexIndex] = c;    
                            }
                            if (vertexColors.IsIndexInArray(vertexIndex + 1)) {
                                vertexColors[vertexIndex + 1] = c;    
                            }
                            if (vertexColors.IsIndexInArray(vertexIndex + 2)) {
                                vertexColors[vertexIndex + 2] = c;    
                            }
                            if (vertexColors.IsIndexInArray(vertexIndex + 3)) {
                                vertexColors[vertexIndex + 3] = c;    
                            }
                            // vertexColors[vertexIndex] = c;
                            // vertexColors[vertexIndex + 1] = c;
                            // vertexColors[vertexIndex + 2] = c;
                            // vertexColors[vertexIndex + 3] = c;
                        }
                    }    
                }
            }

            // Update Geometry
            text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }
        
        InputManager.Instance?.RevertToPreviousCursor();
    }
    public void HoverOutAction() {
        //if (hoverOutAction == null) {
        //    return;
        //}
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
        // if (linkIndex == -1) {
            hoverOutAction?.Invoke();
        // }
        ResetHighlightValues();
    }
    public void SetOnLeftClickAction(System.Action<object> onClickAction) {
        this.onLeftClickAction = onClickAction;
    }
    public void SetOnRightClickAction(System.Action<object> onRightClickAction) {
        this.onRightClickAction = onRightClickAction;
    }
}

[System.Serializable]
public class EventLabelHoverAction : UnityEvent<object> { }
