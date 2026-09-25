using UnityEngine;

public class GyroVR : MonoBehaviour
{
    private bool gyroSupported;

    void Start()
    {
        // Verifica si el celular tiene giroscopio hardware
        gyroSupported = SystemInfo.supportsGyroscope;
        if (gyroSupported)
        {
            Input.gyro.enabled = true;
        }
    }

    void Update()
    {
        if (gyroSupported)
        {
            // Lectura por Giroscopio
            Quaternion gyro = Input.gyro.attitude;
            transform.localRotation = new Quaternion(gyro.x, gyro.y, -gyro.z, -gyro.w);
            transform.Rotate(90f, 0f, 0f, Space.World);
        }
        else
        {
            // Fallback por Acelerómetro (para celulares Moto G sin giroscopio físico)
            Vector3 accel = Input.acceleration;

            // Calcula la inclinación vertical (Pitch) y horizontal (Roll)
            float pitch = Mathf.Atan2(-accel.y, -accel.z) * Mathf.Rad2Deg;
            float roll = Mathf.Atan2(accel.x, -accel.z) * Mathf.Rad2Deg;

            transform.localRotation = Quaternion.Euler(pitch, roll, 0f);
        }
    }
}