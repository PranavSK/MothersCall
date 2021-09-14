using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] [Range(0.0f, 1.0f)] private float parallaxFactorX;
    [SerializeField] [Range(0.0f, 1.0f)] private float parallaxFactorY;
    
    private Transform _cameraTransform;
    private Vector3 _lastCameraPosition;

    private void Start()
    {
        Debug.Assert(Camera.main != null, "Camera.main == null");
        _cameraTransform = Camera.main.transform;
        _lastCameraPosition = _cameraTransform.position;
    }

    private void LateUpdate()
    {
        var position = _cameraTransform.position;
        var deltaPos = position - _lastCameraPosition;
        transform.position += new Vector3(deltaPos.x * parallaxFactorX, deltaPos.y * parallaxFactorY);
        _lastCameraPosition = position;
    }
}
