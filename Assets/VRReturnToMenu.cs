using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VRReturnToMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Material Inicial")]
    [SerializeField] private Material matInicio; // Arrastra aquí el material Mat_inicio

    [Header("Elementos del Menú Inicial")]
    [SerializeField] private GameObject[] menuElements; // Elementos a reactivar (BotonPlaya, BotonCosta, etc.)

    [Header("Botón de Volver")]
    [SerializeField] private GameObject returnButtonObject; // El objeto contenedor de este botón para ocultarlo en el menú

    [Header("Configuración de Mirada (Gaze)")]
    [SerializeField] private float dwellTime = 2.5f; // Tiempo mirando para activar
    [SerializeField] private Image radialProgressImage; // Imagen de carga circular (opcional)

    [Header("Telemetría")]
    [SerializeField] private DriveUploader driveUploader;

    private bool isGazing = false;
    private float gazeTimer = 0f;

    private void Update()
    {
        if (isGazing)
        {
            gazeTimer += Time.deltaTime;

            if (radialProgressImage != null)
            {
                radialProgressImage.fillAmount = Mathf.Clamp01(gazeTimer / dwellTime);
            }

            if (gazeTimer >= dwellTime)
            {
                isGazing = false;
                ExecuteReturnToMenu();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isGazing = true;
        gazeTimer = 0f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetGaze();
    }

    private void ResetGaze()
    {
        isGazing = false;
        gazeTimer = 0f;
        if (radialProgressImage != null)
        {
            radialProgressImage.fillAmount = 0f;
        }
    }

    public void ExecuteReturnToMenu()
    {
        // 1. Cambiar el Skybox de vuelta al material inicial
        if (matInicio != null)
        {
            RenderSettings.skybox = matInicio;
            DynamicGI.UpdateEnvironment(); // Actualiza la iluminación global de la escena
        }

        // 2. Volver a mostrar los botones del menú e instrucciones
        if (menuElements != null)
        {
            foreach (GameObject element in menuElements)
            {
                if (element != null) element.SetActive(true);
            }
        }

        // 3. Enviar la telemetría acumulada a Google Drive
        if (driveUploader != null)
        {
            string localPath = Application.persistentDataPath + "/telemetry_current.csv";
            string driveName = $"Telemetry_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
            driveUploader.UploadCSV(localPath, driveName);
        }

        // 4. Ocultar el botón de volver ya que estamos en el menú
        ResetGaze();
        if (returnButtonObject != null)
        {
            returnButtonObject.SetActive(false);
        }
    }
}