#! coding: utf8
import re

class Spell:
    def __init__(self, spell_id, cur, logger, difficulty_id='17'):
        self.spell_id = spell_id
        self.cur = cur
        self.logger = logger
        self.difficulty_id = difficulty_id
        spell_info = self.get_spell()
        if spell_info:
            self.spell_name, self.spell_description, self.icon_path = spell_info[0]['spell_name'], spell_info[0]['spell_description'], spell_info[0]['icon_path']
        else:
            self.spell_name, self.spell_description, self.icon_path = None, None, None
        
    def __nonzero__(self):
        return bool(self.spell_name or self.spell_description or self.icon_path)
            
    def get_spell(self):
        """
        rvalue: spell_name, spell_description, icon_path
        """
        self.cur.execute('select a.spell_name,a.spell_description,c.icon_path \
            from Spell a \
            left join SpellMisc b on a.spell_misc_id=b.Id \
            left join SpellIcon c on b.spell_icon_id=c.Id \
            where a.spell_id="{self.spell_id}"'.format(self=self))
        ret = self.cur.fetchall()
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
                desc = re.sub('\$@spellname%s' % id, '[wow,spell,%s,cn[%s]]' % (id, Spell(id, self.cur, self.logger).spell_name), desc)
        spelldescs = re.findall('\$@spelldesc([0-9]+)', desc)
        if spelldescs:
            for id in spelldescs:
                desc = re.sub('\$@spelldesc%s' % id, Spell(id, self.cur, self.logger).spell_description , desc)
        
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
        sql = 'select effect.base_points, effect.bonus_points from Spell spell left join SpellEffect effect on spell.spell_id=effect.spell_id left join Difficulty difficulty on effect.difficulty_id=difficulty.difficulty_id where spell.spell_id="{spell_id}" and effect.effect_no="{effect_no}" and (difficulty.difficulty_id="{self.difficulty_id}" or difficulty.difficulty_id is NULL) order by difficulty.difficulty_id desc limit 1'.format(
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
            sql = 'select radius.Unknown0 from spell spell left join spelleffect effect on spell.spell_id=effect.spell_id left join spellradius radius on effect.Unknown22_0=radius.id where spell.spell_id="{spell_id}" and effect_no="{effect_no}" and (effect.difficulty_id="{self.difficulty_id}" or effect.difficulty_id is NULL) order by effect.difficulty_id desc limit 1'.format(
                self = self,
                spell_id = spell_id if spell_id else self.spell_id,
                effect_no = int(effect_no)-1 if effect_no else 0,
            )
        elif typ == 'A':
            sql = 'select radius.Unknown0 from spell spell left join spelleffect effect on spell.spell_id=effect.spell_id left join spellradius radius on effect.Unknown22_1=radius.id where spell.spell_id="{spell_id}" and effect_no="{effect_no}" and (effect.difficulty_id="{self.difficulty_id}" or effect.difficulty_id is NULL) order by effect.difficulty_id desc limit 1'.format(
                self = self,
                spell_id = spell_id if spell_id else self.spell_id,
                effect_no = int(effect_no)-1 if effect_no else 0,
            )
        self.cur.execute(sql)
        result = self.cur.fetchone()
        if result and result['Unknown0']:
            return result['Unknown0']
        else:
            return s
            
    def parse_spell_description_d(self, s):
        """处理类似 $12345d 的文本"""
        spell_id = re.findall('\$([0-9]*)[Dd]{1}', s)[0]
        sql = 'select duration.base_duration from spell spell left join spellmisc misc on spell.spell_misc_id=misc.id left join spellduration duration on misc.duration_index=duration.id where spell.spell_id="{spell_id}"'.format(
            spell_id = spell_id if spell_id else self.spell_id
        )
        self.cur.execute(sql)
        result = self.cur.fetchone()
        if result and result['base_duration']:
            return u'%s秒' % (int(result['base_duration'])/1000)
        else:
            return s
    
    def parse_spell_description_d(self, s):
        """处理类似 $12345d 的文本"""
        spell_id = re.findall('\$([0-9]*)[Dd]{1}', s)[0]
        sql = 'select duration.base_duration from spell spell left join spellmisc misc on spell.spell_misc_id=misc.id left join spellduration duration on misc.duration_index=duration.id where spell.spell_id="{spell_id}"'.format(
            spell_id = spell_id if spell_id else self.spell_id
        )
        self.cur.execute(sql)
        result = self.cur.fetchone()
        if result and result['base_duration']:
            return u'%s秒' % (int(result['base_duration'])/1000)
        else:
            return s

    def parse_spell_description_t(self, s):
        """处理类似 $12345t 的文本"""
        spell_id, effect_no = re.findall('\$([0-9]*)[Tt]{1}([0-9]*)', s)[0]
        sql = 'select Unknown2 from spelleffect where spell_id="{spell_id}" and effect_no="{effect_no}"'.format(
            spell_id = spell_id if spell_id else self.spell_id,
            effect_no = int(effect_no)-1 if effect_no else 0,
        )
        self.cur.execute(sql)
        result = self.cur.fetchone()
        if result and result['Unknown2']:
            return u'%s秒' % (int(result['Unknown2'])/1000)
        else:
            return s