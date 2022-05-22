import textwrap

from bpy.types import UILayout


def draw_lines(layout: UILayout, text: str):
    wrap = textwrap.TextWrapper(width=40)
    lines = wrap.wrap(text=text)
    for line in lines:
        row = layout.row(align=True)
        row.alignment = 'EXPAND'
        row.scale_y = 0.6
        row.label(text=line)
