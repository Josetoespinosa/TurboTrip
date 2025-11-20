using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public float skipTime = 6f;

    private AudioSource audioSource;
    private bool firstLoopDone = false;

    private void Awake()
    {
        // Singleton: si ya hay uno, destruimos este
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // <- CLAVE: no se destruye al cambiar de escena

        // Tomamos el AudioSource del mismo GameObject
        audioSource = GetComponent<AudioSource>();

        // Usamos loop manual (así controlamos desde qué segundo parte)
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

        // Primera vuelta: de 0 al final
        if (!firstLoopDone)
        {
            if (!audioSource.isPlaying && audioSource.time > 0f)
            {
                firstLoopDone = true;
                audioSource.time = skipTime;
                audioSource.Play();
            }
        }
        // Desde la segunda vuelta en adelante: siempre desde skipTime
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