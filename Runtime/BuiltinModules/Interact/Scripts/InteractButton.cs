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
    private Transform _target3D;
    private Camera _cam;

    public void Init(bool isMobile)
    {
        _pc.SetActive(!isMobile);
        _mobile.SetActive(isMobile);
    }

    private void Start()
    {
        gameObject.SetActive(false);
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void Show(Transform target3D, string text, Action callback)
    {
        _cam = Camera.main;
        _target3D = target3D;
        _text.text = text;
        _callback = callback;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        _target3D = null;
        _callback = null;

        gameObject.SetActive(false);
    }

    public void OnClick()
    {
        _callback?.Invoke();
    }

    private void Update()
    {
        if (_target3D == null)
        {
            Debug.LogWarning("Target3D is null.");
            Hide();
            return;
        }

        Vector3 screenPos = _cam.WorldToScreenPoint(_target3D.position);

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