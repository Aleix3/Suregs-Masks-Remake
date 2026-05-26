using TMPro;
using UnityEngine;

public class DialogueOptionButton : MonoBehaviour
{
    [SerializeField] private TMP_Text optionText;

    private DialogueOption currentOption;
    private DialogueManager dialogueManager;

    private Color normalColor;
    public Color hoverColor;

    public void Setup(DialogueOption option,
                      DialogueManager manager)
    {
        currentOption = option;
        dialogueManager = manager;

        optionText.text = option.optionText;
        normalColor = optionText.color;
    }

    public void Select()
    {
        dialogueManager.SelectOption(currentOption);
    }

    public void SetHover(bool isHover)
    {
        optionText.color = isHover ? hoverColor : normalColor;
    }
}