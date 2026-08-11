using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public List<Enemy> enemiesInRoom = new List<Enemy>();
    public bool isPlayerInRoom = false;
    public bool isBossRoom = false;
    private void Awake()
    {
        enemiesInRoom.Clear();

        Enemy[] enemies = GetComponentsInChildren<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            enemiesInRoom.Add(enemy);
            enemy.roomConected = this;
        }
    }

    public void Refresh()
    {
        enemiesInRoom.Clear();

        Enemy[] enemies = GetComponentsInChildren<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            enemiesInRoom.Add(enemy);
            enemy.roomConected = this;
        }
    }
}
