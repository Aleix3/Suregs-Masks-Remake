using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskTree : MonoBehaviour
{
    [SerializeField] private GameObject treeCanvas;

    private bool playerInside;

    public MaskTreeUI maskTreeUi;

    private void Update()
    {
        if (!playerInside)
            return;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("Fire1"))
        {
            treeCanvas.SetActive(true);
            
            Player.Instance.canMove = false;
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Player.Instance.canMove = true;
            maskTreeUi.ClearHovers();
            treeCanvas.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = false;
        }
    }
}
