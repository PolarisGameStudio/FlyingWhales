﻿using ECS;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterUIData {

    public int level { get; private set; }
    public float maxHP { get; private set; }
    public string className { get; private set; }
    public float healthValue { get; private set; }
    public float manaValue { get; private set; }
    public float attackPower { get; private set; }
    public float speed { get; private set; }
    public List<Attribute> attributes { get; private set; }
    public List<CombatAttribute> combatAttributes { get; private set; }
    public Weapon equippedWeapon { get; private set; }
    public Armor equippedArmor { get; private set; }
    public Item equippedAccessory { get; private set; }
    public Item equippedConsumable { get; private set; }
    public List<Item> inventory { get; private set; }
    public List<Relationship> relationships { get; private set; }
    public Faction faction;

    public CharacterUIData() {
        attributes = new List<Attribute>();
        combatAttributes = new List<CombatAttribute>();
        inventory = new List<Item>();
        relationships = new List<Relationship>();
    }

    public void UpdateData(ICharacter character) {
        level = character.level;
        if (character.characterClass != null) {
            className = character.characterClass.className;
        }
        healthValue = (float)character.currentHP / (float)character.maxHP;
        manaValue = (float)character.currentSP / (float)character.maxSP;
        attackPower = character.attackPower;
        speed = character.speed;
        maxHP = character.maxHP;

        attributes.Clear();
        if (character.attributes != null) {
            attributes.AddRange(character.attributes);
        }

        combatAttributes.Clear();
        if (character.combatAttributes != null) {
            combatAttributes.AddRange(character.combatAttributes);
        }

        equippedWeapon = character.equippedWeapon;
        equippedArmor = character.equippedArmor;
        equippedAccessory = character.equippedAccessory;
        equippedConsumable = character.equippedConsumable;

        inventory.Clear();
        if (character.inventory != null) {
            inventory.AddRange(character.inventory);
        }

        relationships.Clear();
        if(character.relationships != null) {
            for (int i = 0; i < character.relationships.Count; i++) {
                relationships.Add(character.relationships.Values.ElementAt(i).CreateCopy()); //create copy instead of instance
            }
        }
    }
}
