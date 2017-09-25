using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuSceneController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
	
	// Update is called once per frame
	void Update () {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting not supported in editor");
    }

    public void LoadScene(string scene)
    {
        FindObjectOfType<GameManager>().LoadSceneWithSplashScreen(scene);
    }
}
