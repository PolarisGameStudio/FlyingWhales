﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectManager : MonoBehaviour {
    public static ObjectManager Instance;

    [SerializeField] private List<StructureObjectComponent> structureObjectComponents;
    [SerializeField] private List<CharacterObjectComponent> characterObjectComponents;
    [SerializeField] private List<ItemObjectComponent> itemObjectComponents;
    [SerializeField] private List<NPCObjectComponent> npcObjectComponents;
    [SerializeField] private List<LandmarkObjectComponent> landmarkObjectComponents;

    private List<StructureObj> _structureObjects;
    private List<CharacterObj> _characterObjects;
    private List<ItemObj> _itemObjects;
    private List<NPCObj> _npcObjects;
    private List<LandmarkObj> _landmarkObjects;
    private List<IObject> _allObjects;

    #region getters/setters
    public List<StructureObj> structureObjects {
        get { return _structureObjects; }
    }
    public List<CharacterObj> characterObjects {
        get { return _characterObjects; }
    }
    public List<ItemObj> itemObjects {
        get { return _itemObjects; }
    }
    public List<NPCObj> npcObjects {
        get { return _npcObjects; }
    }
    public List<LandmarkObj> landmarkObjects {
        get { return _landmarkObjects; }
    }
    public List<IObject> allObjects {
        get { return _allObjects; }
    }
    #endregion

    void Awake() {
        Instance = this;
    }

    public void Initialize() {
        _allObjects = new List<IObject>();
        _structureObjects = new List<StructureObj>();
        _characterObjects = new List<CharacterObj>();
        _itemObjects = new List<ItemObj>();
        _npcObjects = new List<NPCObj>();
        _landmarkObjects = new List<LandmarkObj>();
        for (int i = 0; i < structureObjectComponents.Count; i++) {
            StructureObjectComponent currComp = structureObjectComponents[i];
            StructureObj structureObject = ConvertComponentToStructureObject(currComp);
            SetInitialDataOfObjects(currComp, structureObject);
            _structureObjects.Add(structureObject);
            _allObjects.Add(structureObject);
        }
        for (int i = 0; i < characterObjectComponents.Count; i++) {
            CharacterObjectComponent currComp = characterObjectComponents[i];
            CharacterObj characterObject = ConvertComponentToCharacterObject(currComp);
            SetInitialDataOfObjects(currComp, characterObject, characterObjectComponents[i].gameObject.name);
            _characterObjects.Add(characterObject);
            _allObjects.Add(characterObject);
        }
        for (int i = 0; i < itemObjectComponents.Count; i++) {
            ItemObjectComponent currComp = itemObjectComponents[i];
            ItemObj itemObject = itemObjectComponents[i].itemObject;
            SetInitialDataOfObjects(currComp, itemObject, itemObjectComponents[i].gameObject.name);
            _itemObjects.Add(itemObject);
            _allObjects.Add(itemObject);
        }
        for (int i = 0; i < npcObjectComponents.Count; i++) {
            NPCObjectComponent currComp = npcObjectComponents[i];
            NPCObj npcObject = npcObjectComponents[i].npcObject;
            SetInitialDataOfObjects(currComp, npcObject, npcObjectComponents[i].gameObject.name);
            _npcObjects.Add(npcObject);
            _allObjects.Add(npcObject);
        }
        for (int i = 0; i < landmarkObjectComponents.Count; i++) {
            LandmarkObjectComponent currComp = landmarkObjectComponents[i];
            LandmarkObj landmarkObject = landmarkObjectComponents[i].landmarkObject;
            SetInitialDataOfObjects(currComp, landmarkObject, landmarkObjectComponents[i].gameObject.name);
            landmarkObject.SetObjectName(landmarkObjectComponents[i].gameObject.name);
            _landmarkObjects.Add(landmarkObject);
            _allObjects.Add(landmarkObject);
        }
    }
    private void SetInitialDataOfObjects(ObjectComponent objComp, IObject iobject, string objectName = "") {
        if(objectName != string.Empty) {
            iobject.SetObjectName(objectName);
        }
        for (int i = 0; i < objComp.states.Count; i++) {
            ObjectState state = objComp.states[i];
            state.SetObject(iobject);
            List<CharacterAction> newActions = new List<CharacterAction>();
            for (int j = 0; j < state.actions.Count; j++) {
                CharacterAction originalAction = state.actions[j];
                ConstructActionFilters(originalAction);
                originalAction.GenerateName();
                CharacterAction action = CreateNewCharacterAction(originalAction.actionType, state);
                originalAction.SetCommonData(action);
                action.Initialize();
                //action.SetFilters(originalAction.filters);
                ConstructPrerequisites(action);
                newActions.Add(action);
                //originalAction = action;
            }
            state.SetActions(newActions);
        }
        iobject.SetStates(objComp.states);
    }

    private void ConstructPrerequisites(CharacterAction action) {
        if (action.actionData.resourceAmountNeeded > 0) {
            CharacterActionData copy = action.actionData;
            RESOURCE resourceType = action.actionData.resourceNeeded;
            if(resourceType == RESOURCE.NONE) {
                if (action.state.obj.objectType == OBJECT_TYPE.STRUCTURE) {
                    resourceType = (action.state.obj as StructureObj).madeOf;
                }
            }
            ResourcePrerequisite resourcePrerequisite = new ResourcePrerequisite(resourceType, action.actionData.resourceAmountNeeded, action);
            copy.prerequisites = new List<IPrerequisite>() { resourcePrerequisite };
            action.SetActionData(copy);
        }
    }

    private void ConstructActionFilters(CharacterAction action) {
        ActionFilter[] createdFilters = new ActionFilter[action.actionData.filters.Length];
        for (int i = 0; i < action.actionData.filters.Length; i++) {
            ActionFilterData currData = action.actionData.filters[i];
            ActionFilter createdFilter = CreateActionFilterFromData(currData);
            createdFilters[i] = createdFilter;
        }
        action.SetFilters(createdFilters);
    }

    private ActionFilter CreateActionFilterFromData(ActionFilterData data) {
        switch (data.filterType) {
            case ACTION_FILTER_TYPE.ROLE:
                return CreateRoleFilter(data);
            case ACTION_FILTER_TYPE.LOCATION:
                return CreateLandmarkFilter(data);
            default:
                return null;
        }
    }

    private ActionFilter CreateRoleFilter(ActionFilterData data) {
        switch (data.condition) {
            case ACTION_FILTER_CONDITION.IS:
                return new MustBeRole(data.objects);
            case ACTION_FILTER_CONDITION.IS_NOT:
                return new MustNotBeRole(data.objects);
            default:
                return null;
        }
    }
    private ActionFilter CreateLandmarkFilter(ActionFilterData data) {
        switch (data.condition) {
            case ACTION_FILTER_CONDITION.IS:
                return new LandmarkMustBeState(data.objects[0]);
            default:
                return null;
        }
    }

    public IObject CreateNewObject(OBJECT_TYPE objType, string objectName) {
        if(objType == OBJECT_TYPE.STRUCTURE) {
            return GetNewStructureObject(objectName);
        }else if (objType == OBJECT_TYPE.CHARACTER) {
            return GetNewCharacterObject(objectName);
        }
        //IObject reference = GetReference(objType, objectName);
        //if (reference != null) {
        //    IObject newObj = reference.Clone();
        //    //location.AddObject(newObj);
        //    return newObj;
        //}
        return null;
    }
    //public IObject CreateNewObject(string objectName) {
    //    return CreateNewObject(GetObjectType(objectName), objectName);
    //}
    //public IObject CreateNewObject(LANDMARK_TYPE landmarkType) {
    //    string objectName = Utilities.NormalizeStringUpperCaseFirstLetters(landmarkType.ToString());
    //    return CreateNewObject(objectName);
    //}
    //private IObject GetReference(OBJECT_TYPE objType, string objectName) {
    //    for (int i = 0; i < _allObjects.Count; i++) {
    //        IObject currObject = _allObjects[i];
    //        if (currObject.objectType == objType && currObject.objectName.Equals(objectName)) {
    //            return currObject;
    //        }
    //    }
    //    return null;
    //}
    public OBJECT_TYPE GetObjectType(string objectName) {
        for (int i = 0; i < _allObjects.Count; i++) {
            IObject currObject = _allObjects[i];
            if (currObject.objectName.Equals(objectName)) {
                return currObject.objectType;
            }
        }
        throw new System.Exception("Object with the name " + objectName + " does not exist!");
    }

    public CharacterAction CreateNewCharacterAction(ACTION_TYPE actionType, ObjectState state) {
        switch (actionType) {
            //case ACTION_TYPE.BUILD:
            //return new BuildAction(state);
            case ACTION_TYPE.DESTROY:
                return new DestroyAction(state);
            case ACTION_TYPE.REST:
                return new RestAction(state);
            case ACTION_TYPE.HUNT:
                return new HuntAction(state);
            case ACTION_TYPE.EAT:
                return new EatAction(state);
            case ACTION_TYPE.DRINK:
                return new DrinkAction(state);
            case ACTION_TYPE.IDLE:
                return new IdleAction(state);
            case ACTION_TYPE.POPULATE:
                return new PopulateAction(state);
            case ACTION_TYPE.HARVEST:
                return new HarvestAction(state);
            case ACTION_TYPE.TORTURE:
                return new TortureAction(state);
            case ACTION_TYPE.PATROL:
                return new PatrolAction(state);
            case ACTION_TYPE.REPAIR:
                return new RepairAction(state);
            case ACTION_TYPE.ABDUCT:
                return new AbductAction(state);
            case ACTION_TYPE.PRAY:
                return new PrayAction(state);
            case ACTION_TYPE.ATTACK:
            return new AttackAction(state);
        }
        return null;
    }
    public StructureObj ConvertComponentToStructureObject(StructureObjectComponent component) {
        StructureObj structureObj = null;
        switch (component.specificObjectType) {
            case SPECIFIC_OBJECT_TYPE.DEMONIC_PORTAL:
                structureObj = new DemonicPortal();
                break;
            case SPECIFIC_OBJECT_TYPE.ELVEN_SETTLEMENT:
                structureObj = new ElvenSettlement();
                break;
            case SPECIFIC_OBJECT_TYPE.HUMAN_SETTLEMENT:
                structureObj = new HumanSettlement();
                break;
            case SPECIFIC_OBJECT_TYPE.GARRISON:
                structureObj = new Garrison();
                break;
            case SPECIFIC_OBJECT_TYPE.OAK_FORTIFICATION:
                structureObj = new OakFortification();
                break;
            case SPECIFIC_OBJECT_TYPE.IRON_FORTIFICATION:
                structureObj = new IronFortification();
                break;
            case SPECIFIC_OBJECT_TYPE.OAK_LUMBERYARD:
                structureObj = new OakLumberyard();
                break;
            case SPECIFIC_OBJECT_TYPE.IRON_MINES:
                structureObj = new IronMines();
                break;
            case SPECIFIC_OBJECT_TYPE.INN:
                structureObj = new Inn();
                break;
            case SPECIFIC_OBJECT_TYPE.PUB:
                structureObj = new Pub();
                break;
            case SPECIFIC_OBJECT_TYPE.TEMPLE:
                structureObj = new Temple();
                break;
            case SPECIFIC_OBJECT_TYPE.HUNTING_GROUNDS:
                structureObj = new HuntingGrounds();
                break;
        }
        component.CopyDataToStructureObject(structureObj);
        return structureObj;
    }
    public CharacterObj ConvertComponentToCharacterObject(CharacterObjectComponent component) {
        CharacterObj characterObj = new CharacterObj(null);
        component.CopyDataToCharacterObject(characterObj);
        return characterObj;
    }
    public StructureObj GetNewStructureObject(string name) {
        for (int i = 0; i < _structureObjects.Count; i++) {
            if(_structureObjects[i].objectName == name) {
                return _structureObjects[i].Clone() as StructureObj;
            }
        }
        return null;
    }
    public CharacterObj GetNewCharacterObject(string name) {
        for (int i = 0; i < _characterObjects.Count; i++) {
            if (_characterObjects[i].objectName == name) {
                return _characterObjects[i].Clone() as CharacterObj;
            }
        }
        return null;
    }
}
