using UnityEngine;

namespace RelativeAnimations
{
    /// <summary>
    /// This script will cause its gameobject to follow its target, by initializing a transition 
    /// (using the attached TransitionHandler) whenever the itself or the target changes size or position.
    /// </summary>
    [RequireComponent(typeof(RelativeAnimator))]
    public class TransitionFollower : MonoBehaviour
    {
        private Bounds _lastTransitionEndBounds;
        private RelativeAnimator _relativeAnimator;
        private bool _attachedToTarget = false;

        private void Start()
        {
            _relativeAnimator = GetComponent<RelativeAnimator>();
            _relativeAnimator.Triggers.OnStart.AddListener(DetachFromTarget);
            _relativeAnimator.Triggers.OnComplete.AddListener(AttachToTarget);
        }

        
        private void AttachToTarget()
        {
            _attachedToTarget = true;
        }

        private void DetachFromTarget()
        {
            _attachedToTarget = false;
        }
        
        private void Update()
        {
            if (_relativeAnimator.Target != null && _attachedToTarget)
            {
                transform.position = _relativeAnimator.Target.position;
                transform.rotation = _relativeAnimator.Target.rotation;
                transform.SetWorldSize(_relativeAnimator.Target.WorldSpaceBounds());
            }
//                transform.SetParent(_relativeAnimator.Target, true);
//            if (_relativeAnimator.Target != null && _lastTransitionEndBounds != _relativeAnimator.Target.WorldSpaceBounds())
//            if (_relativeAnimator.Target != null && _lastTransitionEndBounds != _relativeAnimator.Target.WorldSpaceBounds())
//            {
//                _lastTransitionEndBounds = _relativeAnimator.Target.WorldSpaceBounds();
//                _relativeAnimator.PlayAnimation();
//            }
        }
    }
}