using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Ruinarch;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events;

public class EventLabel : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler{

    [SerializeField] private LogItem logItem;
	[SerializeField] private TextMeshProUGUI text;
    [SerializeField] private bool allowClickAction = true;
    [SerializeField] private EventLabelHoverAction hoverAction;
    [SerializeField] private UnityEvent hoverOutAction;
    private Log log;

    private int lastHoveredLinkIndex = -1;
    private bool isHighlighting;

    public System.Func<object, bool> shouldBeHighlightedChecker;
    public System.Action<object> onClickAction;

    protected bool isHovering;

    protected Dictionary<string, object> objectDictionary;

    private void Awake() {
        objectDictionary = new Dictionary<string, object>();
        if (text == null) {
            text = gameObject.GetComponent<TextMeshProUGUI>();
        }
    }
    void Update() {
        if (isHovering) {
            HoveringAction();
        }
    }
    void OnDisable() {
        ResetHighlightValues();
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
            if (logItem == null) {
                string linkText = linkInfo.GetLinkID();
                int idToUse;
                if (!int.TryParse(linkText, out idToUse)) {
                    if (linkText.Contains("_") && linkText.Length > 1) {
                        string id = linkText.Substring(0, linkText.IndexOf('_'));
                        idToUse = int.Parse(id);
                    }
                }
                if (linkText.Contains("_faction")) {
                    Faction faction = FactionManager.Instance.GetFactionBasedOnID(idToUse);
                    obj = faction;
                } else if (linkText.Contains("_character")) {
                    Character character = CharacterManager.Instance.GetCharacterByID(idToUse);
                    obj = character;
                } else if (linkText.Contains("_hextile")) {
                    HexTile tile = GridMap.Instance.allTiles[idToUse];
                    obj = tile;
                } 
                // else if (linkText.Contains("_combat")) {
                //     if (UIManager.Instance.characterInfoUI.activeCharacter != null) {
                //         if (UIManager.Instance.characterInfoUI.activeCharacter.combatHistory.ContainsKey(idToUse)) {
                //             UIManager.Instance.ShowCombatLog(UIManager.Instance.characterInfoUI.activeCharacter.combatHistory[idToUse]);
                //         }
                //     }
                // } 
                else {
                    obj = linkInfo.GetLinkID();
                }
            } else if (logItem.log != null) {
                string linkText = linkInfo.GetLinkID();
                int idToUse;
                if (!int.TryParse(linkText, out idToUse)) {
                    string id = linkText.Substring(0, linkText.IndexOf('_'));
                    idToUse = int.Parse(id);
                }
                //int indexToUse = int.Parse(linkInfo.GetLinkID());
                LogFiller lf = logItem.log.fillers[idToUse];
                obj = lf.obj;
            }
            if (onClickAction != null) {
                if (obj != null) {
                    onClickAction.Invoke(obj);
                }
            } else {
                if(obj != null) {
                    if (obj is Character) {
                        UIManager.Instance.ShowCharacterInfo(obj as Character, true);
                    } else if (obj is NPCSettlement) {
                        UIManager.Instance.ShowRegionInfo((obj as NPCSettlement).region);
                    } else if (obj is Faction) {
                        UIManager.Instance.ShowFactionInfo((obj as Faction));
                    } else if (obj is Minion) {
                        UIManager.Instance.ShowCharacterInfo((obj as Minion).character, true);
                    }
                    //else if (obj is Combat) {
                    //    UIManager.Instance.ShowCombatLog(obj as Combat);
                    //} 
                    else if (obj is Party) {
                        Party party = obj as Party;
                        UIManager.Instance.ShowCharacterInfo(party.owner, true);
                    } else if (obj is IPointOfInterest) {
                        IPointOfInterest poi = obj as IPointOfInterest;
                        if (poi is Character) {
                            UIManager.Instance.ShowCharacterInfo(poi as Character, true);
                        } else if (poi is TileObject) {
                            UIManager.Instance.ShowTileObjectInfo(poi as TileObject);
                        }
                    } else if (obj is Region) {
                        Region region = obj as Region;
                        UIManager.Instance.ShowRegionInfo(region);
                    }
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
            if (log == null) {
                int idToUse;
                if (!int.TryParse(linkText, out idToUse)) {
                    if (linkText.Contains("_") && linkText.Length > 1) {
                        string id = linkText.Substring(0, linkText.IndexOf('_'));
                        idToUse = int.Parse(id);
                    }
                }
                if (objectDictionary.ContainsKey(linkText)) {
                    obj = objectDictionary[linkText];
                } else {
                    if (linkText.Contains("_faction")) {
                        obj = FactionManager.Instance.GetFactionBasedOnID(idToUse);
                    } else if (linkText.Contains("_character")) {
                        obj = CharacterManager.Instance.GetCharacterByID(idToUse);
                    } else if (linkText.Contains("_hextile")) {
                        obj = GridMap.Instance.allTiles[idToUse];
                    } else {
                        obj = linkText;
                    }
                    objectDictionary.Add(linkText, obj);
                }
            } else {
                int idToUse;
                if (!int.TryParse(linkText, out idToUse)) {
                    string id = linkText.Substring(0, linkText.IndexOf('_'));
                    idToUse = int.Parse(id);
                }
                if (objectDictionary.ContainsKey(linkText)) {
                    obj = objectDictionary[linkText];
                } else {
                    obj = log.fillers[idToUse].obj;
                    objectDictionary.Add(linkText, obj);
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
    private void HighlightLink(TMP_LinkInfo linkInfo) {
        string oldText = $"<link={'"'}{linkInfo.GetLinkID()}{'"'}>{linkInfo.GetLinkText()}</link>";
        string newText = $"<u>{oldText}</u>";
        text.text = text.text.Replace(oldText, newText);
        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Link);
    }
    private void UnhighlightLink(TMP_LinkInfo linkInfo) {
        string oldText = $"<link={'"'}{linkInfo.GetLinkID()}{'"'}>{linkInfo.GetLinkText()}</link>";
        string newText = $"<u>{oldText}</u>";
        text.text = text.text.Replace(newText, oldText);
        InputManager.Instance.RevertToPreviousCursor();
    }
    public void HoverOutAction() {
        //if (hoverOutAction == null) {
        //    return;
        //}
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
        if (linkIndex == -1) {
            hoverOutAction?.Invoke();
        }
        ResetHighlightValues();
    }
    public void SetOnClickAction(System.Action<object> onClickAction) {
        this.onClickAction = onClickAction;
    }
}

[System.Serializable]
public class EventLabelHoverAction : UnityEvent<object> { }
