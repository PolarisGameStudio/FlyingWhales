﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using EZObjectPools;
using ECS;

public class CharacterPortrait : PooledObject, IPointerClickHandler {

    private ICharacter _character;
    private int _imgSize;
    private bool _ignoreSize;
    private PortraitSettings _portraitSettings;
    private Vector2 normalSize;

    public bool forceShowPortrait = false;
    public bool ignoreInteractions = false;

    [Header("BG")]
    [SerializeField] private Image bg;

    [Header("Head")]
    [SerializeField] private Image head;

    [Header("Eyes")]
    [SerializeField] private Image eyes;
    [SerializeField] private Image eyebrows;
    
    [Header("Nose")]
    [SerializeField] private Image nose;

    [Header("Mouth")]
    [SerializeField] private Image mouth;

    [Header("Hair")]
    [SerializeField] private Image hair;
    [SerializeField] private Image hairBack;
    [SerializeField] private Image hairOverlay;
    [SerializeField] private Image hairBackOverlay;
    [SerializeField] private Image facialHair;
    [SerializeField] private Image facialHairOverlay;

    [Header("Body")]
    [SerializeField] private Image body;

    [Header("Monster")]
    [SerializeField] private Image wholeImage;

    [Header("Name")]
    [SerializeField] private TextMeshProUGUI nameLbl;

    [Header("Other")]
    [SerializeField] private GameObject unknownGO;

    #region getters/setters
    public ECS.Character thisCharacter {
        get { return _character as ECS.Character; }
    }
    public PortraitSettings portraitSettings {
        get { return _portraitSettings; }
    }
    public bool isLocked {
        get { return unknownGO.activeSelf; }
    }
    #endregion

    public void Initialize() {
        Messenger.AddListener<CharacterIntel>(Signals.CHARACTER_INTEL_ADDED, OnCharacterIntelObtained);
    }

    public void GeneratePortrait(ICharacter character, int imgSize = 97, CHARACTER_ROLE role = CHARACTER_ROLE.NONE) {
        _character = character;
        SetImageSize(imgSize);
        if(character == null) {
            SetBodyPartsState(false);
            SetWholeImageSprite(null);
            wholeImage.gameObject.SetActive(true);
            return;
        }
        _portraitSettings = character.portraitSettings;
        if (character is ECS.Character) {
            SetBody(character.portraitSettings.bodyIndex);
            SetHead(character.portraitSettings.headIndex);
            SetEyes(character.portraitSettings.eyesIndex);
            SetEyebrows(character.portraitSettings.eyesIndex);
            SetNose(character.portraitSettings.noseIndex);
            SetMouth(character.portraitSettings.mouthIndex);
            SetHair(character.portraitSettings.hairIndex);
            SetFacialHair(character.portraitSettings.facialHairIndex);
            SetHairColor(character.portraitSettings.hairColor);
            SetWholeImageSprite(null);
        } else if (character is Monster) {
            SetBodyPartsState(false);
            SetWholeImageSprite(MonsterManager.Instance.GetMonsterSprite(character.name));
        }
        SetBGState(true);
        nameLbl.text = character.urlName;
        UpdateUnknownVisual();
    }
    public void GeneratePortrait(PortraitSettings portraitSettings, int imgSize = 97) {
        _portraitSettings = portraitSettings;
        SetImageSize(imgSize);
        if (portraitSettings == null) {
            SetBodyPartsState(false);
            //wholeImage.sprite = null;
            //wholeImage.gameObject.SetActive(true);
            SetWholeImageSprite(null);
            return;
        }
        SetBody(portraitSettings.bodyIndex);
        SetHead(portraitSettings.headIndex);
        SetEyes(portraitSettings.eyesIndex);
        SetEyebrows(portraitSettings.eyesIndex);
        SetNose(portraitSettings.noseIndex);
        SetMouth(portraitSettings.mouthIndex);
        SetHair(portraitSettings.hairIndex);
        SetFacialHair(portraitSettings.facialHairIndex);
        SetHairColor(portraitSettings.hairColor);
        SetWholeImageSprite(null);
        //wholeImage.gameObject.SetActive(false);
    }

