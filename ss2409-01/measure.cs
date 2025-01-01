using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;
using System.IO;
using System.Xml;

namespace ss2409_01
{
    public class Measure
    {
        private readonly Form1 _form; // Form1 クラス
        private Thread _measurementThread; // 計測スレッド
        private bool _isMeasuring; // 計測中フラグ
        private bool _isCancelled; // 計測中止フラグ
        private bool _isSaved; // データ保存フラグ
        private Mat _cameraMatrix; // カメラ行列
        private Mat _distCoeffs; // 歪み係数
        private int _cameraIndex; // カメラインデックス

        public bool IsMeasuring => _isMeasuring; // 計測中フラグの公開プロパティ
        public bool IsSaved => _isSaved; // データ保存フラグの公開プロパティ

        public Measure(Form1 form)
        {
            // コンストラクタ
            _form = form;
            _isSaved = false; // 初期状態はデータ未保存
        }

        public void PerformMeasurement(int cameraIndex, string filePath)
        {
            // 計測実行
            if (_isMeasuring)
            {
                _form.UpdateStatusLabel1("計測は既に実行中です。");
                _form.UpdateStatusLabel2("計測は既に実行中です。");
                return;
            }

            // 以前の計測データをリセット
            ResetMeasurement();

            _cameraIndex = cameraIndex;

            // カメラが接続されているか確認
            if (!_form.IsCameraConnectedPage1 && !_form.IsCameraConnectedPage2)
            {
                _form.UpdateStatusLabel1("カメラが接続されていないため，計測を実行できません．");
                _form.UpdateStatusLabel2("カメラが接続されていないため，計測を実行できません．");
                return;
            }

            // 計測スレッドを開始
            _isMeasuring = true;
            _isCancelled = false;
            _isSaved = false;
            _form.Invoke((MethodInvoker)delegate
            {
                _form.CameraButton1Enabled(false); // カメラ接続ボタンを無効にする
                _form.CameraButton2Enabled(false); // カメラ接続ボタン2を無効にする
                _form.CalibrationButtonEnabled(false); // キャリブレーションボタンを無効にする
                _form.LoadButtonEnabled(false); // ロードボタンを無効にする
                _form.MeasureButtonEnabled(false); // 計測ボタンを無効にする
            });
            _measurementThread = new Thread(() => MeasurementProcess(filePath))
            {
                IsBackground = true
            };
            _measurementThread.Start();
        }

