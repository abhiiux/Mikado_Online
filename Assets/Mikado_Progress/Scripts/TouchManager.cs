using System.Xml;
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
    private Renderer hoveredRenderer;
    private LayerMask layerMask;
    private Camera mainCamera;
    private Vector2 mousePos;
    private GameObject selectedCube;
    private bool isDragging;
    private Vector3 dragOffset;
    private Color previousOutlineColor;
    private bool hasPreviousOutlineColor;

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
            CheckHoverOnLayerMask();
        }
    }

    private void CheckHoverOnLayerMask()
    {
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit, math.INFINITY, layerMask))
        {
            Debug.Log("yes");
            Renderer currentRenderer = hit.transform.GetComponent<Renderer>();

            if (hoveredRenderer != null && hoveredRenderer != currentRenderer)
            {
                if (!IsSelectedRenderer(hoveredRenderer))
                {
                    shaderControls.SelectionOutline(hoveredRenderer, 0f);
                }
            }

            if (currentRenderer != null)
            {
                shaderControls.SelectionOutline(currentRenderer, 1f);
                hoveredRenderer = currentRenderer;
            }
        }
        else
        {
            Debug.Log("no");
            if (hoveredRenderer != null)
            {
                if (!IsSelectedRenderer(hoveredRenderer))
                {
                    shaderControls.SelectionOutline(hoveredRenderer, 0f);
                    hoveredRenderer = null;
                }
            }
        }
    }

    private bool IsSelectedRenderer(Renderer renderer)
    {
        return isDragging && selectedCube != null && cube != null && renderer == cube;
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit, math.INFINITY, layerMask))
        {
            bool isInLayerMask = (layerMask.value & (1 << hit.transform.gameObject.layer)) != 0;
            if (!isInLayerMask)
            {
                return;
            }

            debugUI.text = "Touch detected!";

            selectedCube = hit.transform.gameObject;
            cube = selectedCube.GetComponent<Renderer>();
            hasPreviousOutlineColor = shaderControls.TryGetOutlineColor(cube, out previousOutlineColor);
            shaderControls.SelectionOutlineColor(cube, Color.blue);
            shaderControls.SelectionOutline(cube, 1f);
            // cube.material.SetFloat("_OutlineWidth", 1f);
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
            if (hasPreviousOutlineColor)
            {
                shaderControls.SelectionOutlineColor(cube, previousOutlineColor);
            }

            shaderControls.SelectionOutline(cube, 0f);
            stickCheck.DetectStickMove(selectedCube);
            stickCheck.OnStickCollected(selectedCube);
            selectedCube.GetComponent<Rigidbody>().useGravity = true;
            isDragging = false;
            hasPreviousOutlineColor = false;
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
