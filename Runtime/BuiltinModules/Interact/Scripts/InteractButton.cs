using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Serbull.GameAssets.Interact
{
    public class InteractButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private TMPro.TMP_Text _text;
        [SerializeField] private RectTransform _root;
        [SerializeField] private Transform _interactImage;
        [SerializeField] private Image _fillImage;

        [SerializeField] private GameObject _pc;
        [SerializeField] private GameObject _mobile;

        private Action _callback;
        private Transform _targetObject;
        private Vector3 _targetOffset;
        private Camera _cam;
        private float _holdTime;
        private float _currentHoldTime;
        private bool _isPressed;

        void OnEnable()
        {
            DeactivatePress();
        }

        void OnDisable()
        {
            DeactivatePress();
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        public void Show(InteractData interactData)
        {
            Show(interactData.TargetObject, interactData.TargetOffset, interactData.Callback, interactData.Text, interactData.HoldTime);
        }

        public void Show(Transform targetObject, Vector3 targetOffset, Action callback, string text, float holdTime = 0.5f)
        {
            _cam = Camera.main;
            _targetObject = targetObject;
            _targetOffset = targetOffset;
            _text.text = text;
            _callback = callback;
            _holdTime = holdTime;

            gameObject.SetActive(true);
            DeactivatePress();
        }

        public void SetMobile(bool isMobile)
        {
            _pc.SetActive(!isMobile);
            _mobile.SetActive(isMobile);
        }

        public void Hide()
        {
            _targetObject = null;
            _callback = null;

            if (gameObject == null)
                return;

            gameObject.SetActive(false);
            DeactivatePress();
        }

        private void Interact()
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

            CheckInput();

            if (_isPressed)
            {
                if (_holdTime > 0f)
                {
                    _currentHoldTime += Time.deltaTime;
                    _fillImage.fillAmount = Mathf.Clamp01(_currentHoldTime / _holdTime);

                    if (_currentHoldTime >= _holdTime)
                    {
                        Interact();
                        DeactivatePress();
                    }
                }
                else
                {
                    Interact();
                    DeactivatePress();
                }
            }
        }

        private void CheckInput()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ActivatePress();
            }
            else if (Input.GetKeyUp(KeyCode.E))
            {
                DeactivatePress();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ActivatePress();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            DeactivatePress();
        }

        private void ActivatePress()
        {
            _isPressed = true;
            _fillImage.enabled = _holdTime > 0f;
            _interactImage.localScale = Vector3.one * 1.2f;
            _text.transform.localScale = Vector3.one * 0.85f;
        }

        private void DeactivatePress()
        {
            _isPressed = false;
            _fillImage.enabled = false;
            _currentHoldTime = 0f;
            _interactImage.localScale = Vector3.one;
            _text.transform.localScale = Vector3.one;
        }
    }
}