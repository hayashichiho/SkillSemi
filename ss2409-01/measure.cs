using System;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;

namespace ss2409_01
{
    public class Measure
    {
        private readonly Form1 _form;
        public Thread _measurementThread;
        private bool _isMeasuring;

        public bool IsMeasuring
        {
            get => _isMeasuring;
            set => _isMeasuring = value;
        }

        public Thread Thread => _measurementThread;

        public Measure(Form1 form)
        {
            _form = form;
        }

        public void PerformMeasurement(int cameraIndex, float markerLength, string filePath)
        {
            if (IsMeasuring)
            {
                _form.UpdateStatusLabel1("計測は既に実行中です．");
                _form.UpdateStatusLabel2("計測は既に実行中です．");
                return;
            }

            _measurementThread = new Thread(() => MeasurementProcess(cameraIndex, markerLength, filePath));
            _measurementThread.Start();
        }

        private void MeasurementProcess(int cameraIndex, float markerLength, string filePath)
        {
            try
            {
                IsMeasuring = true;
                var aruco = new Aruco(_form);
                Point2f rulerEndPoint = new Point2f(); // 初期化
                Point2f markerTopLeft, markerTopRight, initialRulerEndPoint, movedRulerEndPoint;

                using (var capture = new VideoCapture(cameraIndex))
                {
                    // カメラに接続できない場合はエラーをスロー
                    if (!capture.IsOpened())
                    {
                        IsMeasuring = false;

                        _form.SafeInvoke(() =>
                        {
                            _form.UpdateStatusLabel1("カメラに接続できませんでした．");
                            _form.UpdateStatusLabel2("カメラに接続できませんでした．");
                        });
                        return;
                    }

                    // マーカーを検出して位置姿勢を推定
                    if (!aruco.DetectMarkerAndEstimatePose(capture, markerLength, _form.Calibration.CameraMatrix, _form.Calibration.DistCoeffs, out Point2f[] detectedCorners, out int detectedId, out Vec3d[] rvecs, out Vec3d[] tvecs))
                    {
                        IsMeasuring = false;
                        return;
                    }

                    markerTopLeft = detectedCorners[0];
                    markerTopRight = detectedCorners[1];

                    // 定規の先端の座標を推測
                    float distanceFromMarkerTopRight = 0.1f; // 10cm
                    initialRulerEndPoint = Aruco.CalculateRulerEndPoint(markerTopLeft, markerTopRight, distanceFromMarkerTopRight, markerLength);

                    aruco.SaveMeasurementData(filePath, detectedCorners, detectedId, initialRulerEndPoint, true);

                    // 定規を移動させるためのメッセージを表示
                    _form.SafeInvoke(() =>
                    {
                        MessageBox.Show("定規を移動させてください。移動が完了したらOKを押してください。", "定規の移動", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    });

                    // 再度マーカーを検出して位置姿勢を推定
                    if (!aruco.DetectMarkerAndEstimatePose(capture, markerLength, _form.Calibration.CameraMatrix, _form.Calibration.DistCoeffs, out detectedCorners, out detectedId, out rvecs, out tvecs))
                    {
                        IsMeasuring = false;
                        return;
                    }

                    markerTopLeft = detectedCorners[0];
                    markerTopRight = detectedCorners[1];

                    // 移動後の定規の座標を推測
                    movedRulerEndPoint = Aruco.CalculateRulerEndPoint(markerTopLeft, markerTopRight, distanceFromMarkerTopRight, markerLength);

                    // 定規の先端の座標を再計算
                    rulerEndPoint = Aruco.CalculateRulerEndPoint(markerTopLeft, markerTopRight, distanceFromMarkerTopRight, markerLength);

                    // 距離を計算
                    float totalDistance = (float)Math.Sqrt(Math.Pow(rulerEndPoint.X - initialRulerEndPoint.X, 2) + Math.Pow(rulerEndPoint.Y - initialRulerEndPoint.Y, 2));

                    // 計測データを保存
                    aruco.SaveMeasurementData(filePath, detectedCorners, detectedId, rulerEndPoint, false);

                    // 結果をmmTextBoxに表示
                    _form.SafeInvoke(() =>
                    {
                        _form.mmTextBox.Text = $"{totalDistance}";
                        _form.UpdateStatusLabel1("計測データを保存しました。");
                        _form.UpdateStatusLabel2("計測データを保存しました。");
                    });
                }
            }
            catch (Exception ex)
            {
                IsMeasuring = false;

                _form.SafeInvoke(() =>
                {
                    _form.UpdateStatusLabel1($"計測中にエラーが発生しました: {ex.Message}");
                    _form.UpdateStatusLabel2($"計測中にエラーが発生しました: {ex.Message}");
                });
            }
            finally
            {
                // スレッドの終了
                _form.SafeInvoke(() =>
                {
                    _form.CameraButton1Enabled(true);
                    _form.CameraButton2Enabled(true);
                    _form.CalibrationButtonEnabled(true);
                    _form.LoadButtonEnabled(true);
                    _form.MeasureButtonEnabled(true);
                });

                Thread.Sleep(2000); // 1秒待機
                _form.DisconnectCamera(); // カメラの切断

                // フラグの更新
                IsMeasuring = false;
            }
        }

        public void CancelMeasurement()
        {
            if (_measurementThread != null && _measurementThread.IsAlive)
            {
                _measurementThread.Abort();
                IsMeasuring = false;
            }
        }
    }
}


