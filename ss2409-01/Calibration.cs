using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;
using System.IO;
using System.Xml;

namespace ss2409_01
{
    public class Calibration
    {
        private readonly Form1 _form; // Form1 クラス
        private const int NumShots = 10; // キャリブレーション用画像撮影回数
        private Thread _calibrationThread; // キャリブレーションスレッド
        private bool _isCalibrating; // キャリブレーション中フラグ
        private bool _isCancelled; // キャリブレーション中止フラグ
        private bool _applyCalibration; // キャリブレーション適用フラグ
        private Mat _cameraMatrix; // カメラ行列
        private Mat _distCoeffs; // 歪み係数
        private int _cameraIndex; // カメラインデックス

        public bool IsCalibrated { get; private set; } // キャリブレーション完了フラグ
        public bool IsApplyingCalibration => _applyCalibration; // キャリブレーション適用フラグの公開プロパティ
        public bool IsCalibrating => _isCalibrating; // キャリブレーション中フラグの公開プロパティ

        public Mat CameraMatrix => _cameraMatrix;
        public Mat DistCoeffs => _distCoeffs;

        public Calibration(Form1 form)
        {
            // コンストラクタ
            _form = form;
            IsCalibrated = false; // 初期状態はキャリブレーション未完了
            _applyCalibration = false; // 初期状態はキャリブレーション適用しない
        }

        public void PerformCalibration(int cameraIndex, string filePath)
        {
            // キャリブレーション実行
            if (_isCalibrating)
            {
                _form.UpdateStatusLabel1("キャリブレーションは既に実行中です．");
                _form.UpdateStatusLabel2("キャリブレーションは既に実行中です．");
                return;
            }

            // 以前のキャリブレーションデータをリセット
            ResetCalibration();

            _cameraIndex = cameraIndex;
            _applyCalibration = false; // キャリブレーションを適用しない

            // カメラが接続されているか確認
            if (!_form.IsCameraConnectedPage1 && !_form.IsCameraConnectedPage2)
            {
                _form.UpdateStatusLabel1("カメラが接続されていないため，キャリブレーションを実行できません．");
                _form.UpdateStatusLabel2("カメラが接続されていないため，キャリブレーションを実行できません．");
                return;
            }

            // キャリブレーションスレッドを開始
            _isCalibrating = true;
            _isCancelled = false;

            // UIを更新
            _form.Invoke((MethodInvoker)delegate
            {
                _form.CameraButton1Enabled(false); // カメラ接続ボタンを無効にする
                _form.CameraButton2Enabled(false); // カメラ接続ボタン2を無効にする
                _form.CalibrationButtonEnabled(false); // キャリブレーションボタンを無効にする
                _form.LoadButtonEnabled(false); // ロードボタンを無効にする
                _form.MeasureButtonEnabled(false); // 計測ボタンを無効にする
            });
            _calibrationThread = new Thread(() => CalibrationProcess(filePath))
            {
                IsBackground = true
            };
            _calibrationThread.Start();
        }

