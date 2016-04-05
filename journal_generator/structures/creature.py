from collections import OrderedDict

structure = OrderedDict([
    ('Unknown0'            ,'read_int32_LE'),
    ('Unknown1_0'          ,'read_int32_LE'),
    ('Unknown1_1'          ,'read_int32_LE'),
    ('Unknown1_2'          ,'read_int32_LE'),
    ('Unknown2'            ,'read_int32_LE'),
    ('journal_creature_id' ,'read_int32_LE'),
    ('Unknown3_1'          ,'read_int32_LE'),
    ('Unknown3_2'          ,'read_int32_LE'),
    ('Unknown3_3'          ,'read_int32_LE'),
    ('Unknown4_0'          ,'read_float_LE'),
    ('Unknown4_1'          ,'read_float_LE'),
    ('Unknown4_2'          ,'read_float_LE'),
    ('Unknown4_3'          ,'read_float_LE'),
    ('creature_name'       ,'read_string_4'),
    ('Unknown6'            ,'read_string_4'),
    ('Unknown7'            ,'read_string_4'),
    ('Unknown8'            ,'read_string_4'),
    ('Unknown9'            ,'read_uint8_LE'),
    ('Unknown10'           ,'read_uint8_LE'),
    ('Unknown11'           ,'read_uint8_LE'),
    ('Unknown12'           ,'read_uint8_LE'),
])      

