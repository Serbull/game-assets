using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Serbull.GameAssets.Roulette
{
    public class RoulettePopup : Popup
    {
        [SerializeField] private RouletteSlot[] _slots;
        [SerializeField] private Button _spinButton;
        [SerializeField] private Localization.LocalizationText _spinButtonTxt;
        [SerializeField] private GameObject _spinNotificator;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Transform _arrow;
        [SerializeField] private Transform _wheelTransform;
        [SerializeField] private float _spinDuration = 3f;
        [SerializeField] private int _fullRotations = 5;
        [SerializeField] private Localization.LocalizationText _freeSpinText;
        [SerializeField] private Button _iap1Button;
        [SerializeField] private TextMeshProUGUI _iap1AddText;
        [SerializeField] private Button _iap2Button;
        [SerializeField] private TextMeshProUGUI _iap2AddText;

        private bool _isSpinning = false;
        private RouletteConfig _config;

        private void Awake()
        {
            _config = Services.Roulette.Config;

            _spinButton.onClick.AddListener(SpinButton_OnClick);
            _closeButton.onClick.AddListener(Hide);
            _wheelTransform.localEulerAngles = Vector3.forward * Random.Range(0, 360f);

            var parts = Services.Roulette.Config.Parts;
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i].Init(parts[i]);
            }

            _iap1Button.gameObject.SetActive(_config.UseInApps);
            _iap2Button.gameObject.SetActive(_config.UseInApps);

            _iap1AddText.text = "+" + _config.InApp1AddSpins;
            _iap2AddText.text = "+" + _config.InApp2AddSpins;
        }

        private void OnEnable()
        {
            if (_config.Parts.Length != 6)
            {
                Debug.LogError($"RouletteConfig must contain 6 parts. Now {_config.Parts.Length}");
            }

            _freeSpinText.gameObject.SetActive(_config.AddFreeSpins);

            if (Services.Roulette != null)
            {
                Services.Roulette.OnSpinCountChanged += UpdateUI;
            }

            if (Services.Purchase != null)
            {
                Services.Purchase.OnAnyRewardGranted += UpdateUI;
            }

            UpdateUI();
        }

        private void OnDisable()
        {
            if (Services.Roulette != null)
            {
                Services.Roulette.OnSpinCountChanged -= UpdateUI;
            }

            if (Services.Purchase != null)
            {
                Services.Purchase.OnAnyRewardGranted -= UpdateUI;
            }
        }

        private void Update()
        {
            if (_config.AddFreeSpins)
            {
                var leftTime = Services.Roulette.FreeSpinTimer;
                int minutes = Mathf.FloorToInt(leftTime / 60);
                int seconds = Mathf.FloorToInt(leftTime % 60);
                var arg = string.Format("{0:00}:{1:00}", minutes, seconds);
                _freeSpinText.SetArguments(arg);
            }
        }

        private void SpinButton_OnClick()
        {
            if (_isSpinning)
                return;

            if (Services.Roulette.SpinCount <= 0)
                return;

            _isSpinning = true;
            Services.Roulette.SpendSpin(1);

            var winningIndex = MathfUtils.GetRandomIndexByWeight(Services.Roulette.Config.GetWeightIndexes());
            var reward = Services.Roulette.Config.Parts[winningIndex].Reward;

            var anglePerSlot = 360f / _slots.Length;
            var targetAngle = _fullRotations * 360f - ((winningIndex + 1) * anglePerSlot - Random.value * anglePerSlot);

            var lastAngle = _wheelTransform.eulerAngles.z;
            var lastArrowPoint = (int)(lastAngle % 360 / anglePerSlot);

            _wheelTransform
                .DORotate(new Vector3(0, 0, -targetAngle), _spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic)
                .OnUpdate(() =>
                {
                    var angle = _wheelTransform.eulerAngles.z;
                    var arrowPoint = (int)(angle % 360 / anglePerSlot);

                    if (arrowPoint != lastArrowPoint)
                    {
                        lastArrowPoint = arrowPoint;
                        var speed = angle - lastAngle;
                        _arrow.DOKill();
                        _arrow.DORotate(new Vector3(0, 0, 65), 0.1f)
                            .OnComplete(() => _arrow.DORotate(Vector3.zero, 0.2f));
                    }

                    lastAngle = angle;
                })
                .OnComplete(() =>
                {
                    _isSpinning = false;
                    Services.Roulette.AddRewardWithPreview(reward);
                });
        }

        public void UpdateUI()
        {
            var amount = Services.Roulette.SpinCount;
            _spinButtonTxt.SetArguments(amount);
            _spinNotificator.SetActive(amount > 0);
        }
    }
}
