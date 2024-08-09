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

public class UDPTest2 : MonoBehaviour
{
    public Material material;  // Plane�I�u�W�F�N�g�ɃA�^�b�`���ꂽ�}�e���A��
    private UdpClient udpClient0;
    private UdpClient udpClient1;
    private IPEndPoint remoteEP;
    public float distanceFromCamera = 1.0f;     // �J��������̋���
    public Quaternion rotationOffset = Quaternion.identity;    // �I�u�W�F�N�g�̉�]�I�t�Z�b�g
    public float horizontalOffset = 0.0f;       // �I�u�W�F�N�g�̉������I�t�Z�b�g
    public Camera cameraReference;
    private Queue<byte[]> dataQueue = new Queue<byte[]>();
    private byte[] receivedData;
    private byte[] data;
    private object lockObject = new object();
    private bool loop = true;
    private Task dataReceivingTask;
    private CancellationTokenSource cancellationTokenSource;
    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
    private Texture2D reusableTexture; // �ė��p�\�ȃe�N�X�`��
    List<byte> dataList = new List<byte>();


    private void Start()
    {
        // reusableTexture ��������
        reusableTexture = new Texture2D(128, 128, TextureFormat.ARGB32, false);
        material.mainTexture = reusableTexture;

        udpClient0 = new UdpClient(2000);
        //udpClient1 = new UdpClient(2001);
        remoteEP = new IPEndPoint(IPAddress.Any, 0);
        //RunPythonScript();
        //CallPythonFunction("function2", 1);
        StartReceivingData();

        // Application.quitting �C�x���g�ɉ��������o�^
        Application.quitting += OnApplicationQuitting;
    }

    private void RunPythonScript()
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = @"C:\Users\hosei\anaconda3\envs\Py37CV470ARlive\python.exe";// Python�̎��s�t�@�C���ւ̃p�X�����ϐ��ɒǉ�����Ă���ꍇ
        //start.FileName = @"C:\Users\hosei\AppData\Local\Programs\Python\Python37\python.exe";  
        // start.FileName = "/usr/local/bin/python3";  // Python�̎��s�t�@�C���ւ̃p�X�����ϐ��ɒǉ�����Ă��Ȃ��ꍇ
        start.Arguments = @"C:\ARlive\Testcode\Python\Mask.py";  // Python�X�N���v�g�ւ̃p�X���w��
        start.UseShellExecute = false; // �V�F�����g�p�����ɒ��ڎ��s
        start.CreateNoWindow = true; // �E�B���h�E��\�����Ȃ�
        //start.RedirectStandardOutput = true; // �W���o�͂��L���v�`��
        //start.EnvironmentVariables["PYTHONPATH"] = @"C:\Users\hosei\anaconda3\envs\Py37CV470ARlive\Lib\site-packages";

        Process process = new Process();
        process.StartInfo = start;

