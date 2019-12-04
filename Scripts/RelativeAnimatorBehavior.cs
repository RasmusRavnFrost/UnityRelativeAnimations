using System;
using System.Runtime.InteropServices;
using GeneralFunctions.Templates;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityScript.Scripting;

namespace RelativeAnimations
{
    public class RelativeAnimatorBehavior : StateMachineBehaviour
    {
        private bool _canceled = true;
        private RelativeAnimator _objData;

        // RelativeSize information
        private Bounds _startBounds; // The Bounds of active gameobject at the start of the animation
        private Bounds _targetBoundsPointer; // A Bounds pointer that follows the target.
        // This pointer may lag behind the target unitil the end of the animation.
        private Bounds _targetLastBounds; // The last known Bounds of the target (in case it is destroyed)
        private Bounds _targetPreviousBoundsPointer; // The targets Bounds last time the target changed position 

//        // RelativeSize information
//        private Vector3 _startPosition; // The position of active gameobject at the start of the animation
        private Vector3 _targetPositionPointer; // A position pointer that follows the target.
//        // This pointer may lag behind the target unitil the end of the animation.
//        private Vector3 _targetLastPosition; // The last known position of the target (in case it is destroyed)
//        private Vector3 _targetPreviousPositionPointer; // The targets position last time the target changed position
        private float _previousPositionChangeTime; // The relative time last time the position of the target changed    
//        private float _previousBoundsChangeTime; // The relative time last time the position of the target changed    

        // Size information
//        private Vector3 _startSize; // The size of active gameobject at the start of the animation
        private Vector3 _targetSizePointer; // A size pointer that follows the target.
//        // This pointer may lag behind the target unitil the end of the animation.
//        private Vector3 _targetLastSize; // The last known size of the target (in case it is destroyed)
//        private Vector3 _targetPreviousSizePointer; // The targets size last time the target changed size
        private float _previousSizeChangeTime; // The relative time last time the size of the target changed    
        
        private Quaternion _startRotation; // The rotation of active gameobject at the start of the animation
        private Quaternion _targetRotationPointer; // A rotation pointer that follows the target.
        // This pointer may lag behind the target unitil the end of the animation.
        private Quaternion _targetLastRotation; // The last known rotation of the target (in case it is destroyed)
        private Quaternion _targetPreviousRotationPointer; // The targets rotation last time the target changed rotation
        private float _previousRotationAngle; // The angle in degrees from the PreviousRotation pointer to the target pointer
        private float _previousRotationChangeTime; // The relative time last time the rotation of the target changed    
        private bool _targetTransformChange;
//        private bool _lastTarget;
            
        
        
        public bool IsAcitve
        {
            get
            {
                bool wasCanceled = !_canceled && (!_objData || !_objData.DynamicTarget && !_objData.Target);
                if (wasCanceled) Cancel();
                return !_canceled && _objData && (_objData.DynamicTarget || _objData.Target);
            }
//            set { Cancel(); }
        }


        public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            
            _objData = animator.GetComponent<RelativeAnimator>();
            if (!_objData.Target)
            {
                Cancel();
                return;
            }
            
            
            _canceled = false;
            if (IsAcitve)
            {
                _objData.ResetData();
//                _lastTarget = _objData.Target;
                // Update position and size values
                _previousPositionChangeTime = 0;
                _previousSizeChangeTime = 0;
                _startBounds = _objData.transform.WorldSpaceBounds();
                _targetBoundsPointer = _objData.Target.WorldSpaceBounds();
                _targetLastBounds = _targetBoundsPointer;
                _targetPreviousBoundsPointer = _targetBoundsPointer;
                _targetPositionPointer = _targetBoundsPointer.center;
                _targetSizePointer = _targetBoundsPointer.size;
                
                // Update rotation quaternions
                _previousRotationChangeTime = 0;
                _previousRotationAngle = 0;
                _startRotation = _objData.transform.rotation;
                _targetRotationPointer = _objData.Target.rotation;      
                _targetLastRotation = _targetRotationPointer;           
                _targetPreviousRotationPointer = _targetRotationPointer;
                if (!Application.isPlaying)
                    _objData.Triggers.OnStart.Invoke();
                
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            if (!IsAcitve)
                return;
//            if (Application.isPlaying && _lastTarget != _objData.Target)
//            {
//                _objData.Triggers.OnTargetChange.Invoke();
//                return;
//            }
            if (_objData.DynamicTarget)
            {
                _targetTransformChange = false;
                UpdateBoundsPointer(_objData.Target, animatorStateInfo);
                UpdateRotationPointer(_objData.Target, animatorStateInfo);
            }
            PositionValuesUpdate( _objData.RelativePosition);
            SizeValuesUpdate(_objData.RelativeSize);
            RotationValuesUpdate(_objData.RelativeRotation);
            if (Application.isPlaying)
                _objData.Triggers.OnUpdate.Invoke();
            if (_targetTransformChange && Application.isPlaying)
                _objData.Triggers.OnTargetChange.Invoke();
            if (animatorStateInfo.normalizedTime > 1)
            {
                OnStateExit(animator, animatorStateInfo, layerIndex);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!IsAcitve)
                return;
            if (_objData.DynamicTarget && _objData.Target)
            {
                _targetBoundsPointer = _objData.Target.WorldSpaceBounds();
            }
            SizeValuesUpdate(_objData.RelativeSize);
            PositionValuesUpdate(_objData.RelativePosition);
            RotationValuesUpdate(_objData.RelativeRotation);
            if (!_canceled && Application.isPlaying)
                _objData.Triggers.OnComplete.Invoke();
            _canceled = true; // Prevent cancel to trigger if state is cancled later
            _objData = null;
        }

