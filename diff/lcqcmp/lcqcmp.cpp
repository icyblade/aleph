/*
	Project:LCQ
	Aean, 2016.4.1
*/
#define _CRT_SECURE_NO_WARNINGS
#include <vector>
#include <string>
#include <sstream>
#include <utility>
#include <algorithm>
#include <cstdio>
#include <iostream>
#include <fstream>
#include <map>
#include <unordered_map>

#include <driver.h>
#include <connection.h>
#include <statement.h>
#include <prepared_statement.h>
#include <resultset.h>
#include <metadata.h>
#include <resultset_metadata.h>
#include <exception.h>
#include <warning.h>

#if defined(__GNUC__) && (__GNUC__ < 4 || (__GNUC__ == 4 && __GNUC_MINOR__ <= 8))
#undef major
#undef minor
#endif
#if defined(__GNUC__)
char* _itoa( int value, char* result, int base ) {
    if (base < 2 || base > 36) { *result = '\0'; return result; }
    char* ptr = result, *ptr1 = result, tmp_char;
    int tmp_value;
    do {
        tmp_value = value;
        value /= base;
        *ptr++ = "zyxwvutsrqponmlkjihgfedcba9876543210123456789abcdefghijklmnopqrstuvwxyz" [35 + (tmp_value - value * base)];
    } while ( value );
    if (tmp_value < 0) *ptr++ = '-';
    *ptr-- = '\0';
    while(ptr1 < ptr) {
        tmp_char = *ptr;
        *ptr--= *ptr1;
        *ptr1++ = tmp_char;
    }
    return result;
}
#endif


enum { OP_CPY, OP_INS, OP_DEL };

std::vector<std::pair<int, std::string> > lcqcmp( std::vector<std::string> vold, std::vector<std::string> vnew ) {
	std::vector<std::vector<int>> score;
	std::vector<std::vector<int>> op;

	score.resize( vold.size() + 1 );
	for (auto i = score.begin(); i != score.end(); i++) {
		i->resize( vnew.size() + 1 );
	}
	op.resize( vold.size() + 1 );
	for (auto i = op.begin(); i != op.end(); i++) {
		i->resize( vnew.size() + 1 );
	}

	for (int i = 0; i <= vold.size(); i++) {
		score[i][0] = i * 16;
		op[i][0] = OP_DEL;
	}
	for (int j = 0; j <= vnew.size(); j++) {
		score[0][j] = j * 16;
		op[0][j] = OP_INS;
	}

	for (int i = 1; i <= vold.size(); i++) {
		for (int j = 1; j <= vnew.size(); j++) {
			int d1 = vold[i - 1].compare( vnew[j - 1] ) ? 0x10000000 : score[i - 1][j - 1];
			int d2 = score[i][j - 1] + 16;
			int d3 = score[i - 1][j] + 16;
			if (op[i][j - 1] != OP_INS) d2 += 1;
			if (op[i - 1][j] != OP_DEL) d3 += 1;
			if (d1 <= d2 && d1 <= d3) {
				score[i][j] = d1;
				op[i][j] = OP_CPY;
			} else if (d2 <= d1 && d2 <= d3) {
				score[i][j] = d2;
				op[i][j] = OP_INS;
			} else {
				score[i][j] = d3;
				op[i][j] = OP_DEL;
			}
		}
	}

	std::vector<std::pair<int, std::string> > stack;
	int i = vold.size();
	int j = vnew.size();
	while (i || j) {
		switch (op[i][j]) {
		case OP_CPY:
			stack.push_back( std::make_pair( OP_CPY, vnew[j - 1] ) );
			i--, j--;
			break;
		case OP_INS:
			stack.push_back( std::make_pair( OP_INS, vnew[j - 1] ) );
			j--;
			break;
		case OP_DEL:
			stack.push_back( std::make_pair( OP_DEL, vold[i - 1] ) );
			i--;
			break;
		}
	}
	std::reverse( stack.begin(), stack.end() );
	return stack;
}

