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
        if (sceneName == "Town")
        {
            if (Player.Instance.isFacingLeft)
            {
                Player.Instance.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            }
            else
            {
                Player.Instance.transform.localScale = new Vector3(-0.55f, 0.55f, 0.55f);
            }
        }
        SceneManager.LoadScene(sceneName); 
    }
}