using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using OpenCvSharp;

namespace ss2409_01
{
    public partial class Form1 : Form
    {
        private bool isCameraConnected = false; // カメラ接続フラグ
        private Thread cameraThread; // カメラ接続スレッド
        private VideoCapture capture; // カメラキャプチャ

        public Form1()
        {
            InitializeComponent();
            InitializeCameraComponents();
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
        }

        private void InitializeCameraComponents()
        {
            // カメラ候補を取得してコンボボックスに追加
            List<string> cameraList = GetCameraList();
            if (cameraList.Count == 0)
            {
                MessageBox.Show("カメラが見つかりませんでした。");
            }
            else
            {
                cameraComboBox.Items.AddRange(cameraList.ToArray());
            }
        }

        private List<string> GetCameraList()
        {
            // カメラ候補を取得
            List<string> cameraList = new List<string>();

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    using (var tempCapture = new VideoCapture(i))
                    {
                        if (tempCapture.IsOpened())
                        {
                            cameraList.Add($"Camera {i}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // カメラが見つからない場合は例外を無視
                    Console.WriteLine($"カメラ{i}が見つかりませんでした: {ex.Message}");
                }
            }
            cameraList.Add("Dummy Camera"); // ダミーのカメラを追加
            return cameraList;
        }

        private void CameraButton_Click(object sender, EventArgs e)
        {
            // カメラが選択されていない場合はエラーメッセージを表示
            if (cameraComboBox.SelectedItem == null)
            {
                MessageBox.Show("カメラを選択してください.");
                return;
            }

            if (!isCameraConnected)
            {
                // カメラ接続
                int cameraIndex = cameraComboBox.SelectedIndex;
                isCameraConnected = true;
                cameraComboBox.Enabled = false; // カメラ接続中はカメラの選択を無効にする

                // カメラ接続スレッドを開始
                cameraThread = new Thread(() => ConnectCamera(cameraIndex));
                cameraThread.IsBackground = true;
                cameraThread.Start();

                cameraButton.Text = "カメラ切断";
                statusLabel.Text = $"{cameraComboBox.SelectedItem}を接続しました";
            }
            else
            {
                // カメラ切断
                DisconnectCamera();
            }
        }

        private void ConnectCamera(int cameraIndex)
        {
            // カメラ接続
            capture = new VideoCapture(cameraIndex);

            if (!capture.IsOpened())
            {
                this.Invoke((MethodInvoker)delegate
                {
                    statusLabel.Text = $"{cameraComboBox.SelectedItem}に接続できませんでした。";
                });
                return;
            }

            while (isCameraConnected)
            {
                // カメラ画像を取得して表示
                using (var frame = new Mat())
                {
                    capture.Read(frame);
                    if (!frame.Empty())
                    {
                        var image = MatToBitmap(frame); // OpenCvSharpのMatをBitmapに変換
                        this.Invoke((MethodInvoker)delegate // UIスレッドで画像を表示
                        {
                            cameraPictureBox.Image = image;
                        });
                    }
                }
                Thread.Sleep(30); // 30msの遅延を追加してフレームレートを調整
            }
        }

        private Bitmap MatToBitmap(Mat mat)
        {
            // OpenCvSharpのMatをBitmapに変換
            using (var ms = mat.ToMemoryStream())
            {
                return new Bitmap(ms);
            }
        }

        private void DisconnectCamera()
        {
            // カメラ切断
            isCameraConnected = false;

            if (cameraThread != null && cameraThread.IsAlive)
            {
                cameraThread.Join(1000); // スレッドの終了を待機（タイムアウトを設定）
            }

            if (capture != null) // キャプチャが存在する場合は解放
            {
                capture.Release();
                capture.Dispose();
                capture = null;
            }

            this.Invoke((MethodInvoker)delegate // UIスレッドでカメラ切断を反映
            {
                cameraButton.Text = "カメラ接続";
                statusLabel.Text = "カメラが接続されていません";
                cameraPictureBox.Image = null;
                cameraComboBox.Enabled = true; // カメラの選択を有効にする
            });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // フォームが閉じられる前にカメラ接続を切断
            if (isCameraConnected)
            {
                DisconnectCamera();
            }
        }

        private void cameraPictureBox_Click(object sender, EventArgs e)
        {

        }
    }
}