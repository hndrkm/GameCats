using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CatGame
{
    public class LoadingScene : MonoBehaviour
    {
        public bool IsFading => _activeFader != null && !_activeFader.activeSelf == false;

        [SerializeField]
        private GameObject _fadeInObject;
        [SerializeField]
        private GameObject _fadeOutObject;
        [SerializeField]
        private TextMeshProUGUI _status;
        [SerializeField]
        private TextMeshProUGUI _statusDescription;
        

        private GameObject _activeFader;



        public void FadeIn()
        {
            _fadeInObject.SetActive(true);
            _fadeOutObject.SetActive(false);

            _activeFader = _fadeInObject;
        }

        public void FadeOut()
        {
            

            _fadeInObject.SetActive(false);
            _fadeOutObject.SetActive(true);

            _activeFader = _fadeOutObject;
        }
        protected void Awake()
        {
            _fadeInObject.SetActive(false);
            _fadeOutObject.SetActive(false);

            
        }

        protected void Update()
        {
            _status.text = Global.Networking.Status;
            _statusDescription.text = Global.Networking.StatusDescription;
        }

        protected void OnDestroy()
        {
           
        }
    }
}
