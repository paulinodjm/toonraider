using UnityEngine;
using System.Collections;

public class MissingItem : Interaction {
  public MissingItem(LaraCroft user, Interactible interactible) :
      base(user, interactible) {
    // nothing to do
  }

  public override string Caption {
    get { return "Utiliser"; }
  }

  public override bool IsAvailable {
    get { return false; }
  }

  public override Vector3 UsePosition {
    get { return User.transform.position; }
  }

  public override Quaternion UseRotation {
    get { return User.transform.rotation; }
  }

  public override string Use() {
    Debug.Log("Il manque quelque chose...");
    return null;
  }
}
