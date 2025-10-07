using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using System;


public class SimpleDialogueManager : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] TextMeshProUGUI conversantName;


    [SerializeField] float textSpeed;

    [SerializeField] Dialogue defaultDialogue;

    [SerializeField] private float animationDuration = 0.5f; // Duration for the animation

    [SerializeField] private Vector2 initialPosition = new Vector2(-1000, -1000); // Initial off-screen position
    [SerializeField] private Vector2 finalPosition = new Vector2(0, 0);

    Dialogue currentDialogue;
    DialogueTrigger conversant;



    int index = 1;

    public bool InDialogue { get; private set; }

    bool dialogueSkippable;


    public static SimpleDialogueManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

    }

    private void Start()
    {

        transform.transform.DOScale(0, .01f);
        gameObject.SetActive(false);



    }
    void OnEnable()
    {
        Controls.Instance.OnPlayerAttack += PlayerInput_OnInteractAction;
    }
    void OnDisable()
    {
        Controls.Instance.OnPlayerAttack -= PlayerInput_OnInteractAction;
    }

    private void PlayerInput_OnInteractAction()
    {

        Debug.Log("skipping");
        if (currentDialogue != null && InDialogue)
        {
            if (dialogueText.text == currentDialogue.dialogueLines[index])
            {
                Debug.Log("going to next line");
                NextLine();
            }
            else
            {
                Debug.Log("skipping to full line");
                StopAllCoroutines();
                dialogueText.text = currentDialogue.dialogueLines[index];
            }
        }

    }





    public void StartDialogue(Dialogue dialogue, DialogueTrigger conversant)
    {

        
        InDialogue = true;
        Debug.Log("Starting conversation with " + conversant.GetName());
        //InputAction += PlayerInput_OnInteractAction;
        //SoundManager.Instance.PlayOpenDialogueSound();

        gameObject.SetActive(true);

        //add dialogue open animation with dotween
        AnimateTextBoxOpen();

        this.conversant = conversant;


        index = 0;
        dialogueText.text = string.Empty;
        if (dialogue != null)
        {
            currentDialogue = dialogue;
        }
        else
        {
            currentDialogue = defaultDialogue;
        }

        conversantName.text = conversant.GetName();
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in currentDialogue.dialogueLines[index].ToCharArray())
        {
            //AudioManager.Instance.PlayTypingSound();
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
            dialogueSkippable = true;
        }
    }

    void NextLine()
    {
        if (index < currentDialogue.dialogueLines.Count - 1)
        {
            index++;
            dialogueText.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            //PlayerInput.Instance.OnInteractAction -= PlayerInput_OnInteractAction;

            //SoundManager.Instance.PlayOpenDialogueSound();
            //add dialogue open animation with dotween
            AnimateTextBoxClose();


        }
    }
    void AnimateTextBoxOpen()
    {
        gameObject.transform.DOScale(1, .25f);

    }

    void AnimateTextBoxClose()
    {
        gameObject.transform.DOScale(0, .23f).OnComplete(() => {
            gameObject.SetActive(false);
            conversant.OnDialogueEnd();
            currentDialogue = null;
            InDialogue = false;
        });
    }

}
