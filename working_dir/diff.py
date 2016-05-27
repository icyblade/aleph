#! /usr/bin/env python
#! coding: utf8
import os, argparse, re, glob

db_user = 'aleph'
db_pass = 'swbrIcu3Iv4cEhnTzmJL'

# parse args
parser = argparse.ArgumentParser(description='')
parser.add_argument('--locale', '-l', default='zhCN',
                    help='Locale to extract, eg. zhCN, default zhCN')
parser.add_argument('--version', 
                    help='version string from versions, eg. WOW-21655patch7.0.3_Beta')
args = vars(parser.parse_args())

# generate db_name
db_name = '%s.%s_%s' % (
    re.findall('WOW\-([0-9]+)patch([0-9.]+)_Beta',args['version'])[0][::-1]+(args['locale'],)
)

# clear caches/trashes
#os.system('rm -rf CASCExplorer/CASCConsole/cache/')

# Blizzard CDN -> local dbcs
print('[*] Downloading necessary files...')
os.system('mono CASCConsole.exe "DBFilesClient*" ./ ./ {locale} None True "{version}"'.format(
    locale = args['locale'],
    version = args['version']
))
os.system('mv DBFILESCLIENT/ DBFilesClient/')

# create db
os.system('mysql --user={db_user} --password={db_pass} -e "drop database if exists \`{db_name}\`"'.format(
    db_user = db_user, db_pass = db_pass, db_name = db_name
))
os.system('mysql --user={db_user} --password={db_pass} -e "create database \`{db_name}\`"'.format(
    db_user = db_user, db_pass = db_pass, db_name = db_name
))

# dbc2sql
for file in glob.glob('./DBFilesClient/*'):
    dbc_name = os.path.splitext(os.path.basename(file))[0]
    print('[*] Importing %s...' % dbc_name)
    os.system('mono DBC\ Viewer.exe %s' % file)
    os.system('mysql --user=aleph --password=%s %s < %s.sql' % (db_pass, db_name, dbc_name))
    os.system('rm -rf %s.sql' % dbc_name)

# diff
os.system('./lcqcmp {db_user} {db_pass} nga.txt'.format(
    db_user = db_user,
    db_pass = db_pass,
))
