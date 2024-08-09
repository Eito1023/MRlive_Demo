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
    public Material material;  // Plane�I�u�W�F�N�g�ɃA�^�b�`���ꂽ�}�e���A��
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
    private Texture2D reusableTexture; // �ė��p�\�ȃe�N�X�`��
    List<byte> dataList = new List<byte>();


    private void Start()
    {
        // reusableTexture ��������
        reusableTexture = new Texture2D(128, 128, TextureFormat.ARGB32, false);
        material.mainTexture = reusableTexture;

        udpClient0 = new UdpClient(1000);
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
        start.Arguments = @"C:\Users\hosei\Mask.py";  // Python�X�N���v�g�ւ̃p�X���w��
        start.UseShellExecute = false; // �V�F�����g�p�����ɒ��ڎ��s
        start.CreateNoWindow = true; // �E�B���h�E��\�����Ȃ�
        //start.RedirectStandardOutput = true; // �W���o�͂��L���v�`��
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
            //byte[] dataall = dataQueue.Dequeue();
            //Texture2D tex = new Texture2D(128, 128, TextureFormat.ARGB32, false);
            //tex.LoadImage(dataall);
            ////�O�̃e�N�X�`�������
            //if (material.mainTexture != null)
            //{
            //    Destroy(material.mainTexture);
            //}
            //material.mainTexture = tex;
            if (dataQueue.Count > 0)
            {
                byte[] data = dataQueue.Dequeue();
                // �ŏ��̃o�C�g�� loopnum �Ƃ��ēǂݎ��
                byte loopnum = data[0];
                // ���̃o�C�g�� num �Ƃ��ēǂݎ��
                byte dataIndex = data[1];
                // ���̎��̃o�C�g�� num �Ƃ��ēǂݎ��

                //UnityEngine.Debug.Log("Received data: " + loopnum + " " + dataIndex);
                // �ŏ���2�o�C�g���������f�[�^��dataList�ɒǉ�
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
        // �e�N�X�`���Ƀf�[�^��ǂݍ���
        byte[] LoadData = dataList.ToArray();
        reusableTexture.LoadImage(LoadData);

        // �O�̃e�N�X�`������������ɍė��p

        // �e�N�X�`����\��
        material.mainTexture = reusableTexture;
        dataList.Clear();
        loop = true;
    }
}
