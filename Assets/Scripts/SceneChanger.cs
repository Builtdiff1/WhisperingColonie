using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management
using UnityEngine.UI; // Required for UI elements

public class SceneChanger : MonoBehaviour
{
    // Reference to the button
    public Button yourButton;

    // Name of the scene to load
    public string sceneToLoad;

    void Start()
    {
        // Ensure the button is not null before adding the listener
        if (yourButton != null)
        {
            yourButton.onClick.AddListener(OpenNewScene);
        }
    }

    // Method to load the new scene
    void OpenNewScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}