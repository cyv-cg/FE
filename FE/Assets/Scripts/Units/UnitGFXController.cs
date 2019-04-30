using UnityEngine;

public class UnitGFXController : MonoBehaviour
{
    public Animator Anim { get; private set; }
    private Fighter Fighter;

    private const float upThresh = 0f, sideThresh = 0.5f, downThresh = 1f;

    private Vector2 velocity;
    private Vector3 _1;

    private void Start()
    {
        Anim = GetComponent<Animator>();
        Fighter = GetComponentInParent<Fighter>();
    }

    private void Update()
    {
        Anim.SetBool("end", Fighter.TurnOver);

        if (Fighter.TurnOver || PhaseManager.Fighter == null)
        {
            Anim.SetBool("selected", false);
            return;
        }

        if (PhaseManager.Fighter == Fighter)
        {
            if (PhaseManager.TracingPath)
            {
                _1 = Fighter.transform.position;
                Anim.SetBool("moving", true);
                Locomotion();
            }
            else
            {
                Anim.SetBool("moving", false);
                Anim.SetBool("selected", true);
            }
        }
        else
            Anim.SetBool("selected", false);
    }
    private void LateUpdate()
    {
        velocity = (Fighter.transform.position - _1) / Time.deltaTime;
    }

    void Locomotion()
    {
        if (velocity.y > 0)
            Anim.SetFloat("direction", upThresh);
        else if (velocity.y < 0)
            Anim.SetFloat("direction", downThresh);

        if (velocity.x > 0)
        {
            Anim.SetFloat("direction", sideThresh);
            transform.localScale = new Vector2(-1, 1);
            transform.localPosition = new Vector2(1, 0);
        }
        else if (velocity.x < 0)
        {
            Anim.SetFloat("direction", sideThresh);
            transform.localScale = new Vector2(1, 1);
            transform.localPosition = new Vector2(0, 0);
        }
    }
}