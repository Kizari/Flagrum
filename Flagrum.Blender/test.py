import construct as C
from pathlib import Path

image_data_offset = 0x100

imageHeader = C.Struct(
    "w" / C.Int16ul,
    "h" / C.Int16ul,
    "offset" / C.Int32ul,
    "unkn30" / C.Int16ul,
    "unkn31" / C.Int16ul,
    "unkn40" / C.Int16ul,
    "unkn41" / C.Int16ul,
    "unkn5" / C.Int8ul[4],
    "imageSize" / C.Int32ul,
    "mw" / C.Int32ul,
    "mh" / C.Int32ul,
)

fileHeader = C.Struct(
    "null0" / C.Int16ul,
    "w" / C.Int16ul,
    "h" / C.Int16ul,
    "null1" / C.Int8ul,
    "imageCount" / C.Int8ul,
    "_padding0" / C.Int8ul[24],
    "headers" / imageHeader[C.this.imageCount],
)

with open(r"C:\Modding\output.csv","w") as outf:
    header = ",".join(["path","phys_h","phys_w","px_h","px_w","bytesize","30","31","41","42","50","51","52","53"])
    outf.write(header+"\n")
    for heb in Path(r"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\environment\world\heightmaps").rglob("*.heb"):
        f = fileHeader.parse_file(str(heb))
        if f.w != 8192 or f.h != 8192:
            print(heb)
        for h in f.headers:
            outf.write(','.join(map(str,[heb.stem,h.w,h.h,h.mw,h.mh,h.imageSize,h.unkn30,h.unkn31,
                                         h.unkn40,h.unkn41,','.join(map(str,list(h.unkn5)))]))+"\n")

#directory = Path(r"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY XV\datas\environment\world\heightmaps")
#for heb in directory.rglob("*.heb"):
#    f = fileHeader.parse_file(str(heb))
#    if f.w != 8192 or f.h != 8192:
#        print(heb)
