using UnityEngine;

public class OniGroupControl : MonoBehaviour
{

    // プレイヤー.
    public PlayerControl player = null;

    // カメラ.
    public GameObject main_camera = null;

    // シーンコントロール.
    public SceneControl scene_control = null;

    // オニのプレハブ.
    public GameObject[] OniPrefab;

    public AudioClip[] YarareLevel1;
    public AudioClip[] YarareLevel2;
    public AudioClip[] YarareLevel3;

    // グループに属する OniPrefab のインスタンス.
    public OniControl[] onis;

    // -------------------------------------------------------------------------------- //

    // コリジョンボックスの大きさ（１辺の長さ）.
    public static float collision_size = 2.0f;

    // グループに属するオニの数.
    private int oniNum;

    // 今までのオニの最大数.
    static private int oniNumMax = 0;

    // グループ全体が進む速度.
    public float runSpeed = SPEED_MIN;

    // プレイヤーとぶつかった？.
    public bool is_player_hitted = false;

    // -------------------------------------------------------------------------------- //

    // タイプ.

    public enum TYPE
    {

        NONE = -1,

        NORMAL = 0,         // ふつう.

        DECELERATE,         // 途中で減速.
        LEAVE,              // 画面右に急いで退場（プレイヤーがミスした直後）.
        NUM,
    };

    public TYPE type = TYPE.NORMAL;

    // スピード制御の情報（TYPE = DECELERATE のとき）.
    public struct Decelerate
    {

        public bool isActive;          // 減速動作中？.
        public float speedBase;            // 減速動作を開始する前のスピード.
        public float timer;
    };

    public Decelerate decelerate;

    // -------------------------------------------------------------------------------- //

    public static float SPEED_MIN = 2.0f;           // 移動スピードの最低値.
    public static float SPEED_MAX = 10.0f;          // 移動スピードの最高値.
    public static float LEAVE_SPEED = 10.0f;        // 退場する時のスピード.

    // ================================================================ //

    private void Start()
    {
        // コリジョンを表示する（デバッグ用）.
        this.gameObject.GetComponent<Renderer>().enabled = SceneControl.IS_DRAW_ONI_GROUP_COLLISION;

        this.decelerate.isActive = false;
        this.decelerate.timer = 0.0f;
    }

    private void Update()
    {
        this.SpeedControl();

        this.transform.rotation = Quaternion.identity;

        // 退場モードのときは、画面外にでたら削除する.
        // （renderer を disable にしているので、OnBecameInvisible
        // 　は使えない）.
        //
        if (this.type == TYPE.LEAVE)
        {

            // グループのおに全部が画面外だったら、グループごと削除する.

            bool isVisible = false;

            foreach (var oni in this.onis)
            {

                if (oni.GetComponent<Renderer>().isVisible)
                {

                    isVisible = true;
                    break;
                }
            }

            if (!isVisible)
            {

                Destroy(this.gameObject);
            }
        }
    }

    private void FixedUpdate()
    {
        Vector3 newPosition = this.transform.position;

        newPosition.x += this.runSpeed * Time.deltaTime;

        this.transform.position = newPosition;
    }

    // ================================================================ //

