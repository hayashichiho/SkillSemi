#include "Coordinate3D.h"

Coordinate3D::Coordinate3D() {
    _x = 0; _y = 0; _z = 0;
}

Coordinate3D::Coordinate3D(double x, double y, double z) {
    _x = x; _y = y; _z = z;
}

Coordinate3D Coordinate3D::operator=(Coordinate3D coordinate) {
    _x = coordinate._x;
    _y = coordinate._y;
    _z = coordinate._z;
    return *this;
}

Coordinate3D& Coordinate3D::operator-=(Coordinate3D &coordinate) {
    _x -= coordinate._x;
    _y -= coordinate._y;
    _z -= coordinate._z;
    return *this;
}

Coordinate3D& Coordinate3D::operator+=(Coordinate3D &coordinate) {
    _x += coordinate._x;
    _y += coordinate._y;
    _z += coordinate._z;
    return *this;
}

Coordinate3D Coordinate3D::normalize() {
    Coordinate3D ret;
    double norm = std::sqrt(_x * _x + _y * _y + _z * _z);
    if (norm == 0) {
        throw std::runtime_error("(0,0,0)は正規化できません");
    }
    ret._x = _x / norm;
    ret._y = _y / norm;
    ret._z = _z / norm;
    return ret;
}
