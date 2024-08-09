using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Linq;

public class UDP : MonoBehaviour
{
    public Material material;  // Planeオブジェクトにアタッチされたマテリアル
    private UdpClient udpClient0;
    private UdpClient udpClient1;
    private IPEndPoint remoteEP;
    private Queue<byte[]> dataQueue = new Queue<byte[]>();
    private byte[] receivedData;
    private byte[] data;
    private object lockObject = new object();
    private bool loop = true;
    private string Side = "left";
    private Task dataReceivingTask;
    private CancellationTokenSource cancellationTokenSource;
    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
    private Texture2D reusableTexture; // 再利用可能なテクスチャ
    List<byte> dataList = new List<byte>();


    private void Start()
    {
        // reusableTexture を初期化
        reusableTexture = new Texture2D(128, 128, TextureFormat.ARGB32, false);
        material.mainTexture = reusableTexture;

        udpClient0 = new UdpClient(1000);
        //udpClient1 = new UdpClient(2001);
        remoteEP = new IPEndPoint(IPAddress.Any, 0);
        //RunPythonScript();
        //CallPythonFunction("function2", 1);
        StartReceivingData();

        // Application.quitting イベントに解放処理を登録
        Application.quitting += OnApplicationQuitting;
    }

    private void RunPythonScript()
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = @"C:\Users\hosei\anaconda3\envs\Py37CV470ARlive\python.exe";// Pythonの実行ファイルへのパスが環境変数に追加されている場合
        //start.FileName = @"C:\Users\hosei\AppData\Local\Programs\Python\Python37\python.exe";  
        // start.FileName = "/usr/local/bin/python3";  // Pythonの実行ファイルへのパスが環境変数に追加されていない場合
        start.Arguments = @"C:\Users\hosei\Mask.py";  // Pythonスクリプトへのパスを指定
        start.UseShellExecute = false; // シェルを使用せずに直接実行
        start.CreateNoWindow = true; // ウィンドウを表示しない
        //start.RedirectStandardOutput = true; // 標準出力をキャプチャ
        //start.EnvironmentVariables["PYTHONPATH"] = @"C:\Users\hosei\anaconda3\envs\Py37CV470ARlive\Lib\site-packages";

        Process process = new Process();
        process.StartInfo = start;

        process.Start();
    }




    private void StartReceivingData()
    {
        cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        dataReceivingTask = Task.Run(() =>
        {
            //List<byte> dataList_left = new List<byte>();
            //List<byte> dataList_right = new List<byte>();
            //byte[] data = null;
            UnityEngine.Debug.Log("0");
            while (!cancellationToken.IsCancellationRequested)
            {
                //sw.Reset();
                //sw.Start();
                try
                {
                    byte[] data = udpClient0.Receive(ref remoteEP);
                    //// 最初のバイトを loopnum として読み取る
                    //byte loopnum = data[0];
                    //// 次のバイトを num として読み取る
                    //byte dataIndex = data[1];
                    //UnityEngine.Debug.Log("Received data: " + loopnum + " " + dataIndex);

                    dataQueue.Enqueue(data);
                }

                catch (Exception e)
                {
                    UnityEngine.Debug.LogError("Error in data receiving task: " + e.ToString());
                }
                finally
                {
                    //dataList.Clear();
                    //loop = true;
                    //UnityEngine.Debug.Log("3");
                }
                //sw.Stop();
                //UnityEngine.Debug.Log(sw.ElapsedMilliseconds + "ms");

            }
        });
    }


    private void OnApplicationQuitting()
    {
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
        }
        UnityEngine.Debug.Log("3");
        // 非同期スレッドが終了するまで待機
        if (dataReceivingTask != null)
        {
            dataReceivingTask.Wait();
        }
        UnityEngine.Debug.Log("4");
        dataQueue.Clear();
        // アプリケーションが終了する際にテクスチャを解放
        if (reusableTexture != null)
        {
            Destroy(reusableTexture);
        }
        udpClient0.Close();
        //udpClient1.Close();
    }

    private void Update()
    {
        // ここで非同期メソッドを呼び出す
        //Task.Run(ReceiveAndProcessDataAsync);

        while (loop)
        {
            //byte[] dataall = dataQueue.Dequeue();
            //Texture2D tex = new Texture2D(128, 128, TextureFormat.ARGB32, false);
            //tex.LoadImage(dataall);
            ////前のテクスチャを解放
            //if (material.mainTexture != null)
            //{
            //    Destroy(material.mainTexture);
            //}
            //material.mainTexture = tex;
            if (dataQueue.Count > 0)
            {
                byte[] data = dataQueue.Dequeue();
                // 最初のバイトを loopnum として読み取る
                byte loopnum = data[0];
                // 次のバイトを num として読み取る
                byte dataIndex = data[1];
                // その次のバイトを num として読み取る

                //UnityEngine.Debug.Log("Received data: " + loopnum + " " + dataIndex);
                // 最初の2バイトを除いたデータをdataListに追加
                dataList.AddRange(data.Skip(2));
                //data = null;
                //UnityEngine.Debug.Log("1");
                if (loopnum == dataIndex)
                {
                    loop = false;
                    UnityEngine.Debug.Log("Left");
                }
                //else if(dataQueue.Count == 0)
                //{
                //    loop = false;
                //    UnityEngine.Debug.Log("2qqqqqqqqqqqqq");
                //}
            }
            else if (dataQueue.Count == 0)
            {
                UnityEngine.Debug.Log("Space          0000000");
            }
        }
        // テクスチャにデータを読み込む
        byte[] LoadData = dataList.ToArray();
        reusableTexture.LoadImage(LoadData);

        // 前のテクスチャを解放せずに再利用

        // テクスチャを表示
        material.mainTexture = reusableTexture;
        dataList.Clear();
        loop = true;
    }
}
