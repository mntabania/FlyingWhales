using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogItem : MonoBehaviour {

    public enum Position { Left, Right }

    [SerializeField] private RectTransform portraitRT;
    [SerializeField] private CharacterPortrait portrait;
    public RectTransform characterDialogParent;

    [Header("Left Dialog")]
    [SerializeField] private GameObject leftGO;
    [SerializeField] private TextMeshProUGUI leftText;

    [Header("Right Dialog")]
    [SerializeField] private GameObject rightGO;
    [SerializeField] private TextMeshProUGUI rightText;

    [SerializeField] private Vector2 leftPortraitPos;
    [SerializeField] private Vector2 rightPortraitPos;

    public void SetData(Character character, string text, Position position = Position.Left) {
        if (character != null) {
            portrait.enabled = true;
            portrait.GeneratePortrait(character);    
        } else {
            portrait.SetAsDefaultMinion();
            portrait.enabled = false;
        }
        if (position == Position.Left) {
            leftText.text = text;
        } else {
            rightText.text = text;
        }
        AlignToPosition(position);
    }
    private void AlignToPosition(Position position) {
        if (position == Position.Left) {
            rightGO.SetActive(false);
            leftGO.SetActive(true);
            portraitRT.anchoredPosition = leftPortraitPos;
            //characterDialog.alignment = TextAlignmentOptions.MidlineLeft;
        } else if (position == Position.Right) {
            rightGO.SetActive(true);
            leftGO.SetActive(false);
            portraitRT.anchoredPosition = rightPortraitPos;
            //characterDialog.alignment = TextAlignmentOptions.MidlineRight;
        }
    }

    public Position positionTest;
    [ContextMenu("Alignment test")]
    public void AlignmentTest() {
        AlignToPosition(positionTest);
    }
}
