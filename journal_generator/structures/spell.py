from collections import OrderedDict

structure = OrderedDict([
    ('spell_id'                   ,'read_int32_LE'),
    ('spell_name'                 ,'read_string_4'),
    ('spell_name_sub'             ,'read_string_4'),
    ('spell_description'          ,'read_string_4'),
    ('m_auraDescription_lang'     ,'read_string_4'),
    ('spell_misc_id'              ,'read_int32_LE'),
    ('Unknown6'                   ,'read_int32_LE'),
])
