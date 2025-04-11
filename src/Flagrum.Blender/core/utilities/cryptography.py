from ctypes import c_uint64, c_int64, c_uint32

hash_seed_32 = 2166136261
hash_prime_32 = 16777619
hash_seed_64 = 1469598103934665603
hash_prime_64 = 1099511628211


class Cryptography:
    @staticmethod
    def hash32(value: str) -> int:
        buffer = value.encode('utf-8')

        result = hash_seed_32
        for byte in buffer:
            result = c_uint32((result ^ byte) * hash_prime_32).value

        return result

    @staticmethod
    def hash64(value: str) -> int:
        buffer = value.encode('utf-8')

        result = hash_seed_64
        for byte in buffer:
            result = c_uint64((result ^ byte) * hash_prime_64).value

        return result

    @staticmethod
    def uri_hash64(uri: str) -> int:
        tokens = uri.replace("data://", "").split("/")[-1].split(".")
        if len(tokens) > 2:
            extension = ""
            for i in range(1, len(tokens)):
                extension += tokens[i]
                if i < len(tokens) - 1:
                    extension += "."
        else:
            extension = tokens[-1]

        uri_hash = Cryptography.hash64(uri)
        type_hash = Cryptography.hash64(extension)
        return c_uint64(
            (c_int64(uri_hash).value & 17592186044415) | ((c_int64(type_hash).value << 44) & -17592186044416)
        ).value
