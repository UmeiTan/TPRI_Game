using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(CapsuleCollider)), RequireComponent(typeof(Rigidbody))]

public class PlayerMove : MonoBehaviour
{
    #region Variables
    
    #region Look Settings
    [SerializeField] bool lockAndHideCursor = false;
    [SerializeField] bool enableCameraMovement = true;
    [SerializeField] Camera playerCamera;
    [SerializeField] Sprite Point;
    [SerializeField] Vector3 targetAngles;
    private Vector3 followAngles;
    private Vector3 followVelocity;
    [SerializeField] float verticalRotationRange = 170;
    [SerializeField] float mouseSensitivity = 10;
    [SerializeField] float cameraSmoothing = 5f;
    [SerializeField] bool enableCameraShake = false;
    Canvas canvas;
    [SerializeField] float capsuleRadius = 0.5f;
    #endregion

    #region Movement Settings
    [SerializeField] bool playerCanMove = true;
    [SerializeField] float speed;
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float sprintSpeed = 8f;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;
    private CapsuleCollider capsule;
    [SerializeField] Rigidbody fps_Rigidbody;
    [SerializeField] float gravityMultiplier = 1.0f;
    [SerializeField] float maxStepHeight = 0.2f;
    internal bool stairMiniHop = false;
    float yVelocity = Physics.gravity.y;
    [SerializeField] bool IsGrounded;// { get; private set; }
    Vector2 inputXY;
    #endregion

    #region Headbobbing Settings
    [SerializeField] bool useHeadbob = true;
    [SerializeField] Transform head = null;
    [SerializeField] bool snapHeadjointToCapsul = true;
    [SerializeField] float headbobFrequency = 1.5f;
    [SerializeField] float headbobSwayAngle = 5f;
    [SerializeField] float headbobHeight = 3f;
    [SerializeField] float headbobSideMovement = 5f;
    [SerializeField] float jumpLandIntensity = 3f;
    private Vector3 originalLocalPosition;
    private float nextStepTime = 0.5f;
    private float headbobCycle = 0.0f;
    private float headbobFade = 0.0f;
    private float springPosition = 0.0f;
    private float springVelocity = 0.0f;
    private float springElastic = 1.1f;
    private float springDampen = 0.8f;
    private float springVelocityThreshold = 0.05f;
    private float springPositionThreshold = 0.05f;
    Vector3 previousPosition;
    Vector3 previousVelocity = Vector3.zero;
    Vector3 miscRefVel;
    bool previousGrounded;
    AudioSource audioSource;
    #endregion

    #region Audio Settings
    [SerializeField] bool enableAudioSFX = true;
    [SerializeField] float Volume = 5f;
    [SerializeField] AudioClip jumpSound = null;
    [SerializeField] AudioClip landSound = null;
    [SerializeField] List<AudioClip> footStepSounds = null;
    [SerializeField] enum FSMode { Static, Dynamic }
    [SerializeField] FSMode fsmode;
    [System.Serializable] public class DynamicFootStep
    {
        public enum matMode { physicMaterial, Material };
        public matMode materialMode;
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
        capsule = GetComponent<CapsuleCollider>();    
        fps_Rigidbody = GetComponent<Rigidbody>();
        fps_Rigidbody.interpolation = RigidbodyInterpolation.Extrapolate;
        fps_Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;   
    }

    private void Start()
    {
        #region Look Settings - Start

        canvas = new GameObject("Point").AddComponent<Canvas>();
        canvas.gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;
        canvas.transform.SetParent(playerCamera.transform);
        canvas.transform.position = Vector3.zero;

        Image crossHair = new GameObject("Point").AddComponent<Image>();
        crossHair.sprite = Point;
        crossHair.rectTransform.sizeDelta = new Vector2(3, 3);
        crossHair.transform.SetParent(canvas.transform);
        crossHair.transform.position = Vector3.zero;
            
        if (lockAndHideCursor) {
            Cursor.lockState = CursorLockMode.Locked; 
            Cursor.visible = false; 
        }

        #endregion

        #region Movement Settings - Start  
        capsule.radius = capsuleRadius;
        
        #endregion

        #region Headbobbing Settings - Start

        originalLocalPosition = snapHeadjointToCapsul ? new Vector3(head.localPosition.x, (capsule.height / 2) * head.localScale.y, head.localPosition.z) : head.localPosition;
        if (GetComponent<AudioSource>() == null) { gameObject.AddComponent<AudioSource>(); }

        previousPosition = fps_Rigidbody.position;
        audioSource = GetComponent<AudioSource>();
        #endregion

        
    }

