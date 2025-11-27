using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    public static LevelTimer Instance; 
    private float startTime;
    private float endTime;
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

    public void ResetTimer()
    {
        startTime = Time.time;
        finished = false;
        Debug.Log($"Timer reset at Time.time = {Time.time}");
    }

    public void FinishLevel()
    {
        if (!finished)
        {
            finished = true;
            endTime = Time.time;
            Debug.Log($"Level finished at {GetElapsedTime()} seconds");
        }
    }

    public float GetElapsedTime()
    {
        if (finished)
            return endTime - startTime;
        else
            return Time.time - startTime;
    }
}