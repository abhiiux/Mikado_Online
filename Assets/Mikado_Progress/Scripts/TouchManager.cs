using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class TouchManager : MonoBehaviour
{
    [SerializeField] ShaderControls shaderControls;
    [SerializeField] StickCheck stickCheck;
    [SerializeField] TMP_Text debugUI;
    private Renderer cube;
    private LayerMask layerMask;
    private Camera mainCamera;
    private Vector2 mousePos;
    private GameObject selectedCube;
    private bool isDragging;
    private Vector3 dragOffset;

    public InputActionReference clickAction;
    public InputActionReference mousePosAction;

    private void Awake()
    {
        layerMask = LayerMask.GetMask("Stick");
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        clickAction.action.Enable();
        mousePosAction.action.Enable();

        clickAction.action.started += OnClick;
        clickAction.action.canceled += OnRelease;
        mousePosAction.action.performed += OnMouseMove;
    }

    private void OnDisable()
    {
        clickAction.action.started -= OnClick;
        clickAction.action.canceled -= OnRelease;
        mousePosAction.action.performed -= OnMouseMove;
    }

    private void OnMouseMove(InputAction.CallbackContext context)
    {
        bool canMove = stickCheck.GetStatus();
        if (canMove)
        {
            mousePos = context.ReadValue<Vector2>();
        }
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit, math.INFINITY, layerMask))
        {
            debugUI.text = "Touch detected!";

            selectedCube = hit.transform.gameObject;
            cube = selectedCube.GetComponent<Renderer>();
            shaderControls.SelectionGlow(cube);
            // cube.material.color = Color.blue;
            // cube.material.SetFloat(shaderVar, 1f);
            dragOffset = selectedCube.transform.position - mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));
            isDragging = true;
            selectedCube.GetComponent<Rigidbody>().useGravity = false;
        }
        else
        {
            debugUI.text = " ";
        }
    }

    private void OnRelease(InputAction.CallbackContext context)
    {
        if (isDragging && selectedCube != null)
        {
            // cube.material.SetFloat(shaderVar, 0f);
            stickCheck.DetectStickMove(selectedCube);
            stickCheck.OnStickCollected(selectedCube);
            selectedCube.GetComponent<Rigidbody>().useGravity = true;
            isDragging = false;
            selectedCube = null;
        }
    }

    private void Update()
    {
        if (isDragging && selectedCube != null)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));
            selectedCube.transform.position = worldPos + dragOffset;
        }
    }
}
