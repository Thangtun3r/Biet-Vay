using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class StartMenu : MonoBehaviour
{
    [YarnCommand("startGame")]
    public static void startGame()
    {
        SceneManager.LoadScene("Vignette1");
    }
    [YarnCommand("quit")]
    public static void quitGame()
    {
        Application.Quit();
    }
    
    [YarnCommand("loadScene")]
    public static void quitGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    
}
