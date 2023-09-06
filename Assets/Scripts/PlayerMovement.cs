using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// The rate acceleration during movement.
    /// </summary>
    public float Acceleration = 0.1f;

    /// <summary>
    /// The rate of damping on movement.
    /// </summary>
    public float Damping = 0.3f;

    /// <summary>
    /// The rate of additional damping when moving sideways or backwards.
    /// </summary>
    public float BackAndSideDampen = 0.5f;

    /// <summary>
    /// The rate of rotation when using the keyboard.
    /// </summary>
    public float RotationRatchet = 45.0f;

    /// <summary>
    /// The player will rotate in fixed steps if Snap Rotation is enabled.
    /// </summary>
    [Tooltip("The player will rotate in fixed steps if Snap Rotation is enabled.")]
    public bool SnapRotation = true;

    /// <summary>
    /// How many fixed speeds to use with linear movement? 0=linear control
    /// </summary>
    [Tooltip("How many fixed speeds to use with linear movement? 0=linear control")]
    public int FixedSpeedSteps;

    /// <summary>
    /// If true, reset the initial yaw of the player controller when the Hmd pose is recentered.
    /// </summary>
    public bool HmdResetsY = true;

    /// <summary>
    /// If true, tracking data from a child OVRCameraRig will update the direction of movement.
    /// </summary>
    public bool HmdRotatesY = true;

    /// <summary>
    /// Modifies the strength of gravity.
    /// </summary>
    public float GravityModifier = 0.379f;

    /// <summary>
    /// If true, each OVRPlayerController will use the player's physical height.
    /// </summary>
    public bool useProfileData = true;

    /// <summary>
    /// The CameraHeight is the actual height of the HMD and can be used to adjust the height of the character controller, which will affect the
    /// ability of the character to move into areas with a low ceiling.
    /// </summary>
    [NonSerialized]
    public float CameraHeight;

    /// <summary>
    /// This event is raised after the character controller is moved. This is used by the OVRAvatarLocomotion script to keep the avatar transform synchronized
    /// with the OVRPlayerController.
    /// </summary>
    public event Action<Transform> TransformUpdated;

    /// <summary>
    /// This bool is set to true whenever the player controller has been teleported. It is reset after every frame. Some systems, such as
    /// CharacterCameraConstraint, test this boolean in order to disable logic that moves the character controller immediately
    /// following the teleport.
    /// </summary>
    [NonSerialized] // This doesn't need to be visible in the inspector.
    public bool Teleported;

    /// <summary>
    /// This event is raised immediately after the camera transform has been updated, but before movement is updated.
    /// </summary>
    public event Action CameraUpdated;

    /// <summary>
    /// This event is raised right before the character controller is actually moved in order to provide other systems the opportunity to
    /// move the character controller in response to things other than user input, such as movement of the HMD. See CharacterCameraConstraint.cs
    /// for an example of this.
    /// </summary>
    public event Action PreCharacterMove;

    /// <summary>
    /// When true, user input will be applied to linear movement. Set this to false whenever the player controller needs to ignore input for
    /// linear movement.
    /// </summary>
    public bool EnableLinearMovement = true;

    /// <summary>
    /// When true, user input will be applied to rotation. Set this to false whenever the player controller needs to ignore input for rotation.
    /// </summary>
    public bool EnableRotation = true;

    /// <summary>
    /// Rotation defaults to secondary thumbstick. You can allow either here. Note that this won't behave well if EnableLinearMovement is true.
    /// </summary>
    public bool RotationEitherThumbstick = false;

    protected CharacterController Controller = null;
    protected OVRCameraRig CameraRig = null;

    private float MoveScale = 1.0f;
    private Vector3 MoveThrottle = Vector3.zero;
    private float FallSpeed = 0.0f;
    private OVRPose? InitialPose;
    public float InitialYRotation { get; private set; }
    private float MoveScaleMultiplier = 1.0f;
    private float RotationScaleMultiplier = 1.0f;

    // It is rare to want to use mouse movement in VR, so ignore the mouse by default.
    private bool SkipMouseRotation = true;

    private bool HaltUpdateMovement = false;
    private bool prevHatLeft = false;
    private bool prevHatRight = false;
    private float SimulationRate = 60f;
    private float buttonRotation = 0f;

    // Set to true when a snap turn has occurred, code requires one frame of centered thumbstick to enable another snap turn.
    private bool ReadyToSnapTurn;

    private bool playerControllerEnabled = false;

    void Start()
    {
        // Add eye-depth as a camera offset from the player controller
        var p = CameraRig.transform.localPosition;
        p.z = OVRManager.profile.eyeDepth;
        CameraRig.transform.localPosition = p;
    }

    void Awake()
    {
        Controller = gameObject.GetComponent<CharacterController>();

        if (Controller == null)
            Debug.LogWarning("OVRPlayerController: No CharacterController attached.");

        // We use OVRCameraRig to set rotations to cameras,
        // and to be influenced by rotation
        OVRCameraRig[] CameraRigs = gameObject.GetComponentsInChildren<OVRCameraRig>();

        if (CameraRigs.Length == 0)
            Debug.LogWarning("OVRPlayerController: No OVRCameraRig attached.");
        else if (CameraRigs.Length > 1)
            Debug.LogWarning("OVRPlayerController: More then 1 OVRCameraRig attached.");
        else
            CameraRig = CameraRigs[0];

        InitialYRotation = transform.rotation.eulerAngles.y;
    }

    void OnEnable()
    {
    }

    void OnDisable()
    {
        if (playerControllerEnabled)
        {
            OVRManager.display.RecenteredPose -= ResetOrientation;

            if (CameraRig != null)
            {
                CameraRig.UpdatedAnchors -= UpdateTransform;
            }

            playerControllerEnabled = false;
        }
    }

    void Update()
    {
        if (!playerControllerEnabled)
        {
            if (OVRManager.OVRManagerinitialized)
            {
                OVRManager.display.RecenteredPose += ResetOrientation;

                if (CameraRig != null)
                {
                    CameraRig.UpdatedAnchors += UpdateTransform;
                }

                playerControllerEnabled = true;
            }
            else
                return;
        }

        //todo: enable for Unity Input System
#if ENABLE_LEGACY_INPUT_MANAGER

        //Use keys to ratchet rotation
        if (Input.GetKeyDown(KeyCode.Q))
            buttonRotation -= RotationRatchet;

        if (Input.GetKeyDown(KeyCode.E))
            buttonRotation += RotationRatchet;
#endif
    }

    protected virtual void UpdateController()
    {
        if (useProfileData)
        {
            if (InitialPose == null)
            {
                // Save the initial pose so it can be recovered if useProfileData
                // is turned off later.
                InitialPose = new OVRPose()
                {
                    position = CameraRig.transform.localPosition,
                    orientation = CameraRig.transform.localRotation
                };
            }

            var p = CameraRig.transform.localPosition;
            if (OVRManager.instance.trackingOriginType == OVRManager.TrackingOrigin.EyeLevel)
            {
                p.y = OVRManager.profile.eyeHeight - (0.5f * Controller.height) + Controller.center.y;
            }
            else if (OVRManager.instance.trackingOriginType == OVRManager.TrackingOrigin.FloorLevel)
            {
                p.y = -(0.5f * Controller.height) + Controller.center.y;
            }

            CameraRig.transform.localPosition = p;
        }
        else if (InitialPose != null)
        {
            // Return to the initial pose if useProfileData was turned off at runtime
            CameraRig.transform.localPosition = InitialPose.Value.position;
            CameraRig.transform.localRotation = InitialPose.Value.orientation;
            InitialPose = null;
        }

        CameraHeight = CameraRig.centerEyeAnchor.localPosition.y;

        if (CameraUpdated != null)
        {
            CameraUpdated();
        }

        UpdateMovement();

        Vector3 moveDirection = Vector3.zero;

        float motorDamp = (1.0f + (Damping * SimulationRate * Time.deltaTime));

        MoveThrottle.x /= motorDamp;
        MoveThrottle.y = (MoveThrottle.y > 0.0f) ? (MoveThrottle.y / motorDamp) : MoveThrottle.y;
        MoveThrottle.z /= motorDamp;

        moveDirection += MoveThrottle * SimulationRate * Time.deltaTime;

        // Gravity
        if (Controller.isGrounded && FallSpeed <= 0)
            FallSpeed = ((Physics.gravity.y * (GravityModifier * 0.002f)));
        else
            FallSpeed += ((Physics.gravity.y * (GravityModifier * 0.002f)) * SimulationRate * Time.deltaTime);

        moveDirection.y += FallSpeed * SimulationRate * Time.deltaTime;


        if (Controller.isGrounded && MoveThrottle.y <= transform.lossyScale.y * 0.001f)
        {
            // Offset correction for uneven ground
            float bumpUpOffset = Mathf.Max(Controller.stepOffset,
                new Vector3(moveDirection.x, 0, moveDirection.z).magnitude);
            moveDirection -= bumpUpOffset * Vector3.up;
        }

        if (PreCharacterMove != null)
        {
            PreCharacterMove();
            Teleported = false;
        }

        Vector3 predictedXZ = Vector3.Scale((Controller.transform.localPosition + moveDirection), new Vector3(1, 0, 1));

        // Move contoller
        Controller.Move(moveDirection);
        Vector3 actualXZ = Vector3.Scale(Controller.transform.localPosition, new Vector3(1, 0, 1));

        if (MoveThrottle.magnitude < 0.00001f)
        {
            Controller.Move(OffsetHead());
        }

        if (predictedXZ != actualXZ)
            MoveThrottle += (actualXZ - predictedXZ) / (SimulationRate * Time.deltaTime);
    }


    public virtual void UpdateMovement()
    {
        //todo: enable for Unity Input System
#if ENABLE_LEGACY_INPUT_MANAGER
        if (HaltUpdateMovement)
            return;

        if (EnableLinearMovement)
        {
            bool moveForward = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
            bool moveLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
            bool moveRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
            bool moveBack = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

            bool dpad_move = false;

            if (OVRInput.Get(OVRInput.Button.DpadUp))
            {
                moveForward = true;
                dpad_move = true;
            }

            if (OVRInput.Get(OVRInput.Button.DpadDown))
            {
                moveBack = true;
                dpad_move = true;
            }

            MoveScale = 1.0f;

            if ((moveForward && moveLeft) || (moveForward && moveRight) ||
                (moveBack && moveLeft) || (moveBack && moveRight))
                MoveScale = 0.70710678f;

            // No positional movement if we are in the air
            if (!Controller.isGrounded)
                MoveScale = 0.0f;

            MoveScale *= SimulationRate * Time.deltaTime;

            // Compute this for key movement
            float moveInfluence = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;

            // Run!
            if (dpad_move || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                moveInfluence *= 2.0f;

            Quaternion ort = transform.rotation;
            Vector3 ortEuler = ort.eulerAngles;
            ortEuler.z = ortEuler.x = 0f;
            ort = Quaternion.Euler(ortEuler);

            if (moveForward)
                MoveThrottle += ort * (transform.lossyScale.z * moveInfluence * Vector3.forward);
            if (moveBack)
                MoveThrottle += ort * (transform.lossyScale.z * moveInfluence * BackAndSideDampen * Vector3.back);
            if (moveLeft)
                MoveThrottle += ort * (transform.lossyScale.x * moveInfluence * BackAndSideDampen * Vector3.left);
            if (moveRight)
                MoveThrottle += ort * (transform.lossyScale.x * moveInfluence * BackAndSideDampen * Vector3.right);

            moveInfluence = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;

#if !UNITY_ANDROID // LeftTrigger not avail on Android game pad
            moveInfluence *= 1.0f + OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
#endif

            Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

            // If speed quantization is enabled, adjust the input to the number of fixed speed steps.
            if (FixedSpeedSteps > 0)
            {
                primaryAxis.y = Mathf.Round(primaryAxis.y * FixedSpeedSteps) / FixedSpeedSteps;
                primaryAxis.x = Mathf.Round(primaryAxis.x * FixedSpeedSteps) / FixedSpeedSteps;
            }

            if (primaryAxis.y > 0.0f)
                MoveThrottle += ort * (primaryAxis.y * transform.lossyScale.z * moveInfluence * Vector3.forward);

            if (primaryAxis.y < 0.0f)
                MoveThrottle += ort * (Mathf.Abs(primaryAxis.y) * transform.lossyScale.z * moveInfluence *
                                       BackAndSideDampen * Vector3.back);

            if (primaryAxis.x < 0.0f)
                MoveThrottle += ort * (Mathf.Abs(primaryAxis.x) * transform.lossyScale.x * moveInfluence *
                                       BackAndSideDampen * Vector3.left);

            if (primaryAxis.x > 0.0f)
                MoveThrottle += ort * (primaryAxis.x * transform.lossyScale.x * moveInfluence * BackAndSideDampen *
                                       Vector3.right);
        }

        if (EnableRotation)
        {
            float rotate = 0;

            if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft))
            {
                if (ReadyToSnapTurn)
                {
                    rotate -= RotationRatchet;
                    ReadyToSnapTurn = false;
                }
            }
            else if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight))
            {
                if (ReadyToSnapTurn)
                {
                    rotate += RotationRatchet;
                    ReadyToSnapTurn = false;
                }
            }
            else
            {
                ReadyToSnapTurn = true;
            }

            if (rotate != 0)
            {
                transform.Rotate(Vector3.up, rotate);

                Controller.Move(OffsetHead());
            }
        }
