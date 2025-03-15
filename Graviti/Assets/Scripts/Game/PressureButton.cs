using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PressureButton : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer buttonSprite;
    [SerializeField] private Color inactiveColor = Color.red;
    [SerializeField] private Color activeColor = Color.green;

    [Header("Button Settings")]
    [SerializeField] private float snapOffset = 0.1f; 
    [SerializeField] private float snapCooldown = 1f; 
    

    [Header("Events")]
    public UnityEvent onButtonActivated;
    public UnityEvent onButtonDeactivated;

    private bool isActivated;
    private PushableBox currentBox;
    private float nextUnstickTime;
    private bool isPlayerTouchingBox;

    private void Start()
    {
        if (!buttonSprite) buttonSprite = GetComponent<SpriteRenderer>();
        UpdateVisuals();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Box") && currentBox == null)
        {
            PushableBox box = other.GetComponent<PushableBox>();
            if (box != null && !box.IsLocked())
            {
                PlayerController player = FindObjectOfType<PlayerController>();
                bool playerIsHoldingBox = player != null && player.IsStuckToBox() && player.currentBox == box;

                if (!box.requirePlayerToPush || playerIsHoldingBox)
                {
                    Vector3 snapPosition = transform.position;
                    snapPosition.y += snapOffset + (other.bounds.size.y / 2);
                    other.transform.position = snapPosition;

                    currentBox = box;
                    box.LockInPlace();

                    if (playerIsHoldingBox)
                    {
                        player.UnstickFromBox();
                    }

                    Activate();
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentBox != null && Time.time >= nextUnstickTime)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.IsStuckToBox())
            {
                currentBox.UnlockBox();
                Deactivate();
                currentBox = null;
            }
        }
    }

    public void Activate()
    {
        if (!isActivated)
        {
            isActivated = true;
            UpdateVisuals();
            nextUnstickTime = Time.time + snapCooldown;
            onButtonActivated?.Invoke();
        }
    }

    public void Deactivate()
    {
        if (isActivated)
        {
            isActivated = false;
            UpdateVisuals();
            onButtonDeactivated?.Invoke();
        }
    }

    private void UpdateVisuals()
    {
        if (buttonSprite)
        {
            buttonSprite.color = isActivated ? activeColor : inactiveColor;
        }
    }

    public bool IsActivated()
    {
        return isActivated;
    }
}
