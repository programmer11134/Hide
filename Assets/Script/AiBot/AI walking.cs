using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    
    public Transform[] points;     
    public float LookRadius = 10f;  
    public LayerMask obstacleMask;  

    
    private Transform target;      
    private NavMeshAgent agent;     
    private int currentPoint = 0; 
    private bool seesPlayer = false; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        target = PlayerManager.instance.player.transform;
        GoToPatrolPoint(); 
    }

    void Update()
    {
       
        seesPlayer = CheckIfSeesPlayer();

        
        if (seesPlayer)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

   
    void Patrol()
    {
       
        if (!agent.pathPending && agent.remainingDistance < 2f)
        {
            GoToPatrolPoint();
        }
    }

    void GoToPatrolPoint()
    {
        if (points.Length == 0) return;
        agent.destination = points[currentPoint].position;
        currentPoint = (currentPoint + 1) % points.Length; 
    }

   
    void ChasePlayer()
    {
        agent.destination = target.position;
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= agent.stoppingDistance)
        {
            LookTarget();
            Fade.instance.StartFadeAndRestart();
        }
    }
    void LookTarget()
    {
        Vector3 direction = (transform.position - target.position).normalized;
        Quaternion LookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, LookRotation, Time.deltaTime);
    }


    bool CheckIfSeesPlayer()
    {
        float distance = Vector3.Distance(transform.position, target.position);

       
        if (distance > LookRadius) return false;

       
        Vector3 direction = (target.position - transform.position).normalized;

        // Пусти луч из позиции бота, в направлении direction, максимум на расстояние distance, и проверяй столкновение только с объектами из obstacleMask
        bool hitWall = Physics.Raycast(transform.position, direction, distance, obstacleMask);

        return !hitWall; // препятствий нет = игрок
    }
}