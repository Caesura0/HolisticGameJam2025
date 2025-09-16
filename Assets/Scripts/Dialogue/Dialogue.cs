using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "dialogue", menuName = "Dialogues")]
public class Dialogue : ScriptableObject
{
    //public string conversantName;
    [TextArea(3,5)]
    public List<string> dialogueLines;
    //public List<ItemConfig> inventoryItemList;
    bool hasGivenItems;
    public bool hasSaidDialogue; //need to make this part of the conversant
    public bool isRepeatableDialogue;


    private void Awake()
    {

        hasGivenItems = false;
        hasSaidDialogue = false;
    }


}
