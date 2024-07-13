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
    public Material material_left;  // Planeオブジェクトにアタッチされたマテリアル
    public Material material_right;  // Planeオブジェクトにアタッチされたマテリアル
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
    private Texture2D reusableTexture_left; // 再利用可能なテクスチャ
    private Texture2D reusableTexture_right; // 再利用可能なテクスチャ

    private void Start()
    {
        // reusableTexture を初期化
        reusableTexture_left = new Texture2D(128, 128, TextureFormat.ARGB32, false);
        material_left.mainTexture = reusableTexture_left;
        // reusableTexture を初期化
        reusableTexture_right = new Texture2D(128, 128, TextureFormat.ARGB32, false);
        material_right.mainTexture = reusableTexture_right;

        udpClient0 = new UdpClient(1000);
        //udpClient1 = new UdpClient(2001);
        remoteEP = new IPEndPoint(IPAddress.Any, 0);
        RunPythonScript();
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
            List<byte> dataList_left = new List<byte>();
            List<byte> dataList_right = new List<byte>();
            //byte[] data = null;
            UnityEngine.Debug.Log("0");
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {

                    //byte[] loopnum = udpClient0.Receive(ref remoteEP);
                    //UnityEngine.Debug.Log("Received data: " + string.Join(" ", loopnum));
                    //for (int i = 0; i < 1; i++)
                    //{
                    //    byte[] data = udpClient0.Receive(ref remoteEP);
                    //    dataList.AddRange(data);
                    //    UnityEngine.Debug.Log("1");
                    //}
                    while (loop)
                    {
                        UnityEngine.Debug.Log("1");
                        byte[] data = udpClient0.Receive(ref remoteEP);
                        UnityEngine.Debug.Log("2");

                        if (Side == "left")
                        {
                            // 最初のバイトを loopnum として読み取る
                            byte loopnum_left = data[0];
                            // 次のバイトを num として読み取る
                            byte dataIndex_left = data[1];

                            UnityEngine.Debug.Log("Received data: " + loopnum_left + " " + dataIndex_left);

                            // 最初の2バイトを除いたデータをdataListに追加
                            dataList_left.AddRange(data.Skip(2));
                            if (loopnum_left == dataIndex_left)
                            {
                                Side = "right";
                            }
                        }

                        //data = null;
                        //UnityEngine.Debug.Log("1");
                        else if (Side == "right")
                        {
                            // 最初のバイトを loopnum として読み取る
                            byte loopnum_right = data[0];
                            // 次のバイトを num として読み取る
                            byte dataIndex_right = data[1];

                            UnityEngine.Debug.Log("Received data: " + loopnum_right + " " + dataIndex_right);

                            // 最初の2バイトを除いたデータをdataListに追加
                            dataList_right.AddRange(data.Skip(2));
                            if (loopnum_right == dataIndex_right)
                            {
                                Side = "left";
                                loop = false;
                                UnityEngine.Debug.Log("2");
                            }
                        }
                    }

                    byte[] concatenatedData_left = dataList_left.ToArray();
                    byte[] concatenatedData_right = dataList_right.ToArray();

                    lock (dataQueue)
                    {
                        dataQueue.Enqueue(concatenatedData_left);
                        dataQueue.Enqueue(concatenatedData_right);
                    }

                }

                catch (Exception e)
                {
                    UnityEngine.Debug.LogError("Error in data receiving task: " + e.ToString());
                }
                finally
                {
                    dataList_left.Clear();
                    dataList_right.Clear();
                    loop = true;
                    UnityEngine.Debug.Log("3");
                }
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
        if (reusableTexture_left != null)
        {
            Destroy(reusableTexture_left);
        }
        if (reusableTexture_right != null)
        {
            Destroy(reusableTexture_right);
        }
        udpClient0.Close();
        //udpClient1.Close();
    }
    private void Update()
    {
        // ここで非同期メソッドを呼び出す
        //Task.Run(ReceiveAndProcessDataAsync);

        lock (dataQueue)
        {
            while (dataQueue.Count > 0)
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

                byte[] dataall_left = dataQueue.Dequeue();
                byte[] dataall_right = dataQueue.Dequeue();

                // テクスチャにデータを読み込む
                reusableTexture_left.LoadImage(dataall_left);
                reusableTexture_right.LoadImage(dataall_right);

                // 前のテクスチャを解放せずに再利用

                // テクスチャを表示
                material_left.mainTexture = reusableTexture_left;
                material_right.mainTexture = reusableTexture_right;
            }
        }
    }
}
