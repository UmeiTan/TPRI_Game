using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CapsuleCollider)), RequireComponent(typeof(Rigidbody))]

public class PlayerMove : MonoBehaviour
{
    #region Variables
    
    #region Look Settings
    [SerializeField] private bool _lockAndHideCursor = false;
    [SerializeField] private bool _enableCameraMovement = true;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Sprite _point;
    [SerializeField] private Vector3 _targetAngles;
    private Vector3 _followAngles;
    private Vector3 _followVelocity;
    [SerializeField] private float _verticalRotationRange = 170;
    [SerializeField] private float _mouseSensitivity = 10;
    [SerializeField] private float _cameraSmoothing = 5f;
    [SerializeField] private bool _enableCameraShake = false;
    private Canvas _canvas;
    [SerializeField] private float _capsuleRadius = 0.5f;
    #endregion

    #region Movement Settings
    [SerializeField] private bool _playerCanMove = true;
    [SerializeField] private float _speed;
    [SerializeField] private float _walkSpeed = 4f;
    [SerializeField] private float _sprintSpeed = 8f;
    [SerializeField] private KeyCode _sprintKey = KeyCode.LeftShift;
    private CapsuleCollider _capsule;
    [SerializeField] Rigidbody _fpsRigidbody;
    [SerializeField] private float _gravityMultiplier = 1.0f;
    [SerializeField] private float _maxStepHeight = 0.2f;
    internal bool stairMiniHop = false;
    private float _yVelocity = Physics.gravity.y;
    [SerializeField] private bool _isGrounded;// { get; private set; }
    private Vector2 _inputXy;
    #endregion

    #region Headbobbing Settings
    [SerializeField] private bool _useHeadbob = true;
    [SerializeField] private Transform _head = null;
    [SerializeField] private bool _snapHeadjointToCapsul = true;
    [SerializeField] private float _headbobFrequency = 1.5f;
    [SerializeField] private float _headbobSwayAngle = 5f;
    [SerializeField] private float _headbobHeight = 3f;
    [SerializeField] private float _headbobSideMovement = 5f;
    [SerializeField] private float _jumpLandIntensity = 3f;
    private Vector3 _originalLocalPosition;
    private float _nextStepTime = 0.5f;
    private float _headbobCycle = 0.0f;
    private float _headbobFade = 0.0f;
    private float _springPosition = 0.0f;
    private float _springVelocity = 0.0f;
    private float _springElastic = 1.1f;
    private float _springDampen = 0.8f;
    private float _springVelocityThreshold = 0.05f;
    private float _springPositionThreshold = 0.05f;
    private Vector3 _previousPosition;
    private Vector3 _previousVelocity = Vector3.zero;
    private Vector3 _miscRefVel;
    private bool _previousGrounded;
    private AudioSource _audioSource;
    #endregion

    #region Audio Settings
    [SerializeField] private bool _enableAudioSfx = true;
    [SerializeField] private float _volume = 5f;
    [SerializeField] private AudioClip _jumpSound = null;
    [SerializeField] private AudioClip _landSound = null;
    [SerializeField] private List<AudioClip> _footStepSounds = null;
    [SerializeField] private enum FsMode { Static, Dynamic }
    [SerializeField] private FsMode _fsmode;
    [System.Serializable] public class DynamicFootStep
    {
        public enum MatMode { PhysicMaterial, Material };
        public MatMode materialMode;
        public List<PhysicMaterial> woodPhysMat;
        public List<PhysicMaterial> metalAndGlassPhysMat;
        public List<PhysicMaterial> grassPhysMat;
        public List<PhysicMaterial> dirtAndGravelPhysMat;
        public List<PhysicMaterial> rockAndConcretePhysMat;
        public List<PhysicMaterial> mudPhysMat;
        public List<PhysicMaterial> customPhysMat;

        public List<Material> woodMat;
        public List<Material> metalAndGlassMat;
        public List<Material> grassMat;
        public List<Material> dirtAndGravelMat;
        public List<Material> rockAndConcreteMat;
        public List<Material> mudMat;
        public List<Material> customMat;
        public List<AudioClip> currentClipSet;

        public List<AudioClip> woodClipSet;
        public List<AudioClip> metalAndGlassClipSet;
        public List<AudioClip> grassClipSet;
        public List<AudioClip> dirtAndGravelClipSet;
        public List<AudioClip> rockAndConcreteClipSet;
        public List<AudioClip> mudClipSet;
        public List<AudioClip> customClipSet;
    }
    public DynamicFootStep dynamicFootstep = new DynamicFootStep();
    #endregion

