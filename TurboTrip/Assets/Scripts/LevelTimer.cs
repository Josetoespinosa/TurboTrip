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
        }
    }

    void Start()
    {
        startTime = Time.time;
    }

    public void FinishLevel()
    {
        if (!finished)
        {
            finished = true;
            endTime = Time.time;
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