namespace PetPing.Windows.Services;

/// <summary>
/// Single source of truth for the premium cat actor's lifecycle.
/// Voice notifications may be queued on the dispatcher after the user presses
/// Done, Snooze or Mute. Terminal stages cannot restart speech or walking.
/// Unlike WPF storyboard state, transitions are pure and regression-testable.
/// </summary>
public enum CatStage { Stopped, Idle, Walking, Talking, Done, Snoozed }
public enum CatStageEvent { Reset, BeginWalk, Arrive, SpeechStarted, SpeechStopped, Done, Snooze, Stop }

public static class CatAnimationFlowRules
{
    public static CatStage Next(CatStage stage, CatStageEvent action)
    {
        if (!Enum.IsDefined(stage)) throw new ArgumentOutOfRangeException(nameof(stage));
        if (!Enum.IsDefined(action)) throw new ArgumentOutOfRangeException(nameof(action));

        return action switch
        {
            CatStageEvent.Reset => CatStage.Idle,
            CatStageEvent.Stop => CatStage.Stopped,
            CatStageEvent.BeginWalk when stage is CatStage.Idle or CatStage.Walking
                => CatStage.Walking,
            CatStageEvent.Arrive when stage == CatStage.Walking => CatStage.Idle,
            CatStageEvent.SpeechStarted when stage == CatStage.Idle => CatStage.Talking,
            CatStageEvent.SpeechStopped when stage == CatStage.Talking => CatStage.Idle,
            CatStageEvent.Done when stage is CatStage.Idle or CatStage.Talking
                => CatStage.Done,
            CatStageEvent.Snooze when stage is CatStage.Idle or CatStage.Talking
                => CatStage.Snoozed,
            _ => stage
        };
    }

    public static bool CanAnimateSpeech(CatStage stage) => stage == CatStage.Talking;
    public static bool IsTerminal(CatStage stage) =>
        stage is CatStage.Done or CatStage.Snoozed or CatStage.Stopped;
}
