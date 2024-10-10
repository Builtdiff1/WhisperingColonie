using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management

public class SceneSwitcher : MonoBehaviour
{
    // Name of the scene to load after the delay
    public string sceneToLoad;

    // Delay time before switching scene (in seconds)
    public float delayTime = 5f;

    void Start()
    {
        // Invoke the method to switch scene after the delay time
        Invoke("SwitchScene", delayTime);
    }

    // Method to switch scene
    void SwitchScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}