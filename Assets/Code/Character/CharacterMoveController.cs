using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public class CharacterMoveController : NetworkBehaviour
    {
        [SerializeField] private LayerMask _wallLayer;
        private Collider2D _collider;
        public float acceleration = 10f;
        public float braking = 10f;
        public float maxSpeed= 2f;
        public float rotationSpeed = 15f;
        [Networked]
        public Vector2 Velocity { get; set; }

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
        }
        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
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
            transform.Translate(moveVelocity*deltaTime);
            Velocity = (transform.position -previusPos)*Runner.TickRate;
        }
    }
}
