using UnityEngine;

namespace Shared
{
    public class PopupManager : MonoBehaviour
    {
        [SerializeField] private SimplePopUp simplePopupPrefab;
        [SerializeField] private Canvas mainCanvas;

        private static PopupManager _instance;
        public static PopupManager Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void ShowSimplePopup(string message)
        {
            if (_instance == null)
            {
                Debug.LogError("PopupManager is not initialized.");
                return;
            }

            var simplePopup = Instantiate(simplePopupPrefab, mainCanvas.transform);
            simplePopup.Initialize(message);
        }
    }   
}
