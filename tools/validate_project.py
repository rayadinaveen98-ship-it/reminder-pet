"""Static source checks; NOT equivalent to compiling WPF on Windows."""
from pathlib import Path
import re
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[1]
ns = {'x': 'http://schemas.microsoft.com/winfx/2006/xaml'}
cs = {f.stem: f.read_text(encoding='utf-8') for f in root.rglob('*.cs')}
xmls = sorted([*root.rglob('*.xaml'), *root.rglob('*.csproj')])
for path in xmls:
    tree = ET.parse(path)
    print(f'XML parsed: {path.relative_to(root)}')
    if path.suffix != '.xaml':
        continue
    # Standalone WPF ResourceDictionary XAML deliberately has no code-behind.
    xaml_root = tree.getroot()
    xclass = xaml_root.attrib.get('{http://schemas.microsoft.com/winfx/2006/xaml}Class')
    if xclass is None:
        if xaml_root.tag != '{http://schemas.microsoft.com/winfx/2006/xaml/presentation}ResourceDictionary':
            raise SystemExit(f'Unbacked XAML is not a ResourceDictionary: {path}')
        continue
    base = path.name[:-len('.xaml')]
    backing = path.with_name(base + '.xaml.cs')
    if not backing.exists():
        raise SystemExit(f'Missing backing file: {backing}')
    backing_code = backing.read_text(encoding='utf-8')
    for el in tree.iter():
        for event in ('Click',):
            method = el.attrib.get(event)
            if method and not re.search(rf'\b{re.escape(method)}\s*\(', backing_code):
                raise SystemExit(f'XAML handler missing: {method} in {backing}')
print(f'Checked {len(xmls)} XML files and basic event handlers.')
# Release gate: the actual popup and character may no longer walk, slide,
# animate across the screen or call a gait engine. The old research helpers
# remain in unmerged history, not on the live reminder presentation path.
popup = (root / 'desktop/PetPing.Windows/Views/PetWindow.xaml.cs').read_text(encoding='utf-8')
character = (root / 'desktop/PetPing.Windows/PetCharacter.xaml.cs').read_text(encoding='utf-8')
dashboard = (root / 'desktop/PetPing.Windows/MainWindow.xaml').read_text(encoding='utf-8')
for forbidden in ['Character.Walk(', 'BeginAnimation(LeftProperty, walk)',
                  'Character.Float(_motion == PetMotion.Full)']:
    if forbidden in popup:
        raise SystemExit(f'No-walking release regression: {forbidden}')
if 'Left = path.End;' not in popup or 'Arrive();' not in popup:
    raise SystemExit('No-walking release has no stationary arrival placement')
if 'public void Walk(bool active)' in character:
    raise SystemExit('No-walking release accidentally retains an animated pet walk API')
if 'PetPing v0.18 • Original pets • No walking' not in dashboard:
    raise SystemExit('No-walking release identity is missing')
print('Release gate: stationary existing pets only, no walking or floating entrance.')
print('NOTE: actual compile / Windows wake, tray and overlay tests remain pending.')