        public void Cancel(bool triggerOnCancelEvent = true)
        {
            if (_objData)
            {
//                if (triggerOnCancelEvent && !_canceled && Application.isPlaying)
//                    _objData.Triggers.OnCancel.Invoke();
                _objData = null;
            }
            _canceled = true;
        }

        private void UpdateBoundsPointer(Transform target, AnimatorStateInfo animatorStateInfo)
        {

            float t = animatorStateInfo.normalizedTime;
            bool positionChanged = false;
            bool sizeChanged = false;
            if (target && (t - _previousPositionChangeTime > 0.05f || _previousSizeChangeTime > 0.05f))
            {
                var targetBounds = target.WorldSpaceBounds();
                positionChanged = targetBounds.center != _targetLastBounds.center;
                sizeChanged = targetBounds.size != _targetLastBounds.size;
                if (positionChanged || sizeChanged)
                {
                    _targetLastBounds = targetBounds;
                    _targetPreviousBoundsPointer = _targetBoundsPointer;
                    
                }
                if (positionChanged) _previousPositionChangeTime = t;
                if (sizeChanged) _previousSizeChangeTime = t;
                _targetTransformChange = _targetTransformChange || positionChanged || sizeChanged;
            }

            float f = (Mathf.Min(1, t) - _previousPositionChangeTime) / (1 - _previousPositionChangeTime);
            _targetPositionPointer = _targetPreviousBoundsPointer.center +
                                     f * (_targetLastBounds.center - _targetPreviousBoundsPointer.center);
            f = (Mathf.Min(1, t) - _previousSizeChangeTime) / (1 - _previousSizeChangeTime);
            _targetSizePointer = _targetPreviousBoundsPointer.size
                                 + f * (_targetLastBounds.size - _targetPreviousBoundsPointer.size);
            _targetBoundsPointer = new Bounds(_targetPositionPointer, _targetSizePointer);
            
        }
        
        private void UpdateRotationPointer(Transform target, AnimatorStateInfo animatorStateInfo)
        {
            float t = animatorStateInfo.normalizedTime;
            if (target && t - _previousRotationChangeTime > 0.05f)
            {
                var targetMoved = target.rotation != _targetLastRotation;
                if (targetMoved)
                {
                    _targetTransformChange = true;
                    _targetLastRotation = target.rotation;
                    _previousRotationChangeTime = t;
                    _targetPreviousRotationPointer = _targetRotationPointer;
                    _previousRotationAngle = Quaternion.Angle(_targetPreviousRotationPointer, _targetLastRotation);
                }
            }
            float f = (Mathf.Min(1, t) - _previousRotationChangeTime) / (1 - _previousRotationChangeTime);
            _targetRotationPointer = Quaternion.RotateTowards(_targetPreviousRotationPointer,  
                _targetLastRotation, f * _previousRotationAngle);
        }
        
        private void PositionValuesUpdate(RelativePositionValues values)
        {
            Vector3 delta = _targetPositionPointer - _startBounds.center;
            if (values.Format == RelativePositionValues.DataFormat.Cartesian)
            {
                if (values.UseAbsoluteOffset)
                    _objData.transform.position = _startBounds.center + values.BlendFactor * delta + values.Offset;
                else
                    _objData.transform.position = _startBounds.center + values.BlendFactor * delta 
                                                  + values.Offset * delta.magnitude;
            }
            else
            {
                if (Math.Abs(values.RadiusOffset) <= Mathf.Epsilon)
                {
                    _objData.transform.position = _startBounds.center + values.BlendFactor * delta;
                }
                else
                {
                    float angle = values.RadiusAngle;
                    angle = (angle % 2 - 1) * Mathf.PI;
                    Vector3 r = values.RadiusOffset * Vector3.Cross(delta, values.UpwardsDirection).normalized;
                    if (!values.UseAbsoluteOffset)
                        r = r * delta.magnitude;
                    _objData.transform.position = _startBounds.center + values.BlendFactor * delta
                                                  + Vector3.RotateTowards(r, -r, angle, float.PositiveInfinity);
                }
                
            }
        }
        
        
        private void SizeValuesUpdate(RelativeSizeValues values)
        {
            Vector3 delta = _targetSizePointer - _startBounds.size;
            delta.Scale(values.BlendFactor);
            _objData.transform.SetWorldSize( Vector3.Scale(_startBounds.size + delta, values.ScaleFactor));
        }
        
        private void RotationValuesUpdate(RelativeRotationValues values)
        {
            
            float delta = Quaternion.Angle(_startRotation, _targetRotationPointer);
            _objData.transform.rotation = Quaternion.RotateTowards(_startRotation, _targetRotationPointer, 
                values._blendFactor * delta);
            _objData.transform.localEulerAngles = _objData.transform.localEulerAngles + values._additionalRotation;
//            _objData.transform.eulerAngles = _objData.transform.eulerAngles + values._additionalRotation;
        }
    }
    
}