std::vector<std::string> read_split( std::string s ) {
	std::vector<std::string> elems;
	std::stringstream ss( s );
	std::string item = "";
	int gc;
    int decimal = 0;
	while (EOF != ( gc = ss.get() )) {
		if (( gc >= 'a' && gc <= 'z' ) || ( gc >= 'A' && gc <= 'Z' ) || ( gc >= '0' && gc <= '9' ) || (decimal && gc == '.' && isdigit(ss.get()))) {
            decimal = (gc >= '0' && gc <= '9');
            if (gc == '.') ss.unget();
			item.push_back( gc );
		} else {
			if (!item.empty()) elems.push_back( item );
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
			elems.push_back( item );
			item.clear();
		}
	}
	if (!item.empty()) elems.push_back( item );
	return elems;
}
std::unordered_map<std::string, int> ch2code;
std::unordered_map<int, std::string> code2ch;
int next_code = 1;
struct word_t {
	int code[7];
	word_t() : code() { }
	void from_string( const std::string& str ) {
		std::stringstream ss( str );
		char gc;
		std::string item;
		int i = 0;
		while (EOF != ( gc = ss.get() )) {
			if (( gc >= 'a' && gc <= 'z' ) || ( gc >= 'A' && gc <= 'Z' ) || ( gc >= '0' && gc <= '9' )) {
				item.push_back( gc );
			} else {
				if (!item.empty()) code[i++] = ch2code[item];
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
				code[i++] = ch2code[item];
				item.clear();
			}
		}
		while (i < 7) {
			code[i++] = 0;
		}
	}
	int length() const {
		for (int i = 0; i < 7; i++) {
			if (!code[i]) return i;
		}
		return 7;
	}
	bool operator==( const word_t& rhs ) const {
		for (int i = 0; i < 7; i++) {
			if (code[i] != rhs.code[i]) return false;
		}
		return true;
	}
};
struct word_hash_t {
	long long int pair( int l, int r ) const {
		union {
			int i[2];
			long long int l;
		} u;
		u.i[0] = l;
		u.i[1] = r;
		return u.l;
	}
	size_t operator()( const word_t& lhs ) const {
		size_t hash = 0;
		std::hash<long long int> hasher;
		hash ^= hasher( pair( lhs.code[0], 0 ) );
		hash ^= hasher( pair( lhs.code[1], 1 ) );
		hash ^= hasher( pair( lhs.code[2], 2 ) );
		hash ^= hasher( pair( lhs.code[3], 3 ) );
		hash ^= hasher( pair( lhs.code[4], 4 ) );
		hash ^= hasher( pair( lhs.code[5], 5 ) );
		hash ^= hasher( pair( lhs.code[6], 6 ) );
		return hash;
	}
};
std::unordered_map<word_t, int, word_hash_t> word2code;
std::unordered_map<int, word_t> code2word;
std::map<int, int> freq;
std::vector<int> zhch_to_code( std::vector<std::string> chlst ) {
	std::vector<int> codelst;
	for (auto i = chlst.begin(); i != chlst.end(); i++) {
		if (ch2code.find( *i ) != ch2code.end()) {
			codelst.push_back( ch2code[*i] );
			continue;
		}
		ch2code[*i] = next_code;
		code2ch[next_code] = *i;
		codelst.push_back( next_code );
		next_code++;
	}
	return codelst;
}
std::vector<std::string> code_to_zhch( std::vector<int>& codelst ) {
	std::vector<std::string> chlst;
	for (auto i = codelst.begin(); i != codelst.end(); i++) {
		chlst.push_back( code2ch[*i] );
	}
	return chlst;
}
void read_dict() {
	FILE* fdict = fopen( "dict.inc", "rb" );
	fseek( fdict, 0, SEEK_END );
	size_t dictsize = ftell( fdict );
	char* sdict = ( char* ) calloc( dictsize + 1, 1 );
	rewind( fdict );
	fread( sdict, dictsize, 1, fdict );
	fclose( fdict );
	zhch_to_code( read_split( sdict ) );
	free( sdict );

	std::ifstream f( "dict.inc" );
	while (!f.eof()) {
		std::string sword, sfreq, sline;
		word_t word;
		std::getline( f, sword, '\t' );
		std::getline( f, sfreq, '\t' );
		std::getline( f, sline, '\n' );
		word.from_string( sword );
		word2code[word] = next_code;
		code2ch[next_code] = sword;
		code2word[next_code] = word;
		freq[next_code] = std::atoi( sfreq.c_str() );
		next_code++;
	}
}
struct chunk_t {
	int code[3];
	chunk_t() : code() { }
	int length() const {
		int length = 0;
		for (int i = 0; i < 3; i++) {
			if (!code[i]) break;
			auto probe = code2word.find( code[i] );
			if (probe != code2word.end()) length += probe->second.length();
			else length += 1;
		}
		return length;
	}
	double avglength() const {
		double len = length();
		for (int i = 0; i < 3; i++) {
			if (!code[i]) {
				return len / i;
			}
		}
		return len / 3;
	}
	double varlength() const {
		double mean = avglength();
		double var = 0;
		double n = 0;
		for (int i = 0; i < 3; i++) {
			if (!code[i]) break;
			n += 1.0;
			int length;
			auto probe = code2word.find( code[i] );
			if (probe != code2word.end()) length = probe->second.length();
			else length = 1;
			var += ( length - mean ) * ( length - mean );
		}
		return var / n;
	}
	double freedeg() const {
		double deg = 0;
		for (int i = 0; i < 3; i++) {
			if (!code[i]) break;
			auto probe = freq.find( code[i] );
			if (probe != freq.end()) deg += log( ( double ) probe->second );
		}
		return deg;
	}
	int firstlength() const {
		auto probe = code2word.find( code[0] );
		if (probe != code2word.end()) return probe->second.length();
		else return 1;
	}
};
void find_chunk( int* ch, chunk_t basechunk, int baselen, std::vector<chunk_t>& chunks ) {
	if (baselen == 3) {
		chunks.push_back( basechunk );
		return;
	}
	chunk_t monochunk( basechunk );
	monochunk.code[baselen] = ch[0];
	find_chunk( ch + 1, monochunk, baselen + 1, chunks );
	for (int i = 2; i < 7; i++) {
		word_t word;
		for (int j = 0; j < 7; j++) {
			if (j < i) word.code[j] = ch[j];
			else word.code[j] = 0;
		}
		auto probe = word2code.find( word );
		if (probe == word2code.end()) continue;
		chunk_t newchunk( basechunk );
		newchunk.code[baselen] = probe->second;
		find_chunk( ch + i, newchunk, baselen + 1, chunks );
	}
}
std::vector<int> mmseg( std::vector<int>& cclst ) {
	std::vector<int> wclst;
	size_t i = 0;
	while (i < cclst.size()) {
		int ch[21];
		for (int x = 0; x < 21; x++) {
			try {
				ch[x] = cclst.at( i + x );
			} catch (const std::out_of_range& oor) {
				ch[x] = 0;
			}
		}
		std::vector<chunk_t> chunks;
		chunk_t nullchunk;
		find_chunk( ch, nullchunk, 0, chunks );

		std::sort( chunks.begin(), chunks.end(), []( const chunk_t& lhs, const chunk_t& rhs ) {return lhs.length() > rhs.length(); } );
		auto range = std::equal_range( chunks.begin(), chunks.end(), chunks[0], []( const chunk_t& lhs, const chunk_t& rhs ) {return lhs.length() == rhs.length(); } );
		chunks.erase( range.first, range.second );

		std::sort( chunks.begin(), chunks.end(), []( const chunk_t& lhs, const chunk_t& rhs ) {return lhs.avglength() > rhs.avglength(); } );
		range = std::equal_range( chunks.begin(), chunks.end(), chunks[0], []( const chunk_t& lhs, const chunk_t& rhs ) {return lhs.avglength() == rhs.avglength(); } );
		chunks.erase( range.first, range.second );

		std::sort( chunks.begin(), chunks.end(), []( const chunk_t& lhs, const chunk_t& rhs ) {return lhs.varlength() > rhs.varlength(); } );
		range = std::equal_range( chunks.begin(), chunks.end(), chunks[0], []( const chunk_t& lhs, const chunk_t& rhs ) {return lhs.varlength() == rhs.varlength(); } );
		chunks.erase( range.first, range.second );

		std::sort( chunks.begin(), chunks.end(), []( const chunk_t& lhs, const chunk_t& rhs ) {return lhs.freedeg() > rhs.freedeg(); } );

		int code = chunks[0].code[0];
		wclst.push_back( code );
		i += chunks[0].firstlength();
	}
	return wclst;
}
std::string diff( std::string sold, std::string snew ) {
	std::string out = "";
	auto oldlst = read_split( sold );
	auto newlst = read_split( snew );
	auto oldcclst = zhch_to_code( oldlst );
	auto newcclst = zhch_to_code( newlst );
	auto oldwclst = mmseg( oldcclst );
	auto newwclst = mmseg( newcclst );
	auto oldtocmplst = code_to_zhch( oldwclst );
	auto newtocmplst = code_to_zhch( newwclst );

	auto result = lcqcmp( oldtocmplst, newtocmplst );
	int score = 0;
	for (auto i = result.begin(); i != result.end(); i++) {
		std::string begin, end;
		switch (i->first) {
		case OP_INS:
			score++;
			begin = "[color=green]";
			end = "[/color]";
			break;
		case OP_DEL:
			score++;
			begin = "[del][color=red]";
			end = "[/color][/del]";
			break;
		default:
			begin = "";
			end = "";
			break;
		}
		if (i != result.begin() && ( i - 1 )->first == i->first) {
			begin = "";
		}
		if (( i + 1 ) != result.end() && ( i + 1 )->first == i->first) {
			end = "";
		}
		out.append( begin );
		out.append( i->second );
		out.append( end );
	}
	if (score * 10 >= result.size() * 8) {
		out.clear();
		out.append("[del][color=red]");
		out.append(sold);
		out.append("\n[/color][/del]");
		out.append("[color=green]");
		out.append(snew);
		out.append("[/color]");
	}
	return out;
}

