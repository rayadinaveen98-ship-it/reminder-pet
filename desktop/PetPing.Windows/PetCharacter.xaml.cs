using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PetPing.Windows.Services;

namespace PetPing.Windows;

/// <summary>
/// Vector character rig shared by My Pets, Studio, Home and the actual reminder
/// actor. Body, ears, tail, face and accessories change with the saved profile.
/// </summary>
public partial class PetCharacter : System.Windows.Controls.UserControl
{
    private PetProfile _profile = new();
    private int _effectsGeneration;
    private bool _catSpriteEnabled;
    private PetMotion _requestedMotion = PetMotion.Full;
    public bool UsingPremiumCat => _catSpriteEnabled;
    public bool UsingPremiumWalk => _catSpriteEnabled && CatIdleOverlay.WalkAvailable;
    public bool UsingV2GaitStudy => _catSpriteEnabled && CatIdleOverlay.WalkV2Available;
    public bool UsingPremiumTalk => _catSpriteEnabled && CatIdleOverlay.TalkAvailable;

    public PetCharacter()
    {
        InitializeComponent();
    }

    public void Apply(PetProfile profile)
    {
        PetProfileCatalog.Validate(profile);
        Stop();
        _profile = profile;
        // The approved orange base artwork has fixed colors and collar.
        // Respect user-owned palettes/accessories by retaining the old rig
        // whenever they differ from the approved design.
        _catSpriteEnabled = profile.Species == PetSpecies.Cat &&
            profile.Palette == PetPalette.Apricot &&
            profile.Accessory == PetAccessory.None &&
            CatIdleOverlay.TryLoad();
        CatIdleOverlay.SetMotion(_requestedMotion);
        CatIdleOverlay.Visibility = _catSpriteEnabled ? Visibility.Visible : Visibility.Collapsed;
        VectorArt.Visibility = _catSpriteEnabled ? Visibility.Collapsed : Visibility.Visible;
        if (_catSpriteEnabled) CatIdleOverlay.SetIdle();
        CatEars.Visibility = profile.Species == PetSpecies.Cat ? Visibility.Visible : Visibility.Collapsed;
        CatTail.Visibility = CatEars.Visibility;
        CatWhiskers.Visibility = CatEars.Visibility;
        DogEars.Visibility = profile.Species == PetSpecies.Dog ? Visibility.Visible : Visibility.Collapsed;
        DogTail.Visibility = DogEars.Visibility;
        DogMuzzle.Visibility = DogEars.Visibility;
        BunnyEars.Visibility = profile.Species == PetSpecies.Bunny ? Visibility.Visible : Visibility.Collapsed;
        BunnyTail.Visibility = BunnyEars.Visibility;
        BunnyMuzzle.Visibility = BunnyEars.Visibility;
        BowAccessory.Visibility = profile.Accessory == PetAccessory.Bow ? Visibility.Visible : Visibility.Collapsed;
        ScarfAccessory.Visibility = profile.Accessory == PetAccessory.Scarf ? Visibility.Visible : Visibility.Collapsed;

        var (coat, light, edge) = profile.Palette switch
        {
            PetPalette.Cocoa => ("#BB8B65", "#D7A885", "#805D4A"),
            PetPalette.Snow => ("#F4EDE7", "#FFFDFC", "#B5ADB2"),
            PetPalette.Lavender => ("#C7B6E8", "#E4D8F6", "#8C7BAA"),
            _ => ("#E5A06D", "#F0B17E", "#AE7353")
        };
        var coatBrush = Brush(coat);
        var faceBrush = Brush(light);
        var edgeBrush = Brush(edge);
        Torso.Fill = coatBrush;
        Face.Fill = faceBrush;
        Torso.Stroke = edgeBrush;
        Face.Stroke = edgeBrush;
        PawLeft.Fill = coatBrush;
        PawRight.Fill = coatBrush;
        FrontPawLeft.Fill = coatBrush;
        FrontPawRight.Fill = coatBrush;
        CatEarLeft.Fill = coatBrush;
        CatEarRight.Fill = coatBrush;
        DogEarLeft.Fill = coatBrush;
        DogEarRight.Fill = coatBrush;
        CatTail.Stroke = coatBrush;
        DogTail.Stroke = coatBrush;
        Chest.Fill = Brush("#FFF3E4");
        if (profile.Species == PetSpecies.Bunny)
        {
            BunnyEarLeft.Fill = faceBrush;
            BunnyEarRight.Fill = faceBrush;
        }

        var size = profile.Size switch
        {
            PetSize.Small => .82,
            PetSize.Large => 1.12,
            _ => 1.0
        };
        PetSizing.ScaleX = size;
        PetSizing.ScaleY = size;
        System.Windows.Automation.AutomationProperties.SetName(this,
            PetProfileCatalog.Name(profile.Species) + " character preview");
    }