    #endregion

    private void Awake()
    {
        _capsule = GetComponent<CapsuleCollider>();    
        _fpsRigidbody = GetComponent<Rigidbody>();
        _fpsRigidbody.interpolation = RigidbodyInterpolation.Extrapolate;
        _fpsRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;   
    }

    private void Start()
    {
        #region Look Settings - Start

        _canvas = new GameObject("Point").AddComponent<Canvas>();
        _canvas.gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.pixelPerfect = true;
        _canvas.transform.SetParent(_playerCamera.transform);
        _canvas.transform.position = Vector3.zero;

        Image crossHair = new GameObject("Point").AddComponent<Image>();
        crossHair.sprite = _point;
        crossHair.rectTransform.sizeDelta = new Vector2(3, 3);
        crossHair.transform.SetParent(_canvas.transform);
        crossHair.transform.position = Vector3.zero;
            
        if (_lockAndHideCursor) {
            Cursor.lockState = CursorLockMode.Locked; 
            Cursor.visible = false; 
        }

        #endregion

        #region Movement Settings - Start  
        _capsule.radius = _capsuleRadius;
        
        #endregion

        #region Headbobbing Settings - Start

        _originalLocalPosition = _snapHeadjointToCapsul ? new Vector3(_head.localPosition.x, (_capsule.height / 2) * _head.localScale.y, _head.localPosition.z) : _head.localPosition;
        if (GetComponent<AudioSource>() == null) { gameObject.AddComponent<AudioSource>(); }

        _previousPosition = _fpsRigidbody.position;
        _audioSource = GetComponent<AudioSource>();
        #endregion

        
    }

    private void Update()
    {

        #region Look Settings - Update

        if (_enableCameraMovement)
        {
            float mouseYInput = 0;
            float mouseXInput = 0;
            float camFov = _playerCamera.fieldOfView;
            
            mouseYInput = Input.GetAxis("Mouse Y");
            mouseXInput = Input.GetAxis("Mouse X");
          
            if (_targetAngles.y > 180) { _targetAngles.y -= 360; _followAngles.y -= 360; } else if (_targetAngles.y < -180) { _targetAngles.y += 360; _followAngles.y += 360; }
            if (_targetAngles.x > 180) { _targetAngles.x -= 360; _followAngles.x -= 360; } else if (_targetAngles.x < -180) { _targetAngles.x += 360; _followAngles.x += 360; }
            _targetAngles.y += mouseXInput * _mouseSensitivity;
            _targetAngles.x += mouseYInput * _mouseSensitivity;
            _targetAngles.x = Mathf.Clamp(_targetAngles.x, -0.5f * _verticalRotationRange, 0.5f * _verticalRotationRange);
            _followAngles = Vector3.SmoothDamp(_followAngles, _targetAngles, ref _followVelocity, (_cameraSmoothing) / 100);

            _playerCamera.transform.localRotation = Quaternion.Euler(-_followAngles.x, 0, 0);
            transform.localRotation = Quaternion.Euler(0, _followAngles.y, 0);
        }

        #endregion
    }

    private void FixedUpdate()
    {

        #region Movement Settings - FixedUpdate

        Vector3 moveDirection = Vector3.zero;
        _speed = Input.GetKey(_sprintKey) ? _sprintSpeed : _walkSpeed;

        moveDirection = (transform.forward * _inputXy.y * _speed + transform.right * _inputXy.x * _speed * 0.8f);

        #region step logic
        RaycastHit wt;
        if (_maxStepHeight > 0 && Physics.Raycast(transform.position - new Vector3(0, ((_capsule.height / 2) * transform.localScale.y) - 0.01f, 0), moveDirection, out wt, _capsule.radius + 0.15f, Physics.AllLayers, QueryTriggerInteraction.Ignore) && Vector3.Angle(wt.normal, Vector3.up) > 88)
        {
            RaycastHit st;
            if (!Physics.Raycast(transform.position - new Vector3(0, ((_capsule.height / 2) * transform.localScale.y) - (_maxStepHeight), 0), moveDirection, out st, _capsule.radius + 0.25f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                stairMiniHop = true;
                transform.position += new Vector3(0, _maxStepHeight * 1.2f, 0);
            }
        }
        Debug.DrawRay(transform.position, moveDirection, Color.red, 0, false);
        #endregion

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        _inputXy = new Vector2(horizontalInput, verticalInput);


        if (_inputXy.magnitude > 1) { _inputXy.Normalize(); }

        if (_playerCanMove && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)))
        {
            _fpsRigidbody.velocity = moveDirection + (Vector3.up * _yVelocity);
            
        }
        else { _fpsRigidbody.velocity = Vector3.zero; }



