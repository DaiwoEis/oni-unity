using UnityEngine;

public class AttackColliderControl : MonoBehaviour
{

    public PlayerControl player = null;

    // 攻撃判定発生中？.
    private bool isPowered = false;

    // -------------------------------------------------------------------------------- //

    private void Start()
    {
        this.SetPowered(false);
    }


    // OnTriggerEnter はコリジョン同士が接した瞬間しか呼ばれないので、
    // 攻撃判定の球が発生したときにオニが球の内側に完全に入っていると、
    // うまくひろえない.
    //void OnTriggerEnter(Collider other)
    private void OnTriggerStay(Collider other)
    {
        do
        {

            if (!this.isPowered)
            {

                break;
            }

            if (other.tag != "OniGroup")
            {

                break;
            }

            OniGroupControl oni = other.GetComponent<OniGroupControl>();

            if (oni == null)
            {

                break;
            }

            //

            oni.OnAttackedFromPlayer();

            this.player.OnAttackOni(oni.transform.position);

            //// 『攻撃できない中』タイマーをリセットする（すぐに攻撃可にする）.
            //this.player.ResetAttackDisableTimer();

            //// 攻撃ヒットエフェクトを再生する.
            //this.player.PlayHitEffect(oni.transform.position);

            //// 攻撃ヒット音を鳴らす.
            //this.player.PlayHitSound();

        } while (false);
    }

    public void SetPowered(bool sw)
    {
        this.isPowered = sw;

        if (SceneControl.IS_DRAW_PLAYER_ATTACK_COLLISION)
        {

            this.GetComponent<Renderer>().enabled = sw;
        }
    }
}
