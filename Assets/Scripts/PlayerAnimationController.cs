using System.Collections;
using StarterAssets;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] Animator myAnimator;
    [SerializeField] float maxAnimationSpeedModifier = 2f;
    float baseAnimationSpeedModifier = 1f;

    StarterAssetsInputs _input;
    CharacterController myCharacterController;

    // Looping animation config
    [SerializeField] string loopAnimationBoolName = "isLoopingAction"; // e.g., "isThinking"
    [SerializeField] float thinkOnTime = 0.8f;   // how long "thinking" stays ON
    [SerializeField] float thinkOffTime = 0.5f;  // how long it stays OFF (idle)

    // Control from other scripts:
    public bool playLoopAnimation = false;

    public bool isLooping = false;
    Coroutine loopRoutine;

    void Start()
    {
        _input = GetComponent<StarterAssetsInputs>();
        myCharacterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Edge-trigger start/stop so we don't restart every frame
        if (playLoopAnimation && !isLooping)
            StartLoop();

        if (!playLoopAnimation && isLooping)
            StopLoop();

        // When looping, suspend normal movement-driven animation changes
        if (!isLooping)
        {
            HandleMovementAnimations();
            SpeedUpAnimationBasedOnVelocity();
        }
    }

    void OnDisable()
    {
        // Ensure we clean up and reset the bool when disabled
        if (isLooping) StopLoop();
    }

    void StartLoop()
    {
        isLooping = true;
        // make sure any previous routine is stopped
        if (loopRoutine != null) StopCoroutine(loopRoutine);
        loopRoutine = StartCoroutine(LoopThinkingIdle());
    }

    void StopLoop()
    {
        isLooping = false;
        if (loopRoutine != null)
        {
            StopCoroutine(loopRoutine);
            loopRoutine = null;
        }
        // reset to idle state
        myAnimator.SetBool(loopAnimationBoolName, false);
    }

    IEnumerator LoopThinkingIdle()
    {
        while (playLoopAnimation) // master switch you set from other scripts
        {
            // ON: "thinking"
            myAnimator.SetBool(loopAnimationBoolName, true);
            yield return new WaitForSeconds(thinkOnTime);

            // OFF: back to idle
            myAnimator.SetBool(loopAnimationBoolName, false);
            yield return new WaitForSeconds(thinkOffTime);
        }

        // safety reset if we exit naturally
        StopLoop();
    }

    void SpeedUpAnimationBasedOnVelocity()
    {
        float overallSpeed = myCharacterController.velocity.magnitude;
        if (_input.sprint)
            myAnimator.SetFloat("ANIM_SPEED", Mathf.Min(overallSpeed, maxAnimationSpeedModifier));
        else
            myAnimator.SetFloat("ANIM_SPEED", baseAnimationSpeedModifier);
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
            myAnimator.SetBool("isMovingForward", true);
        else if (_move.y < 0)
            myAnimator.SetBool("isMovingBackward", true);
    }
}
