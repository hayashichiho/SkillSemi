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
using OpenCvSharp.Extensions;
using ss2409_01;
using System.Runtime.InteropServices;
using System.Xml;

namespace ss2409_01
{
    public partial class Form1 : Form
    {
        private bool _isCameraConnectedPage1; // カメラ接続フラグ（ページ1）
        private bool _isCameraConnectedPage2; // カメラ接続フラグ（ページ2）
        private bool _isCameraRunning; // カメラ実行フラグ
        private Thread _cameraThread; // カメラスレッド
        private VideoCapture _capture; // カメラキャプチャ
        private readonly Calibration _calibration; // キャリブレーションインスタンス
        private readonly LoadData _loadData; // データ読み込みインスタンス
        private readonly Measure _measure; // 計測インスタンス
        private readonly Aruco _aruco; // ArUcoインスタンス
        private Mat _savedCameraMatrix; // 保存されたカメラ行列
        private Mat _savedDistCoeffs; // 保存された歪み係数

        public Form1()
        {
            // コンストラクタ
            InitializeComponent();
            InitializeCameraComponents();
            _calibration = new Calibration(this);
            _loadData = new LoadData(this);
            _measure = new Measure(this);
            _aruco = new Aruco(this);
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);

            _isCameraConnectedPage1 = false; // カメラ接続フラグをオフにする（ページ1）
            _isCameraConnectedPage2 = false; // カメラ接続フラグをオフにする（ページ2）
            _isCameraRunning = false; // カメラ実行フラグをオフにする

            // フォームのタイトルとアイコンを設定
            this.Text = "カメラ起動";
            this.Icon = new Icon("C:\\Users\\Owner\\source\\repos\\SkillSemi2024\\ss2409-01\\OIP_result.ico");

            calibrationButton.Enabled = false; // キャリブレーションボタンを無効にする
            LoadButtonEnabled(false); // ロードボタンを無効にする
            measureButton.Enabled = false; // 計測ボタンを無効にする
        }

        // カメラ接続状態のプロパティ
        public bool IsCameraConnectedPage1 => _isCameraConnectedPage1;
        public bool IsCameraConnectedPage2 => _isCameraConnectedPage2;

        // カメラキャプチャのプロパティ
        public VideoCapture CameraCapture => _capture;

        // キャリブレーションプロパティ
        public Calibration Calibration => _calibration;

        // 計測のプロパティ
        public Measure Measure => _measure;

        // arucoマーカーについてのプロパティ
        public Aruco Aruco => _aruco;

