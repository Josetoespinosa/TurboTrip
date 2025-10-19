using UnityEngine;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public TMP_Text timeText;

    void Start()
    {
        if (LevelTimer.Instance != null)
        {
            float elapsedTime = LevelTimer.Instance.GetElapsedTime();
            timeText.text = $" ¡NIVEL 1 COMPLETADO! \n \n Tiempo total: {elapsedTime:F2} segundos";
        }
        else
        {
            timeText.text = "Tiempo no disponible";
        }
    }
}