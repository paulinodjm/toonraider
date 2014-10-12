using UnityEngine;
using UnityEditor;
using System;

/// <summary>
/// Custom inspector designed for ledge editing
/// </summary>
[CustomEditor(typeof(Ledge))]
public class LedgeInspector : Editor 
{
    private static readonly Vector3 DefaultSpawnPositionOffset = new Vector3(1, 0, 0);

    private Vector3 SpawnPositionOffset
    {
        get
        {
            if (Ledge.Previous != null)
                return (Ledge.transform.position - Ledge.Previous.transform.position).normalized;

            if (Ledge.Next != null)
                return (Ledge.transform.position - Ledge.Next.transform.position).normalized;

            return DefaultSpawnPositionOffset;
        }
    }

    private Ledge Ledge
    {
        get
        {
            return (Ledge)target;
        }
    }

    [MenuItem("GameObject/Create Other/Ledge")]
    public static void CreateLedgeGroup()
    {
        var group = new GameObject("Ledges");

        var firstLedge = CreateLedge(parent: group.transform);
        var secondLedge = CreateLedge(parent: group.transform, spawnPosition: DefaultSpawnPositionOffset);

        firstLedge.Next = secondLedge;
    }

    private static Ledge CreateLedge(Vector3 spawnPosition = new Vector3(), Transform parent = null)
    {
        var emptyObject = new GameObject("Ledge");
        emptyObject.transform.parent = parent;
        emptyObject.transform.position = spawnPosition;

        var ledge = emptyObject.AddComponent<Ledge>();
        return ledge;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.ObjectField("Previous", Ledge.Previous, typeof(Ledge));
        EditorGUILayout.ObjectField("Next", Ledge.Next, typeof(Ledge));

        if (Ledge.Next != null)
        {
            DrawInvertButton();
        }

        if (Ledge.Next != null && Ledge.Previous != null)
        {
            DrawBreakButton();
        }
        else
        {
            DrawAddButton();

            if (Ledge.Next != Ledge.Previous)
            {
                DrawLoopButton();
            }
        }
    }

    /// <summary>
    /// Draws and controls the add button
    /// </summary>
    private void DrawAddButton()
    {
        var clicked = GUILayout.Button("Add Ledge");
        if (!clicked) return;

        SpawnNextLedge();
    }

    /// <summary>
    /// Draws and control the loop button
    /// </summary>
    private void DrawLoopButton()
    {
        var clicked = GUILayout.Button("Make Loop");
        if (!clicked) return;

        var opposite = FindOppositeLedge();
        if (Ledge.Next == null)
        {
            Ledge.Next = opposite;
        }
        else
        {
            Ledge.Previous = opposite;
        }
    }

    /// <summary>
    /// Draw and handle the break button
    /// </summary>
    private void DrawBreakButton()
    {
        var clicked = GUILayout.Button("Break");
        if (!clicked) return;

        var spawnPosition = Ledge.transform.position + SpawnPositionOffset;
        var newLedge = CreateLedge(Ledge.transform.position, Ledge.transform.parent);
        Ledge.Previous.Next = newLedge;
        Ledge.Previous = null;

        Ledge.transform.position = spawnPosition;
    }

    private void DrawInvertButton()
    {
        var clicked = GUILayout.Button("Switch Direction");
        if (!clicked) return;

        Ledge.InvertGrabDirection();
    }

    /// <summary>
    /// Create a new ledge attached to the current ledge
    /// </summary>
    private void SpawnNextLedge()
    {
        var spawnPosition = Ledge.transform.position + SpawnPositionOffset;

        var nextLedge = CreateLedge(spawnPosition, Ledge.transform.parent);
        if (Ledge.Next == null)
        {
            Ledge.Next = nextLedge;
        }
        else
        {
            Ledge.Previous = nextLedge;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private Ledge FindOppositeLedge()
    {
        if (Ledge.Next != null && Ledge.Previous != null)
            throw new InvalidOperationException("Can only be called on an end ledge!");

        var opposite = (Ledge)null;
        if (Ledge.Next != null)
        {
            opposite = Ledge.Next;
            while (opposite.Next != null)
            {
                opposite = opposite.Next;
            }
        }
        else if (Ledge.Previous != null)
        {
            opposite = Ledge.Previous;
            while (opposite.Previous != null)
            {
                opposite = opposite.Previous;
            }
        }
        else
        {
            // nothing to do, will return null
        }

        return opposite;
    }
}
