using System;
using StarterAssets;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/*
Requirements:
- Animation controller with all movement animations set, transitions defined with conditions.
  - Can reverse animations by setting speed to a negative number.
  - Animations must be set to play forward/backward in editor
- Use matching transition condition parameter names between this script and the editor.
- Have a ANIM_SPEED parameter and set all affected animations to use it

Idea: Design simple all direction movement animation. Can get complicated if we have more than forward/back and left/right.
*/
public class PlayerAnimationController : MonoBehaviour
{

    [SerializeField] Animator myAnimator;
    [SerializeField] float maxAnimationSpeedModifier = 2f;
    float baseAnimationSpeedModifier = 1f;

    StarterAssetsInputs _input;
    CharacterController myCharacterController;

    void Start()
    {
        _input = GetComponent<StarterAssetsInputs>();
        myCharacterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovementAnimations();
        SpeedUpAnimationBasedOnVelocity();
    }

    /*
    Source: https://www.reddit.com/r/Unity3D/comments/15xy6c6/how_would_you_increase_the_speed_of_a_run/
    */
    void SpeedUpAnimationBasedOnVelocity()
    {
        // Source: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/CharacterController-velocity.html
        // The overall speed
        float overallSpeed = myCharacterController.velocity.magnitude;

        if (_input.sprint)
        {
            myAnimator.SetFloat("ANIM_SPEED", Mathf.Min(overallSpeed, maxAnimationSpeedModifier));
        }
        else
        {
            myAnimator.SetFloat("ANIM_SPEED", baseAnimationSpeedModifier);
        }
    }


    void HandleMovementAnimations()
    {
        Vector2 _move = _input.move;



        if (_move == Vector2.zero)
        {
            myAnimator.SetBool("isMovingForward", false);
            myAnimator.SetBool("isMovingBackward", false);

        }

        if (_move.y > 0)
        {
            myAnimator.SetBool("isMovingForward", true);
        }
        else if (_move.y < 0)
        {
            myAnimator.SetBool("isMovingBackward", true);
        }
    }
}
