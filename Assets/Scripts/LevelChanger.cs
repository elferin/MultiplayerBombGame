using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class LevelChanger : MonoBehaviour
{
    [SerializeField] string sceneName = "Test1";
    [SerializeField] string playerTag = "Player";

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            LoadLevel();
    }

    public void LoadLevel()
    {
        SceneManager.LoadScene(sceneName);
    }
}
