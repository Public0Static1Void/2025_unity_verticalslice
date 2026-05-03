using UnityEngine;

public class SC_PlayerAnimations : MonoBehaviour
{
    private SC_PlayerMovement player_move = null;
    private SC_Grab player_grab = null;
    private SC_MarksPlacer player_marks = null;
    private SC_PauseManager player_pause = null;

    public Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player_move = GetComponent<SC_PlayerMovement>();
        player_grab = GetComponent<SC_Grab>();
        player_marks = SC_MarksPlacer.Instance;
        player_pause = GetComponent<SC_PauseManager>();
    }

    void Update()
    {
        if (!player_move.can_move)
        {
            anim.SetBool("Walk", false);
            return;
        }

        anim.SetBool("walk", player_move.moving && player_move.grounded);
        anim.SetBool("jump", !player_move.grounded);
        anim.SetBool("grab", player_grab.grabbing);
        anim.SetBool("pull", player_grab.launching);
        anim.SetBool("open_marks", player_marks.placing_mark);
        anim.SetBool("activate_mark", player_marks.activate_mark);
        anim.SetBool("pause", player_pause.opened);
    }
}