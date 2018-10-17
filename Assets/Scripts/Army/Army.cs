﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Army : Party {

    public ArmyActionData actionData { get; private set; }

    public Army(ICharacter owner) : base(owner) {
        actionData = new ArmyActionData(this);
    }

    #region overrides
    /*
    Create a new icon for this character.
    Each character owns 1 icon.
        */
    public override void CreateIcon() {
        base.CreateIcon();
        GameObject characterIconGO = GameObject.Instantiate(CharacterManager.Instance.characterIconPrefab,
            Vector3.zero, Quaternion.identity, CharacterManager.Instance.characterIconsParent);

        _icon = characterIconGO.GetComponent<CharacterAvatar>();
        _icon.Init(this);
    }
    #endregion
}
