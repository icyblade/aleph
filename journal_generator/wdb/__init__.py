import sqlite3, os, importlib, re, csv
from collections import OrderedDict
from file_object import FileObject

def string_to_dec(string):
    """
    :param string: some binary data, which should be something like int32
    :rtype       : base 16 int
    """
    return int(''.join('{:02x}'.format(ord(c)) for c in string[::-1]), 16)

def wdb2sqlite(wdb_path, sqlite_path):
    """
    :param wdb_path    : .\extract\7.0.1.21063\zhCN\DBFilesClient\Spell.db2
    :param sqlite_path : .\extract\7.0.1.21063\zhCN\DBFilesClient.db
    """
    # wdb to csv
    type = analyze_db(wdb_path)
    wdb_dir = os.path.dirname(wdb_path) # extract\7.0.1.21063\zhCN\DBFilesClient
    wdb_name = os.path.splitext(os.path.basename(wdb_path))[0].lower() # spell
    csv_path = '%s/%s.csv' % (wdb_dir, wdb_name)
    
    if type == 'WDBC':
        print('Not implented yet')
    elif type == 'WDB4':
        wdb42csv(wdb_path, csv_path)
    
    # import into sqlite
    with open(csv_path, 'r') as f:
        #csv_data = [i.split(',') for i in f.read().decode('utf8').splitlines()]
        csv_data = []
        for row in csv.reader(f, delimiter=','):
            csv_data.append([i.decode('utf8') for i in row])
    header, data = csv_data[0], csv_data[1:]
    
    con = sqlite3.connect(sqlite_path)
    cur = con.cursor()
    cur.execute('drop table if exists %s' % wdb_name)
    cur.execute('create table {wdb_name} ({cols})'.format(
        wdb_name = wdb_name,
        cols = ','.join(header)
    ))
    cur.executemany('insert into {wdb_name} values ({value})'.format(
        wdb_name = wdb_name,
        value = ','.join(['?']*len(header)),
    ), data)
    cur.execute('create index idx_{wdb_name} on {wdb_name} ({cols})'.format(
        wdb_name = wdb_name,
        cols = ','.join(header)
    ))
    con.commit()
    cur.close()
    con.close()
    
    
def analyze_db(db_path):
    with open(db_path, 'rb') as f:
        magic = f.read(4)
    return magic

def wdb42csv(db_path, csv_path):
    # https://wowdev.wiki/DB2#WDB4_.28.db2.29_.2F_WCH5_.28.adb.29
    fi = FileObject(db_path)
    fo = open(csv_path, 'wb+')
    wdb_name = os.path.splitext(os.path.basename(db_path))[0]
    structure = getattr(__import__(
        'structures.%s' % wdb_name.lower()
    ), wdb_name.lower()).structure
    header = {}
    header['magic'] = fi.read_string_4()
    header['record_count'] = fi.read_uint32_LE()
    header['field_count'] = fi.read_uint32_LE()
    header['record_size'] = fi.read_uint32_LE()
    header['string_table_size'] = fi.read_uint32_LE()
    header['table_hash'] = fi.read_uint32_LE()
    header['build'] = fi.read_uint32_LE()
    header['timestamp_last_written'] = fi.read_uint32_LE()
    header['min_id'] = fi.read_uint32_LE()
    header['max_id'] = fi.read_uint32_LE()
    header['locale'] = fi.read_uint32_LE()
    header['copy_table_size'] = fi.read_uint32_LE()
    header['flags'] = fi.read_uint32_LE()
    
    # process titles
    titles = [name for name in structure]

    # extract read data
    records = []
    for row in xrange(header['record_count']):
        line = []
        for name, typ in structure.iteritems():
            method = getattr(fi, typ)
            if method.__name__ != 'non_inline_id':
                line.append(method())
        records.append(line)

    # extract string table
    string_table = {}
    string_table_start = fi.tell()
    string_table_end = string_table_start + header['string_table_size']
    pos = string_table_start
    s = fi.read_char()
    while fi.tell() < string_table_end:
        c = fi.read_char()
        if c == '':
            break
        elif c == '\00':
            string_table[pos-string_table_start] = s
            pos = fi.tell()
            s = ''
        else:
            s = s + c
    
    # if has non-inline IDs
    if header['flags'] & 0x04 != 0:
        for idx, row in enumerate(records):
            records[idx] = [fi.read_int32_LE()]+row  
    
    # match string
    for line_no, line in enumerate(records):
        for index, value in enumerate(line):
            if structure.items()[index][1].startswith('read_string'):
                tmp = string_to_dec(value)
                if tmp in string_table:
                    line[index] = string_table[tmp]
                else:
                    line[index] = tmp
        records[line_no] = line

    # trim output
    for i in xrange(len(records)):
        for j in xrange(len(records[i])):
            if records[i][j] == '\00':
                records[i][j] = ''
            else:
                pass

            
    # output
    wr = csv.writer(fo, delimiter=',', quoting=csv.QUOTE_ALL)
    wr.writerow(titles)
    wr.writerows(records)

    fi.close()
    fo.close()
