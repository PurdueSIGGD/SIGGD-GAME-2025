uint globalSeed = 4242;

float random1d(inout uint state) {
    state ^= state << 13;
    state ^= state >> 17;
    state ^= state << 5;
    return float(state & 0xffffffu) / float(0xffffff);
}

// Source: https://www.youtube.com/watch?v=Qz0KTGYJtUk
float randomNormalDist(inout uint state) {
    float theta = 2.0 * 3.14159265 * random1d(state);
    float rho = sqrt(-2.0 * log(random1d(state)));
    return rho * cos(theta);
}