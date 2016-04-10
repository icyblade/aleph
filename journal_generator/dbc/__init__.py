#! coding: utf8
import sqlite3, re, os, glob
from StringIO import StringIO
from client import Client
from logger import configure_logger
from dbc.spell import Spell
from dbc.journal import Journal

class DBFilesClient:
    def __init__(self, sqlite_file_path):
        self.sqlite_file_path = sqlite_file_path
        self.con = sqlite3.connect(sqlite_file_path)
        self.logger = configure_logger()
        
        # return row as dict
        def dict_factory(cursor, row):
            d = {}
            for idx, col in enumerate(cursor.description):
                d[col[0]] = row[idx]
            return d
        self.con.row_factory = dict_factory
        
        self.cur = self.con.cursor()
        
    def __del__(self):
        self.cur.close()
        self.con.close()
        
    def Spell(self, spell_id):
        return Spell(spell_id, self.cur, self.logger)
        
    def Journal(self, difficulty_id='17'):
        return Journal(self.cur, self.logger, difficulty_id=difficulty_id)
