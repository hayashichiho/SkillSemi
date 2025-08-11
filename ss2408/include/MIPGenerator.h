#ifndef MIP_GENERATOR_H
#define MIP_GENERATOR_H

#include "ProcessingInfo.h"
#include "ImageInfo3D.h"
#include "Coordinate3D.h"
#include <limits>

class MIPGenerator {
    int _dimSizeX{};
    int _dimSizeY{};
    int _dimSizeZ{};
    double _elementSpacingX{};
    double _elementSpacingY{};
    double _elementSpacingZ{};
    short* _voxels{};
    int _dimSizeXY{};
    double _halfWidth3D{};
    double _halfHeight3D{};
    double _halfDepth3D{};
    int _halfDimSizeX{};
    int _halfDimSizeY{};
    int _halfDimSizeZ{};
    int _diameter{};
    double _radius{};
    short* _mipPixels{};
    int _mipPixelNum{};
    double _rotation[9]{};
    void rotatePoint(Coordinate3D &point);
public:
    MIPGenerator(ProcessingInfo &processingInfo, ImageInfo3D &imgInfo);
    ~MIPGenerator();
    void renderMIPImage();
    short* getMIPPixels() {
        return _mipPixels;
    }
    int getMIPPixelNum() {
        return _mipPixelNum;
    }
};

#endif
