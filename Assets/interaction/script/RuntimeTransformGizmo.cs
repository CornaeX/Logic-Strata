using UnityEngine;

public class RuntimeTransformGizmo : MonoBehaviour
{
    public enum GizmoMode
    {
        Move,
        Rotate
    }

    public enum TransformAxis
    {
        None,
        X,
        Y,
        Z
    }

    public static bool IsPointerOverGizmo { get; private set; }

    [Header("Gizmo")]
    [SerializeField] private float gizmoSize = 1.5f;
    [SerializeField] private float arrowLength = 1.2f;
    [SerializeField] private float arrowRadius = 0.04f;

    [Header("Rotation")]
    [SerializeField] private float rotationRadius = 1.25f;
    [SerializeField] private float rotationThickness = 0.025f;

    [Header("Camera")]
    [SerializeField] private float gizmoDistance = 8f;

    private GameObject target;

    private GizmoMode mode =
        GizmoMode.Move;

    private TransformAxis axis =
        TransformAxis.None;

    private GameObject gizmoRoot;

    private GameObject xHandle;
    private GameObject yHandle;
    private GameObject zHandle;

    private GameObject rotationHandle;

    private bool dragging = false;

    private Vector3 dragStartWorld;
    private Vector3 targetStartPosition;

    private Quaternion targetStartRotation;

    private Camera mainCamera;

    private Material xMaterial;
    private Material yMaterial;
    private Material zMaterial;
    private Material rotateMaterial;

    private void Start()
    {
        mainCamera = Camera.main;

        CreateMaterials();

        gizmoRoot = new GameObject(
            "Runtime Transform Gizmo"
        );

        gizmoRoot.transform.SetParent(
            transform
        );

        gizmoRoot.SetActive(false);

        BuildMoveGizmo();
        BuildRotationGizmo();

        SetMoveMode();
    }

    private void Update()
    {
        if (target == null)
        {
            gizmoRoot.SetActive(false);
            return;
        }

        UpdateGizmoPosition();

        HandleGizmoInput();
    }

    // ============================================================
    // TARGET
    // ============================================================

    public void SetTarget(GameObject newTarget)
    {
        target = newTarget;

        if (target != null)
        {
            gizmoRoot.SetActive(true);
            UpdateGizmoPosition();
        }
    }

    public void ClearTarget()
    {
        target = null;

        dragging = false;

        if (gizmoRoot != null)
            gizmoRoot.SetActive(false);

        IsPointerOverGizmo = false;
    }

    // ============================================================
    // MODES
    // ============================================================

    public void SetMoveMode()
    {
        mode = GizmoMode.Move;

        axis = TransformAxis.None;

        xHandle.SetActive(true);
        yHandle.SetActive(false);
        zHandle.SetActive(true);

        rotationHandle.SetActive(false);
    }

    public void SetRotateMode()
    {
        mode = GizmoMode.Rotate;

        axis = TransformAxis.Y;

        xHandle.SetActive(false);
        yHandle.SetActive(false);
        zHandle.SetActive(false);

        rotationHandle.SetActive(true);
    }

    public void SetAxis(TransformAxis newAxis)
    {
        axis = newAxis;

        if (mode == GizmoMode.Move)
        {
            // Y movement is intentionally disabled
            if (axis == TransformAxis.Y)
            {
                axis = TransformAxis.None;
                return;
            }
        }

        if (mode == GizmoMode.Rotate)
        {
            // Rotation is only allowed around Y
            axis = TransformAxis.Y;
        }
    }

    // ============================================================
    // GIZMO POSITION
    // ============================================================

