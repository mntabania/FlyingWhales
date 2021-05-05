using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PopUpTextNotification
{
    private static RuinarchText m_popupText;
    private static IEnumerator onDoneFunction;
    private static RuinarchText PopUpText {
        get {
            if (m_popupText == null) {
                m_popupText = GameObject.Find("PopUpText").GetComponent<RuinarchText>();
            }
            return m_popupText;
        }
    }

    public static void ShowPlayerPoppingTextNotif(string p_message, Transform p_startingPosition = null) {
        AudioManager.Instance.OnErrorSoundPlay();
        PopUpText.StopAllCoroutines();
        PopUpText.text = p_message;
        Vector3 pos = PopUpText.transform.position;
        if (p_startingPosition != null) {
            pos = PopUpText.transform.position = p_startingPosition.transform.position;
        }
        PopUpText.StartCoroutine(Move(pos));
    }

    static IEnumerator Move(Vector3 p_targetPos) {
        p_targetPos.y += 40;
        float timer = 0f;
        while (timer < 2f) {
            timer += Time.deltaTime;
            PopUpText.transform.position = Vector3.MoveTowards(PopUpText.transform.position, p_targetPos, 50f * Time.deltaTime);
            yield return 0;
        }
        PopUpText.text = string.Empty;
    }
}
