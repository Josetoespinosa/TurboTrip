using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    public static LevelTimer Instance; 
    private float elapsedTime = 0f;
    private bool isRunning = false;
    private bool finished = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        if (isRunning && !finished)
        {
            elapsedTime += Time.deltaTime;
        }
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        isRunning = true;
        finished = false;
        Debug.Log($"Timer reset - starting at 0 seconds");
    }

    public void FinishLevel()
    {
        if (!finished)
        {
            finished = true;
            isRunning = false;
            Debug.Log($"Level finished at {elapsedTime:F2} seconds");
        }
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }
}