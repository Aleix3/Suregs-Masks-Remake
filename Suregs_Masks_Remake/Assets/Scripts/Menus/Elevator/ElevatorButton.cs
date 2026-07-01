using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ElevatorButton : MonoBehaviour
{
    public bool locked;
    public string sceneName;
    public int levelIndex;

    [Header("Navigation")]
    public ElevatorButton up;
    public ElevatorButton down;
    public ElevatorButton left;
    public ElevatorButton right;
    void Start()
    {
        locked = !ElevatorLevelProgress.IsUnlocked(levelIndex);

        if(!locked)
        {
            GetComponent<Image>().enabled = false;
        }
    }


    public void Press()
    {
        if (locked)
            return;

        if (SceneManager.GetActiveScene().name == sceneName)
            return;
        Player.Instance.canMove = true;
        SceneManager.LoadScene(sceneName); 
    }
}