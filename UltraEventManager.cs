using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class UltraEventManager : MonoBehaviour
{
    [System.Serializable]
    public class UltraEvent
    {
        [Header("��������")]
        public string eventName = "���¼�";
        public float delay = 0f;

        [Header("��������")]
        public EventAction[] actions;
    }

    [System.Serializable]
    public class EventAction
    {
        public enum ActionType { ToggleActive, SetActive, SetInactive, PlayAudio, PlayAnimation, CustomEvent }

        public ActionType actionType;
        public GameObject targetObject;

        [Header("�ض�����")]
        public AudioClip audioClip;
        public string animationName;
        public UnityEvent customEvent;
    }

    [Header("�¼�����")]
    public UltraEvent[] events;

    [Header("ȫ������")]
    public bool debugMode = false;

    // === ��Ҫ�������� ===
    public void Trigger(string eventName) => StartCoroutine(ExecuteEvent(eventName));
    public void Trigger(int index) => StartCoroutine(ExecuteEvent(index));
    public void TriggerAll() { foreach (var e in events) Trigger(e.eventName); }

    private IEnumerator ExecuteEvent(string eventName)
    {
        foreach (var e in events)
        {
            if (e.eventName == eventName)
            {
                if (e.delay > 0) yield return new WaitForSeconds(e.delay);
                ExecuteActions(e.actions);
                if (debugMode) Debug.Log($" �����¼�: {eventName}", this);
                yield break;
            }
        }
        if (debugMode) Debug.LogWarning($" �¼�δ�ҵ�: {eventName}", this);
    }

    private IEnumerator ExecuteEvent(int index)
    {
        if (index >= 0 && index < events.Length)
        {
            var e = events[index];
            if (e.delay > 0) yield return new WaitForSeconds(e.delay);
            ExecuteActions(e.actions);
            if (debugMode) Debug.Log($" �����¼� [{index}]: {e.eventName}", this);
        }
        else if (debugMode) Debug.LogError($" �¼�������Ч: {index}", this);
    }

    private void ExecuteActions(EventAction[] actions)
    {
        foreach (var action in actions)
        {
            GameObject target = action.targetObject ? action.targetObject : gameObject;

            switch (action.actionType)
            {
                case EventAction.ActionType.ToggleActive:
                    target.SetActive(!target.activeSelf);
                    break;
                case EventAction.ActionType.SetActive:
                    target.SetActive(true);
                    break;
                case EventAction.ActionType.SetInactive:
                    target.SetActive(false);
                    break;
                case EventAction.ActionType.PlayAudio:
                    AudioSource audioSource = target.GetComponent<AudioSource>() ?? target.AddComponent<AudioSource>();
                    if (action.audioClip) audioSource.PlayOneShot(action.audioClip);
                    break;
                case EventAction.ActionType.PlayAnimation:
                    Animator anim = target.GetComponent<Animator>();
                    if (anim && !string.IsNullOrEmpty(action.animationName))
                        anim.Play(action.animationName);
                    break;
                case EventAction.ActionType.CustomEvent:
                    action.customEvent?.Invoke();
                    break;
            }
        }
    }

    // === ��ݾ�̬���� ===
    public static void TriggerOnObject(GameObject obj, string eventName) =>
        obj?.GetComponent<EventManager>()?.Trigger(eventName);

    public static void TriggerOnObject(GameObject obj, int index) =>
        obj?.GetComponent<EventManager>()?.Trigger(index);

    // === �༭����ݷ��� ===
    [ContextMenu("������һ���¼�")]
    private void TriggerFirstEvent() => Trigger(0);

    [ContextMenu("���������¼�")]
    private void TriggerAllEvents() => TriggerAll();
}