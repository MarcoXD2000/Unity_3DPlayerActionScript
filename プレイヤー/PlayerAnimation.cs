/*
 * 作者：肖 世鴻（シュウ　サイホン）
 * 
 * Last update: 2025/10/10
 * 
 * 
 * プレイヤーのアニメーション
 * 
 */
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    //ジャンプ
    public void Jump()
    {
        this.animator.SetTrigger("Jump");
    }

    //移動
    public void Move(Vector2 _moveDir)
    {
        animator.SetFloat("Speed", _moveDir.magnitude);
    }
}
