#pragma once

struct EulerAngles {
    double phi;   // X軸周りの回転
    double theta; // Y軸周りの回転
    double psi;   // Z軸周りの回転

    EulerAngles(double phi = 0.0, double theta = 0.0, double psi = 0.0)
        : phi(phi), theta(theta), psi(psi) {}
};