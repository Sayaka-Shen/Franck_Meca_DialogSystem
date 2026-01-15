using System.Collections;
using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [Header("References")]
    [Tooltip("CharacterController")]
    [SerializeField] private GameObject target;
    [SerializeField] private GameObject visualTarget;
    private Transform cam;

    [Header("Camera Offset (local pivot target)")]
    [SerializeField] private float distance = 10f;
    [SerializeField] private float height = 4f;
    [SerializeField] private float lookAtHeight = 1.5f;

    [Header("Player Face Camera")]
    [SerializeField] private bool bKeepPlayerUpright = true;
    [SerializeField] private float playerTurnSpeed = 50f;

    [Header("Freeze while Cam Rotates")]
    [SerializeField] private bool bSetFreezeActive = true;
    [SerializeField] private float freezeRotationDelay = 1f;
    private float _prevTimeScale;
    private float _prevFixedDeltaTime;
    private bool IsTurning;

    [SerializeField] private KeyCode toggleKey = KeyCode.E;
    [SerializeField] private float turnDuration = 0.25f;

    [Tooltip("Angle A in degrees")]
    [SerializeField] private float angleA = 0f;
    [Tooltip("Angle B in degrees")]
    [SerializeField] private float angleB = 90f;

    [SerializeField] private bool bZoomDuringTurn = true;
    [Tooltip("Zoom strength")]
    [SerializeField] private float zoomInAmount = 3f;
    [SerializeField] private AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);

    private float runtimeDistance;

    public enum AxisIndex { X = 0, Y = 1, Z = 2 }
    [Header("Rotation Axis (choose one)")]
    [SerializeField, HideInInspector] private int rotationAxisIndex = 1; // 0=X,1=Y,2=Z

    public enum TurnMode { Snap = 0, Circle = 1 }
    [SerializeField, HideInInspector] private int turnModeIndex = 1; // 0=Snap, 1=Circle


    private float currentAngle;
    private bool atB;
    private Quaternion lockedCamRotation;

    void Awake()
    {
        if (!cam) cam = GetComponentInChildren<Camera>()?.transform;

        runtimeDistance = distance;
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

        cam.localPosition = new Vector3(0f, height, -runtimeDistance);

        cam.LookAt(target.transform.position + Vector3.up * lookAtHeight, Vector3.up);

        if (!IsTurning && Input.GetKeyDown(toggleKey))
        {
            float next = atB ? angleA : angleB;
            atB = !atB;

            if (turnModeIndex == (int)TurnMode.Snap)
            {
                //oh snap
                currentAngle = next;
                ApplyPose(currentAngle);
                FacePlayerTowardCamera();
            }
            else
            {
                //smoov off
                StartCoroutine(TurnTo(next));
            }
        }

    }

    void ApplyPose(float angle)
    {
        float x = (rotationAxisIndex == 0) ? angle : 0f;
        float y = (rotationAxisIndex == 1) ? angle : 0f;
        float z = (rotationAxisIndex == 2) ? angle : 0f;

        transform.rotation = Quaternion.Euler(x, y, z);
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

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, turnDuration);
                float nt = Mathf.Clamp01(t);

                currentAngle = Mathf.LerpAngle(startAngle, targetAngle, nt);
                ApplyPose(currentAngle);

                //zoom effect
                if (bZoomDuringTurn)
                {
                    float peak = Mathf.Sin(nt * Mathf.PI);
                    float shaped = zoomCurve.Evaluate(nt);
                    float zoomFactor = Mathf.Max(peak, shaped);

                    runtimeDistance = distance - zoomInAmount * zoomFactor;
                }

                FacePlayerTowardCamera();
                yield return null;
            }


            FacePlayerTowardCamera();

            yield return null;
        }

        currentAngle = targetAngle;
        ApplyPose(currentAngle);

        FacePlayerTowardCamera();

        yield return new WaitForSecondsRealtime(freezeRotationDelay);

        runtimeDistance = distance;
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
