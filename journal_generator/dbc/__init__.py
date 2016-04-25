#! coding: utf8
import re, os, glob
import mysql.connector
from StringIO import StringIO
from client import Client
from logger import configure_logger
from dbc.spell import Spell
from dbc.journal import Journal
from config.connection import db_config

class DBFilesClient:
    def __init__(self, build_number, locale='zhCN'):
        self.build_number = build_number
        self.locale = locale
        self.con = mysql.connector.connect(**db_config)
        self.logger = configure_logger()
        
        # return row as dict
        self.cur = self.con.cursor(dictionary=True, buffered=True)
        
    def __del__(self):
        self.cur.close()
        self.con.close()
        
    def Spell(self, spell_id):
        return Spell(spell_id, self.cur, self.logger)
        
    def Journal(self, difficulty_id):
        return Journal(self.cur, self.logger, difficulty_id=difficulty_id)
