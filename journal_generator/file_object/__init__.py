from struct import unpack

class FileObject:
    def __init__(self, path):
        self.f = open(path, 'rb+')

    def __del__(self):
        self.close()
        
    def close(self):
        self.f.close()
        
    def read(self, bytes):
        return self.f.read(bytes)
        
    def tell(self):
        return self.f.tell()
        
    def seek(self, pos):
        return self.f.seek(pos)
        
    def read_char(self):
        return self.f.read(1)
        
    def read_string_4(self):
        return self.f.read(4)
    
    def read_int8_LE(self):
        """
        rtype: int base 16
        """
        return unpack('<b', self.f.read(1))[0]
    
    def read_uint8_LE(self):
        """
        rtype: int base 16
        """
        return unpack('<B', self.f.read(1))[0]
       
    def read_int16_LE(self):
        """
        rtype: int base 16
        """
        return unpack('<h', self.f.read(2))[0]
    
    def read_uint16_LE(self):
        """
        rtype: int base 16
        """
        return unpack('<H', self.f.read(2))[0]
        
    def read_int32_LE(self):
        """
        rtype: int base 16
        """
        return unpack('<i', self.f.read(4))[0]
        
    def read_uint32_LE(self):
        """
        rtype: int base 16
        """
        return unpack('<I', self.f.read(4))[0]
        
    def read_int32_BE(self):
        """
        rtype: int base 16
        """
        return unpack('>i', self.f.read(4))[0]
        
    def read_float_LE(self):
        """
        rtype: float
        """
        return unpack('<f', self.f.read(4))[0]
        
    def non_inline_id(self):
        return 0