    #region Utilities
    private void SetBodyPartsState(bool state) {
        body.gameObject.SetActive(state);
        head.gameObject.SetActive(state);
        eyes.gameObject.SetActive(state);
        eyebrows.gameObject.SetActive(state);
        nose.gameObject.SetActive(state);
        mouth.gameObject.SetActive(state);
        if (hair.sprite == null) {
            hair.gameObject.SetActive(false);
        } else {
            hair.gameObject.SetActive(state);
        }
        if (hairBack.sprite == null) {
            hairBack.gameObject.SetActive(false);
        } else {
            hairBack.gameObject.SetActive(state);
        }

        if (facialHair.sprite == null) {
            facialHair.gameObject.SetActive(false);
        } else {
            facialHair.gameObject.SetActive(state);
        }

        if (hairOverlay.sprite == null) {
            hairOverlay.gameObject.SetActive(false);
        } else {
            hairOverlay.gameObject.SetActive(state);
        }

        if (hairBackOverlay.sprite == null) {
            hairBackOverlay.gameObject.SetActive(false);
        } else {
            hairBackOverlay.gameObject.SetActive(state);
        }

        if (facialHairOverlay.sprite == null) {
            facialHairOverlay.gameObject.SetActive(false);
        } else {
            facialHairOverlay.gameObject.SetActive(state);
        }
    }
    private void SetWholeImageSprite(Sprite sprite) {
        wholeImage.sprite = sprite;
        SetWholeImageState(sprite != null);
    }
    private void SetWholeImageState(bool state) {
        wholeImage.gameObject.SetActive(state);
    }
    public Color GetHairColor() {
        return hair.color;
    }
    public void ToggleNameLabel(bool state) {
        if (nameLbl.gameObject.activeSelf != state) {
            nameLbl.gameObject.SetActive(state);
        }
    }
    public void SetImageSize(int imgSize) {
        //if (ignoreSize) {
        //    return;
        //}
        _imgSize = imgSize;
        SetDimensions(imgSize);
        normalSize = (this.transform as RectTransform).sizeDelta;
        //float size = 0f;
        //if (!ignoreSize) {
        //RectTransform[] rt = Utilities.GetComponentsInDirectChildren<RectTransform>(this.gameObject);
        //switch (imgSize) {
        //    case IMAGE_SIZE.X64:
        //        size = 64f;
        //        break;
        //    case IMAGE_SIZE.X256:
        //        size = 256f;
        //        break;
        //    case IMAGE_SIZE.X72:
        //        size = 72f;
        //        break;
        //    case IMAGE_SIZE.X36:
        //        size = 36f;
        //        break;
        //    default:
        //        break;
        //}
        //SetDimensions(size);
        //for (int i = 0; i < rt.Length; i++) {
        //    rt[i].sizeDelta = new Vector2(size, size);
        //}
        //}

    }
    public void SetDimensions(float size) {
        (this.transform as RectTransform).sizeDelta = new Vector2(size, size);
        //head.rectTransform.sizeDelta = new Vector2(size, size);
        //eyes.rectTransform.sizeDelta = new Vector2(size, size);
        //eyebrows.rectTransform.sizeDelta = new Vector2(size, size);
        //nose.rectTransform.sizeDelta = new Vector2(size, size);
        //mouth.rectTransform.sizeDelta = new Vector2(size, size);
        //hair.rectTransform.sizeDelta = new Vector2(size, size);
        //hairBack.rectTransform.sizeDelta = new Vector2(size, size);
        //hairOverlay.rectTransform.sizeDelta = new Vector2(size, size);
        //hairBackOverlay.rectTransform.sizeDelta = new Vector2(size, size);
        //facialHair.rectTransform.sizeDelta = new Vector2(size, size);
        //facialHairOverlay.rectTransform.sizeDelta = new Vector2(size, size);
        //body.rectTransform.sizeDelta = new Vector2(size, size);
        //wholeImage.rectTransform.sizeDelta = new Vector2(size, size);
    }
    public void SetBGState(bool state) {
        bg.enabled = state;
    }
    private void UpdateUnknownVisual() {
        if (_character != null) {
            CharacterIntel characterIntel = _character.characterIntel;
            if (_character is Character) {
                if (forceShowPortrait) {
                    unknownGO.SetActive(false);
                    SetBodyPartsState(true);
                } else {
                    unknownGO.SetActive(!characterIntel.isObtained);
                    SetBodyPartsState(characterIntel.isObtained);
                }
            } else if (_character is Monster) {
                if (forceShowPortrait) {
                    unknownGO.SetActive(false);
                    SetBodyPartsState(true);
                } else {
                    unknownGO.SetActive(!characterIntel.isObtained);
                    SetWholeImageState(characterIntel.isObtained);
                }
            }
        }
    }
    public void SetForceShowPortraitState(bool state) {
        forceShowPortrait = state;
        UpdateUnknownVisual();
    }
    #endregion

