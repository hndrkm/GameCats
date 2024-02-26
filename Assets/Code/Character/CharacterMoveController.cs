using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace CatGame
{
    public class CharacterMoveController : NetworkTransform
    {
        [SerializeField] private LayerMask _wallLayer;

        public float MaxSpeed { get { return _maxSpeed; } set { _maxSpeed = value; } }

        [SerializeField]
        private float _acceleration = 10f;
        [SerializeField]
        private float _braking = 10f;
        [SerializeField]
        private float _maxSpeed = 2f;

        [Networked]
        public Vector2 Velocity { get; set; }
        public CharacterController Controller { get; set; }
        protected override void Awake()
        {
            base.Awake();
            CacheController();
        }
        public override void Spawned()
        {
            base.Spawned();
            CacheController();
        }
        private void CacheController()
        {
            if (Controller == null)
            {
                Controller = GetComponent<CharacterController>();
            }
        }
        protected override void CopyFromBufferToEngine()
        {
            Controller.enabled = false;
            base.CopyFromBufferToEngine();
            Controller.enabled = true;
        }
        public override void FixedUpdateNetwork()
        {

        }

        public void MoveCharacter(Vector2 direction) 
        {
            var deltaTime = Runner.DeltaTime;
            var previusPos = transform.position;
            var moveVelocity = Velocity;

            if (direction == default)
            {
                moveVelocity = Vector2.Lerp(moveVelocity, default, _braking * deltaTime);
            }
            else 
            {
                moveVelocity = Vector2.ClampMagnitude(moveVelocity + direction*_acceleration*deltaTime, _maxSpeed);
            }
            Controller.Move(moveVelocity*deltaTime);
            Velocity = (transform.position - previusPos)*Runner.Config.Simulation.TickRate;
        }

    }
}
