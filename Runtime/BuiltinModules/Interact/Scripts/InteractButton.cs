using System;
using UnityEngine;
using UnityEngine.UI;

public class InteractButton : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text _text;
    [SerializeField] private RectTransform _root;

    [SerializeField] private GameObject _pc;
    [SerializeField] private GameObject _mobile;

    private Action _callback;
    private Transform _targetObject;
    private Vector3 _targetOffset;
    private Camera _cam;

    public void SetMobile(bool isMobile)
    {
        _pc.SetActive(!isMobile);
        _mobile.SetActive(isMobile);
    }

    private void Start()
    {
        gameObject.SetActive(false);
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void Show(Transform targetObject, Vector3 targetOffset, string text, Action callback)
    {
        _cam = Camera.main;
        _targetObject = targetObject;
        _targetOffset = targetOffset;
        _text.text = text;
        _callback = callback;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        _targetObject = null;
        _callback = null;

        gameObject.SetActive(false);
    }

    public void OnClick()
    {
        _callback?.Invoke();
    }

    private void Update()
    {
        if (_targetObject == null)
        {
            Debug.LogWarning("TargetObject is null.");
            Hide();
            return;
        }

        Vector3 screenPos = _cam.WorldToScreenPoint(_targetObject.position + _targetOffset);

        if (screenPos.z < 0)
        {
            Hide();
            return;
        }

        _root.position = screenPos;

        if (Input.GetKeyDown(KeyCode.E))
        {
            OnClick();
        }
    }
}