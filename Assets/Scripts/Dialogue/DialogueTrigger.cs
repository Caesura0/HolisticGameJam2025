using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DialogueTrigger : MonoBehaviour, IInteractable
{

    [SerializeField] List<Dialogue> dialogueList;
    [SerializeField] List<Dialogue> questStartedDialogueList;
    [SerializeField] List<Dialogue> questCompleteDialogueList;



    [SerializeField] string conversantName;
    [SerializeField] bool finishQuestByTalking;


    List<Dialogue> validDialogueList;


    private void Start()
    {
        validDialogueList = new List<Dialogue>();

    }

    public string GetName()
    {
        return conversantName;
    }


    public void Interact(PlayerInteractionHandler interactor)
    {
        if(SimpleDialogueManager.Instance.InDialogue) { return; }  

        foreach (Dialogue dialogue in GetDialogueToSay())
        {
            if (!dialogue.hasSaidDialogue)
            {
                SimpleDialogueManager.Instance.StartDialogue(dialogue, this);
                return;
            }
            else if (dialogue.isRepeatableDialogue)
            {
                validDialogueList.Add(dialogue);
            }
        }


        if (validDialogueList.Count > 0)
        {
            int choices;
            choices = validDialogueList.Count - 1;
            int i = Random.Range(0, choices);
            SimpleDialogueManager.Instance.StartDialogue(validDialogueList[i], this) ;
        }
        else
        {
            SimpleDialogueManager.Instance.StartDialogue(null, this);
        }


    }

    public List<Dialogue> GetDialogueToSay()
    {



            return dialogueList;

    }
    public void OnDialogueEnd()
    {
        Debug.Log("Dialogue over");


    }

}
