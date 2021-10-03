﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void BlockPrePlacedHandler();
public delegate void BlockFallenHandler();

public class Block : MonoBehaviour
{
    public event BlockPrePlacedHandler BlockPrePlaced;
    public event BlockFallenHandler BlockFallen;
    public bool Placed { get; private set; }

    [SerializeField] private PhysicMaterial frictionless = null;
    [SerializeField] private PhysicMaterial general = null;


    [SerializeField] private int startMass = 1;
    [SerializeField] private int placedMass = 20;

    [SerializeField] private BlockLevelType blockLevelType = BlockLevelType.none;

    private bool hasBeenPrePlaced = false;
    private Rigidbody rb;
    private Collider generalCollider;
    private Wind wind = null;


    private void Start()
    {
        Placed = false;
        wind = FindObjectOfType<Wind>();
        rb = GetComponent<Rigidbody>();
        generalCollider = GetComponent<Collider>();

        rb.mass = startMass;
        ToggleFriction(false);
        rb.constraints = RigidbodyConstraints.FreezePositionZ;
    }

    public BlockLevelType GetBlockLevelType()
    {
        return blockLevelType;
    }


    private void ToggleFriction(bool activate)
    {
        if(frictionless == null || general == null) { Debug.LogError("No physics friction materials reference found. Check PhysicMaterial fields."); }
        if(activate)
        {
            generalCollider.material = general;
        }
        else
        {
            generalCollider.material = frictionless;
        }
    }

    public void SetAsPlaced()
    {
        ToggleFriction(true);
        GetComponent<Rigidbody>().useGravity = true;
        rb.mass = placedMass;
        rb.velocity = Vector3.zero;
        Placed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("BoundingBox"))
        {
            Debug.Log("Block has fallen out of bounds. Lose game");
            BlockFallen?.Invoke();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Block collidingBlock = collision.gameObject.GetComponent<Block>();

        switch (blockLevelType)
        {
            case BlockLevelType.none:
                Debug.LogError("Block level type is marked as none. Check the inspector value.");
                break;
            case BlockLevelType.foundation:

                break;
            case BlockLevelType.floor:
                // Lose if floor block is placed on ground
                if (collision.gameObject.CompareTag("Ground"))
                {
                    Debug.Log("Floor block placed on ground surface. Lose game");
                    BlockFallen?.Invoke();
                }
                break;
            case BlockLevelType.roof:
                // Lose if roof block is placed on ground
                if (collision.gameObject.CompareTag("Ground"))
                {
                    Debug.Log("Floor block placed on ground surface. Lose game");
                    BlockFallen?.Invoke();
                }
                break;
            default:
                Debug.LogError("Unknown block level type: " + blockLevelType + ". Check the inspector value.");
                break;
        }
        if (!hasBeenPrePlaced)
        {
            hasBeenPrePlaced = true;

            // Open constraints
            rb.constraints = RigidbodyConstraints.None;

            BlockPrePlaced?.Invoke();
        }
    }

    private void FixedUpdate()
    {
        if(wind != null)
        {
            float strength = 0f;
            if(Placed)
            {
                strength = wind.GetWindPlacedStrength();
            }
            else
            {
                strength = wind.GetWindStrength();
            }

            // Apply wind
            if (wind.IsDirectedRight())
            {
                rb.AddForce(Vector3.right * strength);
            }
            else
            {
                rb.AddForce(-Vector3.right * strength);
            }
        }
    }
}