        process.Start();
    }


    //void CallPythonFunction(string functionName, int arg)
    //{
    //    ProcessStartInfo start = new ProcessStartInfo();

    //    start.FileName = @"C:\Users\hosei\AppData\Local\Programs\Python\Python37\python.exe";  // Python�̃p�X�����ϐ��ɒǉ�����Ă���ꍇ
    //    //start.FileName = @"C:\Users\hosei\anaconda3\envs\Py37CV470ARlive\python.exe";// Python�̎��s�t�@�C���ւ̃p�X�����ϐ��ɒǉ�����Ă���ꍇ
    //    //start.FileName = "/usr/local/bin/python3";  // Python�̃p�X�����ϐ��ɒǉ�����Ă��Ȃ��ꍇ

    //    //start.Arguments = string.Format("{0} {1} {2}", @"C:\ARlive\Mask RCNN UDPtest for Unity\Mask RCNN UDPtest for Unity\Mask_rcnn_cutout_alpha_udp.py", functionName, arg);  // PythonFunctions.py�ւ̃p�X�ƈ���
    //    //start.Arguments = string.Format("{0} {1} {2}", @"C:\ARlive\Mask RCNN UDPtest for Unity\Mask RCNN UDPtest for Unity\abc_1.py", functionName, arg);  // PythonFunctions.py�ւ̃p�X�ƈ���
    //    start.Arguments = string.Format("{0} {1} {2}", @"C:\Users\hosei\Mask.py", functionName, arg);  // PythonFunctions.py�ւ̃p�X�ƈ���

    //    start.UseShellExecute = false;
    //    start.CreateNoWindow = true;
    //    start.RedirectStandardOutput = true;
    //    start.EnvironmentVariables["PYTHONPATH"] = @"C:\Users\hosei\anaconda3\envs\Py37CV470ARlive\Lib\site-packages";

    //    using (Process process = Process.Start(start))
    //    {
    //        using (StreamReader reader = process.StandardOutput)
    //        {
    //            string result = reader.ReadToEnd();
    //            UnityEngine.Debug.Log("10");
    //            UnityEngine.Debug.Log(result);
    //        }
    //    }
    //}

    private void StartReceivingData()
    {
        cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        dataReceivingTask = Task.Run(() =>
        {
            List<byte> dataList = new List<byte>();
            //byte[] data = null;
            //UnityEngine.Debug.Log("0");
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    byte[] data = udpClient0.Receive(ref remoteEP);
                    //// �ŏ��̃o�C�g�� loopnum �Ƃ��ēǂݎ��
                    //byte loopnum = data[0];
                    //// ���̃o�C�g�� num �Ƃ��ēǂݎ��
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
            }
        });
    }

    //private void StartReceivingData()
    //{
    //    cancellationTokenSource = new CancellationTokenSource();
    //    CancellationToken cancellationToken = cancellationTokenSource.Token;

    //    dataReceivingTask = Task.Run(() =>
    //    {
    //        //byte[] loopnum;
    //        List<byte> dataList = new List<byte>();
    //        Debug.Log("0");
    //        while (!cancellationToken.IsCancellationRequested)
    //        {
    //            try
    //            {
    //                for (int r = 1; r <= 1; r++)
    //                //loopnum = udpClient1.Receive(ref remoteEP);
    //                //Debug.Log("Received data: " + string.Join(" ", loopnum));
    //                //for (int i = 0; i < loopnum[0]; i++)
    //                {
    //                    byte[] data = udpClient0.Receive(ref remoteEP);
    //                    dataList.AddRange(data);
    //                    Debug.Log("1");
    //                }
    //                byte[] concatenatedData = dataList.ToArray();

    //                lock (dataQueue)
    //                {
    //                    dataQueue.Enqueue(concatenatedData);
    //                }
    //            }

    //            catch (Exception e)
    //            {
    //                Debug.LogWarning("[Server]" + e);
    //            }
    //            finally
    //            {
    //                dataList.Clear();
    //                Debug.Log("2");
    //            }
    //        }
    //    });
    //}

    private void OnApplicationQuitting()
    {
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
        }
        UnityEngine.Debug.Log("3");
        // �񓯊��X���b�h���I������܂őҋ@
        if (dataReceivingTask != null)
        {
            dataReceivingTask.Wait();
        }
        UnityEngine.Debug.Log("4");
        dataQueue.Clear();
        // �A�v���P�[�V�������I������ۂɃe�N�X�`�������
        if (reusableTexture != null)
        {
            Destroy(reusableTexture);
        }
        udpClient0.Close();
        //udpClient1.Close();
    }
    private void Update()
    {
        // �����Ŕ񓯊����\�b�h���Ăяo��
        //Task.Run(ReceiveAndProcessDataAsync);

        while (loop)
        {

            if (dataQueue.Count > 0)
            {
                byte[] data = dataQueue.Dequeue();
                // �ŏ��̃o�C�g�� loopnum �Ƃ��ēǂݎ��
                byte loopnum = data[0];
                // ���̃o�C�g�� num �Ƃ��ēǂݎ��
                byte dataIndex = data[1];

                //UnityEngine.Debug.Log("Received data: " + loopnum + " " + dataIndex);
                // �ŏ���2�o�C�g���������f�[�^��dataList�ɒǉ�
                dataList.AddRange(data.Skip(2));
                //data = null;
                //UnityEngine.Debug.Log("1");
                if (loopnum == dataIndex)
                {
                    loop = false;
                    UnityEngine.Debug.Log("Right");
                }
                //else if(dataQueue.Count == 0)
                //{
                //    loop = false;
                //    UnityEngine.Debug.Log("2qqqqqqqqqqqqq");
                //}
            }
        }
        byte[] LoadData = dataList.ToArray();

        reusableTexture.LoadImage(LoadData);   // �e�N�X�`���Ƀf�[�^��ǂݍ���

        material.mainTexture = reusableTexture;  // �e�N�X�`����\��
        dataList.Clear();
        loop = true;

        //cameraReference = GameObject.Find("Camera_right").GetComponent<Camera>();
        //// �J�����̈ʒu�Ɖ�]���擾

        //Transform cameraTransform = cameraReference.transform;
        //Vector3 cameraPosition = cameraTransform.position;
        //Quaternion cameraRotation = cameraTransform.rotation;

        //// �I�u�W�F�N�g���J�����̐��ʂɔz�u
        //Transform objectTransform = transform;
        //objectTransform.position = cameraPosition + cameraTransform.forward * distanceFromCamera;
        //objectTransform.rotation = cameraRotation * rotationOffset;

        //// �I�u�W�F�N�g�̉������I�t�Z�b�g��K�p
        //Vector3 rightVector = Quaternion.Euler(cameraRotation.eulerAngles.x, cameraRotation.eulerAngles.y, cameraRotation.eulerAngles.z) * Vector3.right;
        //objectTransform.position += rightVector * horizontalOffset;

    }

    /*
    private void StartReceivingData()
    {
        cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        dataReceivingTask = Task.Run(() =>
        {
            List<byte> dataList = new List<byte>();
            Debug.Log("0");
            //bool stopThread = true;
            while (!cancellationToken.IsCancellationRequested)
            {
                //lock (lockObject)
                //{
                //    stopThread = shouldStopThread;
                //}

                //if (stopThread)
                //{
                //    dataList.Clear();
                //    break; // �X���b�h���I��
                //}

                try
                {
                    for (int r = 1; r <= 3; r++)
                    {
                        byte[] data = udpClient0.Receive(ref remoteEP);
                        dataList.AddRange(data);
                        Debug.Log("1");
                    }
                    byte[] concatenatedData = dataList.ToArray();

                    lock (dataQueue)
                    {
                        dataQueue.Enqueue(concatenatedData);
                    }
                }
                catch (OperationCanceledException)
                {
                    // �L�����Z���v���������ꍇ
                    break; // ���[�v�I��
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[Server]" + e);
                }
                finally
                {
                    dataList.Clear();
                    Debug.Log("2");
                }
            }
        });
    }

    private void StartReceivingData()
    {
        var _ = Task.Run(() =>
        {
            byte[] loopnum;
            List<byte> dataList = new List<byte>();
            while (true)
            {
                Debug.Log("0");
                loopnum = udpClient1.Receive(ref remoteEP);
                Debug.Log("Received data: " + string.Join(" ", loopnum));
                for (int i = 0; i < loopnum[0]; i++)
                {
                    try
                    {
                        byte[] data = udpClient0.Receive(ref remoteEP);
                        dataList.AddRange(data);
                        Debug.Log("2");
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("[Server]" + e);
                    }
                }
                byte[] concatenatedData = dataList.ToArray();

                lock (dataQueue)
                {
                    dataQueue.Enqueue(concatenatedData);
                }
                Debug.Log("3");
                dataList.Clear();
            }
        });
    }
    */
    /*
    private void StartReceivingData()
    {
        var _ = Task.Run(() =>
        {
            byte[] receiveData;
            List<byte> dataList = new List<byte>();
            while (true)
            {
                Debug.Log("0");
                receiveData = udpClient1.Receive(ref remoteEP);
                Debug.Log("Received data: " + string.Join(" ", receiveData));
                if (receiveData[0] == 0)
                {
                    Debug.Log("1");
                    while (true)
                    {
                        Debug.Log("2");
                        try
                        {
                            byte[] data = udpClient0.Receive(ref remoteEP);
                            dataList.AddRange(data);
                            receiveData = udpClient1.Receive(ref remoteEP);
                            Debug.Log("Received data: " + string.Join(" ", receiveData));
                            if (receiveData[0] == 1)
                            {
                                byte[] concatenatedData = dataList.ToArray();
                                lock (dataQueue)
                                {
                                    dataQueue.Enqueue(concatenatedData);
                                }
                                Debug.Log("3");
                                break;
                            }
                        }
                        catch(Exception e)
                        {
                            Debug.LogWarning("[Server]" + e);
                        }
                        Debug.Log("4");
                        dataList.Clear();
                    }
                }
                
            }
        });
    }
    */
    //private async void StartReceivingData()
    //{
    //    //Debug.Log("start");
    //    // �f�[�^�̎�M��񓯊��ŊJ�n
    //    sw.Start();
    //    while (true)
    //        {
    //        if (udpClient1.Available > 0)
    //        {
    //            //List<byte> dataList = new List<byte>();
    //            //Debug.Log("start");
    //            //receivedData = await Task.Run(() => udpClient1.Receive(ref remoteEP));
    //            data = await Task.Run(() => udpClient0.Receive(ref remoteEP));
    //            //receivedData = await Task.Run(() => udpClient1.Receive(ref remoteEP));
    //            //dataList.AddRange(data);
    //            //Debug.Log(receivedData[0]);
    //            lock (dataQueue)
    //            {
    //                dataQueue.Enqueue(data);
    //            }
    //            //if (receivedData[0] != 0)
    //            //{
    //            //    Debug.Log("not");
    //            //}
    //            //dataList.Clear();

    //        }
    //        sw.Stop();
    //        TimeSpan ts1 = sw.Elapsed;
    //        Debug.Log($"�@1��{ts1}");
    //        Debug.Log($"�@1��{sw.ElapsedMilliseconds}�~���b");
    //        sw.Reset();
    //        sw.Start();
    //    }
    //    sw.Stop();
    //}

}

