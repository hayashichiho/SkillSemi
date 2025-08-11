#include "ImageInfo2D.h"

void ImageInfo2D::showImgInfo() {
    std::cout << "---2DImageInformation---" << std::endl;
    std::cout << "DimSize: " << _dimSizeX << " " << _dimSizeY << std::endl;
    std::cout << "ElementSpacing: " << _elementSpacingX << " " << _elementSpacingY << std::endl;
    std::cout << "ElementType: " << _elementType << std::endl;
    std::cout << "ElementDataFile: " << _elementDataFile << std::endl;
}
