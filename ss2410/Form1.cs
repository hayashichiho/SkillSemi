using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace ss2410
{
    public partial class Form1 : Form
    {
        private VideoCapture _camera; // カメラキャプチャオブジェクト
        private Thread _cameraThread; // カメラ表示用のスレッド
        private Thread _sensorThread; // センサー入力用のスレッド
        private Thread _drawThread; // お絵かき用のスレッド
        private bool _isRunning; // スレッドの実行フラグ
        private SerialPort _serialPort; // シリアルポートオブジェクト
        private System.Drawing.Point _currentPoint; // 現在の描画位置
        private Bitmap _bitmap; // 描画用のビットマップ
        private Bitmap _overlayBitmap; // 線を描画するためのビットマップ
        private float _previousX; // 前回のXの値
        private float _previousY; // 前回のYの値
        private int _isDrawing; // 描画中かどうかのフラグ
        private bool start = true; // 描画開始フラグ
        private int startX, startY; // 描画開始位置
        private Stopwatch _cameraInputStopwatch; // カメラ入力FPS計測用のストップウォッチ
        private Stopwatch _cameraDisplayStopwatch; // カメラ表示FPS計測用のストップウォッチ
        private Stopwatch _sensorStopwatch; // センサー取得Hz計測用のストップウォッチ

        // コンストラクタ
        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load; // フォームロード時のイベントハンドラを追加
            FormClosing += Form1_FormClosing; // フォームクローズ時のイベントハンドラを追加

            // Chartの初期設定
            chart1.Series.Clear();
            chart1.Series.Add("X Tilt");
            chart1.Series.Add("Y Tilt");
            chart1.Series.Add("Z Tilt");

            // グラフの設定
            foreach (var series in chart1.Series)
            {
                series.ChartType = SeriesChartType.Line; // 折れ線グラフ
            }

            _cameraInputStopwatch = new Stopwatch();
            _cameraDisplayStopwatch = new Stopwatch();
            _sensorStopwatch = new Stopwatch();
        }

        // フォームロード時の処理
        private void Form1_Load(object sender, EventArgs e)
        {
            _camera = new VideoCapture(0); // カメラを初期化（デバイスID 0）
            if (!_camera.IsOpened())
            {
                MessageBox.Show("カメラが見つかりませんでした"); // カメラが見つからない場合のエラーメッセージ
                return;
            }

            _isRunning = true;
            _cameraThread = new Thread(CameraLoop); // カメラ表示用のスレッドを作成
            _cameraThread.Start(); // スレッドを開始

            // シリアルポートの接続
            if (!Connect())
            {
                MessageBox.Show("シリアルポートの接続に失敗しました");
                _isRunning = false;
                return;
            }

            _sensorThread = new Thread(SensorLoop); // センサー入力用のスレッドを作成
            _sensorThread.Start(); // スレッドを開始

            _drawThread = new Thread(DrawLoop); // お絵かき用のスレッドを作成
            _drawThread.Start(); // スレッドを開始

            _currentPoint = new System.Drawing.Point(Width / 4, Height / 2); // 初期描画位置を画面中央に設定
            _overlayBitmap = new Bitmap(Width, Height); // 透明なボードを初期化
            Console.WriteLine("初期座標:  x = " + _currentPoint.X + ", y = " + _currentPoint.Y);
        }

        // シリアルポートの接続
        public bool Connect()
        {
            // シリアルポートの設定
            _serialPort = new SerialPort
            {
                PortName = "COM4", // ポート名
                BaudRate = 115200,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                Encoding = System.Text.Encoding.ASCII,
                WriteTimeout = 1000,
                ReadTimeout = 1000,
                RtsEnable = false,
            };

            try
            {
                _serialPort.Open();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }

            // 初期のX軸とY軸の値を取得
            for (int i = 0; i < 5; i++) // 最大5回リトライ
            {
                try
                {
                    string data = _serialPort.ReadLine(); // シリアルポートから1行読み込む
                    string[] values = data.Split(',');

                    if (values.Length >= 4 && float.TryParse(values[1], out _previousX) && float.TryParse(values[2], out _previousY))
                    {
                        Console.WriteLine("初期傾斜: x = " + _previousX + ", y = " + _previousY);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("データの読み取りに失敗しました: " + ex.Message);
                }

                if (i == 4)
                {
                    MessageBox.Show("初期値の取得に失敗しました");
                    return false;
                }
            }

            return true;
        }

        // センサー入力用のループ
        private void SensorLoop()
        {
            while (_isRunning)
            {
                _sensorStopwatch.Restart();
                try
                {
                    string data = _serialPort.ReadLine(); // シリアルポートから1行読み込む
                    string[] values = data.Split(',');

                    if (values.Length == 4)
                    {
                        if (float.TryParse(values[1], out float xTilt) && float.TryParse(values[2], out float yTilt) && float.TryParse(values[3], out float zTilt))
                        {
                            // 描画の開始/終了を切り替え
                            _isDrawing = int.Parse(values[0]);

                            _currentPoint.X += (int)((xTilt - _previousX) * 150); // スケーリングを調整
                            _currentPoint.Y += (int)((yTilt - _previousY) * 300); // スケーリングを調整

                            // 前回の傾斜を更新
                            _previousX = xTilt;
                            _previousY = yTilt;

                            // 描画位置が画面外に出ないように制限
                            _currentPoint.X = Math.Max(0, Math.Min(Width, _currentPoint.X));
                            _currentPoint.Y = Math.Max(0, Math.Min(Height, _currentPoint.Y));

                            // グラフに値を追加
                            Invoke(new Action(() =>
                            {
                                chart1.Series["X Tilt"].Points.AddY(xTilt);
                                chart1.Series["Y Tilt"].Points.AddY(yTilt);
                                chart1.Series["Z Tilt"].Points.AddY(zTilt);

                                // データポイントが多すぎる場合は古いデータを削除
                                if (chart1.Series["X Tilt"].Points.Count > 100)
                                {
                                    chart1.Series["X Tilt"].Points.RemoveAt(0);
                                    chart1.Series["Y Tilt"].Points.RemoveAt(0);
                                    chart1.Series["Z Tilt"].Points.RemoveAt(0);
                                }

                                chart1.ResetAutoValues();

                                // センサーの値をTextBoxに表示
                                textBoxSensorFPS.Text = $"X Tilt: {xTilt}\r\nY Tilt: {yTilt}\r\nZ Tilt: {zTilt}";
                            }));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                _sensorStopwatch.Stop();
                double sensorFPS = 1000.0 / _sensorStopwatch.ElapsedMilliseconds;

                // センサー取得FPSをTextBoxに表示
                Invoke(new Action(() =>
                {
                    textBoxSensorFPS.Text = $"Sensor FPS: {sensorFPS:F2}";
                }));
            }
        }

        // お絵かき用のループ
        private void DrawLoop()
        {
            while (_isRunning)
            {
                _cameraDisplayStopwatch.Restart();
                if (_isDrawing == 1) // 描画中の場合のみ描画
                {
                    Console.WriteLine("描画中: x = " + _currentPoint.X + ", y = " + _currentPoint.Y);

                    if (start)
                    {
                        startX = _currentPoint.X;
                        startY = _currentPoint.Y;
                        start = false;
                    }

                    int endX = _currentPoint.X;
                    int endY = _currentPoint.Y;

                    using (Graphics g = Graphics.FromImage(_overlayBitmap))
                    {
                        // 透明なボードに線を描画
                        g.DrawLine(Pens.Red, startX, startY, endX, endY);
                    }

                    // 一時的なビットマップに矢印を描画
                    using (Bitmap tempBitmap = new Bitmap(_overlayBitmap))
                    {
                        using (Graphics g = Graphics.FromImage(tempBitmap))
                        {
                            // 矢印のポイントを定義
                            PointF[] arrowPoints = new PointF[]
                            {
                                    new PointF(_currentPoint.X, _currentPoint.Y - 15), // 上の頂点
                                    new PointF(_currentPoint.X - 5, _currentPoint.Y ), // 左下の頂点
                                    new PointF(_currentPoint.X + 5, _currentPoint.Y ) // 右下の頂点
                            };

                            // 現在の座標に矢印を描画
                            g.FillPolygon(Brushes.Blue, arrowPoints);
                        }

                        if (_bitmap != null)
                        {
                            using (Graphics g = Graphics.FromImage(_bitmap))
                            {
                                // 透明なボードをカメラのフレームに合成
                                g.DrawImage(tempBitmap, 0, 0);
                            }

                            Invoke(new Action(() =>
                            {
                                pictureBox1.Image = (Bitmap)_bitmap.Clone(); // UIスレッドでPictureBoxにビットマップを表示
                            }));
                        }
                    }

                    startX = endX;
                    startY = endY;
                }
                else
                {
                    if (_bitmap != null)
                    {
                        using (Graphics g = Graphics.FromImage(_bitmap))
                        {
                            // 透明なボードをカメラのフレームに合成
                            g.DrawImage(_overlayBitmap, 0, 0);
                        }

                        Invoke(new Action(() =>
                        {
                            pictureBox1.Image = (Bitmap)_bitmap.Clone(); // UIスレッドでPictureBoxにビットマップを表示
                        }));
                    }
                }
                _cameraDisplayStopwatch.Stop();
                double displayFPS = 1000.0 / _cameraDisplayStopwatch.ElapsedMilliseconds;

                // 画像表示FPSをTextBoxに表示
                Invoke(new Action(() =>
                {
                    textBoxCameraDisplayFPS.Text = $"Display FPS: {displayFPS:F2}";
                }));

                Thread.Sleep(3); // 約30fpsで更新
            }
        }

        // カメラ表示用のループ
        private void CameraLoop()
        {
            while (_isRunning)
            {
                _cameraInputStopwatch.Restart();
                using (var frame = new Mat())
                {
                    _camera.Read(frame); // カメラからフレームを読み込む
                    if (!frame.Empty())
                    {
                        _bitmap = BitmapConverter.ToBitmap(frame); // フレームをビットマップに変換

                        // カメラの入力をTextBoxに表示
                        Invoke(new Action(() =>
                        {
                            textBoxCameraInputFPS.Text = frame.ToString();
                        }));
                    }
                }
                _cameraInputStopwatch.Stop();
                double cameraInputFPS = 1000.0 / _cameraInputStopwatch.ElapsedMilliseconds;

                // カメラ入力FPSをTextBoxに表示
                Invoke(new Action(() =>
                {
                    textBoxCameraInputFPS.Text = $"Camera Input FPS: {cameraInputFPS:F2}";
                }));

                Thread.Sleep(33); // 約30fpsで更新
            }
        }

        // フォームを閉じるときの処理
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _isRunning = false; // スレッドの実行フラグを停止
            _cameraThread.Join(); // スレッドの終了を待機
            _sensorThread.Join(); // センサー入力用のスレッドの終了を待機
            _drawThread.Join(); // お絵かき用のスレッドの終了を待機
            _camera.Release(); // カメラリソースを解放
            _camera.Dispose(); // カメラオブジェクトを破棄
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close(); // シリアルポートを閉じる
            }
        }
    }
}




