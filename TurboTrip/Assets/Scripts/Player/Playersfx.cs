using UnityEngine;

[DisallowMultipleComponent]
public class PlayerSfx : MonoBehaviour
{
    [Header("Fuente de audio (se autoconfigura)")]
    public AudioSource audioSource;

    [Header("Clips")]
    public AudioClip jumpSFX;
    public AudioClip doubleJumpSFX;
    public AudioClip dashSFX;
    public AudioClip wallhitSFX;

    [Header("Ajustes")]
    [Range(0f, 1f)] public float volume = 1f;

    void Awake()
    {
        EnsureAudioSource();
    }

    void OnValidate()
    {
        EnsureAudioSource();
    }

    void EnsureAudioSource()
    {
        if (!audioSource)
        {
            audioSource = GetComponent<AudioSource>();
            if (!audioSource)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;  
        audioSource.loop = false;
        audioSource.volume = volume;
    }

    public void PlayJump() { Play(jumpSFX); }
    public void PlayDoubleJump() { Play(doubleJumpSFX ? doubleJumpSFX : jumpSFX); }
    public void PlayDash() { Play(dashSFX); }
    public void PlayWallHit() { Play(wallhitSFX); }

    void Play(AudioClip clip)
    {
        if (!clip) return;
        EnsureAudioSource();
        audioSource.PlayOneShot(clip, volume);
    }
}
