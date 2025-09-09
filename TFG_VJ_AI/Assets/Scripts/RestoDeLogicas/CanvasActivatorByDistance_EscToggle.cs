using UnityEngine;

public class CanvasActivatorByDistance_EscToggle : MonoBehaviour
{
    [SerializeField] private Collider targetCollider;
    [SerializeField] private Transform reference;
    [SerializeField] private GameObject canvasObject;
    [SerializeField] private GameObject hintTextObject;
    [Min(0f)][SerializeField] private float enterDistance = 3f;
    [Min(0f)][SerializeField] private float exitDistance = 3.5f;
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;
    [SerializeField] private MonoBehaviour scriptMovement;

    private bool inRange;
    private bool isVisible;
    private bool prevScriptEnabled;

    private void Reset()
    {
        reference = transform;
    }

    private void OnValidate()
    {
        if (exitDistance < enterDistance) exitDistance = enterDistance;
    }

    private void Awake()
    {
        if (!reference) reference = transform;
        if (exitDistance < enterDistance) exitDistance = enterDistance;

        SetVisible(false);
        SetHintVisible(false);
    }

    private void Update()
    {
        if (!targetCollider || !reference) return;
        Vector3 closest = targetCollider.ClosestPoint(reference.position);
        float distance = Vector3.Distance(reference.position, closest);

        if (!inRange && distance <= enterDistance)
            inRange = true;
        else if (inRange && distance >= exitDistance)
            inRange = false;

        if (!inRange && isVisible)
            SetVisible(false);

        if (inRange && Input.GetKeyDown(toggleKey))
        {
            SetVisible(!isVisible);
        }
        

        UpdateHintVisibility();
    }

    private void SetVisible(bool value)
    {
        if (value && !isVisible)
        {
            if (scriptMovement && scriptMovement != this)
            {
                prevScriptEnabled = scriptMovement.enabled;
                scriptMovement.enabled = false;
            }
        }
        else if (!value && isVisible)
        {
            if (scriptMovement && scriptMovement != this)
                scriptMovement.enabled = prevScriptEnabled;
        }

        isVisible = value;
        if (canvasObject) canvasObject.SetActive(value);
    }

    private void UpdateHintVisibility()
    {
        SetHintVisible(inRange && !isVisible);
    }

    private void SetHintVisible(bool value)
    {
        if (hintTextObject && hintTextObject.activeSelf != value)
            hintTextObject.SetActive(value);
    }

    private void OnDisable()
    {
        if (isVisible && scriptMovement && scriptMovement != this)
            scriptMovement.enabled = prevScriptEnabled;

        if (canvasObject) canvasObject.SetActive(false);
        SetHintVisible(false);
        isVisible = false;
        inRange = false;
    }

    private void OnDrawGizmosSelected()
    {
        Transform r = reference ? reference : transform;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(r.position, enterDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(r.position, exitDistance);
    }
}