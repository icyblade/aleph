from collections import OrderedDict

structure = OrderedDict([
    ('instance_id'           , 'read_int32_LE'),
    ('Unknown1'              , 'read_int32_LE'),
    ('Unknown2'              , 'read_int32_LE'),
    ('Unknown3'              , 'read_int32_LE'),
    ('Unknown4'              , 'read_int32_LE'),
    ('instance_name'         , 'read_string_4'),
    ('instance_description'  , 'read_string_4'),
    ('Unknown7'              , 'read_uint16_LE'),
    ('Unknown8'              , 'read_uint16_LE'),
    ('Unknown9'              , 'read_uint16_LE'),
    ('Unknown10'             , 'read_uint16_LE'),
])      