    private void Update()
    {

        #region Look Settings - Update

        if (enableCameraMovement)
        {
            float mouseYInput = 0;
            float mouseXInput = 0;
            float camFOV = playerCamera.fieldOfView;
            
            mouseYInput = Input.GetAxis("Mouse Y");
            mouseXInput = Input.GetAxis("Mouse X");
          
            if (targetAngles.y > 180) { targetAngles.y -= 360; followAngles.y -= 360; } else if (targetAngles.y < -180) { targetAngles.y += 360; followAngles.y += 360; }
            if (targetAngles.x > 180) { targetAngles.x -= 360; followAngles.x -= 360; } else if (targetAngles.x < -180) { targetAngles.x += 360; followAngles.x += 360; }
            targetAngles.y += mouseXInput * mouseSensitivity;
            targetAngles.x += mouseYInput * mouseSensitivity;
            targetAngles.x = Mathf.Clamp(targetAngles.x, -0.5f * verticalRotationRange, 0.5f * verticalRotationRange);
            followAngles = Vector3.SmoothDamp(followAngles, targetAngles, ref followVelocity, (cameraSmoothing) / 100);

            playerCamera.transform.localRotation = Quaternion.Euler(-followAngles.x, 0, 0);
            transform.localRotation = Quaternion.Euler(0, followAngles.y, 0);
        }

        #endregion
    }

    private void FixedUpdate()
    {

        #region Movement Settings - FixedUpdate

        Vector3 MoveDirection = Vector3.zero;
        speed = Input.GetKey(sprintKey) ? sprintSpeed : walkSpeed;

        MoveDirection = (transform.forward * inputXY.y * speed + transform.right * inputXY.x * speed * 0.8f);

        #region step logic
        RaycastHit WT;
        if (maxStepHeight > 0 && Physics.Raycast(transform.position - new Vector3(0, ((capsule.height / 2) * transform.localScale.y) - 0.01f, 0), MoveDirection, out WT, capsule.radius + 0.15f, Physics.AllLayers, QueryTriggerInteraction.Ignore) && Vector3.Angle(WT.normal, Vector3.up) > 88)
        {
            RaycastHit ST;
            if (!Physics.Raycast(transform.position - new Vector3(0, ((capsule.height / 2) * transform.localScale.y) - (maxStepHeight), 0), MoveDirection, out ST, capsule.radius + 0.25f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                stairMiniHop = true;
                transform.position += new Vector3(0, maxStepHeight * 1.2f, 0);
            }
        }
        Debug.DrawRay(transform.position, MoveDirection, Color.red, 0, false);
        #endregion

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        inputXY = new Vector2(horizontalInput, verticalInput);


        if (inputXY.magnitude > 1) { inputXY.Normalize(); }

        if (playerCanMove && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)))
        {
            fps_Rigidbody.velocity = MoveDirection + (Vector3.up * yVelocity);
            
        }
        else { fps_Rigidbody.velocity = Vector3.zero; }



        fps_Rigidbody.AddForce(Physics.gravity * (gravityMultiplier - 1));

