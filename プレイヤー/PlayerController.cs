using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public virtual bool isGrounded { get { return false; } }            //地面にいるか？
    public virtual bool isFalling { get { return false; } }            //落下中ですが？
   



    //ジャンプ
    public virtual void PlayerJump()
    {

    }

    //移動
    public virtual void PlayerMove(Vector2 _moveDir) 
    {
        
    }

    //回転
    public virtual void PlayerRotate(Vector2 _moveDir)
    {

    }

    //着地
    public virtual bool PlayerLanding()
    {
        return false;
    }

    //停止
    public virtual void PlayerStopMoving()
    {

    }

    //落下
    public virtual void PlayerFalling() { }
}
