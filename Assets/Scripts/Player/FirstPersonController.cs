using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;


namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {

        [Header("Objects")]
        [SerializeField] Data_Manager dataManager;
        [SerializeField] Player_UI playerUI;
        [SerializeField] Camera _mainCamera;
        [SerializeField] Transform camOffet;

        [Header("Input")]
        [SerializeField] InputActionProperty moveAxis;
        [SerializeField] InputActionProperty jump;
        [SerializeField] InputActionProperty crouch;
        [SerializeField] InputActionProperty editorMenu;
        [SerializeField] InputActionProperty run;
        [SerializeField] InputActionProperty fovUp;
        [SerializeField] InputActionProperty fovDown;
        [SerializeField] InputActionProperty flyMenu;


        [Header("Stats")]
        [SerializeField] float crouchSpeed = 7;
        [SerializeField] float acceleration = 9;
        [SerializeField] float deceleration = 6;
        [SerializeField] float baseSpeed = 120;
        [SerializeField] float hasMovedDeltaTimeout = 15;
        [SerializeField] float JumpHeight = 1.0f;
        [SerializeField] float Gravity = -15.0f;
        [SerializeField] float FallTimeout = 0.15f;
        [SerializeField] float _terminalVelocity = 180f;
        [SerializeField] float sensitivity = 10f;
        [SerializeField] float sensitivityFly = 10f;
        [SerializeField] float maxYAngle = 95f;
        [SerializeField] AnimationCurve crouchCurve;
        [SerializeField] AnimationCurve fovCurve;

        //Objects
        CharacterController _controller;

        //Inputs
        PlayerPositionData currentPositionData;
        Vector2 currentRotation;
        float height;
        float fov = 80f;
        float fovAdder = 0;
        float fovFly = 20f;
        float fovFlyAdder = 0;
        Vector3 currentFlyModePan = Vector3.zero;
        Vector2 lastFlyCamRotation;
        Vector2 lastRegularCamRotation;

        //Inputs
         bool inputJump;
         bool inputCrouch;
         bool inputCrouchAlreadyPressed;
         bool inputEditorMenu;
         bool inputEditorMenuAlreadyPressed;
         bool inputRun;
         bool inputFlyMenu;
         bool inputFlyMenuAlreadyPressed;
         Vector2 inputMovement;


        private void Awake()
        {
            _controller = GetComponent<CharacterController>();

            height = PlayerPrefs.GetFloat("Settings: PlayerHeight");
            if (height == 0)
            {
                height = 5.5f;
                PlayerPrefs.SetFloat("Settings: PlayerHeight", height);
            }
            height -= 0.41f;

            fov = PlayerPrefs.GetFloat("Settings: FOV");
            if (fov == 0)
            {
                fov = 70;
                PlayerPrefs.SetFloat("Settings: FOV", fov);
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void OnEnable()
        {
            moveAxis.action?.Enable();
            jump.action?.Enable();
            crouch.action?.Enable();
            editorMenu.action?.Enable();
            run.action?.Enable();
            fovUp.action?.Enable();
            fovDown.action?.Enable();
            flyMenu.action?.Enable();
        }

        void Update()
        {
            if (_controller == null) { return; }

            //Menu
            if (!inputFlyMenu)
            {
                EditorMenuCheck();
            }
            if (!inputEditorMenu)
            {
                FlyMenuChuck();
            }

            inputMovement = Vector2.zero;
            inputJump = false;

            //Mouselook
            if (!inputEditorMenu && !inputFlyMenu)
            {
                GetPlayerCameraInputs();
                GetPlayerMovingInputs();
                GetPauseEscapeInputs();
                UpdateFOV();

                _mainCamera.transform.localPosition = Vector3.zero;

            }
            if (inputFlyMenu)
            {
                GetPlayerCameraInputs();
                GetPauseEscapeInputs();
                UpdateFOV();

                float flyRadius = dataManager.GetWorldFlyingSphereSize();

                if (Input.GetMouseButton(0))
                {
                    currentFlyModePan -= _mainCamera.transform.up * Input.GetAxis("Mouse Y") * sensitivityFly * Mathf.Min(fovFly, 70);
                    currentFlyModePan -= _mainCamera.transform.right * Input.GetAxis("Mouse X") * sensitivityFly * Mathf.Min(fovFly, 70);
                    if(Vector3.Distance(Vector3.zero,currentFlyModePan) > flyRadius)
                    {
                        currentFlyModePan = (currentFlyModePan - Vector3.zero).normalized;
                        currentFlyModePan *= flyRadius;
                    }
                }

                _mainCamera.transform.position = Vector3.zero + currentFlyModePan - (300 * _mainCamera.transform.forward);
            }

            //Move
            currentPositionData.position = transform.position;
            currentPositionData.velocity = _controller.velocity;
            currentPositionData.mainCamforward = _mainCamera.transform.forward;
            currentPositionData.mainCamRight = _mainCamera.transform.right;
            currentPositionData = MovePlayer(currentPositionData);
            _controller.Move(currentPositionData.velocity);
        }

        void GetPlayerMovingInputs()
        {
            //Grab Inputs
            inputMovement = moveAxis.action.ReadValue<Vector2>();
            inputJump = jump.action.IsPressed();
            inputRun = run.action.IsPressed();

            //Crouch
            if (crouch.action.IsPressed() && !inputCrouchAlreadyPressed)
            {
                inputCrouchAlreadyPressed = true;
                inputCrouch = !inputCrouch;
            }
            else if (!crouch.action.IsPressed())
            {
                inputCrouchAlreadyPressed = false;
            }
        }

        void GetPauseEscapeInputs()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        void GetPlayerCameraInputs()
        {
            if (!inputFlyMenu || (inputFlyMenu && Input.GetMouseButton(1)))
            {
                currentRotation.x += Input.GetAxis("Mouse X") * sensitivity * Mathf.Min(fov,70);
                currentRotation.y -= Input.GetAxis("Mouse Y") * sensitivity * Mathf.Min(fov, 70);
            }
            currentRotation.x = Mathf.Repeat(currentRotation.x, 360);
            currentRotation.y = Mathf.Clamp(currentRotation.y, -maxYAngle, maxYAngle);
            Quaternion rot = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);
            _mainCamera.transform.rotation = rot;
            //Fov
            if (inputFlyMenu)
            {
                if (fovUp.action.IsPressed())
                {
                    fovFlyAdder = -1.0f;
                }
                if (fovDown.action.IsPressed())
                {
                    fovFlyAdder = 1.0f;
                }
                fovFly = Mathf.Clamp(Mathf.Lerp(fovFly, fovFly + (fovFlyAdder), fovCurve.Evaluate(Mathf.Abs(fovFlyAdder))), 0.2f, 40f);
                fovFlyAdder = Mathf.Clamp01(Mathf.Abs(fovFlyAdder) - (Time.deltaTime * 10f)) * Mathf.Sign(fovFlyAdder);
            }
            else
            {
                if (fovUp.action.IsPressed())
                {
                    fovAdder = -1.0f;
                }
                if (fovDown.action.IsPressed())
                {
                    fovAdder = 1.0f;
                }
                fov = Mathf.Clamp(Mathf.Lerp(fov, fov + (fovAdder / 2.0f), fovCurve.Evaluate(Mathf.Abs(fovAdder))), 1.0f, 120f);
                fovAdder = Mathf.Clamp01(Mathf.Abs(fovAdder) - (Time.deltaTime * 1.5f)) * Mathf.Sign(fovAdder);
            }

        }

        PlayerPositionData MovePlayer(PlayerPositionData currentPositionData)
        {

            Vector3 forward = currentPositionData.mainCamforward;
            Vector3 right = currentPositionData.mainCamRight;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            Vector3 newAxis = forward * inputMovement.y + right * inputMovement.x;
            Vector3 targetSpeed = new Vector3(newAxis.x, 0.0f, newAxis.z).normalized * baseSpeed / 25.0f;
            if (inputRun)
            {
                targetSpeed *= 2f;
            }


            //Crouch
            if (inputCrouch && _controller.isGrounded)
            {
                currentPositionData.currentCrouchLerp = Mathf.Clamp01(currentPositionData.currentCrouchLerp + (Time.deltaTime * crouchSpeed));
            }
            else
            {
                currentPositionData.currentCrouchLerp = Mathf.Clamp01(currentPositionData.currentCrouchLerp - (Time.deltaTime * crouchSpeed));
            }
            targetSpeed *= ((1 - currentPositionData.currentCrouchLerp) / 2.0f) + 0.5f;
            ModifyPlayerHeight();

            //Movement rotation halted in midair
            if (!_controller.isGrounded)
            {
                if (!currentPositionData.hasBeenGrounded)
                {
                    currentPositionData.oldAxis = newAxis;
                    currentPositionData.oldInput = inputMovement;
                    currentPositionData.hasBeenGrounded = true;
                }
                else
                {
                    if (!currentPositionData.hasBeenStopped)
                    {
                        //No Starting Input In Air
                        if (currentPositionData.oldAxis == Vector3.zero)
                        {
                            currentPositionData.oldAxis = newAxis;
                            currentPositionData.oldInput = inputMovement;
                        }
                        else if (Vector2.Dot(currentPositionData.oldInput, inputMovement) < -0.5f)
                        {
                            //Stop Mid-air if holding opposite direction
                            newAxis = Vector3.zero;
                            currentPositionData.hasBeenStopped = true;
                        }
                        else
                        {
                            newAxis = currentPositionData.oldAxis;
                        }
                    }
                    else
                    {
                        newAxis = Vector3.zero;
                    }
                }
            }
            else
            {
                currentPositionData.hasBeenGrounded = false;
                currentPositionData.hasBeenStopped = false;
                // accelerate or decelerate to target speed
                if (newAxis == Vector3.zero) targetSpeed = Vector2.zero;


                if (targetSpeed != Vector3.zero)
                {
                    if (currentPositionData._hasBeenMovingDelta > 0.01f)
                    {
                        currentPositionData._speed.x = Mathf.Lerp(currentPositionData.velocity.x, targetSpeed.x, Time.deltaTime * acceleration);
                        currentPositionData._speed.z = Mathf.Lerp(currentPositionData.velocity.z, targetSpeed.z, Time.deltaTime * acceleration);
                    }
                    else
                    {
                        currentPositionData._speed = targetSpeed;
                    }
                }
                else
                {
                    currentPositionData._speed.x = Mathf.Lerp(currentPositionData.velocity.x, targetSpeed.x, Time.deltaTime * deceleration);
                    currentPositionData._speed.z = Mathf.Lerp(currentPositionData.velocity.z, targetSpeed.z, Time.deltaTime * deceleration);
                }
            }

            //Jump
            if (_controller.isGrounded)
            {
                // reset the fall timeout timer
                currentPositionData._fallTimeoutDelta = FallTimeout;

                // stop our velocity dropping infinitely when grounded
                if (currentPositionData._verticalVelocity < 0.0f)
                {
                    currentPositionData._verticalVelocity = -2f;
                }

                // Jump
                if (inputJump)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    currentPositionData._verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                }
            }
            else
            {
                // fall timeout
                if (currentPositionData._fallTimeoutDelta >= 0.0f)
                {
                    currentPositionData._fallTimeoutDelta -= Time.deltaTime;
                }
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (currentPositionData._verticalVelocity < _terminalVelocity)
            {
                currentPositionData._verticalVelocity += Gravity * Time.deltaTime;
            }

            if (inputMovement.magnitude == 0)
            {
                currentPositionData._hasBeenMovingDelta = Mathf.Lerp(currentPositionData._hasBeenMovingDelta, 0, Time.deltaTime * hasMovedDeltaTimeout);
            }
            else
            {
                currentPositionData._hasBeenMovingDelta = Mathf.Lerp(currentPositionData._hasBeenMovingDelta, 1, Time.deltaTime * hasMovedDeltaTimeout);
            }

            // move the player
            Vector3 finalVelocity = (currentPositionData._speed * Time.deltaTime) + new Vector3(0.0f, currentPositionData._verticalVelocity, 0.0f) * Time.deltaTime;

            currentPositionData.velocity = finalVelocity;
            return currentPositionData;
        }

        void EditorMenuCheck()
        {
            if (editorMenu.action.IsPressed() && !inputEditorMenuAlreadyPressed)
            {
                inputEditorMenuAlreadyPressed = true;
                if (inputEditorMenu)
                {
                    inputEditorMenu = !playerUI.CheckIfCanExitMenu();
                }
                else
                {
                    inputEditorMenu = true;
                }
                if (inputEditorMenu)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }

            }
            else if (!editorMenu.action.IsPressed())
            {
                inputEditorMenuAlreadyPressed = false;
            }
        }

        void FlyMenuChuck()
        {
            if (flyMenu.action.IsPressed() && !inputFlyMenuAlreadyPressed)
            {
                inputFlyMenuAlreadyPressed = true;
                if (inputFlyMenu)
                {
                    inputFlyMenu = !playerUI.CheckIfCanExitMenu();
                }
                else
                {
                    inputFlyMenu = true;
                }
                if (inputFlyMenu)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    lastRegularCamRotation = currentRotation;
                    currentRotation = lastFlyCamRotation;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    lastFlyCamRotation = currentRotation;
                    currentRotation = lastRegularCamRotation;
                }

            }
            else if (!flyMenu.action.IsPressed())
            {
                inputFlyMenuAlreadyPressed = false;
            }
        }

        public void UpdateFOV()
        {
            if (inputFlyMenu)
            {
                _mainCamera.fieldOfView = fovFly;
            }
            else
            {
                _mainCamera.fieldOfView = fov;
            }
        }

        public void ForceNewPosition(Vector3 pos)
        {
            _controller.enabled = false;
            transform.position = pos;
            _controller.enabled = true;
        }


        void ModifyPlayerHeight()
        {
            float newHeight = Mathf.Lerp(height, height / 2.0f, crouchCurve.Evaluate(currentPositionData.currentCrouchLerp));
            camOffet.localPosition = new Vector3(0, newHeight, 0);
            _controller.height = newHeight + 0.41f;
            _controller.center = new Vector3(0, (newHeight + 0.41f) / 2.0f, 0);
        }

        public bool CheckifPlayerInMenu()
        {
            return inputEditorMenu;
        }

    }


    [System.Serializable]
    public struct PlayerPositionData
    {
        //Position
        public Vector3 position;
        public Vector3 velocity;

        //Physics Calculations
        public Vector3 _speed;
        public float _verticalVelocity;
        public float _hasBeenMovingDelta;
        public Vector3 oldAxis;
        public Vector2 oldInput;
        public bool hasBeenGrounded;
        public bool hasBeenStopped;
        public float currentCrouchLerp;
        public bool hasBeenCrouched;
        public Vector3 mainCamforward;
        public Vector3 mainCamRight;
        public float _fallTimeoutDelta;
    }
}