using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class PopUpTextNotification
{
    private static RuinarchText m_popupText;
    private static IEnumerator onDoneFunction;
    private static Image m_parentGO;
    private static MonoBehaviour m_mono;
    private static GameObject PopUpparent {
        get {
            if (m_popupText == null) {
                GameObject go = GameObject.Find("PopUpNotificationUI");
                m_parentGO = go.GetComponentInChildren<Image>(true);
                m_popupText = go.GetComponentInChildren<RuinarchText>(true);
                m_mono = m_parentGO.GetComponent<MonoBehaviour>();
            }
            return m_parentGO.gameObject;
        }
    }

    public static void ShowPlayerPoppingTextNotif(string p_message, Transform p_startingPosition = null, int p_stringCount = 0) {
        AudioManager.Instance.OnTextPopUpSoundPlay();
        PopUpparent.gameObject.SetActive(true);
        m_mono.StopAllCoroutines();
        m_popupText.text = p_message;
        Vector3 pos = PopUpparent.transform.position;
        if (p_startingPosition != null) {
            pos = PopUpparent.transform.position = p_startingPosition.transform.position;
        }
        m_mono.StartCoroutine(EnvelopText());
        m_mono.StartCoroutine(Move(pos));
    }
    static IEnumerator EnvelopText() {
        yield return null;
        m_parentGO.rectTransform.sizeDelta = m_popupText.rectTransform.sizeDelta;//new Vector2(p_stringCount * 10, m_parentGO.rectTransform.sizeDelta.y);
        yield return null;
    }
    static IEnumerator Move(Vector3 p_targetPos) {
        yield return null;
        p_targetPos.y += 40;
        float timer = 0f;
        while (timer < 2f) {
            timer += Time.deltaTime;
            PopUpparent.transform.position = Vector3.MoveTowards(PopUpparent.transform.position, p_targetPos, 120f * Time.deltaTime);
            yield return 0;
        }
        // m_popupText.text = string.Empty;
        PopUpparent.gameObject.SetActive(false);
    }
}
