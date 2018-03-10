using UnityEngine;
using System.Collections;

[System.Serializable]
public class PlayerInput {
  public float RunThreshold = .5f;

  public bool ButJump { get; private set; }
  public bool ButAction { get; private set; }
  public bool ButWalk { get; private set; }
  public Vector3 MoveDirection { get; private set; }

  public void ProcessInput() {
    var joyMove = new Vector3(Input.GetAxis("JoyStrafe"), 0, Input.GetAxis("JoyMove"));
    var keyMove = new Vector3(Input.GetAxis("Strafe"), 0, Input.GetAxis("Move"));

    var joyWalk = false;
    if (joyMove.x != .0f || joyMove.z != .0f) {
      joyWalk = joyMove.magnitude < RunThreshold;
      MoveDirection = joyMove.normalized;
    } else {
      MoveDirection = keyMove.normalized;
    }

    ButJump = Input.GetButtonDown("Jump");
    ButAction = Input.GetButtonDown("Action");
    ButWalk = Input.GetButton("Walk") || joyWalk;
  }
}
