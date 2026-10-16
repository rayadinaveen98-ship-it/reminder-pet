namespace PetPing.Windows.Services;

/// <summary>
/// Small deterministic animation budget shared by the actor and its tests.
/// No timers or WPF dependencies here, so all species/motion choices can be
/// validated without launching Windows.
/// </summary>
public sealed record PetGaitPlan(
    int StepMilliseconds, double FootLift, double BobHeight,
    int BobMilliseconds, double TailAngle, int EarWiggleMilliseconds);

public sealed record PetReactionPlan(
    int DurationMilliseconds, int ParticleCount, double HopHeight,
    bool AllowContinuousIdle);

public static class PetAnimationRules
{
    public static PetGaitPlan Gait(PetSpecies species, PetPersonality personality)
    {
        var source = species switch
        {
            PetSpecies.Cat => new PetGaitPlan(225, 7, 3.0, 560, 11, 0),
            PetSpecies.Dog => new PetGaitPlan(175, 12, 5.5, 350, 24, 0),
            PetSpecies.Bunny => new PetGaitPlan(320, 4, 17.0, 640, 0, 330),
            _ => throw new ArgumentOutOfRangeException(nameof(species))
        };
        return personality switch
        {
            PetPersonality.Playful => source,
            PetPersonality.Calm => source with
            {
                StepMilliseconds = (int)(source.StepMilliseconds * 1.4),
                FootLift = source.FootLift * .65,
                BobHeight = source.BobHeight * .65,
                BobMilliseconds = (int)(source.BobMilliseconds * 1.35)
            },
            PetPersonality.Curious => source with
            {
                StepMilliseconds = (int)(source.StepMilliseconds * .9),
                EarWiggleMilliseconds = source.EarWiggleMilliseconds is > 0
                    ? (int)(source.EarWiggleMilliseconds * .85) : 0
            },
            _ => throw new ArgumentOutOfRangeException(nameof(personality))
        };
    }

    /// <summary>
    /// Off and Gentle are actual effect budgets, not merely UI decoration.
    /// No per-frame particle allocations when effects are disabled.
    /// </summary>
    public static PetReactionPlan Reaction(
        PetProfile profile, bool done, PetMotion motion)
    {
        PetProfileCatalog.Validate(profile);
        if (!Enum.IsDefined(motion))
            throw new ArgumentOutOfRangeException(nameof(motion));
        if (motion == PetMotion.Off)
            return new PetReactionPlan(220, 0, 0, false);

        var hop = done && profile.DoneAction == PetDoneAction.Hop;
        var hopHeight = hop ? profile.Species switch
        {
            PetSpecies.Bunny => 38.0,
            PetSpecies.Dog => 26.0,
            _ => 22.0
        } : 0.0;
        var duration = done ? 900 : 950;
        if (motion == PetMotion.Gentle)
            return new PetReactionPlan(520, 0, hop ? 8 : 0, false);

        var count = profile.Effects switch
        {
            PetEffects.Off => 0,
            PetEffects.Gentle => done ? 4 : 2,
            PetEffects.Playful => done ? 10 : 5,
            _ => throw new ArgumentOutOfRangeException(nameof(profile))
        };
        return new PetReactionPlan(duration, count, hopHeight, true);
    }

    public static int MaxParticles => 10;
    public static int MaxReactionMs => 950;
}
