using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public GameObject pausePanel;
    public GameObject levelCompletePanel;
    public GameObject deathPanel;
    public GameObject hud;
    public Text hpText;
    PlayerController player;
    public Image gunEnabledIndicator;
    public Image latchedIndicator;
    public Image screenFlash;
    float screenFlashVisibility = 0f;
    bool menuShown;
    

    // Use this for initialization
    void Start ()
    {
        player = FindObjectOfType<PlayerController>();
        latchedIndicator.color = Color.green;
        gunEnabledIndicator.color = Color.red;
        latchedIndicator.enabled = false;
        screenFlash.enabled = false;
        menuShown = false;
        pausePanel.SetActive(false);
        levelCompletePanel.SetActive(false);
        deathPanel.SetActive(false);

        Messenger.AddListener(GameEvent.LEVEL_COMPLETE, ShowLevelCompleteMenu);
        Messenger.AddListener(GameEvent.PLAYER_DEATH, ShowDeathMenu);
    }

    void OnDestroy()
    {
        Messenger.RemoveListener(GameEvent.LEVEL_COMPLETE, ShowLevelCompleteMenu);
        Messenger.RemoveListener(GameEvent.PLAYER_DEATH, ShowDeathMenu);
    }
    
    // Update is called once per frame
    void Update () {
        latchedIndicator.enabled = player.state == PlayerController.PlayerState.Latched;
        gunEnabledIndicator.color = player.gunEnabled ? Color.green : Color.red;
        
		if(menuShown)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (Input.GetButtonDown("Cancel") && pausePanel.activeInHierarchy)
            {
                HideMenu();
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (Input.GetButtonDown("Cancel"))
            {
                ShowPauseMenu();
            }
        }

        if (screenFlash.enabled)
        {
            if (screenFlashVisibility <= 0f)
            {
                screenFlash.enabled = false;
                screenFlashVisibility = 0f;
            }
            Color temp = screenFlash.color;
            temp.a = Mathf.Lerp(0, 0.2f, screenFlashVisibility);
            screenFlash.color = temp;

            screenFlashVisibility -= Time.deltaTime * 3;
        }
        
    }

    public void Restart()
    {
        FindObjectOfType<GameManager>().LoadSceneWithSplashScreen(SceneManager.GetActiveScene().name);
    }

    public void LoadScene(string scene)
    {
        Debug.Log("Loading Scene " + scene);
        SceneManager.LoadScene(scene);
        HideMenu();
    }

    public void LoadSceneWithSplashScreen(string scene)
    {
        FindObjectOfType<GameManager>().LoadSceneWithSplashScreen(scene);
        HideMenu();
    }

    public void HideMenu()
    {
        StopAllCoroutines(); // so timeScale isn't messed with later
        pausePanel.SetActive(false);
        levelCompletePanel.SetActive(false);
        menuShown = false;
        hud.SetActive(true);
        Time.timeScale = 1.0f;
        Input.ResetInputAxes();
    }

    public void ShowPauseMenu()
    {
        pausePanel.SetActive(true);
        Input.ResetInputAxes();
        menuShown = true;
        hud.SetActive(false);
        Time.timeScale = 0.0f;
    }

    public void ShowLevelCompleteMenu()
    {
        levelCompletePanel.SetActive(true);
        Input.ResetInputAxes();
        menuShown = true;
        hud.SetActive(false);
        Time.timeScale = 0f;
    }

    public void ShowDeathMenu()
    {
        deathPanel.SetActive(true);
        Input.ResetInputAxes();
        hud.SetActive(false);
        StartCoroutine(DisableInSeconds(2f));
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting not supported in editor");
    }

    IEnumerator DisableInSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Time.timeScale = 0f;
        menuShown = true;
    }
    public void DamageFlash()
    {
        screenFlashVisibility = 1f;
        screenFlash.enabled = true;
    }
}
