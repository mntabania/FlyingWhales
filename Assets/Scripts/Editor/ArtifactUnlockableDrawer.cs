using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ArtifactUnlockable))]
public class ArtifactUnlockableDrawer : PropertyDrawer {
    private ARTIFACT_UNLOCKABLE_TYPE _unlockableType;
    private int _selectedIndex;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Element"));

        EditorGUI.indentLevel = 0;
        
        var unlockableTypeRect = new Rect(position.x, position.y, 150, position.height);
        var unlockableStrRect = new Rect(position.x + 150, position.y, 150, position.height);
        
        SerializedProperty unlockableTypeProperty = property.FindPropertyRelative("unlockableType");
        SerializedProperty unlockableIdentifierProperty = property.FindPropertyRelative("identifier");
        
        _unlockableType = (ARTIFACT_UNLOCKABLE_TYPE) unlockableTypeProperty.enumValueIndex;
        _unlockableType = (ARTIFACT_UNLOCKABLE_TYPE)EditorGUI.EnumPopup(unlockableTypeRect, _unlockableType);
        
        string[] unlockables = PlayerDB.GetChoicesForUnlockableType(_unlockableType);

        if (string.IsNullOrEmpty(unlockableIdentifierProperty.stringValue) == false) {
            _selectedIndex = Array.IndexOf(unlockables, unlockableIdentifierProperty.stringValue);
            if (_selectedIndex == -1) {
                _selectedIndex = 0;
            }
        }
        _selectedIndex = EditorGUI.Popup(unlockableStrRect, _selectedIndex,unlockables);
        
        unlockableTypeProperty.enumValueIndex = (int) _unlockableType;
        unlockableIdentifierProperty.stringValue = unlockables[_selectedIndex];

        EditorGUI.EndProperty();
        property.serializedObject.ApplyModifiedProperties();
    }
}