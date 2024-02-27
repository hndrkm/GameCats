using UnityEngine;
namespace CatGame.UI
{
    public class UIMobileInput : UIView
    {
       
        public Vector2 Move { get; set; }
        public Vector2 Aim { get; set; }
        public bool PressPower { get; set; }
        public bool PressedAim { get; set; }
        public bool PressAim { get; set; }
        public bool PressCast { get; set; }
        [SerializeField]
        private SimpleTouchController MoveController;
        [SerializeField]
        private SimpleTouchController AimController;
        [SerializeField] 
        private bool continuousRightController = true;

        protected override void OnOpen()
        {
            base.OnOpen();
            AimController.TouchEvent += RightController_TouchEvent;
        }

        public bool ContinuousRightController
        {
            set { continuousRightController = value; }
        }

        void RightController_TouchEvent(Vector2 value)
        {
            if (!continuousRightController)
            {
                UpdateAim(value);
            }
        }

        protected override void OnTick()
        {
            base.OnTick();
            UpdateMove(MoveController.GetTouchPosition);
            UpdatePressCast();
            UpdatePressedAim();
            UpdatePressAim();
            if (continuousRightController)
            {
                UpdateAim(AimController.GetTouchPosition);
            }
        }
        void UpdatePressedAim() 
        {
            PressedAim = AimController.Pressed;
        }
        void UpdatePressAim()
        {
            PressAim = AimController.Press;
        }
        void UpdatePressCast()
        {
            PressCast = AimController.Released;
        }
        void UpdateMove(Vector2 value)
        {
            Move = value;
        }
        void UpdateAim(Vector2 value)
        {
            Aim = value;
        }

        protected override void OnClose()
        {
            AimController.TouchEvent -= RightController_TouchEvent;
            base.OnClose();
        }

    }
}
