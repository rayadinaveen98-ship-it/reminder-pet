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

    public PetCharacter()
    {
        InitializeComponent();
    }

    public void Apply(PetProfile profile)
    {
        PetProfileCatalog.Validate(profile);
        _profile = profile;
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
        new((Color)ColorConverter.ConvertFromString(hex));

    public void FaceToward(bool movingRight) => PetFacing.ScaleX = movingRight ? 1 : -1;

    public void Walk(bool active)
    {
        LeftFootShift.BeginAnimation(TranslateTransform.YProperty, null);
        RightFootShift.BeginAnimation(TranslateTransform.YProperty, null);
        TailWag.BeginAnimation(RotateTransform.AngleProperty, null);
        DogTailWag.BeginAnimation(RotateTransform.AngleProperty, null);
        LeftFootShift.Y = 0;
        RightFootShift.Y = 0;
        if (!active) return;
        var step = _profile.Personality == PetPersonality.Calm ? 300 : 190;
        LeftFootShift.BeginAnimation(TranslateTransform.YProperty,
            new DoubleAnimation(0, -10, TimeSpan.FromMilliseconds(step))
            { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever });
        RightFootShift.BeginAnimation(TranslateTransform.YProperty,
            new DoubleAnimation(-10, 0, TimeSpan.FromMilliseconds(step))
            { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever });
        var tail = new DoubleAnimation(-8, 12, TimeSpan.FromMilliseconds(step * 2))
        { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
        if (_profile.Species == PetSpecies.Dog)
            DogTailWag.BeginAnimation(RotateTransform.AngleProperty, tail);
        else if (_profile.Species == PetSpecies.Cat)
            TailWag.BeginAnimation(RotateTransform.AngleProperty, tail);
    }

    public void Float(bool active)
    {
        PetFloat.BeginAnimation(TranslateTransform.YProperty, null);
        if (active)
            PetFloat.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(0, -5, TimeSpan.FromSeconds(
                    _profile.Personality == PetPersonality.Calm ? 2 : 1.2))
                { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever });
    }

    public void Blink()
    {
        var animation = new DoubleAnimation(1, .1, TimeSpan.FromMilliseconds(100))
        { AutoReverse = true };
        LeftBlink.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        RightBlink.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
    }

    public void SetSpeaking(bool speaking, bool animate)
    {
        MouthSmile.Visibility = speaking ? Visibility.Collapsed : Visibility.Visible;
        MouthSpeak.Visibility = speaking ? Visibility.Visible : Visibility.Collapsed;
        MouthSpeak.BeginAnimation(OpacityProperty, null);
        if (speaking && animate)
            MouthSpeak.BeginAnimation(OpacityProperty,
                new DoubleAnimation(1, .5, TimeSpan.FromMilliseconds(190))
                { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever });
    }

    public void React(bool completed, bool animate)
    {
        Walk(false);
        Float(false);
        SetSpeaking(false, false);
        if (!animate) return;

        if (completed && _profile.DoneAction == PetDoneAction.Hop)
            PetFloat.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(0, _profile.Species == PetSpecies.Bunny ? -36 : -24,
                    TimeSpan.FromMilliseconds(270)) { AutoReverse = true });
        else if (completed)
            PetFacing.BeginAnimation(ScaleTransform.ScaleXProperty,
                new DoubleAnimation(PetFacing.ScaleX, PetFacing.ScaleX * .8,
                    TimeSpan.FromMilliseconds(250)) { AutoReverse = true });
        else if (_profile.SnoozeAction == PetSnoozeAction.Yawn)
            MouthSpeak.Visibility = Visibility.Visible;
        else
            PetFloat.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(0, 8, TimeSpan.FromMilliseconds(270))
                { AutoReverse = true });
    }

    public void Stop()
    {
        Walk(false);
        Float(false);
        MouthSpeak.BeginAnimation(OpacityProperty, null);
    }
}
