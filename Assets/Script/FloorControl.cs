using UnityEngine;

public class FloorControl : MonoBehaviour
{

    // カメラ.
    private GameObject mainCamera = null;

    // 初期位置.
    private Vector3 initialPosition;

    // 床の幅（X方向）.
    public const float WIDTH = 10.0f * 4.0f;

    // 床モデルの数.
    public const int MODEL_NUM = 3;

    // ================================================================ //

    private void Start()
    {
        // カメラのインスタンスを探しておく.
        this.mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        this.initialPosition = this.transform.position;

        this.GetComponent<Renderer>().enabled = SceneControl.IS_DRAW_DEBUG_FLOOR_MODEL;

    }

    private void Update()
    {
        // 無限に床が繰り返すようにする.

#if true
        // 簡易的な方法.
        // 画面外に出たらプレイヤーの前方（後方）にワープする.
        // プレイヤーがワープしたときに問題あり.


        // 背景全体（すべてのモデルを並べた）の幅.
        //
        float total_width = FloorControl.WIDTH * FloorControl.MODEL_NUM;

        // 背景の位置.
        Vector3 floor_position = this.transform.position;

        // カメラの位置.
        Vector3 camera_position = this.mainCamera.transform.position;

        if (floor_position.x + total_width / 2.0f < camera_position.x)
        {

            // 前にワープ.
            floor_position.x += total_width;

            this.transform.position = floor_position;
        }

        if (camera_position.x < floor_position.x - total_width / 2.0f)
        {

            // 後ろにワープ.
            floor_position.x -= total_width;

            this.transform.position = floor_position;
        }
#else
		// プレイヤーがワープしても対応できる方法.

		// 背景全体（すべてのモデルを並べた）の幅.
		//
		float		total_width = FloorControl.WIDTH*FloorControl.MODEL_NUM;

		Vector3		camera_position = this.main_camera.transform.position;

		float		dist = camera_position.x - this.initial_position.x;

		// モデルは total_width の整数倍の位置に現れる.
		// 初期位置の距離を背景全体の幅で割って、四捨五入する.

		int			n = Mathf.RoundToInt(dist/total_width);

		Vector3		position = this.initial_position;

		position.x += n*total_width;

		this.transform.position = position;
#endif
    }
}
