using UnityEngine;
using System.Collections.Generic;

public class SC_ArrowCoordiner : MonoBehaviour
{
    public List<SC_ObjectSpawner> list_arrow_spawners = new List<SC_ObjectSpawner>();

    [Header("Stats")]
    public bool sequential = true;
    public bool loop = true;
    public float delay_between_shots = 0.1f;
    public float delay_after_shots = 1;

    private float _timer = 0, _timer_shots = 0;
    [Header("Shoots")]
    public int _shoot_num = 0, _current_shots;

    public bool active = true;

    private void Start()
    {
        _shoot_num = list_arrow_spawners.Count;
    }

    void Update()
    {
        if (!active) return;

        if (_current_shots < _shoot_num && _timer == 0)
        {
            _timer_shots += Time.deltaTime;
            if (_timer_shots > delay_between_shots)
            {
                _timer_shots = 0;

                if (!sequential)
                {
                    list_arrow_spawners[_current_shots].PrepareShoot();
                    _current_shots++;
                }
                else
                {
                    list_arrow_spawners[_current_shots].PrepareShoot();
                    int next = _current_shots + 1;
                    if (next < _shoot_num)
                    {
                        list_arrow_spawners[next].PrepareShoot();
                        _current_shots = Mathf.Min(_current_shots + 2, _shoot_num);
                    }
                    else
                    {
                        _current_shots++;
                    }
                }
            }
        }
        else
        {
            _timer += Time.deltaTime;
            if (_timer > delay_after_shots)
            {
                _timer = 0;
                _timer_shots = 0; // Reset this too so first shot of next volley doesn't fire instantly
                _current_shots = 0;
            }
        }
    }

    public void SetShootersState(bool v) => active = v;
}