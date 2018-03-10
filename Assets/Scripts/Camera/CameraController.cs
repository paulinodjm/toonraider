using System.Linq;
using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
  #region Unity Settings

  // desired distance between the camera and the target
  public float Distance = 2;
  public float RayWidth = 0.2f;
  public float FarSpeed = .1f;

  // pitch limitation
  public float PitchMin = -60f;
  public float PitchMax = 60f;

  // sensitivity
  public float VerticalSensitivity = 15f;
  public float HorizontalSensitivity = 15f;

  #endregion

  private LaraCroft _player;
  private Transform _target;

  public LaraCroft Player {
    get { return _player; }
    set {
      _player = value;
      _target = _player.CameraTarget;
    }
  }

  private float _pitch;
  private float _distance;

  void Awake() {
    var characters = FindObjectsOfType<LaraCroft>();
    foreach (var character in characters.Where(character => character.tag == "Player")) {
      Player = character;
      break;
    }
  }

  void OnEnable() {
    if (Player == null) return;

    Player.Camera = transform;
  }

  void OnDisable() {
    if (Player == null) return;

    if (Player.Camera == transform) Player.Camera = null;
  }

  void Start() {
    if (_target == null) return;

    _pitch = transform.localEulerAngles.x;
    _distance = Vector3.Distance(_target.position, transform.position);
  }

  void Update() {
    float yaw = transform.localEulerAngles.y + Input.GetAxis("Heading") * HorizontalSensitivity;

    _pitch += Input.GetAxis("Pitch") * VerticalSensitivity;
    _pitch = Mathf.Clamp(_pitch, PitchMin, PitchMax);

    transform.localEulerAngles = new Vector3(-_pitch, yaw, 0);
  }

  void LateUpdate() {
    if (_target == null) return;

    var startPos = _target.position;
    var endPos = startPos - transform.forward * Distance;
    var result = Vector3.zero;

    RayCast(startPos, endPos, ref result, RayWidth);
    var resultDistance = Vector3.Distance(_target.position, result);

    if (resultDistance <= _distance)    // closest collision
    {
      transform.position = result;
      _distance = resultDistance;
    } else {
      _distance = Mathf.Lerp(_distance, resultDistance, FarSpeed);
      transform.position = startPos - transform.forward * _distance;
    }
  }

  private bool RayCast(Vector3 start, Vector3 end, ref Vector3 result, float thickness) {
    var layerMask = (1 << 8) | (1 << 9);
    layerMask = ~layerMask;

    var direction = end - start;
    var distance = Vector3.Distance(start, end);

    RaycastHit hit;
    if (Physics.SphereCast(new Ray(start, direction), thickness, out hit, distance, layerMask)) {
      result = hit.point + hit.normal * RayWidth;
      return true;
    } else {
      result = end;
      return false;
    }
  }
}
