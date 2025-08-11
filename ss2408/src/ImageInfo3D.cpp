#include "ImageInfo3D.h"

void ImageInfo3D::showImgInfo() {
    std::cout << "---3DImageInformation---" << std::endl;
    std::cout << "DimSize: " << _dimSizeX << " " << _dimSizeY << " " << _dimSizeZ << std::endl;
    std::cout << "ElementSpacing: " << _elementSpacingX << " "
              << _elementSpacingY << " " << _elementSpacingZ << std::endl;
    std::cout << "ElementType: " << _elementType << std::endl;
    std::cout << "ElementDataFile: " << _elementDataFile << std::endl;
}
