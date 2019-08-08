using UnityEngine;

public class RankDisp : MonoBehaviour
{

    protected const float ZOOM_TIME = 0.4f;

    public float timer = 0.0f;
    public float scale = 1.0f;
    public float alpha = 0.0f;

    public UnityEngine.UI.Image uiImageGrade;       // 評価の文字（優/良/可/不可）のイメージ

    public Sprite[] uiSpriteRank;       // 『鬼切り』『見切り』用の評価の文字（優/良/可/不可）のスプライト

    // ================================================================ //
    // MonoBehaviour からの継承.

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        this.UpdateSub();

        this.timer += deltaTime;
    }

    protected void UpdateSub()
    {
        float zoomInTime = ZOOM_TIME;
        float rate;

        if (this.timer < zoomInTime)
        {

            rate = this.timer / zoomInTime;
            rate = Mathf.Pow(rate, 2.5f);
            this.scale = Mathf.Lerp(1.5f, 1.0f, rate);

        }
        else
        {

            this.scale = 1.0f;
        }

        if (this.timer < zoomInTime)
        {

            rate = this.timer / zoomInTime;
            rate = Mathf.Pow(rate, 2.5f);
            this.alpha = Mathf.Lerp(0.0f, 1.0f, rate);

        }
        else
        {

            this.alpha = 1.0f;
        }

        // アルファーを UI.Image にセットする.

        UnityEngine.UI.Image[] images = this.GetComponentsInChildren<UnityEngine.UI.Image>();

        foreach (var image in images)
        {

            Color color = image.color;

            color.a = this.alpha;

            image.color = color;
        }

        // スケールをセットする.
        this.GetComponent<RectTransform>().localScale = Vector3.one * this.scale;
    }

    // ================================================================ //

    public void StartDisp(int rank)
    {
        this.uiImageGrade.sprite = this.uiSpriteRank[rank];

        this.gameObject.SetActive(true);

        this.timer = 0.0f;

        this.UpdateSub();
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

}
