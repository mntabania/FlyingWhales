using System.Collections.Generic;
using Ruinarch.Custom_UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace Ruinarch.Custom_UI {
#if UNITY_EDITOR
    [ExecuteInEditMode]
    public class ButtonConverter : MonoBehaviour {

        [ContextMenu("Convert All Buttons")]
        public void ConvertAllButtons() {
            List<GameObject> gameObjects = GetAllObjectsOnlyInScene();
            for (int i = 0; i < gameObjects.Count; i++) {
                GameObject obj = gameObjects[i];
                Button[] buttons = obj.GetComponentsInChildren<Button>();
                for (int j = 0; j < buttons.Length; j++) {
                    Button button = buttons[j];
                    GameObject buttonGameObject = button.gameObject;
                    ButtonSettings buttonSettings = new ButtonSettings(button);
                    DestroyImmediate(button);
                    RuinarchButton ruinarchButton = buttonGameObject.AddComponent<RuinarchButton>();
                    buttonSettings.ApplySettings(ruinarchButton);
                    Debug.Log($"Successfully converted {buttonGameObject.name}");
                }
            }
        } 
        
        private List<GameObject> GetAllObjectsOnlyInScene() {
            List<GameObject> objectsInScene = new List<GameObject>();

            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]) {
                if (!EditorUtility.IsPersistent(go.transform.root.gameObject) && !(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave))
                    objectsInScene.Add(go);
            }

            return objectsInScene;
        }
    }
#endif
}

public class ButtonSettings {
    private readonly bool _interactable;
    private readonly Selectable.Transition _transition;
    private readonly Graphic _targetGraphic;
    //sprite swap
    private SpriteState _spriteState;
    //color
    private ColorBlock _colorBlock;
    //navigation
    private Navigation _navigation;
    
    private readonly Button.ButtonClickedEvent _onClickAction;

    public ButtonSettings(Button button) {
        _interactable = button.interactable;
        _transition = button.transition;
        _targetGraphic = button.targetGraphic;
        _spriteState = button.spriteState;
        _colorBlock = button.colors;
        _navigation = button.navigation;
        _onClickAction = button.onClick;
        
    }

    public void ApplySettings(RuinarchButton ruinarchButton) {
        ruinarchButton.interactable = _interactable;
        ruinarchButton.transition = _transition;
        ruinarchButton.targetGraphic = _targetGraphic;
        ruinarchButton.spriteState = _spriteState;
        ruinarchButton.colors = _colorBlock;
        ruinarchButton.navigation = _navigation;
        ruinarchButton.onClick = _onClickAction;
    }
}