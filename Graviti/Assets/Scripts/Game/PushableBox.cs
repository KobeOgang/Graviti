using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableBox : MonoBehaviour
{
    [Header("Box Settings")]
    [SerializeField] private float pushForce = 2f;
    [SerializeField] private LayerMask buttonLayer;
    [SerializeField] private float unstickForce = 3f;
    [SerializeField] public bool requirePlayerToPush = false;

    private Rigidbody2D rb;
    private bool isLocked;
    private PlayerController player;
    private bool isBeingPushed;
    private Vector2 lastGravityDirection;
    private FixedJoint2D joint; 

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lastGravityDirection = Physics2D.gravity.normalized;
    }

    private void FixedUpdate()
    {
        if (!isLocked)
        {
            Vector2 currentGravity = Physics2D.gravity.normalized;
            if (currentGravity != lastGravityDirection)
            {
                float angle = Mathf.Atan2(currentGravity.y, currentGravity.x) * Mathf.Rad2Deg;
                angle += 90;
                transform.rotation = Quaternion.Euler(0, 0, angle);
                lastGravityDirection = currentGravity;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            if (playerController != null && !isLocked)
            {
                playerController.StickToBox(this);
            }
        }
        else if (((1 << collision.gameObject.layer) & buttonLayer) != 0)
        {
            PressureButton button = collision.gameObject.GetComponent<PressureButton>();
            if (button != null)
            {
                LockInPlace();
                button.Activate();
            }
        }
    }

    public void OnPlayerStick()
    {
        if (isLocked) return;

        isBeingPushed = true;
        player = FindObjectOfType<PlayerController>();

        if (player != null)
        {
            joint = gameObject.AddComponent<FixedJoint2D>();
            joint.connectedBody = player.GetComponent<Rigidbody2D>();
            joint.enableCollision = false;
        }
    }

    public void OnPlayerUnstick()
    {
        isBeingPushed = false;
        player = null;

        if (joint != null)
        {
            Destroy(joint);
            joint = null;
        }
    }

    public void LockInPlace()
    {
        isLocked = true;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        if (player != null)
        {
            player.StickToBox(null);
        }
    }

    public void UnlockBox()
    {
        isLocked = false;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public bool IsLocked()
    {
        return isLocked;
    }
}