    private static SolidColorBrush Brush(string hex) =>
        new((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex));

    public void FaceToward(bool movingRight) => PetFacing.ScaleX = movingRight ? 1 : -1;

    public void SetMotion(PetMotion motion)
    {
        _requestedMotion = motion;
        if (_catSpriteEnabled) CatIdleOverlay.SetMotion(motion);
    }

    private void ShowCatSprite(bool active)
    {
        if (!_catSpriteEnabled) return;
        VectorArt.Visibility = active ? Visibility.Collapsed : Visibility.Visible;
        CatIdleOverlay.Visibility = active ? Visibility.Visible : Visibility.Collapsed;
        if (active) CatIdleOverlay.Start();
        else CatIdleOverlay.Pause();
    }

    private void ResetCharacterMotion()
    {
        LeftFootShift.BeginAnimation(TranslateTransform.YProperty, null);
        RightFootShift.BeginAnimation(TranslateTransform.YProperty, null);
        PetFloat.BeginAnimation(TranslateTransform.YProperty, null);
        TailWag.BeginAnimation(RotateTransform.AngleProperty, null);
        DogTailWag.BeginAnimation(RotateTransform.AngleProperty, null);
        DogEarTilt.BeginAnimation(RotateTransform.AngleProperty, null);
        BunnyEarTilt.BeginAnimation(RotateTransform.AngleProperty, null);
        PawWaveLeft.BeginAnimation(RotateTransform.AngleProperty, null);
        PawWaveRight.BeginAnimation(RotateTransform.AngleProperty, null);
        LeftFootShift.Y = 0;
        RightFootShift.Y = 0;
        PetFloat.Y = 0;
        TailWag.Angle = 0;
        DogTailWag.Angle = 0;
        DogEarTilt.Angle = 0;
        BunnyEarTilt.Angle = 0;
        PawWaveLeft.Angle = 0;
        PawWaveRight.Angle = 0;
    }

    private static DoubleAnimation Cycle(double start, double end, int milliseconds) => new(
        start, end, TimeSpan.FromMilliseconds(milliseconds))
    {
        AutoReverse = true,
        RepeatBehavior = RepeatBehavior.Forever,
        EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
    };

    public void Walk(bool active)
    {
        ResetCharacterMotion();
        // Walking needs its own pose-specific frames. Until approved, use
        // the existing walking rig without showing a sitting cat sliding.
        if (_catSpriteEnabled)
        {
            if (active && CatIdleOverlay.WalkAvailable)
            {
                ShowCatSprite(true);
                CatIdleOverlay.SetWalking(true);
                return; // no older cat rig flashes during actual walking
            }
            ShowCatSprite(!active);
            if (!active) CatIdleOverlay.SetIdle();
        }
        if (!active) return;
        var gait = PetAnimationRules.Gait(_profile.Species, _profile.Personality);
        LeftFootShift.BeginAnimation(TranslateTransform.YProperty,
            Cycle(0, -gait.FootLift, gait.StepMilliseconds));
        RightFootShift.BeginAnimation(TranslateTransform.YProperty,
            Cycle(-gait.FootLift, 0, gait.StepMilliseconds));
        PetFloat.BeginAnimation(TranslateTransform.YProperty,
            Cycle(0, -gait.BobHeight, gait.BobMilliseconds / 2));

        switch (_profile.Species)
        {
            case PetSpecies.Cat:
                TailWag.BeginAnimation(RotateTransform.AngleProperty,
                    Cycle(-gait.TailAngle, gait.TailAngle, 390));
                break;
            case PetSpecies.Dog:
                DogTailWag.BeginAnimation(RotateTransform.AngleProperty,
                    Cycle(-gait.TailAngle, gait.TailAngle, 205));
                DogEarTilt.BeginAnimation(RotateTransform.AngleProperty,
                    Cycle(-5, 5, 290));
                break;
            case PetSpecies.Bunny:
                BunnyEarTilt.BeginAnimation(RotateTransform.AngleProperty,
                    Cycle(-7, 7, gait.EarWiggleMilliseconds));
                break;
        }
    }

