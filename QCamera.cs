#if ENABLE_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM_PACKAGE
#define USE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;
#endif

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityTemplateProjects
{
    public class QCamera : MonoBehaviour
    {
        [Header("Object to Chase")]
        public Rigidbody cameraTarget;

        [Header("Chase Behavior")]
        public float followDistance = 20.0f;
        public float followHeight = 5.0f;

        public float chaseStrength = 5.0f;

        private Volume vol;
        private DepthOfField dof;

        private Vector3 v;

        void Start()
        {
            vol = GetComponent<Volume>();
            DepthOfField tmp;
            if(vol.profile.TryGet<DepthOfField>(out tmp))
            {
                dof = tmp;
            }
            //20ish, 145.2, 5.6
            dof.aperture.SetValue(new ClampedFloatParameter(3.0f, 0.0f, 32.0f, true));
        }

        void FixedUpdate()
        {

#if ENABLE_LEGACY_INPUT_MANAGER

            // Exit Sample  
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false; 
#endif
            }

#elif USE_INPUT_SYSTEM
            // TODO: make the new input system work
#endif

            //Vector3 dif = cameraTarget.transform.position + new Vector3(0.0f, followDistance, followHeight) - transform.position;
            Vector3 dif = 
                cameraTarget.transform.position 
                - followDistance*cameraTarget.transform.up 
                - transform.position;
            dif.y = followHeight + cameraTarget.transform.position.y - transform.position.y;
            Vector3 difN = dif.normalized;
            float r = dif.magnitude;

            v = chaseStrength * r * difN;

            dof.focusDistance.SetValue(new MinFloatParameter(Vector3.Distance(cameraTarget.transform.position, transform.position), 0f, true));

            transform.Translate(Time.deltaTime * v, Space.World);
            
            transform.LookAt(cameraTarget.transform.position);
            
        }
    }

}