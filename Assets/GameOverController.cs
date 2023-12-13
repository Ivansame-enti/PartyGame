using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    public void RestartButton()
    {
        SceneManager.LoadScene("ArenaPowerLevel");
    }

    public void ExitMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