#endif
    }


    /// <summary>
    /// Invoked by OVRCameraRig's UpdatedAnchors callback. Allows the Hmd rotation to update the facing direction of the player.
    /// </summary>
    public void UpdateTransform(OVRCameraRig rig)
    {
        Transform root = CameraRig.trackingSpace;
        Transform centerEye = CameraRig.centerEyeAnchor;

        if (HmdRotatesY && !Teleported)
        {
            Vector3 prevPos = root.position;
            Quaternion prevRot = root.rotation;

            transform.rotation = Quaternion.Euler(0.0f, centerEye.rotation.eulerAngles.y, 0.0f);

            root.position = prevPos;
            root.rotation = prevRot;
        }

        UpdateController();
        if (TransformUpdated != null)
        {
            TransformUpdated(root);
        }
    }

    /// <summary>
    /// Resets the player look rotation when the device orientation is reset.
    /// </summary>
    public void ResetOrientation()
    {
        if (HmdResetsY && !HmdRotatesY)
        {
            Vector3 euler = transform.rotation.eulerAngles;
            euler.y = InitialYRotation;
            transform.rotation = Quaternion.Euler(euler);
        }
    }

    [SerializeField] Transform _centerOffset;
    [SerializeField] Transform _centerEye;

    Vector3 _prevHead2dLocalPos;

    Vector3 OffsetHead()
    {
        Vector3 head2dLocalPos = Vector3.Scale(_centerOffset.InverseTransformPoint(_centerEye.transform.position), new Vector3(1, 0, 1));
        Vector3 localOffset = head2dLocalPos - _prevHead2dLocalPos;
        _prevHead2dLocalPos = head2dLocalPos;

        _centerOffset.transform.localPosition -= localOffset;
        Vector3 offset = _centerOffset.TransformPoint(localOffset) - _centerOffset.transform.position;

        return offset;
    }
}

