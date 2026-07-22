using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonSequenceManager : MonoBehaviour
{
    [Header("Secuencia de botones (en el ORDEN correcto)")]
    [SerializeField] private List<FloorButton> buttonSequence = new List<FloorButton>();

    [SerializeField] private Note itemPrefab;
    public int noteId;
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private float wrongDelay = 0.5f;

    public UnityEvent onSequenceCompleted;
    public UnityEvent onWrongButton;
    public UnityEvent onCorrectButton;

    private int currentIndex = 0;
    private bool sequenceCompleted = false;
    private bool isResetting = false;

    public void OnButtonPressed(FloorButton button)
    {
        if (sequenceCompleted || isResetting) return;

        bool isCorrect = buttonSequence[currentIndex] == button;

        if (isCorrect)
        {
            button.SetPressed();
            currentIndex++;
            onCorrectButton?.Invoke();

            if (currentIndex >= buttonSequence.Count)
            {
                CompleteSequence();
            }
        }
        else
        {
            button.SetPressed();
            StartCoroutine(WrongSequenceRoutine());
        }
    }

    private IEnumerator WrongSequenceRoutine()
    {
        isResetting = true;
        onWrongButton?.Invoke();

        yield return new WaitForSeconds(wrongDelay);

        ResetAllButtons();
        isResetting = false;
    }

    private void ResetAllButtons()
    {
        currentIndex = 0;
        foreach (var btn in buttonSequence)
        {
            if (btn != null) btn.SetNormal();
        }
    }

    private void CompleteSequence()
    {
        sequenceCompleted = true;

        if (itemPrefab != null)
        {
            itemPrefab.id = noteId;
            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            Instantiate(itemPrefab, pos, Quaternion.identity);
            
        }

        onSequenceCompleted?.Invoke();
    }

    public void ResetGame()
    {
        sequenceCompleted = false;
        isResetting = false;
        StopAllCoroutines();
        ResetAllButtons();
    }
}
