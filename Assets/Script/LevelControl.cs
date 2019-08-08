using UnityEngine;

// おにの出現を制御する.
public class LevelControl
{

    // -------------------------------------------------------------------------------- //
    // プレハブ.

    public GameObject oniGroupPrefab = null;

    // -------------------------------------------------------------------------------- //

    public SceneControl sceneControl = null;
    public PlayerControl player = null;

    // オニが発生する位置
    // プレイヤーのX座標がこのラインを超えたら、プレイヤーの前方に
    // オニを発生させる.
    private float oniGenerateLine;

    // プレイヤーの appear_margin 前方の位置にオニが発生する.
    private float appearMargin = 15.0f;

    // １グループのオニの数（＝一度に出現するオニの数）.
    private int oniAppearNum = 1;

    // 連続成功のカウント.
    private int noMissCount = 0;

    // おにのタイプ.
    public enum GROUP_TYPE
    {

        NONE = -1,

        SLOW = 0,           // おそい.
        DECELERATE,         // 途中で減速.
        PASSING,            // ふたつのグループで追い抜き.
        RAPID,              // 超短い間隔で.

        NORMAL,             // ふつう.

        NUM,
    };

    public GROUP_TYPE groupType = GROUP_TYPE.NORMAL;
    public GROUP_TYPE groupTypeNext = GROUP_TYPE.NORMAL;

    private bool canDispatch = false;

    // ランダム制御（通常のゲーム）.
    public bool isRandom = true;

    // 次のグループの発生位置（ノーマルのとき　プレイヤーの位置からのオフセット）.
    private float normalGenrateLineDist = 50.0f;

    // 次のグループのスピード（ノーマルのとき）.
    private float nextSpeed = OniGroupControl.SPEED_MIN * 5.0f;

    // 残りのノーマル発生回数.
    private int normalCount = 5;

    // 残りのイベント発生回数.
    private int eventCount = 1;

    // 発生中のイベント.
    private GROUP_TYPE eventType = GROUP_TYPE.NONE;

    // -------------------------------------------------------------------------------- //

    public const float INTERVAL_MIN = 20.0f;            // おにが出現する間隔の最短値.
    public const float INTERVAL_MAX = 50.0f;            // おにが出現する間隔の最長値.

    // -------------------------------------------------------------------------------- //

    public void Create()
    {
        // ゲーム開始直後に最初のオニが発生するよう、
        // 発生位置をプレイヤーの後方に初期化しておく.
        this.oniGenerateLine = this.player.transform.position.x - 1.0f;
    }

    public void OnPlayerMissed()
    {
        // 一度に出現するオニの数をリセットする.
        this.oniAppearNum = 1;

        this.noMissCount = 0;
    }

