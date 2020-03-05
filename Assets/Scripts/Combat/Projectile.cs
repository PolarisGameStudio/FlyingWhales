﻿using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EZObjectPools;
using Inner_Maps;
using UnityEngine;

public class Projectile : PooledObject {

    [SerializeField] private Transform targetTransform;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private float rotateSpeed = 200f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private ParticleSystem projectileParticles;
    [SerializeField] private ParticleSystem collisionParticles;
    [SerializeField] private ParticleCallback collisionParticleCallback;
    
    public IDamageable targetObject { get; private set; }
    public System.Action<IDamageable, CombatState> onHitAction;

    private Vector3 _pausedVelocity;
    private float _pausedAngularVelocity;
    private CombatState createdBy;
    private Tweener tween;

    #region Monobehaviours
    private void OnDestroy() {
        Messenger.RemoveListener<bool>(Signals.PAUSED, OnGamePaused);
        Messenger.RemoveListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnCharacterAreaTravelling);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        // Messenger.RemoveListener<SpecialToken, LocationGridTile>(Signals.ITEM_REMOVED_FROM_TILE, OnItemRemovedFromTile);
    }
    #endregion

    public void SetTarget(Transform target, IDamageable targetObject, CombatState createdBy) {
        // Vector3 diff = target.position - transform.position;
        // diff.Normalize();
        // float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        // transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
        name = $"Projectile from {createdBy.stateComponent.character.name} targeting {targetObject.name}";
        this.targetTransform = target;
        this.targetObject = targetObject;
        this.createdBy = createdBy;
        if (projectileParticles != null) {
            projectileParticles.Play();    
        }
        if (targetObject is Character) {
            Messenger.AddListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnCharacterAreaTravelling);
            Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        } else if (targetObject is TileObject) {
            Messenger.AddListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        }
        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
        collisionParticleCallback.SetAction(DestroyProjectile); //when the collision particles have successfully stopped. Destroy this object.
        
        tween = transform.DOMove(target.position, 25f).SetSpeedBased(true).SetEase(Ease.Linear).SetAutoKill(false);
        tween.OnUpdate (() => tween.ChangeEndValue (target.position, true));
    }

    // private void FixedUpdate() {
    //     if (targetTransform == null) {
    //         return;
    //     }
    //     if (GameManager.Instance != null && GameManager.Instance.isPaused) {
    //         return;
    //     }
    //     Vector2 direction = (Vector2)targetTransform.position - rigidBody.position;
    //     direction.Normalize();
    //     float rotateAmount = Vector3.Cross(direction, transform.up).z;
    //     rigidBody.angularVelocity = -rotateAmount * rotateSpeed;
    //     rigidBody.velocity = transform.up * speed;
    // }

    public void OnProjectileHit(IDamageable poi) {
        // rigidBody.velocity = Vector2.zero;
        // rigidBody.angularVelocity = 0f;
        tween.Kill();
        if (projectileParticles != null) { projectileParticles.Stop(); }
        onHitAction?.Invoke(poi, createdBy);
        targetTransform = null;
        _collider.enabled = false;
        collisionParticles.Play(true);
    }

    private void DestroyProjectile() {
        // GameObject.Destroy(this.gameObject);
        ObjectPoolManager.Instance.DestroyObject(this);
    }

    #region Object Pool
    public override void Reset() {
        base.Reset();
        Messenger.RemoveListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnCharacterAreaTravelling);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        Messenger.RemoveListener<bool>(Signals.PAUSED, OnGamePaused);
        _collider.enabled = true;
        // rigidBody.velocity = Vector2.zero;
        // rigidBody.angularVelocity = 0f;
        tween?.Kill();
        tween = null;
        if (projectileParticles != null) {
            projectileParticles.Stop();
            projectileParticles.Clear();
        }
        collisionParticles.Clear();
        onHitAction = null;
        targetTransform = null;
    }
    #endregion
    
    #region Listeners
    private void OnGamePaused(bool isPaused) {
        if (isPaused) {
            // _pausedVelocity = rigidBody.velocity;
            // _pausedAngularVelocity = rigidBody.angularVelocity;
            // rigidBody.velocity = Vector2.zero;
            // rigidBody.angularVelocity = 0f;
            // rigidBody.isKinematic = true;
            tween.Pause();
        } else {
            // rigidBody.isKinematic = false;
            // rigidBody.velocity = _pausedVelocity;
            // rigidBody.angularVelocity = _pausedAngularVelocity;
            tween.Play();
        }
    }
    private void OnCharacterAreaTravelling(Party party) {
        if (targetObject is Character) {
            if (party.owner == targetObject || party.carriedPOI == targetObject) { //party.characters.Contains(targetPOI as Character)
                DestroyProjectile();
            }
        }
    }
    private void OnCharacterDied(Character character) {
        if (character == targetObject) {
            DestroyProjectile();
        }
    }
    private void OnTileObjectRemoved(TileObject obj, Character removedBy, LocationGridTile removedFrom) {
        if (obj == targetObject) {
            DestroyProjectile();
        }
    }
    #endregion


}
