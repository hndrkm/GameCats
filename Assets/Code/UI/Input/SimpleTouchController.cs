using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public class SimpleTouchController : MonoBehaviour
    {
        public bool Press => touchPresent;
        public bool Pressed => _isPressed;
        public bool Released => _isReleased;

        public delegate void TouchDelegate(Vector2 value);
        public event TouchDelegate TouchEvent;

        public delegate void TouchStateDelegate(bool touchPresent);
        public event TouchStateDelegate TouchStateEvent;

        
        private bool _isPressed;
        private bool _isReleased;

        // PRIVATE
        [SerializeField]
        private RectTransform joystickArea;
        private bool touchPresent = false;
        private Vector2 movementVector;
        public void Update()
        {
            //Debug.Log($"{_isPressed}...{_isReleased}");

            if (_isReleased)
                _isPressed = false;
            if (_isReleased )
                _isReleased = false;
        }

        public Vector2 GetTouchPosition
        {
            get { return movementVector; }
        }

        public void Down() 
        {
            
        }
        public void BeginDrag()
        {
            _isPressed = true;
            touchPresent = true;
            if (TouchStateEvent != null)
                TouchStateEvent(touchPresent);
            //_isPressed = false;
        }
        public void Up()
        {
            
        }
        public void EndDrag()
        {
            _isReleased = true;
            touchPresent = false;
            movementVector = joystickArea.anchoredPosition = Vector2.zero;
            if (TouchStateEvent != null)
                TouchStateEvent(touchPresent);
            //_isReleased = false;
        }

        public void OnValueChanged(Vector2 value)
        {
            if (touchPresent)
            {
                // convert the value between 1 0 to -1 +1
                movementVector.x = ((1 - value.x) - 0.5f) * 2f;
                movementVector.y = ((1 - value.y) - 0.5f) * 2f;
                
                if (TouchEvent != null)
                {
                    TouchEvent(movementVector);
                }
            }

        }
    }
}
