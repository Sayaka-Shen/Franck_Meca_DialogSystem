using System.Collections;
using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [Header("References")]
    [Tooltip("CharacterController")]
    [SerializeField] private GameObject target;
    [SerializeField] private GameObject visualTarget;
    private Transform cam;

    [Header("Camera Offset (local to pivot)")]
    [SerializeField] private float distance = 10f;
    [SerializeField] private float height = 4f;
    [SerializeField] private float lookAtHeight = 1.5f;

    [Header("Toggle Camera Rotation")]
    [SerializeField] private KeyCode toggleKey = KeyCode.E;
    [SerializeField] private float turnDuration = 0.25f;

    [Tooltip("Angle A in degrees")]
    [SerializeField] private float angleA = 0f;
    [Tooltip("Angle B in degrees")]
    [SerializeField] private float angleB = 90f;

    [Header("Player Changes")]
    [SerializeField] private bool bKeepPlayerUpright = true;
    [SerializeField] private float playerTurnSpeed = 50f;

    [Header("Freeze while Cam Rotates")]
    [SerializeField] private bool bSetFreezeActive = true;
    [SerializeField] private float freezeRotationDelay = 1f;
    private float _prevTimeScale;
    private float _prevFixedDeltaTime;
    private bool IsTurning;

    private float currentAngle;
    private bool atB;
    private Quaternion lockedCamRotation;

    void Awake()
    {
        if (!cam) cam = GetComponentInChildren<Camera>()?.transform;
    }

    void Start()
    {
        currentAngle = angleA;
        atB = false;

        ApplyPose(currentAngle);
    }

    void LateUpdate()
    {
        if (!target || !cam) return;

        transform.position = target.transform.position;

        cam.localPosition = new Vector3(0f, height, -distance);

        cam.LookAt(target.transform.position + Vector3.up * lookAtHeight, Vector3.up);

        if (!IsTurning && Input.GetKeyDown(toggleKey))
        {
            float next = atB ? angleA : angleB;
            StartCoroutine(TurnTo(next));
            atB = !atB;
        }
    }

    void ApplyPose(float yAngle)
    {
        transform.rotation = Quaternion.Euler(0f, yAngle, 0f);
    }

    IEnumerator TurnTo(float targetAngle)
    {
        IsTurning = true;
        FreezeScene();

        yield return new WaitForSecondsRealtime(freezeRotationDelay);

        float startAngle = currentAngle;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, turnDuration);
            currentAngle = Mathf.LerpAngle(startAngle, targetAngle, t);
            ApplyPose(currentAngle);

            FacePlayerTowardCamera();

            yield return null;
        }

        currentAngle = targetAngle;
        ApplyPose(currentAngle);

        FacePlayerTowardCamera();

        yield return new WaitForSecondsRealtime(freezeRotationDelay);

        UnfreezeScene();
        IsTurning = false;
    }

    // --- ROTATE PLAYER ---

    private void FacePlayerTowardCamera()
    {
        if (!visualTarget || !cam) return;

        Vector3 lookPos = cam.position;
        lookPos.y = visualTarget.transform.position.y;
        visualTarget.transform.LookAt(lookPos);
    }


    // --- FREEZE SCENE ---
    private void FreezeScene()
    {
        if (bSetFreezeActive)
        {
            _prevTimeScale = Time.timeScale;
            _prevFixedDeltaTime = Time.fixedDeltaTime;

            Time.timeScale = 0f;

            Time.fixedDeltaTime = _prevFixedDeltaTime * Mathf.Max(Time.timeScale, 0.0001f);
        }
    }

    private void UnfreezeScene()
    {
        if (bSetFreezeActive)
        {
            Time.timeScale = _prevTimeScale;
            Time.fixedDeltaTime = _prevFixedDeltaTime;
        }
    }
}
