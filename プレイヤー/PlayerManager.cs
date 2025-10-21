/*
 * 作者：肖 世鴻（シュウ　サイホン）
 * 
 * Last update: 2025/10/10
 * 
 * 
 * プレイヤーのデータ　プレイヤーの管理
 * 
 */
using System.Collections.Generic;
using System.Xml;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    //プレイヤーのアクションの種類
    public enum PlayerAction
    {
        Idling,
        Moving,
        Jumping,
        Falling,
        Attacking,
        SuperAttacking,
        Step,
    }

    //プレイヤーの状態
    public enum PlayerState
    {
        Ground,
        Air,
    }
    private PlayerAction action;    //プレイヤーの現在のアクション
    private PlayerState state;      //プレイヤーの現在の状態

    
    private InputAction playerJump;     //ジャンプボタン
    private InputAction playerMove;     //移動スティック
    private InputAction playerRotate;   //回転スティック

    private PlayerController controller;    //プレイヤーコントローラー　スクリプト
    private PlayerAnimation animator;      //プレイヤーアニメーション　スクリプト

    //ジャンプ
    private int jumpFrames = 0;
    private int jumpFrameAmount = 3;

    //歩む
    private Vector2 moveDirection;                              //移動方向

    //次のアクション
    private delegate void NextAction();
    private NextAction nextAction;

    //デバッグ
    [SerializeField] private Text debugText; 

    //初期化
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("More than one 'PlayerManager' in scene");
            return;
        }

        Instance = this;
    }

    //初期化
    private void Start()
    {
        this.controller = GetComponent<PlayerController>();
        this.animator = GetComponent<PlayerAnimation>();

        var playerActionMap = InputManager.Instance.PlayerActionMap;

        this.playerMove = playerActionMap.FindAction("Move");   //移動のボタン（スティック）
        this.playerJump = playerActionMap.FindAction("Jump");   //跳ぶのボタン
        //this.playerRotate = playerActionMap.FindAction("Look");   //回転のボタン（スティック）


        state = PlayerState.Ground;
        action = PlayerAction.Idling;
    }

    private void FixedUpdate()
    {
        if (nextAction != null)
        {
            nextAction();
            nextAction = null;
        }
    }

    //操作のインプット
    private void Update()
    {
        //プレイヤー現在の状態チェック
        debugText.text = "State: " + state
                        + "\n Action:  " + action
                        + "\n IsGrounded: " + controller.isGrounded;
                        //+ "\n numGrounded: " + controller.contactedGroundListCount;

        switch (this.state)
        {
            //状態：地面にいる
            case PlayerState.Ground:

                GroundInput();

                break;

            //状態：空中にいる
            case PlayerState.Air:

                AirInput();

                break;
        }
    }

    //-----地面にいる時のインプット処理-----
    private void GroundInput()
    {
        //プレイヤー現在のアクションチェック
        switch (this.action)
        {
            //アクション：放置中
            case PlayerAction.Idling:
                GroundInput_Idling();
                break;

            //アクション：移動中
            case PlayerAction.Moving:
                GroundInput_Moving();
                break;

            ////アクション：ジャンプ中
            //case PlayerAction.Jumping:
            //    break;

            //アクション：通常攻撃中
            case PlayerAction.Attacking:
                break;

            //アクション：必殺技中
            case PlayerAction.SuperAttacking:
                break;
        }
    }

    //地面：放置
    private void GroundInput_Idling()
    {
        //落下
        if (controller.isFalling)
        {
            nextAction = Fall;
            return;
        }

        //ジャンプ
        if (playerJump.WasPressedThisFrame())
        {
            nextAction = Jump; //ジャンプ
        }
        //移動
        else if (playerMove.IsPressed()) 
        {
            nextAction = Move;　
        }
        //放置
        else
        {
            nextAction = Idle; 
        }
    }

    //地面：移動
    private void GroundInput_Moving()
    {
        //落下
        if (controller.isFalling)
        {
            nextAction = Fall;
            return;
        }

        //ジャンプ
        if (playerJump.WasPressedThisFrame())
        {
            nextAction = Jump; //ジャンプ
        }
        //移動
        else if (playerMove.IsPressed())
        {
            nextAction = Move; 
        }
        //放置
        else
        {
            nextAction = Idle; 
        }


    }
    
    ////地面：ジャンプ
    //private void GroundInput_Jumping()
    //{

    //}



    //-----空にいる時のインプット処理-----
    private void AirInput()
    {
        //プレイヤー現在のアクションチェック
        switch (this.action)
        {
            //アクション：放置中
            case PlayerAction.Idling:
                break;

            //アクション：移動中
            case PlayerAction.Moving:
                break;

            //アクション：ジャンプ中
            case PlayerAction.Jumping:
                AirInput_Jumping();
                break;

            //アクション：通常攻撃中
            case PlayerAction.Attacking:
                break;

            //アクション：必殺技中
            case PlayerAction.SuperAttacking:
                break;

            //アクション：落下
            case PlayerAction.Falling: 
                AirInput_Falling();
                break;
        }
    }

    //空中：ジャンプ
    private void AirInput_Jumping()
    {
        //if ((playerMove.IsPressed()))
        //{
        //    moveDirection = playerMove.ReadValue<Vector2>();    //空中回転方向
        //}

        //落下
        if (controller.isFalling)
        {
            nextAction = Fall;
            return;
        }

        nextAction = Rotate; //回転

        if (jumpFrames > 0)
        {
            jumpFrames--;
            return;
        }

        if (controller.isGrounded)
        {
            nextAction = Land;
            return;
        }
    }
    
    //空中：落下
    private void AirInput_Falling()
    {
        nextAction = Rotate; //回転

        if (controller.isGrounded)
        {
            nextAction = Land;
            return;
        }
    }

    //-----アクション-----

    //回転
    private void Rotate()
    {
        //moveDirection = playerMove.ReadValue<Vector2>();
        controller.PlayerRotate(moveDirection);
    }
    
    //落下
    private void Fall()
    {
        action = PlayerAction.Falling; //現在のアクション：ジャンプ
        state = PlayerState.Air;        //現在の状態：空中

        animator.Jump();
    }

    //ジャンプ
    private void Jump()
    {
        action = PlayerAction.Jumping; //現在のアクション：ジャンプ
        state = PlayerState.Air;        //現在の状態：空中

        controller.PlayerJump();
        //animator.Jump();

        jumpFrames = jumpFrameAmount;
    }

    //移動
    private void Move()
    {
        action = PlayerAction.Moving; //現在のアクション：移動

        moveDirection = playerMove.ReadValue<Vector2>();
        controller.PlayerMove(moveDirection);
        animator.Move(moveDirection);
        Rotate();
    }


    //放置
    private void Idle()
    {
        action = PlayerAction.Idling;
        controller.PlayerStopMoving();

        moveDirection = Vector2.zero;
        animator.Move(moveDirection);
    }

    //着地
    private void Land()
    {
        animator.ResetJump();
        if (!controller.PlayerLanding()) { return; }
        state = PlayerState.Ground;
        Idle();
    }

}
