#ifndef COORDINATE_3D_H
#define COORDINATE_3D_H

#include <stdexcept>
#include <cmath>

class Coordinate3D {
public:
    double _x;
    double _y;
    double _z;
    Coordinate3D();
    Coordinate3D(double x, double y, double z);
    Coordinate3D operator=(Coordinate3D coordinate);
    Coordinate3D& operator-=(Coordinate3D &coordinate);
    Coordinate3D& operator+=(Coordinate3D &coordinate);
    Coordinate3D normalize();
};

#endif
