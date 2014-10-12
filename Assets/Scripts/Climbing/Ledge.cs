using UnityEngine;
using System.Collections;

public class Ledge : Interactible
{
    #region Constants

    public static readonly Color LedgeColor = new Color(.7f, .8f, 1.0f, 0.7f);
    public static readonly float LedgeHeight = .5f;
    public static readonly float LedgeThickness = .2f;

    public static readonly Color HandleColor = new Color(.7f, .8f, 1.0f, 0.9f);
    public static readonly float HandleRadius = 0.3f;

    public static readonly float CapsuleRadius = 1.0f;

    #endregion

    #region Properties

    [SerializeField()]
    private Ledge _next;

    /// <summary>
    /// Next ledge, if chained
    /// </summary>
    public Ledge Next
    {
        get { return _next; }
        set 
        {
            if (_next != null)
                _next._previous = null;

            _next = value;

            if (_next != null)
                _next._previous = this;
        }
    }

    [SerializeField()]
    private Ledge _previous;

    /// <summary>
    /// Previous ledge, if chained
    /// </summary>
    public Ledge Previous
    {
        get { return _previous; }
        set 
        {
            if (_previous != null)
                _previous._next = null;

            _previous = value;

            if (_previous != null)
                _previous._next = this;
        }
    }

    /// <summary>
    /// The grab direction
    /// </summary>
    public Vector3 GrabDirection 
    { 
        get
        {
            var direction = Vector3.Cross(transform.position - Next.transform.position, Vector3.up).normalized;
            return _isInverted ? -direction : direction;
        }
    }

    [SerializeField()]
    private bool _isInverted = false;

    #endregion

    protected void Awake()
    {
        if (Next == null) return;

        transform.LookAt(Next.transform.position);
        SpawnCapsule();
    }

    protected void OnDrawGizmos()
    {
        if (Next != null)
        {
            DrawLedge();
        }

        DrawHandle();
    }

    /// <summary>
    /// Draws the ledge shape, in editor
    /// </summary>
    private void DrawLedge()
    {
        var center = Vector3.Lerp(transform.position, Next.transform.position, .5f);
        var rotation = Quaternion.LookRotation(Next.transform.position - transform.position);
        var length = Vector3.Distance(transform.position, Next.transform.position);
        var size = new Vector3(LedgeThickness, LedgeHeight, length);

        Gizmos.color = LedgeColor;
        Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, size);

        Debug.DrawRay(center, GrabDirection);
    }
    
    /// <summary>
    /// Draws the handle shape, in editor
    /// </summary>
    private void DrawHandle()
    {
        Gizmos.color = HandleColor;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        Gizmos.DrawSphere(Vector3.zero, HandleRadius);
    }

    /// <summary>
    /// Spawn the capsule collider in game mode
    /// </summary>
    /// <returns>The capsule collider</returns>
    private CapsuleCollider SpawnCapsule()
    {
        var distance = Vector3.Distance(transform.position, Next.transform.position);

        var capsule = gameObject.AddComponent<CapsuleCollider>();
        capsule.radius = CapsuleRadius;
        capsule.height = distance;
        capsule.direction = 2;
        capsule.isTrigger = true;

        var center = new Vector3(0.0f, 0.0f, distance / 2);
        capsule.center = center;

        return capsule;
    }

    public override Interaction GetInteractionFor(LaraCroft user)
    {
        return null;
    }

    /// <summary>
    /// Invert the grab direction
    /// </summary>
    public void InvertGrabDirection()
    {
        _isInverted = !_isInverted;
    }
}
