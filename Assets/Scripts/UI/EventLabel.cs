using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
    public System.Action<object> onClickAction;

    [SerializeField] protected bool isHovering;
    [SerializeField] private bool wasHoveringPreviousFrame = false;

    protected Dictionary<string, object> objectDictionary;

    //cached this so as not to create a new array everytime this is hovered/clicked. This is used for splitting words in linkText
    private static char[] linkTextSeparators = new[] {'|'}; 
    
    private void Awake() {
        objectDictionary = new Dictionary<string, object>();
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
            
            // if (logItem == null) {
            //     string linkText = linkInfo.GetLinkID();
            //     if (!int.TryParse(linkText, out var idToUse)) {
            //         if (linkText.Contains("_") && linkText.Length > 1) {
            //             string id = linkText.Substring(0, linkText.IndexOf('_'));
            //             idToUse = int.Parse(id);
            //         }
            //     }
            //     if (linkText.Contains("_faction")) {
            //         Faction faction = FactionManager.Instance.GetFactionBasedOnID(idToUse);
            //         obj = faction;
            //     } else if (linkText.Contains("_character")) {
            //         Character character = CharacterManager.Instance.GetCharacterByID(idToUse);
            //         obj = character;
            //     } else if (linkText.Contains("_hextile")) {
            //         HexTile tile = GridMap.Instance.normalHexTiles[idToUse];
            //         obj = tile;
            //     }
            //     else {
            //         obj = linkInfo.GetLinkID();
            //     }
            // } else if (logItem.log != null) {
            //     string linkText = linkInfo.GetLinkID();
            //     if (!int.TryParse(linkText, out var idToUse)) {
            //         string id = linkText.Substring(0, linkText.IndexOf('_'));
            //         idToUse = int.Parse(id);
            //     }
            //     LogFiller lf = logItem.log.fillers[idToUse];
            //     obj = lf.obj;
            // }
            if (onClickAction != null) {
                if (obj != null) {
                    onClickAction.Invoke(obj);
                    Messenger.Broadcast(UISignals.EVENT_LABEL_LINK_CLICKED, this);
                }
            } else {
                if(obj != null) {
                    UIManager.Instance.OpenObjectUI(obj);
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
        objectDictionary.Clear();
        isHovering = false;
        HoverOutAction();
    }
    public void SetLog(Log log) {
        this.log = log;
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
    public void HoveringAction() {
        //if (hoverAction == null) {
        //    return;
        //}
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
            // if (log == null) {
            //     int idToUse;
            //     if (!int.TryParse(linkText, out idToUse)) {
            //         if (linkText.Contains("_") && linkText.Length > 1) {
            //             string id = linkText.Substring(0, linkText.IndexOf('_'));
            //             idToUse = int.Parse(id);
            //         }
            //     }
            //     if (objectDictionary.ContainsKey(linkText)) {
            //         obj = objectDictionary[linkText];
            //     } else {
            //         if (linkText.Contains("_faction")) {
            //             obj = FactionManager.Instance.GetFactionBasedOnID(idToUse);
            //         } else if (linkText.Contains("_character")) {
            //             obj = CharacterManager.Instance.GetCharacterByID(idToUse);
            //         } else if (linkText.Contains("_hextile")) {
            //             obj = GridMap.Instance.normalHexTiles[idToUse];
            //         } else {
            //             obj = linkText;
            //         }
            //         objectDictionary.Add(linkText, obj);
            //     }
            // } else {
            //     if (!int.TryParse(linkText, out var idToUse)) {
            //         string id = linkText.Substring(0, linkText.IndexOf('_'));
            //         idToUse = int.Parse(id);
            //     }
            //     if (objectDictionary.ContainsKey(linkText)) {
            //         obj = objectDictionary[linkText];
            //     } else {
            //         obj = log.fillers[idToUse].obj;
            //         objectDictionary.Add(linkText, obj);
            //     }
            // }
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
                int meshIndex = text.textInfo.characterInfo[characterIndex].materialReferenceIndex;

                int vertexIndex = text.textInfo.characterInfo[characterIndex].vertexIndex;

                // Get a reference to the vertex color
                Color32[] vertexColors = text.textInfo.meshInfo[meshIndex].colors32;
                if (vertexColors.Length > 0) {
                    originalColor = vertexColors[vertexIndex + 0];
            
                    Color32 c = Color.white; //vertexColors[vertexIndex + 0].Tint(0.75f);

                    vertexColors[vertexIndex + 0] = c;
                    vertexColors[vertexIndex + 1] = c;
                    vertexColors[vertexIndex + 2] = c;
                    vertexColors[vertexIndex + 3] = c;
                    
                }
            }

            // Update Geometry
            text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);    
        }

        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Link);
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
                int meshIndex = text.textInfo.characterInfo[characterIndex].materialReferenceIndex;

                int vertexIndex = text.textInfo.characterInfo[characterIndex].vertexIndex;

                // Get a reference to the vertex color
                Color32[] vertexColors = text.textInfo.meshInfo[meshIndex].colors32;

                if (vertexColors.Length > 0) {
                    Color32 c = originalColor; //vertexColors[vertexIndex + 0].Tint(1.33333f);

                    vertexColors[vertexIndex + 0] = c;
                    vertexColors[vertexIndex + 1] = c;
                    vertexColors[vertexIndex + 2] = c;
                    vertexColors[vertexIndex + 3] = c;
                }
            }

            // Update Geometry
            text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }
        
        InputManager.Instance.RevertToPreviousCursor();
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
    public void SetOnClickAction(System.Action<object> onClickAction) {
        this.onClickAction = onClickAction;
    }
}

[System.Serializable]
public class EventLabelHoverAction : UnityEvent<object> { }
