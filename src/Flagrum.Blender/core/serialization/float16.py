import binascii
import struct


class Float16:
    @staticmethod
    def compress(float32: float) -> int:
        f16_exponent_bits = 0x1F
        f16_exponent_shift = 10
        f16_exponent_bias = 15
        f16_mantissa_bits = 0x3ff
        f16_mantissa_shift = (23 - f16_exponent_shift)
        f16_max_exponent = (f16_exponent_bits << f16_exponent_shift)

        a = struct.pack('>f', float32)
        b = binascii.hexlify(a)

        f32 = int(b, 16)
        f16 = 0
        sign = (f32 >> 16) & 0x8000
        exponent = ((f32 >> 23) & 0xff) - 127
        mantissa = f32 & 0x007fffff

        if exponent == 128:
            f16 = sign | f16_max_exponent
            if mantissa:
                f16 |= (mantissa & f16_mantissa_bits)
        elif exponent > 15:
            f16 = sign | f16_max_exponent
        elif exponent > -15:
            exponent += f16_exponent_bias
            mantissa >>= f16_mantissa_shift
            f16 = sign | exponent << f16_exponent_shift | mantissa
        else:
            f16 = sign

        return f16

    @staticmethod
    def decompress(float16):
        s = int((float16 >> 15) & 0x00000001)  # sign
        e = int((float16 >> 10) & 0x0000001f)  # exponent
        f = int(float16 & 0x000003ff)  # fraction

        if e == 0:
            if f == 0:
                return int(s << 31)
            else:
                while not (f & 0x00000400):
                    f = f << 1
                    e -= 1
                e += 1
                f &= ~0x00000400
        elif e == 31:
            if f == 0:
                return int((s << 31) | 0x7f800000)
            else:
                return int((s << 31) | 0x7f800000 | (f << 13))

        e = e + (127 - 15)
        f = f << 13
        return int((s << 31) | (e << 23) | f)
