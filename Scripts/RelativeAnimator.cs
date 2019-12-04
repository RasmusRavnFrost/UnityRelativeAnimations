using System;
using System.Linq;
using System.Text.RegularExpressions;
using DefaultNamespace;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace RelativeAnimations
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    [ExecuteInEditMode]
    [CanEditMultipleObjects]
    public class RelativeAnimator : MonoBehaviour
    {
        public Transform Target;

        [Tooltip("If true the animation will accomedate for changes in the targets transform after the animation has " +
                 "started. Otherwise the animation target will be a copy of the target at the start of the animation")]
        public bool DynamicTarget = true;

        public EventTriggers Triggers;

        [Header("Relative Animation Values")] 
        [Tooltip("The relative distance to the target using cylindrical coordinates " +
                 "relative to this object and the direction of the target")] 
        public RelativePositionValues RelativePosition;

        [Tooltip("The relative size compared to start-size and target-size")] 
        public RelativeSizeValues RelativeSize;
        [Tooltip("The relative rotation compared to start-rotation and target-rotation")] 
        public RelativeRotationValues RelativeRotation;
        
        private Animator _animator;
        private AnimatorStateInfo[] _startStates;
        private RelativeAnimatorBehavior _behavior;
        private bool _wasInAnimationMode; // Checks if the editor was in AnimationMode last OnDrawGizmos()
        private Vector3 _playbackResetPosition; // RelativeSize to return to when playback mode ends
        private Quaternion _playbackResetRotation; // Rotation to return to when playback mode ends
        private Vector3 _playbackResetSize; // Scale to return to when playback mode ends
        
        public bool IsAnimating {get { return _animator.enabled && _behavior.IsAcitve; }}
        
        private void Start()
        {
            _animator = GetComponent<Animator>();
            _behavior = _animator.GetBehaviour<RelativeAnimatorBehavior>();
            _startStates = new AnimatorStateInfo[_animator.layerCount];
            for (var i = 0; i < _startStates.Length; i++)
            {
                _startStates[i] = _animator.GetCurrentAnimatorStateInfo(i);
            }
            if (Application.isPlaying)
            {
                _animator.enabled = false;
                ResetData();
                if (!_behavior)
                {
                    Debug.LogWarning(typeof(RelativeAnimator).Name + " couldn't find a behavior script " +
                                     "of type RelativeAnimatorBehavior attached to any of it's animation states. Without it the " +
                                     "RelativeAnimator will have no function");
                }
                else if (_animator.GetBehaviours<RelativeAnimatorBehavior>().Length > 1)
                {
                    Debug.LogWarning(
                        "This gameobject found more than one RelativeAnimatorBehavior scripts to choose from. " +
                        "This is not recommended or supported");
                }
            }
        }

        public void PlayAnimation(int stateLayer = 0)
        {
            if (!Target)
                return;
            _animator.enabled = true;
            _animator.Play(_startStates[stateLayer].fullPathHash, stateLayer, 0);
        }

        public void CancelAnimation(bool triggerOnCancelEvent = true)
        {
            _animator.enabled = false;
            _behavior.Cancel(triggerOnCancelEvent);
            ResetData();
        }

        public void ResetData()
        {
            RelativePosition.Reset();
            RelativeSize.Reset();
            RelativeRotation.Reset();
        }

        private void OnDrawGizmosSelected()
        {
            if (Target)
            {
                var bounds = Target.WorldSpaceBounds();
                Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.4f);
                Gizmos.DrawWireSphere(Target.position, Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z));
                Gizmos.DrawLine(Target.position, transform.position);
            }
        }
        
        private void Update()
        {
            if (!Application.isPlaying)
            {
                if (AnimationMode.InAnimationMode())
                {
                    if (!_wasInAnimationMode)
                    {
                        Start();
                        _playbackResetPosition = transform.position;
                        _playbackResetRotation = transform.rotation;
                        _playbackResetSize = transform.WorldSpaceBounds().size;
                        _behavior.OnStateEnter(_animator, _startStates[0], 0);
                    }
                    else
                    {
                        _behavior.OnStateUpdate(_animator, _startStates[0], 0);
                    }
                }
                else
                {
                    ResetData();
                    if (_wasInAnimationMode)
                    {
                        transform.position = _playbackResetPosition;
                        transform.rotation = _playbackResetRotation;
                        transform.SetWorldSize(_playbackResetSize);
                        Start();
                    }
                }
            }
            _wasInAnimationMode = AnimationMode.InAnimationMode();
        }

        [CustomEditor(typeof(RelativeAnimator))]
        [CanEditMultipleObjects]
        public class RelativeAnimationDataEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                RelativeAnimator myScript = (RelativeAnimator) target;
                if (GUILayout.Button("PlayAnimation")) myScript.PlayAnimation();
                if (GUILayout.Button("Cancel")) myScript.CancelAnimation();
            }
        }
    }

    [Serializable]
    public struct RelativeRotationValues
    {
         [SerializeField] public float _blendFactor;
         [SerializeField] public Vector3 _additionalRotation;

        public void Reset()
        {
            _blendFactor = 0;
            _additionalRotation = Vector3.zero;
        }
    } 
    


    /// <summary>
    /// Datastructure containing values for the relative distance to the target using cylindrical coordinates 
    /// relative to this object and the direction of the target
    /// </summary>
    [Serializable]
    [CanEditMultipleObjects]
    public struct RelativePositionValues
    {
        public DataFormat Format;
        public float BlendFactor;
        public Vector3 Offset;     
        
        public Vector3 UpwardsDirection;
        public float RadiusOffset;
        public float RadiusAngle;
        public bool UseAbsoluteOffset;

        
        public void Reset()
        {
            if (UpwardsDirection == Vector3.zero)
                UpwardsDirection = Vector3.forward;
            Offset = Vector3.zero;
            BlendFactor = 0;
            RadiusOffset = 0;
        }
        
        public enum DataFormat
        {
            Cartesian,
            Radial
        }
        
        [CustomPropertyDrawer(typeof(RelativePositionValues))]
        [CanEditMultipleObjects]
        public class RelativePositionValuesEditor : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

                // Using BeginProperty / EndProperty on the parent property means that
                // prefab override logic works on the entire property.
                EditorGUI.BeginProperty(position, label, property);
                if (property.isExpanded)
                {
                    // Make child fields be indented
                    int indent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 1;

                    var formatProp = property.FindPropertyRelative("Format");
                    EditorGUILayout.PropertyField(
                        formatProp, new GUIContent("Data DataFormat", "You can choose the Cartesian (normal xyz " +
                                                                      "coordiante) " +
                                                                      "data formet or dataformat using the radial " +
                                                                      "distance away from the line between start " +
                                                                      "position and target"), 
                        true
                        );
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("BlendFactor"),
                        new GUIContent("Blend Factor", ""), true);
                    if (formatProp.enumValueIndex == 0)
                    {

                        EditorGUILayout.PropertyField(property.FindPropertyRelative("Offset"),
                            new GUIContent("Offset", ""), true);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("RadiusOffset"), new GUIContent("RadiusOffset", ""), true);
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("RadiusAngle"), new GUIContent("RadiusAngle", ""), true);
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("UpwardsDirection"), new GUIContent("UpwardsDirection", ""), true);
                    }
                    
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("UseAbsoluteOffset"),
                        new GUIContent("Use Absolute Offset", ""), true);

                    // Set indent back to what it was
                    EditorGUI.indentLevel = indent;
                }

                EditorGUI.EndProperty();
            }
        }
        
    }


    /// <summary>
    /// Datastructure containing values for the relative size to the target using.
    /// </summary>
    [Serializable]
    [CanEditMultipleObjects]
    public struct RelativeSizeValues
    {
        public DataFormat Format;
        [SerializeField] private Vector3 _blendFactor;
        [SerializeField] private float _blendFactorFloat;
        [SerializeField] private Vector3 _scaleFactor;
        [SerializeField] private float _scaleFactorFloat;

        public void Reset()
        {
            _blendFactor = Vector3.zero;
            _scaleFactor = Vector3.one;
            _scaleFactorFloat = 1;
            _blendFactorFloat = 0;
        }

        public Vector3 BlendFactor
        {
            get
            {
                if (Format == 0)
                    return new Vector3(_blendFactorFloat, _blendFactorFloat, _blendFactorFloat);
                return _blendFactor;
            }
        }

        public Vector3 ScaleFactor
        {

            get
            {
                if (Format == 0)
                    return new Vector3(_scaleFactorFloat, _scaleFactorFloat, _scaleFactorFloat);
                return _scaleFactor;
            }
        }

        public enum DataFormat
        {
            Simple,
            Expanded
        }

        [CustomPropertyDrawer(typeof(RelativeSizeValues))]
        [CanEditMultipleObjects]
        public class RelativeSizeValuesEditor : PropertyDrawer
        {
            /// <summary>
            /// Draws the RelativeSizeValues so that either _blendFactor and _scaleFactor is shown or _blendFactorFloat 
            /// and _ScaleFactorFloat depending on the value of DataFormat
            /// </summary>
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

                // Using BeginProperty / EndProperty on the parent property means that
                // prefab override logic works on the entire property.
                EditorGUI.BeginProperty(position, label, property);
                if (property.isExpanded)
                {
                    // Make child fields be indented
                    int indent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 1;

                    var formatProp = property.FindPropertyRelative("Format");
                    EditorGUILayout.PropertyField(
                        formatProp, new GUIContent("Data DataFormat", "You can choose expanded data formet if relative " +
                                                                  "x size is different than the relative y size"), 
                        true
                        );
                    
                    
                    string ending = formatProp.enumValueIndex == 0 ? "Float" : "";

                    EditorGUILayout.PropertyField(property.FindPropertyRelative("_blendFactor" + ending),
                        new GUIContent("Blend Ratio", "The size of this gameobject relative to the starting size and " +
                                                      "the target size. 0 means the the starting size, 1 means the " +
                                                      "target size. 0.5 means the average size between the two"), true);
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("_scaleFactor" + ending),
                        new GUIContent("Scale Factor", "A scale factor applied after the blend ration has been " +
                                                       "applied. (So for instance this can be set to 0.5 to ensure " +
                                                       "that the animation ends with half the size of the target " +
                                                       "in a smooth transition"), true);

                    // Set indent back to what it was
                    EditorGUI.indentLevel = indent;
                }

                EditorGUI.EndProperty();
            }
        }
    }

    /// <summary>
    /// This is simply a container class that will make all triggers collapsable in the unity editor.
    /// It can also make changing and storing triggers easier.
    /// </summary>
    [Serializable]
    public class EventTriggers
    {
        [Tooltip("Is triggered when a state is started succesfully")] 
        public UnityEvent OnStart = new UnityEvent();

        [Tooltip("Is triggered while state animation is running successfully")]
        public UnityEvent OnUpdate = new UnityEvent();

        [Tooltip("Is triggered once the relative animation state is completed succesfully")]
        public UnityEvent OnComplete = new UnityEvent();

        [Tooltip("Is triggered the position, size or rotation of the target changes. Is also triggered if the " +
                 "target is lost")] 
        public UnityEvent OnTargetChange = new UnityEvent();
        
//        [Tooltip("Is triggered if event is canceld (i.e. via \"Cancel\" method,")] 
//        public UnityEvent OnCancel = new UnityEvent();
    }

}