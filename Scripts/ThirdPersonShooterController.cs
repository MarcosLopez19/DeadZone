using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;
using UnityEngine.InputSystem;

public class ThirdPersonShooterController : MonoBehaviour {

    // C�mara virtual para apuntar
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    // Sensibilidad normal
    [SerializeField] private float normalSensitivity;
    // Sensibilidad al apuntar
    [SerializeField] private float aimSensitivity;
    // M�scara de capas para el colisionador de apuntar
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    // Prefab del proyectil de bala
    [SerializeField] private Transform pfBulletProjectile;
    // Posici�n de generaci�n del proyectil de bala
    private Transform spawnBulletPosition;
    // Efecto visual al impactar en el objetivo
    [SerializeField] private Transform vfxHitTarget;
    // Referencia al controlador de tercera persona
    private ThirdPersonController thirdPersonController;
    // Referencia a los inputs del jugador
    private StarterAssetsInputs starterAssetsInputs;
    // Referencia al animador
    private Animator animator;
    // Objeto que referencia la particula de disparo.
    public GameObject disparo;
    // Objeto que referencia el gestor de armas
    public GameObject weaponManager;
    // Fuente de audio para el disparo
    public AudioSource audioDisparo;
    // Referencia al script del arma
    private Arma arma;

    /// <summary>
    /// Cambia el arma actual por la nueva arma especificada.
    /// </summary>
    /// <param name="arma">La nueva arma a equipar.</param>
    public void ChangeWeapon(Arma arma)
    {
        // Obtiene el componente Arma de la nueva arma
        this.arma = arma.GetComponent<Arma>();
        // Obtiene el componente AudioSource de la nueva arma
        audioDisparo = arma.GetComponent<AudioSource>();
        // Obtiene la posici�n de generaci�n del proyectil de bala de la nueva arma
        spawnBulletPosition = arma.gameObject.transform.GetChild(0).transform;
    }

    /// <summary>
    /// Configura el estado del cursor, obtiene referencias a componentes necesarios 
    /// y establece la posici�n de generaci�n del proyectil de bala.
    /// </summary>
    private void Awake() {
        // Bloquea el cursor en el centro de la pantalla
        Cursor.lockState = CursorLockMode.Locked;
        // Obtiene el componente Arma del arma actualmente equipada
        arma = weaponManager.GetComponentInChildren<Arma>();
        // Obtiene el componente ThirdPersonController
        thirdPersonController = GetComponent<ThirdPersonController>();
        // Obtiene el componente StarterAssetsInputs
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        // Obtiene el componente Animator
        animator = GetComponent<Animator>();
        // Obtiene la posici�n de generaci�n del proyectil de bala del arma actual
        spawnBulletPosition = arma.gameObject.transform.GetChild(0).transform;
    }