using namespace sql;

struct build_t {
	int major;
	int minor;
	int rev;
	int build;
	bool operator<( const build_t& rhs ) const {
		if (major < rhs.major) return true;
		if (major > rhs.major) return false;
		if (minor < rhs.minor) return true;
		if (minor > rhs.minor) return false;
		if (rev < rhs.rev) return true;
		if (rev > rhs.rev) return false;
		return build < rhs.build;
	}
	std::string toStr() const {
		std::string str = "";
		char buf[32];
		str.append( _itoa( major, buf, 10 ) );
		str.append( "." );
		str.append( _itoa( minor, buf, 10 ) );
		str.append( "." );
		str.append( _itoa( rev, buf, 10 ) );
		str.append( "." );
		str.append( _itoa( build, buf, 10 ) );
		return str;
	}
	build_t() : major( 0 ), minor( 0 ), rev( 0 ), build( 0 ) { };
	build_t( const std::string& str ) {
		std::stringstream ss( str );
		std::string num;
		std::getline( ss, num, '.' );
		major = atoi( num.c_str() );
		std::getline( ss, num, '.' );
		minor = atoi( num.c_str() );
		std::getline( ss, num, '.' );
		rev = atoi( num.c_str() );
		std::getline( ss, num, '_' );
		build = atoi( num.c_str() );
	}
};
std::string empty_line(std::string in) {
	std::string out;
	int state = 0;
	for (auto i = in.begin(); i != in.end(); i++) {
		switch (state) {
		case 0:
			if (*i == '<') state = 2;
			else out.push_back( *i );
			break;
		case 2:
			if (*i == 'i' ) state = 100;
			else if (*i == 's') state = 100;
			else if (*i == 'b') state = 200;
			else if (*i == '/') state = 3;
			else {
				out.push_back( '<' );
				state = 0;
				i--;
			}
			break;
		case 3:
			if (*i == 'i' ) state = 100;
			else if (*i == 's') state = 100;
			else if (*i == 'b') state = 100;
			else {
				out.push_back( '<' );
				out.push_back( '/' );
				state = 0;
				i--;
			}
			break;
		case 100: 
			if (*i == '>') state = 0;
			break;
		case 200: 
			if (*i == '>') {
				out.push_back( '\r' );
				out.push_back( '\n' );
				state = 0;
			}
			break;
		}
	}
	in = out;
	out.clear();
	state = 0;
	for (auto i = in.begin(); i != in.end(); i++) {
		if (state) {
			if (*i == '\n' || *i == '\r') continue;
			else {
				state = 0;
				out.push_back( '\n' );
				out.push_back( *i );
			}
		} else {
			if (*i == '\n' || *i == '\r') state = 1;
			else out.push_back( *i );
		}

	}
	return out;
};
struct spell_tt_t {
	int spell_id;
	std::string name;
	std::string subname;
	std::string tooltips;
	bool operator<( const spell_tt_t& rhs ) const {
		return spell_id < rhs.spell_id;
	}
	std::string to_str() const {
		char buf[32];
		std::string str = "[wow,spell,";
		str.append( _itoa(spell_id, buf, 10) );
		str.append( ",cn[" );
		str.append( name );
		str.append( "]]" );
		if (!subname.empty()) {
			str.append("(");
			str.append( subname );
			str.append(")");
		}
		str.append( u8"：" );
		str.append( empty_line(tooltips) );
		str.append( "\n" );
		return str;
	}
	bool operator==( const spell_tt_t& rhs ) const {
		if (spell_id != rhs.spell_id) return false;
		if (to_str().compare( rhs.to_str() )) return false;
		return true;
	}
	std::string operator^( const spell_tt_t& rhs ) const {
		char buf[32];
		std::string d = "";
		if ( name.compare(rhs.name) ) {
			d += "[del][color=red][b]";
			d += name;
			d += "[/b][/color][/del]";
		}
		d += "[wow,spell,";
		d.append( _itoa(spell_id, buf, 10) );
		d += ",cn[";
		d += rhs.name;
		d += "]]";
		if ( !subname.empty() && subname.compare(rhs.subname) ) {
			d += "[del][color=red](";
			d += subname;
			d += ")[/color][/del]";
		}
		if ( !rhs.subname.empty() && subname.compare(rhs.subname) ) {
			d += "[color=green](";
			d += rhs.subname;
			d += ")[/color]";
		}
		if ( !subname.empty() && !subname.compare(rhs.subname) ) {
			d += "(";
			d += subname;
			d += ")";
		}
		d += u8"：";
		d += diff( empty_line(tooltips), empty_line(rhs.tooltips) );
		d += "\n";
		return d;
	}
};
struct spec_tt_t {
	int spec_id;
	std::string spec_name;
	std::vector<spell_tt_t> spells;
	std::vector<spell_tt_t> talents;
	std::vector<spell_tt_t> pvptalents;
	bool operator<( const spec_tt_t& rhs ) const {
		return spec_id < rhs.spec_id;
	}
};
struct class_tt_t {
	int class_id;
	std::string class_name;
	std::string en_class_name;
	std::vector<spell_tt_t> spells;
	std::vector<spell_tt_t> talents;
	std::vector<spell_tt_t> pvptalents;
	std::vector<spec_tt_t> specs;
	bool operator<( const class_tt_t& rhs ) const {
		return en_class_name < rhs.en_class_name;
	}
};
struct build_tt_t {
	std::vector<class_tt_t> c;
};