    /// <summary>Species-specific idle, stopped on voice reaction, exit or sleep.</summary>
    public void Float(bool active)
    {
        ResetCharacterMotion();
        if (_catSpriteEnabled)
        {
            ShowCatSprite(true);
            CatIdleOverlay.SetMotion(active ? PetMotion.Full : PetMotion.Gentle);
            CatIdleOverlay.SetIdle();
            return;
        }
        if (!active) return;
        var calm = _profile.Personality == PetPersonality.Calm;
        var bob = _profile.Species == PetSpecies.Bunny ? -7.0 : -3.0;
        PetFloat.BeginAnimation(TranslateTransform.YProperty,
            Cycle(0, bob, calm ? 1300 : 800));
        if (_profile.Species == PetSpecies.Cat)
            TailWag.BeginAnimation(RotateTransform.AngleProperty, Cycle(-5, 9, 1200));
        else if (_profile.Species == PetSpecies.Dog)
        {
            DogTailWag.BeginAnimation(RotateTransform.AngleProperty,
                Cycle(-17, 17, calm ? 550 : 310));
            DogEarTilt.BeginAnimation(RotateTransform.AngleProperty, Cycle(-3, 3, 900));
        }
        else
            BunnyEarTilt.BeginAnimation(RotateTransform.AngleProperty,
                Cycle(-7, 8, calm ? 1450 : 740));
    }

    public void Blink()
    {
        if (_catSpriteEnabled && CatIdleOverlay.IsVisible) return; // sprite blinks on its own
        var animation = new DoubleAnimation(1, .1, TimeSpan.FromMilliseconds(100))
        { AutoReverse = true };
        LeftBlink.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        RightBlink.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
    }

    public void OnSpeechWord()
    {
        if (_catSpriteEnabled && CatIdleOverlay.IsVisible)
            CatIdleOverlay.OnSpeechWord();
    }

    public void SetSpeaking(bool speaking, bool animate)
    {
        if (_catSpriteEnabled && CatIdleOverlay.IsVisible)
        {
            CatIdleOverlay.SetSpeaking(speaking);
            return;
        }
        MouthSmile.Visibility = speaking ? Visibility.Collapsed : Visibility.Visible;
        MouthSpeak.Visibility = speaking ? Visibility.Visible : Visibility.Collapsed;
        MouthSpeak.BeginAnimation(OpacityProperty, null);
        if (speaking && animate)
            MouthSpeak.BeginAnimation(OpacityProperty,
                Cycle(.55, 1, _profile.Species == PetSpecies.Bunny ? 250 : 185));
    }

    /// <summary>
    /// Called only after the Done/Snooze storage operation succeeded. All
    /// effects are finite, bounded by PetAnimationRules, and stoppable.
    /// </summary>
    public int React(bool completed, PetMotion motion)
    {
        var plan = PetAnimationRules.Reaction(_profile, completed, motion);
        if (_catSpriteEnabled)
        {
            // Keep the stage alive until the user response is displayed.
            // Calling Stop() here would make Done/Snooze an invalid
            // transition and leave the sprite showing its previous mouth.
            _effectsGeneration++;
            ResetCharacterMotion();
            EffectStage.Children.Clear();
            ShowCatSprite(true);
            CatIdleOverlay.SetMotion(motion);
            CatIdleOverlay.SetResponse(completed);
            if (motion != PetMotion.Off)
            {
                if (completed && motion == PetMotion.Full &&
                    _profile.DoneAction == PetDoneAction.Hop)
                    PetFloat.BeginAnimation(TranslateTransform.YProperty,
                        new DoubleAnimation(0, -plan.HopHeight,
                            TimeSpan.FromMilliseconds(290))
                        { AutoReverse = true, EasingFunction = new SineEase() });
            }
            return plan.DurationMilliseconds;
        }
        Stop();
        if (motion == PetMotion.Off) return plan.DurationMilliseconds;
        var shortMotion = motion == PetMotion.Gentle;
        var speed = shortMotion ? 210 : 310;
        if (completed)
        {
            if (_profile.DoneAction == PetDoneAction.Hop)
                PetFloat.BeginAnimation(TranslateTransform.YProperty,
                    new DoubleAnimation(0, -plan.HopHeight, TimeSpan.FromMilliseconds(speed))
                    { AutoReverse = true, EasingFunction = new SineEase() });
            else
            {
                PawWaveLeft.BeginAnimation(RotateTransform.AngleProperty,
                    new DoubleAnimation(0, -34, TimeSpan.FromMilliseconds(speed))
                    { AutoReverse = true });
                PawWaveRight.BeginAnimation(RotateTransform.AngleProperty,
                    new DoubleAnimation(0, 27, TimeSpan.FromMilliseconds(speed))
                    { AutoReverse = true });
            }
            if (!shortMotion && _profile.Species == PetSpecies.Dog)
                DogTailWag.BeginAnimation(RotateTransform.AngleProperty,
                    Cycle(-24, 24, 145));
            if (!shortMotion && _profile.Species == PetSpecies.Bunny)
                BunnyEarTilt.BeginAnimation(RotateTransform.AngleProperty,
                    Cycle(-12, 12, 180));
        }
        else if (_profile.SnoozeAction == PetSnoozeAction.Yawn)
        {
            MouthSmile.Visibility = Visibility.Collapsed;
            MouthSpeak.Visibility = Visibility.Visible;
            MouthSpeak.BeginAnimation(OpacityProperty,
                new DoubleAnimation(.75, 1, TimeSpan.FromMilliseconds(340))
                { AutoReverse = true });
            if (!shortMotion)
            {
                // Droopy ears and sleepy eyes give bunny/dog a distinct yawn.
                Blink();
                if (_profile.Species == PetSpecies.Bunny)
                    BunnyEarTilt.BeginAnimation(RotateTransform.AngleProperty,
                        new DoubleAnimation(0, -13, TimeSpan.FromMilliseconds(340))
                        { AutoReverse = true });
                if (_profile.Species == PetSpecies.Dog)
                    DogEarTilt.BeginAnimation(RotateTransform.AngleProperty,
                        new DoubleAnimation(0, 10, TimeSpan.FromMilliseconds(340))
                        { AutoReverse = true });
            }
        }
        else
        {
            PawWaveLeft.BeginAnimation(RotateTransform.AngleProperty,
                new DoubleAnimation(0, -20, TimeSpan.FromMilliseconds(speed))
                { AutoReverse = true });
            PawWaveRight.BeginAnimation(RotateTransform.AngleProperty,
                new DoubleAnimation(0, 17, TimeSpan.FromMilliseconds(speed))
                { AutoReverse = true });
            PetFloat.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(0, 7, TimeSpan.FromMilliseconds(speed))
                { AutoReverse = true });
        }