    /// <summary>
    /// Metodo encargado de gestionar el apuntado y disparo del arma
    /// </summary>
    private void Update() {

        // Si se ha pulsado el bot�n de disparar pero no se est� apuntando,
        // cambia el valor del bot�n de disparar a falso.
        if (!starterAssetsInputs.aim && starterAssetsInputs.shoot) starterAssetsInputs.shoot = false;

       if (starterAssetsInputs.aim) {
            // Establece el par�metro "aimingPistol" en el animador seg�n el tipo de arma equipada
            animator.SetBool("aimingPistol", arma.tipoArma == Arma.Tipos.Pistola);
            // Establece el par�metro "aimingRifle" en el animador seg�n el tipo de arma equipada
            animator.SetBool("aimingRifle", arma.tipoArma == Arma.Tipos.Rifle);

            // Variable que almacena la posici�n en el mundo del rat�n
            Vector3 mouseWorldPosition = Vector3.zero;

            // Punto central de la pantalla en coordenadas 2D
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);

            // Rayo que parte desde la c�mara y apunta hacia el punto central de la pantalla
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            // Transform que representa el objeto alcanzado por el rayo
            Transform hitTransform = null;

            // Realiza un lanzamiento de rayo y comprueba si colisiona con un objeto de la capa de aim
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
            {
                // La posici�n del rat�n en el mundo se actualiza con el punto de colisi�n del rayo
                mouseWorldPosition = raycastHit.point;
                // El objeto alcanzado por el rayo se asigna al Transform 
                hitTransform = raycastHit.transform;
            }
            // Si el rayo no colisiona con ning�n objeto, la posici�n del rat�n en el mundo
            // se establece en un punto a una distancia de 10 unidades
            else { mouseWorldPosition = ray.GetPoint(10); }

            // Activa la c�mara virtual
            aimVirtualCamera.gameObject.SetActive(true);

            // Establece la sensibilidad de rotaci�n del controlador de tercera persona
            // al valor de sensibilidad de apuntado
            thirdPersonController.SetSensitivity(aimSensitivity);
            // Desactiva la rotaci�n autom�tica al moverse
            thirdPersonController.SetRotateOnMove(false);

            // Calcula la posici�n objetivo de apuntado en el mundo,
            // manteniendo la misma altura del objeto en el que se encuentra el script
            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;

            // Calcula la direcci�n de apuntado normalizada hacia el objetivo
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            // Interpola suavemente la direcci�n actual del objeto hacia la direcci�n de apuntado
            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);

            // Verifica si el "calor" del arma es mayor a 0
            // (esta caliente, no puede disparar)
            if (arma.gunHeat > 0)
            {
                //Reduce el valor segun el tiempo que est� pasando.
                arma.gunHeat -= Time.deltaTime;
            }

            // Verifica si se ha presionado el bot�n de disparo
            // y el calor del arma es menor o igual a cero
            if (starterAssetsInputs.shoot && arma.gunHeat <= 0)
            {
                // Establece el calor del arma al tiempo entre disparos configurado en el arma
                arma.gunHeat = arma.tiempoEntreDisparos;

                // Reproduce el sonido de disparo
                audioDisparo.Play();
                // Establece el tiempo de finalizaci�n programado del sonido del disparo
                audioDisparo.SetScheduledEndTime(AudioSettings.dspTime + (0.5f));

                // Calcula la direcci�n de apuntado normalizada hacia la posici�n de generaci�n de la bala
                Vector3 aimDir = (mouseWorldPosition - spawnBulletPosition.position).normalized;

                // Instancia la part�cula de disparo en la posici�n de generaci�n de la bala
                // y con la direcci�n de apuntado
                var disparoparticle = 
                    Instantiate(disparo, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));

                // Destruye la part�cula de disparo despu�s de 0.1 segundos
                Destroy(disparoparticle,0.1f);

                // Instancia el proyectil de bala en la posici�n de generaci�n de la bala
                // y con la direcci�n de apuntado
                var bullet = Instantiate(pfBulletProjectile, spawnBulletPosition.position, 
                    Quaternion.LookRotation(aimDir, Vector3.up));

                // Restablece el estado de la variable de disparo en false
                starterAssetsInputs.shoot = false;
            }
        } else {
            // Si el tipo de arma es una pistola,
            // establece el estado de animaci�n de apuntado de pistola en falso
            if (arma.tipoArma == Arma.Tipos.Pistola) animator.SetBool("aimingPistol", false);

            // Si el tipo de arma es un rifle,
            // establece el estado de animaci�n de apuntado de rifle en falso
            else animator.SetBool("aimingRifle", false);

            // Desactiva la c�mara virtual de apuntado
            aimVirtualCamera.gameObject.SetActive(false);

            // Restablece la sensibilidad del controlador de tercera persona a la sensibilidad normal
            thirdPersonController.SetSensitivity(normalSensitivity);

            // Habilita la rotaci�n autom�tica del objeto cuando se mueve
            thirdPersonController.SetRotateOnMove(true);
            //animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
        }

       
    }

    public void SetpfBulletProjectile(Transform bullet)
    {
        this.pfBulletProjectile = bullet;
    }

}