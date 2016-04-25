/*
    dictionary extractor.
    Aeandarian(a.k.a. fhsvengetta) 2016.4.11
*/
#define _CRT_SECURE_NO_WARNINGS

#include <cstdio>
#include <vector>
#include <string>
#include <list>
#include <unordered_map>
#include <set>
#include <sstream>
#include <utility>
#include <cmath>
#include <algorithm>

const int max_word_length = 7;
const double frequency_threshold = 2.0;
const double coherency_threshold = 20.0;
const double entropy_threshold = 2.0;
const double relative_entropy_threshold = 0.6;
const char* structural_list[] = {
    u8"你", u8"您", u8"谁",
    u8"哪", u8"那", u8"这", u8"的", u8"了", u8"着", u8"也", u8"是", u8"有", u8"在", u8"与", u8"呢",
    u8"啊", u8"呀", u8"吧", u8"嗯", u8"哦", u8"哈", u8"呐",
    u8"之", u8"·", u8"并",
    0
};

std::unordered_map<int, std::string> zhch_code;
std::unordered_map<std::string, int> zhch_code_inv;
std::vector<int> coded_text;
unsigned int character_counter = 0;

struct zhword_t {
    std::list<int> chs;
    std::string to_string() const {
        std::string k = "";
        for (auto i = chs.begin(); i != chs.end(); i++) {
            k += zhch_code[*i];
        }
        return k;
    }
    void from_string( std::string str ) {
        std::stringstream ss( str );
        std::string item = "";
        int gc;
        chs.clear();
        while (EOF != ( gc = ss.get() )) {
            if (( gc >= 'a' && gc <= 'z' ) || ( gc >= 'A' && gc <= 'Z' ) || ( gc >= '0' && gc <= '9' )) {
                item.push_back( gc );
            } else {
                if (!item.empty()) chs.push_back( zhch_code_inv[item] );
                item.clear();
                item.push_back( gc );
                if (( gc & 0xE0 ) == 0xC0) {
                    item.push_back( ss.get() );
                } else if (( gc & 0xF0 ) == 0xE0) {
                    item.push_back( ss.get() );
                    item.push_back( ss.get() );
                } else if (( gc & 0xF8 ) == 0xF0) {
                    item.push_back( ss.get() );
                    item.push_back( ss.get() );
                    item.push_back( ss.get() );
                } else if (( gc & 0xFC ) == 0xF8) {
                    item.push_back( ss.get() );
                    item.push_back( ss.get() );
                    item.push_back( ss.get() );
                    item.push_back( ss.get() );
                } else if (( gc & 0xFE ) == 0xFC) {
                    item.push_back( ss.get() );
                    item.push_back( ss.get() );
                    item.push_back( ss.get() );
                    item.push_back( ss.get() );
                    item.push_back( ss.get() );
                }
                    chs.push_back( zhch_code_inv[item] );
                item.clear();
            }
        }
        if (!item.empty()) chs.push_back( zhch_code_inv[item] );
    }
    int pop_front() {
        int ret = chs.front();
        chs.erase( chs.begin() );
        return ret;
    }
    void push_back( int code ) {
        chs.push_back( code );
    }
    void push_front( int code ) {
        chs.insert( chs.begin(), code );
    }
    void push_back( std::string zhch ) {
        int code = zhch_code_inv[zhch];
        chs.push_back( code );
    }
    void push_front( std::string zhch ) {
        int code = zhch_code_inv[zhch];
        chs.insert( chs.begin(), code );
    }
    size_t length() const {
        return chs.size();
    }
    void clear() {
        chs.clear();
    }
    void reverse() {
        chs.reverse();
    }
};
std::unordered_map<std::string, std::tuple<int,double,double> > eval;
std::vector<std::string> candidate;
#define frequency(str) (std::get<0>(eval[str]))
#define coherency(str) (std::get<1>(eval[str]))
#define entropy(str)   (std::get<2>(eval[str]))

unsigned int feasible_occur_counter = 0;

