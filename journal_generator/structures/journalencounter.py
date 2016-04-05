from collections import OrderedDict

structure = OrderedDict([
    ('boss_id'          ,'read_int32_LE'),
    ('Unknown1_0'       ,'read_float_LE'),
    ('Unknown1_1'       ,'read_float_LE'),
    ('boss_name'        ,'read_string_4'),
    ('boss_description' ,'read_string_4'),
    ('Unknown4'         ,'read_uint16_LE'),
    ('Unknown5'         ,'read_uint16_LE'),
    ('Unknown6'         ,'read_uint16_LE'),
    ('instance_id'      ,'read_uint16_LE'),
    ('Unknown8'         ,'read_uint8_LE'),
    ('Unknown9'         ,'read_int16_LE'),
    ('Unknown10'        ,'read_uint8_LE'),
])      