        _fpsRigidbody.AddForce(Physics.gravity * (_gravityMultiplier - 1));

        if (Mathf.Abs(_fpsRigidbody.velocity.x) > 0.5f || Mathf.Abs(_fpsRigidbody.velocity.z) > 0.5f)
        {
            //          playerCamera.fieldOfView = Mathf.SmoothDamp(playerCamera.fieldOfView, baseCamFOV + (advanced.FOVKickAmount * 2), ref advanced.fovRef, advanced.changeTime);
        }

        #endregion

        #region Headbobbing Settings - FixedUpdate
        float yPos = 0;
        float xPos = 0;
        float zTilt = 0;
        float xTilt = 0;
        float bobSwayFactor = 0;
        float bobFactor = 0;
        float strideLangthen = 0;
        float flatVel = 0;
        //calculate headbob freq
        if (_useHeadbob == true || _enableAudioSfx)
        {
            Vector3 vel = (_fpsRigidbody.position - _previousPosition) / Time.deltaTime;
            Vector3 velChange = vel - _previousVelocity;
            _previousPosition = _fpsRigidbody.position;
            _previousVelocity = vel;
            _springVelocity -= velChange.y;
            _springVelocity -= _springPosition * _springElastic;
            _springVelocity *= _springDampen;
            _springPosition += _springVelocity * Time.deltaTime;
            _springPosition = Mathf.Clamp(_springPosition, -0.3f, 0.3f);

            if (Mathf.Abs(_springVelocity) < _springVelocityThreshold && Mathf.Abs(_springPosition) < _springPositionThreshold) { _springPosition = 0; _springVelocity = 0; }
            flatVel = new Vector3(vel.x, 0.0f, vel.z).magnitude;
            strideLangthen = 1 + (flatVel * ((_headbobFrequency * 2) / 10));
            _headbobCycle += (flatVel / strideLangthen) * (Time.deltaTime / _headbobFrequency);
            bobFactor = Mathf.Sin(_headbobCycle * Mathf.PI * 2);
            bobSwayFactor = Mathf.Sin(Mathf.PI * (2 * _headbobCycle + 0.5f));
            bobFactor = 1 - (bobFactor * 0.5f + 1);
            bobFactor *= bobFactor;

            yPos = 0;
            xPos = 0;
            zTilt = 0;

            if (_isGrounded)
            {
                if (new Vector3(vel.x, 0.0f, vel.z).magnitude < 0.1f) { _headbobFade = Mathf.MoveTowards(_headbobFade, 0.0f, 0.5f); } else { _headbobFade = Mathf.MoveTowards(_headbobFade, 1.0f, Time.deltaTime); }
                float speedHeightFactor = 1 + (flatVel * 0.3f);
                xPos = -(_headbobSideMovement / 10) * _headbobFade * bobSwayFactor;
                yPos = _springPosition * (_jumpLandIntensity / 10) + bobFactor * (_headbobHeight / 10) * _headbobFade * speedHeightFactor;
                zTilt = bobSwayFactor * (_headbobSwayAngle / 10) * _headbobFade;
            }
        }
        //apply headbob position
        if (_useHeadbob == true)
        {
            if (_fpsRigidbody.velocity.magnitude > 0.1f)
            {
                _head.localPosition = Vector3.MoveTowards(_head.localPosition, _snapHeadjointToCapsul ? (new Vector3(_originalLocalPosition.x, (_capsule.height / 2) * _head.localScale.y, _originalLocalPosition.z) + new Vector3(xPos, yPos, 0)) : _originalLocalPosition + new Vector3(xPos, yPos, 0), 0.5f);
            }
            else
            {
                _head.localPosition = Vector3.SmoothDamp(_head.localPosition, _snapHeadjointToCapsul ? (new Vector3(_originalLocalPosition.x, (_capsule.height / 2) * _head.localScale.y, _originalLocalPosition.z) + new Vector3(xPos, yPos, 0)) : _originalLocalPosition + new Vector3(xPos, yPos, 0), ref _miscRefVel, 0.15f);
            }
            _head.localRotation = Quaternion.Euler(xTilt, 0, zTilt);


        }
        #endregion