void get_build_tooltips( Connection *con, build_t build, build_tt_t& tt ) {
	std::string query;
	char buf[32];
	try {
		con->setSchema( build.toStr() + "_zhCN" );
		Statement *stmt;
		stmt = con->createStatement(); std::cerr << "SELECT m_ID, m_className, m_ShortName FROM dbc_ChrClasses" << std::endl;
		ResultSet *res_class = stmt->executeQuery( "SELECT m_ID, m_className, m_ShortName FROM dbc_ChrClasses" );

		auto query_spell = []( ResultSet* res, int id ) {
			spell_tt_t spell;
			spell.spell_id = res->getInt( "m_ID" );
			spell.name = res->getString( "m_name_lang" );
			spell.subname = res->getString( "m_nameSubtext_lang" );
			spell.tooltips = res->getString( "m_description_lang" );
			//if (spell.tooltips.length() > 1) spell.tooltips.append( "\n" );
			//spell.tooltips.append( res->getString( "m_auraDescription_lang" ) );
			return spell;
		};
		while (res_class->next()) {
			class_tt_t c;
			std::string class_name = res_class->getString( "m_className" );
			int class_id = res_class->getInt( "m_ID" );
			int class_mask = 1 << ( class_id - 1 );
			c.class_id = class_id;
			c.class_name = class_name;
			c.en_class_name = res_class->getString( "m_ShortName" );
			query = "SELECT rep_Spell.m_ID, rep_Spell.m_name_lang, rep_Spell.m_nameSubtext_lang, rep_Spell.m_description_lang, rep_Spell.m_auraDescription_lang FROM rep_Spell, dbc_PvpTalent WHERE rep_Spell.m_ID = dbc_PvpTalent.m_spellID AND dbc_PvpTalent.m_classID = ";
			query.append( _itoa( class_id, buf, 10 ) );
			query.append( " AND dbc_PvpTalent.m_specID = 0" );
			stmt = con->createStatement(); std::cerr << query << std::endl;
			ResultSet *res_pvptalent = stmt->executeQuery( query );
			while (res_pvptalent->next()) {
				int spell_id = res_pvptalent->getInt( "m_ID" );
				spell_tt_t spell = query_spell( res_pvptalent, spell_id );
				c.pvptalents.push_back( spell );
			}
			std::sort( c.pvptalents.begin(), c.pvptalents.end() );
			query = "SELECT rep_Spell.m_ID, rep_Spell.m_name_lang, rep_Spell.m_nameSubtext_lang, rep_Spell.m_description_lang, rep_Spell.m_auraDescription_lang FROM rep_Spell, dbc_Talent WHERE rep_Spell.m_ID = dbc_Talent.m_spellID AND dbc_Talent.m_classID = ";
			query.append( _itoa( class_id, buf, 10 ) );
			query.append( " AND dbc_Talent.m_SpecID = 0" );
			stmt = con->createStatement(); std::cerr << query << std::endl;
			ResultSet *res_talent = stmt->executeQuery( query );
			while (res_talent->next()) {
				int spell_id = res_talent->getInt( "m_ID" );
				spell_tt_t spell = query_spell( res_talent, spell_id );
				c.talents.push_back( spell );
			}
			std::sort( c.talents.begin(), c.talents.end() );
			query = "SELECT rep_Spell.m_ID, rep_Spell.m_name_lang, rep_Spell.m_nameSubtext_lang, rep_Spell.m_description_lang, rep_Spell.m_auraDescription_lang FROM rep_Spell, dbc_SkillLineAbility WHERE rep_Spell.m_ID = dbc_SkillLineAbility.m_spellID AND dbc_SkillLineAbility.m_acquireMethod = 2 AND dbc_SkillLineAbility.m_reqChrClasses = ";
			query.append( _itoa( class_mask, buf, 10 ) );
			stmt = con->createStatement(); std::cerr << query << std::endl;
			ResultSet *res_ability = stmt->executeQuery( query );
			while (res_ability->next()) {
				int spell_id = res_ability->getInt( "m_ID" );
				spell_tt_t spell = query_spell( res_ability, spell_id );
				c.spells.push_back( spell );
			}
			std::sort( c.spells.begin(), c.spells.end() );
			query = "SELECT m_ID, m_classname1 FROM dbc_ChrSpecialization WHERE m_classID = ";
			query.append( _itoa( class_id, buf, 10 ) );
			stmt = con->createStatement(); std::cerr << query << std::endl;
			ResultSet *res_spec = stmt->executeQuery( query );
			while (res_spec->next()) {
				int spec_id = res_spec->getInt( "m_ID" );
				std::string spec_name = res_spec->getString( "m_classname1" );
				spec_tt_t spec;
				spec.spec_id = spec_id;
				spec.spec_name = spec_name;
				query = "SELECT rep_Spell.m_ID, rep_Spell.m_name_lang, rep_Spell.m_nameSubtext_lang, rep_Spell.m_description_lang, rep_Spell.m_auraDescription_lang FROM rep_Spell, dbc_PvpTalent WHERE rep_Spell.m_ID = dbc_PvpTalent.m_spellID AND m_specID = ";
				query.append( _itoa( spec_id, buf, 10 ) );
				stmt = con->createStatement(); std::cerr << query << std::endl;
				ResultSet *res_pvptalent = stmt->executeQuery( query );
				while (res_pvptalent->next()) {
					int spell_id = res_pvptalent->getInt( "m_ID" );
					spell_tt_t spell = query_spell( res_pvptalent, spell_id );
					spec.pvptalents.push_back( spell );
				}
				std::sort( spec.pvptalents.begin(), spec.pvptalents.end() );
				query = "SELECT rep_Spell.m_ID, rep_Spell.m_name_lang, rep_Spell.m_nameSubtext_lang, rep_Spell.m_description_lang, rep_Spell.m_auraDescription_lang FROM rep_Spell, dbc_Talent WHERE rep_Spell.m_ID = dbc_Talent.m_spellID AND m_SpecID = ";
				query.append( _itoa( spec_id, buf, 10 ) );
				stmt = con->createStatement(); std::cerr << query << std::endl;
				ResultSet *res_talent = stmt->executeQuery( query );
				while (res_talent->next()) {
					int spell_id = res_talent->getInt( "m_ID" );
					spell_tt_t spell = query_spell( res_talent, spell_id );
					spec.talents.push_back( spell );
				}
				std::sort( spec.talents.begin(), spec.talents.end() );
				query = "SELECT rep_Spell.m_ID, rep_Spell.m_name_lang, rep_Spell.m_nameSubtext_lang, rep_Spell.m_description_lang, rep_Spell.m_auraDescription_lang FROM rep_Spell, dbc_SpecializationSpells WHERE rep_Spell.m_ID = dbc_SpecializationSpells.m_spellID AND m_specID = ";
				query.append( _itoa( spec_id, buf, 10 ) );
				stmt = con->createStatement(); std::cerr << query << std::endl;
				ResultSet *res_spells = stmt->executeQuery( query );
				while (res_spells->next()) {
					int spell_id = res_spells->getInt( "m_ID" );
					spell_tt_t spell = query_spell( res_spells, spell_id );
					spec.spells.push_back( spell );
				}
				std::sort( spec.spells.begin(), spec.spells.end() );
				c.specs.push_back( spec );
			}
			std::sort( c.specs.begin(), c.specs.end() );
			tt.c.push_back( c );
		}
		std::sort( tt.c.begin(), tt.c.end() );
	} catch (SQLException &e) {
		std::cerr << "ERROR: SQLException in " << __FILE__;
		std::cerr << " (" << __func__ << ") on line " << __LINE__ << std::endl;
		std::cerr << "ERROR: " << e.what();
		std::cerr << " (MySQL error code: " << e.getErrorCode();
		std::cerr << ", SQLState: " << e.getSQLState() << ")" << std::endl;

		if (e.getErrorCode() == 1047) {
			/*
			Error: 1047 SQLSTATE: 08S01 (ER_UNKNOWN_COM_ERROR)
			Message: Unknown command
			*/
			std::cerr << "\nYour server does not seem to support Prepared Statements at all. ";
			std::cerr << "Perhaps MYSQL < 4.1?" << std::endl;
		}

		exit( EXIT_FAILURE );
	} catch (std::runtime_error &e) {

		std::cerr << "ERROR: runtime_error in " << __FILE__;
		std::cerr << " (" << __func__ << ") on line " << __LINE__ << std::endl;
		std::cerr << "ERROR: " << e.what() << std::endl;

		exit( EXIT_FAILURE );
	}
}