        if (Mathf.Abs(fps_Rigidbody.velocity.x) > 0.5f || Mathf.Abs(fps_Rigidbody.velocity.z) > 0.5f)
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
        if (useHeadbob == true || enableAudioSFX)
        {
            Vector3 vel = (fps_Rigidbody.position - previousPosition) / Time.deltaTime;
            Vector3 velChange = vel - previousVelocity;
            previousPosition = fps_Rigidbody.position;
            previousVelocity = vel;
            springVelocity -= velChange.y;
            springVelocity -= springPosition * springElastic;
            springVelocity *= springDampen;
            springPosition += springVelocity * Time.deltaTime;
            springPosition = Mathf.Clamp(springPosition, -0.3f, 0.3f);

            if (Mathf.Abs(springVelocity) < springVelocityThreshold && Mathf.Abs(springPosition) < springPositionThreshold) { springPosition = 0; springVelocity = 0; }
            flatVel = new Vector3(vel.x, 0.0f, vel.z).magnitude;
            strideLangthen = 1 + (flatVel * ((headbobFrequency * 2) / 10));
            headbobCycle += (flatVel / strideLangthen) * (Time.deltaTime / headbobFrequency);
            bobFactor = Mathf.Sin(headbobCycle * Mathf.PI * 2);
            bobSwayFactor = Mathf.Sin(Mathf.PI * (2 * headbobCycle + 0.5f));
            bobFactor = 1 - (bobFactor * 0.5f + 1);
            bobFactor *= bobFactor;

            yPos = 0;
            xPos = 0;
            zTilt = 0;

            if (IsGrounded)
            {
                if (new Vector3(vel.x, 0.0f, vel.z).magnitude < 0.1f) { headbobFade = Mathf.MoveTowards(headbobFade, 0.0f, 0.5f); } else { headbobFade = Mathf.MoveTowards(headbobFade, 1.0f, Time.deltaTime); }
                float speedHeightFactor = 1 + (flatVel * 0.3f);
                xPos = -(headbobSideMovement / 10) * headbobFade * bobSwayFactor;
                yPos = springPosition * (jumpLandIntensity / 10) + bobFactor * (headbobHeight / 10) * headbobFade * speedHeightFactor;
                zTilt = bobSwayFactor * (headbobSwayAngle / 10) * headbobFade;
            }
        }
        //apply headbob position
        if (useHeadbob == true)
        {
            if (fps_Rigidbody.velocity.magnitude > 0.1f)
            {
                head.localPosition = Vector3.MoveTowards(head.localPosition, snapHeadjointToCapsul ? (new Vector3(originalLocalPosition.x, (capsule.height / 2) * head.localScale.y, originalLocalPosition.z) + new Vector3(xPos, yPos, 0)) : originalLocalPosition + new Vector3(xPos, yPos, 0), 0.5f);
            }
            else
            {
                head.localPosition = Vector3.SmoothDamp(head.localPosition, snapHeadjointToCapsul ? (new Vector3(originalLocalPosition.x, (capsule.height / 2) * head.localScale.y, originalLocalPosition.z) + new Vector3(xPos, yPos, 0)) : originalLocalPosition + new Vector3(xPos, yPos, 0), ref miscRefVel, 0.15f);
            }
            head.localRotation = Quaternion.Euler(xTilt, 0, zTilt);


        }
        #endregion

