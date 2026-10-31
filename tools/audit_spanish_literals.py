#!/usr/bin/env python3
"""Read-only Spanish-string inventory for C# source.

This is a heuristic report, NOT proof that each match is user-facing.
Escaped, multi-line, interpolated and verbatim literals require manual review.
Do not auto-replace matches: legacy persistence keys and test data are protected.
"""
import argparse
import json
import re
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
LITERAL = re.compile(r'(?:\$@|@\$|\$|@)?"(?:\\.|[^"\\])*"')
SPANISH = re.compile(
    r'[áéíóúñÁÉÍÓÚÑ¿¡]|\b(?:'
    r'cuenta|cuentas|conexión|conexiones|operación|operaciones|estrategia|estrategias|'
    r'guardar|guardado|descargar|descargado|cargando|datos|borrar|eliminar|cerrar|'
    r'abrir|restablecer|ganancia|pérdida|plantilla|plantillas|tamaño|período|'
    r'gráfico|gráficos|indicador|indicadores|mercado|volumen|precio|'
    r'volvé|elegí|tocá|revisá|hacé|activá|arrancá|todavía|'
    r'ahora|después|primero|último|última|desde|hasta|'
    r'sin datos|no hay|no se pudo|falta|máximo|mínimo|'
    r'simulado|simulador|comisión|sugerir|reporte|historia|'
    r'en vivo|ordenes|órdenes|servidor|prueba|opción|configuración'
    r')\b', re.IGNORECASE
)
SKIP_DIRS = {'.git', 'bin', 'obj', 'dist', 'node_modules'}

def source_type(path: Path) -> str:
    rel = path.relative_to(ROOT).as_posix()
    if rel.endswith('Strings.g.cs'):
        return 'generated_sdk_do_not_edit'
    if rel.startswith('tests/') or rel.startswith('tools/TradingHub.UiSnapshot/'):
        return 'tests'
    if rel.startswith('src/TradingHub.App/'):
        return 'application'
    if rel.startswith('src/TradingHub.Charting/'):
        return 'charting'
    if rel.startswith('src/TradingHub.Sdk/'):
        return 'sdk'
    return 'other'

def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', default='spanish-literals-audit.json')
    args = parser.parse_args()
    findings = []
    scanned = 0
    for path in sorted(ROOT.rglob('*.cs')):
        if any(part in SKIP_DIRS for part in path.relative_to(ROOT).parts):
            continue
        scanned += 1
        for number, line in enumerate(path.read_text(encoding='utf-8-sig').splitlines(), 1):
            if line.lstrip().startswith(('//', '*', '/*')):
                continue
            for match in LITERAL.finditer(line):
                value = match.group(0)
                if not SPANISH.search(value):
                    continue
                findings.append({
                    'file': path.relative_to(ROOT).as_posix(),
                    'line': number,
                    'category': source_type(path),
                    'literal': value[:350],
                })
    counts = dict(sorted(Counter(x['category'] for x in findings).items()))
    file_counts = Counter(x['file'] for x in findings)
    report = {
        'disclaimer': 'Heuristic only; review every candidate before translation. Do not rename persisted identifiers.',
        'files_scanned': scanned,
        'flagged_literals': len(findings),
        'flagged_files': len(file_counts),
        'category_counts': counts,
        'top_files': file_counts.most_common(30),
        'findings': findings,
    }
    Path(args.output).write_text(json.dumps(report, indent=2, ensure_ascii=False) + '\n', encoding='utf-8')
    print(f"Scanned {scanned} C# files; flagged {len(findings)} literals across {len(file_counts)} files.")
    print(f"Categories: {counts}")
    for file, count in file_counts.most_common(15):
        print(f"  {count:4}  {file}")
    print('NOTE: Spanish content in documentation screenshots, raw strings, or comments may not be detected.')

if __name__ == '__main__':
    main()
