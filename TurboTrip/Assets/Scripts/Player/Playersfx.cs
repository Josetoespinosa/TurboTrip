using UnityEngine;

[DisallowMultipleComponent]
public class PlayerSfx : MonoBehaviour
{
    public AudioSource audioSource;

    [Header("Clips")]
    public AudioClip jumpSFX;
    public AudioClip doubleJumpSFX;
    public AudioClip dashSFX;
    public AudioClip wallhitSFX;

    public void PlayJump() { Play(jumpSFX); }
    public void PlayDoubleJump() { Play(doubleJumpSFX ? doubleJumpSFX : jumpSFX); }
    public void PlayDash() { Play(dashSFX); }
    public void PlayWallHit() { Play(wallhitSFX); }

    void Play(AudioClip clip)
    {
        if (!audioSource || !clip) return;
        audioSource.PlayOneShot(clip);
    }
}
