using UnityEngine;

public class GrabLedgeDown : Interaction {
  public const float Gap = .5F;

  /// <summary>
  /// The ledge that will be used
  /// </summary>
  public Ledge Ledge { get; private set; }

  /// <summary>
  /// The point where the ledge will be grabbed
  /// </summary>
  public Vector3 GrabPosition { get; private set; }

  public GrabLedgeDown(LaraCroft user, Interactible interactible, Vector3 grabPosition) :
      base(user, interactible) {
    GrabPosition = grabPosition;
    Ledge = (Ledge)interactible;
    CalcUsePosition();

    Debug.DrawRay(UsePosition, Ledge.GrabDirection);
  }

  public override string Caption {
    get { return "Descendre"; }
  }

  public override bool IsAvailable {
    get { return true; }
  }

  private Vector3 _usePosition;

  public override Vector3 UsePosition {
    get {
      return _usePosition;
    }
  }

  public override Quaternion UseRotation {
    get {
      return Quaternion.LookRotation(Ledge.GrabDirection);
    }
  }

  public override string Use() {
    return null;
  }

  #region Internal stuff

  /// <summary>
  /// Calculate the use position
  /// </summary>
  private void CalcUsePosition() {
    var characterController = User.GetComponent<CharacterController>();
    if (characterController == null) {
      _usePosition = User.transform.position;
    } else {
      _usePosition = GrabPosition + Ledge.GrabDirection * (characterController.radius + Gap);
    }
  }

  #endregion
}
