using UnityEngine;
using UnityEngine.UI; // Agregado para controlar la UI del puntero

public class VRSelector : MonoBehaviour
{
    [Header("Entorno Inicial")]
    public Material skyboxInicio;

    [Header("Entorno 1: Playa")]
    public Material skyboxPlaya;
    public AudioClip audioPlaya;

    [Header("Entorno 2: Costa / Bosque")]
    public Material skyboxBosque;
    public AudioClip audioBosque;

    [Header("Entorno 3: Cielo Estrellado")]
    public Material skyboxEstrellado;
    public AudioClip audioEstrellado;

    [Header("Elementos UI a Ocultar")]
    public GameObject botonPlaya;
    public GameObject botonCosta;
    public GameObject botonEstrellado;
    public GameObject textoInstrucciones;

    [Header("Componentes VR")]
    public AudioSource audioSource;
    public Camera mainCamera;
    
    [Header("Puntero / Feedback Visual")]
    public Image punteroCarga; // Arrastra aquí la Image del puntero (ponle Image Type: Filled)
    public float rayDistance = 20f; // Distancia máxima de detección

    private float timer = 0f;
    private float timeToSelect = 2f;
    private GameObject currentTarget;

    void Start()
    {
        if (skyboxInicio != null)
        {
            RenderSettings.skybox = skyboxInicio;
            DynamicGI.UpdateEnvironment();
        }

        if (punteroCarga != null)
        {
            punteroCarga.fillAmount = 0f;
        }
    }

    void Update()
    {
        if (mainCamera == null) return;

        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            // Verificamos si lo que miramos es alguno de los botones interactivos
            if (EsBotonValido(hit.transform.name))
            {
                if (currentTarget == hit.transform.gameObject)
                {
                    timer += Time.deltaTime;

                    // Actualiza la barra/círculo de carga visual (de 0 a 1)
                    if (punteroCarga != null)
                    {
                        punteroCarga.fillAmount = timer / timeToSelect;
                    }

                    if (timer >= timeToSelect)
                    {
                        ActivarOpcion(hit.transform.name);
                        ResetTimer();
                    }
                }
                else
                {
                    currentTarget = hit.transform.gameObject;
                    timer = 0f;
                }
            }
            else
            {
                ResetTimer();
            }
        }
        else
        {
            ResetTimer();
        }
    }

    bool EsBotonValido(string nombre)
    {
        return nombre == "BotonPlaya" || nombre == "BotonCosta" || nombre == "BotonBosque" || nombre == "BotonEstrellas" || nombre == "BotonEstrellado";
    }

    void ResetTimer()
    {
        currentTarget = null;
        timer = 0f;
        if (punteroCarga != null)
        {
            punteroCarga.fillAmount = 0f;
        }
    }

    void ActivarOpcion(string nombreOpcion)
    {
        if (nombreOpcion == "BotonPlaya")
        {
            CambiarEntorno(skyboxPlaya, audioPlaya);
        }
        else if (nombreOpcion == "BotonCosta" || nombreOpcion == "BotonBosque")
        {
            CambiarEntorno(skyboxBosque, audioBosque);
        }
        else if (nombreOpcion == "BotonEstrellas" || nombreOpcion == "BotonEstrellado")
        {
            CambiarEntorno(skyboxEstrellado, audioEstrellado);
        }

        OcultarInterfaz();
        DynamicGI.UpdateEnvironment();
    }

    void CambiarEntorno(Material nuevoSkybox, AudioClip nuevoAudio)
    {
        if (nuevoSkybox != null)
        {
            RenderSettings.skybox = nuevoSkybox;
        }

        if (audioSource != null && nuevoAudio != null)
        {
            if (audioSource.clip != nuevoAudio || !audioSource.isPlaying)
            {
                audioSource.clip = nuevoAudio;
                audioSource.Play();
            }
        }
    }

    void OcultarInterfaz()
    {
        if (botonPlaya != null) botonPlaya.SetActive(false);
        if (botonCosta != null) botonCosta.SetActive(false);
        if (botonEstrellado != null) botonEstrellado.SetActive(false);
        if (textoInstrucciones != null) textoInstrucciones.SetActive(false);
        if (punteroCarga != null) punteroCarga.gameObject.SetActive(false); // Oculta la mira al entrar al entorno
    }
}