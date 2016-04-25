#! coding: utf8
import sqlite3, os
from logger import configure_logger

class Client:
    """WoW 客户端类"""
    def __init__(self, root, locale_flags='zhCN', content_flags=None):
        """
        :param root         : 客户端根目录，如 D:/World of Warcraft Beta
        :param locale_flags : 语系，如 zhCN
        :param content_flags: content flags
        """
        self.root = root
        self.locale_flags = locale_flags
        self.content_flags = content_flags
        self.logger = configure_logger()
        self.get_build_info()
        
    def get_build_info(self):
        """获取版本号"""
        with open('{self.root}\\.build.info'.format(self = self)) as f:
            data = f.read()
        build_number = list(set([
            i.split('|')[11] 
            for i in data.split('\n')[1:] 
            if i
        ]))
        if len(build_number) == 1:
            self.build_number = build_number[0]
        else:
            raise Exception('More than one build info in .build.info')
