using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    //velocidad de la barra de carga
    public float velocidadBarra = 10f;
    //menu de cargar la pantalla
    public GameObject menuCarga;
    //slider del menu de cargar la pantalla
    public Slider sliderMenuCarga;
    //menu que estaba activo anteriormente
    public GameObject previousPanel;
    //Botones a manejar en caso de hacer la navegabilidad hacia atras.
    public List<GameObject> botonesAManejar;
    //Botones que se usan para salir 
    public List<Button> botonesSalida;
    //Botones de los mapas
    public List<Button> botonesMapa;

    // Start is called before the first frame update
    void Start()
    {
        //A todos los botones de salida le asignamos la funci�n salir al hacer clic
        botonesSalida.ForEach(boton => boton.onClick.AddListener(() => Salir()));
        // a todos los botones de los mapas les asignamos la funci�n de cambiar mapa pasando el boton como parametro al hacer clic
        botonesMapa.ForEach(boton => boton.onClick.AddListener(() => CambiarMapa(boton)));
    }

    /// <summary>
    /// Funci�n encargada de cambiar de mapa seg�n el boton
    /// </summary>
    /// <param name="boton"></param>
    private void CambiarMapa(Button boton)
    {
        //Switch sobre el nombre del boton.
        //Si es el Mapa 1 asignamos la dificultad del juego a facil y la escena a Alex
        //Si es el Mapa 2 asignamos la dificultad del juego a Normal y la escena a Carles
        //Si es el Mapa 3 asignamos la dificultad del juego a Dificil y la escena a Marcos
        var escena = "";
        switch (boton.name)
        {
            case "Mapa 1":
                DifficultyManager.instance.SetDifficulty(Dificultad.Facil);
                escena = "Alex";
                break;
            case "Mapa 2":
                DifficultyManager.instance.SetDifficulty(Dificultad.Normal);
                escena = "Carles";
                break;
            case "Mapa 3":
                DifficultyManager.instance.SetDifficulty(Dificultad.Dificil);
                escena = "Marcos";
                break;
        }
        //despausamos el tiempo en caso de que est� pausado.
        Time.timeScale = 1;
        //Bloqueamos el rat�n
        Cursor.lockState = CursorLockMode.Locked;
        //Obtenemos el men� anterior
        previousPanel = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        //Llamamos a la corrutina encargada de cargar la escena.
        StartCoroutine(CargarEscenaAsync(escena));
    }

    /// <summary>
    /// Corrutina encargada de cargar una escena y actualizar la barra de carha
    /// </summary>
    /// <param name="escena">Nombre de la escena a cargar</param>
    /// <returns></returns>
    IEnumerator CargarEscenaAsync(string escena)
    {
        //Instancia la carga de la escena como una operaci�n asincronica.
        AsyncOperation operacion = SceneManager.LoadSceneAsync(escena);

        //Habilitamos el menu de carga
        menuCarga.SetActive(true);
        //Si existe un menu anterior, se desactiva
        if(previousPanel != null) previousPanel.SetActive(false);

        //mientras la operacion asincronica de cargar la escena no est� completa
        while (!operacion.isDone)
        {
            //Tomamos el valor de dividir el progreso de la operaci�n entre la velocidad de la barra
            //A continuaci�n fija el valor entre 0 y 1
            float valorProgreso = Mathf.Clamp01(operacion.progress / velocidadBarra);

            //actualiza el valor del slider al del progreso.
            sliderMenuCarga.value = valorProgreso;

            //no devuelve nada y espera al siguiente frame para realizar la operaci�n de cargar otra vez.
            yield return null;
        }

    }

    // En esta funci�n nos encargamos de la navegabilidad para poder hacer funcionar al bot�n de volver.
    void Update()
    {

        //Obtenemos el ultimo bot�n pulsado
        var button = EventSystem.current.currentSelectedGameObject;

        //Si el bot�n no es nulo
        if (button != null)
        {
            //Si el bot�n se encuentra en la lista de botones a manejar se asigna como panel anterior el padre del boton.
            if (botonesAManejar.Contains(button.gameObject)) previousPanel = button.transform.parent.gameObject;

            //si el nombre del boton es Volver
            if (button.name.Equals("Volver"))
            {
                //Se obtiene el panel actual
                var currentPanel = EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;

                //En caso de que el panel actual sea setting el panel actual pasa a ser el padre del panel actual
                if(currentPanel.name.Equals("Settings")) currentPanel = currentPanel.transform.parent.gameObject;
                //se desactiva el panel actual.
                currentPanel.SetActive(false);
                //se habilita el panel anterior.
                previousPanel.SetActive(true);
            }
        }

    }

    /// <summary>
    /// Funci�n encargada de avanzar en el mapa.
    /// </summary>
    public void AvanzarMapa()
    {

        //Obtiene el indice del build actual
        int sceneId = SceneManager.GetActiveScene().buildIndex;

        //Si el valor del indice es mayor o igual al numero de indices menos 1, el valor se pone en 0
        if (sceneId >= SceneManager.sceneCountInBuildSettings -1) sceneId = 0;
        //si no sumamos 1 al valor de la escena.
        else sceneId++;

        //Switch basado en el id de la escena. Asigna la dificultad al singleton de dificultad.
        //Si el id es 1 --> facil
        //Si el id es 2 --> Normal
        //Si el id es 3 --> Dificil
        switch (sceneId)
        {
            case 1:
                DifficultyManager.instance.SetDifficulty(Dificultad.Facil);
                break;
            case 2:
                DifficultyManager.instance.SetDifficulty(Dificultad.Normal);
                break;

           case 3:
                DifficultyManager.instance.SetDifficulty(Dificultad.Dificil);
                break;
        }

        //Obtenemos la dirrecci�n a la escena
        string pathToScene = SceneUtility.GetScenePathByBuildIndex(sceneId);
        //Obtenemos el nombre de la escena utilizando el path
        string nombreEscena = System.IO.Path.GetFileNameWithoutExtension(pathToScene);
        //Llamamos a la corrutina de cargar la escena de forma asincronica.
        StartCoroutine(CargarEscenaAsync(nombreEscena));
    }

    // Metodo para regresar al men� principal
    public void salirMenu()
    {
        SceneManager.LoadScene("StartScene");
    }
    // Metodo para recargar la escena activa
    public void restart()
    {
        previousPanel = GameObject.Find("Muerte");
        StartCoroutine(CargarEscenaAsync(SceneManager.GetActiveScene().name));
    }
	
	public void Salir()
    {
        Debug.Log("Saliendo...");
        Application.Quit();
    }
}
