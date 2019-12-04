
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RelativeAnimations
{
    /// <summary>
    /// This scripts 
    /// </summary>
    [RequireComponent(typeof(RelativeAnimator))]
    public class CardDrawAnimator : MonoBehaviour
    {
        public List<Transform> Targets;
        public int currentTargetIndex = -1;
//        public Transform RevealPosition;
//        public Transform HandPosition;
//        public bool InDeck = true;
        private RelativeAnimator _relativeAnimator;
//        private float _waitTime = float.NaN;

        public RelativeAnimator RelAnimator
        {
            get
            {
                if (_relativeAnimator == null)
                    _relativeAnimator = GetComponent<RelativeAnimator>();
                return _relativeAnimator;
            }
        }

        public void PrintName()
        {
            print(gameObject.name);
        }
        
        public void SetTarget()
        {
            if (RelAnimator.Target != null && currentTargetIndex < 0)
                currentTargetIndex = Targets.FindIndex(t => t == RelAnimator.Target);
            for (int i = 0; i < Targets.Count; i++)
            {
                RelAnimator.Target = Targets[++currentTargetIndex % Targets.Count];
                if (RelAnimator.Target != null)
                    break;
            }
            

//            RelAnimator.CancelAnimation(false);
//            RelAnimator
//            if (InDeck)
//            {
//                RelAnimator.Target = RevealPosition;
//                InDeck = false;
//            }
//            else
//            RelAnimator.PlayAnimation();
//            RelAnimator.Target = HandPosition;
//            print("SetTarget() = " + RelAnimator.Target.name);
//            if (RelAnimator.Target.position )

        }

//        public void DrawFromDeck(Transform revealPosition, Transform handPosition)
//        {
////            InDeck = true;
//            RevealPosition = revealPosition;
//            HandPosition = handPosition;
//        }

//        private void Update()
//        {
//            if (!float.IsInfinity(_waitTime) || float.IsNaN(_waitTime))
//            {
//                _waitTime -= Time.deltaTime;
//            }
//
//            if (_waitTime < 0)
//            {
//                SetTarget();
//                _waitTime = float.NaN;
//            }
//        }
//
//        public void waitThenGoToHand()
//        {
//            _waitTime = 1;
//        }
    }
}