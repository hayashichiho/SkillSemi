using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using System;
using System.Threading;
using System.Xml;
using System.Runtime.InteropServices;

namespace ss2409_01
{
    public class Aruco
    {
        private readonly Form1 _form;

        public Aruco(Form1 form)
        {
            _form = form;
        }

        public static void EstimatePoseSingleMarkers(
            Point2f[][] corners,
            float markerLength,
            Mat cameraMatrix,
            Mat distCoeffs,
            out Vec3d[] rvecs,
            out Vec3d[] tvecs)
        {
            // カメラ行列と歪み係数が存在しない場合はエラーをスロー
            if (cameraMatrix.Empty() || distCoeffs.Empty())
            {
                throw new ArgumentException("キャリブレーションデータが存在しません．");
            }

            // 回転ベクトルと並進ベクトルを取得
            using (var rvecsMat = new Mat()) // 回転ベクトル
            using (var tvecsMat = new Mat()) // 並進ベクトル
            {
                // 位置姿勢を推定
                CvAruco.EstimatePoseSingleMarkers(corners, markerLength, cameraMatrix, distCoeffs, OutputArray.Create(rvecsMat), OutputArray.Create(tvecsMat));

                rvecs = new Vec3d[rvecsMat.Rows];
                tvecs = new Vec3d[tvecsMat.Rows];
                for (int i = 0; i < rvecsMat.Rows; i++)
                {
                    rvecs[i] = rvecsMat.At<Vec3d>(i);
                    tvecs[i] = tvecsMat.At<Vec3d>(i);
                }
            }
        }

        public static Point2f CalculateRulerEndPoint(Point2f markerTopLeft, Point2f markerTopRight, float distance, float markerLength)
        {
            // マーカーの右上と左上の間のベクトルを計算
            var directionVector = new Point2f(
                markerTopRight.X - markerTopLeft.X,
                markerTopRight.Y - markerTopLeft.Y
            );

            // ベクトルを正規化
            var length = Math.Sqrt(directionVector.X * directionVector.X + directionVector.Y * directionVector.Y);
            var normalizedVector = new Point2f(
                (float)(directionVector.X / length),
                (float)(directionVector.Y / length)
            );

            // 比を使用して距離を調整
            var adjustedDistance = (length / markerLength) * distance;

            // 調整後の定規の先端の座標を計算
            return new Point2f(
                markerTopRight.X - normalizedVector.X * (float)adjustedDistance,
                markerTopRight.Y - normalizedVector.Y * (float)adjustedDistance
            );
        }

        public void SaveMeasurementData(string filePath, Point2f[] corners, int id, Point2f rulerEndPoint, bool isInitial)
        {
            // 計測データを保存するメソッド
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    throw new ArgumentException("ファイルパスが無効です。");
                }

                XmlDocument xmlDoc = new XmlDocument();
                XmlElement root;

                if (System.IO.File.Exists(filePath))
                {
                    xmlDoc.Load(filePath);
                    root = xmlDoc.DocumentElement;
                }
                else
                {
                    root = xmlDoc.CreateElement("MeasurementData");
                    xmlDoc.AppendChild(root);
                }

                // マーカーの座標を保存
                var markerElement = xmlDoc.CreateElement(isInitial ? "InitialMarker" : "MovedMarker");
                markerElement.SetAttribute("ID", id.ToString());

                // マーカーの4つの角の座標を保存
                for (int j = 0; j < corners.Length; j++)
                {
                    var cornerElement = xmlDoc.CreateElement("Corner");
                    cornerElement.SetAttribute("X", corners[j].X.ToString());
                    cornerElement.SetAttribute("Y", corners[j].Y.ToString());
                    markerElement.AppendChild(cornerElement);
                }

                root.AppendChild(markerElement);

                // 定規の先端の座標を保存
                var rulerEndPointElement = xmlDoc.CreateElement(isInitial ? "InitialRulerEndPoint" : "MovedRulerEndPoint");
                rulerEndPointElement.SetAttribute("X", rulerEndPoint.X.ToString());
                rulerEndPointElement.SetAttribute("Y", rulerEndPoint.Y.ToString());
                root.AppendChild(rulerEndPointElement);

                xmlDoc.Save(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"データ保存中にエラーが発生しました: {ex.Message}");
            }
        }

        public bool DetectMarkerAndEstimatePose(VideoCapture capture, float markerLength, Mat cameraMatrix, Mat distCoeffs, out Point2f[] detectedCorners, out int detectedId, out Vec3d[] rvecs, out Vec3d[] tvecs)
        {
            detectedCorners = null;
            detectedId = -1;
            rvecs = null;
            tvecs = null;

            while (true)
            {
                using (var frame = new Mat())
                {
                    // カメラからフレームを取得
                    capture.Read(frame);
                    if (frame.Empty())
                    {
                        return false;
                    }

                    // グレースケールに変換
                    var gray = new Mat();
                    Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);

                    // マーカーを検出
                    var dictionary = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict4X4_50);
                    var parameters = new DetectorParameters();
                    CvAruco.DetectMarkers(gray, dictionary, out Point2f[][] corners, out int[] ids, parameters, out _);

                    // マーカーが検出された場合
                    if (ids.Length == 1)
                    {
                        detectedCorners = corners[0];
                        detectedId = ids[0];

                        // 位置姿勢を推定
                        EstimatePoseSingleMarkers(new Point2f[][] { detectedCorners }, markerLength, cameraMatrix, distCoeffs, out rvecs, out tvecs);
                        return true;
                    }
                }
            }
        }
    }
}


