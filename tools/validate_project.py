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
print('NOTE: actual compile / Windows wake, tray and overlay tests remain pending.')
