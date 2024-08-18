using UnityEngine;
using UnityEditor;

// Adds a button to the inspector for BulletSpawner.cs and allows reading the JSON.
#if UNITY_EDITOR
[CustomEditor(typeof(BulletSpawner))]
public class BulletSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BulletSpawner bulletSpawner = (BulletSpawner)target;
        if (GUILayout.Button("Read JSON and Write"))
        {
            bulletSpawner.ReadJSON();
        }
    }
}
#endif