    // 走るスピードの制御.
    private void SpeedControl()
    {
        switch (this.type)
        {

            case TYPE.DECELERATE:
                {
                    // プレイヤーとの距離がこれ以下になったら、減速動作を始める.
                    //
                    const float decelerate_start = 8.0f;

                    if (this.decelerate.isActive)
                    {

                        // １．加速して逃げる.
                        // ２．プレイヤーと同じ速度でしばらく粘る.
                        // ３．やっぱだめだ～という感じで一気に減速.

                        float rate;

                        const float time0 = 0.7f;
                        const float time1 = 0.4f;
                        const float time2 = 2.0f;

                        const float speedMax = 30.0f;
                        float speedMin = OniGroupControl.SPEED_MIN;

                        float time = this.decelerate.timer;

                        do
                        {

                            // 加速する.

                            if (time < time0)
                            {

                                rate = Mathf.Clamp01(time / time0);
                                rate = (Mathf.Sin(Mathf.Lerp(-Mathf.PI / 2.0f, Mathf.PI / 2.0f, rate)) + 1.0f) / 2.0f;

                                this.runSpeed = Mathf.Lerp(this.decelerate.speedBase, speedMax, rate);

                                this.SetOniMotionSpeed(2.0f);

                                break;
                            }
                            time -= time0;

                            // プレイヤーと同じ速度まで減速.

                            if (time < time1)
                            {

                                rate = Mathf.Clamp01(time / time1);
                                rate = (Mathf.Sin(Mathf.Lerp(-Mathf.PI / 2.0f, Mathf.PI / 2.0f, rate)) + 1.0f) / 2.0f;

                                this.runSpeed = Mathf.Lerp(speedMax, PlayerControl.RUN_SPEED_MAX, rate);

                                break;
                            }
                            time -= time1;

                            // ものすごく遅い速度まで減速.

                            if (time < time2)
                            {

                                rate = Mathf.Clamp01(time / time2);
                                rate = (Mathf.Sin(Mathf.Lerp(-Mathf.PI / 2.0f, Mathf.PI / 2.0f, rate)) + 1.0f) / 2.0f;

                                this.runSpeed = Mathf.Lerp(PlayerControl.RUN_SPEED_MAX, speedMin, rate);

                                this.SetOniMotionSpeed(1.0f);

                                break;
                            }

                            this.runSpeed = speedMin;

                        } while (false);

                        this.decelerate.timer += Time.deltaTime;

                    }
                    else
                    {

                        float distance = this.transform.position.x - this.player.transform.position.x;

                        if (distance < decelerate_start)
                        {

                            this.decelerate.isActive = true;
                            this.decelerate.speedBase = this.runSpeed;
                            this.decelerate.timer = 0.0f;
                        }
                    }
                }
                break;

            case TYPE.LEAVE:
                {
                    // プレイヤーに追い付かれないよう、プレイヤーのスピードを足す.
                    this.runSpeed = LEAVE_SPEED + this.player.run_speed;
                }
                break;

        }

    }

    // オニのグループを生成する.
    public void CreateOnis(int oniNum, Vector3 basePosition)
    {
        this.oniNum = oniNum;
        oniNumMax = Mathf.Max(oniNumMax, oniNum);

        this.onis = new OniControl[this.oniNum];

        Vector3 position;

        for (int i = 0; i < this.oniNum; i++)
        {

            GameObject go = Instantiate(this.OniPrefab[i % this.OniPrefab.Length]) as GameObject;

            this.onis[i] = go.GetComponent<OniControl>();

            // オニの位置をばらつかせる.

            position = basePosition;

            if (i == 0)
            {

                // かならず一つはプレイヤーと正面からぶつかるようにしたいので、
                // ０番目はオフセットをつけない.				

            }
            else
            {

                Vector3 splatRange;

                splatRange.x = OniControl.collision_size;
                splatRange.z = OniControl.collision_size * 0.5f;

                position.x += Random.Range(0.0f, splatRange.x);
                position.z += Random.Range(-splatRange.z, splatRange.z);
            }

            position.y = 0.0f;

            this.onis[i].transform.position = position;
            this.onis[i].transform.parent = this.transform;

            this.onis[i].player = this.player;
            this.onis[i].main_camera = this.main_camera;

            this.onis[i].wave_amplitude = (i + 1) * 0.1f;
            this.onis[i].wave_angle_offset = (i + 1) * Mathf.PI / 4.0f;
        }
    }

    private static int count = 0;

