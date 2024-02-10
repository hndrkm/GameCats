using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace CatGame
{
    public class CharacterMoveController : NetworkBehaviour
    {
        [SerializeField] private LayerMask _wallLayer;
        //public Collider2D Collider => _characterController;
        private CharacterController _characterController;
        public float acceleration = 10f;
        public float braking = 10f;
        public float maxSpeed= 2f;
        public float rotationSpeed = 15f;
        [Networked]
        public Vector2 Velocity { get; set; }

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
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
                moveVelocity = Vector2.Lerp(moveVelocity, default, braking * deltaTime);
            }
            else 
            {
                moveVelocity = Vector2.ClampMagnitude(moveVelocity + direction*acceleration*deltaTime, maxSpeed);
            }
            _characterController.Move(moveVelocity*deltaTime);
            Velocity = (transform.position - previusPos)*Runner.Config.Simulation.TickRate;
        }

    }
}