        private void MeasurementProcess(string filePath)
        {
            try
            {
                // 計測用の画像を取得
                using (var capture = new VideoCapture(_cameraIndex))
                {
                    if (!capture.IsOpened())
                    {
                        _form.Invoke((MethodInvoker)delegate
                        {
                            _form.UpdateStatusLabel1("カメラに接続できませんでした．");
                            _form.UpdateStatusLabel2("カメラに接続できませんでした．");
                        });
                        _isMeasuring = false;
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
                        _form.UpdateStatusLabel1($"チェスボードを映してください．計測を開始します．");
                        _form.UpdateStatusLabel2($"チェスボードを映してください．計測を開始します．");
                    });
                    Thread.Sleep(2000);

                    bool found = false;
                    Point2f[] corners = null;

                    // チェスボードが検出されるまでループ
                    while (_isMeasuring && !found)
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
                                _isMeasuring = false;
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
                                _form.Invoke((MethodInvoker)delegate
                                {
                                    _form.UpdateStatusLabel1($"チェスボードを正しく検知できました．計測中");
                                    _form.UpdateStatusLabel2($"チェスボードを正しく検知できました．計測中");
                                });
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                _form.Invoke((MethodInvoker)delegate
                                {
                                    _form.UpdateStatusLabel1($"チェスボードが検出されませんでした．再試行中");
                                    _form.UpdateStatusLabel2($"チェスボードが検出されませんでした．再試行中");
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

                        // 計測を実行
                        _cameraMatrix = new Mat();
                        _distCoeffs = new Mat();
                        Mat[] rvecs, tvecs;
                        Cv2.CalibrateCamera(objectPoints, imagePoints, patternSize, _cameraMatrix, _distCoeffs, out rvecs, out tvecs);

                        // カメラパラメータを保存
                        _form.Invoke((MethodInvoker)delegate
                        {
                            _form.UpdateStatusLabel1("計測が完了しました。");
                            _form.UpdateStatusLabel2("計測が完了しました。");
                        });

                        // 計測データを保存
                        SaveMeasurementData(filePath);
                    }
                    else if (!_isCancelled)
                    {
                        _form.Invoke((MethodInvoker)delegate
                        {
                            _form.UpdateStatusLabel1("計測に失敗しました．");
                            _form.UpdateStatusLabel2("計測に失敗しました．");
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
                            _form.DisconnectCamera(); 
                            _form.CameraButton1Enabled(true); // カメラ接続ボタンを有効にする
                            _form.CameraButton2Enabled(true); // カメラ接続ボタン2を有効にする
                            _form.CalibrationButtonEnabled(true); // キャリブレーションボタンを有効にする
                            _form.LoadButtonEnabled(true); // ロードボタンを有効にする
                            _form.MeasureButtonEnabled(true); // 計測ボタンを有効にする
                        });
                    }

                    _isMeasuring = false;
                }
            }
            catch (Exception ex)
            {
                _form.Invoke((MethodInvoker)delegate
                {
                    _form.UpdateStatusLabel1($"計測中にエラーが発生しました: {ex.Message}");
                    _form.UpdateStatusLabel2($"計測中にエラーが発生しました: {ex.Message}");
                });
            }
            finally
            {
                _isMeasuring = false;
                if (_measurementThread != null && _measurementThread.IsAlive)
                {
                    _measurementThread.Join(1000); // スレッドの終了を待機
                }
            }
        }

        public void ResetMeasurement()
        {
            // 計測リセット
            _cameraMatrix?.Dispose();
            _distCoeffs?.Dispose();
            _cameraMatrix = null;
            _distCoeffs = null;
            _isSaved = false;
        }

        public void CancelMeasurement()
        {
            // 計測中止
            _isMeasuring = false;
            _isCancelled = true;
            if (_measurementThread != null && _measurementThread.IsAlive)
            {
                // スレッドの終了を待機
                _measurementThread.Join(1000); // タイムアウトを設定
            }
        }

        public Mat UndistortImage(Mat image)
        {
            // 画像補正
            if (_cameraMatrix == null || _distCoeffs == null)
            {
                throw new InvalidOperationException("キャリブレーションデータがありません．");
            }

            var undistortedImage = new Mat();
            Cv2.Undistort(image, undistortedImage, _cameraMatrix, _distCoeffs);
            return undistortedImage;
        }

        public void SaveMeasurementData(string filePath)
        {
            // 計測データをXMLファイルに保存
            if (_isCancelled || _isSaved)
            {
                _form.UpdateStatusLabel1("計測が中止されたか、既に保存されています。");
                _form.UpdateStatusLabel2("計測が中止されたか、既に保存されています。");
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
                var root = xmlDoc.CreateElement("MeasurementData");
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
                    var cellElement = xmlDoc.CreateElement("Cell");
                    cellElement.InnerText = _distCoeffs.At<double>(i, 0).ToString();
                    distCoeffsElement.AppendChild(cellElement);
                }
                root.AppendChild(distCoeffsElement);

                // XMLファイルを保存
                xmlDoc.Save(filePath);

                _isSaved = true;
                _form.UpdateStatusLabel2("計測データを保存しました。");
            }
            catch (Exception ex)
            {
                _form.UpdateStatusLabel2($"データ保存中にエラーが発生しました: {ex.Message}");
                // エラーログをファイルに記録
                File.AppendAllText("error_log.txt", $"{DateTime.Now}: {ex.ToString()}\n");
            }
        }
    }
}
