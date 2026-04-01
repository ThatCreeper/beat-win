using System;
using System.Collections.Generic;
using System.Text;

namespace beat_win;

public static class MathHelpers
{
    public static float Lerp(float from, float to, float x)
    {
        return from + (to - from) * x;
    }
}
