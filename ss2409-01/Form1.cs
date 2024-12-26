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
using ss2409_01;

namespace ss2409_01
{
    public partial class Form1 : Form
    {
        private bool _isCameraConnected = false; // カメラ接続フラグ
        private Thread _cameraThread; // カメラ接続スレッド
        private VideoCapture _capture; // カメラキャプチャ
        private readonly Calibration _calibration; // キャリブレーションクラス

        public Form1()
        {
            InitializeComponent();
            InitializeCameraComponents();
            _calibration = new Calibration(this); // キャリブレーションクラスのインスタンスを生成
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);

            // フォームのタイトルとアイコンを設定
            this.Text = "カメラ起動";
            this.Icon = new Icon("C:\\Users\\Owner\\source\\repos\\SkillSemi2024\\ss2409-01\\OIP_result.ico");
        }

        public bool IsCameraConnected => _isCameraConnected; // カメラ接続状態を取得
        public VideoCapture CameraCapture => _capture; // VideoCapture インスタンスを取得

        public void UpdateStatusLabel(string text)
        {
            // ステータスラベルのテキストを更新
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateStatusLabel), text);
            }
            else
            {
                statusLabel.Text = text;
            }
        }

        public void CameraButtonEnabled(bool enabled)
        {
            // カメラ接続ボタンの有効/無効を設定
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(CameraButtonEnabled), enabled);
            }
            else
            {
                cameraButton.Enabled = enabled;
            }
        }

        public void CalibrationButtonEnabled(bool enabled)
        {
            // キャリブレーションボタンの有効/無効を設定
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(CalibrationButtonEnabled), enabled);
            }
            else
            {
                calibrationButton.Enabled = enabled;
            }
        }

        private void InitializeCameraComponents()
        {
            // カメラ候補を取得してコンボボックスに追加
            List<string> cameraList = GetCameraList();
            if (cameraList.Count == 0)
            {
                statusLabel.Text = "カメラが見つかりませんでした．";
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
                statusLabel.Text = "カメラを選択してください.";
                return;
            }

            if (!_isCameraConnected)
            {
                // カメラ接続
                int cameraIndex = cameraComboBox.SelectedIndex;
                _isCameraConnected = true;
                cameraComboBox.Enabled = false; // カメラ接続中はカメラの選択を無効にする

                // カメラ接続スレッドを開始
                _cameraThread = new Thread(() => ConnectCamera(cameraIndex));
                _cameraThread.IsBackground = true;
                _cameraThread.Start();

                cameraButton.Text = "カメラ切断";
                statusLabel.Text = $"{cameraComboBox.SelectedItem}を接続しました";
            }
            else
            {
                // カメラ切断
                DisconnectCamera();
            }
        }

        public void ConnectCamera(int cameraIndex)
        {
            // カメラインデックスの範囲チェック
            if (cameraIndex < 0 || cameraIndex >= cameraComboBox.Items.Count)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    statusLabel.Text = "そのカメラは使用できません．";
                });
                return;
            }

            // カメラ接続
            _capture = new VideoCapture(cameraIndex);

            if (!_capture.IsOpened())
            {
                this.Invoke((MethodInvoker)delegate
                {
                    statusLabel.Text = $"{cameraComboBox.SelectedItem}に接続できませんでした．";
                });
                _isCameraConnected = false;
                this.Invoke((MethodInvoker)delegate
                {
                    cameraComboBox.Enabled = true; // カメラの選択を有効にする
                    cameraButton.Text = "カメラ接続";
                });
                return;
            }

            while (_isCameraConnected)
            {
                // カメラ画像を取得して表示
                using (var frame = new Mat())
                {
                    try
                    {
                        _capture.Read(frame);
                        if (!frame.Empty())
                        {
                            // キャリブレーションパラメータがある場合は画像を補正
                            var image = _calibration.IsCalibrated ? _calibration.UndistortImage(frame) : frame;
                            var bitmap = MatToBitmap(image); // OpenCvSharpのMatをBitmapに変換
                            this.Invoke((MethodInvoker)delegate // UIスレッドで画像を表示
                            {
                                cameraPictureBox.Image = bitmap;
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            statusLabel.Text = $"カメラ画像取得中にエラーが発生しました: {ex.Message}";
                        });
                        DisconnectCamera();
                        return;
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

        public void DisconnectCamera()
        {
            // カメラ切断
            _isCameraConnected = false;

            // キャリブレーションを停止
            _calibration.StopCalibration();

            // カメラ接続スレッドが実行中の場合は終了
            if (_cameraThread != null && _cameraThread.IsAlive)
            {
                _cameraThread.Join(800); // スレッドの終了を待機（タイムアウトを設定）
            }

            // キャプチャが存在する場合は解放
            if (_capture != null) // キャプチャが存在する場合は解放
            {
                _capture.Release();
                _capture.Dispose();
                _capture = null;
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
            if (_isCameraConnected)
            {
                DisconnectCamera();
            }

            // キャリブレーションスレッドが実行中の場合は停止
            _calibration.StopCalibration();
        }

        private void CalibrationButton_Click(object sender, EventArgs e)
        {
            // キャリブレーションをリセット
            _calibration.ResetCalibration();

            // キャリブレーションを実行
            _calibration.PerformCalibration(cameraComboBox.SelectedIndex);
        }
    }
}
