#! coding: utf8
import re

class Spell:
    def __init__(self, spell_id, cur, logger, difficulty_id, manual_parse=False):
        self.spell_id = spell_id
        self.cur = cur
        self.logger = logger
        self.difficulty_id = difficulty_id
        spell_info = self.get_spell(manual_parse)
        if spell_info:
            self.spell_name, self.spell_description, self.icon_path = spell_info[0]['spell_name'], spell_info[0]['spell_description'], spell_info[0]['icon_path']
        else:
            self.spell_name, self.spell_description, self.icon_path = None, None, None
        
    def __nonzero__(self):
        return bool(self.spell_name or self.spell_description or self.icon_path)
            
    def get_spell(self, manual_parse):
        """
        rvalue: spell_name, spell_description, icon_path
        """
        sql = 'select a.m_name_lang as spell_name, \
            a.m_description_lang as spell_description,c.m_Name as icon_path \
            from dbc_Spell a \
            left join dbc_SpellMisc b on a.m_spellMisc=b.m_ID \
            left join dbc_SpellIcon c on b.m_spellIconID=c.m_ID \
            where a.m_ID={self.spell_id}'.format(self=self)
        self.logger.debug(sql)
        self.cur.execute(sql)
        ret = self.cur.fetchall()
        if not manual_parse:
            for idx, row in enumerate(ret):
                ret[idx]['spell_description'] = self.parse_spell_description(row['spell_description'])
        return ret

    def parse_spell_description(self, desc):
        """
        LEG 团队本里只有
        {
            '$A1' ,
            '$A2',
            '$D',
            '$E1',
            '$T1',
            '$a1',
            '$d',
            '$s1',
            '$s2',
            '$s22',
            '$s3',
            '$t',
            '$t1',
            '$t2'
        }
        """
        # 处理 $@spellname $@spelldesc
        spellnames = re.findall('\$@spellname([0-9]+)', desc)
        if spellnames:
            for id in spellnames:
                desc = re.sub('\$@spellname%s' % id, '[wow,spell,%s,cn[%s]]' % (id, Spell(id, self.cur, self.logger, self.difficulty_id, manual_parse=True).spell_name), desc)
        spelldescs = re.findall('\$@spelldesc([0-9]+)', desc)
        if spelldescs:
            for id in spelldescs:
                desc = re.sub('\$@spelldesc%s' % id, Spell(id, self.cur, self.logger, self.difficulty_id, manual_parse=True).spell_description , desc)
                
        # 处理 $[16 blahblah $]
        for s in re.findall('\$\[([0-9]+)([\s\S]+?)\$\]', desc):
            if self.difficulty_id == int(s[0]):
                desc = desc.replace('$[%s%s$]' % s, s[1])
            else:
                desc = desc.replace('$[%s%s$]' % s, '')

        # 处理 s
        for s in re.findall('\$[0-9]*s[0-9]*', desc):
            desc = desc.replace(s, self.parse_spell_description_s(s))
            
        # 处理 d
        for s in re.findall('\$[0-9]*[Dd]{1}[0-9]*', desc):
            desc = desc.replace(s, self.parse_spell_description_d(s))
            
        # 处理 a
        for s in re.findall('\$[0-9]*[Aa]{1}[0-9]*', desc):
            desc = desc.replace(s, self.parse_spell_description_a(s))
        
        # 处理 t
        for s in re.findall('\$[0-9]*[Tt]{1}[0-9]*', desc):
            desc = desc.replace(s, self.parse_spell_description_t(s))
            
        return desc
        
    def parse_spell_description_s(self, s):
        """处理类似 $12345s1 的文本"""
        spell_id, effect_no = re.findall('\$([0-9]*)s([0-9]*)', s)[0]
        sql = 'select effect.m_effectBasePoints as base_points, effect.m_effectBonusPoints as bonus_points from dbc_Spell spell left join dbc_SpellEffect effect on spell.m_ID=effect.m_spellID left join dbc_Difficulty difficulty on effect.m_difficultyID=difficulty.m_ID where spell.m_ID={spell_id} and effect.m_effectIndex={effect_no} and (effect.m_difficultyID={self.difficulty_id} or effect.m_difficultyID=0) order by difficulty.m_ID desc limit 1'.format(
            self = self,
            spell_id = spell_id if spell_id else self.spell_id,
            effect_no = int(effect_no)-1 if effect_no else 0,
        )
        self.cur.execute(sql)
        result = self.cur.fetchone()
        if result:
            base_points, bonus_points = abs(int(result['base_points'])), abs(int(result['bonus_points']))
            if bonus_points == 0:
                return '%s' % base_points
            else:
                return '%s-%s' % (base_points+1, base_points+bonus_points)
        else:
            return s
            
    def parse_spell_description_a(self, s):
        """处理类似 $12345a1 的文本"""
        spell_id, typ, effect_no = re.findall('\$([0-9]*)([Aa]{1})([0-9]*)', s)[0]
        if typ == 'a':
            sql = 'select radius.m_radius from dbc_Spell spell left join dbc_SpellEffect effect on spell.m_ID=effect.m_spellID left join dbc_SpellRadius radius on effect.m_radiusIndex1=radius.m_ID where spell.m_ID={spell_id} and effect.m_effectIndex={effect_no} and (effect.m_difficultyID={self.difficulty_id} or effect.m_difficultyID=0) order by effect.m_difficultyID desc limit 1'.format(
                self = self,
                spell_id = spell_id if spell_id else self.spell_id,
                effect_no = int(effect_no)-1 if effect_no else 0,
            )
        elif typ == 'A':
            sql = 'select radius.m_radius from dbc_Spell spell left join dbc_SpellEffect effect on spell.m_ID=effect.m_spellID left join dbc_SpellRadius radius on effect.m_radiusIndex2=radius.m_ID where spell.m_ID={spell_id} and effect.m_effectIndex={effect_no} and (effect.m_difficultyID={self.difficulty_id} or effect.m_difficultyID=0) order by effect.m_difficultyID desc limit 1'.format(
                self = self,
                spell_id = spell_id if spell_id else self.spell_id,
                effect_no = int(effect_no)-1 if effect_no else 0,
            )
        self.cur.execute(sql)
        result = self.cur.fetchone()
        if result and result['m_radius']:
            return str(result['m_radius'])
        else:
            return s
            
    def parse_spell_description_d(self, s):
        """处理类似 $12345d 的文本"""
        spell_id = re.findall('\$([0-9]*)[Dd]{1}', s)[0]
        sql = 'select duration.m_ms_base from dbc_Spell spell left join dbc_SpellMisc misc on spell.m_spellMisc=misc.m_ID left join dbc_SpellDuration duration on misc.m_durationIndex=duration.m_ID where spell.m_ID={spell_id}'.format(
            spell_id = spell_id if spell_id else self.spell_id
        )
        self.cur.execute(sql)
        result = self.cur.fetchone()
        if result and result['m_ms_base']:
            return u'%s秒' % (int(result['m_ms_base'])/1000)
        else:
            return s

    def parse_spell_description_t(self, s):
        """处理类似 $12345t 的文本"""
        spell_id, effect_no = re.findall('\$([0-9]*)[Tt]{1}([0-9]*)', s)[0]
        sql = 'select m_effectAuraPeriod from dbc_SpellEffect where m_spellID={spell_id} and m_effectIndex={effect_no}'.format(
            spell_id = spell_id if spell_id else self.spell_id,
            effect_no = int(effect_no)-1 if effect_no else 0,
        )
        self.cur.execute(sql)
        result = self.cur.fetchone()
        if result and result['m_effectAuraPeriod']:
            return u'%s秒' % (int(result['m_effectAuraPeriod'])/1000)
        else:
            return s
