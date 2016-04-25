/*
	dbc to corpus extractor.
	Aeandarian(a.k.a. fhsvengetta) 2016.3.24
*/
#define _CRT_SECURE_NO_WARNINGS
#pragma pack(1)

#include "CascLib.h"
#include <cstdio>
#include <vector>
#include <utility>
#include <cmath>
#include <algorithm>
#include <functional>

struct uint24_t {
	uint8_t b[3];
	operator uint32_t() const {
		uint32_t u = 0;
		memcpy(&u, b, 3);
		return u;
	}
};

struct record_t {
	void inline_str_process( void* data, size_t length, size_t size_of_this ) {
		if (length >= size_of_this) {
			memcpy( this, data, size_of_this );
		}
	}
};

struct dummy_record_t : public record_t {
	UINT8 dummy;
};
struct spell_rec_t : public record_t {
	uint32_t name;
	uint32_t rank;
	uint32_t desc;
	uint32_t tt;
	uint32_t id_misc;
	uint24_t id;
	uint16_t id_desc_var;
};
struct item_sparse_record_t {
	INT32   dc0[3];
	float   dc1;
	float   dc2;
	UINT32  dc3;
	UINT32  dc4;
	INT32   dc5;
	INT32   dc6;
	UINT32  dc7;
	INT32   dc8;
	INT32   dc9;
	INT32   dc10[10];
	float   dc11[10];
	float   dc12; // 132 bytes
	std::string  name;
	std::string  desc; // variable length
	INT32   dc67;
	INT32   dc68;
	INT32   dc69;
	UINT32  dc18;
	float   dc19;
	UINT32  dc20;
	float   dc21;
	UINT16  dc22;
	UINT16  dc23;
	UINT16  dc24;
	UINT16  dc25;
	INT16   dc26[10];
	UINT16  dc27;
	UINT16  dc28;
	UINT16  dc29;
	UINT16  dc30;
	UINT16  dc31;
	UINT16  dc32;
	UINT16  dc33;
	UINT16  dc34;
	UINT16  dc35;
	UINT16  dc36;
	UINT16  gem_props;
	UINT16  dc37;
	// gem props was here.
	UINT16  dc39;
	UINT16  dc40;
	UINT16  dc41;
	UINT8   dc42;
	UINT8   dc43;
	UINT8   dc44;
	INT8    dc45;
	UINT8   dc46;
	UINT8   dc47;
	UINT8   dc48;
	UINT8   dc49;
	INT8    dc50[10];
	UINT8   dc51;
	UINT8   dc52;
	UINT8   dc53;
	UINT8   dc54;
	UINT8   dc55;
	UINT8   dc56;
	UINT8   dc57;
	UINT8   dc58[3];
	UINT8   dc59;
	UINT8   dc60;
	UINT8   dc61;
	UINT8   dc62;
	//UINT8   dc63;
	//UINT8   dc64;
	void inline_str_process( void* data, size_t length, size_t size_of_this ) {
		size_t invariant1 = 132;
		size_t invariant2 = sizeof( *this ) - invariant1 - 2 * sizeof( std::string );

		memcpy( this, data, invariant1 );
		memcpy( &dc67, ( char* ) data + length - invariant2, invariant2 );

		char* q = ( char* ) data + invariant1;
		while (*q) {
			name.push_back( *q++ );
		}
		q += 4;
		while (*q) {
			desc.push_back( *q++ );
		}
	}
};

template <typename T>
int wdbc_reader( HANDLE file, std::vector<std::pair<UINT32, T> >& records, std::vector<char>& string_block ) {
	struct {
		UINT32 record_count;
		UINT32 field_count;
		UINT32 record_size;
		UINT32 string_block_size;
	} header;
	void* discard;
	DWORD read;
	if (!CascReadFile( file, &header, sizeof( header ), &read )) {
		printf( "failed to read dbc header\n" );
		return 0;
	}
	if (read != sizeof( header )) {
		printf( "dbc header broken\n" );
		return 0;
	}
	if (header.record_size < sizeof( T )) {
		printf( "record size is lesser than expect\n" );
		return 0;
	}
	if (header.record_size > sizeof( T )) {
		printf( "record size is greater than expect\n" );
		// this is a warning. resume.
		discard = alloca( header.record_size - sizeof( T ) );
	}
	for (UINT32 i = 0; i < header.record_count; i++) {
		T new_record;
		if (!CascReadFile( file, &new_record, sizeof( T ), &read )) {
			printf( "failed to read dbc record\n" );
			return 0;
		}
		if (read != sizeof( T )) {
			printf( "dbc record broken\n" );
			return 0;
		}
		records.push_back( std::make_pair( i, new_record ) );
		if (header.record_size > sizeof( T )) {
			if (!CascReadFile( file, discard, header.record_size - sizeof( T ), &read )) {
				printf( "failed to read dbc record (padding)\n" );
				return 0;
			}
			if (read != header.record_size - sizeof( T )) {
				printf( "dbc record (padding) broken\n" );
				return 0;
			}
		}
	}
	string_block.resize( header.string_block_size );
	if (!CascReadFile( file, &string_block[0], header.string_block_size, &read )) {
		printf( "failed to read dbc string block\n" );
		return 0;
	}
	if (read != header.string_block_size) {
		printf( "dbc string block broken\n" );
		return 0;
	}
	return 1;
}

