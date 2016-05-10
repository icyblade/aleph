## diff

* *dbc_extract* extract all zh_CN strings from dbc into corpus.

  command-line: `dbc_extract x:\path\to\wow\data`

* copy `corpus_zh.inc` from dbc_extract to dict_extract.

* *dict_extract* find words from corpus and construct a dictionary.

* copy `dict.inc` from dict_extract to lcqcmp.

* lcqcmp connect to the db server, find the latest 2 db, and do diff compare.

  command-line: `lcqcmp db_user db_password output`

  output bbcode for NGA via stdout, encoded in utf-8 (without BOM). if `output` parameter is given, redirect output into file.

  ## 去你妹的吧

  命令行 lcqcmp 后面可以接三个参数

  数据库用户名、数据库密码、输出文件名

  不指定第三个参数就会输出到stdout里

  输出格式UTF-8 without BOM，换行为\n

  第一行是标题余下是内容

  lcqcmp需要读取一个字典文件来帮助分词，字典文件是由另外两个程序提取出来的

  https://github.com/AeanSR/wow_dict

  这里有现成的，放在同一目录下
