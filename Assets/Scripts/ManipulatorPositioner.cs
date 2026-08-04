#if UNITY_EDITOR
using UnityEngine;

public class ManipulatorPositioner : MonoBehaviour
{
    [SerializeField] private RectTransform canvas;
    [SerializeField] private Transform anchorTopLeft;
    [SerializeField] private Transform anchorTopRight;
    [SerializeField] private Transform anchorBottomLeft;
    [SerializeField] private Transform anchorBottomRight;
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private Vector2 spacing = new(0.05f, 0.05f);
    
    [ContextMenu("Reset Manipulator")]
    private void OnValidate()
    {
        var scale = canvas.transform.localScale;
        var resolution = canvas.sizeDelta;
        var horizontalOffset = resolution.x * scale.x / 2;
        var verticalOffset = resolution.y * scale.y / 2;
        
        anchorTopLeft.localPosition = new Vector3(-(horizontalOffset + spacing.x), verticalOffset + spacing.y, 0);
        anchorTopRight.localPosition = new Vector3(horizontalOffset + spacing.x, verticalOffset + spacing.y, 0);
        anchorBottomLeft.localPosition = new Vector3(-(horizontalOffset + spacing.x), -(verticalOffset + spacing.y), 0);
        anchorBottomRight.localPosition = new Vector3(horizontalOffset + spacing.x, -(verticalOffset + spacing.y), 0);
        
        boxCollider.size = new Vector3(resolution.x * scale.x, resolution.y * scale.y, boxCollider.size.z);
    }
}
#endif