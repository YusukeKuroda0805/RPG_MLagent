using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

public class ExportCSV : MonoBehaviour
{

    //private int BattleCount = 1;
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

    public void OutputCSV(string AITYPE,int Bcount,int clearTurn, List<string>felloActions , List<int> HP, List<float> Attack, List<float> Defense, List<string> enemyActions, List<int> eHP, List<float> eAttack, List<float> eDefense, List<int>FBC,List<int>EFBC,string csvID )
    {
        // ファイル書き出し
        // 現在のフォルダにsaveData.csvを出力する(決まった場所に出力したい場合は絶対パスを指定してください)
        // 引数説明：第1引数→ファイル出力先, 第2引数→ファイルに追記(true)or上書き(false), 第3引数→エンコード
        //StreamWriter sw = new StreamWriter(@"RPGLogData.csv", true, Encoding.GetEncoding("Shift_JIS"));
        //RPGLogFiles
        string logpath1 = Application.dataPath + @"/RPGLogData"+csvID+ ".csv";
        string logpath2 = logpath1.Replace("Assets", "RPGLogFiles");

        StreamWriter sw = new StreamWriter(logpath2, true, Encoding.GetEncoding("Shift_JIS"));


        // ヘッダー出力
        string[] ds1 = {"戦闘" + Bcount.ToString() + "回目",AITYPE};
        string ds2 = string.Join(",", ds1);
        sw.WriteLine(ds2);

        // ヘッダー出力
        string[] s1 = { "ターン数", "仲間の行動", "HP", "攻撃力", "防御力", "ボスの行動","敵のHP", "敵の攻撃力", "敵の防御力", "FB回数(味方行動時)","FB回数（敵行動時）"};
        string s2 = string.Join(",", s1);
        sw.WriteLine(s2);

        // データ出力
        for (int i = 0; i <  clearTurn ; i++)
        {
            string[] str = { (i + 1) + "ターン目", felloActions[i], HP[i].ToString(), Attack[i].ToString(), Defense[i].ToString(),enemyActions[i], eHP[i].ToString(), eAttack[i].ToString(), eDefense[i].ToString(), FBC[i].ToString() + "回", EFBC[i].ToString() + "回" };
            string str2 = string.Join(",", str);
            sw.WriteLine(str2);
        }

        string end1 = " ";
        sw.WriteLine(end1);

        // StreamWriterを閉じる
        sw.Close();
    }
}
