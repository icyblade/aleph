from collections import OrderedDict

structure = OrderedDict([
    ('difficulty_id'     ,'read_int32_LE'),
    ('difficulty_name'   ,'read_string_4'),
    ('Unknown2'          ,'read_uint8_LE'),
    ('Unknown3'          ,'read_uint8_LE'),
    ('min_players'       ,'read_uint8_LE'),
    ('max_players'       ,'read_uint8_LE'),
    ('Unknown6'          ,'read_int8_LE'),
    ('Unknown7'          ,'read_uint8_LE'),
    ('Unknown8'          ,'read_uint8_LE'),
    ('Unknown9'          ,'read_uint8_LE'),
    ('Unknown10'         ,'read_uint8_LE'),
    ('Unknown11'         ,'read_uint8_LE'),
    ('Unknown12'         ,'read_uint8_LE'),
    ('Unknown13'         ,'read_uint8_LE'),
])
