﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipController : MonoBehaviour {
    public float speedMultiplier;
    public float acceleration;
    public float maxBanking;

    private float smoothHorizontal;
    private float smoothVertical;
    private float banking;

    private Transform mesh;
    private Rigidbody rigidbody;
    private BoxCollider boxCollider;

    private float aliveTimer;

    public bool isDead;

    private float zFormation;

    public bool hasSlowmotion;

    private float slowMotionTime;

    public AudioClip powerupPickup;
    public AudioClip freezeTimeStart;
    public AudioClip freezeTimeStop;

    private bool isInSlowmotion = false;

	// Use this for initialization
	void Start () {
        mesh = transform.Find("MeshRepresentation");
        rigidbody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        aliveTimer = 6f;
    }

    void OnTriggerEnter(Collider other)
    {
        aliveTimer = 6f;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isDead)
        {
            Die();
        }
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Die()
    {
        rigidbody.useGravity = true;
        rigidbody.constraints = RigidbodyConstraints.None;

        var mc = transform.Find("Main Camera");
        if (mc != null) mc.parent = null;
        
        var map = transform.Find("Map");
        if (map != null) map.parent = null;

        transform.gameObject.AddComponent<MeshCollider>();
        transform.GetComponent<MeshCollider>().convex = true;
        isDead = true;
    }

    public void PlayPowerupPickup()
    {
        GetComponent<AudioSource>().clip = powerupPickup;
        GetComponent<AudioSource>().Play();
    }

    void PlayTimefreezeStart()
    {
        GetComponent<AudioSource>().clip = freezeTimeStart;
        GetComponent<AudioSource>().Play();
    }

    void PlayTimefreezeStop()
    {
        GetComponent<AudioSource>().clip = freezeTimeStop;
        GetComponent<AudioSource>().Play();
    }

	// Update is called once per frame
	void Update () {
        aliveTimer -= Time.deltaTime;

        if (aliveTimer < 0f) Die();

        if (!isDead)
        {
            smoothHorizontal = Mathf.Lerp(smoothHorizontal, Input.GetAxis("Horizontal"), 0.2f);
            smoothVertical = Mathf.Lerp(smoothVertical, Input.GetAxis("Vertical"), 0.2f);

            transform.position += (transform.forward * Time.deltaTime * acceleration)
                + (transform.right * smoothHorizontal * speedMultiplier)
                + (transform.up * smoothVertical * speedMultiplier);

            if (Input.GetButtonDown("Slowmotion") && hasSlowmotion)
            {
                hasSlowmotion = false;
                slowMotionTime = 3f;
            }

            if (slowMotionTime > 0f)
            {
                if (!isInSlowmotion)
                {
                    PlayTimefreezeStart();
                    isInSlowmotion = true;
                }

                Time.timeScale = 0.5f;
                slowMotionTime -= Time.deltaTime;
            }
            else
            {
                if (isInSlowmotion)
                {
                    PlayTimefreezeStop();
                    isInSlowmotion = false;
                }

                Time.timeScale = Mathf.Lerp(Time.timeScale, 1f, 0.01f);
            }

            if (Input.GetButton("Bank"))
            {
                banking = Mathf.Lerp(banking, maxBanking, 0.2f);
            }
            else
            {
                banking = Mathf.Lerp(banking, 0f, 0.2f);


                int targetValue = ((int)((transform.localEulerAngles.y + 45.0f) / 90f)) * 90;

                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, Mathf.Lerp(transform.localEulerAngles.y, targetValue, 0.2f), transform.localEulerAngles.z);
            }

            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + smoothHorizontal * banking, transform.eulerAngles.z);

            if (Input.GetButton("Formation"))
            {
                zFormation = Mathf.Lerp(zFormation, 90f, 0.2f);
                boxCollider.size = new Vector3(0.32f, boxCollider.size.y, boxCollider.size.z);
            }
            else
            {
                zFormation = Mathf.Lerp(zFormation, 0f, 0.2f);
                boxCollider.size = new Vector3(1f, boxCollider.size.x, boxCollider.size.z);
            }

            mesh.localEulerAngles = new Vector3(-smoothVertical * 25f, 0f, -smoothHorizontal * 25f + zFormation);
            
        }
    }
}
