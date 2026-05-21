using System.Drawing;
using UnityEngine;
using System.Collections.Generic;

public class SC_Mark_Stone_Visualizer : MonoBehaviour
{
    private Transform _player;
    public SC_Test_Mark_Stone ob_previsualization;

    private Rigidbody _playerRb;

    private Quaternion _targetRot, _startRot;
    private float _throw_force = 0;

    [Header("Trajectory")]
    public Transform tr_hit_point;
    public LineRenderer trajectory_renderer;
    public int trajectory_points = 15;
    public float trjy_time_between_points = 0.1f;
    public LayerMask layer_trajectory_collision;
    public float force = 10;

    public List<Vector3> list_positions = new List<Vector3>(); /// Trajectory points

    void Start()
    {
        _player = SC_PlayerMovement.instance.transform;
        _playerRb = SC_PlayerMovement.instance.rb;
        _targetRot = ob_previsualization._startRot * Quaternion.Euler(ob_previsualization.rotAngle, 0f, 0f);
        _startRot = ob_previsualization._startRot;
        _throw_force = ob_previsualization.throwForce;
        trajectory_renderer.useWorldSpace = true;

        CalculateTrajectory();
    }


    void Update()
    {
        Vector3 dir = (transform.position - _player.transform.position).normalized;
        // Check if the player is looking in direction to this object
        float dot = Vector3.Dot(_player.transform.forward, dir);
        if (dot > 0.9f || dot < -0.6f) /// 1 looking 0 looking sideways -1 looking inverse
        {
            ob_previsualization.ActivatePillar();
        }
        else
        {
            ob_previsualization.DeactivatePillar();
        }
        //Debug.Log($"Condition: {dot > 9}, dot: {dot}");

        
    }

    #region Trajectory calculation
    private void CalculateTrajectory()
    {
        if (_playerRb == null || trajectory_renderer == null) return;

        // _targetRot is the final world rotation of the pillar after flipping
        // -transform.up in the original code means the tip pushes the player upward/forward
        // Reconstruct the same way SC_Test_Mark_Stone does it
        Vector3 launch_dir = transform.up + transform.forward;
        launch_dir.Normalize();

        Vector3 launch_force = launch_dir * (force / _playerRb.mass);
        Debug.DrawRay(transform.position, launch_dir * 5f, UnityEngine.Color.red);

        DrawTrajectory(transform.position, launch_force);
    }

    private void DrawTrajectory(Vector3 pos_start, Vector3 vel_start)
    {
        list_positions.Clear();

        trajectory_renderer.positionCount = trajectory_points;
        Vector3 pos_current = pos_start;
        Vector3 vel_current = vel_start;
        float time_step = trjy_time_between_points / trajectory_points; // total time spread across all points

        for (int i = 0; i < trajectory_points; i++)
        {
            list_positions.Add(pos_current);
            trajectory_renderer.SetPosition(i, pos_current);

            if (Physics.Raycast(pos_current, vel_current.normalized, out RaycastHit hit,
                vel_current.magnitude * time_step, layer_trajectory_collision))
            {
                trajectory_renderer.positionCount = i + 1;
                trajectory_renderer.SetPosition(i, hit.point);

                

                pos_current = hit.point;
                break;
            }

            vel_current += Physics.gravity * time_step;
            pos_current += vel_current * time_step;
        }

        tr_hit_point.position = pos_current;
    }
    #endregion

}
