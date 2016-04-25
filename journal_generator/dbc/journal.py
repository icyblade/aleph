#! coding: utf8
import re
from dbc.spell import Spell

class Journal:
    def __init__(self, cur, logger, difficulty_id):
        self.cur = cur
        self.logger = logger
        self.difficulty_id = difficulty_id

    def Spell(self, spell_id, difficulty_id=None):
        if difficulty_id is None:
            difficulty_id = self.difficulty_id
        return Spell(spell_id, self.cur, self.logger, difficulty_id=difficulty_id)
        
    def get_expansions(self):
        """
        rvalue: expansion_id, expansion_name
        """
        self.cur.execute(
            'select m_ID as expansion_id, field0 as expansion_name from dbc_JournalTier'
        )
        return self.cur.fetchall()
        
    def get_instances(self, expansion_id=None):
        """
        rvalue: instance_id, instance_name, instance_description
        """
        if expansion_id:
            sql = 'select a.m_ID as instance_id, a.field4 as instance_name, \
                a.field5 as instance_description from dbc_JournalInstance a \
                left join dbc_JournalTierXInstance b \
                on a.m_ID=b.field1 \
                where b.field0=%s' % expansion_id
        else:
            sql = 'select a.m_ID as instance_id, a.field4 as instance_name, \
                a.field5 as instance_description from dbc_JournalInstance'
        self.logger.debug(sql)
        self.cur.execute(sql)
        return self.cur.fetchall()
        
    def get_bosses(self, instance_id=None):
        """
        ravalue: boss_id, boss_name, boss_description
        """
        if instance_id:
            sql = 'select m_ID as boss_id, field2 as boss_name, field3 boss_description \
                from dbc_JournalEncounter where field7=%s order by field10' % instance_id
        else:
            sql = 'select m_ID as boss_id, field2 as boss_name, field3 boss_description \
                from dbc_JournalEncounter order by field10'
        self.cur.execute(sql)
        return self.cur.fetchall()
        
    def get_encounter_section(self, boss_id=None):
        """
        rvalue: id, journal_title, journal_body_text, \
                next_sibling_section_id, first_child_section_id, \
                parent_section_id, spell_id, section_type, creature_name
        """
        if boss_id:
            sql = 'SELECT a.m_ID as id, a.field0 as journal_title, a.field1 as journal_body_text, a.field6 as next_sibling_section_id, a.field7 as first_child_section_id, a.field8 as parent_section_id, a.field3 as spell_id, a.field10 as section_type, "" AS creature_name FROM dbc_JournalEncounterSection a WHERE field5 = {boss_id} AND a.field2 = 0 UNION ALL SELECT a.m_ID as id, a.field0 as journal_title, a.field1 as journal_body_text, a.field6 as next_sibling_section_id, a.field7 as first_child_section_id, a.field8 as parent_section_id, a.field3 as spell_id, a.field10 as section_type, b.m_name FROM dbc_JournalEncounterSection a LEFT JOIN dbc_Creature b ON a.field2 = b.m_displayID_1 WHERE field5 = {boss_id} AND a.field2 <> 0'.format(boss_id = boss_id)
        else:
            sql = 'SELECT a.m_ID as id, a.field0 as journal_title, a.field1 as journal_body_text, a.field6 as next_sibling_section_id, a.field7 as first_child_section_id, a.field8 as parent_section_id, a.field3 as spell_id, a.field10 as section_type, "" AS creature_name FROM dbc_JournalEncounterSection a WHERE D a.field2 = 0 UNION ALL SELECT a.m_ID as id, a.field0 as journal_title, a.field1 as journal_body_text, a.field6 as next_sibling_section_id, a.field7 as first_child_section_id, a.field8 as parent_section_id, a.field3 as spell_id, a.field10 as section_type, b.m_name FROM dbc_JournalEncounterSection a LEFT JOIN dbc_Creature b ON a.field2 = b.m_displayID_1 WHERE a.field2 <> 0'
        self.logger.debug(sql)
        self.cur.execute(sql)
        return self.process_sections(self.cur.fetchall())
        
    def get_encounter_root_section(self, sections):
        """
        param sections: return value of get_encounter_section()
        """
        if not sections:
            return []
        tmp = [i['next_sibling_section_id'] for i in sections]
        ret = [i['id'] for i in sections if (not i['id'] in tmp) and i['parent_section_id']==0]
        if len(ret) != 1:
            print(sections)
            print(ret)
            raise Exception('Invalid section')
        return ret[0]
      
    def process_section(self, section):
        """整理格式，包括技能图标等
        param section: rvalue of get_encounter_section
            id, journal_title, journal_body_text, \
            next_sibling_section_id, first_child_section_id, \
            parent_section_id, spell_id, section_type, creature_name
        rvalue       : id, journal_title, journal_body_text, \
            next_sibling_section_id, first_child_section_id, \
            parent_section_id, spell_id, section_type, creature_name, \
            icon_path
        """
        ret = section.copy()
        
        # process spell info
        icon = None
        if ret['spell_id'] != '0':
            spell = self.Spell(ret['spell_id'])
            if spell:
                icon = spell.icon_path.split('\\')[-1].lower() if not spell.icon_path is None else None
                if re.match('Section [0-9]+', ret['journal_title']) or re.match(u'第[0-9]+区', ret['journal_title']):
                    ret['journal_title'] = spell.spell_name
                if not ret['journal_title']: ret['journal_title'] = spell.spell_name
                if not ret['journal_body_text']: ret['journal_body_text'] = spell.spell_description
        ret['icon_path'] = icon

        # replace bullet
        if ret['journal_body_text'].find('$bullet;') != -1:
            ret['journal_body_text'] = ret['journal_body_text'].replace('$bullet;', u'•')
            
        # spell
        ret['journal_body_text'] = re.sub(
            '\|c[0-9A-F]{8}\|Hspell:(?P<spell_id>[0-9]+)\|h\[(?P<spell_name>[\S ]+?)\]\|h\|r',
            '[wow,spell,\g<spell_id>,cn[\g<spell_name>]]',
            ret['journal_body_text']
        )

        # dollar handler
        ret['journal_body_text'] = self.Spell(0).parse_spell_description(ret['journal_body_text'])
                
        # section type (可打断、重要、法术效果等)
        section_types = {
            0  : u'坦克预警',
            1  : u'DPS预警',
            2  : u'治疗预警',
            4  : u'灭团技',
            5  : u'重要',
            6  : u'可打断技能',
            7  : u'法术效果',
            11 : u'激怒',
            12 : u'史诗难度',
        }
        section_type = []
        for k, v in section_types.iteritems():
            try:
                if '{0:b}'.format(int(ret['section_type']))[-k-1] == '1':
                    section_type.append(v)
            except IndexError:
                pass
        ret['section_type'] = ','.join(section_type)
        
        return ret
            
    def process_sections(self, sections):
        """
        param sections: rvalue of get_encounter_section
            id, journal_title, journal_body_text, \
            next_sibling_section_id, first_child_section_id, \
            parent_section_id, spell_id, section_type, creature_name
        """
        root = self.get_encounter_root_section(sections)
        sections_dict = dict(
            (i['id'], i)
            for i in sections
        )
        current_sections = list(sections)
        for i in sections:
            if i['id'] == root:
                pivot = i
        current_layer = 0
        while current_sections:
            # process return value
            yield (self.process_section(pivot), current_layer)
            
            # find next
            if pivot['first_child_section_id'] != 0: # has child
                new_pivot = sections_dict[pivot['first_child_section_id']]
                current_layer += 1
            elif pivot['next_sibling_section_id'] == 0: # end of current layer
                if current_layer > 0:
                    current_layer -= 1
                    if sections_dict[pivot['parent_section_id']]['next_sibling_section_id'] == 0:
                        tmp = pivot.copy()
                        while tmp['next_sibling_section_id'] == 0 and current_layer >= 0:
                            current_layer -= 1
                            tmp = sections_dict[tmp['parent_section_id']]
                        if tmp['next_sibling_section_id'] == 0: # end of everything
                            current_sections = [pivot]
                            break
                        tmp = sections_dict[tmp['next_sibling_section_id']]
                        current_layer += 1
                        new_pivot = tmp.copy()
                        if current_layer == -1: # end of everything
                            current_sections = [pivot]
                    else:
                        new_pivot = sections_dict[sections_dict[pivot['parent_section_id']]['next_sibling_section_id']]
                else: # end of everything
                    pass
            else: # go on
                new_pivot = sections_dict[pivot['next_sibling_section_id']]
            current_sections.remove(pivot)
            pivot = new_pivot
