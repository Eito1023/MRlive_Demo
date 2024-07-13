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
    public Material material_left;  // Plane�I�u�W�F�N�g�ɃA�^�b�`���ꂽ�}�e���A��
    public Material material_right;  // Plane�I�u�W�F�N�g�ɃA�^�b�`���ꂽ�}�e���A��
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
    private Texture2D reusableTexture_left; // �ė��p�\�ȃe�N�X�`��
    private Texture2D reusableTexture_right; // �ė��p�\�ȃe�N�X�`��

    private void Start()
    {
        // reusableTexture ��������
        reusableTexture_left = new Texture2D(128, 128, TextureFormat.ARGB32, false);
        material_left.mainTexture = reusableTexture_left;
        // reusableTexture ��������
        reusableTexture_right = new Texture2D(128, 128, TextureFormat.ARGB32, false);
        material_right.mainTexture = reusableTexture_right;

        udpClient0 = new UdpClient(1000);
        //udpClient1 = new UdpClient(2001);
        remoteEP = new IPEndPoint(IPAddress.Any, 0);
        RunPythonScript();
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
                            // �ŏ��̃o�C�g�� loopnum �Ƃ��ēǂݎ��
                            byte loopnum_left = data[0];
                            // ���̃o�C�g�� num �Ƃ��ēǂݎ��
                            byte dataIndex_left = data[1];

                            UnityEngine.Debug.Log("Received data: " + loopnum_left + " " + dataIndex_left);

                            // �ŏ���2�o�C�g���������f�[�^��dataList�ɒǉ�
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
                            // �ŏ��̃o�C�g�� loopnum �Ƃ��ēǂݎ��
                            byte loopnum_right = data[0];
                            // ���̃o�C�g�� num �Ƃ��ēǂݎ��
                            byte dataIndex_right = data[1];

                            UnityEngine.Debug.Log("Received data: " + loopnum_right + " " + dataIndex_right);

                            // �ŏ���2�o�C�g���������f�[�^��dataList�ɒǉ�
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
        // �񓯊��X���b�h���I������܂őҋ@
        if (dataReceivingTask != null)
        {
            dataReceivingTask.Wait();
        }
        UnityEngine.Debug.Log("4");
        dataQueue.Clear();
        // �A�v���P�[�V�������I������ۂɃe�N�X�`�������
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
        // �����Ŕ񓯊����\�b�h���Ăяo��
        //Task.Run(ReceiveAndProcessDataAsync);

        lock (dataQueue)
        {
            while (dataQueue.Count > 0)
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

                byte[] dataall_left = dataQueue.Dequeue();
                byte[] dataall_right = dataQueue.Dequeue();

                // �e�N�X�`���Ƀf�[�^��ǂݍ���
                reusableTexture_left.LoadImage(dataall_left);
                reusableTexture_right.LoadImage(dataall_right);

                // �O�̃e�N�X�`������������ɍė��p

                // �e�N�X�`����\��
                material_left.mainTexture = reusableTexture_left;
                material_right.mainTexture = reusableTexture_right;
            }
        }
    }
}
