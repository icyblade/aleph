#! coding: utf8
import re

class Spell:
    def __init__(self, spell_id, cur, logger, difficulty_id=None):
        self.spell_id = spell_id
        self.cur = cur
        self.logger = logger
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
        spellnames = re.findall('\$@spellname([0-9]+)', desc)
        if spellnames:
            for id in spellnames:
                desc = re.sub('\$@spellname%s' % id, '[wow,spell,%s,cn[%s]]' % (id, Spell(id, self.cur, self.logger).spell_name), desc)
        spelldescs = re.findall('\$@spelldesc([0-9]+)', desc)
        if spelldescs:
            for id in spelldescs:
                desc = re.sub('\$@spelldesc%s' % id, Spell(id, self.cur, self.logger).spell_description , desc)
        return desc