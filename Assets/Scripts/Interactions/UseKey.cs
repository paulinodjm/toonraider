using System.Text;
using UnityEngine;

public class UseKey : Interaction
{
    public UseKey(LaraCroft user, Lock interactible)
        : base(user, interactible)
    {
        var strBuilder = new StringBuilder();
        strBuilder.Append("Utiliser ");
        strBuilder.Append(interactible.KeyName);
        _caption = strBuilder.ToString();
    }

    private string _caption;

    public override string Caption
    {
        get { return _caption; }
    }

    public override bool IsAvailable
    {
        get { return true; }
    }

    public override Vector3 UsePosition
    {
        get { return ((Lock)Interactible).InteractPoint.position; }
    }

    public override Quaternion UseRotation
    {
        get { return ((Lock)Interactible).InteractPoint.rotation; }
    }

    public override string Use()
    {
        return null;
    }
}
