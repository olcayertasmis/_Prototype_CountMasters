using System.Collections;
using Cinemachine;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Transform player;

    private int _numberOfStickmans, _numberOfEnemyStickmans;

    [SerializeField] private TextMeshPro counterTxt;
    [SerializeField] private GameObject stickMan;
    [Range(0f, 1f)][SerializeField] private float distanceFactor, radius;

    public bool gameState;

    private bool _moveByTouch;

    private Vector3 _mouseStartPos, _playerStartPos;

    [SerializeField] private float _playerSpeed, _roadSpeed;

    private Camera cam;

    [SerializeField] private Transform road;
    [SerializeField] private Transform enemy;

    [SerializeField] private ParticleSystem blood;
    private bool attack;
    public static PlayerManager PlayerManagerInstance;

    [SerializeField] private GameObject SecondCam;

    public bool moveTheCamera;
    private bool _finishLine;

    [SerializeField] private GameObject loseGameObj, winGameObj;

    void Start()
    {
        player = transform;

        _numberOfStickmans = transform.childCount - 1;

        counterTxt.text = _numberOfStickmans.ToString();

        cam = Camera.main;

        PlayerManagerInstance = this;

        gameState = false;
    }

    void Update()
    {
        if (attack)
        {
            SetRotation();

            if (enemy.GetChild(1).childCount > 1)
            {
                SetPosition();
            }

            else
            {
                attack = false;
                _roadSpeed = 2f;

                FormatStickMan();

                for (int i = 1; i < transform.childCount; i++)
                    transform.GetChild(i).rotation = Quaternion.identity;

                enemy.gameObject.SetActive(false);

            }

            if (transform.childCount == 1)
            {
                enemy.transform.GetChild(1).GetComponent<EnemyManager>().StopAttacking();
                gameObject.SetActive(false);

                loseGameObj.SetActive(true);
            }
        }
        else
        {
            MoveThePlayer();
        }

        if (transform.childCount == 1 && _finishLine)
        {
            gameState = false;

            winGameObj.SetActive(true);
        }

        if (gameState)
        {
            road.Translate(road.forward * Time.deltaTime * _roadSpeed);
        }

        if (moveTheCamera && transform.childCount > 1)
        {
            var cinemachineTransposer = SecondCam.GetComponent<CinemachineVirtualCamera>()
              .GetCinemachineComponent<CinemachineTransposer>();

            var cinemachineComposer = SecondCam.GetComponent<CinemachineVirtualCamera>()
                .GetCinemachineComponent<CinemachineComposer>();

            cinemachineTransposer.m_FollowOffset = new Vector3(4.5f, Mathf.Lerp(cinemachineTransposer.m_FollowOffset.y,
                transform.GetChild(1).position.y + 2f, Time.deltaTime * 1f), -5f);

            cinemachineComposer.m_TrackedObjectOffset = new Vector3(0f, Mathf.Lerp(cinemachineComposer.m_TrackedObjectOffset.y,
                4f, Time.deltaTime * 1f), 0f);
        }
    }

    private void SetRotation()
    {
        var enemyDirection = new Vector3(enemy.position.x, transform.position.y, enemy.position.z) - transform.position;

        for (int i = 1; i < transform.childCount; i++)
        {
            transform.GetChild(i).rotation = Quaternion.Slerp
            (transform.GetChild(i).rotation, Quaternion.LookRotation(enemyDirection, Vector3.up), Time.deltaTime * 3f);
        }
    }

    private void SetPosition()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var Distance = enemy.GetChild(1).GetChild(0).position - transform.GetChild(i).position;

            if (Distance.magnitude < 1.5f)
            {
                transform.GetChild(i).position = Vector3.Lerp(transform.GetChild(i).position,
                    new Vector3(enemy.GetChild(1).GetChild(0).position.x, transform.GetChild(i).position.y,
                        enemy.GetChild(1).GetChild(0).position.z), Time.deltaTime * 1f);
            }
        }
    }

    private void MoveThePlayer()
    {
        if (Input.GetMouseButtonDown(0) && gameState)
        {
            _moveByTouch = true;

            var plane = new Plane(Vector3.up, 0f);

            var ray = cam.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out var distance))
            {
                _mouseStartPos = ray.GetPoint(distance + 1f);
                _playerStartPos = transform.position;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            _moveByTouch = false;
        }

        if (_moveByTouch)
        {
            var plane = new Plane(Vector3.up, 0f);
            var ray = cam.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out var distance))
            {
                var mousePos = ray.GetPoint(distance + 1f);

                var move = mousePos - _mouseStartPos;

                var control = _playerStartPos + move;


                if (_numberOfStickmans > 50)
                    control.x = Mathf.Clamp(control.x, -0.7f, 0.7f);
                else
                    control.x = Mathf.Clamp(control.x, -1.1f, 1.1f);

                transform.position = new Vector3(Mathf.Lerp(transform.position.x, control.x, Time.deltaTime * _playerSpeed)
                    , transform.position.y, transform.position.z);
            }
        }
    }

    public void FormatStickMan()
    {
        for (int i = 1; i < player.childCount; i++)
        {
            var x = distanceFactor * Mathf.Sqrt(i) * Mathf.Cos(i * radius);
            var z = distanceFactor * Mathf.Sqrt(i) * Mathf.Sin(i * radius);

            var NewPos = new Vector3(x, -0.55f, z);

            player.transform.GetChild(i).DOLocalMove(NewPos, 0.5f).SetEase(Ease.OutBack);
        }
    }

    private void MakeStickMan(int number)
    {
        for (int i = _numberOfStickmans; i < number; i++)
        {
            Instantiate(stickMan, transform.position, quaternion.identity, transform);
        }

        _numberOfStickmans = transform.childCount - 1;
        counterTxt.text = _numberOfStickmans.ToString();
        FormatStickMan();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("gate"))
        {
            other.transform.parent.GetChild(0).GetComponent<BoxCollider>().enabled = false; // gate 1
            other.transform.parent.GetChild(1).GetComponent<BoxCollider>().enabled = false; // gate 2

            var gateManager = other.GetComponent<GateManager>();

            _numberOfStickmans = transform.childCount - 1;

            if (gateManager.multiply)
            {
                MakeStickMan(_numberOfStickmans * gateManager.randomNumber);
            }
            else
            {
                MakeStickMan(_numberOfStickmans + gateManager.randomNumber);
            }
        }

        if (other.CompareTag("enemy"))
        {
            enemy = other.transform;
            attack = true;

            _roadSpeed = 0.5f;

            other.transform.GetChild(1).GetComponent<EnemyManager>().AttackThem(transform);

            StartCoroutine(UpdateTheEnemyAndPlayerStickMansNumbers());
        }

        if (other.CompareTag("Finish"))
        {
            SecondCam.SetActive(true);
            _finishLine = true;
            Tower.towerInstance.CreateTower(transform.childCount - 1);
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    IEnumerator UpdateTheEnemyAndPlayerStickMansNumbers()
    {
        _numberOfEnemyStickmans = enemy.transform.GetChild(1).childCount - 1;
        _numberOfStickmans = transform.childCount - 1;

        while (_numberOfEnemyStickmans > 0 && _numberOfStickmans > 0)
        {
            _numberOfEnemyStickmans--;
            _numberOfStickmans--;

            enemy.transform.GetChild(1).GetComponent<EnemyManager>().counterTxt.text = _numberOfEnemyStickmans.ToString();
            counterTxt.text = _numberOfStickmans.ToString();

            yield return null;
        }

        if (_numberOfEnemyStickmans == 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).rotation = Quaternion.identity;
            }
        }
    }
}
