#! coding: utf8
from collections import OrderedDict

structure = OrderedDict([
    ('id'                      , 'non_inline_id'),
    ('journal_title'           , 'read_string_4'),
    ('journal_body_text'       , 'read_string_4'),
    ('creature_id'             , 'read_uint32_LE'),
    ('spell_id'                , 'read_int32_LE'),
    ('Unknown4'                , 'read_int32_LE'),
    ('boss_id'                 , 'read_uint16_LE'),
    ('next_sibling_section_id' , 'read_uint16_LE'),
    ('first_child_section_id'  , 'read_uint16_LE'),
    ('parent_section_id'       , 'read_uint16_LE'),
    ('Unknown9'                , 'read_uint16_LE'),
    ('section_type'            , 'read_uint16_LE'),
    ('Unknown11'               , 'read_uint8_LE'),
    ('Unknown12'               , 'read_uint8_LE'),
    ('Unknown13'               , 'read_int16_LE'),
])      

"""
section type:
1 坦克预警
2 DPS预警
4 治疗预警
16 灭团技
32 重要
64 可打断技能
128 法术效果
2048 激怒
4096 史诗难度
"""