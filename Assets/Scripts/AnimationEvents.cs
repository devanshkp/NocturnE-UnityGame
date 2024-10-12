using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    public SwordController swordController;
    public PlayerController playerController;

    [Header("Audio Settings")]
    public List<AudioClip> walkSounds;
    public List<AudioClip> runSounds;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public float movementVolume = 0.35f;

    private AudioSource audioSource;

    private int walkSoundIndex = 0;
    private int runSoundIndex = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (swordController == null){
            swordController = GetComponentInChildren<SwordController>();
        }
        if (playerController == null){
            playerController = GetComponentInParent<PlayerController>();
        }
    }

    public void EnableSwordCollider()
    {
        swordController.EnableSwordCollider();
    }

    public void DisableSwordCollider()
    {
        swordController.DisableSwordCollider();
    }

    public void ResetCoolDown()
    {
        swordController.ResetCoolDown();
    }

    public void RestartLevel()
    {
        playerController.EndLevel();
    }

    public void EndAutoTargetState()
    {
        playerController.StopAutoTargeting();
    }

    public void PlayWalkSound()
    {
        if (walkSounds.Count == 0) return;
        float playerVelocity = playerController.GetVelocity();
        int movementState = GetMovementState(playerVelocity);
        if (movementState == 1){
            Debug.Log("Walk Sound played");
            audioSource.PlayOneShot(walkSounds[walkSoundIndex], movementVolume);

            // Cycle through the sounds
            walkSoundIndex = (walkSoundIndex + 1) % walkSounds.Count;
        }

        if (movementState == 0 && audioSource.isPlaying){
            audioSource.Stop();  // Stop playing any sounds
        }
    }

    public void PlayRunSound()
    {
        if (runSounds.Count == 0) return;
        float playerVelocity = playerController.GetVelocity();
        int movementState = GetMovementState(playerVelocity);
        if (movementState == 2){
            Debug.Log("Run Sound played");
            audioSource.PlayOneShot(runSounds[runSoundIndex], movementVolume);

            // Cycle through the sounds
            runSoundIndex = (runSoundIndex + 1) % runSounds.Count;
        }

        if (movementState == 0 && audioSource.isPlaying){
            audioSource.Stop();  // Stop playing any sounds
        }
    }

    public void PlayJumpSound(float velocityParam)
    {
        // Determine which jump sound to play based on the state before the jump
        float playerVelocity = playerController.GetVelocity();
        int movementState = GetMovementState(playerVelocity);

        if (((movementState == 0 || movementState == 1) && velocityParam == 0.01f) || (movementState == 2 && velocityParam == 1f)){
            audioSource.PlayOneShot(jumpSound, movementVolume * 2);
        }
    }

    public void PlayLandSound()
    {
        audioSource.PlayOneShot(landSound, movementVolume);
    }
    
    private int GetMovementState(float playerVelocity)
    {
        if (playerVelocity < 0.5){
            return 0; // Idle
        }
        if (playerVelocity < 5){
            return 1; // Walking
        }
        return 2; // Running
    }
}
