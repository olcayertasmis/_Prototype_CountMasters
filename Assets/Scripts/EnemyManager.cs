using TMPro;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    public TextMeshPro counterTxt;

    [SerializeField] private GameObject stickMan;

    [Range(0f, 1f)][SerializeField] private float distanceFactor, radius;

    [SerializeField] private Transform enemy;

    private bool _attack;

    void Start()
    {
        SpawnEnemy();
    }

    private void Update()
    {
        if (_attack && transform.childCount > 1)
        {
            SetPositionAndRotationToAttack();
        }
    }

    private void SpawnEnemy()
    {
        for (int i = 0; i < Random.Range(20, 120); i++)
        {
            Instantiate(stickMan, transform.position, new Quaternion(0f, 180f, 0f, 1f), transform);
        }

        counterTxt.text = (transform.childCount - 1).ToString();

        FormatStickMan();
    }

    private void SetPositionAndRotationToAttack()
    {
        var enemyDirection = enemy.position - transform.position;

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).rotation = Quaternion.Slerp(transform.GetChild(i).rotation,
            quaternion.LookRotation(enemyDirection, Vector3.up), Time.deltaTime * 3f);

            if (enemy.childCount > 1)
            {
                var distance = enemy.GetChild(1).position - transform.GetChild(i).position;

                if (distance.magnitude < 1.5f)
                {
                    transform.GetChild(i).position = Vector3.Lerp(transform.GetChild(i).position,
                        enemy.GetChild(1).position, Time.deltaTime * 2f);
                }
            }
        }
    }

    private void FormatStickMan()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var x = distanceFactor * Mathf.Sqrt(i) * Mathf.Cos(i * radius);
            var z = distanceFactor * Mathf.Sqrt(i) * Mathf.Sin(i * radius);

            var NewPos = new Vector3(x, -0.6f, z);

            transform.transform.GetChild(i).localPosition = NewPos;
        }
    }


    public void AttackThem(Transform enemyForce)
    {
        enemy = enemyForce;
        _attack = true;

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<Animator>().SetBool("run", true);
        }
    }

    public void StopAttacking()
    {
        PlayerManager.PlayerManagerInstance.gameState = _attack = false;

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<Animator>().SetBool("run", false);
        }
    }
}
