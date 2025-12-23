using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraBehaviour : MonoBehaviour
{
    [SerializeField] private InputActionsHolder inputActionsHolder;

    [SerializeField] private Transform _targetTransform;
    [SerializeField] private float _cameraSize;
    [SerializeField] private float _lerpSpeed = 1f;
    [SerializeField] private float _targetDistance = -10f;

    [SerializeField] private float _changePosY = -5f;
    private bool lookingDown;

    [SerializeField] private Vector3 _targetPosition;

    private GameInputActions _inputActions;
    
    void Start()
    {
        
    }
    void Update()
    {
        var dis = new Vector3(0f, 0f, _targetDistance) + _targetPosition;     
        _targetPosition = _targetTransform.position;
        transform.position = Vector3.Lerp(this.transform.position, dis, _lerpSpeed * Time.deltaTime);
    }
}
