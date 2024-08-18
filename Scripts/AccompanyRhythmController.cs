using System.Collections;
using UnityEngine;

public class AccompanyRhythmController : MonoBehaviour
{
    #region Variables
    // BPM value
    public float beatsPerMinute = 150f;
    // Amount of beat size increment
    public float beatSizeIncrement = 0.1f;
    public bool scaleUp = true;

    // Beat interval
    private float beatInterval;
    // Original scale
    private Vector3 originalScale;
    // Target scale
    private Vector3 targetScale;
    // Only initiates the event. May be unnecessary for your scenario.
    private DialogueUIForBattle dialogueUIForBattle;
    #endregion

    private void Awake()
    {
        dialogueUIForBattle = GameObject.FindWithTag("BattleCanvas").GetComponent<DialogueUIForBattle>();
    }

    void Start()
    {
        // Stores the original scale.
        originalScale = transform.localScale;

        // Calculates beat interval based on BPM.
        beatInterval = 60f / beatsPerMinute;

        StartCoroutine(StartRhythm());
    }

    IEnumerator StartRhythm()
    {
        // Checks the algorithm based on a boolean value from another script. May be unnecessary for your scenario.
        while (dialogueUIForBattle.startBeatObjects)
        {
            // Changes the scale suddenly.
            ChangeScale();

            // Gradually returns to the original scale.
            yield return StartCoroutine(ReturnToOriginalScale());

            // Wait until the next beat time.
            yield return new WaitForSeconds(beatInterval);

            // If the algorithm should not run, proceeds to the next frame.
            yield return null;
        }
    }

    void ChangeScale()
    {
        // Target scale.
        if (scaleUp)
            targetScale = transform.localScale + new Vector3(beatSizeIncrement, beatSizeIncrement, beatSizeIncrement);
        else
            targetScale = transform.localScale - new Vector3(beatSizeIncrement, beatSizeIncrement, beatSizeIncrement);

        // Change the scale.
        transform.localScale = targetScale;
    }

    IEnumerator ReturnToOriginalScale()
    {
        // Starting and target scales.
        Vector3 startScale = targetScale;

        // Scale over the transition period.
        float elapsedTime = 0f;
        while (elapsedTime < beatInterval)
        {
            // Advance the time
            elapsedTime += Time.deltaTime;

            // Set the scale as part of the transition.
            transform.localScale = Vector3.Lerp(startScale, originalScale, elapsedTime / beatInterval);

            yield return null;
        }
    }
}
