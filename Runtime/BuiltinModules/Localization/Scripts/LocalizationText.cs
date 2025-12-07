using TMPro;
using UnityEngine;

namespace Serbull.GameAssets.Localization
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizationText : MonoBehaviour
    {
        [SerializeField] private string _id;

        private TextMeshProUGUI _text;
        private string[] _args = new string[0];

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            UpdateText();
        }

        protected void UpdateText()
        {
            var locText = Services.Localization.GetText(_id);
            if (_args.Length > 0)
            {
                locText = string.Format(locText, _args);
            }

            if (_text != null && locText != null)
            {
                _text.text = locText;
            }
        }

        public void SetLocalizationId(string id)
        {
            _id = id;
            UpdateText();
        }

        public void SetLocalizationId(string id, params object[] args)
        {
            _id = id;
            System.Array.Resize(ref _args, args.Length);
            for (int i = 0; i < _args.Length; i++)
            {
                _args[i] = args[i].ToString();
            }
            UpdateText();
        }

        public void SetArguments(params object[] args)
        {
            System.Array.Resize(ref _args, args.Length);
            for (int i = 0; i < _args.Length; i++)
            {
                _args[i] = args[i].ToString();
            }
            UpdateText();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_id != null)
            {
                _id = _id.Replace(" ", "_").ToLower();
            }
        }
#endif
    }
}
