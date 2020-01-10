using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

public class ExportCSV : MonoBehaviour
{

    private int BattleCount = 1;
    // Start is called before the first frame update
    void Start()
    {
        //OutputCSV();
        //GameObject BattleSystem = this.GetComponent<BattleController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OutputCSV(int clearTurn, List<string>felloActions , List<int> fbCounts, List<string>enemyActions ,string AITYPE)
    {
        // ファイル書き出し
        // 現在のフォルダにsaveData.csvを出力する(決まった場所に出力したい場合は絶対パスを指定してください)
        // 引数説明：第1引数→ファイル出力先, 第2引数→ファイルに追記(true)or上書き(false), 第3引数→エンコード
        StreamWriter sw = new StreamWriter(@"RPGLogData.csv", false, Encoding.GetEncoding("Shift_JIS"));


        // ヘッダー出力
        string[] ds1 = {"戦闘" + BattleCount.ToString() + "回目","デフォルトAIの種類",AITYPE};
        string ds2 = string.Join(",", ds1);
        sw.WriteLine(ds2);

        // ヘッダー出力
        string[] s1 = { "ターン", "仲間の行動", "フィードバック回数", "ボスの行動" };
        string s2 = string.Join(",", s1);
        sw.WriteLine(s2);

        // データ出力
        for (int i = 0; i <  clearTurn ; i++)
        {
            string[] str = { (i + 1) + "ターン目", felloActions[i], fbCounts[i] + "回", enemyActions[i] };
            string str2 = string.Join(",", str);
            sw.WriteLine(str2);
        }
        // StreamWriterを閉じる
        sw.Close();
    }
}
