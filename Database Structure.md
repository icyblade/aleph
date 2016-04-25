# 数据库名
统一使用 **完整版本号_locale** 格式。区分大小写

例如: 7.0.3.21414_enUS， 6.2.4.21463_zhCN

# 表名
统一使用 **前缀_表名** 格式。区分大小写

前缀有:

- dbc

  用于储存从 dbc 里解出来的原始表
  
  例如 dbc_Spell, dbc_FileDataComplete
  
- rep

  用于储存替换过 $ 的表

  例如 rep_Spell

# 表结构

## 字段类型

要求所有字段类型和 dbc 内统一

目前字段类型和 dbc 类型对应为:

| 字段类型       | dbc 类型      |
| -------------  |:-------------:| 
| varchar        | string        |
| tinyint        | int8          |
| smallint       | int16         |
| mediumint      | int24         |
| int            | int32         |
| bigint         | int64         |
| float          | float         |

## 索引

所有表必须加相关索引、主键

## 表引擎

统一使用 MyISAM