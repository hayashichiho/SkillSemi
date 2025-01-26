using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using NetMQ;
using NetMQ.Sockets;

namespace connectDifferentLanguage
{
    public partial class Form1 : Form
    {
        private Thread receiveThread; // 受信スレッド
        private bool isRunning; // スレッドの実行状態を管理するフラグ
        private Process cppProcess; // C++ プログラムのプロセス
        private Process pythonProcess; // Python スクリプトのプロセス
        private PushSocket controlSocketCpp; // C++制御用ソケット
        private PushSocket controlSocketPython; // Python制御用ソケット
        private PullSocket receiveSocket; // 画像受信用ソケット
        private bool isSocketBound; // ソケットのバインド状態を管理するフラグ

        // コンストラクタ
        public Form1()
        {
            // アプリのタイトルとアイコンを設定
            this.Icon = new Icon("C:\\Users\\Owner\\source\\repos\\SkillSemi2024\\ss2409-01\\OIP_result.ico");
            this.Text = "cameraApp";

            InitializeComponent();

            // ソケットの初期化
            InitializeSockets();

            // C++とPythonプログラムを開始
            StartCppProgram();
            Console.WriteLine("C++プログラムを開始しました。");

            StartPythonScript();
            Console.WriteLine("Pythonスクリプトを開始しました。");

            // フラグとボタンの初期化
            isRunning = false;
            endButton.Enabled = false;

            // フォームの閉じるイベントを設定
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
        }

        // ソケットの初期化
        private void InitializeSockets()
        {
            controlSocketCpp = new PushSocket();
            controlSocketCpp.Connect("tcp://localhost:5557");

            controlSocketPython = new PushSocket();
            controlSocketPython.Connect("tcp://localhost:5558");

            receiveSocket = new PullSocket();
            isSocketBound = false;
        }

        // 画像受信スレッドの開始
        private void StartReceiveThread()
        {
            isRunning = true;
            receiveThread = new Thread(ReceiveImage);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        // 画像を受信するメソッド
        private void ReceiveImage()
        {
            try
            {
                receiveSocket.Bind("tcp://*:5556");
                isSocketBound = true;
                Console.WriteLine($"画像受信を開始しました。isRunning = {isRunning}");

                while (isRunning)
                {
                    try
                    {
                        // 画像情報を受信
                        var jpegData = receiveSocket.ReceiveFrameBytes();

                        // データの検証
                        if (jpegData == null || jpegData.Length == 0)
                        {
                            throw new ArgumentException("受信したデータが無効です。");
                        }

                        // JPEG形式の画像データを再構築
                        Bitmap image;
                        using (var ms = new MemoryStream(jpegData))
                        {
                            image = new Bitmap(ms);
                        }

                        // 画像を表示
                        this.Invoke((MethodInvoker)delegate
                        {
                            pictureBox.Image = image;
                        });
                    }
                    catch (Exception ex)
                    {
                        if (isRunning) // フォームが閉じられていない場合のみエラーメッセージを表示
                        {
                            MessageBox.Show($"エラーが発生しました: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!isSocketBound)
                {
                    MessageBox.Show($"ソケットのバインドに失敗しました: {ex.Message}");
                }
                else
                {
                    Console.WriteLine($"ソケットのバインドに成功しましたが、例外が発生しました: {ex.Message}");
                }
            }
        }

        // 開始ボタンがクリックされたときの処理
        private void startButton_Click(object sender, EventArgs e)
        {
            SendStartMessageCpp(); // c++に画像を送信するよう要求
            StartReceiveThread(); // 画像受信スレッドの開始

            startButton.Enabled = false;
            endButton.Enabled = true;
        }

        // C++ プログラムを開始するメソッド
        private void StartCppProgram()
        {
            cppProcess = new Process();
            cppProcess.StartInfo.FileName = "C:\\Users\\Owner\\source\\repos\\SkillSemi2024\\ss2411-1\\ss2411-1\\build\\Debug\\ss2411-1.exe"; // C++ プログラムのパスを指定
            cppProcess.StartInfo.UseShellExecute = false;
            cppProcess.StartInfo.CreateNoWindow = true;
            cppProcess.StartInfo.RedirectStandardOutput = true; // 標準出力をリダイレクト
            cppProcess.OutputDataReceived += new DataReceivedEventHandler(CppOutputHandler);
            cppProcess.Start();
            cppProcess.BeginOutputReadLine(); // 非同期で標準出力を読み取る
        }

        // C++ プログラムの標準出力を処理するハンドラ
        private void CppOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                // 標準出力をコンソールに表示
                Console.WriteLine($"C++: {outLine.Data}");
            }
        }

        // Python スクリプトを開始するメソッド
        private void StartPythonScript()
        {
            pythonProcess = new Process();
            pythonProcess.StartInfo.FileName = "python";
            pythonProcess.StartInfo.Arguments = "C:\\Users\\Owner\\source\\repos\\SkillSemi2024\\ss2411-1\\ss2411-1\\receiveCamera.py"; // Python スクリプトのパスを指定
            pythonProcess.StartInfo.UseShellExecute = false;
            pythonProcess.StartInfo.CreateNoWindow = true;
            pythonProcess.StartInfo.RedirectStandardOutput = true; // 標準出力をリダイレクト
            pythonProcess.OutputDataReceived += new DataReceivedEventHandler(pythonOutputHandler);
            pythonProcess.Start();
            pythonProcess.BeginOutputReadLine(); // 非同期で標準出力を読み取る
        }

        // Python プログラムの標準出力を処理するハンドラ
        private void pythonOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                // 標準出力をコンソールに表示
                Console.WriteLine($"python: {outLine.Data}");
            }
        }

        // 終了ボタンがクリックされたときの処理
        private void endButton_Click(object sender, EventArgs e)
        {
            SendStopMessageCpp(); // C++に画像送信をしないフラグの送信

            isRunning = false;
            startButton.Enabled = true;
            endButton.Enabled = false;
        }

        // C++に撮影開始を通知するメソッド
        private void SendStartMessageCpp()
        {
            int command = 1; // start
            controlSocketCpp.SendFrame(BitConverter.GetBytes(command)); // 撮影開始を通知
        }

        // C++に撮影中断を通知するメソッド
        private void SendStopMessageCpp()
        {
            int command = 0; // stop
            controlSocketCpp.SendFrame(BitConverter.GetBytes(command)); // 撮影中断を通知
        }

        // C++に終了を通知するメソッド
        private void SendExitMessageCpp()
        {
            int command = -1; // exit
            controlSocketCpp.SendFrame(BitConverter.GetBytes(command));
            Console.WriteLine("C++に終了を通知しました。");
        }

        // pythonに終了を通知するメソッド
        private void SendExitMessagePython()
        {
            controlSocketPython.SendFrame("exit");
            Console.WriteLine("Pythonに終了を通知しました。");
        }

        // フォームが閉じられるときの処理
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Console.WriteLine("フォームが閉じられました。");

            // C++とPythonの終了
            SendExitMessageCpp();
            SendExitMessagePython();

            isRunning = false;

            // 画像受信スレッドの終了
            if (receiveThread != null && receiveThread.IsAlive)
            {
                receiveThread.Join(); // スレッドが終了するのを待つ
            }

            // ソケットのクローズ
            controlSocketCpp.Close();
            controlSocketPython.Close();
            receiveSocket.Close();
        }
    }
}


