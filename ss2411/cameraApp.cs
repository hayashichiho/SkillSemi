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
    public partial class cameraApp : Form
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
        public cameraApp()
        {
            // アプリのタイトルとアイコンを設定
            this.Icon = new Icon("C:\\Users\\Owner\\source\\repos\\SkillSemi2024\\ss2409-01\\OIP_result.ico");
            InitializeComponent();
            this.Text = "cameraApp";

            // ソケットの初期化
            InitializeSockets();

            // カメラの候補を取得
            LoadCameraList();

            // C++とPythonプログラムを開始
            StartCppProgram();
            Console.WriteLine("C++プログラムを開始しました。");

            StartPythonScript();
            Console.WriteLine("Pythonスクリプトを開始しました。");

            // フラグとボタンの初期化
            isRunning = false;
            endButton.Enabled = false;

            // 画像受信スレッドの開始
            StartReceiveThread();

            // フォームの閉じるイベントを設定
            this.FormClosing += new FormClosingEventHandler(cameraApp_FormClosing);
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

        // カメラの候補を取得するメソッド
        public void LoadCameraList()
        {
            // カメラの候補を取得
            string[] cameraList = new string[] { "camera 0", "dummy" };

            // カメラの候補をコンボボックスに追加
            ComboBox.Items.AddRange(cameraList);
        }

        // 画像受信スレッドの開始
        private void StartReceiveThread()
        {
            isRunning = true;
            receiveThread = new Thread(ReceiveImage);
            receiveThread.IsBackground = true;
            receiveThread.Start();
            Console.WriteLine("画像受信スレッドを開始しました。");
        }

        // 画像を受信するメソッド
        private void ReceiveImage()
        {
            try
            {
                receiveSocket.Bind("tcp://*:5556");
                isSocketBound = true;
                Console.WriteLine($"画像受信を開始しました。isRunning = {isRunning}");
                int count = 0;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                // ラベル1の表示
                SetLabel1Text("fps: 0.00");

                while (isRunning)
                {
                    try
                    {
                        // カメラ接続状態と画像データを受信
                        var message = receiveSocket.ReceiveMultipartMessage();
                        bool cameraConnected = BitConverter.ToBoolean(message[0].Buffer, 0);

                        if (!cameraConnected)
                        {
                            MessageBox.Show("カメラが接続されていません。");
                            continue;
                        }

                        // 画像情報を受信
                        var jpegData = message[1].Buffer;

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

                        Console.WriteLine("画像を受信しました。");

                        // 画像を表示
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            pictureBox.Image = image;
                        });

                        count++;

                        if (count % 10 == 0)
                        {
                            // label1にfpsを表示
                            stopwatch.Stop();
                            double fps = 1000.0 / (stopwatch.ElapsedMilliseconds / 10.0);
                            SetLabel1Text($"fps: {fps:F2}");
                            stopwatch.Reset();
                            stopwatch.Start();
                        }
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
                Console.WriteLine($"画像受信スレッドが終了しました。");
            }
        }

        // C++ プログラムを開始するメソッド
        private void StartCppProgram()
        {
            cppProcess = new Process();
            cppProcess.StartInfo.FileName = "C:\\Users\\Owner\\source\\repos\\SkillSemi2024\\ss2411-1\\ss2411-1\\build\\Debug\\ss2411-1.exe"; // C++プログラムのパスを指定
            cppProcess.StartInfo.UseShellExecute = false; // シェルを使わない
            cppProcess.StartInfo.CreateNoWindow = true; // コンソールウィンドウを表示しない
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
            pythonProcess.StartInfo.Arguments = "-u C:\\Users\\Owner\\source\\repos\\SkillSemi2024\\ss2411-1\\ss2411-1\\receiveCamera.py"; // Pythonスクリプトのパスを指定し、バッファリングを無効にするために-uオプションを追加
            pythonProcess.StartInfo.UseShellExecute = false; // シェルを使わない
            pythonProcess.StartInfo.CreateNoWindow = true; // コンソールウィンドウを表示しない
            pythonProcess.StartInfo.RedirectStandardOutput = true; // 標準出力をリダイレクト
            pythonProcess.StartInfo.RedirectStandardError = true; // 標準エラー出力をリダイレクト
            pythonProcess.OutputDataReceived += new DataReceivedEventHandler(PythonOutputHandler);
            pythonProcess.ErrorDataReceived += new DataReceivedEventHandler(PythonOutputHandler);
            pythonProcess.Start();
            pythonProcess.BeginOutputReadLine(); // 非同期で標準出力を読み取る
            pythonProcess.BeginErrorReadLine(); // 非同期で標準エラー出力を読み取る
        }

        // Python プログラムの標準出力を処理するハンドラ
        private void PythonOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                // 標準出力をコンソールに表示
                Console.WriteLine($"Python: {outLine.Data}");
            }
        }

        // 開始ボタンがクリックされたときの処理
        private void startButton_Click(object sender, EventArgs e)
        {
            // 選択されたカメラを取得
            try
            {
                string selectedCamera = ComboBox.SelectedItem.ToString();
                Console.WriteLine($"選択されたカメラ: {selectedCamera}");

                SendStartMessageCpp(selectedCamera); // c++に画像を送信するよう要求

                // ボタンの状態を変更
                startButton.Enabled = false;
                endButton.Enabled = true;
                ComboBox.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"カメラを選択してください");
                return;
            }
        }

        // 終了ボタンがクリックされたときの処理
        private void endButton_Click(object sender, EventArgs e)
        {
            SendStopMessageCpp(); // C++に画像送信をしないフラグの送信

            // フラグとボタンの状態を変更
            startButton.Enabled = true;
            endButton.Enabled = false;
            ComboBox.Enabled = true;
        }

        // C++に撮影開始を通知するメソッド
        private void SendStartMessageCpp(string selectCamera)
        {
            int command = 1; // start
            controlSocketCpp.SendFrame(BitConverter.GetBytes(command)); // 撮影開始を通知
            controlSocketCpp.SendFrame(selectCamera); // 選択されたカメラを送信
            Console.WriteLine($"C++に撮影開始を通知しました。 isRunning = {isRunning}, selectCamera = {selectCamera}");
        }

        // C++に撮影中断を通知するメソッド
        private void SendStopMessageCpp()
        {
            int command = 0; // stop
            controlSocketCpp.SendFrame(BitConverter.GetBytes(command)); // 撮影中断を通知
            Console.WriteLine("C++に撮影中断を通知しました。");
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

        // label1のテキストを変更するメソッド
        public void SetLabel1Text(string text)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                label1.Text = text;
            });
        }

        // フォームが閉じられるときの処理
        private void cameraApp_FormClosing(object sender, FormClosingEventArgs e)
        {
            Console.WriteLine("フォームが閉じられました。");

            if (endButton.Enabled == false)
            {
                endButton_Click(sender, e); // 終了ボタンが押されていない場合は終了処理を実行
            }
            isRunning = false;

            // C++とPythonの終了
            try
            {
                Console.WriteLine("C++プログラムとPythonスクリプトを終了します。");
                SendExitMessageCpp();
                SendExitMessagePython();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"C++とPython終了処理中にエラーが発生しました: {ex.Message}");
            }

            // 画像受信スレッドの終了
            if (receiveThread != null && receiveThread.IsAlive)
            {
                Console.WriteLine("画像受信スレッドを終了します。");
                receiveThread.Join(1000); // スレッドが終了するのを待つ
                if (receiveThread.IsAlive)
                {
                    receiveThread.Abort(); // スレッドを強制終了
                }
            }

            // ソケットのクローズ
            controlSocketCpp.Close();
            controlSocketPython.Close();
            receiveSocket.Close();
        }
    }
}