int register_zhch( std::string zhch ) {
    if (zhch_code_inv.find( zhch ) == zhch_code_inv.end()) {
        zhch_code[character_counter] = zhch;
        zhch_code_inv[zhch] = character_counter;
        return character_counter++;
    }
    return zhch_code_inv[zhch];
}

void read_corpus( FILE* f ) {
    char gc;
    std::string item;
    while (EOF != ( gc = fgetc( f ) )) {
        if (( gc >= 'a' && gc <= 'z' ) || ( gc >= 'A' && gc <= 'Z' ) || ( gc >= '0' && gc <= '9' )) {
            item.push_back( gc );
        } else {
            if (!item.empty()) coded_text.push_back( register_zhch( item ) );
            item.clear();
            item.push_back( gc );
            if (( gc & 0xE0 ) == 0xC0) {
                item.push_back( fgetc( f ) );
            } else if (( gc & 0xF0 ) == 0xE0) {
                item.push_back( fgetc( f ) );
                item.push_back( fgetc( f ) );
            } else if (( gc & 0xF8 ) == 0xF0) {
                item.push_back( fgetc( f ) );
                item.push_back( fgetc( f ) );
                item.push_back( fgetc( f ) );
            } else if (( gc & 0xFC ) == 0xF8) {
                item.push_back( fgetc( f ) );
                item.push_back( fgetc( f ) );
                item.push_back( fgetc( f ) );
                item.push_back( fgetc( f ) );
            } else if (( gc & 0xFE ) == 0xFC) {
                item.push_back( fgetc( f ) );
                item.push_back( fgetc( f ) );
                item.push_back( fgetc( f ) );
                item.push_back( fgetc( f ) );
                item.push_back( fgetc( f ) );
            }
            coded_text.push_back( register_zhch( item ) );
            item.clear();
        }
    }
    if (!item.empty()) coded_text.push_back( register_zhch( item ) );
}

void statistic_priori() {
    for (int length = 1; length <= max_word_length + 1; length++) {
        zhword_t zhw;
        zhw.clear();
        int last = 0;
        for (auto i = coded_text.begin(); i != coded_text.end(); i++) {
            if (*i == zhch_code_inv["\n"]) {
                zhw.clear();
                continue;
            }
            zhw.push_back( *i );
            if (zhw.length() == length) {
                feasible_occur_counter++;
                frequency(zhw.to_string()) ++;
                zhw.pop_front();
            }
        }
    }
}

void statistic_coherency() {
    for (auto i = eval.begin(); i != eval.end(); i++) {
        zhword_t zhw;
        zhw.from_string(i->first);
        if (zhw.length() < 2) {
            candidate.push_back(zhw.to_string());
            continue;
        }
        if (zhw.length() > max_word_length) continue;
        if (std::get<0>(i->second) < frequency_threshold) continue; 
        double min_co = 1e308;
        zhword_t lhs, rhs;
        rhs = zhw;
        while (rhs.length() >= 2) {
            lhs.push_back( rhs.pop_front() );
            double pri = ( 1.0 / feasible_occur_counter ) * std::get<0>(i->second);
            double lco = ( 1.0 / feasible_occur_counter ) * frequency(lhs.to_string());
            double rco = ( 1.0 / feasible_occur_counter ) * frequency(rhs.to_string());
            double co = 2 * pri / ((lco * lco) + (rco * rco));
            min_co = std::min( min_co, co );
        }
        coherency(zhw.to_string()) = min_co;
        if (min_co >= (zhw.length() - 1) * coherency_threshold)
            candidate.push_back(zhw.to_string());
    }
}

int probe_frequency( zhword_t& zhw ) {
    auto it = eval.find( zhw.to_string() );
    if (it == eval.end()) {
        return 0;
    }
    return std::get<0>(it->second);
}

std::vector<std::string> radix;
std::vector<std::string> inv_radix;
struct dict_t {
    std::string zhw;
    int freq;
    double co;
    double en;
    dict_t( std::string& z, int f, double co, double en ) : zhw( z ), freq( f ), co(co), en(en) { }
    bool operator<( const dict_t& rhs ) {
        return rhs.freq < freq;
    }
};
std::vector<dict_t> dict;

