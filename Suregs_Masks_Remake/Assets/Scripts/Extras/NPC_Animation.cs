using UnityEngine;

public class NPCAnimation : MonoBehaviour
{
    public Sprite[] frames;
    public float fps = 8f;

    private SpriteRenderer sr;
    private int currentFrame;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        InvokeRepeating(nameof(NextFrame), 0f, 1f / fps);
    }

    void NextFrame()
    {
        if (frames.Length == 0)
            return;

        currentFrame++;

        if (currentFrame >= frames.Length)
            currentFrame = 0;

        sr.sprite = frames[currentFrame];
    }
}