int main( int argc, char** argv ) {
	read_dict();

	std::string user( argc > 1 ? argv[1] : "root" );
	std::string password( argc > 2 ? argv[2] : "root" );
	std::string outpath( argc > 3 ? argv[3] : "" );

	Driver *driver;
	Connection *con;

	driver = get_driver_instance();
	con = driver->connect( "123.206.68.214", user.c_str(), password.c_str() );
	con->setAutoCommit( 0 );
	std::cerr << "\nDatabase connection\'s autocommit mode = " << con->getAutoCommit() << std::endl;

	DatabaseMetaData *dbcon_meta = con->getMetaData();
	std::auto_ptr < ResultSet > rs( dbcon_meta->getSchemas() );
	std::vector<std::string> schem_list;
	std::vector<build_t> zhcn_build_list;
	while (rs->next()) {
		schem_list.push_back( rs->getString( "TABLE_SCHEM" ) );
	}
	for (auto i = schem_list.begin(); i != schem_list.end(); i++) {
		if (i->find( "_zhCN" ) == std::string::npos) continue;
		zhcn_build_list.push_back( build_t( *i ) );
	}
	std::sort( zhcn_build_list.begin(), zhcn_build_list.end() );
	build_t new_build = zhcn_build_list.rbegin()[0];
	build_t old_build = zhcn_build_list.rbegin()[1];
	build_tt_t old_tt, new_tt;
	FILE* f = fopen( outpath.c_str(), "wb" );
	if ( !f ) f = stdout;

	get_build_tooltips( con, old_build, old_tt );
	get_build_tooltips( con, new_build, new_tt );
	fprintf( f, "%s - %s " u8"技能文本改动\n", old_build.toStr().c_str(), new_build.toStr().c_str() );

	auto vector_diff = []( std::vector<spell_tt_t>& vold, std::vector<spell_tt_t>& vnew, std::vector<std::string>& vdiff ) {
		vdiff.clear();
		auto i = vold.begin();
		auto j = vnew.begin();
		while (i != vold.end() || j != vnew.end()) {
			if (i != vold.end() && j != vnew.end()) {
				if (*i == *j) {
					i++;
					j++;
				} else if (*i < *j) {
					std::string d( u8"移除技能[del][color=red]" );
					d.append( i->to_str() );
					d.append( "[/color][/del]" );
					vdiff.push_back( d );
					i++;
				} else if (*j < *i) {
					std::string d( u8"新技能[color=green]" );
					d.append( j->to_str() );
					d.append( "[/color]" );
					vdiff.push_back( d );
					j++;
				} else {
					vdiff.push_back( *i ^ *j );
					i++;
					j++;
				}
			} else if (i != vold.end()) {
				std::string d( u8"移除技能[del][color=red]" );
				d.append( i->to_str() );
				d.append( "[/color][/del]" );
				vdiff.push_back( d );
				i++;
			} else {
				std::string d( u8"新技能[color=green]" );
				d.append( j->to_str() );
				d.append( "[/color]" );
				vdiff.push_back( d );
				j++;
			}
		}
	};
	std::vector<std::string> vdiff;
	for (int c = 0; c < 12; c++) {
		std::string class_header = "[h]";
		class_header.append( new_tt.c[c].class_name );
		class_header.append( "[/h][list]" );
		std::string class_tail = "[/list]";
		std::string universal;
		vector_diff( old_tt.c[c].spells, new_tt.c[c].spells, vdiff );
		if (!vdiff.empty()) {
			fprintf( f, "%s", class_header.c_str() );
			class_header.clear();
			fprintf( f, u8"[*]通用技能[list]" );
			for (auto i = vdiff.begin(); i != vdiff.end(); i++) {
				fprintf( f, "[*]%s", i->c_str() );
			}
			fprintf( f, "[/list]" );
		}
		vector_diff( old_tt.c[c].talents, new_tt.c[c].talents, vdiff );
		if (!vdiff.empty()) {
			fprintf( f, "%s", class_header.c_str() );
			class_header.clear();
			fprintf( f, u8"[*]天赋[list]" );
			for (auto i = vdiff.begin(); i != vdiff.end(); i++) {
				fprintf( f, "[*]%s", i->c_str() );
			}
			fprintf( f, "[/list]" );
		}
		vector_diff( old_tt.c[c].pvptalents, new_tt.c[c].pvptalents, vdiff );
		if (!vdiff.empty()) {
			fprintf( f, "%s", class_header.c_str() );
			class_header.clear();
			fprintf( f, u8"[*]PvP天赋[list]" );
			for (auto i = vdiff.begin(); i != vdiff.end(); i++) {
				fprintf( f, "[*]%s", i->c_str() );
			}
			fprintf( f, "[/list]" );
		}
		for (int s = 0; s < new_tt.c[c].specs.size(); s++) {
			std::string spec_header = "[*]";
			spec_header.append( new_tt.c[c].specs[s].spec_name );
			spec_header.append( "[list]" );
			std::string spec_tail = "[/list]";
			vector_diff( old_tt.c[c].specs[s].spells, new_tt.c[c].specs[s].spells, vdiff );
			if (!vdiff.empty()) {
				fprintf( f, "%s", class_header.c_str() );
				class_header.clear();
				fprintf( f, "%s", spec_header.c_str() );
				spec_header.clear();
				fprintf( f, u8"[*]专精技能[list]" );
				for (auto i = vdiff.begin(); i != vdiff.end(); i++) {
					fprintf( f, "[*]%s", i->c_str() );
				}
				fprintf( f, "[/list]" );
			}
			vector_diff( old_tt.c[c].specs[s].talents, new_tt.c[c].specs[s].talents, vdiff );
			if (!vdiff.empty()) {
				fprintf( f, "%s", class_header.c_str() );
				class_header.clear();
				fprintf( f, "%s", spec_header.c_str() );
				spec_header.clear();
				fprintf( f, u8"[*]天赋[list]" );
				for (auto i = vdiff.begin(); i != vdiff.end(); i++) {
					fprintf( f, "[*]%s", i->c_str() );
				}
				fprintf( f, "[/list]" );
			}
			vector_diff( old_tt.c[c].specs[s].pvptalents, new_tt.c[c].specs[s].pvptalents, vdiff );
			if (!vdiff.empty()) {
				fprintf( f, "%s", class_header.c_str() );
				class_header.clear();
				fprintf( f, "%s", spec_header.c_str() );
				spec_header.clear();
				fprintf( f, u8"[*]PvP天赋[list]" );
				for (auto i = vdiff.begin(); i != vdiff.end(); i++) {
					fprintf( f, "[*]%s", i->c_str() );
				}
				fprintf( f, "[/list]" );
			}
			if (spec_header.empty()) {
				fprintf( f, "%s", spec_tail.c_str() );
			}
		}
		if (class_header.empty()) {
			fprintf( f, "%s", class_tail.c_str() );
		}
	}
	// fclose( f );
	return EXIT_SUCCESS;
}

