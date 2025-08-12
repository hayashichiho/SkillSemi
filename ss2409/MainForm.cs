using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Aruco;
using System.Linq;

namespace CameraApp
{
    public partial class MainForm : Form
    {
        private VideoCapture cap;
        private readonly List<Mat> calibImgs = new List<Mat>(); // キャリブレーション用の画像を格納するリスト
        private int calibCount = 0; // キャリブレーション回数をカウント
        private bool isCalib = false; // キャリブレーション中かどうかを示すフラグ
        private readonly object lockObj = new object(); // コレクションのロック用オブジェクト
        private int capInterval = 30; // 撮影間隔（フレーム数）
        private int capCount = 0; // 撮影枚数
        private int targetCapCount = 0; // 目標撮影枚数
        private bool firstFrameChecked = false; // 最初のフレームがチェックされたかどうかを示すフラグ

        public MainForm()
        {
            InitializeComponent();
            StartCameraAsync();
        }

        private async void StartCameraAsync() // カメラを非同期で起動
        {
            await Task.Run(() => StartCamera());
        }

        private void StartCamera() // カメラを起動
        {
            try
            {
                cap = new VideoCapture(0);
                if (!cap.IsOpened())
                {
                    throw new Exception("カメラを起動できませんでした");
                }

                // カメラの解像度を設定
                cap.Set(VideoCaptureProperties.FrameWidth, 1920);
                cap.Set(VideoCaptureProperties.FrameHeight, 1080);

                Task.Run(() => ProcessFrame());
            }
            catch (Exception excpt)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show("カメラの起動に失敗しました: " + excpt.Message);
                }));
            }
        }

        private void ProcessFrame() // カメラのフレームを処理
        {
            while (cap.IsOpened())
            {
                using (Mat frame = new Mat()) // フレームを取得
                {
                    cap.Read(frame);
                    if (!frame.Empty())
                    {
                        UpdateFrame(frame);

                        if (isCalib && !firstFrameChecked)
                        {
                            firstFrameChecked = true;
                            if (!CheckForMarkers(frame))
                            {
                                ShowMessage("ArUcoマーカーが見つかりませんでした．");
                                Console.WriteLine("ArUcoマーカーが見つかりませんでした．");
                                isCalib = false;
                            }
                        }

                        if (isCalib && capCount % capInterval == 0)
                        {
                            lock (lockObj)
                            {
                                calibImgs.Add(frame.Clone());
                            }
                            Console.WriteLine("画像を撮影しました");

                            if (calibImgs.Count >= targetCapCount)
                            {
                                isCalib = false; // キャリブレーションモードを終了
                                Task.Run(() => PerformCalibAsync());
                            }
                        }
                        capCount++;
                    }
                    else
                    {
                        Console.WriteLine("フレームの取得に失敗しました");
                    }
                }
            }
        }

        private bool CheckForMarkers(Mat frame)
        {
            var dict = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict4X4_50);
            var detectorParams = new DetectorParameters();
            Point2f[][] corners;
            int[] ids;
            Point2f[][] rejectedImgPoints;

            CvAruco.DetectMarkers(frame, dict, out corners, out ids, detectorParams, out rejectedImgPoints);

            return ids.Length > 0;
        }

        private void UpdateFrame(Mat frame)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<Mat>(UpdateFrame), frame);
            }
            else
            {
                picBoxCalib.Image = BitmapConverter.ToBitmap(frame);
                picBoxMeasure.Image = BitmapConverter.ToBitmap(frame);
            }
        }

        private void BtnStartCalib_Click(object sender, EventArgs e) // キャリブレーション開始
        {
            if (!int.TryParse(txtCapInterval.Text, out capInterval) || !int.TryParse(txtTargetCapCount.Text, out targetCapCount))
            {
                MessageBox.Show("撮影間隔と撮影枚数を正しく入力してください。");
                return;
            }

            isCalib = true;
            firstFrameChecked = false; // フラグをリセット
            MessageBox.Show("キャリブレーションモードを開始しました．");
            Console.WriteLine("キャリブレーションモードを開始しました．");
        }

        private async Task PerformCalibAsync()
        {
            List<Mat> calibImgsCopy;
            lock (lockObj)
            {
                calibImgsCopy = new List<Mat>(calibImgs);
            }

            var objPoints = new List<List<Point3f>>(); // ArUcoマーカーのコーナー座標を格納するリスト 
            var imgPoints = new List<List<Point2f>>(); // ArUcoマーカーのコーナー座標を格納するリスト

            var dict = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict4X4_50);
            var detectorParams = new DetectorParameters();

            bool markerFound = false;

            foreach (Mat img in calibImgsCopy)
            {
                Point2f[][] corners;
                int[] ids;
                Point2f[][] rejectedImgPoints;

                CvAruco.DetectMarkers(img, dict, out corners, out ids, detectorParams, out rejectedImgPoints);

                if (ids.Length > 0)
                {
                    markerFound = true;
                    CvAruco.DrawDetectedMarkers(img, corners, ids);
                    UpdateFrame(img);

                    foreach (var corner in corners)
                    {
                        imgPoints.Add(corner.ToList());
                        objPoints.Add(new List<Point3f> { new Point3f(0, 0, 0), new Point3f(1, 0, 0), new Point3f(1, 1, 0), new Point3f(0, 1, 0) });
                    }
                }
            }

            if (!markerFound)
            {
                ShowMessage("キャリブレーション用の画像にArUcoマーカーが見つかりませんでした．");
                Console.WriteLine("キャリブレーション用の画像にArUcoマーカーが見つかりませんでした．");
                return;
            }

            var camMatrix = new Mat();
            var distCoeffs = new Mat();

            // ここで objPoints と imgPoints を Mat に変換
            var objPointsMat = objPoints.Select(op => Mat.FromArray(op.ToArray())).ToList();
            var imgPointsMat = imgPoints.Select(ip => Mat.FromArray(ip.ToArray())).ToList();

            // キャリブレーション実行
            try
            {
                await Task.Run(() =>
                {
                    if (objPointsMat.Count < 4 || imgPointsMat.Count < 4)
                    {
                        Console.WriteLine("キャリブレーションに必要なポイントが不足しています。");
                        return;
                    }

                    Cv2.CalibrateCamera(
                        objPointsMat,
                        imgPointsMat,
                        calibImgsCopy[0].Size(),
                        camMatrix,
                        distCoeffs,
                        out _, // 出力: 回転ベクトル
                        out _, // 出力: 並進ベクトル
                        CalibrationFlags.None,
                        new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.Count, 30, 0.001));
                });
            }
            catch (OpenCVException ex)
            {
                ShowMessage("キャリブレーション中にエラーが発生しました: " + ex.Message);
                Console.WriteLine("キャリブレーション中にエラーが発生しました: " + ex.Message);
                return;
            }

            calibCount++;
            ShowMessage($"{calibCount}回目のキャリブレーションが完了しました．\n次のステップ:\n1. ArUcoマーカーを異なる角度や距離から撮影してください。\n2. 撮影するときは，撮影ボタンを押してください．\n3. キャリブレーションを終了するときは，再度キャリブレーションボタンを押してください．");
            Console.WriteLine($"{calibCount}回目のキャリブレーションが完了しました．\n次のステップ:\n1. ArUcoマーカーを異なる角度や距離から撮影してください。\n2. 撮影するときは，撮影ボタンを押してください。\n3. キャリブレーションを終了するときは，再度キャリブレーションを押してください．");
            Console.WriteLine("カメラ行列: " + camMatrix.Dump());
            Console.WriteLine("歪み係数: " + distCoeffs.Dump());

            using (var fs = new FileStorage("cameraParams.xml", FileStorage.Modes.Write))
            {
                fs.Write("camMatrix", camMatrix);
                fs.Write("distCoeffs", distCoeffs);
            }

            // キャリブレーション終了
            EndCalib();
        }

        private void ShowMessage(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(ShowMessage), message);
            }
            else
            {
                MessageBox.Show(message);
            }
        }

        private void BtnSaveData_Click(object sender, EventArgs e) // データ保存
        {
            using (var fs = new FileStorage("calibData.xml", FileStorage.Modes.Write))
            {
                lock (lockObj)
                {
                    foreach (var img in calibImgs)
                    {
                        fs.Write("image", img);
                    }
                }
            }
            MessageBox.Show("データを保存しました．");
            Console.WriteLine("データを保存しました．");
        }

        private void BtnLoadData_Click(object sender, EventArgs e) // データ読み込み
        {
            // データ読み込みの処理をここに追加
        }

        private void BtnMeasure_Click(object sender, EventArgs e) // 計測
        {
            // 計測の処理をここに追加
        }

        private void EndCalib()
        {
            ShowMessage("キャリブレーションを終了しました．");
            Console.WriteLine("キャリブレーションを終了しました．");
            isCalib = false;
            firstFrameChecked = false; // フラグをリセット
            lock (lockObj)
            {
                calibImgs.Clear();
            }
        }

        private void ShowDetailedExplanation()
        {
            Console.WriteLine("このアプリケーションは、カメラを使用してキャリブレーションを行うためのものです。");
            Console.WriteLine("1. カメラの起動: カメラを初期化し、解像度を設定します。");
            Console.WriteLine("2. フレームの処理: カメラからフレームを取得し、表示します。キャリブレーションモードが有効な場合、一定間隔でフレームを保存します。");
            Console.WriteLine("3. キャリブレーションの開始と終了: キャリブレーションモードを開始または終了します。");
            Console.WriteLine("4. キャリブレーションの実行: 保存したフレームを使用してカメラのキャリブレーションを行います。ArUcoマーカーを検出し、カメラ行列と歪み係数を計算します。");
            Console.WriteLine("5. データの保存と読み込み: キャリブレーション用の画像データを保存します。");
            Console.WriteLine("6. 計測: 計測の処理を追加する予定です。");
            Console.WriteLine("ArUcoマーカーを使ったキャリブレーションは、カメラの内部パラメータ（カメラ行列と歪み係数）を求めるためのプロセスです。これにより、カメラの歪みを補正し、正確な画像処理を行うことができます。");
        }

        private void txtMeasureDist_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtCapInterval_TextChanged(object sender, EventArgs e)
        {

        }
    }
}