        #region Dynamic Footsteps
        if (enableAudioSFX)
        {
            if (fsmode == FSMode.Dynamic)
            {
                RaycastHit hit = new RaycastHit();

                if (Physics.Raycast(transform.position, Vector3.down, out hit))
                {

                    if (dynamicFootstep.materialMode == DynamicFootStep.matMode.physicMaterial)
                    {
                        dynamicFootstep.currentClipSet = (dynamicFootstep.woodPhysMat.Any() && dynamicFootstep.woodPhysMat.Contains(hit.collider.sharedMaterial) && dynamicFootstep.woodClipSet.Any()) ? // If standing on Wood
                        dynamicFootstep.woodClipSet : ((dynamicFootstep.grassPhysMat.Any() && dynamicFootstep.grassPhysMat.Contains(hit.collider.sharedMaterial) && dynamicFootstep.grassClipSet.Any()) ? // If standing on Grass
                        dynamicFootstep.grassClipSet : ((dynamicFootstep.metalAndGlassPhysMat.Any() && dynamicFootstep.metalAndGlassPhysMat.Contains(hit.collider.sharedMaterial) && dynamicFootstep.metalAndGlassClipSet.Any()) ? // If standing on Metal/Glass
                        dynamicFootstep.metalAndGlassClipSet : ((dynamicFootstep.rockAndConcretePhysMat.Any() && dynamicFootstep.rockAndConcretePhysMat.Contains(hit.collider.sharedMaterial) && dynamicFootstep.rockAndConcreteClipSet.Any()) ? // If standing on Rock/Concrete
                        dynamicFootstep.rockAndConcreteClipSet : ((dynamicFootstep.dirtAndGravelPhysMat.Any() && dynamicFootstep.dirtAndGravelPhysMat.Contains(hit.collider.sharedMaterial) && dynamicFootstep.dirtAndGravelClipSet.Any()) ? // If standing on Dirt/Gravle
                        dynamicFootstep.dirtAndGravelClipSet : ((dynamicFootstep.mudPhysMat.Any() && dynamicFootstep.mudPhysMat.Contains(hit.collider.sharedMaterial) && dynamicFootstep.mudClipSet.Any()) ? // If standing on Mud
                        dynamicFootstep.mudClipSet : ((dynamicFootstep.customPhysMat.Any() && dynamicFootstep.customPhysMat.Contains(hit.collider.sharedMaterial) && dynamicFootstep.customClipSet.Any()) ? // If standing on the custom material 
                        dynamicFootstep.customClipSet : footStepSounds)))))); // If material is unknown, fall back
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
                        dynamicFootstep.customClipSet : footStepSounds.Any() ? footStepSounds : null)))))); // If material is unknown, fall back
                    }

                    if (IsGrounded)
                    {
                        if (!previousGrounded)
                        {
                            if (dynamicFootstep.currentClipSet.Any()) { audioSource.PlayOneShot(dynamicFootstep.currentClipSet[Random.Range(0, dynamicFootstep.currentClipSet.Count)], Volume / 10); }
                            nextStepTime = headbobCycle + 0.5f;
                        }
                        else
                        {
                            if (headbobCycle > nextStepTime)
                            {
                                nextStepTime = headbobCycle + 0.5f;
                                if (dynamicFootstep.currentClipSet.Any()) { audioSource.PlayOneShot(dynamicFootstep.currentClipSet[Random.Range(0, dynamicFootstep.currentClipSet.Count)], Volume / 10); }
                            }
                        }
                        previousGrounded = true;
                    }
                    else
                    {
                        if (previousGrounded)
                        {
                            if (dynamicFootstep.currentClipSet.Any()) { audioSource.PlayOneShot(dynamicFootstep.currentClipSet[Random.Range(0, dynamicFootstep.currentClipSet.Count)], Volume / 10); }
                        }
                        previousGrounded = false;
                    }

                }
                else
                {
                    dynamicFootstep.currentClipSet = footStepSounds;
                    if (IsGrounded)
                    {
                        if (!previousGrounded)
                        {
                            if (landSound) { audioSource.PlayOneShot(landSound, Volume / 10); }
                            nextStepTime = headbobCycle + 0.5f;
                        }
                        else
                        {
                            if (headbobCycle > nextStepTime)
                            {
                                nextStepTime = headbobCycle + 0.5f;
                                int n = Random.Range(0, footStepSounds.Count);
                                if (footStepSounds.Any()) { audioSource.PlayOneShot(footStepSounds[n], Volume / 10); }
                                footStepSounds[n] = footStepSounds[0];
                            }
                        }
                        previousGrounded = true;
                    }
                    else
                    {
                        if (previousGrounded)
                        {
                            if (jumpSound) { audioSource.PlayOneShot(jumpSound, Volume / 10); }
                        }
                        previousGrounded = false;
                    }
                }

            }
            else
            {
                if (IsGrounded)
                {
                    if (!previousGrounded)
                    {
                        if (landSound) { audioSource.PlayOneShot(landSound, Volume / 10); }
                        nextStepTime = headbobCycle + 0.5f;
                    }
                    else
                    {
                        if (headbobCycle > nextStepTime)
                        {
                            nextStepTime = headbobCycle + 0.5f;
                            int n = Random.Range(0, footStepSounds.Count);
                            if (footStepSounds.Any() && footStepSounds[n] != null) { audioSource.PlayOneShot(footStepSounds[n], Volume / 10); }

                        }
                    }
                    previousGrounded = true;
                }
                else
                {
                    if (previousGrounded)
                    {
                        if (jumpSound) { audioSource.PlayOneShot(jumpSound, Volume / 10); }
                    }
                    previousGrounded = false;
                }
            }

        }
        #endregion

        #region  Reset Checks

        IsGrounded = false;

        #endregion
    }

    private void OnCollisionEnter(Collision CollisionData)
    {
        for (int i = 0; i < CollisionData.contactCount; i++)
        {
            if (CollisionData.GetContact(i).point.y < transform.position.y - ((capsule.height / 2) - capsule.radius * 0.95f))
            {
                if (!IsGrounded)
                {
                    IsGrounded = true;
                    stairMiniHop = false;
                    
                }
            }
        }
    }
    private void OnCollisionStay(Collision CollisionData)
    {
        for (int i = 0; i < CollisionData.contactCount; i++)
        {
            if (CollisionData.GetContact(i).point.y < transform.position.y - ((capsule.height / 2) - capsule.radius * 0.95f))
            {

                if (!IsGrounded)
                {
                    IsGrounded = true;
                    stairMiniHop = false;
                }
            }
        }
    }
    private void OnCollisionExit()
    {
        IsGrounded = false;
    }

    public void SwitchState(bool state)
    {
        enableCameraMovement = state;
        playerCanMove = state;
        canvas.enabled = state;
    }

}