void statistic_entropy() {
    int c = 0;
    for (auto i = eval.begin(); i != eval.end(); i++) {
        zhword_t zhw;
        zhw.from_string(i->first);
        if (zhw.length() < 2) continue;
        radix.push_back(zhw.to_string());
        zhw.reverse();
        inv_radix.push_back(zhw.to_string());
    }
    std::sort(radix.begin(), radix.end());
    std::sort(inv_radix.begin(), inv_radix.end());
    for (auto i = candidate.begin(); i != candidate.end(); i++) {
        zhword_t zhw, inv_zhw;
        std::vector<int> ln, rn;
        double lentropy = 0, rentropy = 0;
        double lcount = 0, rcount = 0;

        c++;
        if (!( c & 0xff )) {
            printf( "%d/%d            \r", c, candidate.size() );
        }

        zhw.from_string(*i);
        auto er = std::equal_range(radix.begin(), radix.end(), zhw.to_string(), 
            [](const std::string& lhs, const std::string& rhs) {
                if (rhs.find(lhs) == 0) return false;
                return lhs < rhs;
            }
        );
        int last = 0;
        for (auto j = er.first; j < er.second; j++) {
            zhword_t ext;
            ext.from_string(*j);
            if (ext.length() <= zhw.length()) continue;
            for(int i = 0; i < zhw.length(); i++) ext.pop_front();
            int next = ext.pop_front();
            if (last == next) continue;
            last = next;
            const char** s = &structural_list[0];
            int skip_structural = 0;
            while (*s) {
                if (0 == zhch_code[next].compare( *s )) {
                    skip_structural = 1;
                    break;
                }
                s++;
            }
            if (skip_structural) {
                rentropy = entropy_threshold;
            }

            ext = zhw;
            ext.push_back(next);
            int count = frequency(ext.to_string());
            rcount += count;
            rn.push_back(count);
        }

        inv_zhw = zhw;
        inv_zhw.reverse();
        er = std::equal_range(inv_radix.begin(), inv_radix.end(), inv_zhw.to_string(), 
            [](const std::string& lhs, const std::string& rhs) {
            if (rhs.find(lhs) == 0) return false;
            return lhs < rhs;
        }
        );
        last = 0;
        for (auto j = er.first; j < er.second; j++) {
            zhword_t ext;
            ext.from_string(*j);
            if (ext.length() <= inv_zhw.length()) continue;
            for(int i = 0; i < inv_zhw.length(); i++) ext.pop_front();
            int next = ext.pop_front();
            if (last == next) continue;
            last = next;
            const char** s = &structural_list[0];
            int skip_structural = 0;
            while (*s) {
                if (0 == zhch_code[next].compare( *s )) {
                    skip_structural = 1;
                    break;
                }
                s++;
            }
            if (skip_structural) {
                lentropy = entropy_threshold;
            }
            ext = inv_zhw;
            ext.push_back(next);
            ext.reverse();
            int count = frequency(ext.to_string());
            lcount += count;
            ln.push_back(count);
        }

        if (lcount > 0.0) {
            lcount = 1.0 / lcount;
            for (auto i = ln.begin(); i != ln.end(); i++) {
                double en = lcount * *i;
                if (en <= .0) continue;
                en = en * -log( en );
                lentropy += en;
            }
        } else {
            lentropy = -1.0;
        }
        if (rcount > 0.0) {
            rcount = 1.0 / rcount;
            for (auto i = rn.begin(); i != rn.end(); i++) {
                double en = rcount * *i;
                if (en <= .0) continue;
                en = en * -log( en );
                rentropy += en;
            }
        } else {
            rentropy = -1.0;
        }
        double min_en;
        if (lentropy < .0 && rentropy < .0) {
            min_en = 2.0;
        } else if (lentropy < .0) {
            min_en = rentropy * 2.0;
        } else if (rentropy < .0) {
            min_en = lentropy * 2.0;
        } else {
            min_en = lentropy * rentropy;
        }
        entropy(zhw.to_string()) = min_en;
    }    
}

