using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string splashSceneName;
    public float splashTime = 3;
    

    void Awake()
    {
        // enforce only one object that stays alive thru scene loads, new ones are killed
        GameObject[] objs = GameObject.FindGameObjectsWithTag("GameManager");
        if (objs.Length > 1)
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);
    }

    // Use this for initialization
    void Start()
    {
    }

    void OnEnable()
    {
        //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        // might not need this
    }

    public void LoadSceneWithSplashScreen(string scene)
    {
        Debug.Log("Loading Scene " + scene);
        StartCoroutine(ShowSplashScreenWhileLoading(scene));
    }

    public void LoadScene(string scene)
    {
        Debug.Log("Loading Scene " + scene);
        SceneManager.LoadScene(scene);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting not supported in editor");
    }

    IEnumerator ShowSplashScreenWhileLoading(string resultingScene)
    {
        SceneManager.LoadScene(splashSceneName, LoadSceneMode.Single);
        Time.timeScale = 1f;
        yield return new WaitForSeconds(splashTime);
        SceneManager.LoadScene(resultingScene, LoadSceneMode.Single);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
