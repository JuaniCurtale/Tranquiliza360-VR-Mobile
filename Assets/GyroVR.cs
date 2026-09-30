using UnityEngine;

public class GyroVR : MonoBehaviour
{
    private bool gyroSupported;
    private readonly Quaternion baseOffset = Quaternion.Euler(90f, 0f, 0f);

    void Start()
    {
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
            Quaternion gyro = Input.gyro.attitude;
            // Cálculo directo de Quaternions sin forzar recalculación de matrices mundiales
            transform.localRotation = baseOffset * new Quaternion(gyro.x, gyro.y, -gyro.z, -gyro.w);
        }
        else
        {
            Vector3 accel = Input.acceleration;
            float pitch = Mathf.Atan2(-accel.y, -accel.z) * Mathf.Rad2Deg;
            float roll = Mathf.Atan2(accel.x, -accel.z) * Mathf.Rad2Deg;

            transform.localRotation = Quaternion.Euler(pitch, roll, 0f);
        }
    }
}