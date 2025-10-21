/*
 * 作者：肖 世鴻（シュウ　サイホン）
 * 
 * Last update: 2025/10/08
 * 
 * 
 * プレイヤーの制御
 * 
 */

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController_Rigid : PlayerController
{
    public static PlayerController Instance { get; private set; }

    //プレイヤーキャラの回転のモード
    public enum RotateMode
    {
        Independent,    //  独立
        WithCamera,     //　カメラの向き
    }

    public RotateMode rotateMode;

    [Header("Unity セットアップ")]
    public Rigidbody mRigid { get; private set; }

    private Camera mCamera;

    //public PlayerAction playerState { get; private set; }

    ////跳ぶ
    private Vector3 previousHorizontalVelocity = Vector3.zero;
    [SerializeField] private float jumpSpeed = 5.0f;    //ジャンプ速度
    
    [SerializeField]
    [Range(0.0f,1.0f)] 
    private float horizontalJumpFactor = 0.75f;   //ジャンプする時どのくらいの割合の水平速度を保つか

    ////歩む
    [SerializeField] private float moveSpeed = 5.0f;   //移動速度


    //回転
    [SerializeField] private float rotateSpeed = 360.0f;  //回転スピード（角度／秒）
    private Quaternion rotateTarget;



    //地面判定
    [SerializeField] private float slopeTolerance = 0.7f;   //斜め移動の限界

    private List<Collider> contactedGroundList = new List<Collider>();            //接触している地面
    public int contactedGroundListCount { get { return contactedGroundList.Count; } }
    //public bool isGrounded { get { return contactedGroundListCount > 0; } }            //地面にいるか？
    private float startFallingTime = 0.0f;
    private float startFallingTimeMax = 0.1f;
    public override bool isFalling { get { return startFallingTime >= startFallingTimeMax; } }  //落下中ですが？

    public override bool isGrounded { get { return contactedGroundListCount > 0; } }            //地面にいるか？

    //壁判定
    private List<Collider> contactedWallList = new List<Collider>();            //接触している壁
    private bool isWalled { get { return contactedWallList.Count > 0; } }            //壁はあるか？



    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("More than one 'PlayerController' in scene");
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (InputManager.Instance == null)
        {
            Debug.Log("No InputManager");
            return;
        }
        var playerActionMap = InputManager.Instance.PlayerActionMap;

        mCamera = Camera.main;
        mRigid = GetComponent<Rigidbody>();
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {
        if (isFalling)
        {
            //壁にぶつかった
            if (isWalled)
            {
                previousHorizontalVelocity = Vector3.zero;
                mRigid.linearVelocity = new Vector3(0.0f, -2.0f, 0.0f);
            }

            //CameraController.Instance.CameraPositon = mRigid.position;
            //Debug.Log(mRigid.linearVelocity);

            //ジャンプ中の速度
            mRigid.linearVelocity = new Vector3(previousHorizontalVelocity.x * horizontalJumpFactor
                                                , mRigid.linearVelocity.y
                                                , previousHorizontalVelocity.z * horizontalJumpFactor);

            if (contactedGroundListCount > 0) { startFallingTime = 0; }
        }
        else
        {
            //地面から離れた時間を加算
            if (contactedGroundListCount <= 0)
            {
                startFallingTime += Time.deltaTime;
            }
        }
    }

    //跳ぶ処理
    public override void PlayerJump()
    {
        startFallingTime = startFallingTimeMax + 1.0f;
        //mRigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        mRigid.linearVelocity = new Vector3(previousHorizontalVelocity.x, jumpSpeed, previousHorizontalVelocity.z);
    }

    //水平移動処理
    public override void PlayerMove(Vector2 _moveDir)
    {
        //入力した移動方向
        Vector2 direction = _moveDir;

        //カメラの向きを取得
        Vector3 cameraForward = Vector3.ProjectOnPlane(mCamera.transform.forward, Vector3.up).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(mCamera.transform.right, Vector3.up).normalized;

        //前方
        mRigid.linearVelocity += cameraForward * direction.y * moveSpeed;
        //mRigit.AddForce(cameraForward * direction.y * moveSpeed, ForceMode.VelocityChange);
        //mRigit.MovePosition(mRigit.position + cameraForward * direction.y * moveSpeed * Time.deltaTime);


        //横
        mRigid.linearVelocity += cameraRight * direction.x * moveSpeed;
        //mRigit.MovePosition(mRigit.position + cameraRight * direction.x * moveSpeed * Time.deltaTime);
        //mRigit.AddForce(cameraRight * direction.x * moveSpeed, ForceMode.VelocityChange);

        //下
        mRigid.linearVelocity += Vector3.up * -0.5f;


        //カメラの位置の設定
        //CameraController.Instance.CameraPositon = mRigid.position;

        previousHorizontalVelocity = mRigid.linearVelocity;
    }

    //キャラ回転処理
    public override void PlayerRotate(Vector2 _moveDir)
    {
        switch (rotateMode) {
            case RotateMode.Independent:
                PlayerRotate_Independent(_moveDir);
                break;

            case RotateMode.WithCamera:
                PlayerRotate_WithCamera();
                break;
         }
    }

    public void PlayerRotate_Independent(Vector2 _moveDir)
    {
        //カメラの方向を取得
        Vector3 cameraForward = Vector3.ProjectOnPlane(mCamera.transform.forward, Vector3.up).normalized;
        Quaternion forwardRotation = Quaternion.LookRotation(cameraForward);
        Vector3 eulerRotation = forwardRotation.eulerAngles;

        //回転の量を追加
        eulerRotation.y += (float)((Math.Atan2(_moveDir.x, _moveDir.y)) * 180 / Math.PI);
        Quaternion deltaRotation = Quaternion.Euler(eulerRotation);

        //回転
        rotateTarget = deltaRotation;
        mRigid.rotation = Quaternion.RotateTowards(mRigid.rotation, rotateTarget, rotateSpeed * Time.fixedDeltaTime);
    }

    private void PlayerRotate_WithCamera()
    {
        mRigid.rotation = Quaternion.RotateTowards(mRigid.rotation, 
                        CameraController.Instance.transform.rotation, 
                        rotateSpeed * Time.fixedDeltaTime);
    
    }


    //落下
    public override void PlayerFalling()
    {
        if (isWalled)
        {
        mRigid.linearVelocity = Vector3.up * -0.2f;

        }
    }

    //着地
    public override bool PlayerLanding()
    {
        //if (isGrounded) { return false; }
        mRigid.linearVelocity = Vector3.zero;
        previousHorizontalVelocity = Vector3.zero;
        return true;
    }

    //停止
    public override void PlayerStopMoving()
    {
        mRigid.linearVelocity = Vector3.up * -0.1f;
        previousHorizontalVelocity = Vector3.zero;
    }

    //接触したのは地面か？
    public bool IsGroundCheck(Collision collision)
    {
        //Debug.Log(collision.gameObject.name);
        foreach (ContactPoint contact in collision.contacts)
        {
            //ノーマルを取得
            Vector3 normal = contact.normal;

            //ドット
            float dotProduct = Vector3.Dot(normal, Vector3.up);

            //スロープの判定
            if (dotProduct > slopeTolerance)
            {
                return true;
            }
        }
        return false;
    }

    //接触したのは壁か？
    public bool IsWallCheck(Collision collision)
    {
        //Debug.Log(collision.gameObject.name);
        foreach (ContactPoint contact in collision.contacts)
        {
            //ノーマルを取得
            Vector3 normal = contact.normal;

            //ドット
            float dotProduct = Vector3.Dot(normal, Vector3.up);

            //スロープの判定
            if (dotProduct <= slopeTolerance)
            {
                return true;
            }
        }
        return false;
    }


    //地面にいるか？
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            //地面
            if (IsGroundCheck(collision))
            {
                contactedGroundList.Add(collision.collider);
                Debug.Log("Enter ground" + collision.gameObject.name);
            }

            //壁
            if (IsWallCheck(collision))
            {
                contactedWallList.Add(collision.collider);
                Debug.Log("Enter wall" + collision.gameObject.name);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {

        if (collision.gameObject.CompareTag("Platform"))
        {
            //地面
            if (contactedGroundList.Contains(collision.collider))
            {
                contactedGroundList.Remove(collision.collider);
                Debug.Log("Exit Ground" + collision.gameObject.name);

            }
            
            //壁
            if (contactedWallList.Contains(collision.collider))
            {
                contactedWallList.Remove(collision.collider);
                Debug.Log("Exit Wall" + collision.gameObject.name);
            }

        }
    }
}