/*
async Task ReceiveAndProcessDataAsync()
{
    try
    {
        List<byte> dataList = new List<byte>();
        if (udpClient1.Available > 0)
        {
            Debug.Log("start");
            byte[] receivedData = await Task.Run(() => udpClient1.Receive(ref remoteEP));
            /*
            while (receivedData[0] == 0)
            {
                byte[] data = udpClient0.Receive(ref remoteEP);
                dataList.AddRange(data);
                Debug.Log(receivedData[0]);
                receivedData = await Task.Run(() => udpClient1.Receive(ref remoteEP));
                /*
                if (Input.GetKeyDown("space"))
                {
                    break;
                }
            }

            byte[] data = udpClient0.Receive(ref remoteEP);
            dataList.AddRange(data);
            Debug.Log(receivedData[0]);
            receivedData = await Task.Run(() => udpClient1.Receive(ref remoteEP));
            if (receivedData[0] != 0)
            {
                Debug.Log("not");
            }

            Debug.Log("2");
            Texture2D tex = new Texture2D(128, 128, TextureFormat.ARGB32, false);
            Debug.Log("3");
            byte[] concatenatedData = dataList.ToArray();
            Debug.Log("4");
            tex.LoadImage(concatenatedData);
            Debug.Log("5");
            material.mainTexture = tex;
            dataList.Clear();
        }
    }
    catch (Exception e)
    {
        Debug.LogError("An error occurred: " + e.Message);
    }
}
*/


