#include "MIPGenerator.h"
#include <iostream>
#include <cmath>
#include <cstdio>

#define PI_180 0.017453292519943

MIPGenerator::MIPGenerator(ProcessingInfo &processingInfo, ImageInfo3D &imgInfo) {
    FILE* fp = fopen(imgInfo._elementDataFile.c_str(), "rb");
    if (!fp) {
        std::cout << "raw画像が開けませんでした" << std::endl;
        return;
    } else {
        if (imgInfo._elementType == "MET_SHORT") {
            _voxels = new short[imgInfo._dimSizeX * imgInfo._dimSizeY * imgInfo._dimSizeZ];
            fread(_voxels, sizeof(short), imgInfo._dimSizeX * imgInfo._dimSizeY * imgInfo._dimSizeZ, fp);
            fclose(fp);
            // 使用する画像情報を保存
            _dimSizeX = imgInfo._dimSizeX;
            _dimSizeY = imgInfo._dimSizeY;
            _dimSizeZ = imgInfo._dimSizeZ;
            _elementSpacingX = imgInfo._elementSpacingX;
            _elementSpacingY = imgInfo._elementSpacingY;
            _elementSpacingZ = imgInfo._elementSpacingZ;
            _dimSizeXY = _dimSizeX * _dimSizeY;

            double width3D, height3D, depth3D;
            width3D = _dimSizeX * _elementSpacingX;
            height3D = _dimSizeY * _elementSpacingY;
            depth3D = _dimSizeZ * _elementSpacingZ;

            _halfWidth3D = width3D * 0.5;
            _halfHeight3D = height3D * 0.5;
            _halfDepth3D = depth3D * 0.5;

            _halfDimSizeX = _dimSizeX / 2;
            _halfDimSizeY = _dimSizeY / 2;
            _halfDimSizeZ = _dimSizeZ / 2;

            _diameter = (int)std::sqrt(width3D * width3D + height3D * height3D + depth3D * depth3D);
            _radius = _diameter / 2.0;

            _mipPixelNum = _diameter * _diameter;
            _mipPixels = new short[_mipPixelNum];

            double phi, theta, psi;
            phi = processingInfo.viewAnglePhi * PI_180;
            theta = processingInfo.viewAngleTheta * PI_180;
            psi = processingInfo.viewAnglePsi * PI_180;
            _rotation[0] = std::cos(phi) * std::cos(theta) * std::cos(psi) - (std::sin(phi) * std::sin(psi));
            _rotation[1] = -(std::cos(phi) * std::cos(theta) * std::sin(psi)) - (std::sin(phi) * std::cos(psi));
            _rotation[2] = std::cos(phi) * std::sin(theta);
            _rotation[3] = std::sin(phi) * std::cos(theta) * std::cos(psi) + std::cos(phi) * std::sin(psi);
            _rotation[4] = -(std::sin(phi) * std::cos(theta) * std::sin(psi)) + std::cos(phi) * std::cos(psi);
            _rotation[5] = std::sin(phi) * std::sin(theta);
            _rotation[6] = -(std::sin(theta) * std::cos(psi));
            _rotation[7] = std::sin(theta) * std::sin(psi);
            _rotation[8] = std::cos(theta);
        } else {
            std::cout << "raw画像のデータ型はshort型のみが有効です" << std::endl;
            fclose(fp);
        }
    }
}

MIPGenerator::~MIPGenerator() {
    delete[] _voxels;
    delete[] _mipPixels;
}

inline void MIPGenerator::rotatePoint(Coordinate3D &point) {
    double originalx = point._x;
    double originaly = point._y;
    double originalz = point._z;
    point._x = _rotation[0] * originalx + _rotation[1] * originaly + _rotation[2] * originalz;
    point._y = _rotation[3] * originalx + _rotation[4] * originaly + _rotation[5] * originalz;
    point._z = _rotation[6] * originalx + _rotation[7] * originaly + _rotation[8] * originalz;
}

void MIPGenerator::renderMIPImage() {
    Coordinate3D center(0, 0, -_radius);
    rotatePoint(center);
    Coordinate3D normal(0, 0, 0);
    normal -= center;
    try {
        normal = normal.normalize();
    } catch (const std::runtime_error& error) {
        std::cout << error.what() << std::endl;
    }

    int centerVoxel = +_halfDimSizeZ * _dimSizeXY + _halfDimSizeY * _dimSizeX + _halfDimSizeX;
    double weightZ = 1.0 / _elementSpacingZ;
    double weightY = 1.0 / _elementSpacingY;
    double weightX = 1.0 / _elementSpacingX;

#pragma omp parallel for
    for (int j = 0; j < _diameter; j++) {
        for (int i = 0; i < _diameter; i++) {
            Coordinate3D point(-_radius + i, -_radius + j, -_radius);
            rotatePoint(point);
            short max = std::numeric_limits<short>::min();
            short value;
            bool isEdge = false;
            for (int k = 0; k < _diameter; k++) {
                if (point._x < -_halfWidth3D || _halfWidth3D < point._x ||
                    point._y < -_halfHeight3D || _halfHeight3D < point._y ||
                    point._z < -_halfDepth3D || _halfDepth3D < point._z) {
                    if (isEdge) {
                        break;
                    }
                } else {
                    if (!isEdge) {
                        isEdge = true;
                    }
                    value = *(_voxels + (int)(point._z * weightZ) * _dimSizeXY
                                        + (int)(point._y * weightY) * _dimSizeX
                                        + (int)(point._x * weightX)
                                        + centerVoxel);
                    if (value > max) {
                        max = value;
                    }
                }
                point += normal;
            }
            _mipPixels[j * _diameter + i] = max;
        }
    }
}
