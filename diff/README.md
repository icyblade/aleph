## diff

* *dbc_extract* extract all zh_CN strings from dbc into corpus.

  command-line: `dbc_extract x:\path\to\wow\data`

* copy `corpus_zh.inc` from dbc_extract to dict_extract.

* *dict_extract* find words from corpus and construct a dictionary.

* copy `dict.inc` from dict_extract to lcqcmp.

* lcqcmp connect to the db server, find the latest 2 db, and do diff compare.

  command-line: `lcqcmp db_user db_password output`

  output bbcode for NGA via stdout, encoded in utf-8 (without BOM). if `output` parameter is given, redirect output into file.