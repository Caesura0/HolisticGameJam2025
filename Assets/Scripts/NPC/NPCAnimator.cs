using UnityEngine;

public class NPCAnimator : MonoBehaviour
{
    [SerializeField] Animator animator;


    public void SetAnimationParameters(float direction, float speed)
    {
        animator.SetFloat("Direction", direction);
        animator.SetFloat("Speed", speed);
    }

    public void SetIsCatchingBreath(bool isCatchingBreath)
    {
        animator.SetBool("IsCatchingBreath", isCatchingBreath);
    }
}
