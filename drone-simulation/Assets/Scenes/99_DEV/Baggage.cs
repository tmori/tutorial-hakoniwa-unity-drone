using UnityEngine;

namespace hakoniwa.objects.core
{
    /// <summary>
    /// HakoBaggageクラス
    /// ドローンや他のオブジェクトに運搬される荷物を模したオブジェクトの挙動を管理します。
    /// </summary>
    public class Baggage : MonoBehaviour
    {
        public GameObject parent = null; // 現在の親オブジェクト（運搬元）
        public float speed = 10f; // 移動速度（Lerpの補間速度）
        private Transform initial_parent; // 初期の親オブジェクト（元の状態を戻すため）
        private Rigidbody rd; // Rigidbodyコンポーネントへの参照

        /// <summary>
        /// 初期化処理
        /// </summary>
        void Start()
        {
            // 初期の親オブジェクトを保存
            initial_parent = this.transform.parent;

            // Rigidbodyコンポーネントを取得
            rd = this.GetComponentInChildren<Rigidbody>();
            if (rd == null)
            {
                // Rigidbodyが見つからない場合は例外をスロー
                throw new System.Exception($"Not found Rigidbody on {this.transform.name}");
            }
        }

        /// <summary>
        /// 他のオブジェクトに掴まれる（親として登録される）
        /// </summary>
        /// <param name="grab_parent">掴むオブジェクト</param>
        public void Grab(GameObject grab_parent)
        {
            this.parent = grab_parent; // 親オブジェクトを設定
        }

        /// <summary>
        /// 現在の親オブジェクトからリリースされる（自由な状態になる）
        /// </summary>
        public void Release()
        {
            this.parent = null; // 親オブジェクトをリセット
        }

        /// <summary>
        /// 自由な状態かどうかを確認
        /// </summary>
        /// <returns>自由であればtrue、掴まれていればfalse</returns>
        public bool IsFree()
        {
            return this.parent == null;
        }

        /// <summary>
        /// 毎フレームの更新処理
        /// </summary>
        void Update()
        {
            if (parent != null)
            {
                // 親が設定されている場合（掴まれている状態）
                if (this.transform.parent != parent.transform)
                {
                    // 親オブジェクトが変更されていれば更新
                    this.transform.parent = parent.transform;
                }

                // Rigidbodyを停止（物理挙動を無効化）
                this.rd.isKinematic = true;

                // 親オブジェクトの位置に向かって補間で移動
                this.transform.position = Vector3.Lerp(
                    this.transform.position,
                    parent.transform.position,
                    Time.deltaTime * speed // 補間速度
                );

                // 親オブジェクトの回転に向かって補間
                this.transform.rotation = Quaternion.Lerp(
                    this.transform.rotation,
                    parent.transform.rotation,
                    Time.deltaTime * speed // 補間速度
                );
            }
            else
            {
                // 親が設定されていない場合（自由な状態）
                this.transform.parent = initial_parent; // 初期の親に戻す
                this.rd.isKinematic = false; // Rigidbodyを再度有効化
            }
        }
    }
}
