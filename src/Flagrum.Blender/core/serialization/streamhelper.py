from typing import IO


class StreamHelper:
    @staticmethod
    def write_align(stream: IO, block_size: int):
        offset = stream.tell()
        padding = block_size + block_size * int(offset / block_size) - offset
        if 0 < padding < block_size:
            for i in range(padding):
                stream.write(b'\0x00')
