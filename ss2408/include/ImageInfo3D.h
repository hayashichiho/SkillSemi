#ifndef IMAGE_INFO_3D_H
#define IMAGE_INFO_3D_H

#include "ImageInfo.h"
#include <iostream>

class ImageInfo3D : public ImageInfo {
public:
    int _dimSizeZ{};
    double _elementSpacingZ{};
    virtual void showImgInfo();
};

#endif
