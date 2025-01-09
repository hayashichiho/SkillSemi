using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using OpenCvSharp;
using System.IO;
using System.Xml;
using OpenCvSharp.Extensions;

namespace ss2409_01
{
    public class LoadData
    {
        private readonly Form1 _form;

        public LoadData(Form1 form)
        {
            _form = form;
        }

        // キャリブレーションデータを読み込むメソッド
        public void LoadCalibrationData(string filePath)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);

                var cameraMatrixNode = xmlDoc.SelectSingleNode("//CameraMatrix"); // XMLファイルからカメラ行列を取得
                var distCoeffsNode = xmlDoc.SelectSingleNode("//DistCoeffs"); // XMLファイルから歪み係数を取得

                // カメラ行列と歪み係数が取得できなかった場合は例外をスロー
                if (cameraMatrixNode == null || distCoeffsNode == null)
                {
                    throw new InvalidOperationException("キャリブレーションデータが正しくありません。");
                }

                var cameraMatrix = new Mat(3, 3, MatType.CV_64F);
                var distCoeffs = new Mat(distCoeffsNode.ChildNodes.Count, 1, MatType.CV_64F);

                // カメラ行列と歪み係数を読み込む
                int i = 0;
                foreach (XmlNode rowNode in cameraMatrixNode.ChildNodes)
                {
                    int j = 0;
                    foreach (XmlNode cellNode in rowNode.ChildNodes)
                    {
                        cameraMatrix.Set(i, j, double.Parse(cellNode.InnerText));
                        j++;
                    }
                    i++;
                }

                i = 0;
                foreach (XmlNode cellNode in distCoeffsNode.ChildNodes)
                {
                    distCoeffs.Set(i, 0, double.Parse(cellNode.InnerText));
                    i++;
                }

                // デバッグ情報を追加
                Console.WriteLine("カメラ行列を読み込みました");
                for (i = 0; i < cameraMatrix.Rows; i++)
                {
                    for (int j = 0; j < cameraMatrix.Cols; j++)
                    {
                        Console.Write(cameraMatrix.At<double>(i, j) + " ");
                    }
                    Console.WriteLine();
                }

                Console.WriteLine("歪み係数を読み込みました");
                for (i = 0; i < distCoeffs.Rows; i++)
                {
                    Console.WriteLine(distCoeffs.At<double>(i, 0));
                }

                _form.Calibration.SetCalibrationData(cameraMatrix, distCoeffs);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"キャリブレーションデータの読み込み中にエラーが発生しました: {ex.Message}");
            }
        }
    }
}
