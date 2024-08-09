using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

public class UDPTest1 : MonoBehaviour
{
    public Material material;  // Planeオブジェクトにアタッチされたマテリアル
    private UdpClient udpClient0;
    private UdpClient udpClient1;
    private IPEndPoint remoteEP;
    public float distanceFromCamera = 1.0f;     // カメラからの距離
    public Quaternion rotationOffset = Quaternion.identity;    // オブジェクトの回転オフセット
    public float horizontalOffset = 0.0f;       // オブジェクトの横方向オフセット
    public Camera cameraReference;
    private Queue<byte[]> dataQueue = new Queue<byte[]>();
    private byte[] receivedData;
    private byte[] data;
    private byte Distance;
    private bool loop = true;
    private object lockObject = new object();
    //private bool shouldStopThread = true;
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

        udpClient0 = new UdpClient(1500);  // UDP受信ポート番号を指定する
        //udpClient1 = new UdpClient(1001);  // UDP受信ポート番号を指定する
        remoteEP = new IPEndPoint(IPAddress.Any, 0);
        StartReceivingData();

        // Application.quitting イベントに解放処理を登録
        Application.quitting += OnApplicationQuitting;

    }

    private void StartReceivingData()
    {
        cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        dataReceivingTask = Task.Run(() =>
        {
            //List<byte> dataList = new List<byte>();
            //byte[] data = null;
            //UnityEngine.Debug.Log("0");
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

        //lock (dataQueue)
        //{
        //    while (dataQueue.Count > 0)
        //    {
        //        byte[] dataall = dataQueue.Dequeue();
        //        Texture2D tex = new Texture2D(128, 128, TextureFormat.ARGB32, false);
        //        tex.LoadImage(dataall);
        //        material.mainTexture = tex;
        //    }
        //}

        //// カメラの位置と回転を取得
        //cameraReference = GameObject.Find("Camera_left").GetComponent<Camera>();

        //Transform cameraTransform = cameraReference.transform;
        //Vector3 cameraPosition = cameraTransform.position;
        //Quaternion cameraRotation = cameraTransform.rotation;

        //// オブジェクトをカメラの正面に配置
        //Transform objectTransform = transform;
        //objectTransform.position = cameraPosition + cameraTransform.forward * distanceFromCamera;
        //objectTransform.rotation = cameraRotation * rotationOffset;

        //// オブジェクトの横方向オフセットを適用
        //Vector3 rightVector = Quaternion.Euler(cameraRotation.eulerAngles.x, cameraRotation.eulerAngles.y, cameraRotation.eulerAngles.z) * Vector3.right;
        //objectTransform.position += rightVector * horizontalOffset;
    }
}
/*
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System;

public class UDPTest1 : MonoBehaviour
{
    public Material material;  // Planeオブジェクトにアタッチされたマテリアル

    private UdpClient udpClient0;
    private UdpClient udpClient1;
    private IPEndPoint remoteEP;
    // カメラからの距離
    public float distanceFromCamera = 1.0f;
    // オブジェクトの回転オフセット
    public Quaternion rotationOffset = Quaternion.identity;
    // オブジェクトの横方向オフセット
    public float horizontalOffset = 0.0f;
    public Camera cameraReference;


    private void Start()
    {
        udpClient0 = new UdpClient(1000);  // UDP受信ポート番号を指定する
        //udpClient1 = new UdpClient(1001);  // UDP受信ポート番号を指定する
        remoteEP = new IPEndPoint(IPAddress.Any, 0);
    }
    private void Update()
    {
        if (udpClient0.Available > 0)
        {
            byte[] receivedBytes = udpClient0.Receive(ref remoteEP);
            //byte[] receivedData = udpClient1.Receive(ref remoteEP);
            //Debug.Log(receivedData[0]);
            Texture2D tex = new Texture2D(128, 128, TextureFormat.ARGB32, false);
            tex.LoadImage(receivedBytes);
            material.mainTexture = tex;
        }

        cameraReference = GameObject.Find("Camera_left").GetComponent<Camera>();
        // カメラの位置と回転を取得

        Transform cameraTransform = cameraReference.transform;
        Vector3 cameraPosition = cameraTransform.position;
        Quaternion cameraRotation = cameraTransform.rotation;

        // オブジェクトをカメラの正面に配置
        Transform objectTransform = transform;
        objectTransform.position = cameraPosition + cameraTransform.forward * distanceFromCamera;
        objectTransform.rotation = cameraRotation * rotationOffset;

        // オブジェクトの横方向オフセットを適用
        Vector3 rightVector = Quaternion.Euler(cameraRotation.eulerAngles.x, cameraRotation.eulerAngles.y, cameraRotation.eulerAngles.z) * Vector3.right;
        objectTransform.position += rightVector * horizontalOffset;

    }

    private void OnApplicationQuit()
    {
        udpClient0.Close();
        //udpClient1.Close();
    }

}
*/
/*
private void Update()
{
    List<byte> dataList = new List<byte>();
    if (udpClient1.Available > 0)
    {
        byte[] receivedData = udpClient1.Receive(ref remoteEP);
        while (receivedData[0] == 10)
        {
            byte[] data1 = udpClient0.Receive(ref remoteEP);
            dataList.AddRange(data1);
            if (Input.GetKeyDown("space"))
            {
                break;
            }
        }
        Texture2D tex = new Texture2D(128, 128, TextureFormat.ARGB32, false);
        byte[] concatenatedData = dataList.ToArray();
        tex.LoadImage(concatenatedData);
        material.mainTexture = tex;
        dataList.Clear();
    }

    cameraReference = GameObject.Find("Camera_left").GetComponent<Camera>();
    // カメラの位置と回転を取得

    Transform cameraTransform = cameraReference.transform;
    Vector3 cameraPosition = cameraTransform.position;
    Quaternion cameraRotation = cameraTransform.rotation;

    // オブジェクトをカメラの正面に配置
    Transform objectTransform = transform;
    objectTransform.position = cameraPosition + cameraTransform.forward * distanceFromCamera;
    objectTransform.rotation = cameraRotation * rotationOffset;

    // オブジェクトの横方向オフセットを適用
    Vector3 rightVector = Quaternion.Euler(cameraRotation.eulerAngles.x, cameraRotation.eulerAngles.y, cameraRotation.eulerAngles.z) * Vector3.right;
    objectTransform.position += rightVector * horizontalOffset;

}

private void OnApplicationQuit()
{
    udpClient0.Close();
    udpClient1.Close();
}
    */