template <typename T>
int wdb4_reader( HANDLE file, std::vector<std::pair<UINT32, T> >& records, std::vector<char>& string_block ) {
	struct {
		UINT32 record_count;
		UINT32 field_count;
		UINT32 record_size;
		UINT32 string_block_size;
		UINT32 table_hash;
		UINT32 build;
		UINT32 timestamp_last_written;
		UINT32 min_id;
		UINT32 max_id;
		UINT32 locale;
		UINT32 copy_table_size;
		UINT32 flags;
	} header;
	void* discard;
	DWORD read;
	if (!CascReadFile( file, &header, sizeof( header ), &read )) {
		printf( "failed to read dbc header\n" );
		return 0;
	}
	if (read != sizeof( header )) {
		printf( "dbc header broken\n" );
		return 0;
	}
	if (header.flags & 0x01) {
		struct offset_map_entry_t {
			UINT32 offset;
			UINT16 length;
		};
		std::vector<offset_map_entry_t> offset_map;
		CascSetFilePointer( file, header.string_block_size, 0, FILE_BEGIN );
		for (UINT32 i = header.min_id; i <= header.max_id; i++) {
			offset_map_entry_t o;
			if (!CascReadFile( file, &o, sizeof( offset_map_entry_t ), &read )) {
				printf( "failed to read dbc offset map\n" );
				return 0;
			}
			if (read != sizeof( offset_map_entry_t )) {
				printf( "dbc offset map broken\n" );
				return 0;
			}
			offset_map.push_back( o );
		}
		records.resize( header.record_count );
		if (header.flags & 0x04) {
			for (UINT32 i = 0; i < header.record_count; i++) {
				UINT32 id;
				if (!CascReadFile( file, &id, sizeof( UINT32 ), &read )) {
					printf( "failed to read dbc explicit id\n" );
					return 0;
				}
				if (read != sizeof( UINT32 )) {
					printf( "dbc explicit id broken\n" );
					return 0;
				}
				records[i].first = id;
			}
		}
		for (auto i = records.begin(); i != records.end(); i++) {
			T new_record;
			std::string buf;
			buf.resize( offset_map[i->first - header.min_id].length );
			CascSetFilePointer( file, offset_map[i->first - header.min_id].offset, 0, FILE_BEGIN );
			if (!CascReadFile( file, &buf[0], buf.size(), &read )) {
				printf( "failed to read dbc record\n" );
				return 0;
			}
			if (read != buf.size()) {
				printf( "dbc record broken\n" );
				return 0;
			}
			new_record.typename T::inline_str_process( &buf[0], buf.size(), sizeof( T ) );
			i->second = new_record;
		}
		CascSetFilePointer( file, header.string_block_size + offset_map.size() * sizeof( offset_map_entry_t ) + header.record_count * sizeof( UINT32 ), 0, FILE_BEGIN );
	} else {
		if (header.record_size < sizeof( T )) {
			printf( "record size is lesser than expect\n" );
			return 0;
		}
		if (header.record_size > sizeof( T )) {
			printf( "record size is greater than expect, %d %d\n", sizeof( T ), header.record_size );
			// this is a warning. resume.
			discard = alloca( header.record_size - sizeof( T ) );
		}
		for (UINT32 i = 0; i < header.record_count; i++) {
			T new_record;
			if (!CascReadFile( file, &new_record, sizeof( T ), &read )) {
				printf( "failed to read dbc record\n" );
				return 0;
			}
			if (read != sizeof( T )) {
				printf( "dbc record broken\n" );
				return 0;
			}
			records.push_back( std::make_pair( i, new_record ) );
			if (header.record_size > sizeof( T )) {
				if (!CascReadFile( file, discard, header.record_size - sizeof( T ), &read )) {
					printf( "failed to read dbc record (padding)\n" );
					return 0;
				}
				if (read != header.record_size - sizeof( T )) {
					printf( "dbc record (padding) broken\n" );
					return 0;
				}
			}
		}
		string_block.resize( header.string_block_size );
		if (!CascReadFile( file, string_block.data(), header.string_block_size, &read )) {
			printf( "failed to read dbc string block\n" );
			return 0;
		}
		if (read != header.string_block_size) {
			printf( "dbc string block broken %d %d\n", read, header.string_block_size );
			return 0;
		}
		if (header.flags & 0x04) {
			for (UINT32 i = 0; i < header.record_count; i++) {
				UINT32 id;
				if (!CascReadFile( file, &id, sizeof( UINT32 ), &read )) {
					printf( "failed to read dbc explicit id\n" );
					return 0;
				}
				if (read != sizeof( UINT32 )) {
					printf( "dbc explicit id broken\n" );
					return 0;
				}
				records[i].first = id;
			}
		}
	}
	if (header.copy_table_size > 0) {
		struct copy_table_entry_t {
			UINT32 id_of_new_row;
			UINT32 id_of_copied_row;
		};
		copy_table_entry_t* copy_table = ( copy_table_entry_t* ) alloca( header.copy_table_size );
		if (!CascReadFile( file, copy_table, header.copy_table_size, &read )) {
			printf( "failed to read dbc copy table\n" );
			return 0;
		}
		if (read != header.copy_table_size) {
			printf( "dbc copy table broken\n" );
			return 0;
		}
		for (UINT32 i = 0; i < header.copy_table_size / sizeof( copy_table_entry_t ); i++) {
			for (auto rec = records.begin(); rec != records.end(); rec++) {
				if (rec->first == copy_table[i].id_of_copied_row) {
					T copied = rec->second;
					records.push_back( std::make_pair( copy_table[i].id_of_new_row, copied ) );
					break;
				}
			}
		}
	}
	return 1;
}
template <typename T>
int wdb5_reader( HANDLE file, std::vector<std::pair<UINT32, T> >& records, std::vector<char>& string_block ) {
	struct {
		UINT32 record_count;
		UINT32 field_count;
		UINT32 record_size;
		UINT32 string_block_size;
		UINT32 table_hash;
		UINT32 build;
		UINT32 min_id;
		UINT32 max_id;
		UINT32 locale;
		UINT32 copy_table_size;
		UINT32 flags;
	} header;
	void* discard;
	DWORD read;
	if (!CascReadFile( file, &header, sizeof( header ), &read )) {
		printf( "failed to read dbc header\n" );
		return 0;
	}
	if (read != sizeof( header )) {
		printf( "dbc header broken\n" );
		return 0;
	}
	for (int i = 0; i < header.field_count; i++) {
		struct field_info_t {
			UINT16 type;
			UINT16 pos;
		} field_info;
		if (!CascReadFile( file, &field_info, sizeof( field_info_t ), &read )) {
			printf( "failed to read dbc field info\n" );
			return 0;
		}
		if (read != sizeof( field_info_t )) {
			printf( "dbc field info broken\n" );
			return 0;
		}
		printf( "\tfield %d: type %d, pos %d\n", i + 1, field_info.type, field_info.pos );
	}
	printf( "%d, %d\n", sizeof( T ), sizeof( std::string ) );
	if (header.flags & 0x01) {
		struct offset_map_entry_t {
			UINT32 offset;
			UINT16 length;
		};
		std::vector<offset_map_entry_t> offset_map;
		CascSetFilePointer( file, header.string_block_size, 0, FILE_BEGIN );
		for (UINT32 i = header.min_id; i <= header.max_id; i++) {
			offset_map_entry_t o;
			if (!CascReadFile( file, &o, sizeof( offset_map_entry_t ), &read )) {
				printf( "failed to read dbc offset map\n" );
				return 0;
			}
			if (read != sizeof( offset_map_entry_t )) {
				printf( "dbc offset map broken\n" );
				return 0;
			}
			offset_map.push_back( o );
		}
		records.resize( header.record_count );
		if (header.flags & 0x04) {
			for (UINT32 i = 0; i < header.record_count; i++) {
				UINT32 id;
				if (!CascReadFile( file, &id, sizeof( UINT32 ), &read )) {
					printf( "failed to read dbc explicit id\n" );
					return 0;
				}
				if (read != sizeof( UINT32 )) {
					printf( "dbc explicit id broken\n" );
					return 0;
				}
				records[i].first = id;
			}
		}
		for (auto i = records.begin(); i != records.end(); i++) {
			T new_record;
			std::string buf;
			buf.resize( offset_map[i->first - header.min_id].length );
			CascSetFilePointer( file, offset_map[i->first - header.min_id].offset, 0, FILE_BEGIN );
			if (!CascReadFile( file, &buf[0], buf.size(), &read )) {
				printf( "failed to read dbc record\n" );
				return 0;
			}
			if (read != buf.size()) {
				printf( "dbc record broken\n" );
				return 0;
			}
			new_record.typename T::inline_str_process( &buf[0], buf.size(), sizeof( T ) );
			i->second = new_record;
		}
		CascSetFilePointer( file, header.string_block_size + offset_map.size() * sizeof( offset_map_entry_t ) + header.record_count * sizeof( UINT32 ), 0, FILE_BEGIN );
	} else {
		if (header.record_size < sizeof( T )) {
			printf( "record size is lesser than expect\n" );
			return 0;
		}
		if (header.record_size > sizeof( T )) {
			printf( "record size is greater than expect, %d %d\n", sizeof( T ), header.record_size );
			// this is a warning. resume.
			discard = alloca( header.record_size - sizeof( T ) );
		}
		for (UINT32 i = 0; i < header.record_count; i++) {
			T new_record;
			if (!CascReadFile( file, &new_record, sizeof( T ), &read )) {
				printf( "failed to read dbc record\n" );
				return 0;
			}
			if (read != sizeof( T )) {
				printf( "dbc record broken\n" );
				return 0;
			}
			records.push_back( std::make_pair( i, new_record ) );
			if (header.record_size > sizeof( T )) {
				if (!CascReadFile( file, discard, header.record_size - sizeof( T ), &read )) {
					printf( "failed to read dbc record (padding)\n" );
					return 0;
				}
				if (read != header.record_size - sizeof( T )) {
					printf( "dbc record (padding) broken\n" );
					return 0;
				}
			}
		}
		string_block.resize( header.string_block_size );
		if (!CascReadFile( file, string_block.data(), header.string_block_size, &read )) {
			printf( "failed to read dbc string block\n" );
			return 0;
		}
		if (read != header.string_block_size) {
			printf( "dbc string block broken %d %d\n", read, header.string_block_size );
			return 0;
		}
		if (header.flags & 0x04) {
			for (UINT32 i = 0; i < header.record_count; i++) {
				UINT32 id;
				if (!CascReadFile( file, &id, sizeof( UINT32 ), &read )) {
					printf( "failed to read dbc explicit id\n" );
					return 0;
				}
				if (read != sizeof( UINT32 )) {
					printf( "dbc explicit id broken\n" );
					return 0;
				}
				records[i].first = id;
			}
		}
	}
	if (header.copy_table_size > 0) {
		struct copy_table_entry_t {
			UINT32 id_of_new_row;
			UINT32 id_of_copied_row;
		};
		copy_table_entry_t* copy_table = ( copy_table_entry_t* ) alloca( header.copy_table_size );
		if (!CascReadFile( file, copy_table, header.copy_table_size, &read )) {
			printf( "failed to read dbc copy table\n" );
			return 0;
		}
		if (read != header.copy_table_size) {
			printf( "dbc copy table broken\n" );
			return 0;
		}
		for (UINT32 i = 0; i < header.copy_table_size / sizeof( copy_table_entry_t ); i++) {
			for (auto rec = records.begin(); rec != records.end(); rec++) {
				if (rec->first == copy_table[i].id_of_copied_row) {
					T copied = rec->second;
					records.push_back( std::make_pair( copy_table[i].id_of_new_row, copied ) );
					break;
				}
			}
		}
	}
	return 1;
}
template <typename T>
int dbc_reader( HANDLE storage, const char* dbc_name, std::vector<std::pair<UINT32, T> >& records, std::vector<char>& string_block ) {
	UINT32 magic;
	HANDLE file;
	DWORD read;
	int ok;
	printf( "%s\n", dbc_name );
	if (!CascOpenFile( storage, dbc_name, CASC_LOCALE_ALL, 0, &file )) {
		printf( "failed to open dbc %s, error %d\n", dbc_name, GetLastError() );
		return 0;
	}
	/*{
		DWORD size = CascGetFileSize( file, 0 );
		char* buf = ( char* ) malloc( size );
		CascReadFile( file, buf, size, &read );
		FILE* f = fopen( dbc_name, "wb" );
		fwrite( buf, 1, read, f );
		fclose( f );
		CascSetFilePointer( file, 0, 0, FILE_BEGIN );
	}*/
	if (!CascReadFile( file, &magic, sizeof( magic ), &read )) {
		printf( "failed to read dbc magic %s\n", dbc_name );
		return 0;
	}
	if (read != sizeof( magic )) {
		printf( "dbc magic broken %s\n", dbc_name );
		return 0;
	}
	if (magic == ( ( ( UINT32 )'W' ) | ( ( UINT32 )'D' << 8 ) | ( ( UINT32 )'B' << 16 ) | ( ( UINT32 )'C' << 24 ) )) {
		ok = wdbc_reader( file, records, string_block );
	} else if (magic == ( ( ( UINT32 )'W' ) | ( ( UINT32 )'D' << 8 ) | ( ( UINT32 )'B' << 16 ) | ( ( UINT32 )'4' << 24 ) )) {
		ok = wdb4_reader( file, records, string_block );
	} else if (magic == ( ( ( UINT32 )'W' ) | ( ( UINT32 )'D' << 8 ) | ( ( UINT32 )'B' << 16 ) | ( ( UINT32 )'5' << 24 ) )) {
		ok = wdb5_reader( file, records, string_block );
	} else {
		printf( "wrong format specified, this file is %c%c%c%c, not WDBC, WDB4 or WDB5\n",
			( char ) magic, ( char ) ( magic >> 8 ), ( char ) ( magic >> 16 ), ( char ) ( magic >> 24 ) );
		return 0;
	}
	CascCloseFile( file );
	if (ok) printf( "OK, %d records read\n", records.size() );
	return ok;
}
int is_zhpunc( std::string zhch ) {
	const char* punc_list[] = {
		u8"，",
		u8"。",
		u8"…",
		u8"—",
		u8"”",
		u8"“",
		u8"！",
		u8"？",
		u8"【",
		u8"】",
		u8"『",
		u8"』",
		u8"《",
		u8"》",
		u8"（",
		u8"）",
		u8"￥",
		u8"：",
		u8"；",
		u8"‘",
		u8"’",
		u8"、",
		0
	};
	for (int i = 0; punc_list[i]; i++) {
		if (0 == zhch.compare( punc_list[i] )) return 1;
	}
	return 0;
}
void process_ch( char ch, FILE* f, int& space ) {
	static std::string mem;
	if (ch & 0x80) {
		mem.push_back( ch );
		if (mem.length() == 3) {
			if (is_zhpunc( mem )) {
				// space = 1;
				mem.clear();
			} else {
				if (space) fputc( '\n', f );
				space = 0;
				fputc( mem[0], f );
				mem.erase( 0, 1 );
			}
		}
	} else {
		fprintf( f, "%s", mem.c_str() );
		mem.clear();
		space = 1;
	}
}