        if (plan.ParticleCount > 0)
            PlayParticles(plan, completed);
        return plan.DurationMilliseconds;
    }

    private void PlayParticles(PetReactionPlan plan, bool completed)
    {
        // At most ten short-lived shapes; never allocate particles on a frame
        // tick, and do not add a fullscreen or click-catching effect layer.
        var generation = ++_effectsGeneration;
        EffectStage.Children.Clear();
        var colors = _profile.Species switch
        {
            PetSpecies.Dog => new[] { "#F3BD59", "#FFAB7E", "#FFF5CF" },
            PetSpecies.Bunny => new[] { "#CEB4F0", "#FFC6DE", "#FFF2C9" },
            _ => new[] { "#FFC778", "#FFE3A2", "#D1B4F5" }
        };
        for (var i = 0; i < Math.Min(plan.ParticleCount, PetAnimationRules.MaxParticles); i++)
        {
            var angle = 2 * Math.PI * i / plan.ParticleCount - Math.PI / 2;
            var distance = completed ? 58 : 37;
            var particle = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse("M 6,0 L 7.7,4.1 L 12,5.2 L 8.2,8 L 9.3,12 L 6,9.5 L 2.7,12 L 3.8,8 L 0,5.2 L 4.3,4.1 Z"),
                Fill = new SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(
                        colors[i % colors.Length])),
                Width = 12,
                Height = 12,
                Opacity = 0,
                RenderTransform = new TranslateTransform(),
                IsHitTestVisible = false
            };
            System.Windows.Controls.Canvas.SetLeft(particle, 75);
            System.Windows.Controls.Canvas.SetTop(particle, completed ? 88 : 135);
            EffectStage.Children.Add(particle);

            var travel = (TranslateTransform)particle.RenderTransform;
            var time = TimeSpan.FromMilliseconds(Math.Max(280, plan.DurationMilliseconds - 75));
            travel.BeginAnimation(TranslateTransform.XProperty,
                new DoubleAnimation(0, Math.Cos(angle) * distance, time));
            travel.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(0, Math.Sin(angle) * distance - 17, time));
            var fade = new DoubleAnimation(1, 0, time);
            if (i == plan.ParticleCount - 1)
                fade.Completed += (_, _) =>
                {
                    if (_effectsGeneration == generation)
                        EffectStage.Children.Clear();
                };
            particle.BeginAnimation(OpacityProperty, fade);
        }
    }

    public void Stop()
    {
        CatIdleOverlay.StopAnimation();
        _effectsGeneration++;
        ResetCharacterMotion();
        MouthSpeak.BeginAnimation(OpacityProperty, null);
        MouthSpeak.Visibility = Visibility.Collapsed;
        MouthSmile.Visibility = Visibility.Visible;
        foreach (System.Windows.UIElement particle in EffectStage.Children)
        {
            particle.BeginAnimation(OpacityProperty, null);
            if (particle.RenderTransform is TranslateTransform transform)
            {
                transform.BeginAnimation(TranslateTransform.XProperty, null);
                transform.BeginAnimation(TranslateTransform.YProperty, null);
            }
        }
        EffectStage.Children.Clear();
    }
}
