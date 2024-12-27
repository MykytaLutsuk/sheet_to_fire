using FirebaseIntegration;
using GoogleSheetsIntegration;
using UnityEngine;
using UnityEngine.UI;

namespace Shared
{
    public class RefreshButton : MonoBehaviour
    {
        [SerializeField] private SpreadsheetController spreadsheetController;
        [SerializeField] private FirebaseController firebaseController;

        [SerializeField] private Button button;
        
        private void Start()
        {
            button.onClick.AddListener(OnButtonClick);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            spreadsheetController.RefreshData();
            firebaseController.RefreshData();
        }

#if UNITY_EDITOR
        private void OnValidate()
        { 
            TryGetComponent(out button);
        }
#endif
    }   
}
