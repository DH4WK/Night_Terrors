namespace Player
{
    using UnityEngine;
    using System.Collections;

    public class PlayerAnimationControllScript : MonoBehaviour
    {
        // The animator component
        private Animator anim;

        // User input for our directions
        float move;
        float turn;

        // Animation Paramters converted to hash strings for a more efficient approach
        int speedHashID = Animator.StringToHash("Speed");
        int turnHashID = Animator.StringToHash("Direction");

        // Use this for initialization
        void Start()
        {
            // Get the animator
            anim = this.GetComponentInChildren<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            // Getting the user input
            move = Input.GetAxis("Vertical");
            turn = Input.GetAxis("Horizontal");

            // Now actually moving the animations
            anim.SetFloat(speedHashID, move);
            anim.SetFloat(turnHashID, turn);
        }
    }
}
