# Windows-only overlay exploration, NOT a reminder scheduler or installed app.
# Does not access the network, require administrator privileges, or write settings.
Add-Type -AssemblyName PresentationFramework, PresentationCore, WindowsBase
$markup = @'
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        Title="PetPing Overlay Preview" Width="342" Height="208" WindowStyle="None"
        ResizeMode="NoResize" AllowsTransparency="True" Background="Transparent"
        ShowInTaskbar="False" ShowActivated="False" Topmost="True">
  <Border CornerRadius="20" Margin="8" Padding="14" Background="#FFFFFF" BorderThickness="1" BorderBrush="#EABDA8">
    <Border.Effect><DropShadowEffect Color="#80543E" ShadowDepth="5" BlurRadius="15" Opacity="0.25"/></Border.Effect>
    <Grid>
      <Grid.RowDefinitions><RowDefinition Height="*"/><RowDefinition Height="Auto"/></Grid.RowDefinitions>
      <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
        <Canvas Width="86" Height="86" Margin="0,0,12,0">
          <Polygon Points="10,30 17,2 38,20" Fill="#EFAC7C"/>
          <Polygon Points="51,20 71,2 80,30" Fill="#EFAC7C"/>
          <Ellipse Canvas.Left="6" Canvas.Top="17" Width="76" Height="68" Fill="#F4B17D"/>
          <Ellipse Canvas.Left="22" Canvas.Top="43" Width="7" Height="9" Fill="#312D2D"/>
          <Ellipse Canvas.Left="60" Canvas.Top="43" Width="7" Height="9" Fill="#312D2D"/>
          <Ellipse Canvas.Left="39" Canvas.Top="58" Width="12" Height="8" Fill="#D47C72"/>
          <Path Data="M 45,65 Q 37,75 29,67 M 45,65 Q 53,75 61,67" Stroke="#8B5D48" StrokeThickness="2" Fill="Transparent"/>
        </Canvas>
        <StackPanel VerticalAlignment="Center" Width="175">
          <TextBlock Text="Hey, it's reminder time!" FontWeight="Bold" FontSize="14" Foreground="#2D3135" TextWrapping="Wrap"/>
          <TextBlock Text="Work on your next film scene" FontSize="12" Margin="0,7,0,0" Foreground="#61666A" TextWrapping="Wrap"/>
        </StackPanel>
      </StackPanel>
      <StackPanel Grid.Row="1" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,10,0,0">
        <Button Name="SnoozeButton" Content="Snooze 10s" Padding="12,7" Margin="0,0,8,0" Background="#EEEEEB" BorderThickness="0"/>
        <Button Name="DoneButton" Content="Done ✓" Padding="12,7" Background="#F2B18C" BorderThickness="0"/>
      </StackPanel>
    </Grid>
  </Border>
</Window>
'@
try { $window = [System.Windows.Markup.XamlReader]::Parse($markup) }
catch { Write-Error "Could not load WPF overlay preview: $_"; exit 1 }
$work = [System.Windows.SystemParameters]::WorkArea
$window.Left = $work.Right - $window.Width - 16
$window.Top = $work.Bottom - $window.Height - 16
$speaker = $null
try {
  Add-Type -AssemblyName System.Speech
  $speaker = New-Object System.Speech.Synthesis.SpeechSynthesizer
} catch { Write-Host 'Voice unavailable; visual preview will still work.' }
$announce = {
  if ($null -ne $speaker) {
    try { $speaker.SpeakAsyncCancelAll(); $null = $speaker.SpeakAsync('Hey! It is reminder time. Work on your next film scene.') }
    catch { Write-Host "Voice preview unavailable: $_" }
  }
}
$delay = New-Object System.Windows.Threading.DispatcherTimer
$delay.Interval = [TimeSpan]::FromSeconds(10)
$delay.Add_Tick({
  $delay.Stop()
  $window.Show()
  & $announce
})
$window.FindName('DoneButton').Add_Click({ $window.Close() })
$window.FindName('SnoozeButton').Add_Click({ $window.Hide(); $delay.Start() })
$window.Add_Closed({ $delay.Stop(); if ($null -ne $speaker) { $speaker.Dispose() } })
Write-Host 'Opening a pet reminder overlay preview in 3 seconds. This is NOT an installed scheduler.'
Start-Sleep -Seconds 3
$window.ShowDialog() | Out-Null
