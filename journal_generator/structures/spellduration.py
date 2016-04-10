from collections import OrderedDict

structure = OrderedDict([
    ('id'                   ,'non_inline_id'),
    ('base_duration'        ,'read_int32_LE'),
    ('max_duration'         ,'read_int32_LE'),
    ('Unknown2'             ,'read_int32_LE'),
])
