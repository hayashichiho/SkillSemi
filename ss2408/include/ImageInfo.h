#ifndef IMAGE_INFO_H
#define IMAGE_INFO_H

#include <string>

class ImageInfo {
public:
    int _dimSizeX{};
    int _dimSizeY{};
    double _elementSpacingX = 1.0;
    double _elementSpacingY = 1.0;
    std::string _elementType;
    std::string _elementDataFile;
    virtual void showImgInfo() = 0;
};

#endif