        #region Dynamic Footsteps
        if (_enableAudioSfx)
        {
            if (_fsmode == FsMode.Dynamic)
            {
                RaycastHit hit = new RaycastHit();

                if (Physics.Raycast(transform.position, Vector3.down, out hit))
                {

                    if (dynamicFootstep.materialMode == DynamicFootStep.MatMode.PhysicMaterial)
                    {
                        dynamicFootstep.currentClipSet = (dynamicFootstep.woodPhysMat.Any() && dynamicFootstep.woodPhysMat.Contains(hit.collider.sharedMaterial) && dynamicFootstep.woodClipSet.Any()) ? // If standing on Wood
                        dynamicFootstep.woodClipSet : ((dynamicFootstep.grassPhysMat.Any() && dynamicFootstep.grassPhysMat.Contains(hit.collider.sharedMaterial) && dynamicFootstep.grassClipSet.Any()) ? // If standing on Grass
                        dynamicFootstep.grassClipSet : ((dynamicFootstep.metalAndGlassPhysMat.Any() && dynamicFootstep.metalAndGlassPhysMat.Contains(hit.collider.sharedMaterial) && dynamicFootstep.metalAndGlassClipSet.Any()) ? // If standing on Metal/Glass
                        dynamicFootstep.metalAndGlassClipSet : ((dynamicFootstep.rockAndConcretePhysMat.Any() && dynamicFootstep.rockAndConcretePhysMat.Contains(hit.collider.sharedMaterial) && dynamicFootstep.rockAndConcreteClipSet.Any()) ? // If standing on Rock/Concrete
                        dynamicFootstep.rockAndConcreteClipSet : ((dynamicFootstep.dirtAndGravelPhysMat.Any() && dynamicFootstep.dirtAndGravelPhysMat.Contains(hit.collider.sharedMaterial) && dynamicFootstep.dirtAndGravelClipSet.Any()) ? // If standing on Dirt/Gravle
                        dynamicFootstep.dirtAndGravelClipSet : ((dynamicFootstep.mudPhysMat.Any() && dynamicFootstep.mudPhysMat.Contains(hit.collider.sharedMaterial) && dynamicFootstep.mudClipSet.Any()) ? // If standing on Mud
                        dynamicFootstep.mudClipSet : ((dynamicFootstep.customPhysMat.Any() && dynamicFootstep.customPhysMat.Contains(hit.collider.sharedMaterial) && dynamicFootstep.customClipSet.Any()) ? // If standing on the custom material 
                        dynamicFootstep.customClipSet : _footStepSounds)))))); // If material is unknown, fall back
                    }
                    else if (hit.collider.GetComponent<MeshRenderer>())
                    {
                        dynamicFootstep.currentClipSet = (dynamicFootstep.woodMat.Any() && dynamicFootstep.woodMat.Contains(hit.collider.GetComponent<MeshRenderer>().sharedMaterial) && dynamicFootstep.woodClipSet.Any()) ? // If standing on Wood
                        dynamicFootstep.woodClipSet : ((dynamicFootstep.grassMat.Any() && dynamicFootstep.grassMat.Contains(hit.collider.GetComponent<MeshRenderer>().sharedMaterial) && dynamicFootstep.grassClipSet.Any()) ? // If standing on Grass
                        dynamicFootstep.grassClipSet : ((dynamicFootstep.metalAndGlassMat.Any() && dynamicFootstep.metalAndGlassMat.Contains(hit.collider.GetComponent<MeshRenderer>().sharedMaterial) && dynamicFootstep.metalAndGlassClipSet.Any()) ? // If standing on Metal/Glass
                        dynamicFootstep.metalAndGlassClipSet : ((dynamicFootstep.rockAndConcreteMat.Any() && dynamicFootstep.rockAndConcreteMat.Contains(hit.collider.GetComponent<MeshRenderer>().sharedMaterial) && dynamicFootstep.rockAndConcreteClipSet.Any()) ? // If standing on Rock/Concrete
                        dynamicFootstep.rockAndConcreteClipSet : ((dynamicFootstep.dirtAndGravelMat.Any() && dynamicFootstep.dirtAndGravelMat.Contains(hit.collider.GetComponent<MeshRenderer>().sharedMaterial) && dynamicFootstep.dirtAndGravelClipSet.Any()) ? // If standing on Dirt/Gravle
                        dynamicFootstep.dirtAndGravelClipSet : ((dynamicFootstep.mudMat.Any() && dynamicFootstep.mudMat.Contains(hit.collider.GetComponent<MeshRenderer>().sharedMaterial) && dynamicFootstep.mudClipSet.Any()) ? // If standing on Mud
                        dynamicFootstep.mudClipSet : ((dynamicFootstep.customMat.Any() && dynamicFootstep.customMat.Contains(hit.collider.GetComponent<MeshRenderer>().sharedMaterial) && dynamicFootstep.customClipSet.Any()) ? // If standing on the custom material 
                        dynamicFootstep.customClipSet : _footStepSounds.Any() ? _footStepSounds : null)))))); // If material is unknown, fall back
                    }

                    if (_isGrounded)
                    {
                        if (!_previousGrounded)
                        {
                            if (dynamicFootstep.currentClipSet.Any()) { _audioSource.PlayOneShot(dynamicFootstep.currentClipSet[Random.Range(0, dynamicFootstep.currentClipSet.Count)], _volume / 10); }
                            _nextStepTime = _headbobCycle + 0.5f;
                        }
                        else
                        {
                            if (_headbobCycle > _nextStepTime)
                            {
                                _nextStepTime = _headbobCycle + 0.5f;
                                if (dynamicFootstep.currentClipSet.Any()) { _audioSource.PlayOneShot(dynamicFootstep.currentClipSet[Random.Range(0, dynamicFootstep.currentClipSet.Count)], _volume / 10); }
                            }
                        }
                        _previousGrounded = true;
                    }
                    else
                    {
                        if (_previousGrounded)
                        {
                            if (dynamicFootstep.currentClipSet.Any()) { _audioSource.PlayOneShot(dynamicFootstep.currentClipSet[Random.Range(0, dynamicFootstep.currentClipSet.Count)], _volume / 10); }
                        }
                        _previousGrounded = false;
                    }

                }
                else
                {
                    dynamicFootstep.currentClipSet = _footStepSounds;
                    if (_isGrounded)
                    {
                        if (!_previousGrounded)
                        {
                            if (_landSound) { _audioSource.PlayOneShot(_landSound, _volume / 10); }
                            _nextStepTime = _headbobCycle + 0.5f;
                        }
                        else
                        {
                            if (_headbobCycle > _nextStepTime)
                            {
                                _nextStepTime = _headbobCycle + 0.5f;
                                int n = Random.Range(0, _footStepSounds.Count);
                                if (_footStepSounds.Any()) { _audioSource.PlayOneShot(_footStepSounds[n], _volume / 10); }
                                _footStepSounds[n] = _footStepSounds[0];
                            }
                        }
                        _previousGrounded = true;
                    }
                    else
                    {
                        if (_previousGrounded)
                        {
                            if (_jumpSound) { _audioSource.PlayOneShot(_jumpSound, _volume / 10); }
                        }
                        _previousGrounded = false;
                    }
                }

            }
            else
            {
                if (_isGrounded)
                {
                    if (!_previousGrounded)
                    {
                        if (_landSound) { _audioSource.PlayOneShot(_landSound, _volume / 10); }
                        _nextStepTime = _headbobCycle + 0.5f;
                    }
                    else
                    {
                        if (_headbobCycle > _nextStepTime)
                        {
                            _nextStepTime = _headbobCycle + 0.5f;
                            int n = Random.Range(0, _footStepSounds.Count);
                            if (_footStepSounds.Any() && _footStepSounds[n] != null) { _audioSource.PlayOneShot(_footStepSounds[n], _volume / 10); }

                        }
                    }
                    _previousGrounded = true;
                }
                else
                {
                    if (_previousGrounded)
                    {
                        if (_jumpSound) { _audioSource.PlayOneShot(_jumpSound, _volume / 10); }
                    }
                    _previousGrounded = false;
                }
            }

        }
        #endregion

        #region  Reset Checks

        _isGrounded = false;

        #endregion
    }

    private void OnCollisionEnter(Collision collisionData)
    {
        for (int i = 0; i < collisionData.contactCount; i++)
        {
            if (collisionData.GetContact(i).point.y < transform.position.y - ((_capsule.height / 2) - _capsule.radius * 0.95f))
            {
                if (!_isGrounded)
                {
                    _isGrounded = true;
                    stairMiniHop = false;
                    
                }
            }
        }
    }
    private void OnCollisionStay(Collision collisionData)
    {
        for (int i = 0; i < collisionData.contactCount; i++)
        {
            if (collisionData.GetContact(i).point.y < transform.position.y - ((_capsule.height / 2) - _capsule.radius * 0.95f))
            {

                if (!_isGrounded)
                {
                    _isGrounded = true;
                    stairMiniHop = false;
                }
            }
        }
    }
    private void OnCollisionExit()
    {
        _isGrounded = false;
    }

    public void SwitchState(bool state)
    {
        _enableCameraMovement = state;
        _playerCanMove = state;
        _canvas.enabled = state;
    }
}