using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private Animator animator;

    public AudioClip[] AttackSound;             
    public AudioClip SwordSound;                 
    public AudioClip SwordHitSound;             
    public AudioClip MissSound;                 
    public AudioClip runSound;

    public AudioSource attack_voice_audio;         
    public AudioSource sword_audio;             
    public AudioSource miss_audio;                
    public AudioSource run_audio;

    public int attack_sound_index = 0;      

    public float run_speed = 5.0f;

    public const float RUN_SPEED_MAX = 20.0f;

    protected const float run_speed_add = 5.0f;

    protected const float run_speed_sub = 5.0f * 4.0f;

    protected const float MISS_GRAVITY = 9.8f * 2.0f;

    protected AttackColliderControl attack_collider = null;

    public SceneControl scene_control = null;

    protected float attackTimer = 0.0f;

    protected float attack_disable_timer = 0.0f;

    protected const float ATTACK_TIME = 0.3f;

    protected const float ATTACK_DISABLE_TIME = 1.0f;

    protected bool is_running = true;
    protected bool is_contact_floor = false;
    protected bool isPlayable = true;

    public float stop_position = -1.0f;

    public enum ATTACK_MOTION
    {

        NONE = -1,

        RIGHT = 0,
        LEFT,

        NUM,
    };

    public ATTACK_MOTION attack_motion = ATTACK_MOTION.LEFT;

    public AnimatedTextureExtendedUV kiseki_left = null;
    public AnimatedTextureExtendedUV kiseki_right = null;

    public ParticleSystem fx_hit = null;

    public ParticleSystem fx_run = null;

    public float min_rate = 0.0f;
    public float max_rate = 3.0f;

    public enum STEP
    {

        NONE = -1,

        RUN = 0,        
        STOP,          
        MISS,          
        NUM,
    };

    public STEP step = STEP.NONE;
    public STEP next_step = STEP.NONE;

    public void Start()
    {
        this.animator = this.GetComponentInChildren<Animator>();

        this.attack_collider = GameObject.FindGameObjectWithTag("AttackCollider").GetComponent<AttackColliderControl>();

        this.attack_collider.player = this;


        this.kiseki_left = GameObject.FindGameObjectWithTag("FX_Kiseki_L").GetComponent<AnimatedTextureExtendedUV>();
        this.kiseki_left.StopPlay();

        this.kiseki_right = GameObject.FindGameObjectWithTag("FX_Kiseki_R").GetComponent<AnimatedTextureExtendedUV>();
        this.kiseki_right.StopPlay();


        this.fx_hit = GameObject.FindGameObjectWithTag("FX_Hit").GetComponent<ParticleSystem>();

        this.fx_run = GameObject.FindGameObjectWithTag("FX_Run").GetComponent<ParticleSystem>();

        this.run_speed = 0.0f;

        this.next_step = STEP.RUN;

        this.attack_voice_audio = this.gameObject.AddComponent<AudioSource>();
        this.sword_audio = this.gameObject.AddComponent<AudioSource>();
        this.miss_audio = this.gameObject.AddComponent<AudioSource>();

        this.run_audio = this.gameObject.AddComponent<AudioSource>();
        this.run_audio.clip = this.runSound;
        this.run_audio.loop = true;
        this.run_audio.Play();
    }

    private void Update()
    {
#if false
		if(Input.GetKey(KeyCode.Keypad1)) {
			min_rate -= 0.1f;
		}
		if(Input.GetKey(KeyCode.Keypad2)) {
			min_rate += 0.1f;
		}
		if(Input.GetKey(KeyCode.Keypad4)) {
			max_rate -= 0.1f;
		}
		if(Input.GetKey(KeyCode.Keypad5)) {
			max_rate += 0.1f;
		}
#endif
        min_rate = Mathf.Clamp(min_rate, 0.0f, max_rate);
        max_rate = Mathf.Clamp(max_rate, min_rate, 5.0f);

        if (this.next_step == STEP.NONE)
        {

            switch (this.step)
            {

                case STEP.RUN:
                    {
                        if (!this.is_running)
                        {

                            if (this.run_speed <= 0.0f)
                            {

                                this.fx_run.Stop();

                                this.next_step = STEP.STOP;
                            }
                        }
                    }
                    break;

                case STEP.MISS:
                    {
                        if (this.is_contact_floor)
                        {

                            this.fx_run.Play();

                            this.GetComponent<Rigidbody>().useGravity = true;
                            this.next_step = STEP.RUN;
                        }
                    }
                    break;
            }
        }

        if (this.next_step != STEP.NONE)
        {

            switch (this.next_step)
            {

                case STEP.STOP:
                    {
                        this.animator.SetTrigger("stop");
                    }
                    break;

                case STEP.MISS:
                    {
                        Vector3 velocity = this.GetComponent<Rigidbody>().velocity;

                        float jump_height = 1.0f;

                        velocity.x = -2.5f;
                        velocity.y = Mathf.Sqrt(MISS_GRAVITY * jump_height);
                        velocity.z = 0.0f;

                        this.GetComponent<Rigidbody>().velocity = velocity;
                        this.GetComponent<Rigidbody>().useGravity = false;

                        this.run_speed = 0.0f;

                        this.animator.SetTrigger("yarare");

                        this.miss_audio.PlayOneShot(this.MissSound);

                        this.fx_run.Stop();
                    }
                    break;
            }

            this.step = this.next_step;

            this.next_step = STEP.NONE;
        }

        if (this.is_running)
        {

            this.run_audio.volume = 1.0f;

        }
        else
        {

            this.run_audio.volume = Mathf.Max(0.0f, this.run_audio.volume - 0.05f);
        }

        switch (this.step)
        {

            case STEP.RUN:
                {

                    if (this.is_running)
                    {

                        this.run_speed += PlayerControl.run_speed_add * Time.deltaTime;

                    }
                    else
                    {

                        this.run_speed -= PlayerControl.run_speed_sub * Time.deltaTime;
                    }

                    this.run_speed = Mathf.Clamp(this.run_speed, 0.0f, PlayerControl.RUN_SPEED_MAX);

                    Vector3 new_velocity = this.GetComponent<Rigidbody>().velocity;

                    new_velocity.x = run_speed;

                    if (new_velocity.y > 0.0f)
                    {

                        new_velocity.y = 0.0f;
                    }

                    this.GetComponent<Rigidbody>().velocity = new_velocity;

                    float rate;

                    rate = this.run_speed / PlayerControl.RUN_SPEED_MAX;
                    this.run_audio.pitch = Mathf.Lerp(min_rate, max_rate, rate);

                    this.AttackControl();

                    this.sword_fx_control();


                    if (this.attack_disable_timer > 0.0f)
                    {

                        this.GetComponent<Renderer>().material.color = Color.gray;

                    }
                    else
                    {

                        this.GetComponent<Renderer>().material.color = Color.Lerp(Color.white, Color.blue, 0.5f);
                    }

#if UNITY_EDITOR
                    if (Input.GetKeyDown(KeyCode.W))
                    {

                        Vector3 position = this.transform.position;

                        position.x += 100.0f * FloorControl.WIDTH * FloorControl.MODEL_NUM;

                        this.transform.position = position;
                    }
#endif
                }
                break;

            case STEP.MISS:
                {
                    this.GetComponent<Rigidbody>().velocity += Vector3.down * MISS_GRAVITY * Time.deltaTime;
                }
                break;

        }

        this.is_contact_floor = false;
    }


    public void OnCollisionStay(Collision other)
    {

        if (other.gameObject.tag == "OniGroup")
        {

            do
            {

                if (this.attackTimer > 0.0f)
                {

                    break;
                }

                if (this.step == STEP.MISS)
                {

                    break;
                }

                this.next_step = STEP.MISS;

                this.scene_control.OnPlayerMissed();

                OniGroupControl oniGroup = other.gameObject.GetComponent<OniGroupControl>();

                oniGroup.OnPlayerHitted();

            } while (false);

        }

        if (other.gameObject.tag == "Floor")
        {

            if (other.relativeVelocity.y >= Physics.gravity.y * Time.deltaTime)
            {

                this.is_contact_floor = true;
            }
        }
    }

    public void OnCollisionEnter(Collision other)
    {
        this.OnCollisionStay(other);
    }

    public void OnAttackOni(Vector3 postion)
    {
        this.ResetAttackDisableTimer();

        this.PlayHitEffect(postion);

        this.PlayHitSound();
    }

    public void PlayHitEffect(Vector3 position)
    {
        this.fx_hit.transform.position = position;
        this.fx_hit.Play();
    }

    public void PlayHitSound()
    {
        this.sword_audio.PlayOneShot(this.SwordHitSound);
    }

    public void ResetAttackDisableTimer()
    {
        this.attack_disable_timer = 0.0f;
    }

    public float GetAttackTimer()
    {
        return (PlayerControl.ATTACK_TIME - this.attackTimer);
    }

    public float GetSpeedRate()
    {
        float player_speed_rate = Mathf.InverseLerp(0.0f, PlayerControl.RUN_SPEED_MAX, this.GetComponent<Rigidbody>().velocity.magnitude);

        return (player_speed_rate);
    }

    public void StopRequest()
    {
        this.is_running = false;
    }

    public void Playable()
    {
        this.isPlayable = true;
    }

    public void UnPlayable()
    {
        this.isPlayable = false;
    }

    public bool IsStopped()
    {
        bool is_stopped = false;

        do
        {

            if (this.is_running)
            {

                break;
            }
            if (this.run_speed > 0.0f)
            {

                break;
            }

            //

            is_stopped = true;

        } while (false);

        return (is_stopped);
    }

    public float CalcDistanceToStop()
    {
        float distance = this.GetComponent<Rigidbody>().velocity.sqrMagnitude / (2.0f * PlayerControl.run_speed_sub);

        return (distance);
    }

    private bool is_attack_input()
    {
        bool is_attacking = false;

        if (Input.GetMouseButtonDown(0))
        {

            is_attacking = true;
        }

        if (SceneControl.IS_AUTO_ATTACK)
        {

            GameObject[] oni_groups = GameObject.FindGameObjectsWithTag("OniGroup");

            foreach (GameObject oni_group in oni_groups)
            {

                float distance = oni_group.transform.position.x - this.transform.position.x;

                distance -= 1.0f / 2.0f;
                distance -= OniGroupControl.collision_size / 2.0f;

                // 後ろにいるものは無視.
                // （今回はゲームの仕様的にありえないが、念のため）.
                //
                if (distance < 0.0f)
                {

                    continue;
                }

                // 衝突までの予想時間.

                float time_left = distance / (this.GetComponent<Rigidbody>().velocity.x - oni_group.GetComponent<OniGroupControl>().runSpeed);

                // 離れていくものは無視.
                //
                if (time_left < 0.0f)
                {

                    continue;
                }

                if (time_left < 0.1f)
                {

                    is_attacking = true;
                }
            }
        }

        return (is_attacking);
    }

    private void AttackControl()
    {
        if (!this.isPlayable)
        {
            return;
        }

        if (this.attackTimer > 0.0f)
        {

            this.attackTimer -= Time.deltaTime;

            if (this.attackTimer <= 0.0f)
            {

                // コライダー（攻撃の当たり判定）の当たり判定を無効にする.
                //
                attack_collider.SetPowered(false);
            }

        }
        else
        {

            this.attack_disable_timer -= Time.deltaTime;

            if (this.attack_disable_timer > 0.0f)
            {

                // まだ攻撃できない中.

            }
            else
            {

                this.attack_disable_timer = 0.0f;

                if (this.is_attack_input())
                {
                    attack_collider.SetPowered(true);

                    this.attackTimer = PlayerControl.ATTACK_TIME;
                    this.attack_disable_timer = PlayerControl.ATTACK_DISABLE_TIME;

                    switch (this.attack_motion)
                    {

                        default:
                        case ATTACK_MOTION.RIGHT: this.attack_motion = ATTACK_MOTION.LEFT; break;
                        case ATTACK_MOTION.LEFT: this.attack_motion = ATTACK_MOTION.RIGHT; break;
                    }


                    switch (this.attack_motion)
                    {

                        default:
                        case ATTACK_MOTION.RIGHT: this.animator.SetTrigger("attack_r"); break;
                        case ATTACK_MOTION.LEFT: this.animator.SetTrigger("attack_l"); break;
                    }

                    this.attack_voice_audio.PlayOneShot(this.AttackSound[this.attack_sound_index]);

                    this.attack_sound_index = (this.attack_sound_index + 1) % this.AttackSound.Length;

                    this.sword_audio.PlayOneShot(this.SwordSound);

                }
            }
        }
    }

    private void sword_fx_control()
    {

        do
        {

            if (this.attackTimer <= 0.0f)
            {
                break;
            }

            Animator animator = this.GetComponentInChildren<Animator>();

            AnimatorStateInfo state_info = animator.GetCurrentAnimatorStateInfo(0);
            AnimatorClipInfo clip_info = animator.GetCurrentAnimatorClipInfo(0)[0];
            AnimationClip clip = clip_info.clip;

            AnimatedTextureExtendedUV anim_player;

            switch (this.attack_motion)
            {

                default:
                case ATTACK_MOTION.RIGHT:
                    {
                        anim_player = this.kiseki_right;
                    }
                    break;

                case ATTACK_MOTION.LEFT:
                    {
                        anim_player = this.kiseki_left;
                    }
                    break;
            }

            float start_frame = 2.5f;
            float start_time = start_frame / clip.frameRate;
            float current_time = state_info.normalizedTime * state_info.length;

            if (current_time < start_time)
            {
                break;
            }

            anim_player.StartPlay(current_time - start_time);

        } while (false);
    }
}
