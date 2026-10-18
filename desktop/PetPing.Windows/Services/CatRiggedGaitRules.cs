namespace PetPing.Windows.Services;

/// <summary>
/// Kinematic motion study ONLY. Four independent planted/swinging feline legs
/// with two-bone inverse kinematics. This is NOT a finished textured 3D model,
/// renderer or accepted visual walking cycle.
/// Coordinates correspond to the 640x850 approved walking reference canvas.
/// The spec is deliberately pure for cross-platform gait/contact tests.
/// </summary>
public enum CatLegId { HindFar, FrontFar, HindNear, FrontNear }

public readonly record struct CatPoint(double X, double Y);
public readonly record struct CatLegPose(
    CatLegId Leg, CatPoint Hip, CatPoint Knee, CatPoint Paw,
    bool Planted, double StridePhase);

public static class CatRiggedGaitRules
{
    public const int FullCycleMilliseconds = 1000;
    public const int GentleCycleMilliseconds = 1400;
    public const int ProofFrameCount = 32;
    public const double DutyFactor = 0.60;

    private sealed record LegSpec(
        CatPoint Hip, CatPoint Paw, double Upper, double Lower,
        double Stride, double Lift, double Phase, int Bend);

    private static readonly LegSpec[] Legs =
    [
        new(new(179,567),new(171,741),110,122,72,41,.51,-1),
        new(new(322,544),new(304,748),113,121,67,48,.01, 1),
        new(new(177,564),new(183,773),110,126,80,54,.06,-1),
        new(new(407,536),new(419,783),133,140,93,63,.55, 1)
    ];

    public static CatLegPose Sample(long elapsedMilliseconds, CatLegId leg,
        PetMotion motion = PetMotion.Full)
    {
        if (!Enum.IsDefined(leg)) throw new ArgumentOutOfRangeException(nameof(leg));
        if (!Enum.IsDefined(motion))
            throw new ArgumentOutOfRangeException(nameof(motion));
        var spec = Legs[(int)leg];
        if (motion == PetMotion.Off)
        {
            var kneeStill = SolveKnee(spec.Hip, spec.Paw,
                spec.Upper, spec.Lower, spec.Bend);
            return new CatLegPose(leg,spec.Hip,kneeStill,spec.Paw,true,0);
        }

        var cycle = motion == PetMotion.Gentle
            ? GentleCycleMilliseconds : FullCycleMilliseconds;
        // Modulo normalization keeps phases periodic even for negative times
        // supplied by an animation preview / seek.
        var ticks = ((elapsedMilliseconds % cycle) + cycle) % cycle;
        var t = (double)ticks / cycle;
        var phase = (t + spec.Phase) % 1.0;
        var planted = phase < DutyFactor;
        double stepX, liftY;
        if (planted)
        {
            stepX = spec.Stride * .5 -
                    spec.Stride * (phase / DutyFactor);
            liftY = 0;
        }
        else
        {
            var k = (phase - DutyFactor) / (1.0 - DutyFactor);
            stepX = -spec.Stride*.5 +
                spec.Stride*(.5 - .5*Math.Cos(Math.PI*k));
            liftY = -spec.Lift * Math.Pow(Math.Sin(Math.PI*k),1.15);
        }
        var bob = motion == PetMotion.Full
            ? 3.5 * Math.Sin(Math.PI * 4 * t - .3)
            : 1.5 * Math.Sin(Math.PI * 4 * t - .3);
        var hip = spec.Hip with { Y = spec.Hip.Y + bob };
        var paw = new CatPoint(spec.Paw.X + stepX, spec.Paw.Y + liftY);
        var knee = SolveKnee(hip,paw,spec.Upper,spec.Lower,spec.Bend);
        return new CatLegPose(leg,hip,knee,paw,planted,phase);
    }

    public static IReadOnlyList<CatLegPose> SampleAll(long elapsedMilliseconds,
        PetMotion motion = PetMotion.Full) =>
        Enum.GetValues<CatLegId>()
            .Select(leg => Sample(elapsedMilliseconds,leg,motion)).ToArray();

    private static CatPoint SolveKnee(
        CatPoint hip, CatPoint paw, double upper, double lower, int bend)
    {
        var dx = paw.X - hip.X;
        var dy = paw.Y - hip.Y;
        var d = Math.Sqrt(dx*dx + dy*dy);
        if (d > upper + lower - 1)
        {
            var f = (upper+lower-1) / d;
            dx *= f;
            dy *= f;
            d = Math.Sqrt(dx*dx + dy*dy);
        }
        d = Math.Max(d,Math.Abs(upper-lower)+1);
        var projection = (upper*upper - lower*lower + d*d)/(2*d);
        var height = Math.Sqrt(Math.Max(0,upper*upper-projection*projection));
        return new CatPoint(
            hip.X + projection*dx/d + bend*(-dy/d)*height,
            hip.Y + projection*dy/d + bend*( dx/d)*height);
    }
}
