using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public class BaseService : MonoBehaviour
    {
        public BaseContext Context => _context;
        public Base Base => _base;
        public bool IsActive => _isActive;
        public bool IsInitialized => _isInitialized;

        private Base _base;
        private BaseContext _context;
        private bool _isInitialized;
        private bool _isActive;

        internal void Initialize(Base bases, BaseContext context)
        {
            if (_isInitialized == true)
                return;

            _base = bases;
            _context = context;
            
            OnInitialize();

            _isInitialized = true;
        }

        internal void Deinitialize()
        {
            if (_isInitialized == false)
                return;

            Deactivate();

            OnDeinitialize();

            _base = null;
            _context = null;

            _isInitialized = false;
        }

        internal void Activate()
        {
            if (_isInitialized == false)
                return;

            if (_isActive == true)
                return;

            OnActivate();

            _isActive = true;
        }

        internal void Tick()
        {
            if (_isActive == false)
                return;

            OnTick();
        }

        internal void LateTick()
        {
            if (_isActive == false)
                return;

            OnLateTick();
        }

        internal void Deactivate()
        {
            if (_isActive == false)
                return;

            OnDeactivate();

            _isActive = false;
        }

        protected virtual void OnInitialize()
        {

        }

        protected virtual void OnDeinitialize()
        {
        }

        protected virtual void OnActivate()
        {

        }

        protected virtual void OnDeactivate()
        {
        }

        protected virtual void OnTick()
        {
        }

        protected virtual void OnLateTick()
        {
        }
    }
}