void dict_sort() {
    for (auto i = candidate.begin(); i != candidate.end(); i++) {
        zhword_t zhw, inv_zhw;
        zhw.from_string(*i);
        inv_zhw = zhw;
        inv_zhw.reverse();
        double max_en = 0.0;
        double this_en = entropy(*i);
        auto er = std::equal_range(radix.begin(), radix.end(), zhw.to_string(), 
            [](const std::string& lhs, const std::string& rhs) {
            if (rhs.find(lhs) == 0) return false;
            return lhs < rhs;
        }
        );
        for (auto j = er.first; j < er.second; j++) {
            zhword_t fix;
            fix.from_string(*j);
            const char** s = &structural_list[0];
            int skip_structural = 0;
            while (*s) {
                if (0 == zhch_code[fix.chs.back()].compare( *s )) {
                    skip_structural = 1;
                    break;
                }
                if (0 == zhch_code[fix.chs.front()].compare( *s )) {
                    skip_structural = 1;
                    break;
                }
                s++;
            }
            if (skip_structural) continue;

            max_en = std::max( max_en, entropy(*j) );
        }
        er = std::equal_range(inv_radix.begin(), inv_radix.end(), inv_zhw.to_string(), 
            [](const std::string& lhs, const std::string& rhs) {
            if (rhs.find(lhs) == 0) return false;
            return lhs < rhs;
        }
        );
        for (auto j = er.first; j < er.second; j++) {
            zhword_t inv_fix;
            inv_fix.from_string(*j);
            inv_fix.reverse();
            const char** s = &structural_list[0];
            int skip_structural = 0;
            while (*s) {
                if (0 == zhch_code[inv_fix.chs.back()].compare( *s )) {
                    skip_structural = 1;
                    break;
                }
                if (0 == zhch_code[inv_fix.chs.front()].compare( *s )) {
                    skip_structural = 1;
                    break;
                }
                s++;
            }
            if (skip_structural) continue;

            max_en = std::max( max_en, entropy(inv_fix.to_string()) );
        }
        for (auto p = structural_list; *p; p++) {
            zhword_t ext = zhw;
            ext.push_back(*p);
            this_en = std::max( this_en, entropy(ext.to_string()) );
            ext = zhw;
            ext.push_front(*p);
            this_en = std::max( this_en, entropy(ext.to_string()) );
        }
        if ( this_en > relative_entropy_threshold * max_en) {
            continue;
        }
        entropy(*i) = 0;
    }
    radix.clear();
    inv_radix.clear();
    for (auto i = candidate.begin(); i != candidate.end(); i++) {
        zhword_t zhw;
        zhw.from_string(*i);
        if (zhw.length() < 2) continue;
        if (entropy(*i) < entropy_threshold) {
            continue;
        }
        const char** s = &structural_list[0];
        int skip_structural = 0;
        while (*s) {
            if (0 == zhch_code[zhw.chs.back()].compare( *s )) {
                skip_structural = 1;
                break;
            }
            if (0 == zhch_code[zhw.chs.front()].compare( *s )) {
                skip_structural = 1;
                break;
            }
            s++;
        }
        if (skip_structural) continue;
        if (entropy(*i) > 0)
            dict.push_back( dict_t( *i, frequency(*i), coherency(*i), entropy(*i) ) );
    }
    std::sort( dict.begin(), dict.end() );
}

int main( int argc, char** argv ) {
    FILE* f = fopen( "corpus_zh.inc", "rb" );
    register_zhch( "\n" );
    printf( "read corpus\n" );
    read_corpus( f );
    fclose( f );
    printf( "frequency\n" );
    statistic_priori();
    coded_text.clear();
    printf( "coherency\n" );
    statistic_coherency();
    printf( "entropy\n" );
    statistic_entropy();
    printf( "\nsort\n" );
    dict_sort();

    f = fopen( "dict.inc", "wb" );
    fprintf( f, "\xEF\xBB\xBF" );
    for (auto i = dict.begin(); i != dict.end(); i++) {
        fprintf( f, "%s\t%d\t%.3f\t%.3f\n", i->zhw.c_str(), i->freq, i->co, i->en );
    }
    fclose( f );
}