    public void OniAppearControl()
    {
#if false
		for(int i = 0;i < 4;i++) {

			if(Input.GetKeyDown((KeyCode)(KeyCode.Alpha1 + i))) {

				this.group_type_next = (GROUP_TYPE)i;

				this.is_random = false;
			}
		}
		if(Input.GetKeyDown(KeyCode.Alpha0)) {

			this.is_random = !this.is_random;
		}
#endif

        // プレイヤーが一定距離進むごとに、オニのグループを発生させる.

        if (this.canDispatch)
        {

            // つぎのグループの発生準備ができている.

        }
        else
        {

            // つぎのグループの発生準備ができていない.

            if (this.IsExclusiveGroup())
            {

                // 特別パターンのときは、画面からおにがいなくなるのを待つ.

                if (GameObject.FindGameObjectsWithTag("OniGroup").Length == 0)
                {

                    this.canDispatch = true;
                }

            }
            else
            {

                // 通常パターンのときは、すぐに出せる.
                this.canDispatch = true;
            }

            if (this.canDispatch)
            {

                // 出現させる準備ができたら、プレイヤーの現在位置から出現位置を計算する.

                if (this.groupTypeNext == GROUP_TYPE.NORMAL)
                {

                    this.oniGenerateLine = this.player.transform.position.x + this.normalGenrateLineDist;

                }
                else if (this.groupTypeNext == GROUP_TYPE.SLOW)
                {

                    this.oniGenerateLine = this.player.transform.position.x + 50.0f;

                }
                else
                {

                    this.oniGenerateLine = this.player.transform.position.x + 10.0f;
                }
            }
        }

        // プレイヤーが一定距離進んだら、次のグループを発生させる.

        do
        {

            if (this.sceneControl.oni_group_num >= this.sceneControl.oni_group_appear_max)
            {

                break;
            }
            if (!this.canDispatch)
            {

                break;
            }
            if (this.player.transform.position.x <= this.oniGenerateLine)
            {

                break;
            }

            //

            this.groupType = this.groupTypeNext;

            switch (this.groupType)
            {

                case GROUP_TYPE.SLOW:
                    {
                        this.DispatchSlow();
                    }
                    break;

                case GROUP_TYPE.DECELERATE:
                    {
                        this.DispatchDecelerate();
                    }
                    break;

                case GROUP_TYPE.PASSING:
                    {
                        this.DispatchPassing();
                    }
                    break;

                case GROUP_TYPE.RAPID:
                    {
                        this.DispatchRapid();
                    }
                    break;

                case GROUP_TYPE.NORMAL:
                    {
                        this.DispatchNormal(this.nextSpeed);
                    }
                    break;
            }

            // 次回出現するグループのオニの数を更新しておく
            // （だんだん増える）.
            this.oniAppearNum++;
            this.oniAppearNum = Mathf.Min(this.oniAppearNum, SceneControl.ONI_APPEAR_NUM_MAX);

            this.canDispatch = false;

            this.noMissCount++;

            this.sceneControl.oni_group_num++;

            if (this.isRandom)
            {

                // 次に出現するグループを選ぶ.
                this.selectNextGroupType();
            }

        } while (false);
    }

    // 画面にひとつしかだせないグループ？.
    public bool IsExclusiveGroup()
    {
        bool ret;

        do
        {

            ret = true;

            if (this.groupType == GROUP_TYPE.PASSING || this.groupTypeNext == GROUP_TYPE.PASSING)
            {

                break;
            }
            if (this.groupType == GROUP_TYPE.DECELERATE || this.groupTypeNext == GROUP_TYPE.DECELERATE)
            {

                break;
            }
            if (this.groupType == GROUP_TYPE.SLOW || this.groupTypeNext == GROUP_TYPE.SLOW)
            {

                break;
            }

            ret = false;

        } while (false);

        return (ret);
    }

    public void selectNextGroupType()
    {

        // ノーマルとイベントの遷移チェック.

        if (this.eventType != GROUP_TYPE.NONE)
        {

            this.eventCount--;

            if (this.eventCount <= 0)
            {

                this.eventType = GROUP_TYPE.NONE;

                this.normalCount = Random.Range(3, 7);
            }

        }
        else
        {

            this.normalCount--;

            if (this.normalCount <= 0)
            {

                // イベントを発生させる.

                this.eventType = (GROUP_TYPE)Random.Range(0, 4);

                switch (this.eventType)
                {

                    default:
                    case GROUP_TYPE.DECELERATE:
                    case GROUP_TYPE.PASSING:
                    case GROUP_TYPE.SLOW:
                        {
                            this.eventCount = 1;
                        }
                        break;

                    case GROUP_TYPE.RAPID:
                        {
                            this.eventCount = Random.Range(2, 4);
                        }
                        break;
                }
            }
        }

        // ノーマル、イベントのグループを発生させる.

        if (this.eventType == GROUP_TYPE.NONE)
        {

            // ノーマルタイプのグループ.			

            float rate = (float)this.noMissCount / 10.0f;

            rate = Mathf.Clamp01(rate);

            this.nextSpeed = Mathf.Lerp(OniGroupControl.SPEED_MAX, OniGroupControl.SPEED_MIN, rate);

            this.normalGenrateLineDist = Mathf.Lerp(LevelControl.INTERVAL_MAX, LevelControl.INTERVAL_MIN, rate);

            this.groupTypeNext = GROUP_TYPE.NORMAL;

        }
        else
        {

            // イベントタイプのグループ.

            this.groupTypeNext = this.eventType;
        }

    }