    // プレイヤーの攻撃を受けたとき.
    public void OnAttackedFromPlayer()
    {

        // 倒したオニの数を増やす.
        // （↓の中で評価の計算もしているので、先に実行しておく）.
        this.scene_control.AddDefeatNum(this.oniNum);

        // オニをばらばらに吹き飛ばす.
        //
        // 円すいの表面にそうような形で、それぞれのオニを吹き飛ばす方向を決めます.
        // 評価が高いほど円すいが末広がりになって、より広い範囲にばら撒かれます.
        // プレイヤーの速度が速いと、円すいが前のめりになります.

        Vector3 blowout;                // オニが吹き飛ぶ方向（速度ベクトル）
        Vector3 blowout_up;             // ↑の、垂直成分
        Vector3 blowout_xz;             // ↑の、水平成分

        float y_angle;
        float blowout_speed;
        float blowout_speed_base;

        float forward_back_angle;       // 円すいの前後の傾き.

        float base_radius;          // 円すいの底面の半径.

        float y_angle_center;
        float y_angle_swing;            // 円弧の中心（モーションの左右によって決まる値）.

        float arc_length;               // 円弧の長さ（円周）.

        switch (this.scene_control.evaluation)
        {

            default:
            case SceneControl.EVALUATION.OKAY:
                {
                    base_radius = 0.3f;

                    blowout_speed_base = 10.0f;

                    forward_back_angle = 40.0f;

                    y_angle_center = 180.0f;
                    y_angle_swing = 10.0f;
                }
                break;

            case SceneControl.EVALUATION.GOOD:
                {
                    base_radius = 0.3f;

                    blowout_speed_base = 10.0f;

                    forward_back_angle = 0.0f;

                    y_angle_center = 0.0f;
                    y_angle_swing = 60.0f;
                }
                break;

            case SceneControl.EVALUATION.GREAT:
                {
                    base_radius = 0.5f;

                    blowout_speed_base = 15.0f;

                    forward_back_angle = -20.0f;

                    y_angle_center = 0.0f;
                    y_angle_swing = 30.0f;
                }
                break;
        }

        forward_back_angle += Random.Range(-5.0f, 5.0f);

        arc_length = (this.onis.Length - 1) * 30.0f;
        arc_length = Mathf.Min(arc_length, 120.0f);

        // プレイヤーのモーション（右切り、左切り）で、左右に飛ばす方向を変える.

        y_angle = y_angle_center;

        y_angle += -arc_length / 2.0f;

        if (this.player.attack_motion == PlayerControl.ATTACK_MOTION.RIGHT)
        {

            y_angle += y_angle_swing;

        }
        else
        {

            y_angle -= y_angle_swing;
        }

        y_angle += ((OniGroupControl.count * 7) % 11) * 3.0f;

        // グループに属するオニ全部をやられたことにする.
        foreach (OniControl oni in this.onis)
        {

            //

            blowout_up = Vector3.up;

            blowout_xz = Vector3.right * base_radius;
            blowout_xz = Quaternion.AngleAxis(y_angle, Vector3.up) * blowout_xz;

            blowout = blowout_up + blowout_xz;

            blowout.Normalize();

            // 円すいを前後に傾ける.

            blowout = Quaternion.AngleAxis(forward_back_angle, Vector3.forward) * blowout;

            // 吹き飛びの速度.

            blowout_speed = blowout_speed_base * Random.Range(0.8f, 1.2f);
            blowout *= blowout_speed;

            if (!SceneControl.IS_ONI_BLOWOUT_CAMERA_LOCAL)
            {

                // グローバルで吹き飛ぶ（カメラの動きと連動しない）ときは、
                // プレイヤーの速度を足す.
                blowout += this.player.GetComponent<Rigidbody>().velocity;
            }

            // 回転.

            Vector3 angular_velocity = Vector3.Cross(Vector3.up, blowout);

            angular_velocity.Normalize();
            angular_velocity *= 3.14f * 8.0f * blowout_speed / 15.0f * Random.Range(0.5f, 1.5f);

            //angular_velocity = Quaternion.AngleAxis(Random.Range(-30.0f, 30.0f), Vector3.up)*angular_velocity;

            //

            oni.AttackedFromPlayer(blowout, angular_velocity);

            //Debug.DrawRay(this.transform.position, blowout*2.0f, Color.white, 1000.0f);

            //

            y_angle += arc_length / (this.onis.Length - 1);

        }

        // やられ声のSEを鳴らす.
        // たくさん鳴るときれいに聞こえないので、いっこだけ.
        //
        if (this.onis.Length > 0)
        {
            AudioClip[] yarareSE = null;

            if (this.onis.Length >= 1 && this.onis.Length < 3)
            {
                yarareSE = this.YarareLevel1;
            }
            else if (this.onis.Length >= 3 && this.onis.Length < 8)
            {
                yarareSE = this.YarareLevel2;
            }
            else if (this.onis.Length >= 8)
            {
                yarareSE = this.YarareLevel3;
            }

            if (yarareSE != null)
            {
                int index = Random.Range(0, yarareSE.Length);

                this.onis[0].GetComponent<AudioSource>().clip = yarareSE[index];
                this.onis[0].GetComponent<AudioSource>().Play();
            }
        }

        OniGroupControl.count++;

        // インスタンスを削除する.
        //
        // Destroy(this) とすると　OniGroupPrefab のインスタンスではなく、スクリプト（OniGroupControl）
        // を削除してしまうので、注意すること.
        //
        Destroy(this.gameObject);

    }

    // -------------------------------------------------------------------------------- //

    // プレイヤーがぶつかったときの処理.
    public void OnPlayerHitted()
    {
        this.scene_control.result.score_max += this.scene_control.eval_rate_okay * oniNumMax * this.scene_control.eval_rate;
        this.is_player_hitted = true;
    }

    // 退場を開始する.
    public void BeginLeave()
    {
        this.GetComponent<Collider>().enabled = false;
        this.type = TYPE.LEAVE;
    }

    // おにのモーションの再生スピードをセットする.
    private void SetOniMotionSpeed(float speed)
    {
        foreach (OniControl oni in this.onis)
        {

            oni.SetMotionSpeed(speed);
        }
    }

}