    private void UpdateGizmoPosition()
    {
        if (target == null)
            return;

        // Calculate the visual center of the entire component
        Renderer[] renderers =
            target.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            Bounds bounds =
                renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    bounds.Encapsulate(
                        renderers[i].bounds
                    );
                }
            }

            gizmoRoot.transform.position =
                bounds.center;
        }
        else
        {
            // Fallback to object pivot
            gizmoRoot.transform.position =
                target.transform.position;
        }

        // Gizmo stays aligned with world axes
        gizmoRoot.transform.rotation =
            Quaternion.identity;

        if (mainCamera != null)
        {
            float distance =
                Vector3.Distance(
                    mainCamera.transform.position,
                    gizmoRoot.transform.position
                );

            float scale =
                Mathf.Clamp(
                    distance / gizmoDistance,
                    0.6f,
                    2.5f
                );

            gizmoRoot.transform.localScale =
                Vector3.one *
                scale *
                gizmoSize;
        }
    }

    // ============================================================
    // INPUT
    // ============================================================

    private void HandleGizmoInput()
    {
        if (!Input.GetMouseButtonDown(0) &&
            !Input.GetMouseButton(0) &&
            !Input.GetMouseButtonUp(0))
            return;

        Ray ray =
            mainCamera.ScreenPointToRay(
                Input.mousePosition
            );

        if (Input.GetMouseButtonDown(0))
        {
            TryStartGizmoDrag(ray);
        }

        if (Input.GetMouseButton(0) && dragging)
        {
            ContinueGizmoDrag(ray);
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
            IsPointerOverGizmo = false;
        }
    }

    private void TryStartGizmoDrag(Ray ray)
    {
        RaycastHit hit;

        if (!Physics.Raycast(
            ray,
            out hit,
            1000f))
        {
            return;
        }

        Transform hitTransform =
            hit.collider.transform;

        if (hitTransform == xHandle.transform ||
            hitTransform.IsChildOf(xHandle.transform))
        {
            axis = TransformAxis.X;
            StartDrag(hit.point);
            return;
        }

        if (hitTransform == zHandle.transform ||
            hitTransform.IsChildOf(zHandle.transform))
        {
            axis = TransformAxis.Z;
            StartDrag(hit.point);
            return;
        }

        if (mode == GizmoMode.Rotate &&
            (hitTransform == rotationHandle.transform ||
             hitTransform.IsChildOf(rotationHandle.transform)))
        {
            axis = TransformAxis.Y;
            StartDrag(hit.point);
        }
    }

    private void StartDrag(Vector3 hitPoint)
    {
        if (target == null)
            return;

        dragging = true;

        IsPointerOverGizmo = true;

        dragStartWorld = hitPoint;

        targetStartPosition =
            target.transform.position;

        targetStartRotation =
            target.transform.rotation;
    }

    // ============================================================
    // DRAG
    // ============================================================

    private void ContinueGizmoDrag(Ray ray)
    {
        if (!dragging ||
            target == null)
            return;

        if (mode == GizmoMode.Move)
        {
            HandleMove(ray);
        }
        else
        {
            HandleRotate(ray);
        }
    }

    private void HandleMove(Ray ray)
    {
        // Movement plane is always horizontal.
        Plane groundPlane =
            new Plane(
                Vector3.up,
                targetStartPosition
            );

        if (!groundPlane.Raycast(
            ray,
            out float enter))
        {
            return;
        }

        Vector3 currentPoint =
            ray.GetPoint(enter);

        Vector3 delta =
            currentPoint -
            dragStartWorld;

        delta.y = 0f;

        Vector3 newPosition =
            targetStartPosition;

        if (axis == TransformAxis.X)
        {
            newPosition.x += delta.x;
        }
        else if (axis == TransformAxis.Z)
        {
            newPosition.z += delta.z;
        }
        else
        {
            // No axis selected = free X/Z movement
            newPosition.x += delta.x;
            newPosition.z += delta.z;
        }

        // Never change Y.
        newPosition.y =
            targetStartPosition.y;

        // Grid snapping
        if (GridManager.Instance != null)
        {
            Vector2Int gridPosition =
                GridManager.Instance.WorldToGridPosition(
                    newPosition
                );

            newPosition =
                GridManager.Instance.GridToWorldPosition(
                    gridPosition
                );

            // Preserve original Y
            newPosition.y =
                targetStartPosition.y;
        }

        target.transform.position =
            newPosition;
    }

    private void HandleRotate(Ray ray)
    {
        Plane groundPlane =
            new Plane(
                Vector3.up,
                targetStartPosition
            );

        if (!groundPlane.Raycast(
            ray,
            out float enter))
        {
            return;
        }

        Vector3 currentPoint =
            ray.GetPoint(enter);

        Vector3 startDirection =
            dragStartWorld -
            targetStartPosition;

        Vector3 currentDirection =
            currentPoint -
            targetStartPosition;

        startDirection.y = 0f;
        currentDirection.y = 0f;

        if (startDirection.sqrMagnitude < 0.001f ||
            currentDirection.sqrMagnitude < 0.001f)
            return;

        startDirection.Normalize();
        currentDirection.Normalize();

        float angle =
            Vector3.SignedAngle(
                startDirection,
                currentDirection,
                Vector3.up
            );

        // Snap rotation to 15 degrees
        angle =
            Mathf.Round(
                angle / 15f
            ) * 15f;

        target.transform.rotation =
            targetStartRotation *
            Quaternion.Euler(
                0f,
                angle,
                0f
            );
    }

    // ============================================================
    // CANCEL
    // ============================================================

    public void CancelTransform()
    {
        if (target == null)
            return;

        if (dragging)
        {
            target.transform.position =
                targetStartPosition;

            target.transform.rotation =
                targetStartRotation;

            dragging = false;
        }
    }

    // ============================================================
    // BUILD GIZMO
    // ============================================================

    private void BuildMoveGizmo()
    {
        xHandle =
            CreateAxisHandle(
                "X Axis",
                Vector3.right,
                xMaterial
            );

        yHandle =
            CreateAxisHandle(
                "Y Axis",
                Vector3.up,
                yMaterial
            );

        zHandle =
            CreateAxisHandle(
                "Z Axis",
                Vector3.forward,
                zMaterial
            );
    }

    private GameObject CreateAxisHandle(
        string name,
        Vector3 direction,
        Material material)
    {
        GameObject root =
            new GameObject(name);

        root.transform.SetParent(
            gizmoRoot.transform,
            false
        );

        // Shaft
        GameObject shaft =
            GameObject.CreatePrimitive(
                PrimitiveType.Cylinder
            );

        shaft.name = "Shaft";

        shaft.transform.SetParent(
            root.transform,
            false
        );

        shaft.transform.localPosition =
            direction *
            (arrowLength * 0.5f);

        shaft.transform.localRotation =
            Quaternion.FromToRotation(
                Vector3.up,
                direction
            );

        shaft.transform.localScale =
            new Vector3(
                arrowRadius,
                arrowLength * 0.5f,
                arrowRadius
            );

        shaft.GetComponent<Renderer>().material =
            material;

        // Arrow head
        GameObject head =
            GameObject.CreatePrimitive(
                PrimitiveType.Cylinder
            );

        head.name = "Arrow";

        head.transform.SetParent(
            root.transform,
            false
        );

        head.transform.localPosition =
            direction *
            arrowLength;

        head.transform.localRotation =
            Quaternion.FromToRotation(
                Vector3.up,
                direction
            );

        head.transform.localScale =
            new Vector3(
                arrowRadius * 2.5f,
                arrowRadius * 3f,
                arrowRadius * 2.5f
            );

        head.GetComponent<Renderer>().material =
            material;

        return root;
    }

    private void BuildRotationGizmo()
    {
        rotationHandle =
            new GameObject(
                "Y Rotation"
            );

        rotationHandle.transform.SetParent(
            gizmoRoot.transform,
            false
        );

        LineRenderer line =
            rotationHandle.AddComponent<LineRenderer>();

        line.useWorldSpace = false;

        line.loop = true;

        line.widthMultiplier =
            rotationThickness;

        line.material =
            rotateMaterial;

        int segments = 64;

        line.positionCount =
            segments;

        for (int i = 0; i < segments; i++)
        {
            float angle =
                (float)i /
                segments *
                Mathf.PI *
                2f;

            float x =
                Mathf.Cos(angle) *
                rotationRadius;

            float z =
                Mathf.Sin(angle) *
                rotationRadius;

            line.SetPosition(
                i,
                new Vector3(
                    x,
                    0f,
                    z
                )
            );
        }

        // Collider for rotation ring
        SphereCollider collider =
            rotationHandle.AddComponent<SphereCollider>();

        collider.radius =
            rotationRadius;

        collider.isTrigger = true;

        rotationHandle.SetActive(false);
    }

    // ============================================================
    // MATERIALS
    // ============================================================

    private void CreateMaterials()
    {
        Shader shader =
            Shader.Find(
                "HDRP/Lit"
            );

        if (shader == null)
        {
            shader =
                Shader.Find(
                    "Universal Render Pipeline/Lit"
                );
        }

        if (shader == null)
        {
            shader =
                Shader.Find(
                    "Standard"
                );
        }

        xMaterial =
            CreateMaterial(
                shader,
                new Color(
                    0.9f,
                    0.05f,
                    0.05f
                )
            );

        yMaterial =
            CreateMaterial(
                shader,
                new Color(
                    0.1f,
                    0.9f,
                    0.1f
                )
            );

        zMaterial =
            CreateMaterial(
                shader,
                new Color(
                    0.05f,
                    0.3f,
                    1f
                )
            );

        rotateMaterial =
            CreateMaterial(
                shader,
                new Color(
                    1f,
                    0.8f,
                    0.05f
                )
            );
    }

    private Material CreateMaterial(
        Shader shader,
        Color color)
    {
        Material material =
            new Material(shader);

        material.color =
            color;

        return material;
    }
}