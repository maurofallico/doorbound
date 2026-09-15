using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

// Run with -executeMethod CircularRoomValidation.Run, without -quit.
// Runtime changes are never saved to the scene.
public static class CircularRoomValidation
{
    private static readonly Vector3 Center = new Vector3(13.147564f, 0.6f, -4.102249f);
    private static Vector3 previous;
    private static float travelled;
    private static bool started;

    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        SessionState.SetBool("RoomValidation", true);
        EditorApplication.EnterPlaymode();
    }

    [InitializeOnLoadMethod]
    private static void Resume()
    {
        if (SessionState.GetBool("RoomValidation", false)) EditorApplication.update += Check;
    }

    private static void Check()
    {
        if (!EditorApplication.isPlaying || Time.time < 1f) return;
        try
        {
            EnemyPatrol enemy = Object.FindFirstObjectByType<EnemyPatrol>();
            Require(enemy != null, "Enemy missing or unexpected Game Over.");
            if (!started)
            {
                started = true;
                previous = enemy.transform.position;
                Time.timeScale = 5f;
            }
            Vector3 offset = enemy.transform.position - Center;
            Require(new Vector2(offset.x / 9.9f, offset.z / 12.5f).magnitude <= 1.01f, "Enemy left room.");
            Require(Mathf.Abs(offset.y) < 0.3f, "Enemy fell or floated away from floor.");
            travelled += Vector3.Distance(previous, enemy.transform.position);
            previous = enemy.transform.position;
            if (Time.time < 25f) return;
            Require(travelled > 45f, "Patrol stalled.");
            Physics.SyncTransforms();
            for (int i = 0; i < 16; i++)
            for (int j = i + 1; j < 16; j++)
            {
                Vector3 p = Point(i), q = Point(j);
                Require(!Physics.SphereCast(p, 0.5f, (q-p).normalized, out var hit,
                    (q-p).magnitude, 64, QueryTriggerInteraction.Ignore), "Blocked chord: " + hit.collider);
            }
            PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
            player.enabled = false;
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var detect = typeof(EnemyPatrol).GetMethod("DetectPlayer", flags);
            var state = typeof(EnemyPatrol).GetField("currentState", flags);
            enemy.GetComponent<CharacterController>().enabled = false;
            player.GetComponent<CharacterController>().enabled = false;
            enemy.transform.SetPositionAndRotation(Center, Quaternion.identity);
            // A distant player behind the enemy must trigger pursuit immediately.
            player.transform.position = Center - Vector3.forward * 12.8f;
            Physics.SyncTransforms();
            detect.Invoke(enemy, null);
            Require(state.GetValue(enemy).ToString() == "Chasing", "No distant detection behind enemy at room entrance.");
            player.transform.position = Center + Vector3.forward * 14f;
            detect.Invoke(enemy, null);
            Require(state.GetValue(enemy).ToString() == "Patrolling", "Chases outside room.");
            player.transform.position = Center + Vector3.up * 4f;
            detect.Invoke(enemy, null);
            Require(state.GetValue(enemy).ToString() == "Patrolling", "Detects another floor.");
            Debug.Log("ROOM VALIDATION PASSED: patrol, confinement, wall clearance, detection and return.");
            Finish(0);
        }
        catch (Exception e) { Debug.LogException(e); Finish(1); }
    }

    private static Vector3 Point(int index)
    {
        float angle = index * Mathf.PI / 8f;
        return Center + new Vector3(Mathf.Cos(angle) * 9.9f, 0f, Mathf.Sin(angle) * 12.5f);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static void Finish(int code)
    {
        Time.timeScale = 1f;
        SessionState.SetBool("RoomValidation", false);
        EditorApplication.update -= Check;
        EditorApplication.Exit(code);
    }
}