    // ノーマルパターン.
    public void DispatchNormal(float speed)
    {
        Vector3 appearPosition = this.player.transform.position;

        // プレイヤーの前方、ぎりぎり画面外ぐらいの位置で発生する.
        appearPosition.x += appearMargin;

        this.CreateOniGroup(appearPosition, speed, OniGroupControl.TYPE.NORMAL);
    }

    // おそいパターン.
    public void DispatchSlow()
    {
        Vector3 appearPosition = this.player.transform.position;

        // プレイヤーの前方、ぎりぎり画面外ぐらいの位置で発生する.
        appearPosition.x += appearMargin;

        this.CreateOniGroup(appearPosition, OniGroupControl.SPEED_MIN * 5.0f, OniGroupControl.TYPE.NORMAL);
    }

    // 超短いパターン.
    public void DispatchRapid()
    {
        Vector3 appearPosition = this.player.transform.position;

        // プレイヤーの前方、ぎりぎり画面外ぐらいの位置で発生する.
        appearPosition.x += appearMargin;

        //this.create_oni_group(appear_position, OniGroupControl.SPEED_MIN, OniGroupControl.TYPE.NORMAL);
        this.CreateOniGroup(appearPosition, this.nextSpeed, OniGroupControl.TYPE.NORMAL);
    }

    // 途中で減速パターン.
    public void DispatchDecelerate()
    {
        Vector3 appearPosition = this.player.transform.position;

        // プレイヤーの前方、ぎりぎり画面外ぐらいの位置で発生する.
        appearPosition.x += appearMargin;

        this.CreateOniGroup(appearPosition, 9.0f, OniGroupControl.TYPE.DECELERATE);
    }

    // 途中でおに同士で追い抜きが発生するパターン.
    public void DispatchPassing()
    {
        float speedLow = 2.0f;
        float speedRate = 2.0f;
        float playerSpeed = this.player.GetComponent<Rigidbody>().velocity.x;
        float speedHigh = (speedLow - playerSpeed) / speedRate + playerSpeed;

        // 遅いおにが速いおにに追い抜かれる位置（0.0 プレイヤーの位置 ～ 1.0 画面右端）.
        float passing_point = 0.5f;

        Vector3 appear_position = this.player.transform.position;

        // ふたつのグループが途中で交差するように、発生位置を調整する.

        appear_position.x = this.player.transform.position.x + appearMargin;

        this.CreateOniGroup(appear_position, speedHigh, OniGroupControl.TYPE.NORMAL);

        appear_position.x = this.player.transform.position.x + appearMargin * Mathf.Lerp(speedRate, 1.0f, passing_point);

        this.CreateOniGroup(appear_position, speedLow, OniGroupControl.TYPE.NORMAL);
    }

    // -------------------------------------------------------------------------------- //

    // オニのグループを発生させる.
    private void CreateOniGroup(Vector3 appearPosition, float speed, OniGroupControl.TYPE type)
    {
        // -------------------------------------------------------- //
        // グループ全体のコリジョン（当たり判定）を生成する.	

        Vector3 position = appearPosition;

        // OniGroupPrefab のインスタンスを生成します.
        // "as GameObject" を末尾につけると、生成されたオブジェクトは
        // GameObject オブジェクトになります.
        //
        GameObject go = GameObject.Instantiate(this.oniGroupPrefab) as GameObject;

        OniGroupControl newGroup = go.GetComponent<OniGroupControl>();

        // 地面に接する高さ.
        position.y = OniGroupControl.collision_size / 2.0f;

        position.z = 0.0f;

        newGroup.transform.position = position;

        newGroup.scene_control = this.sceneControl;
        newGroup.main_camera = this.sceneControl.main_camera;
        newGroup.player = this.player;
        newGroup.runSpeed = speed;
        newGroup.type = type;

        // -------------------------------------------------------- //
        // グループに属するオニの集団を生成する.

        Vector3 basePosition = position;

        int oniNum = this.oniAppearNum;

        // コリジョンボックスの左端によせる.
        basePosition.x -= (OniGroupControl.collision_size / 2.0f - OniControl.collision_size / 2.0f);

        // 地面に接する高さ.
        basePosition.y = OniControl.collision_size / 2.0f;

        // オニを発生させる.
        newGroup.CreateOnis(oniNum, basePosition);

    }
}