//std::vector<char> spell_string_block;
//std::vector<std::pair<UINT32, spell_rec_t> > spell_records;
//std::string func_spell_desc( int id );

int _tmain( int argc, TCHAR* argv[] ) {
	if (argc < 2) {
		printf( "need wow data directory.\n" );
		return 0;
	}
	HANDLE storage;
	if (!CascOpenStorage( argv[1], CASC_LOCALE_ZHCN, &storage )) {
		printf( "failed to open casc file system, error %d\n", GetLastError() );
		return 0;
	}
	
	std::vector<std::pair<UINT32, item_sparse_record_t> > item_sparse_records;
	std::vector<char> item_sparse_string_block;
	if (!dbc_reader( storage, "DBFilesClient\\Item-sparse.db2", item_sparse_records, item_sparse_string_block )) {
		return 0;
	}
	// only want string block.
	std::vector<std::pair<UINT32, dummy_record_t> > records;
	std::vector<std::vector<char>*> string_blocks;
	std::vector<std::string> dblist = {
		"DBFilesClient\\Achievement.db2",
		"DBFilesClient\\Achievement_Category.db2",
		"DBFilesClient\\AdventureJournal.db2",
		"DBFilesClient\\AdventureMapPOI.db2",
		"DBFilesClient\\AnimKit.db2",
		"DBFilesClient\\AnimKitBoneSet.db2",
		"DBFilesClient\\AnimKitBoneSetAlias.db2",
		"DBFilesClient\\AnimKitConfig.db2",
		"DBFilesClient\\AnimKitConfigBoneSet.db2",
		"DBFilesClient\\AnimKitPriority.db2",
		"DBFilesClient\\AnimKitSegment.db2",
		"DBFilesClient\\AnimReplacement.db2",
		"DBFilesClient\\AnimReplacementSet.db2",
		"DBFilesClient\\AnimationData.db2",
		"DBFilesClient\\AreaAssignment.db2",
		"DBFilesClient\\AreaGroupMember.db2",
		"DBFilesClient\\AreaPOI.db2",
		"DBFilesClient\\AreaPOIState.db2",
		"DBFilesClient\\AreaTable.db2",
		"DBFilesClient\\AreaTrigger.db2",
		"DBFilesClient\\AreaTriggerActionSet.db2",
		"DBFilesClient\\AreaTriggerBox.db2",
		"DBFilesClient\\AreaTriggerCylinder.db2",
		"DBFilesClient\\AreaTriggerSphere.db2",
		"DBFilesClient\\ArmorLocation.db2",
		"DBFilesClient\\Artifact.db2",
		"DBFilesClient\\ArtifactAppearance.db2",
		"DBFilesClient\\ArtifactAppearanceSet.db2",
		"DBFilesClient\\ArtifactCategory.db2",
		"DBFilesClient\\ArtifactPower.db2",
		"DBFilesClient\\ArtifactPowerLink.db2",
		"DBFilesClient\\ArtifactPowerRank.db2",
		"DBFilesClient\\ArtifactQuestXP.db2",
		"DBFilesClient\\ArtifactUnlock.db2",
		"DBFilesClient\\AuctionHouse.db2",
		"DBFilesClient\\BankBagSlotPrices.db2",
		"DBFilesClient\\BannedAddOns.db2",
		"DBFilesClient\\BarberShopStyle.db2",
		"DBFilesClient\\BattlePetAbility.db2",
		"DBFilesClient\\BattlePetAbilityEffect.db2",
		"DBFilesClient\\BattlePetAbilityState.db2",
		"DBFilesClient\\BattlePetAbilityTurn.db2",
		"DBFilesClient\\BattlePetBreedQuality.db2",
		"DBFilesClient\\BattlePetBreedState.db2",
		"DBFilesClient\\BattlePetEffectProperties.db2",
		"DBFilesClient\\BattlePetNPCTeamMember.db2",
		"DBFilesClient\\BattlePetSpecies.db2",
		"DBFilesClient\\BattlePetSpeciesState.db2",
		"DBFilesClient\\BattlePetSpeciesXAbility.db2",
		"DBFilesClient\\BattlePetState.db2",
		"DBFilesClient\\BattlePetVisual.db2",
		"DBFilesClient\\BattlemasterList.db2",
		"DBFilesClient\\Bounty.db2",
		"DBFilesClient\\BountySet.db2",
		"DBFilesClient\\BroadcastText.db2",
		"DBFilesClient\\BroadcastText_internal.db2",
		"DBFilesClient\\CameraEffect.db2",
		"DBFilesClient\\CameraEffectEntry.db2",
		"DBFilesClient\\CameraMode.db2",
		"DBFilesClient\\CameraShakes.db2",
		"DBFilesClient\\CastableRaidBuffs.db2",
		"DBFilesClient\\Cfg_Categories.db2",
		"DBFilesClient\\Cfg_Configs.db2",
		"DBFilesClient\\Cfg_Regions.db2",
		"DBFilesClient\\CharBaseInfo.db2",
		"DBFilesClient\\CharBaseSection.db2",
		"DBFilesClient\\CharComponentTextureLayouts.db2",
		"DBFilesClient\\CharComponentTextureSections.db2",
		"DBFilesClient\\CharHairGeosets.db2",
		"DBFilesClient\\CharSections.db2",
		"DBFilesClient\\CharShipment.db2",
		"DBFilesClient\\CharShipmentContainer.db2",
		"DBFilesClient\\CharStartOutfit.db2",
		"DBFilesClient\\CharTitles.db2",
		"DBFilesClient\\CharacterFaceBoneSet.db2",
		"DBFilesClient\\CharacterFacialHairStyles.db2",
		"DBFilesClient\\CharacterLoadout.db2",
		"DBFilesClient\\CharacterLoadoutItem.db2",
		"DBFilesClient\\ChatChannels.db2",
		"DBFilesClient\\ChatProfanity.db2",
		"DBFilesClient\\ChrClassRaceSex.db2",
		"DBFilesClient\\ChrClassTitle.db2",
		"DBFilesClient\\ChrClassUIDisplay.db2",
		"DBFilesClient\\ChrClassVillain.db2",
		"DBFilesClient\\ChrClasses.db2",
		"DBFilesClient\\ChrClassesXPowerTypes.db2",
		"DBFilesClient\\ChrRaces.db2",
		"DBFilesClient\\ChrSpecialization.db2",
		"DBFilesClient\\ChrUpgradeBucket.db2",
		"DBFilesClient\\ChrUpgradeBucketSpell.db2",
		"DBFilesClient\\ChrUpgradeTier.db2",
		"DBFilesClient\\CinematicCamera.db2",
		"DBFilesClient\\CinematicSequences.db2",
		"DBFilesClient\\CombatCondition.db2",
		"DBFilesClient\\ComponentModelFileData.db2",
		"DBFilesClient\\ComponentTextureFileData.db2",
		"DBFilesClient\\ConsoleScripts.db2",
		"DBFilesClient\\ConversationLine.db2",
		"DBFilesClient\\Creature.db2",
		"DBFilesClient\\CreatureDifficulty.db2",
		"DBFilesClient\\CreatureDifficulty_internal.db2",
		"DBFilesClient\\CreatureDispXUiCamera.db2",
		"DBFilesClient\\CreatureDisplayInfo.db2",
		"DBFilesClient\\CreatureDisplayInfoCond.db2",
		"DBFilesClient\\CreatureDisplayInfoExtra.db2",
		"DBFilesClient\\CreatureDisplayInfoTrn.db2",
		"DBFilesClient\\CreatureFamily.db2",
		"DBFilesClient\\CreatureImmunities.db2",
		"DBFilesClient\\CreatureModelData.db2",
		"DBFilesClient\\CreatureMovementInfo.db2",
		"DBFilesClient\\CreatureSoundData.db2",
		"DBFilesClient\\CreatureType.db2",
		"DBFilesClient\\Creature_internal.db2",
		"DBFilesClient\\Criteria.db2",
		"DBFilesClient\\CriteriaTree.db2",
		"DBFilesClient\\CriteriaTreeXEffect.db2",
		"DBFilesClient\\CurrencyCategory.db2",
		"DBFilesClient\\CurrencyTypes.db2",
		"DBFilesClient\\Curve.db2",
		"DBFilesClient\\CurvePoint.db2",
		"DBFilesClient\\DeathThudLookups.db2",
		"DBFilesClient\\DecalProperties.db2",
		"DBFilesClient\\DeclinedWord.db2",
		"DBFilesClient\\DeclinedWordCases.db2",
		"DBFilesClient\\DestructibleModelData.db2",
		"DBFilesClient\\DeviceBlacklist.db2",
		"DBFilesClient\\DeviceDefaultSettings.db2",
		"DBFilesClient\\Difficulty.db2",
		"DBFilesClient\\DissolveEffect.db2",
		"DBFilesClient\\DriverBlacklist.db2",
		"DBFilesClient\\DungeonEncounter.db2",
		"DBFilesClient\\DungeonMap.db2",
		"DBFilesClient\\DungeonMapChunk.db2",
		"DBFilesClient\\DurabilityCosts.db2",
		"DBFilesClient\\DurabilityQuality.db2",
		"DBFilesClient\\EdgeGlowEffect.db2",
		"DBFilesClient\\Emotes.db2",
		"DBFilesClient\\EmotesText.db2",
		"DBFilesClient\\EmotesTextData.db2",
		"DBFilesClient\\EmotesTextSound.db2",
		"DBFilesClient\\EnvironmentalDamage.db2",
		"DBFilesClient\\Exhaustion.db2",
		"DBFilesClient\\Faction.db2",
		"DBFilesClient\\FactionGroup.db2",
		"DBFilesClient\\FactionTemplate.db2",
		"DBFilesClient\\FileDataComplete.db2",
		"DBFilesClient\\FootprintTextures.db2",
		"DBFilesClient\\FootstepTerrainLookup.db2",
		"DBFilesClient\\FriendshipRepReaction.db2",
		"DBFilesClient\\FriendshipReputation.db2",
		"DBFilesClient\\FullScreenEffect.db2",
		"DBFilesClient\\GMSurveyAnswers.db2",
		"DBFilesClient\\GMSurveyCurrentSurvey.db2",
		"DBFilesClient\\GMSurveyQuestions.db2",
		"DBFilesClient\\GMSurveySurveys.db2",
		"DBFilesClient\\GameObjectArtKit.db2",
		"DBFilesClient\\GameObjectDiffAnimMap.db2",
		"DBFilesClient\\GameObjectDisplayInfo.db2",
		"DBFilesClient\\GameObjectDisplayInfoXSoundKit.db2",
		"DBFilesClient\\GameObjects.db2",
		"DBFilesClient\\GameObjects_internal.db2",
		"DBFilesClient\\GameTips.db2",
		"DBFilesClient\\GarrAbility.db2",
		"DBFilesClient\\GarrAbilityCategory.db2",
		"DBFilesClient\\GarrAbilityEffect.db2",
		"DBFilesClient\\GarrBuilding.db2",
		"DBFilesClient\\GarrBuildingDoodadSet.db2",
		"DBFilesClient\\GarrBuildingPlotInst.db2",
		"DBFilesClient\\GarrClassSpec.db2",
		"DBFilesClient\\GarrClassSpecPlayerCond.db2",
		"DBFilesClient\\GarrEncounter.db2",
		"DBFilesClient\\GarrEncounterSetXEncounter.db2",
		"DBFilesClient\\GarrEncounterXMechanic.db2",
		"DBFilesClient\\GarrFollItemSetMember.db2",
		"DBFilesClient\\GarrFollSupportSpell.db2",
		"DBFilesClient\\GarrFollower.db2",
		"DBFilesClient\\GarrFollowerLevelXP.db2",
		"DBFilesClient\\GarrFollowerQuality.db2",
		"DBFilesClient\\GarrFollowerSetXFollower.db2",
		"DBFilesClient\\GarrFollowerType.db2",
		"DBFilesClient\\GarrFollowerXAbility.db2",
		"DBFilesClient\\GarrMechanic.db2",
		"DBFilesClient\\GarrMechanicSetXMechanic.db2",
		"DBFilesClient\\GarrMechanicType.db2",
		"DBFilesClient\\GarrMission.db2",
		"DBFilesClient\\GarrMissionReward.db2",
		"DBFilesClient\\GarrMissionTexture.db2",
		"DBFilesClient\\GarrMissionType.db2",
		"DBFilesClient\\GarrMissionXEncounter.db2",
		"DBFilesClient\\GarrMissionXFollower.db2",
		"DBFilesClient\\GarrMssnBonusAbility.db2",
		"DBFilesClient\\GarrPlot.db2",
		"DBFilesClient\\GarrPlotBuilding.db2",
		"DBFilesClient\\GarrPlotInstance.db2",
		"DBFilesClient\\GarrPlotUICategory.db2",
		"DBFilesClient\\GarrSiteLevel.db2",
		"DBFilesClient\\GarrSiteLevelPlotInst.db2",
		"DBFilesClient\\GarrSpecialization.db2",
		"DBFilesClient\\GarrTalent.db2",
		"DBFilesClient\\GarrTalentTree.db2",
		"DBFilesClient\\GarrType.db2",
		"DBFilesClient\\GarrUiAnimClassInfo.db2",
		"DBFilesClient\\GarrUiAnimRaceInfo.db2",
		"DBFilesClient\\GemProperties.db2",
		"DBFilesClient\\GemProperties_internal.db2",
		"DBFilesClient\\GlobalStrings.db2",
		"DBFilesClient\\GlueScreenEmote.db2",
		"DBFilesClient\\GlyphBindableSpell.db2",
		"DBFilesClient\\GlyphExclusiveCategory.db2",
		"DBFilesClient\\GlyphProperties.db2",
		"DBFilesClient\\GlyphRequiredSpec.db2",
		"DBFilesClient\\GlyphSlot.db2",
		"DBFilesClient\\GroundEffectDoodad.db2",
		"DBFilesClient\\GroundEffectTexture.db2",
		"DBFilesClient\\GroupFinderActivity.db2",
		"DBFilesClient\\GroupFinderActivityGrp.db2",
		"DBFilesClient\\GroupFinderCategory.db2",
		"DBFilesClient\\GuildColorBackground.db2",
		"DBFilesClient\\GuildColorBorder.db2",
		"DBFilesClient\\GuildColorEmblem.db2",
		"DBFilesClient\\GuildPerkSpells.db2",
		"DBFilesClient\\Heirloom.db2",
		"DBFilesClient\\HelmetAnimScaling.db2",
		"DBFilesClient\\HelmetGeosetVisData.db2",
		"DBFilesClient\\HighlightColor.db2",
		"DBFilesClient\\HolidayDescriptions.db2",
		"DBFilesClient\\HolidayNames.db2",
		"DBFilesClient\\Holidays.db2",
		"DBFilesClient\\ImportPriceArmor.db2",
		"DBFilesClient\\ImportPriceQuality.db2",
		"DBFilesClient\\ImportPriceShield.db2",
		"DBFilesClient\\ImportPriceWeapon.db2",
		"DBFilesClient\\InvasionClientData.db2",
		"DBFilesClient\\Item-sparse.db2",
		"DBFilesClient\\Item.db2",
		"DBFilesClient\\ItemAppearance.db2",
		"DBFilesClient\\ItemAppearanceXUiCamera.db2",
		"DBFilesClient\\ItemArmorQuality.db2",
		"DBFilesClient\\ItemArmorShield.db2",
		"DBFilesClient\\ItemArmorTotal.db2",
		"DBFilesClient\\ItemBagFamily.db2",
		"DBFilesClient\\ItemBonus.db2",
		"DBFilesClient\\ItemBonusListLevelDelta.db2",
		"DBFilesClient\\ItemBonusTreeNode.db2",
		"DBFilesClient\\ItemChildEquipment.db2",
		"DBFilesClient\\ItemChildEquipment_internal.db2",
		"DBFilesClient\\ItemClass.db2",
		"DBFilesClient\\ItemContextPickerEntry.db2",
		"DBFilesClient\\ItemCurrencyCost.db2",
		"DBFilesClient\\ItemCurrencyCost_internal.db2",
		"DBFilesClient\\ItemDamageAmmo.db2",
		"DBFilesClient\\ItemDamageOneHand.db2",
		"DBFilesClient\\ItemDamageOneHandCaster.db2",
		"DBFilesClient\\ItemDamageTwoHand.db2",
		"DBFilesClient\\ItemDamageTwoHandCaster.db2",
		"DBFilesClient\\ItemDisenchantLoot.db2",
		"DBFilesClient\\ItemDisplayInfo.db2",
		"DBFilesClient\\ItemDisplayInfoMaterialRes.db2",
		"DBFilesClient\\ItemDisplayXUiCamera.db2",
		"DBFilesClient\\ItemEffect.db2",
		"DBFilesClient\\ItemEffect_internal.db2",
		"DBFilesClient\\ItemExtendedCost.db2",
		"DBFilesClient\\ItemGroupSounds.db2",
		"DBFilesClient\\ItemLimitCategory.db2",
		"DBFilesClient\\ItemLimitCategoryCondition.db2",
		"DBFilesClient\\ItemModifiedAppearance.db2",
		"DBFilesClient\\ItemModifiedAppearanceExtra.db2",
		"DBFilesClient\\ItemModifiedAppearance_internal.db2",
		"DBFilesClient\\ItemNameDescription.db2",
		"DBFilesClient\\ItemPetFood.db2",
		"DBFilesClient\\ItemPriceBase.db2",
		"DBFilesClient\\ItemRandomProperties.db2",
		"DBFilesClient\\ItemRandomSuffix.db2",
		"DBFilesClient\\ItemRangedDisplayInfo.db2",
		"DBFilesClient\\ItemSearchName.db2",
		"DBFilesClient\\ItemSet.db2",
		"DBFilesClient\\ItemSetSpell.db2",
		"DBFilesClient\\ItemSpec.db2",
		"DBFilesClient\\ItemSpecOverride.db2",
		"DBFilesClient\\ItemSpecOverride_internal.db2",
		"DBFilesClient\\ItemSubClass.db2",
		"DBFilesClient\\ItemSubClassMask.db2",
		"DBFilesClient\\ItemUpgrade.db2",
		"DBFilesClient\\ItemVisualEffects.db2",
		"DBFilesClient\\ItemVisuals.db2",
		"DBFilesClient\\ItemXBonusTree.db2",
		"DBFilesClient\\ItemXBonusTree_internal.db2",
		"DBFilesClient\\Item_internal.db2",
		"DBFilesClient\\JournalEncounter.db2",
		"DBFilesClient\\JournalEncounterCreature.db2",
		"DBFilesClient\\JournalEncounterItem.db2",
		"DBFilesClient\\JournalEncounterSection.db2",
		"DBFilesClient\\JournalEncounterXDifficulty.db2",
		"DBFilesClient\\JournalInstance.db2",
		"DBFilesClient\\JournalItemXDifficulty.db2",
		"DBFilesClient\\JournalSectionXDifficulty.db2",
		"DBFilesClient\\JournalTier.db2",
		"DBFilesClient\\JournalTierXInstance.db2",
		"DBFilesClient\\KeyChain.db2",
		"DBFilesClient\\KeystoneAffix.db2",
		"DBFilesClient\\LanguageWords.db2",
		"DBFilesClient\\Languages.db2",
		"DBFilesClient\\LfgDungeonExpansion.db2",
		"DBFilesClient\\LfgDungeonGroup.db2",
		"DBFilesClient\\LfgDungeons.db2",
		"DBFilesClient\\LfgDungeonsGroupingMap.db2",
		"DBFilesClient\\LfgRoleRequirement.db2",
		"DBFilesClient\\Light.db2",
		"DBFilesClient\\LightData.db2",
		"DBFilesClient\\LightParams.db2",
		"DBFilesClient\\LightSkybox.db2",
		"DBFilesClient\\LiquidMaterial.db2",
		"DBFilesClient\\LiquidObject.db2",
		"DBFilesClient\\LiquidType.db2",
		"DBFilesClient\\LoadingScreenTaxiSplines.db2",
		"DBFilesClient\\LoadingScreens.db2",
		"DBFilesClient\\Locale.db2",
		"DBFilesClient\\Location.db2",
		"DBFilesClient\\Lock.db2",
		"DBFilesClient\\LockType.db2",
		"DBFilesClient\\LookAtController.db2",
		"DBFilesClient\\MailTemplate.db2",
		"DBFilesClient\\ManifestInterfaceActionIcon.db2",
		"DBFilesClient\\ManifestInterfaceData.db2",
		"DBFilesClient\\ManifestInterfaceItemIcon.db2",
		"DBFilesClient\\ManifestInterfaceTOCData.db2",
		"DBFilesClient\\ManifestMP3.db2",
		"DBFilesClient\\Map.db2",
		"DBFilesClient\\MapChallengeMode.db2",
		"DBFilesClient\\MapDifficulty.db2",
		"DBFilesClient\\MapDifficultyXCondition.db2",
		"DBFilesClient\\MarketingPromotionsXLocale.db2",
		"DBFilesClient\\Material.db2",
		"DBFilesClient\\MinorTalent.db2",
		"DBFilesClient\\ModelFileData.db2",
		"DBFilesClient\\ModelManifest.db2",
		"DBFilesClient\\ModelNameToManifest.db2",
		"DBFilesClient\\ModelRibbonQuality.db2",
		"DBFilesClient\\ModifierTree.db2",
		"DBFilesClient\\Mount.db2",
		"DBFilesClient\\MountCapability.db2",
		"DBFilesClient\\MountType.db2",
		"DBFilesClient\\MountTypeXCapability.db2",
		"DBFilesClient\\Movie.db2",
		"DBFilesClient\\MovieFileData.db2",
		"DBFilesClient\\MovieVariation.db2",
		"DBFilesClient\\NPCSounds.db2",
		"DBFilesClient\\NameGen.db2",
		"DBFilesClient\\NamesProfanity.db2",
		"DBFilesClient\\NamesReserved.db2",
		"DBFilesClient\\NamesReservedLocale.db2",
		"DBFilesClient\\NpcModelItemSlotDisplayInfo.db2",
		"DBFilesClient\\ObjectEffect.db2",
		"DBFilesClient\\ObjectEffectGroup.db2",
		"DBFilesClient\\ObjectEffectModifier.db2",
		"DBFilesClient\\ObjectEffectPackage.db2",
		"DBFilesClient\\ObjectEffectPackageElem.db2",
		"DBFilesClient\\OutlineEffect.db2",
		"DBFilesClient\\OverrideSpellData.db2",
		"DBFilesClient\\PageTextMaterial.db2",
		"DBFilesClient\\PaperDollItemFrame.db2",
		"DBFilesClient\\ParticleColor.db2",
		"DBFilesClient\\Path.db2",
		"DBFilesClient\\PathNode.db2",
		"DBFilesClient\\PathNodeProperty.db2",
		"DBFilesClient\\PathProperty.db2",
		"DBFilesClient\\Phase.db2",
		"DBFilesClient\\PhaseShiftZoneSounds.db2",
		"DBFilesClient\\PhaseXPhaseGroup.db2",
		"DBFilesClient\\Phase_internal.db2",
		"DBFilesClient\\PlayerCondition.db2",
		"DBFilesClient\\Positioner.db2",
		"DBFilesClient\\PositionerState.db2",
		"DBFilesClient\\PositionerStateEntry.db2",
		"DBFilesClient\\PowerDisplay.db2",
		"DBFilesClient\\PowerType.db2",
		"DBFilesClient\\PrestigeLevelInfo.db2",
		"DBFilesClient\\PvpBracketTypes.db2",
		"DBFilesClient\\PvpDifficulty.db2",
		"DBFilesClient\\PvpItem.db2",
		"DBFilesClient\\PvpReward.db2",
		"DBFilesClient\\PvpTalent.db2",
		"DBFilesClient\\PvpTalentUnlock.db2",
		"DBFilesClient\\QuestFactionReward.db2",
		"DBFilesClient\\QuestFeedbackEffect.db2",
		"DBFilesClient\\QuestInfo.db2",
		"DBFilesClient\\QuestLine.db2",
		"DBFilesClient\\QuestLineXQuest.db2",
		"DBFilesClient\\QuestMoneyReward.db2",
		"DBFilesClient\\QuestObjective.db2",
		"DBFilesClient\\QuestPOIBlob.db2",
		"DBFilesClient\\QuestPOIPoint.db2",
		"DBFilesClient\\QuestPOIPointCliTask.db2",
		"DBFilesClient\\QuestPackageItem.db2",
		"DBFilesClient\\QuestSort.db2",
		"DBFilesClient\\QuestV2.db2",
		"DBFilesClient\\QuestV2CliTask.db2",
		"DBFilesClient\\QuestV2_internal.db2",
		"DBFilesClient\\QuestXP.db2",
		"DBFilesClient\\RacialMounts.db2",
		"DBFilesClient\\RandPropPoints.db2",
		"DBFilesClient\\ResearchBranch.db2",
		"DBFilesClient\\ResearchField.db2",
		"DBFilesClient\\ResearchProject.db2",
		"DBFilesClient\\ResearchSite.db2",
		"DBFilesClient\\Resistances.db2",
		"DBFilesClient\\RewardPack.db2",
		"DBFilesClient\\RewardPackXCurrencyType.db2",
		"DBFilesClient\\RewardPackXItem.db2",
		"DBFilesClient\\RibbonQuality.db2",
		"DBFilesClient\\RulesetItemUpgrade.db2",
		"DBFilesClient\\ScalingStatDistribution.db2",
		"DBFilesClient\\ScalingStatDistribution_internal.db2",
		"DBFilesClient\\Scenario.db2",
		"DBFilesClient\\ScenarioEventEntry.db2",
		"DBFilesClient\\ScenarioStep.db2",
		"DBFilesClient\\Scenario_internal.db2",
		"DBFilesClient\\SceneScript.db2",
		"DBFilesClient\\SceneScriptPackage.db2",
		"DBFilesClient\\SceneScriptPackageMember.db2",
		"DBFilesClient\\SceneScriptPackage_internal.db2",
		"DBFilesClient\\ScheduledInterval.db2",
		"DBFilesClient\\ScheduledUniqueCategory.db2",
		"DBFilesClient\\ScheduledWorldState.db2",
		"DBFilesClient\\ScheduledWorldStateGroup.db2",
		"DBFilesClient\\ScheduledWorldStateXUniqCat.db2",
		"DBFilesClient\\ScreenEffect.db2",
		"DBFilesClient\\ScreenLocation.db2",
		"DBFilesClient\\SeamlessSite.db2",
		"DBFilesClient\\ServerMessages.db2",
		"DBFilesClient\\ShadowyEffect.db2",
		"DBFilesClient\\SkillLine.db2",
		"DBFilesClient\\SkillLineAbility.db2",
		"DBFilesClient\\SkillRaceClassInfo.db2",
		"DBFilesClient\\SoundAmbience.db2",
		"DBFilesClient\\SoundAmbienceFlavor.db2",
		"DBFilesClient\\SoundBus.db2",
		"DBFilesClient\\SoundBusName.db2",
		"DBFilesClient\\SoundEmitterPillPoints.db2",
		"DBFilesClient\\SoundEmitters.db2",
		"DBFilesClient\\SoundFilter.db2",
		"DBFilesClient\\SoundFilterElem.db2",
		"DBFilesClient\\SoundKit.db2",
		"DBFilesClient\\SoundKitAdvanced.db2",
		"DBFilesClient\\SoundKitChild.db2",
		"DBFilesClient\\SoundKitEntry.db2",
		"DBFilesClient\\SoundKitFallback.db2",
		"DBFilesClient\\SoundKit_internal.db2",
		"DBFilesClient\\SoundOverride.db2",
		"DBFilesClient\\SoundProviderPreferences.db2",
		"DBFilesClient\\SourceInfo.db2",
		"DBFilesClient\\SpamMessages.db2",
		"DBFilesClient\\SpecializationSpells.db2",
		"DBFilesClient\\Spell.db2",
		"DBFilesClient\\SpellActionBarPref.db2",
		"DBFilesClient\\SpellActivationOverlay.db2",
		"DBFilesClient\\SpellActivationOverlay_internal.db2",
		"DBFilesClient\\SpellAuraOptions.db2",
		"DBFilesClient\\SpellAuraOptions_internal.db2",
		"DBFilesClient\\SpellAuraRestrictions.db2",
		"DBFilesClient\\SpellAuraRestrictions_internal.db2",
		"DBFilesClient\\SpellAuraVisXChrSpec.db2",
		"DBFilesClient\\SpellAuraVisibility.db2",
		"DBFilesClient\\SpellAuraVisibility_internal.db2",
		"DBFilesClient\\SpellCastTimes.db2",
		"DBFilesClient\\SpellCastingRequirements.db2",
		"DBFilesClient\\SpellCastingRequirements_internal.db2",
		"DBFilesClient\\SpellCategories.db2",
		"DBFilesClient\\SpellCategories_internal.db2",
		"DBFilesClient\\SpellCategory.db2",
		"DBFilesClient\\SpellChainEffects.db2",
		"DBFilesClient\\SpellClassOptions.db2",
		"DBFilesClient\\SpellClassOptions_internal.db2",
		"DBFilesClient\\SpellCooldowns.db2",
		"DBFilesClient\\SpellCooldowns_internal.db2",
		"DBFilesClient\\SpellDescriptionVariables.db2",
		"DBFilesClient\\SpellDispelType.db2",
		"DBFilesClient\\SpellDuration.db2",
		"DBFilesClient\\SpellEffect.db2",
		"DBFilesClient\\SpellEffectCameraShakes.db2",
		"DBFilesClient\\SpellEffectEmission.db2",
		"DBFilesClient\\SpellEffectGroupSize.db2",
		"DBFilesClient\\SpellEffectGroupSize_internal.db2",
		"DBFilesClient\\SpellEffectScaling.db2",
		"DBFilesClient\\SpellEffect_internal.db2",
		"DBFilesClient\\SpellEquippedItems.db2",
		"DBFilesClient\\SpellEquippedItems_internal.db2",
		"DBFilesClient\\SpellFlyout.db2",
		"DBFilesClient\\SpellFlyoutItem.db2",
		"DBFilesClient\\SpellFocusObject.db2",
		"DBFilesClient\\SpellIcon.db2",
		"DBFilesClient\\SpellInterrupts.db2",
		"DBFilesClient\\SpellInterrupts_internal.db2",
		"DBFilesClient\\SpellItemEnchantment.db2",
		"DBFilesClient\\SpellItemEnchantmentCondition.db2",
		"DBFilesClient\\SpellKeyboundOverride.db2",
		"DBFilesClient\\SpellLabel.db2",
		"DBFilesClient\\SpellLabel_internal.db2",
		"DBFilesClient\\SpellLearnSpell.db2",
		"DBFilesClient\\SpellLearnSpell_internal.db2",
		"DBFilesClient\\SpellLevels.db2",
		"DBFilesClient\\SpellLevels_internal.db2",
		"DBFilesClient\\SpellMechanic.db2",
		"DBFilesClient\\SpellMisc.db2",
		"DBFilesClient\\SpellMiscDifficulty.db2",
		"DBFilesClient\\SpellMisc_internal.db2",
		"DBFilesClient\\SpellMissile.db2",
		"DBFilesClient\\SpellMissileMotion.db2",
		"DBFilesClient\\SpellMissile_internal.db2",
		"DBFilesClient\\SpellPower.db2",
		"DBFilesClient\\SpellPowerDifficulty.db2",
		"DBFilesClient\\SpellPower_internal.db2",
		"DBFilesClient\\SpellProceduralEffect.db2",
		"DBFilesClient\\SpellProcsPerMinute.db2",
		"DBFilesClient\\SpellProcsPerMinuteMod.db2",
		"DBFilesClient\\SpellRadius.db2",
		"DBFilesClient\\SpellRange.db2",
		"DBFilesClient\\SpellReagents.db2",
		"DBFilesClient\\SpellReagentsCurrency.db2",
		"DBFilesClient\\SpellReagentsCurrency_internal.db2",
		"DBFilesClient\\SpellReagents_internal.db2",
		"DBFilesClient\\SpellScaling.db2",
		"DBFilesClient\\SpellScaling_internal.db2",
		"DBFilesClient\\SpellShapeshift.db2",
		"DBFilesClient\\SpellShapeshiftForm.db2",
		"DBFilesClient\\SpellShapeshift_internal.db2",
		"DBFilesClient\\SpellSpecialUnitEffect.db2",
		"DBFilesClient\\SpellTargetRestrictions.db2",
		"DBFilesClient\\SpellTargetRestrictions_internal.db2",
		"DBFilesClient\\SpellTotems.db2",
		"DBFilesClient\\SpellTotems_internal.db2",
		"DBFilesClient\\SpellVisual.db2",
		"DBFilesClient\\SpellVisualAnim.db2",
		"DBFilesClient\\SpellVisualColorEffect.db2",
		"DBFilesClient\\SpellVisualEffectName.db2",
		"DBFilesClient\\SpellVisualKit.db2",
		"DBFilesClient\\SpellVisualKitAreaModel.db2",
		"DBFilesClient\\SpellVisualKitEffect.db2",
		"DBFilesClient\\SpellVisualKitModelAttach.db2",
		"DBFilesClient\\SpellVisualMissile.db2",
		"DBFilesClient\\SpellXSpellVisual.db2",
		"DBFilesClient\\SpellXSpellVisual_internal.db2",
		"DBFilesClient\\Spell_internal.db2",
		"DBFilesClient\\Startup_Strings.db2",
		"DBFilesClient\\Stationery.db2",
		"DBFilesClient\\StringLookups.db2",
		"DBFilesClient\\SummonProperties.db2",
		"DBFilesClient\\TactKey.db2",
		"DBFilesClient\\TactKeyLookup.db2",
		"DBFilesClient\\Talent.db2",
		"DBFilesClient\\TaxiNodes.db2",
		"DBFilesClient\\TaxiPath.db2",
		"DBFilesClient\\TaxiPathNode.db2",
		"DBFilesClient\\TerrainMaterial.db2",
		"DBFilesClient\\TerrainType.db2",
		"DBFilesClient\\TerrainTypeSounds.db2",
		"DBFilesClient\\TextureBlendSet.db2",
		"DBFilesClient\\TextureFileData.db2",
		"DBFilesClient\\TotemCategory.db2",
		"DBFilesClient\\Toy.db2",
		"DBFilesClient\\TradeSkillCategory.db2",
		"DBFilesClient\\TradeSkillItem.db2",
		"DBFilesClient\\TransformMatrix.db2",
		"DBFilesClient\\TransmogSet.db2",
		"DBFilesClient\\TransmogSetItem.db2",
		"DBFilesClient\\TransportAnimation.db2",
		"DBFilesClient\\TransportPhysics.db2",
		"DBFilesClient\\TransportRotation.db2",
		"DBFilesClient\\Trophy.db2",
		"DBFilesClient\\UiCamFbackTransmogChrRace.db2",
		"DBFilesClient\\UiCamFbackTransmogWeapon.db2",
		"DBFilesClient\\UiCamera.db2",
		"DBFilesClient\\UiCameraType.db2",
		"DBFilesClient\\UiTextureAtlas.db2",
		"DBFilesClient\\UiTextureAtlasMember.db2",
		"DBFilesClient\\UiTextureKit.db2",
		"DBFilesClient\\UnitBlood.db2",
		"DBFilesClient\\UnitBloodLevels.db2",
		"DBFilesClient\\UnitCondition.db2",
		"DBFilesClient\\UnitPowerBar.db2",
		"DBFilesClient\\Vehicle.db2",
		"DBFilesClient\\VehicleSeat.db2",
		"DBFilesClient\\VehicleUIIndSeat.db2",
		"DBFilesClient\\VehicleUIIndicator.db2",
		"DBFilesClient\\VideoHardware.db2",
		"DBFilesClient\\Vignette.db2",
		"DBFilesClient\\VocalUISounds.db2",
		"DBFilesClient\\WMOAreaTable.db2",
		"DBFilesClient\\WbAccessControlList.db2",
		"DBFilesClient\\WbAccessControlList_internal.db2",
		"DBFilesClient\\WbCertBlacklist.db2",
		"DBFilesClient\\WbCertWhitelist.db2",
		"DBFilesClient\\WbCertWhitelist_internal.db2",
		"DBFilesClient\\WbPermissions.db2",
		"DBFilesClient\\WeaponImpactSounds.db2",
		"DBFilesClient\\WeaponSwingSounds2.db2",
		"DBFilesClient\\WeaponTrail.db2",
		"DBFilesClient\\WeaponTrailModelDef.db2",
		"DBFilesClient\\WeaponTrailParam.db2",
		"DBFilesClient\\Weather.db2",
		"DBFilesClient\\WindSettings.db2",
		"DBFilesClient\\WmoMinimapTexture.db2",
		"DBFilesClient\\WorldBossLockout.db2",
		"DBFilesClient\\WorldChunkSounds.db2",
		"DBFilesClient\\WorldEffect.db2",
		"DBFilesClient\\WorldElapsedTimer.db2",
		"DBFilesClient\\WorldMapArea.db2",
		"DBFilesClient\\WorldMapContinent.db2",
		"DBFilesClient\\WorldMapOverlay.db2",
		"DBFilesClient\\WorldMapTransforms.db2",
		"DBFilesClient\\WorldSafeLocs.db2",
		"DBFilesClient\\WorldState.db2",
		"DBFilesClient\\WorldStateExpression.db2",
		"DBFilesClient\\WorldStateUI.db2",
		"DBFilesClient\\WorldStateZoneSounds.db2",
		"DBFilesClient\\World_PVP_Area.db2",
		"DBFilesClient\\ZoneIntroMusicTable.db2",
		"DBFilesClient\\ZoneLight.db2",
		"DBFilesClient\\ZoneLightPoint.db2",
		"DBFilesClient\\ZoneMusic.db2",
	};
	std::vector<std::string> dbblacklist = {
		"DBFilesClient\\AreaAssignment.db2",
		"DBFilesClient\\SpamMessages.db2",
		"DBFilesClient\\Item-sparse.db2",
		"DBFilesClient\\FileDataComplete.db2",
		"DBFilesClient\\ChatProfanity.db2",
	};
	for (auto i = dblist.begin(); i != dblist.end(); i++) {
		int black = 0;
		for (auto j = dbblacklist.begin(); j != dbblacklist.end(); j++) {
			if ( 0 == i->compare(*j) ) {
				black = 1;
				break;
			}
		}
		if (black) continue;
		std::vector<char>* new_string_block = new std::vector<char>;
		if (!dbc_reader( storage, i->c_str(), records, *new_string_block )) {
			continue;
		}
		records.clear();
		string_blocks.push_back( new_string_block );
	}
	struct item_t {
		int id;
		std::string name;
		std::string desc;
		item_t() : id( 0 ) { }
		bool operator< ( const item_t& rhs ) { return id < rhs.id; }
	};
	std::vector<item_t> item_data;
	for (auto i = item_sparse_records.begin(); i != item_sparse_records.end(); i++) {
		item_t item;
		item.id = i->first;
		item.name = i->second.name;
		item.desc = i->second.desc;
		item_data.push_back( item );
	}

	std::sort( item_data.begin(), item_data.end() );

	FILE* f = fopen( "corpus.inc", "wb" );
	fprintf( f, "\xEF\xBB\xBF" );
	for (auto i = item_data.begin(); i != item_data.end(); i++) {
		if (!i->name.empty()) fprintf( f, "%s\n", i->name.c_str() );
		if (!i->desc.empty()) fprintf( f, "%s\n", i->desc.c_str() );
	}
	for (auto db = string_blocks.begin(); db != string_blocks.end(); db++) {
		for (auto i = (*db)->begin(); i != (*db)->end(); i++) {
			if (*i == 0) fprintf( f, "\n" );
			else fprintf( f, "%c", *i );
		}
	}
	fclose( f );
	printf( "dumped into \"corpus.inc\"\n" );
	FILE* fin = fopen( "corpus.inc", "rb" );
	FILE* fout = fopen( "corpus_zh.inc", "wb" );
	char ch;
	int space = 0;
	while (EOF != ( ch = fgetc( fin ) )) {
		process_ch( ch, fout, space );
	}
	fclose( fin );
	fclose( fout );
	//system("pause");
	/*
	FILE* fout = fopen( "desc.txt", "wb" );
	fprintf( fout, "\xEF\xBB\xBF" );
	std::vector<char> chrspec_string_block;
	struct chrspec_rec_t : public record_t {
		uint32_t id_mastery;
		uint32_t id_mastery2;
		uint32_t string_unk4;
		uint32_t string_name;
		uint32_t string_desc;
		uint32_t string_unk5;
		uint16_t icon;
		uint8_t  class_id;
		uint8_t  index;
		uint8_t  f9;
		uint8_t  spec_type;
		uint8_t  unk_6;
		uint16_t id;
		uint8_t  unk_2;
		uint16_t  unk_3;
	};
	std::vector<std::pair<UINT32, chrspec_rec_t> > chrspec_records;
	dbc_reader( storage, "DBFilesClient\\ChrSpecialization.db2", chrspec_records, chrspec_string_block );
	std::vector<char> specspell_string_block;
	struct specspell_rec_t : public record_t {
		uint32_t spell_id;
		uint32_t replace_spell_id;
		uint32_t unk_1;
		uint16_t spec_id;
		uint8_t  unk_2;
		uint16_t id;
	};
	std::vector<std::pair<UINT32, specspell_rec_t> > specspell_records;
	dbc_reader( storage, "DBFilesClient\\SpecializationSpells.db2", specspell_records, specspell_string_block );
	struct chrclass_rec_t : public record_t {
		uint32_t dc1;
		uint32_t name;
		uint32_t dc18;
		uint32_t dc2;
		uint32_t dc3;
		uint32_t dc4;
		uint32_t dc5;
		uint32_t dc6;
		uint16_t dc7;
		uint16_t dc8;
		uint16_t dc9;
		uint8_t dc10;
		uint8_t dc11;
		uint8_t dc12;
		uint8_t dc13;
		uint8_t dc14;
		uint8_t dc15;
		uint8_t dc16;
		uint8_t dc17;
	};
	std::vector<char> chrclass_string_block;
	std::vector<std::pair<UINT32, chrclass_rec_t> > chrclass_records;
	dbc_reader( storage, "DBFilesClient\\ChrClasses.db2", chrclass_records, chrclass_string_block );
	struct skillline_rec_t : public record_t {
		uint32_t id_spell;
		uint32_t mask_race;
		uint32_t unk_mask_class;
		uint32_t id_replace;
		uint16_t id_skill;
		uint16_t req_skill_level;
		uint16_t max_learn_skill;
		uint16_t unk_1;
		uint16_t index;
		uint16_t id_filter;
		int8_t unk_2;
		int8_t reward_skill_points;
		uint16_t mask_class;
	};
	std::vector<char> skillline_string_block;
	std::vector<std::pair<UINT32, skillline_rec_t> > skillline_records;
	dbc_reader( storage, "DBFilesClient\\SkillLineAbility.db2", skillline_records, skillline_string_block );
	struct talent_rec_t : public record_t {
		uint32_t id_spell;
		uint32_t id_replace;
		uint32_t desc;
		uint16_t spec_id;
		uint8_t row;
		uint8_t col;
		uint8_t pet;
		uint8_t unk1;
		uint8_t unk2;
		uint8_t class_id;
	};
	std::vector<char> talent_string_block;
	std::vector<std::pair<UINT32, talent_rec_t> > talent_records;
	dbc_reader( storage, "DBFilesClient\\Talent.db2", talent_records, talent_string_block );

	dbc_reader( storage, "DBFilesClient\\Spell.db2", spell_records, spell_string_block );
	std::string class_name[13];
	auto select_spell = []( int id ) {
		for (auto i = spell_records.begin(); i != spell_records.end(); i++) {
			if (i->second.id == id) return i->second;
		}
		return spell_rec_t();
	};
	auto print_spell = [&]( int id ) {
		auto rec = select_spell( id );
		if (spell_string_block.data()[rec.name] && spell_string_block.data()[rec.desc])
			return fprintf( fout, "[*][wow,spell,%d,cn[%s]]:%s\r\n", id, &spell_string_block.data()[rec.name], func_spell_desc( id ).c_str() );
		else return 0;
	};
	for (auto c = chrclass_records.begin(); c != chrclass_records.end(); c++) {
		fprintf( fout, "[h]%s[/h]", &chrclass_string_block.data()[c->second.name] );
		class_name[c->first + 1] = &chrclass_string_block.data()[c->second.name];
		fprintf( fout, u8"[list][*]通用技能[list]\r\n" );
		for (auto i = skillline_records.begin(); i != skillline_records.end(); i++) {
			if (i->second.mask_class == ( 1 << c->first ) && i->second.mask_race == 0) {
				print_spell( i->second.id_spell );
			}
		}
		fprintf( fout, u8"[/list][*]天赋[list]\r\n" );
		for (auto i = talent_records.begin(); i != talent_records.end(); i++) {
			if (i->second.class_id == ( c->first + 1 ) && i->second.spec_id == 0) {
				print_spell( i->second.id_spell );
			}
		}
		fprintf( fout, "[/list]\r\n" );
		for (auto s = chrspec_records.begin(); s != chrspec_records.end(); s++) {
			if (s->second.class_id != c->first + 1) continue;
			fprintf( fout, u8"[*]%s\r\n[list][*]专精技能[list]\r\n", &chrspec_string_block.data()[s->second.string_name] );
			for (auto i = specspell_records.begin(); i != specspell_records.end(); i++) {
				if (i->second.spec_id == s->second.id) {
					print_spell( i->second.spell_id );
				}
			}
			fprintf( fout, "[/list]" );
			fprintf( fout, u8"[*]天赋[list]\r\n" );
			for (auto i = talent_records.begin(); i != talent_records.end(); i++) {
				if (i->second.class_id == ( c->first + 1 ) && i->second.spec_id == s->second.id) {
					print_spell( i->second.id_spell );
				}
			}
			fprintf( fout, "[/list][/list]" );
		}
		fprintf( fout, "[/list]\r\n" );
	}
	fclose( fout );*/
	CascCloseStorage( storage );
	return 0;
}
/*
std::string func_spell_desc( int id ) {
	spell_rec_t rec;
	int found = 0;
	for (auto i = spell_records.begin(); i != spell_records.end(); i++) {
		if (i->second.id == id) {
			rec = i->second;
			found = 1;
			break;
		}
	}
	if (!found) return std::string( "spelldesc not found" );
	std::string in( &spell_string_block.data()[rec.desc] );
	std::string out( "" );
	int state = 0;
	int spelldesc = 0;
	// delete redundant line breaker
	for (auto i = in.begin(); i != in.end(); i++) {
		if (state) {
			if (*i == '\n' || *i == '\r') continue;
			else {
				state = 0;
				out.push_back( '\r' );
				out.push_back( '\n' );
				out.push_back( *i );
			}
		} else {
			if (*i == '\n' || *i == '\r') state = 1;
			else out.push_back( *i );
		}

	}
	in = out;
	out.clear();
	in.push_back( ' ' );
	for (auto i = in.begin(); i != in.end(); i++) {
		switch (state) {
		case 0:
			if (*i == '$') state = 1;
			else if (*i == '|') state = 6;
			else out.push_back( *i );
			break;
		case 1:
			if (*i == '?') state = 2;
			else if (*i == '@') state = 100;
			else {
				out.push_back( '$' );
				out.push_back( *i );
				state = 0;
			}
			break;
		case 2:
			if (*i == '[') state = 3;
			break;
		case 3:
			if (*i == ']') state = 4;
			break;
		case 4:
			if (*i == '[') state = 5;
			else state = 0;
			break;
		case 5:
			if (*i == ']') state = 0;
			else out.push_back( *i );
			break;
		case 6:
			if (*i == 'c') state = 200;
			else if (*i == 'r') state = 0;
			else {
				out.push_back( '|' );
				out.push_back( *i );
				state = 0;
			}
			break;
		case 100:
			if (*i == 's') state = 101; 
			if (*i == 'v') state = 110;
			break;
		case 101://p
		case 102://e
		case 103://l
		case 104://l
		case 105://d
		case 106://e
		case 107://s
		case 108://c
			state++;
			break;
		case 109:
			if (*i >= '0' && *i <= '9') {
				spelldesc *= 10;
				spelldesc += ( int ) ( *i - '0' );
			} else {
				out.append( func_spell_desc( spelldesc ) );
				i--;
				spelldesc = 0;
				state = 0;
			}
			break;
		case 110://e
		case 111://r
		case 112://s
		case 113://a
		case 114://d
		case 115://m
			state++;
			break;
		case 116://g
			out.append( "$@versadmg" );
			state = 0;
			break;
		case 200:
		case 201:
		case 202:
		case 203:
		case 204:
		case 205:
		case 206:
			state++;
			break;
		case 207:
			state = 0;
			break;
		}
		

	}
	return out;
};*/