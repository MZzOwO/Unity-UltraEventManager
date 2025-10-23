using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class UltraEventManager : MonoBehaviour
{
    [System.Serializable]
    public class UltraEvent
    {
        [Header("基础设置")]
        public string eventName = "新事件";
        public float delay = 0f;

        [Header("动作类型")]
        public EventAction[] actions;
    }

    [System.Serializable]
    public class EventAction
    {
        public enum ActionType { ToggleActive, SetActive, SetInactive, PlayAudio, PlayAnimation, CustomEvent }

        public ActionType actionType;
        public GameObject targetObject;

        [Header("特定设置")]
        public AudioClip audioClip;
        public string animationName;
        public UnityEvent customEvent;
    }

    [Header("事件配置")]
    public UltraEvent[] events;

    [Header("全局设置")]
    public bool debugMode = false;

    // === 主要触发方法 ===
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
                if (debugMode) Debug.Log($" 触发事件: {eventName}", this);
                yield break;
            }
        }
        if (debugMode) Debug.LogWarning($" 事件未找到: {eventName}", this);
    }

    private IEnumerator ExecuteEvent(int index)
    {
        if (index >= 0 && index < events.Length)
        {
            var e = events[index];
            if (e.delay > 0) yield return new WaitForSeconds(e.delay);
            ExecuteActions(e.actions);
            if (debugMode) Debug.Log($" 触发事件 [{index}]: {e.eventName}", this);
        }
        else if (debugMode) Debug.LogError($" 事件索引无效: {index}", this);
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

    // === 便捷静态方法 ===
    public static void TriggerOnObject(GameObject obj, string eventName) =>
        obj?.GetComponent<EventManager>()?.Trigger(eventName);

    public static void TriggerOnObject(GameObject obj, int index) =>
        obj?.GetComponent<EventManager>()?.Trigger(index);

    // === 编辑器快捷方法 ===
    [ContextMenu("触发第一个事件")]
    private void TriggerFirstEvent() => Trigger(0);

    [ContextMenu("触发所有事件")]
    private void TriggerAllEvents() => TriggerAll();
}