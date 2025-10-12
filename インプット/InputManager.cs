/*
 * 作者：肖 世鴻（シュウ　サイホン）
 * 
 * Last update: 2025/10/04
 * 
 * 
 * インプットシステムの設定
 * 
 */

using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    //入力装置
    [SerializeField] private InputActionAsset playerControls;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("More than one 'InputManager' in scene");
            return;
        }

        Instance = this;
    }

    //有効化時実行
    private void OnEnable()
    {
        PlayerActionMap.Enable();
    }

    //無効化時実行
    private void OnDisable()
    {
        PlayerActionMap.Disable();
    }

    //public InputActionAsset PlayerControls
    //{
    //    get
    //    {
    //        return this.playerControls;
    //    }
    //}

    public InputActionMap PlayerActionMap
    {
        get
        {
            return this.playerControls.FindActionMap("PlayerAction");
        }
    }
}