    #region Pointer Actions
    //    public void OnPointerEnter(PointerEventData eventData) {
    //#if !WORLD_CREATION_TOOL
    //        if (ignoreInteractions) {
    //            return;
    //        }
    //        Vector2 currentSize = (this.transform as RectTransform).sizeDelta;
    //        Vector2 newSize = new Vector2(currentSize.x + (currentSize.x * 0.5f), currentSize.y + (currentSize.y * 0.5f));
    //        (this.transform as RectTransform).sizeDelta = newSize;
    //        RectTransform[] rt = Utilities.GetComponentsInDirectChildren<RectTransform>(this.gameObject);
    //        for (int i = 0; i < rt.Length; i++) {
    //            if (rt[i] == borders.transform) {
    //                continue;
    //            }
    //            rt[i].sizeDelta = newSize;
    //        }
    //        //_isNormalSize = false;
    //#endif
    //    }
    //    public void OnPointerExit(PointerEventData eventData) {
    //#if !WORLD_CREATION_TOOL
    //        if (ignoreInteractions) {
    //            return;
    //        }
    //        NormalizeSize();
    //        //LandmarkVisual lv = this.gameObject.GetComponentInParent<LandmarkVisual>();
    //        //if (lv != null) {
    //        //    lv.SnapTo(this.transform as RectTransform);
    //        //}
    //        //this.transform.localScale = Vector3.one;
    //#endif
    //    }
    public void OnPointerClick(PointerEventData eventData) {
#if !WORLD_CREATION_TOOL
        if (ignoreInteractions || isLocked) {
            return;
        }
        if (eventData.button == PointerEventData.InputButton.Right) {
            //Debug.Log("Right clicked character portrait!");
            if (_character != null) {
                if (_character is ECS.Character) {
                    UIManager.Instance.ShowCharacterInfo(_character as ECS.Character);
                } else if (_character is Monster) {
                    UIManager.Instance.ShowMonsterInfo(_character as Monster);
                }
            }
        }
        
#endif
    }
    public void OnClick(BaseEventData eventData) {
        if (ignoreInteractions) {
            return;
        }
        OnPointerClick(eventData as PointerEventData);
    }
    #endregion

