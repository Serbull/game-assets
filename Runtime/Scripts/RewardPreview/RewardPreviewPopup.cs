using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Serbull.GameAssets
{
    public class RewardPreviewPopup : Popup
    {
        [SerializeField] private Transform _content;
        [SerializeField] private RewardPreviewSlot _slotPrefab;
        [SerializeField] private Button _closeButton;

        private void Awake()
        {
            _closeButton.onClick.AddListener(Hide);
            OnHidden += PopupOnHidden;
        }

        public void Show(params RewardPreviewItem[] items)
        {
            base.Show();
            StartCoroutine(ShowItems(items));
        }

        private IEnumerator ShowItems(RewardPreviewItem[] items)
        {
            _closeButton.gameObject.SetActive(false);

            foreach (var item in items)
            {
                var slot = Instantiate(_slotPrefab, _content);
                slot.Init(item);
                yield return new WaitForSeconds(0.2f);
            }

            yield return new WaitForSeconds(0.3f);
            _closeButton.gameObject.SetActive(true);
        }

        private void PopupOnHidden()
        {
            foreach (Transform child in _content)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