        // ステータスラベル1を更新するメソッド
        public void UpdateStatusLabel1(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateStatusLabel1), text);
            }
            else
            {
                statusLabel1.Text = text;
            }
        }

        // ステータスラベル2を更新するメソッド
        public void UpdateStatusLabel2(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateStatusLabel2), text);
            }
            else
            {
                statusLabel2.Text = text;
            }
        }

        // カメラを切断するメソッド
        public void DisconnectCamera()
        {
            // デバッグ情報を追加
            Console.WriteLine("DisconnectCamera メソッドが呼び出されました");
            Console.WriteLine($"_isCameraConnectedPage1: {_isCameraConnectedPage1}");
            Console.WriteLine($"_isCameraConnectedPage2: {_isCameraConnectedPage2}");

            // カメラが接続されている場合は切断する
            if (_isCameraConnectedPage1 || _isCameraConnectedPage2)
            {
                // カメラ実行フラグをオフにする
                _isCameraRunning = false;

                // スレッドの終了処理を行う
                if (_cameraThread != null && _cameraThread.IsAlive)
                {
                    // デバッグ情報を追加
                    Console.WriteLine("カメラスレッドが実行中です。スレッドを終了します。");
                    _cameraThread.Join(1000); // スレッドが終了するまで待機
                    Console.WriteLine("カメラスレッドが終了しました。");
                }

                // デバッグ情報を追加
                Console.WriteLine($"_capture is null: {_capture == null}");
                Console.WriteLine($"_capture is disposed: {_capture?.IsDisposed}");

                // カメラキャプチャが解放されていない場合は解放する
                lock (this)
                {
                    if (_capture != null && !_capture.IsDisposed)
                    {
                        _capture.Release();
                        _capture.Dispose();
                        _capture = null;
                        Console.WriteLine("カメラキャプチャが解放されました。");
                    }
                }

                // UIコントロールの状態を更新
                this.Invoke((MethodInvoker)delegate
                {
                    cameraComboBox1.Enabled = true;
                    cameraComboBox2.Enabled = true;
                    calibrationButton.Enabled = false;
                    cameraButton1.Text = "カメラ接続";
                    cameraButton2.Text = "カメラ接続";
                    statusLabel1.Text = "カメラを切断しました";
                    statusLabel2.Text = "カメラを切断しました";
                    CameraButton1Enabled(true);
                    CameraButton2Enabled(true);
                    LoadButtonEnabled(false);
                    MeasureButtonEnabled(false);
                    cameraPictureBox1.Image = null;
                    cameraPictureBox2.Image = null;
                    Console.WriteLine("UIコントロールの状態が更新されました。");
                });

                _isCameraConnectedPage1 = false; // カメラ接続フラグをオフにする（ページ1）
                _isCameraConnectedPage2 = false; // カメラ接続フラグをオフにする（ページ2）
                Console.WriteLine("カメラ接続フラグがオフにされました。");
            }
        }

        // カメラ接続ボタン1の有効/無効を設定するメソッド
        public void CameraButton1Enabled(bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(CameraButton1Enabled), enabled);
            }
            else
            {
                cameraButton1.Enabled = enabled;
            }
        }

        // カメラ接続ボタン2の有効/無効を設定するメソッド
        public void CameraButton2Enabled(bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(CameraButton2Enabled), enabled);
            }
            else
            {
                cameraButton2.Enabled = enabled;
            }
        }

        // キャリブレーションボタンの有効/無効を設定するメソッド
        public void CalibrationButtonEnabled(bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(CalibrationButtonEnabled), enabled);
            }
            else
            {
                calibrationButton.Enabled = enabled;
            }
        }

        // ロードボタンの有効/無効を設定するメソッド
        public void LoadButtonEnabled(bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(LoadButtonEnabled), enabled);
            }
            else
            {
                loadButton.Enabled = enabled;
            }
        }

        // 計測ボタンの有効/無効を設定するメソッド
        public void MeasureButtonEnabled(bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(MeasureButtonEnabled), enabled);
            }
            else
            {
                measureButton.Enabled = enabled;
            }
        }

        // カメラのリストを取得するメソッド
        private void InitializeCameraComponents()
        {
            var cameraThread = new Thread(() =>
            {
                var cameraList = GetCameraList();
                this.Invoke((MethodInvoker)delegate
                {
                    if (cameraList.Count == 0)
                    {
                        statusLabel1.Text = "カメラが見つかりませんでした．";
                        statusLabel2.Text = "カメラが見つかりませんでした．";
                    }
                    else
                    {
                        cameraComboBox1.Items.AddRange(cameraList.ToArray());
                        cameraComboBox2.Items.AddRange(cameraList.ToArray());
                    }
                });
            });
            cameraThread.Start();
        }

        // 利用可能なカメラのリストを取得するメソッド
        private List<string> GetCameraList()
        {
            var cameraList = new List<string>();

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
                    Console.WriteLine($"カメラ{i}が見つかりませんでした: {ex.Message}");
                }
            }

            cameraList.Add("Dummy Camera"); // ダミーカメラを追加
            return cameraList;
        }

        // カメラ接続ボタン1のクリックイベントハンドラ
        private void CameraButton1_Click(object sender, EventArgs e)
        {
            if (_isCameraConnectedPage1 || _isCameraConnectedPage2)
            {
                DisconnectCamera();
                return;
            }

            var cameraIndex = cameraComboBox1.SelectedIndex;
            if (cameraIndex == -1)
            {
                UpdateStatusLabel1("カメラを選択してください.");
                UpdateStatusLabel2("カメラを選択してください.");
                return;
            }
            var connectThread = new Thread(() => ConnectCamera(cameraIndex, 1));
            connectThread.Start();
        }

        // カメラ接続ボタン2のクリックイベントハンドラ
        private void CameraButton2_Click(object sender, EventArgs e)
        {
            if (_isCameraConnectedPage1 || _isCameraConnectedPage2)
            {
                DisconnectCamera();
                return;
            }

            var cameraIndex = cameraComboBox2.SelectedIndex;
            if (cameraIndex == -1)
            {
                UpdateStatusLabel1("カメラを選択してください.");
                UpdateStatusLabel2("カメラを選択してください.");
                return;
            }
            var connectThread = new Thread(() => ConnectCamera(cameraIndex, 2));
            connectThread.Start();
        }

        // カメラ接続処理を行うメソッド
        public void ConnectCamera(int cameraIndex, int buttonNumber)
        {
            string selectedCamera = null;
            this.Invoke((MethodInvoker)delegate
            {
                selectedCamera = buttonNumber == 1 ? cameraComboBox1.SelectedItem?.ToString() : cameraComboBox2.SelectedItem?.ToString();
            });

            if (string.IsNullOrEmpty(selectedCamera))
            {
                this.Invoke((MethodInvoker)delegate
                {
                    statusLabel1.Text = "カメラを選択してください.";
                    statusLabel2.Text = "カメラを選択してください.";
                });
                return;
            }

            if (_isCameraConnectedPage1 || _isCameraConnectedPage2)
            {
                // デバッグ情報を追加
                Console.WriteLine("カメラが接続されているため、DisconnectCamera メソッドを呼び出します");

                // カメラが接続されている場合は切断する
                this.Invoke((MethodInvoker)delegate
                {
                    DisconnectCamera();
                });
            }

            // カメラを接続する
            if (buttonNumber == 1)
            {
                _isCameraConnectedPage1 = true;
            }
            else
            {
                _isCameraConnectedPage2 = true;
            }
            _isCameraRunning = true; // カメラ実行フラグをオンにする

            this.Invoke((MethodInvoker)delegate
            {
                cameraComboBox1.Enabled = false;
                cameraComboBox2.Enabled = false;
                cameraButton1.Text = "カメラ切断";
                cameraButton2.Text = "カメラ切断";
                calibrationButton.Enabled = true;
                LoadButtonEnabled(true);
                MeasureButtonEnabled(true);
            });

            // キャリブレーションデータを適用
            if (_savedCameraMatrix != null && _savedDistCoeffs != null)
            {
                _calibration.SetCalibrationData(_savedCameraMatrix, _savedDistCoeffs);
            }

            // カメラキャプチャスレッドを開始
            _cameraThread = new Thread(() => CameraCaptureProcess(cameraIndex))
            {
                IsBackground = true
            };
            _cameraThread.Start();

            this.Invoke((MethodInvoker)delegate
            {
                statusLabel1.Text = $"{selectedCamera}を接続しました";
                statusLabel2.Text = $"{selectedCamera}を接続しました";
            });
        }

        // カメラキャプチャ処理を行うメソッド
        private void CameraCaptureProcess(int cameraIndex)
        {
            try
            {
                using (var capture = new VideoCapture(cameraIndex))
                {
                    if (!capture.IsOpened())
                    {
                        // カメラに接続できなかった場合はエラーメッセージを表示して処理を終了
                        SafeInvoke(() =>
                        {
                            this.UpdateStatusLabel1("カメラに接続できませんでした．");
                            this.UpdateStatusLabel2("カメラに接続できませんでした．");
                            cameraComboBox1.Enabled = true;
                            cameraComboBox2.Enabled = true;
                            calibrationButton.Enabled = false;
                            LoadButtonEnabled(false);
                            MeasureButtonEnabled(false);
                            cameraButton1.Text = "カメラ接続";
                            cameraButton2.Text = "カメラ接続";
                        });

                        _isCameraConnectedPage1 = false;
                        _isCameraConnectedPage2 = false;
                        _isCameraRunning = false;

                        return;
                    }

                    _capture = capture;

                    while (_isCameraRunning)
                    {
                        // カメラからフレームを取得して表示
                        using (var frame = new Mat())
                        {
                            capture.Read(frame);
                            if (!frame.Empty())
                            {
                                Mat processedFrame = frame;
                                if (_calibration.IsCalibrated && _calibration.IsApplyingCalibration)
                                {
                                    processedFrame = _calibration.UndistortImage(frame);
                                }
                                var bitmap = BitmapConverter.ToBitmap(processedFrame);
                                SafeInvoke(() =>
                                {
                                    cameraPictureBox1.Image = bitmap;
                                    cameraPictureBox2.Image = bitmap;
                                });
                            }
                        }
                    }
                }
            }
            catch (AccessViolationException ex)
            {
                // 例外が発生した場合の処理
                SafeInvoke(() =>
                {
                    UpdateStatusLabel1($"カメラキャプチャ中にエラーが発生しました: {ex.Message}");
                    UpdateStatusLabel2($"カメラキャプチャ中にエラーが発生しました: {ex.Message}");
                });
            }
            finally
            {
                // リソースの解放
                lock (this)
                {
                    if (_capture != null && !_capture.IsDisposed)
                    {
                        _capture.Release();
                        _capture.Dispose();
                        _capture = null;
                    }
                }
            }
        }

        // 安全にInvokeを実行するためのヘルパーメソッド
        public void SafeInvoke(Action action)
        {
            if (this.IsHandleCreated && !this.IsDisposed && !this.Disposing)
            {
                try
                {
                    this.Invoke(action);
                }
                catch (ObjectDisposedException)
                {
                    // フォームが破棄されている場合は何もしない
                }
            }
        }

        // キャリブレーションボタンのクリックイベントハンドラ
        private void CalibrationButton_Click(object sender, EventArgs e)
        {
            var calibrationThread = new Thread(() =>
            {
                try
                {
                    // 既存のキャリブレーションデータをリセット
                    _calibration.ResetCalibration();

                    // 保存されているキャリブレーションデータを解放
                    _savedCameraMatrix?.Dispose();
                    _savedDistCoeffs?.Dispose();
                    _savedCameraMatrix = null;
                    _savedDistCoeffs = null;

                    // カメラインデックスを取得
                    int cameraIndex = 0;
                    this.Invoke((MethodInvoker)delegate
                    {
                        if (_isCameraConnectedPage1 == true)
                        {
                            cameraIndex = cameraComboBox1.SelectedIndex;
                        }
                        else if (_isCameraConnectedPage2 == true)
                        {
                            cameraIndex = cameraComboBox2.SelectedIndex;
                        }
                    });

                    // ファイル保存ダイアログを表示
                    this.Invoke((MethodInvoker)delegate
                    {
                        using (var saveFileDialog = new SaveFileDialog
                        {
                            Filter = "XML Files|*.xml",
                            Title = "キャリブレーションデータの保存先を選択してください",
                            FileName = "CalibrationData.xml"
                        })
                        {
                            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                // 選択されたファイルパスを取得
                                var filePath = saveFileDialog.FileName;

                                // キャリブレーションを実行
                                CalibrationButtonEnabled(false);
                                CameraButton1Enabled(false);
                                CameraButton2Enabled(false);
                                LoadButtonEnabled(false);
                                MeasureButtonEnabled(false);

                                _calibration.PerformCalibration(cameraIndex, filePath);
                            }
                            else
                            {
                                // キャンセルされた場合の処理
                                _calibration.CancelCalibration();
                                this.Invoke((MethodInvoker)delegate
                                {
                                    CalibrationButtonEnabled(true);
                                    CameraButton1Enabled(true);
                                    CameraButton2Enabled(true);
                                    LoadButtonEnabled(true);
                                    MeasureButtonEnabled(true);
                                    UpdateStatusLabel1("キャリブレーションがキャンセルされました");
                                    UpdateStatusLabel2("キャリブレーションがキャンセルされました");
                                });
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    // 例外が発生した場合の処理
                    this.Invoke((MethodInvoker)delegate
                    {
                        UpdateStatusLabel1($"キャリブレーション中にエラーが発生しました: {ex.Message}");
                        UpdateStatusLabel2($"キャリブレーション中にエラーが発生しました: {ex.Message}");
                        CalibrationButtonEnabled(true);
                        CameraButton1Enabled(true);
                        CameraButton2Enabled(true);
                        LoadButtonEnabled(true);
                        MeasureButtonEnabled(true);
                    });
                }
            });
            calibrationThread.Start();
        }

        // キャリブレーションデータ読込ボタンのクリックイベントハンドラ
        private void LoadCalibrationDataButton_Click(object sender, EventArgs e)
        {
            if (!_isCameraConnectedPage1 && !_isCameraConnectedPage2)
            {
                UpdateStatusLabel1("カメラを接続してからデータを読み込んでください．");
                UpdateStatusLabel2("カメラを接続してからデータを読み込んでください．");
                return;
            }

            // ファイル読込ダイアログを表示
            var loadThread = new Thread(() =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    using (var openFileDialog = new OpenFileDialog
                    {
                        Filter = "XML Files|*.xml",
                        Title = "キャリブレーションデータを読み込む",
                        FileName = "CalibrationData.xml"
                    })
                    {
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            // 選択されたファイルパスを取得
                            var filePath = openFileDialog.FileName;

                            // キャリブレーションデータを非同期で読み込む
                            var loadDataThread = new Thread(() =>
                            {
                                try
                                {
                                    _loadData.LoadCalibrationData(filePath);

                                    // キャリブレーションデータを保存
                                    _savedCameraMatrix = _calibration.CameraMatrix.Clone();
                                    _savedDistCoeffs = _calibration.DistCoeffs.Clone();

                                    // キャリブレーションデータの適用が成功した場合にUIを更新
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        UpdateStatusLabel1("キャリブレーションデータを読み込みました");
                                        UpdateStatusLabel2("キャリブレーションデータを読み込みました");
                                        CalibrationButtonEnabled(true);
                                        CameraButton1Enabled(true);
                                        CameraButton2Enabled(true);
                                        LoadButtonEnabled(true);
                                        MeasureButtonEnabled(true);
                                    });
                                }
                                catch (Exception ex)
                                {
                                    // 例外が発生した場合の処理
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        UpdateStatusLabel1($"キャリブレーションデータの読み込み中にエラーが発生しました: {ex.Message}");
                                        UpdateStatusLabel2($"キャリブレーションデータの読み込み中にエラーが発生しました: {ex.Message}");
                                        CalibrationButtonEnabled(true);
                                        CameraButton1Enabled(true);
                                        CameraButton2Enabled(true);
                                        LoadButtonEnabled(true);
                                        MeasureButtonEnabled(true);
                                    });
                                }
                            });
                            loadDataThread.Start();
                        }
                    }
                });
            });
            loadThread.Start();
        }

        // フォームが閉じられるときのイベントハンドラ
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // カメラ接続フラグをオフにする
            _isCameraConnectedPage1 = false;
            _isCameraConnectedPage2 = false;
            _isCameraRunning = false;
            _measure.IsMeasuring = false;

            // カメラスレッドの終了処理
            if (_cameraThread != null && _cameraThread.IsAlive)
            {
                _cameraThread.Join(1000);
            }

            // キャリブレーションスレッドの終了処理
            if (_calibration.IsCalibrating)
            {
                _calibration.CancelCalibration();
            }

            // 計測スレッドの終了処理
            if (_measure.IsMeasuring)
            {
                _measure.CancelMeasurement();
            }

            // リソースの解放
            lock (this)
            {
                if (_capture != null && !_capture.IsDisposed)
                {
                    _capture.Release();
                    _capture.Dispose();
                    _capture = null;
                }
            }

            // スレッドの終了を待機
            Thread.Sleep(500);

            // フォームのリソースを解放
            if (components != null)
            {
                components.Dispose();
            }
        }

        // measureButton のクリックイベントハンドラ
        private void MeasureButton_Click(object sender, EventArgs e)
        {
            // キャリブレーションデータが存在するか確認
            if (!_calibration.IsCalibrated)
            {
                UpdateStatusLabel1("キャリブレーションデータが存在しません．データを読み込んでください");
                UpdateStatusLabel2("キャリブレーションデータが存在しません．データを読み込んでください");
                return;
            }

            // 計測処理を実行
            int cameraIndex = 0;
            this.Invoke((MethodInvoker)delegate
            {
                if (_isCameraConnectedPage1)
                {
                    cameraIndex = cameraComboBox1.SelectedIndex;
                }
                else if (_isCameraConnectedPage2)
                {
                    cameraIndex = cameraComboBox2.SelectedIndex;
                }
            });

            // ファイル保存ダイアログを表示
            string filePath = null;
            this.Invoke((MethodInvoker)delegate
            {
                using (var saveFileDialog = new SaveFileDialog
                {
                    Filter = "XML Files|*.xml",
                    Title = "計測データの保存先を選択してください",
                    FileName = "MeasurementData.xml"
                })
                {
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        filePath = saveFileDialog.FileName;
                    }
                }
            });

            // ファイルパスが指定されていない場合はエラーメッセージを表示して処理を終了
            if (string.IsNullOrEmpty(filePath))
            {
                this.Invoke((MethodInvoker)delegate
                {
                    UpdateStatusLabel1("保存先が選択されませんでした。");
                    UpdateStatusLabel2("保存先が選択されませんでした。");
                });
                return;
            }

            // UIコントロールの状態を更新
            this.Invoke((MethodInvoker)delegate
            {
                MeasureButtonEnabled(false);
                CameraButton1Enabled(false);
                CameraButton2Enabled(false);
                LoadButtonEnabled(false);
                CalibrationButtonEnabled(false);
            });

            // マーカーの大きさを指定
            float markerLength = 0.02f; // 2cmのマーカー

            // MeasureクラスのPerformMeasurementメソッドを呼び出す
            _measure.PerformMeasurement(cameraIndex, markerLength, filePath);
        }
    }
}
