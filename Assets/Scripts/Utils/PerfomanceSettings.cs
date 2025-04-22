using UnityEngine;

public class PerfomanceSettings : MonoBehaviour
{
    void Awake()
        {
            QualitySettings.vSyncCount = 1; // Disable VSync
            Application.targetFrameRate = 120; // Cap FPS to 120

        }
}
