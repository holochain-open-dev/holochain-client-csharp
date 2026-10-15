"""Merges the Cobertura reports in .holochain-test/coverage-* and lists uncovered lines per method of the
HoloNET client assembly (most first).
Usage: python scripts/uncovered.py [class-substring] [limit]"""
import glob, sys, collections, xml.etree.ElementTree as ET

import os
PACKAGE = os.environ.get('HOLONET_COV_PACKAGE', 'NextGenSoftware.Holochain.HoloNET.Client')  # e.g. ...HoloNET.ORM
flt = sys.argv[1] if len(sys.argv) > 1 else ''
limit = int(sys.argv[2]) if len(sys.argv) > 2 else 60
hits = collections.defaultdict(int)          # (file, line) -> hits across all reports
owner = {}                                   # (file, line) -> class::method
for f in glob.glob('.holochain-test/coverage-*/*/coverage.cobertura.xml'):
    root = ET.parse(f).getroot()
    for pkg in root.iter('package'):
        if pkg.get('name') != PACKAGE:
            continue
        for cls in pkg.iter('class'):
            name, fn = cls.get('name'), cls.get('filename')
            if flt not in name:
                continue
            for m in cls.iter('method'):
                for ln in m.iter('line'):
                    key = (fn, int(ln.get('number')))
                    hits[key] += int(ln.get('hits'))
                    owner[key] = name.split('.')[-1] + '::' + m.get('name')
per = collections.defaultdict(list)
for key, h in hits.items():
    if h == 0:
        per[owner[key]].append(key[1])
rows = sorted(per.items(), key=lambda kv: -len(kv[1]))
for meth, lines in rows[:limit]:
    lines.sort()
    print(f"{len(lines):4} {meth}  [{lines[0]}-{lines[-1]}]")
print(sum(len(v) for v in per.values()), 'uncovered lines in', len(per), 'methods')