    #region Body Parts
    public void SetHair(int index) {
        HairSetting chosenHairSettings = CharacterManager.Instance.GetHairSprite(index, _portraitSettings.race, _portraitSettings.gender);
        //Sprite hairSprite = CharacterManager.Instance.GetHairSprite(index, _imgSize, _character.);
        hair.sprite = chosenHairSettings.hairSprite;
        hairOverlay.sprite = chosenHairSettings.hairSprite;
        hairBack.sprite = chosenHairSettings.hairBackSprite;
        hairBackOverlay.sprite = chosenHairSettings.hairBackSprite;
        if (chosenHairSettings.hairBackSprite == null) {
            hairBack.gameObject.SetActive(false);
            hairBackOverlay.gameObject.SetActive(false);
        } else {
            hairBack.gameObject.SetActive(true);
            hairBackOverlay.gameObject.SetActive(true);
        }

        //if (!_ignoreSize) {
        //    hair.SetNativeSize();
        //    hairBack.SetNativeSize();
        //    hairOverlay.SetNativeSize();
        //    hairBackOverlay.SetNativeSize();
        //}
        hair.gameObject.SetActive(true);
        hairOverlay.gameObject.SetActive(true);
        //if (chosenHairSettings.hairBackSprite == null) {
        //    hairBack.sprite = chosenHairSettings.hairSprite;
        //} else {
        //    hairBack.sprite = chosenHairSettings.hairBackSprite;
        //}
    }
    public void SetHead(int index) {
        Sprite headSprite = CharacterManager.Instance.GetHeadSprite(index, _portraitSettings.race, _portraitSettings.gender);
        head.sprite = headSprite;
        //if (!_ignoreSize) {
        //    head.SetNativeSize();
        //}
        head.gameObject.SetActive(true);
    }
    public void SetEyes(int index) {
        Sprite eyeSprite = CharacterManager.Instance.GetEyeSprite(index, _portraitSettings.race, _portraitSettings.gender);
        eyes.sprite = eyeSprite;
        //if (!_ignoreSize) {
        //    eyes.SetNativeSize();
        //}
        eyes.gameObject.SetActive(true);
    }
    public void SetEyebrows(int index) {
        Sprite eyeBrowSprite = CharacterManager.Instance.GetEyebrowSprite(index, _portraitSettings.race, _portraitSettings.gender);
        eyebrows.sprite = eyeBrowSprite;
        //if (!_ignoreSize) {
        //    eyebrows.SetNativeSize();
        //}
        eyebrows.gameObject.SetActive(true);
    }
    public void SetNose(int index) {
        nose.sprite = CharacterManager.Instance.GetNoseSprite(index, _portraitSettings.race, _portraitSettings.gender);
        //if (!_ignoreSize) {
        //    nose.SetNativeSize();
        //}
        nose.gameObject.SetActive(true);
    }
    public void SetMouth(int index) {
        mouth.sprite = CharacterManager.Instance.GetMouthSprite(index, _portraitSettings.race, _portraitSettings.gender);
        //if (!_ignoreSize) {
        //    mouth.SetNativeSize();
        //}
        mouth.gameObject.SetActive(true);
    }
    public void SetFacialHair(int index) {
        facialHair.sprite = CharacterManager.Instance.GetFacialHairSprite(index, _portraitSettings.race, _portraitSettings.gender);
        facialHairOverlay.sprite = facialHair.sprite;
        if (facialHair.sprite == null) {
            facialHair.gameObject.SetActive(false);
            facialHairOverlay.gameObject.SetActive(false);
        } else {
            facialHair.gameObject.SetActive(true);
            facialHairOverlay.gameObject.SetActive(true);
        }
        //if (!_ignoreSize) {
        //    body.SetNativeSize();
        //}
    }
    public void SetBody(int index) {
        body.sprite = CharacterManager.Instance.GetBodySprite(index, _portraitSettings.race, _portraitSettings.gender);
        //if (!_ignoreSize) {
        //    body.SetNativeSize();
        //}
        body.gameObject.SetActive(true);
    }
    public void SetHairColor(Color hairColor) {
        //hair.color = hairColor;
        //hairBack.color = hairColor;
        Color newColor = new Color(hairColor.r, hairColor.g, hairColor.b, 115f/255f);
        hairOverlay.color = newColor;
        hairBackOverlay.color = newColor;
        facialHairOverlay.color = newColor;
    }
    #endregion

    #region Listeners
    private void OnCharacterIntelObtained(CharacterIntel intel) {
        if (_character != null && _character.characterIntel == intel) {
            UpdateUnknownVisual();
        }
    }
    private void RemoveListeners() {
        if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_INTEL_ADDED)) {
            Messenger.RemoveListener<CharacterIntel>(Signals.CHARACTER_INTEL_ADDED, OnCharacterIntelObtained);
        }
    }
    #endregion

    #region Pooled Object
    public override void Reset() {
        base.Reset();
        _character = null;
        ignoreInteractions = false;
        RemoveListeners();
    }
    #endregion
}
