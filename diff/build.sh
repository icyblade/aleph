cp ./dict_extract/dict.inc ~/dict.inc
cd lcqcmp
g++ lcqcmp.cpp -o lcqcmp -I/usr/include/cppconn -lmysqlcppconn-static `mysql_config --cflags --libs` -std=c++11
cp ./lcqcmp ~/lcqcmp