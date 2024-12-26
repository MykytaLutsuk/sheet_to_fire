using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shared
{
    public class SimplePopUp : MonoBehaviour
    {
        public TMP_Text messageText;
        public Button okButton;

        public void Initialize(string message)
        {
            messageText.text = message;
            okButton.onClick.AddListener(OnOkButtonClick);
        }

        private void OnDestroy()
        {
            okButton.onClick.RemoveListener(OnOkButtonClick);
        }

        private void OnOkButtonClick()
        {
            Destroy(gameObject);
        }
    }
}
