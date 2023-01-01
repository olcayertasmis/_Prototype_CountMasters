using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject startMenuObj;
 
    public void StartTheGame()
    {
        startMenuObj.SetActive(false);
        PlayerManager.PlayerManagerInstance.gameState = true;

        PlayerManager.PlayerManagerInstance.player.GetChild(1).GetComponent<Animator>().SetBool("run", true);
    }

    public void RestartTheGame()
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
}