/*
private void Update()
{
    if (udpClient0.Available > 0)
    {
        byte[] receivedBytes = udpClient0.Receive(ref remoteEP);
        Texture2D tex = new Texture2D(128, 128, TextureFormat.ARGB32, false);
        tex.LoadImage(receivedBytes);
        material.mainTexture = tex;
    }

    cameraReference = GameObject.Find("Camera_right").GetComponent<Camera>();
    // �J�����̈ʒu�Ɖ�]���擾

    Transform cameraTransform = cameraReference.transform;
    Vector3 cameraPosition = cameraTransform.position;
    Quaternion cameraRotation = cameraTransform.rotation;

    // �I�u�W�F�N�g���J�����̐��ʂɔz�u
    Transform objectTransform = transform;
    objectTransform.position = cameraPosition + cameraTransform.forward * distanceFromCamera;
    objectTransform.rotation = cameraRotation * rotationOffset;

    // �I�u�W�F�N�g�̉������I�t�Z�b�g��K�p
    Vector3 rightVector = Quaternion.Euler(cameraRotation.eulerAngles.x, cameraRotation.eulerAngles.y, cameraRotation.eulerAngles.z) * Vector3.right;
    objectTransform.position += rightVector * horizontalOffset;

}

private void OnApplicationQuit()
{
    udpClient0.Close();
}
*/

