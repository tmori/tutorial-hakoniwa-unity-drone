using UnityEngine;

public class HakoMagnet : MonoBehaviour
{
    public bool on; // MagnetのOn/Off状態（trueでOn、falseでOff）
    public float detectionRange = 5f; // 想定距離範囲（Magnetが影響を及ぼす範囲）
    private HakoBaggage currentBaggage; // 現在掴んでいるBaggageオブジェクト

    void Start()
    {
        on = false; // 初期状態はOff（Magnetが無効）
    }

    void Update()
    {
        // Magnetの状態に応じて処理を実行
        if (on && currentBaggage == null)
        {
            // MagnetがOnで、まだ何も掴んでいない場合
            FindAndGrabNearestBaggage(); // 近くのBaggageを探して掴む
        }
        else if (!on && currentBaggage != null)
        {
            // MagnetがOffで、現在掴んでいるBaggageがある場合
            ReleaseBaggage(); // Baggageをリリースする
        }
    }

    /// <summary>
    /// MagnetをOnにする
    /// 外部からこの関数を呼び出すことで、Magnetを有効化できる
    /// </summary>
    public void TurnOn()
    {
        on = true;
    }

    /// <summary>
    /// MagnetをOffにする
    /// 外部からこの関数を呼び出すことで、Magnetを無効化できる
    /// </summary>
    public void TurnOff()
    {
        on = false;
    }
    /// <summary>
    /// 荷物を掴んでいるかどうか
    /// </summary>
    public bool IsGrabbed()
    {
        if (on && (currentBaggage != null)) {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 想定距離範囲内にいる最も近いHakoBaggageを探し、掴む
    /// </summary>
    private void FindAndGrabNearestBaggage()
    {
        HakoBaggage nearestBaggage = null; // 最も近いBaggageを保持する変数
        float nearestDistance = detectionRange; // 検出範囲（初期値は設定された最大範囲）

        // シーン内に存在するすべてのHakoBaggageオブジェクトを取得
        HakoBaggage[] baggages = FindObjectsByType<HakoBaggage>(FindObjectsSortMode.None);

        foreach (HakoBaggage baggage in baggages)
        {
            // 掴まれていない状態かつ、自分より下に位置しているBaggageのみを対象とする
            if (baggage.IsFree() && baggage.transform.position.y < this.transform.position.y)
            {
                float distance = Vector3.Distance(transform.position, baggage.transform.position); // 自分とBaggage間の距離を計算
                if (distance < nearestDistance) // 距離が現在の最短距離よりも短い場合
                {
                    nearestDistance = distance; // 最短距離を更新
                    nearestBaggage = baggage; // 最も近いBaggageを更新
                }
            }
        }

        // 最も近いBaggageが見つかった場合、掴む
        if (nearestBaggage != null)
        {
            currentBaggage = nearestBaggage; // 現在のBaggageとして記録
            currentBaggage.Grab(this.gameObject); // BaggageのGrabメソッドを呼び出して掴む
        }
    }

    /// <summary>
    /// 現在掴んでいるBaggageをリリースする
    /// </summary>
    private void ReleaseBaggage()
    {
        if (currentBaggage != null) // 現在掴んでいるBaggageが存在する場合
        {
            currentBaggage.Release(); // BaggageのReleaseメソッドを呼び出してリリース
            currentBaggage = null; // 現在掴んでいるBaggageをリセット
        }
    }
}
