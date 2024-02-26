using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public class CharacterAnimationController : MonoBehaviour
    {
        Agent agent;
        [SerializeField]
        private Animator animator;
        [SerializeField]
        private SpriteRenderer _renderer;
        private void Awake()
        {
            agent = GetComponent<Agent>();
            agent.Health.HitTaken += AnimationTakeDamage;
        }
        private void AnimationTakeDamage(HitData data)
        {
            if (animator == null)
            {
                return;
            }
            animator.SetTrigger("Herido");
        }
        private void LateUpdate()
        {
            if (animator == null || _renderer == null)
            {
                return;
            }
            if (agent.Character.CharacteController.Velocity.x < -0.1f)
                _renderer.flipX = true;
            else if (agent.Character.CharacteController.Velocity.x > 0.1f)
                _renderer.flipX = false;
            if (animator == null)
                return;
            animator.SetFloat("vel",agent.Character.CharacteController.Velocity.magnitude);
        }
    }
}
