#! coding: utf8
import sqlite3, re, os, glob
from StringIO import StringIO
from client import Client
from logger import configure_logger
from dbc import DBFilesClient

def main():
    c = Client('D:/Program Files/World of Warcraft Beta')
    print('Current build: {c.build_number}'.format(c=c))
    
    c.extract_dbs()
    wdbs = ['JournalTier.db2', 'JournalEncounter.db2', 'JournalEncounterSection.db2', 'JournalInstance.db2', 'JournalTierXInstance.db2', 'Spell.db2', 'SpellMisc.db2', 'Creature.db2']
    for path in wdbs:
        print('Processing: %s...' % path),
        c.wdb2sqlite(path)
        print('DONE')
    
    sqlite_file_path = '.\\extract\\{c.build_number}\\{c.locale_flags}\\DBFilesClient.db'.format(
        c = c,
    )
    
    # generate journal
    print('Generating Journal...'),
    journal = {}
    dbc = DBFilesClient(sqlite_file_path)
    j = dbc.Journal()
    for expansions in j.get_expansions():
        expansion_id, expansion_name = expansions['expansion_id'], expansions['expansion_name']
        journal[expansion_name] = {}
        for instances in j.get_instances(expansion_id):
            instance_id, instance_name, instance_description = instances['instance_id'], instances['instance_name'], instances['instance_description']
            journal[expansion_name][instance_name] = {}
            for bosses in j.get_bosses(instance_id):
                boss_id, boss_name, boss_description = bosses['boss_id'], bosses['boss_name'], bosses['boss_description']
                journal[expansion_name][instance_name][boss_name] = j.get_encounter_section(boss_id)
    print('DONE')

    # formatter
    ret = ''
    for expansion in journal:
        if expansion != u'军团再临': continue
        for raid in journal[expansion]:
            if not raid in [u'The Nighthold', u'翡翠梦魇']: continue
            ret += '[quote][collapse=%s]' % raid
            for boss in journal[expansion][raid]:
                if boss != u'梦魇之龙': continue
                ret += '[h]%s[/h]' % boss
                current_layer = 0
                ret += '[list]'
                for k, layer in journal[expansion][raid][boss]:
                    if layer > current_layer:
                        ret += '[list]'*(layer-current_layer)
                        current_layer = layer
                    elif layer < current_layer:
                        ret += '[/list]'*(current_layer-layer)
                        current_layer = layer
                        
                    creature_name = '[%s]' % k['creature_name'] if k['creature_name'] else ''
                    icon = '[img]http://wow.zamimg.com/images/wow/icons/small/%s.jpg[/img]' % k['icon_path'].replace(' ', '_') if k['icon_path'] else ''
                    
                    ret += u'[*] {creature}{icon}{title}{type}\n{desc}'.format(
                        creature = creature_name,
                        icon = icon,
                        title = k['journal_title'],
                        type = '(%s)' % k['section_type'] if k['section_type'] else '',
                        desc = k['journal_body_text']
                    )
                ret += '[/list]'*(current_layer+1)
            ret += '[/collapse][/quote]'

    with open('./nga_format.txt', 'w') as f:
        f.write(ret.encode('utf8'))
    
if __name__ == '__main__':
    main()
