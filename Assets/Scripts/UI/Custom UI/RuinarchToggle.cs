﻿using Coffee.UIExtensions;
using Quests.Steps;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Ruinarch.Custom_UI {
    public class RuinarchToggle : UnityEngine.UI.Toggle{
        
        private UIShiny shineEffect;
        
        #region Monobehaviours
        protected override void Awake() {
            base.Awake();
            shineEffect = GetComponent<UIShiny>();
            if (shineEffect == null) {
                shineEffect = targetGraphic.gameObject.GetComponent<UIShiny>();
            }
            if (shineEffect != null) {
                shineEffect.Stop();
            }
        }
        protected override void OnEnable() {
            base.OnEnable();
            if (Application.isPlaying && shineEffect != null) {
                if (shineEffect != null) {
                    Messenger.AddListener<string>(Signals.SHOW_SELECTABLE_GLOW, OnReceiveShowGlowSignal);
                    Messenger.AddListener<string>(Signals.HIDE_SELECTABLE_GLOW, OnReceiveHideGlowSignal);
                    if (InputManager.Instance.ShouldBeHighlighted(this)) {
                        StartGlow();
                    }
                }
                Messenger.AddListener<QuestStep>(Signals.QUEST_STEP_ACTIVATED, OnQuestStepActivated);
                FireToggleShownSignal();
            }
        }
        
        protected override void OnDisable() {
            base.OnDisable();
            if (Application.isPlaying) {
                if (shineEffect != null) {
                    Messenger.RemoveListener<string>(Signals.SHOW_SELECTABLE_GLOW, OnReceiveShowGlowSignal);
                    Messenger.RemoveListener<string>(Signals.HIDE_SELECTABLE_GLOW, OnReceiveHideGlowSignal);
                    HideGlow();    
                }
                Messenger.RemoveListener<QuestStep>(Signals.QUEST_STEP_ACTIVATED, OnQuestStepActivated);
            }
        }
        #endregion
        
        public override void OnPointerClick(PointerEventData eventData) {
            base.OnPointerClick(eventData);
            Messenger.Broadcast(Signals.TOGGLE_CLICKED, this);    
        }
        
        #region Shine
        private void StartGlow() {
            if (shineEffect != null) {
                shineEffect.Play();
            }
        }
        private void HideGlow() {
            if (shineEffect != null) {
                shineEffect.Stop();
            }
        }
        private void OnReceiveShowGlowSignal(string buttonName) {
            if (name == buttonName) {
                StartGlow();
            }
        }
        private void OnReceiveHideGlowSignal(string buttonName) {
            if (name == buttonName) {
                HideGlow();
            }
        }
        #endregion

        #region Signals
        public void FireToggleShownSignal() {
            Messenger.Broadcast(Signals.TOGGLE_SHOWN, this);
        }
        private void OnQuestStepActivated(QuestStep questStep) {
            if (questStep is ToggleTurnedOnStep turnedOnStep) {
                if (turnedOnStep.neededToggleName.Equals(name)) {
                    FireToggleShownSignal();
                }
            }
        }
        #endregion
    }
}