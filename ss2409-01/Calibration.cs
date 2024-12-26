using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;

namespace ss2409_01
{
    public class Calibration
    {
        private Form1 _form; // Form1 クラス
        private const int NumShots = 5; // キャリブレーション用画像撮影回数
        private Thread _calibrationThread; // キャリブレーションスレッド
        private bool _isCalibrating; // キャリブレーション中フラグ
        private Mat _cameraMatrix; // カメラ行列
        private Mat _distCoeffs; // 歪み係数
        private int _cameraIndex; // カメラインデックス

        public bool IsCalibrated { get; private set; } // キャリブレーション完了フラグ

        public Calibration(Form1 form)
        {
            // コンストラクタ
            _form = form;
            IsCalibrated = false; // 初期状態はキャリブレーション未完了
        }

        public void PerformCalibration(int cameraIndex)
        {
            // キャリブレーション実行
            _cameraIndex = cameraIndex;

            // カメラが接続されているか確認
            if (!_form.IsCameraConnected || _form.CameraCapture == null || !_form.CameraCapture.IsOpened())
            {
                _form.UpdateStatusLabel("カメラが接続されていないため，キャリブレーションを実行できません．");
                return;
            }

            // キャリブレーションスレッドを開始
            _isCalibrating = true;
            _form.Invoke((MethodInvoker)delegate
            {
                _form.CameraButtonEnabled(false); // カメラ接続ボタンを無効にする
                _form.CalibrationButtonEnabled(false); // キャリブレーションボタンを無効にする
            });
            _calibrationThread = new Thread(CalibrationProcess);
            _calibrationThread.IsBackground = true;
            _calibrationThread.Start();
        }

        private void CalibrationProcess()
        {
            // キャリブレーション用の画像を取得
            using (var capture = new VideoCapture(_cameraIndex))
            {
                if (!capture.IsOpened())
                {
                    _form.Invoke((MethodInvoker)delegate
                    {
                        _form.UpdateStatusLabel("カメラに接続できませんでした．");
                    });
                    _isCalibrating = false;
                    _form.Invoke((MethodInvoker)delegate
                    {
                        _form.CameraButtonEnabled(true); // カメラ接続ボタンを有効にする
                        _form.CalibrationButtonEnabled(true); // キャリブレーションボタンを有効にする
                    });
                    return;
                }

                var objectPoints = new List<Mat>(); // オブジェクトポイント
                var imagePoints = new List<Mat>(); // イメージポイント
                var patternSize = new Size(9, 6); // チェスボードのサイズ

                _form.Invoke((MethodInvoker)delegate
                {
                    _form.UpdateStatusLabel($"チェスボードを映してください．{NumShots}回撮影します．");
                });
                Thread.Sleep(5000);

                for (int i = 1; i <= NumShots && _isCalibrating;)
                {
                    bool found = false;
                    Point2f[] corners = null;

                    // チェスボードが検出されるまでループ
                    while (_isCalibrating && !found)
                    {
                        using (var frame = new Mat())
                        {
                            capture.Read(frame);
                            if (frame.Empty())
                            {
                                _form.Invoke((MethodInvoker)delegate
                                {
                                    _form.UpdateStatusLabel("画像を取得できませんでした．");
                                });
                                _isCalibrating = false;
                                _form.Invoke((MethodInvoker)delegate
                                {
                                    _form.CameraButtonEnabled(true); // カメラ接続ボタンを有効にする
                                    _form.CalibrationButtonEnabled(true); // キャリブレーションボタンを有効にする
                                });
                                return;
                            }

                            // グレースケールに変換
                            var gray = new Mat();
                            Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);

                            // チェスボードのコーナーを検出
                            found = Cv2.FindChessboardCorners(gray, patternSize, out corners);

                            if (found)
                            {
                                _form.Invoke((MethodInvoker)delegate
                                {
                                    _form.UpdateStatusLabel($"チェスボードを正しく検知できました．違う角度で再撮影してください {i}/{NumShots}");
                                });
                            }
                            else
                            {
                                _form.Invoke((MethodInvoker)delegate
                                {
                                    _form.UpdateStatusLabel($"チェスボードが検出されませんでした．再試行中 {i}/{NumShots}");
                                });
                            }

                            // 少し待機して次の試行に備える
                            Thread.Sleep(5000);
                        }
                    }

                    if (found)
                    {
                        // オブジェクトポイントとイメージポイントを保存
                        var objPts = new Mat(patternSize.Width * patternSize.Height, 1, MatType.CV_32FC3);
                        for (int j = 0; j < patternSize.Height; j++)
                        {
                            for (int k = 0; k < patternSize.Width; k++)
                            {
                                objPts.Set(j * patternSize.Width + k, new Point3f(k, j, 0));
                            }
                        }
                        objectPoints.Add(objPts);

                        var imgPts = new Mat(corners.Length, 1, MatType.CV_32FC2);
                        for (int j = 0; j < corners.Length; j++)
                        {
                            imgPts.Set(j, corners[j]);
                        }
                        imagePoints.Add(imgPts);

                        // 撮影回数を増やす
                        i++;
                    }
                }

                if (_isCalibrating && objectPoints.Count > 0 && imagePoints.Count > 0)
                {
                    // キャリブレーションを実行
                    _cameraMatrix = new Mat();
                    _distCoeffs = new Mat();
                    Mat[] rvecs, tvecs;
                    Cv2.CalibrateCamera(objectPoints, imagePoints, patternSize, _cameraMatrix, _distCoeffs, out rvecs, out tvecs);

                    // カメラパラメータを保存
                    IsCalibrated = true; // キャリブレーション完了フラグを設定
                }
                else
                {
                    _form.Invoke((MethodInvoker)delegate
                    {
                        _form.UpdateStatusLabel("キャリブレーションに失敗しました．");
                    });
                }

                // リソースを解放
                foreach (var mat in objectPoints)
                {
                    mat.Dispose();
                }
                foreach (var mat in imagePoints)
                {
                    mat.Dispose();
                }

                _isCalibrating = false;
                _form.Invoke((MethodInvoker)delegate
                {
                    _form.CameraButtonEnabled(true); // カメラ接続ボタンを有効にする
                    _form.CalibrationButtonEnabled(true); // キャリブレーションボタンを有効にする
                    _form.DisconnectCamera(); // カメラを切断
                    _form.UpdateStatusLabel("キャリブレーションが完了しました．再度カメラを選択し，接続ボタンを押してください．");
                });
            }
        }

        public void StopCalibration()
        {
            // キャリブレーション中止
            _isCalibrating = false;
            if (_calibrationThread != null && _calibrationThread.IsAlive)
            {
                _calibrationThread.Join(1000); // スレッドの終了を待機（タイムアウトを設定）
            }
        }

        public void ResetCalibration()
        {
            // キャリブレーションリセット
            _cameraMatrix?.Dispose();
            _distCoeffs?.Dispose();
            _cameraMatrix = null;
            _distCoeffs = null;
            IsCalibrated = false;
        }

        public Mat UndistortImage(Mat image)
        {
            // 画像補正
            if (!IsCalibrated)
            {
                throw new InvalidOperationException("キャリブレーションが完了していません．");
            }

            var undistortedImage = new Mat();
            Cv2.Undistort(image, undistortedImage, _cameraMatrix, _distCoeffs);
            return undistortedImage;
        }
    }
}
