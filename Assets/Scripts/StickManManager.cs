using System;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class StickManManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem blood;

    private Animator _stickManAnimator;

    private void Start()
    {
        _stickManAnimator = GetComponent<Animator>();
        _stickManAnimator.SetBool("run", true);
    }

    private void OnTriggerEnter(Collider other)
    {
        var playerManager = PlayerManager.PlayerManagerInstance;

        if (other.CompareTag("red") && other.transform.parent.childCount > 0)
        {
            Destroy(other.gameObject);
            Destroy(gameObject);

            Instantiate(blood, transform.position, Quaternion.identity);
        }

        if (other.CompareTag("stair"))
        {
            transform.parent.parent = null; // for instance tower_0
            transform.parent = null; // stickman
            GetComponent<Rigidbody>().isKinematic = GetComponent<Collider>().isTrigger = false;
            _stickManAnimator.SetBool("run", false);

            if (!playerManager.moveTheCamera)
                playerManager.moveTheCamera = true;

            if (playerManager.player.transform.childCount == 2)
            {
                other.GetComponent<Renderer>().material.DOColor(new Color(0.4f, 0.98f, 0.65f), 0.5f).
                SetLoops(1000, LoopType.Yoyo).SetEase(Ease.Flash);
            }
        }
    }
}