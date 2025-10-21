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

public class PlayerController_CharacterController : PlayerController
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

    private CharacterController mCharacterController;
    private Camera mCamera;

    //public PlayerAction playerState { get; private set; }

    ////跳ぶ
    private Vector3 previousHorizontalVelocity = Vector3.zero;
    [SerializeField] private float jumpSpeed = 5.0f;    //ジャンプ速度

    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float horizontalJumpFactor = 0.75f;   //ジャンプする時どのくらいの割合の水平速度を保つか

    ////歩む
    [SerializeField] private float moveSpeed = 5.0f;   //移動速度


    //回転
    [SerializeField] private float rotateSpeed = 360.0f;  //回転スピード（角度／秒）
    private Quaternion rotateTarget;



    //地面判定
    private List<Collider> contactedGroundList = new List<Collider>();            //地面にいるか？
    public int contactedGroundListCount { get { return contactedGroundList.Count; } }
    //public bool isGrounded { get { return contactedGroundListCount > 0; } }            //地面にいるか？
    public override bool isGrounded { get { return mCharacterController.isGrounded; } }            //地面にいるか？

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
        mCharacterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        //Debug.Log(isGrounded);

    }

    private void FixedUpdate()
    {
        if (!isGrounded)
        {
            //CameraController.Instance.CameraPositon = mRigid.position;
            //Debug.Log(mRigid.linearVelocity);

            //ジャンプ中の速度
            //mRigid.linearVelocity = new Vector3(previousHorizontalVelocity.x * horizontalJumpFactor
            //                                    , mRigid.linearVelocity.y
            //                                    , previousHorizontalVelocity.z * horizontalJumpFactor);

        }
    }

    //跳ぶ処理
    public override void PlayerJump()
    {
        //mRigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        //mRigid.linearVelocity = new Vector3(previousHorizontalVelocity.x, jumpSpeed, previousHorizontalVelocity.z);
    }

    //水平移動処理
    public override void PlayerMove(Vector2 _moveDir)
    {
        //入力した移動方向
        Vector2 direction = _moveDir;

        //カメラの向きを取得
        Vector3 cameraForward = Vector3.ProjectOnPlane(mCamera.transform.forward, Vector3.up).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(mCamera.transform.right, Vector3.up).normalized;

        Vector3 linearVelocity = Vector3.zero;
        
        //前方
        linearVelocity += cameraForward * direction.y * moveSpeed;
        //mRigit.AddForce(cameraForward * direction.y * moveSpeed, ForceMode.VelocityChange);
        //mRigit.MovePosition(mRigit.position + cameraForward * direction.y * moveSpeed * Time.deltaTime);


        //横
        linearVelocity += cameraRight * direction.x * moveSpeed;
        //mRigit.MovePosition(mRigit.position + cameraRight * direction.x * moveSpeed * Time.deltaTime);
        //mRigit.AddForce(cameraRight * direction.x * moveSpeed, ForceMode.VelocityChange);


        //カメラの位置の設定
        //CameraController.Instance.CameraPositon = mRigid.position;

        previousHorizontalVelocity = linearVelocity;
        mCharacterController.Move(linearVelocity * Time.deltaTime);
    }

    //キャラ回転処理
    public override void PlayerRotate(Vector2 _moveDir)
    {
        switch (rotateMode)
        {
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
        this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, rotateTarget, rotateSpeed * Time.fixedDeltaTime);
    }

    private void PlayerRotate_WithCamera()
    {
        this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation,
                        CameraController.Instance.transform.rotation,
                        rotateSpeed * Time.fixedDeltaTime);

    }
}
