using UnityEngine;

public class GUIControl : MonoBehaviour
{

    private SceneControl sceneControl = null;
    public ScoreControl scoreControl = null;

    public GameObject uiImageStart;     // 『はじめっ！』
    public GameObject uiImageReturn;        // 『もどる！』

    public RankDisp rankSmallDefeat;                // 『鬼切り』評価.
    public RankDisp rankSmallEval;                  // 『見切り』評価.
    public RankDisp rankTotal;                      // トータル評価.

    public Sprite[] uiSprite_GradeSmall;    // 『鬼切り』『見切り』用の評価の、小さな文字（優/良/可/不可）
    public Sprite[] uiSprite_Grade;         // トータル評価の文字（優/良/可/不可）

    // ================================================================ //

    private void Awake()
    {
        this.sceneControl = SceneControl.get();
        this.scoreControl = GetComponent<ScoreControl>();

        this.scoreControl.SetNumForce(this.sceneControl.result.oni_defeat_num);

        this.rankSmallDefeat.uiSpriteRank = this.uiSprite_GradeSmall;
        this.rankSmallEval.uiSpriteRank = this.uiSprite_GradeSmall;
        this.rankTotal.uiSpriteRank = this.uiSprite_Grade;
    }


    private void Update()
    {
        // 『切ったおにの数』をスコアー表示にセット.
        this.scoreControl.SetNum(this.sceneControl.result.oni_defeat_num);

        // ---------------------------------------------------------------- //
        // デバッグ用
#if false
		SceneControl	scene = this.scene_control;

		dbPrint.setLocate(10, 5);
		dbPrint.print(scene.attack_time);
		dbPrint.print(scene.evaluation);
		if(this.scene_control.level_control.is_random) {

			dbPrint.print("RANDOM(" + scene.level_control.group_type_next + ")");

		} else {

			dbPrint.print(scene.level_control.group_type_next);
		}

		dbPrint.print(scene.result.oni_defeat_num);

		// 切りの評価（近くで切った？）の合計
		for(int i = 0;i < (int)SceneControl.EVALUATION.NUM;i++) {

			dbPrint.print(((SceneControl.EVALUATION)i).ToString() + " " + scene.result.eval_count[i].ToString());
		}
#endif
    }

    // 『はじめっ！』の文字を表示/非表示にする.
    public void SetVisibleStart(bool is_visible)
    {
        this.uiImageStart.SetActive(is_visible);
    }

    // 『もどる！』の文字を表示/非表示にする.
    public void SetVisibleReturn(bool is_visible)
    {
        this.uiImageReturn.SetActive(is_visible);
    }

    // 『鬼切り』の評価の表示をスタートする.
    public void StartDispDefeatRank()
    {
        int rank = this.sceneControl.result_control.getDefeatRank();

        this.rankSmallDefeat.StartDisp(rank);
    }

    // 『鬼切り』の評価の表示を消す.
    public void HideDefeatRank()
    {
        this.rankSmallDefeat.Hide();
    }

    // 『鬼切り』の評価の表示をスタートする.
    public void StartDispEvaluationRank()
    {
        int rank = this.sceneControl.result_control.getEvaluationRank();

        this.rankSmallEval.StartDisp(rank);
    }

    // 『鬼切り』の評価の表示を消す.
    public void HideEvaluationRank()
    {
        this.rankSmallEval.Hide();
    }

    // トータル評価の表示をスタートする.
    public void StartDispTotalRank()
    {
        int rank = this.sceneControl.result_control.getTotalRank();

        this.rankTotal.StartDisp(rank);
    }

    // ================================================================ //
    // インスタンス.

    protected static GUIControl instance = null;

    public static GUIControl get()
    {
        if (GUIControl.instance == null)
        {

            GameObject go = GameObject.Find("GameCanvas");

            if (go != null)
            {

                GUIControl.instance = go.GetComponent<GUIControl>();

            }
            else
            {

                Debug.LogError("Can't find game object \"GUIControl\".");
            }
        }

        return (GUIControl.instance);
    }
}
