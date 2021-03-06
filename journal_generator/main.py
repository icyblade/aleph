#! coding: utf8
import sqlite3, re, os, glob
from StringIO import StringIO
from client import Client
from logger import configure_logger
from dbc import DBFilesClient
from collections import OrderedDict

DIFFICULTY_ID = 15 # 10-30 弹性英雄模式
#DIFFICULTY_ID = 16 # 20 人史诗模式

def section_type_to_nga_img(s):
    types = s.split(',')
    d = {
        u'坦克预警': 'http://wow.zamimg.com/images/icons/ej-tank.png',
        u'DPS预警': 'http://wow.zamimg.com/images/icons/ej-dps.png',
        u'治疗预警': 'http://wow.zamimg.com/images/icons/ej-healer.png',
        u'灭团技': 'http://wow.zamimg.com/images/icons/ej-deadly.png',
        u'重要': 'http://wow.zamimg.com/images/icons/ej-important.png',
        u'可打断技能': 'http://wow.zamimg.com/images/icons/ej-interrupt.png',
        u'法术效果': 'http://wow.zamimg.com/images/icons/ej-magic.png',
        u'激怒': 'http://wow.zamimg.com/images/icons/ej-enrage.png',
        u'史诗难度': 'http://wow.zamimg.com/images/icons/ej-heroic.png',
    }
    return ''.join(['[img]%s[/img]' % d[i] for i in types])

def main():
    c = Client('D:/Program Files/World of Warcraft Beta')
    
    # generate journal
    print('Generating Journal...'),
    journal = {}
    dbc = DBFilesClient(c.build_number)
    j = dbc.Journal(difficulty_id=DIFFICULTY_ID)
    for expansions in j.get_expansions():
        expansion_id, expansion_name = expansions['expansion_id'], expansions['expansion_name']
        if expansion_name != u'军团再临': continue
        journal[expansion_name] = {}
        for instances in j.get_instances(expansion_id):
            instance_id, instance_name, instance_description = instances['instance_id'], instances['instance_name'], instances['instance_description']
            #if not instance_name in [u'暗夜要塞', u'翡翠梦魇']: continue
            journal[expansion_name][instance_name] = OrderedDict([])
            for bosses in j.get_bosses(instance_id):
                boss_id, boss_name, boss_description = bosses['boss_id'], bosses['boss_name'], bosses['boss_description']
                journal[expansion_name][instance_name][boss_name] = j.get_encounter_section(boss_id)
   

    # formatter
    if not os.path.exists('./output'): os.mkdir('./output')
    for expansion in journal:
        for raid in journal[expansion]:
            with open('./output/%s.txt' % raid, 'w') as f:
                ret = ''
                for boss in journal[expansion][raid]:
                    ret += '[collapse=%s]\n' % boss
                    current_layer = 0
                    ret += '[list]\n'
                    for k, layer in journal[expansion][raid][boss]:
                        if layer > current_layer:
                            ret += '[list]\n'*(layer-current_layer)
                            current_layer = layer
                        elif layer < current_layer:
                            ret += '[/list]\n'*(current_layer-layer)
                            current_layer = layer
                            
                        creature_name = '[%s]' % k['creature_name'] if k['creature_name'] else ''
                        icon = '[img]http://wow.zamimg.com/images/wow/icons/small/%s.jpg[/img]' % k['icon_path'].replace(' ', '_') if k['icon_path'] else ''
                        
                        ret += u'[*] {creature}{icon}{title}{type}\n{desc}\n'.format(
                            creature = creature_name,
                            icon = icon,
                            title = k['journal_title'] if not k['spell_id'] else '[wow,spell,%s,cn[%s]]' % (k['spell_id'], k['journal_title']),
                            type = section_type_to_nga_img(k['section_type']) if k['section_type'] else '',
                            desc = k['journal_body_text']
                        )
                    ret += '[/list]\n'*(current_layer+1)
                    ret += '[/collapse]\n'
                ret += '\n'
                f.write(ret.encode('utf8'))
    print('DONE')
    
if __name__ == '__main__':
    main()
