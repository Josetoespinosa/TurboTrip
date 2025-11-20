using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public float skipTime = 6f;

    private AudioSource audioSource;
    private bool firstLoopDone = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); 

        audioSource = GetComponent<AudioSource>();

        audioSource.loop = false;

        if (!audioSource.isPlaying && audioSource.clip != null)
        {
            audioSource.time = 0f;
            audioSource.Play();
        }
    }

    private void Update()
    {
        if (audioSource == null || audioSource.clip == null)
            return;

        if (!firstLoopDone)
        {
            if (!audioSource.isPlaying && audioSource.time > 0f)
            {
                firstLoopDone = true;
                audioSource.time = skipTime;
                audioSource.Play();
            }
        }
        else
        {
            if (!audioSource.isPlaying)
            {
                audioSource.time = skipTime;
                audioSource.Play();
            }
        }
    }
}