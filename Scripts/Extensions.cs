using System;
using UnityEngine;

namespace RelativeAnimations
{
    /// <summary>
    /// Extenstion classes for transforms that allows you to handle the worldspace bounds of transform 
    /// (or RectTranforms)
    /// </summary>
    public static class Extensions
    {
        
        /// <summary>
        /// Returns the unrotated bounds encapsulating either the gameobjects sprite renderer or the RectTransforms
        /// converted to world space coordinates. By unrotated, it means that its the size it would have after rotating
        /// it to 0,0,0 (Quaternion.Identity).
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static Bounds WorldSpaceBounds( this Transform transform) 
        {
            RectTransform rectTransform = transform.GetComponent<RectTransform>();
            Quaternion originalRotation = transform.rotation;
            Bounds bounds;
            if (rectTransform == null)
            {
                SpriteRenderer spriteRenderer = transform.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    return new Bounds(transform.position, Vector3.zero);
                }
                transform.rotation = Quaternion.identity;
                bounds = spriteRenderer.bounds;
                transform.rotation = originalRotation;
                return bounds;
            }
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>().rootCanvas;
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                throw new NotImplementedException("RectTransform.WorldSpaceBounds() " +
                                                  "is not supperted for Canvas render mode: ScreenSpaceOverlay");
            transform.rotation = Quaternion.identity;
            
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            
            Vector3 min = corners[0];
            Vector3 max = corners[2];
            
            bounds = new Bounds((max + min)/2, max - min);
            transform.rotation = originalRotation;
            return bounds;
        }

        /// <summary>
        /// Sets the unrotated world size for the SpriteRenderer/RectTransform of the gameobject.
        /// By unrotated, it means that its the bounds would fit into the provided targetSize if it was unrotated
        /// (Quaternion.Identity). In case the object is actually rotated the bounding box will be larger.
        /// </summary>
        /// <param name="transform"></param> This transform
        /// <param name="targetSize"></param> The TargetSize of the transform
        public static void SetWorldSize(this Transform transform, Vector3 targetSize)
        {
            SpriteRenderer spriteRenderer = transform.GetComponent<SpriteRenderer>();
            RectTransform rect = transform.GetComponent<RectTransform>();
            Quaternion originalRotation = transform.rotation;
            transform.rotation = Quaternion.identity;
            
            Vector3 oldScale = transform.localScale;
            if (rect)
                rect.sizeDelta = Vector3.one;
            transform.localScale = Vector3.one;

            Bounds bounds = transform.WorldSpaceBounds();

            if (spriteRenderer)
            {
                float xNewScale = transform.localScale.x * targetSize.x / bounds.size.x;
                float yNewScale = transform.localScale.y * targetSize.y / bounds.size.y;
                float zNewScale = transform.localScale.z * targetSize.z / bounds.size.z;
                if (float.IsNaN(xNewScale) || float.IsInfinity(xNewScale)) xNewScale = oldScale.x;
                if (float.IsNaN(yNewScale) || float.IsInfinity(yNewScale)) yNewScale = oldScale.y;
                if (float.IsNaN(zNewScale) || float.IsInfinity(zNewScale)) zNewScale = oldScale.z;
                transform.localScale = new Vector3(xNewScale, yNewScale, zNewScale);
            }
            if (rect)
            {
                if (spriteRenderer)
                    bounds = transform.WorldSpaceBounds();
                float xNewSize = rect.sizeDelta.x * targetSize.x / bounds.size.x;
                float yNewSize = rect.sizeDelta.y * targetSize.y / bounds.size.y;
                rect.sizeDelta = new Vector3(xNewSize, yNewSize, 1);
            }
            transform.rotation = originalRotation;
        }
        
        /// <summary>
        /// Sets the unrotated world size for the SpriteRenderer/RectTransform of the gameobject.
        /// By unrotated, it means that its the bounds would fit into the provided targetBounds if it was unrotated
        /// (Quaternion.Identity). In case the object is actually rotated the resulting bounding box will be larger.
        /// </summary>
        /// <param name="transform"></param> This transform
        /// <param name="targetBounds"></param> The bounds defining the size of targe tsize of the transform. 
        public static void SetWorldSize(this Transform transform, Bounds targetBounds)
        {
            transform.SetWorldSize(targetBounds.size);

        }
        
    }
}