﻿using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShareIntelMenu : PopupMenuBase {

    [Header("Main")]
    [SerializeField] private ScrollRect dialogScrollView;
    [SerializeField] private GameObject dialogItemPrefab;
    [SerializeField] private Button closeBtn;
    [SerializeField] private TextMeshProUGUI instructionLbl;
    [SerializeField] private TextMeshProUGUI endOfConversationLbl;

    [Header("Intel")]
    [SerializeField] private GameObject intelGO;
    [SerializeField] private IntelItem[] intelItems;

    private Character targetCharacter;
    private Character actor;

    private bool wasPausedOnOpen;
    public void Open(Character targetCharacter, Character actor, IIntel intelToShare) {
        base.Open();

        wasPausedOnOpen = GameManager.Instance.isPaused;
        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);

        Messenger.Broadcast(Signals.ON_OPEN_SHARE_INTEL);

        this.targetCharacter = targetCharacter;
        this.actor = actor;
        instructionLbl.text = $"Share Intel with {targetCharacter.name}";
        endOfConversationLbl.transform.SetParent(this.transform);
        endOfConversationLbl.gameObject.SetActive(false);

        UtilityScripts.Utilities.DestroyChildren(dialogScrollView.content);

        GameObject targetDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        DialogItem item = targetDialog.GetComponent<DialogItem>();
        item.SetData(targetCharacter, "What do you want from me?");

        GameObject actorDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        DialogItem actorItem = actorDialog.GetComponent<DialogItem>();
        actorItem.SetData(actor, UtilityScripts.Utilities.LogReplacer(intelToShare.log), DialogItem.Position.Right);

        DirectlyShowIntelReaction(intelToShare);
    }
    private void DirectlyShowIntelReaction(IIntel intel) {
        HideIntel();
        ReactToIntel(intel);
    }
    private void HideIntel() {
        intelGO.SetActive(false);
    }
    public override void Close() {
        //UIManager.Instance.SetCoverState(false);
        //UIManager.Instance.SetSpeedTogglesState(true);
        base.Close();
        UIManager.Instance.SetSpeedTogglesState(true);
        GameManager.Instance.SetPausedState(wasPausedOnOpen);
        Messenger.Broadcast(Signals.ON_CLOSE_SHARE_INTEL);
    }

    private void ReactToIntel(IIntel intel) {
        closeBtn.interactable = false;
        //HideIntel();
        //UpdateIntel(new List<Intel>() { intel });
        //intelItems[0].SetClickedState(true);
        //SetIntelButtonsInteractable(false);

        //GameObject actorDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        //DialogItem actorItem = actorDialog.GetComponent<DialogItem>();
        //actorItem.SetData(actor, Utilities.LogReplacer(intel.intelLog), DialogItem.Position.Right);

        //ShareIntel share = PlayerManager.Instance.player.shareIntelAbility;
        //share.BaseActivate(targetCharacter);
        //List<string> reactions = targetCharacter.ShareIntel(intel);
        //StartCoroutine(ShowReactions(reactions));
        string response = targetCharacter.ShareIntel(intel);
        if ((string.IsNullOrEmpty(response) || string.IsNullOrWhiteSpace(response)) && intel.actor != targetCharacter) {
            ActualGoapNode action = null;
            if(intel is ActionIntel actionIntel) {
                action = actionIntel.node;
            }
            response = CharacterManager.Instance.TriggerEmotion(EMOTION.Disinterest, targetCharacter, intel.actor, REACTION_STATUS.INFORMED, action);
        }
        StartCoroutine(ShowReaction(response, intel, targetCharacter));
    }
    private IEnumerator ShowReaction(string reaction, IIntel intel, Character reactor) {
        if (reaction == string.Empty) {
            //character had no reaction
            CreateDialogItem(reactor, intel.actor == targetCharacter ? "I know what I did." : "A proper response to this information has not been implemented yet.");
        } else {
            if (reaction == "aware") {
                CreateDialogItem(reactor, $"{UtilityScripts.Utilities.ColorizeAndBoldName(reactor.name)} already knows this.");
            } else {
                string[] emotionsToActorAndTarget = reaction.Split('/');

                string emotionsTowardsActor = emotionsToActorAndTarget.ElementAtOrDefault(0);
                string emotionsTowardsTarget = emotionsToActorAndTarget.ElementAtOrDefault(1);

                bool hasReactionToActor = string.IsNullOrEmpty(emotionsTowardsActor) == false;
                bool hasReactionToTarget = string.IsNullOrEmpty(emotionsTowardsTarget) == false;
                
                if (hasReactionToActor == false && hasReactionToTarget == false) {
                    //has no reactions to actor and target
                    CreateDialogItem(reactor, $"{reactor.visuals.GetCharacterStringIcon()}{UtilityScripts.Utilities.ColorizeAndBoldName(reactor.name)} seemed Disinterested about this.");
                } else {
                    if (hasReactionToActor) {
                        CreateDialogItem(reactor, $"{reactor.visuals.GetCharacterStringIcon()}{UtilityScripts.Utilities.ColorizeAndBoldName(reactor.name)} seemed {UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsTowardsActor, 2)} at {intel.actor.visuals.GetCharacterStringIcon()}{UtilityScripts.Utilities.ColorizeAndBoldName(intel.actor.name)} after receiving the new information.");    
                    }
                    if (hasReactionToTarget) {
                        if (intel.target is Character intelTarget) {
                            CreateDialogItem(reactor, $"{reactor.visuals.GetCharacterStringIcon()}{UtilityScripts.Utilities.ColorizeAndBoldName(reactor.name)} seemed {UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsTowardsTarget, 2)} at {intelTarget.visuals.GetCharacterStringIcon()}{UtilityScripts.Utilities.ColorizeAndBoldName(intel.target.name)} after receiving the new information.");  
                        } else {
                            CreateDialogItem(reactor, $"{reactor.visuals.GetCharacterStringIcon()}{UtilityScripts.Utilities.ColorizeAndBoldName(reactor.name)} seemed {UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsTowardsTarget, 2)} at {UtilityScripts.Utilities.ColorizeAndBoldName(intel.target.name)} after receiving the new information.");    
                        }
                        
                    }
                }
                
                
                // string finalReaction = string.Empty;
                // for (int i = 0; i < emotionsToActorAndTarget.Length; i++) {
                //     string[] words = emotionsToActorAndTarget[i].Split(' ');
                //     {
                //         string responses = string.Empty;
                //         for (int j = 0; j < words.Length; j++) {
                //             string currWord = words[j];
                //             if(string.IsNullOrEmpty(currWord) || string.IsNullOrWhiteSpace(currWord)){ continue; }
                //             if(responses != string.Empty) {
                //                 responses += ", ";
                //             }
                //             responses += currWord;
                //         }
                //         if (responses != string.Empty) {
                //             if (finalReaction != string.Empty && i > 0) {
                //                 finalReaction += "\n";
                //             }
                //             finalReaction +=
                //                 $"{reactor.name} felt {responses} towards {(i == 0 ? intel.actor.name : intel.target.name)}.";
                //         }
                //     }
                // }
                // reaction = finalReaction != string.Empty ? finalReaction : $"{reactor.name} felt disinterest towards this.";
            }
        }
        // GameObject targetDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        // DialogItem item = targetDialog.GetComponent<DialogItem>();
        // item.SetData(targetCharacter, reaction);
        endOfConversationLbl.transform.SetParent(dialogScrollView.content);
        endOfConversationLbl.gameObject.SetActive(true);
        closeBtn.interactable = true;
        dialogScrollView.verticalNormalizedPosition = 1f;
        yield return null;
        //ShareIntel share = PlayerManager.Instance.player.shareIntelAbility;
        //share.DeactivateAction();
    }

    private void CreateDialogItem(Character targetCharacter, string reaction) {
        GameObject targetDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        DialogItem item = targetDialog.GetComponent<DialogItem>();
        item.SetData(targetCharacter, reaction);
    }
}
