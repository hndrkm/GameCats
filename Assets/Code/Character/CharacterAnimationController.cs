using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame
{
    public class CharacterAnimationController : MonoBehaviour
    {
        Agent agent;
        [SerializeField]
        Animator animator;
        [SerializeField]
        SpriteRenderer _renderer;
        private void Awake()
        {
            //animator = tryGetComponent<Animator>();
            agent = GetComponent<Agent>();
            agent.Health.HitTaken += AnimationTakeDamage;
        }
        private void AnimationTakeDamage(HitData data)
        { 
            animator.SetTrigger("Herido");
        }
        private void LateUpdate()
        {
            if (agent.Character.CharacteController.VelocityT.x < -0.1f)
                _renderer.flipX = true;
            else if (agent.Character.CharacteController.VelocityT.x > 0.1f)
                _renderer.flipX = false;
            if (animator == null)
                return;
            animator.SetFloat("vel",agent.Character.CharacteController.VelocityT.magnitude);
        }
    }
}