/*
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Transform _ovrRig;
    [SerializeField] Transform _centerEye;
    [SerializeField] Transform _forwardDirection;
    [SerializeField] CharacterController _controller;

    [SerializeField] float _speed = 1;

    [SerializeField] float _rotateRate = 45;

    Vector3 _prevHead2dLocalPos;

    bool _readyToSnapTurn;

    void LateUpdate()
    {
        Turn();

        MoveCharacter();

        OffsetHead();
    }

    void MoveCharacter()
    {
        _forwardDirection.eulerAngles = new Vector3(0, _centerEye.transform.eulerAngles.y, 0);

        Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Vector3 moveDirection = _forwardDirection.TransformDirection(new Vector3(primaryAxis.x, 0, primaryAxis.y));

        _controller.Move(moveDirection * Time.deltaTime * _speed);
    }

    void OffsetHead()
    {
        Vector3 head2dLocalPos = new Vector3(_centerEye.localPosition.x, 0, _centerEye.localPosition.z);
        Vector3 localOffset = head2dLocalPos - _prevHead2dLocalPos;
        _prevHead2dLocalPos = head2dLocalPos;

        _ovrRig.transform.localPosition = -head2dLocalPos;
        Vector3 offset = _ovrRig.TransformPoint(localOffset) - _ovrRig.transform.position;

        transform.position += offset;
    }

    void Turn()
    {
        float rotate = 0;

        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft))
        {
            if (_readyToSnapTurn)
            {
                rotate -= _rotateRate;
                _readyToSnapTurn = false;
            }
        }
        else if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight))
        {
            if (_readyToSnapTurn)
            {
                rotate += _rotateRate;
                _readyToSnapTurn = false;
            }
        }
        else
        {
            _readyToSnapTurn = true;
        }

        if (rotate != 0)
        {
            transform.Rotate(Vector3.up, rotate);
        }
    }
}


*/