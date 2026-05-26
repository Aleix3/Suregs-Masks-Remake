using System.Collections.Generic;

[System.Serializable]
public class DialogueOption
{
    public string optionText;

    public List<DialogueSentence> nextSentences;
}