        private void CalibrationProcess(string filePath)
        {
            try
            {
                // キャリブレーション用の画像を取得
                using (var capture = new VideoCapture(_cameraIndex))
                {
                    if (!capture.IsOpened())
                    {
                        // カメラに接続できない場合
                        _form.Invoke((MethodInvoker)delegate
                        {
                            _form.UpdateStatusLabel1("カメラに接続できませんでした．");
                            _form.UpdateStatusLabel2("カメラに接続できませんでした．");
                        });
                        _isCalibrating = false;
                        _form.Invoke((MethodInvoker)delegate
                        {
                            _form.CameraButton1Enabled(true); // カメラ接続ボタンを有効にする
                            _form.CameraButton2Enabled(true); // カメラ接続ボタン2を有効にする
                            _form.CalibrationButtonEnabled(true); // キャリブレーションボタンを有効にする
                            _form.LoadButtonEnabled(true); // ロードボタンを有効にする
                            _form.MeasureButtonEnabled(true); // 計測ボタンを有効にする
                        });
                        return;
                    }

                    var objectPoints = new List<Mat>(); // オブジェクトポイント
                    var imagePoints = new List<Mat>(); // イメージポイント
                    var patternSize = new Size(9, 6); // チェスボードのサイズ

                    _form.Invoke((MethodInvoker)delegate
                    {
                        _form.UpdateStatusLabel1($"チェスボードを映してください．{NumShots}回撮影します．");
                        _form.UpdateStatusLabel2($"チェスボードを映してください．{NumShots}回撮影します．");

                        _form.CameraButton1Enabled(false); // カメラ接続ボタンを無効にする
                        _form.CameraButton2Enabled(false); // カメラ接続ボタン2を無効にする
                        _form.CalibrationButtonEnabled(false); // キャリブレーションボタンを無効にする
                        _form.LoadButtonEnabled(false); // ロードボタンを無効にする
                        _form.MeasureButtonEnabled(false); // 計測ボタンを無効にする
                    });
                    Thread.Sleep(2000);

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
                                        _form.UpdateStatusLabel1("画像を取得できませんでした．");
                                        _form.UpdateStatusLabel2("画像を取得できませんでした．");
                                    });
                                    _isCalibrating = false;
                                    _form.Invoke((MethodInvoker)delegate
                                    {
                                        _form.CameraButton1Enabled(true); // カメラ接続ボタンを有効にする
                                        _form.CameraButton2Enabled(true); // カメラ接続ボタン2を有効にする
                                        _form.CalibrationButtonEnabled(true); // キャリブレーションボタンを有効にする
                                        _form.LoadButtonEnabled(true); // ロードボタンを有効にする
                                        _form.MeasureButtonEnabled(true); // 計測ボタンを有効にする
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
                                    if (i != NumShots)
                                    {
                                        _form.Invoke((MethodInvoker)delegate
                                        {
                                            _form.UpdateStatusLabel1($"チェスボードを正しく検知できました．違う角度で再撮影してください {i + 1}/{NumShots}");
                                            _form.UpdateStatusLabel2($"チェスボードを正しく検知できました．違う角度で再撮影してください {i + 1}/{NumShots}");
                                        });
                                    }
                                    else
                                    {
                                        _form.Invoke((MethodInvoker)delegate
                                        {
                                            _form.UpdateStatusLabel1($"チェスボードを正しく検知できました．パラメータ計算中");
                                            _form.UpdateStatusLabel2($"チェスボードを正しく検知できました．パラメータ計算中");
                                        });
                                    }
                                    Thread.Sleep(1000);

                                }
                                else
                                {
                                    _form.Invoke((MethodInvoker)delegate
                                    {
                                        _form.UpdateStatusLabel1($"チェスボードが検出されませんでした．再試行中 {i}/{NumShots}");
                                        _form.UpdateStatusLabel2($"チェスボードが検出されませんでした．再試行中 {i}/{NumShots}");
                                    });
                                }
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
                        Cv2.CalibrateCamera(objectPoints, imagePoints, patternSize, _cameraMatrix, _distCoeffs, out Mat[] rvecs, out Mat[] tvecs);

                        // カメラパラメータを保存
                        IsCalibrated = true; // キャリブレーション完了フラグを設定
                        _form.Invoke((MethodInvoker)delegate
                        {
                            _form.UpdateStatusLabel1("キャリブレーションが完了しました。");
                            _form.UpdateStatusLabel2("キャリブレーションが完了しました。");
                        });

                        // キャリブレーションデータを保存
                        SaveCalibrationData(filePath);
                    }
                    else if (!_isCancelled)
                    {
                        _form.Invoke((MethodInvoker)delegate
                        {
                            _form.UpdateStatusLabel1("キャリブレーションに失敗しました．");
                            _form.UpdateStatusLabel2("キャリブレーションに失敗しました．");
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

                    if (!_isCancelled)
                    {
                        _form.Invoke((MethodInvoker)delegate
                        {
                            
                            _form.CameraButton1Enabled(true); // カメラ接続ボタンを有効にする
                            _form.CameraButton2Enabled(true); // カメラ接続ボタン2を有効にする
                            _form.CalibrationButtonEnabled(true); // キャリブレーションボタンを有効にする
                            _form.LoadButtonEnabled(true); // ロードボタンを有効にする
                            _form.MeasureButtonEnabled(true); // 計測ボタンを有効にする
                        });
                    }
                    _form.DisconnectCamera(); // カメラを切断
                }
            }
            catch (Exception ex)
            {
                _form.Invoke((MethodInvoker)delegate
                {
                    _form.UpdateStatusLabel1($"キャリブレーション中にエラーが発生しました: {ex.Message}");
                    _form.UpdateStatusLabel2($"キャリブレーション中にエラーが発生しました: {ex.Message}");
                });
            }
            finally
            {
                // キャリブレーション終了
                _isCalibrating = false;
                if (_calibrationThread != null && _calibrationThread.IsAlive)
                {
                    _calibrationThread.Join(1000); // スレッドの終了を待機
                }
                Console.WriteLine("キャリブレーション終了");
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

        public void CancelCalibration()
        {
            // キャリブレーション中止
            _isCalibrating = false;
            _isCancelled = true;
            if (_calibrationThread != null && _calibrationThread.IsAlive)
            {
                // スレッドの終了を待機
                _calibrationThread.Join(1000); // タイムアウトを設定
            }
        }

        public Mat UndistortImage(Mat image)
        {
            // 画像補正
            if (!IsCalibrated)
            {
                throw new InvalidOperationException("キャリブレーションが完了していません．");
            }

            if (!_applyCalibration)
            {
                return image; // キャリブレーションデータを適用しない場合はそのまま返す
            }

            var undistortedImage = new Mat();
            Cv2.Undistort(image, undistortedImage, _cameraMatrix, _distCoeffs);
            return undistortedImage;
        }

        public void SaveCalibrationData(string filePath)
        {
            // キャリブレーションデータをXMLファイルに保存
            if (_isCancelled)
            {
                _form.UpdateStatusLabel1("キャリブレーションが中止されました。");
                _form.UpdateStatusLabel2("キャリブレーションが中止されました。");
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    throw new ArgumentException("ファイルパスが無効です。");
                }

                // ログにファイルパスを記録
                File.AppendAllText("debug_log.txt", $"{DateTime.Now}: Saving to {filePath}\n");

                // カメラ行列と歪み係数が初期化されているか確認
                if (_cameraMatrix == null || _distCoeffs == null)
                {
                    throw new InvalidOperationException("カメラ行列または歪み係数が初期化されていません。");
                }

                // XMLドキュメントを作成
                var xmlDoc = new XmlDocument();
                var root = xmlDoc.CreateElement("CalibrationData");
                xmlDoc.AppendChild(root);

                // カメラ行列を保存
                var cameraMatrixElement = xmlDoc.CreateElement("CameraMatrix");
                for (int i = 0; i < _cameraMatrix.Rows; i++)
                {
                    var rowElement = xmlDoc.CreateElement("Row");
                    for (int j = 0; j < _cameraMatrix.Cols; j++)
                    {
                        var cellElement = xmlDoc.CreateElement("Cell");
                        cellElement.InnerText = _cameraMatrix.At<double>(i, j).ToString();
                        rowElement.AppendChild(cellElement);
                    }
                    cameraMatrixElement.AppendChild(rowElement);
                }
                root.AppendChild(cameraMatrixElement);

                // 歪み係数を保存
                var distCoeffsElement = xmlDoc.CreateElement("DistCoeffs");
                for (int i = 0; i < _distCoeffs.Rows; i++)
                {
                    for (int j = 0; j < _distCoeffs.Cols; j++)
                    {
                        var cellElement = xmlDoc.CreateElement("Cell");
                        cellElement.InnerText = _distCoeffs.At<double>(i, j).ToString();
                        distCoeffsElement.AppendChild(cellElement);
                    }
                }
                root.AppendChild(distCoeffsElement);

                // XMLファイルを保存
                xmlDoc.Save(filePath);

                _form.UpdateStatusLabel1("キャリブレーションデータを保存しました。");
                _form.UpdateStatusLabel2("キャリブレーションデータを保存しました。");
            }
            catch (Exception ex)
            {
                _form.UpdateStatusLabel1($"データ保存中にエラーが発生しました: {ex.Message}");
                _form.UpdateStatusLabel2($"データ保存中にエラーが発生しました: {ex.Message}");
                // エラーログをファイルに記録
                File.AppendAllText("error_log.txt", $"{DateTime.Now}: {ex.ToString()}\n");
            }
        }

        public void SetCalibrationData(Mat cameraMatrix, Mat distCoeffs)
        {
            // キャリブレーションデータを設定
            _cameraMatrix = cameraMatrix.Clone();
            _distCoeffs = distCoeffs.Clone();
            IsCalibrated = true;
            _applyCalibration = true; // キャリブレーションデータを適用する

            // デバッグ情報を追加
            Console.WriteLine("Set Camera Matrix:");
            for (int i = 0; i < _cameraMatrix.Rows; i++)
            {
                for (int j = 0; j < _cameraMatrix.Cols; j++)
                {
                    Console.Write(_cameraMatrix.At<double>(i, j) + " ");
                }
                Console.WriteLine();
            }

            Console.WriteLine("Set Dist Coeffs:");
            for (int i = 0; i < _distCoeffs.Rows; i++)
            {
                Console.WriteLine(_distCoeffs.At<double>(i, 0));
            }
        }
    }
}

