using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace RelativeAnimations
{
    public class ShuffleTransitionAnchors : MonoBehaviour
    {
        private static System.Random _rnd = new System.Random();
        public RectTransform Panel;

        public Transform ItemsParent;
//        public Transform AnimatedObjectParent;

        public void Shuffle()
        {
            if (Panel != null && ItemsParent != null)
            {
                for (int i = 0; i < Panel.transform.childCount; i++)
                {
                    Panel.transform.GetChild(i).SetSiblingIndex(_rnd.Next(i + 1));
                }

                foreach (RelativeAnimator child in ItemsParent.GetComponentsInChildren<RelativeAnimator>())
                {
                    child.PlayAnimation();
                }
//                LayoutRebuilder.ForceRebuildLayoutImmediate(Panel);
//                if (AnimatedObjectParent)
//                {
//                    foreach (var a in AnimatedObjectParent.GetComponentsInChildren<RelativeAnimator>())
//                    {
//                        a.PlayAnimation();
//                    }
//                    
//                }
            }
            
        }
    }
}