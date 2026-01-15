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
    private float currentRollZ;

    [SerializeField] private bool bZoomDuringTurn = true;
    [Tooltip("Zoom strength")]
    [SerializeField] private float zoomInAmount = 3f;
    [SerializeField] private AnimationCurve turnSpeedCurve = AnimationCurve.EaseInOut(0f, 0.2f, 1f, 0.2f);
    [SerializeField] private int curveSamples = 80;
    private float runtimeDistance;

    public enum AxisIndex { X = 0, Y = 1, Z = 2 }
    [Header("Rotation Axis (choose one)")]
    [SerializeField, HideInInspector] private int rotationAxisIndex = 1; // 0=X, 1=Y, 2=Z

    public enum TurnMode
    {
        SnapToggle = 0,     // A to B instantly
        CircleToggle = 1,   // A to B smoothly
        PingPong = 2,       // A to B to A in one press
        Loop360 = 3,        // 360 no scope
        Continuous = 4      // increment by degree per input
    }

    [SerializeField, HideInInspector] private int turnModeIndex = 1; // 0=Snap, 1=Circle

    [Header("Continuous Mode")]
    [SerializeField] private float stepAngle = 90f;
    [SerializeField] private bool useOppositeKey = true;
    [SerializeField] private KeyCode oppositeKey = KeyCode.Q;


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

        Vector3 lookTarget = target.transform.position + Vector3.up * lookAtHeight;

        Quaternion lookRot = Quaternion.LookRotation(lookTarget - cam.position, Vector3.up);

        if (rotationAxisIndex == 2)
            lookRot *= Quaternion.Euler(0f, 0f, currentRollZ);

        cam.rotation = lookRot;

        if (IsTurning) return;

        if ((TurnMode)turnModeIndex == TurnMode.Continuous)
        {
            if (Input.GetKeyDown(toggleKey))
                StartCoroutine(TurnByDelta(+stepAngle));

            if (useOppositeKey && Input.GetKeyDown(oppositeKey))
                StartCoroutine(TurnByDelta(-stepAngle));

            return;
        }

        if (!Input.GetKeyDown(toggleKey)) return;

        switch ((TurnMode)turnModeIndex)
        {
            case TurnMode.SnapToggle:
                {
                    float next = atB ? angleA : angleB;
                    atB = !atB;
                    currentAngle = next;
                    ApplyPose(currentAngle);
                    FacePlayerTowardCamera();
                    break;
                }

            case TurnMode.CircleToggle:
                {
                    float next = atB ? angleA : angleB;
                    atB = !atB;
                    StartCoroutine(TurnTo(next));
                    break;
                }

            case TurnMode.PingPong:
                {
                    StartCoroutine(PingPongTurn());
                    break;
                }

            case TurnMode.Loop360:
                {
                    StartCoroutine(TurnByDelta(+360f));
                    break;
                }
        }
    }

    void ApplyPose(float angle)
    {
        if (rotationAxisIndex == 2)
        {
            currentRollZ = angle;
            transform.rotation = Quaternion.identity;
            return;
        }

        currentRollZ = 0f;

        float x = (rotationAxisIndex == 0) ? angle : 0f;
        float y = (rotationAxisIndex == 1) ? angle : 0f;

        transform.rotation = Quaternion.Euler(x, y, 0f);
    }


    #region --- COROUTINES ---

    // --- BASE TURN TO FUNCTION ---
    IEnumerator TurnTo(float targetAngle)
    {
        IsTurning = true;
        FreezeScene();

        yield return new WaitForSecondsRealtime(freezeRotationDelay);

        float startAngle = currentAngle;
        float t = 0f;

        float duration = Mathf.Max(0.0001f, turnDuration);
        float elapsed = 0f;

        float totalArea = TotalCurveArea01(turnSpeedCurve, curveSamples);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float nt = Mathf.Clamp01(elapsed / duration);

            float mapped = EvaluateIntegrated01(turnSpeedCurve, nt, curveSamples) / totalArea;

            currentAngle = Mathf.LerpAngle(startAngle, targetAngle, mapped);
            ApplyPose(currentAngle);

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

    // --- PING PONG ---
    private IEnumerator PingPongTurn()
    {
        float first = atB ? angleA : angleB;
        float second = atB ? angleB : angleA;

        yield return TurnTo(first);
        atB = !atB;

        yield return TurnTo(second);
        atB = !atB;
    }

    // --- TURN BY DELTA --- (360 full rota + continuous for steps)
    private IEnumerator TurnByDelta(float deltaAngle)
    {
        IsTurning = true;
        FreezeScene();

        yield return new WaitForSecondsRealtime(freezeRotationDelay);

        float startAngle = currentAngle;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, turnDuration);
            float nt = Mathf.Clamp01(t);

            currentAngle = startAngle + deltaAngle * nt;
            ApplyPose(currentAngle);

            FacePlayerTowardCamera();
            yield return null;
        }

        currentAngle = startAngle + deltaAngle;
        ApplyPose(currentAngle);
        FacePlayerTowardCamera();

        yield return new WaitForSecondsRealtime(freezeRotationDelay);

        UnfreezeScene();
        IsTurning = false;
    }


    #endregion

    // --- ROTATE PLAYER ---

    private void FacePlayerTowardCamera()
    {
        if (!visualTarget || !cam) return;

        Vector3 lookPos = cam.position;
        lookPos.y = visualTarget.transform.position.y;
        visualTarget.transform.LookAt(lookPos);
    }


    // --- FREEZE SCENE ---

    #region --- FREEZE SCENE ---
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
    #endregion


    // --- HELPERS ---

    #region --- HELPERS ---

    private float EvaluateIntegrated01(AnimationCurve curve, float t01, int samples)
    {
        t01 = Mathf.Clamp01(t01);
        samples = Mathf.Max(4, samples);

        float area = 0f;
        float prevT = 0f;
        float prevV = Mathf.Max(0f, curve.Evaluate(0f));

        for (int i = 1; i <= samples; i++)
        {
            float t = (t01 * i) / samples;
            float v = Mathf.Max(0f, curve.Evaluate(t));
            area += (prevV + v) * 0.5f * (t - prevT);
            prevT = t;
            prevV = v;
        }

        return area;
    }

    private float TotalCurveArea01(AnimationCurve curve, int samples)
    {
        return Mathf.Max(0.0001f, EvaluateIntegrated01(curve, 1f, samples));
    }

    #endregion
}
