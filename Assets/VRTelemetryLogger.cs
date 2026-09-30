using UnityEngine;
using System.IO;
using System.Text;

public class VRTelemetryLogger : MonoBehaviour
{
    [Header("Referencias y Configuración")]
    public Transform vrCamera; 
    public float intervalSeconds = 1.0f;

    [Header("Configuración del Sensor (IMU)")]
    [Tooltip("Intervalo del Giroscopio en segundos (0.01s = 100 Hz, 0.005s = 200 Hz)")]
    public float imuUpdateInterval = 0.01f; 

    private DriveUploader uploader;
    private string filePath;
    private Quaternion initialRotation;
    private bool initialized = false;

    private StringBuilder logBuffer = new StringBuilder();

    // Acumuladores de rendimiento
    private int frameCounter = 0;
    private float timeCounter = 0f;
    private float lastCalculatedFPS = 60f;
    private float lastLatRender = 16.66f;

    // Cache para lectura de Batería en Android
    #if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject currentActivity;
    private AndroidJavaObject intentFilter;
    #endif

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        uploader = GetComponent<DriveUploader>();
        filePath = Path.Combine(Application.persistentDataPath, "vr_telemetry_log.csv");

        // Borrar el archivo local anterior para iniciar una sesión completamente limpia
        if (File.Exists(filePath))
        {
            try { File.Delete(filePath); } catch {}
        }

        logBuffer.Clear();
        logBuffer.AppendLine("Timestamp_s,FPS,latIMU,latRender,latPantalla,IMU_Hz,BatteryTemp_C,AngularVelocity_deg_s,DriftAngle_Degrees");

        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            Input.gyro.updateInterval = imuUpdateInterval;
        }

        #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }
            intentFilter = new AndroidJavaObject("android.content.IntentFilter", "android.intent.action.BATTERY_CHANGED");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[VRTelemetryLogger] Error inicializando JNI Batería: " + e.Message);
        }
        #endif

        Invoke(nameof(SetInitialRotation), 0.5f);
    }

    void Update()
    {
        frameCounter++;
        timeCounter += Time.unscaledDeltaTime;

        if (timeCounter >= intervalSeconds)
        {
            lastCalculatedFPS = frameCounter / timeCounter;
            lastLatRender = (timeCounter / frameCounter) * 1000f; // Latencia de renderizado (ms)

            LogTelemetry();

            frameCounter = 0;
            timeCounter = 0f;
        }
    }

    void SetInitialRotation()
    {
        if (vrCamera != null)
        {
            initialRotation = vrCamera.rotation;
            initialized = true;
        }
    }

    void LogTelemetry()
    {
        float temp = GetAndroidBatteryTemperature();
        float angularVelocity = 0f;
        float driftAngle = 0f;

        // Calculo de Frecuencia del IMU (IMU_Hz) y latIMU (ms)
        float imuHz = 60f;
        float latIMU = 16.67f;

        if (SystemInfo.supportsGyroscope && Input.gyro.updateInterval > 0f)
        {
            imuHz = 1.0f / Input.gyro.updateInterval;
            latIMU = Input.gyro.updateInterval * 1000f;
        }

        // Calculo de Latencia de Pantalla (latPantalla en ms)
        float refreshRate = 60f;
        #if UNITY_2022_2_OR_NEWER
        if (Screen.currentResolution.refreshRateRatio.value > 0)
        {
            refreshRate = (float)Screen.currentResolution.refreshRateRatio.value;
        }
        #else
        if (Screen.currentResolution.refreshRate > 0)
        {
            refreshRate = (float)Screen.currentResolution.refreshRate;
        }
        #endif
        float latPantalla = 1000f / refreshRate;

        // Telemetria de movimiento
        if (initialized && vrCamera != null)
        {
            driftAngle = Quaternion.Angle(initialRotation, vrCamera.rotation);
            if (SystemInfo.supportsGyroscope)
            {
                angularVelocity = Input.gyro.rotationRate.magnitude * Mathf.Rad2Deg;
            }
        }

        // Escritura formateada en el buffer
        logBuffer.AppendLine($"{Time.timeSinceLevelLoad:F2},{lastCalculatedFPS:F1},{latIMU:F2},{lastLatRender:F2},{latPantalla:F2},{imuHz:F0},{temp:F1},{angularVelocity:F3},{driftAngle:F3}");
    }

    float GetAndroidBatteryTemperature()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        if (currentActivity != null && intentFilter != null)
        {
            using (AndroidJavaObject batteryIntent = currentActivity.Call<AndroidJavaObject>("registerReceiver", null, intentFilter))
            {
                if (batteryIntent != null)
                {
                    int temp = batteryIntent.Call<int>("getIntExtra", "temperature", 0);
                    return temp / 10.0f;
                }
            }
        }
        #endif
        return 0f;
    }

    void SaveLogToDisk()
    {
        if (logBuffer.Length > 0)
        {
            File.WriteAllText(filePath, logBuffer.ToString());
        }
    }

    void OnApplicationQuit()
    {
        SaveLogToDisk();
        TriggerUpload();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveLogToDisk();
            TriggerUpload();
        }
    }

    public void TriggerUpload()
    {
        if (uploader != null && File.Exists(filePath))
        {
            string fileName = $"telemetry_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
            uploader.UploadCSV(filePath, fileName);
        }
    }
}