using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Text {

    internal unsafe struct TrueTypeFont {

        public UnsafeHashMap<uint, int> glyph_map;

        public void LoadTrueType(byte* bytes, int cnt) { }

        private static bool is_font(byte* ttf) {
            bool res = check_tag(ttf, '1', '\0', '\0', '\0') 
                       || check_tag(ttf, '\0', '1', '\0', '\0')
                       || check_tag(ttf, 't', 'y', 'p', '1')
                       || check_tag(ttf, 'O', 'T', 'T', 'O');
            return res;
        }

        private static bool check_tag(byte* bytes, char a, char b, char c, char d) {
            bool match = bytes[0] == (byte) a &&
                         bytes[1] == (byte) b &&
                         bytes[2] == (byte) c &&
                         bytes[3] == (byte) d;
            return match;
        }

        private static ushort ttf_u16(byte* p) {
            return (ushort) (p[0] * 256 + p[1]);
        }

        private static uint ttf_u32(byte* p) {
            return (uint) ((p[0] << 24) + (p[1] << 16) + (p[2] << 8) + p[3]);
        }

        private static short ttf_i16(byte* p) {
            return (short) (p[0] * 256 + p[1]);
        }

        private static int ttf_i32(byte* p) {
            return (p[0] << 24) + (p[1] << 16) + (p[2] << 8) + p[3];
        }

        private static byte* find_table(byte* ttf, char a, char b, char c, char d) {
            uint num_tables = ttf_u16(ttf + 4);
            byte* table = ttf + 12;

            for (uint itbl = 0; itbl < num_tables; ++itbl) {
                if (check_tag(table, a, b, c, d)) {
                    uint offset = ttf_u32(table + 8);
                    return ttf + offset;
                }

                table += 16;
            }

            return null;
        }

        private static bool fill_cmap(ref TrueTypeFont trueTypeFont, byte* ttf) {
            byte* cmap = find_table(ttf, 'c', 'm', 'a', 'p');
            if (cmap == null) return false;

            uint num_tables = ttf_u16(cmap + 2);
            byte* imap = null;

            for (int itbl = 0; itbl < num_tables; ++itbl) {
                byte* enc_table = cmap + 4 + 8 * itbl;
                ushort platform = ttf_u16(enc_table);
                ushort encoding = ttf_u16(enc_table + 2);
                uint offset = ttf_u32(enc_table + 4);

                if (platform == 0) { // Unicode
                    imap = cmap + offset;
                    break;
                }

                if (platform == 3) { // MS
                    if (encoding == 1 || encoding == 10) {
                        imap = cmap + offset;
                        break;
                    }
                }
            }

            if (imap == null) return false;

            ushort format = ttf_u16(imap);

            if (format == 0) {
                byte* idx_data = imap + 6;
                for (uint i = 1; i < 256; ++i) {
                    int idx = (int) (idx_data[i]);
                    trueTypeFont.glyph_map.Add(i, idx);
                }

                return true;

            }
            else if (format == 4) {
                uint seg_count = (uint) (ttf_u16(imap + 6) >> 1);
                byte* end_code = imap + 7 * 2;
                byte* start_code = end_code + 2 + seg_count * 2;
                byte* offset = imap + 8 * 2 + seg_count * 2 * 3;
                byte* delta = imap + 8 * 2 + seg_count * 2 * 2;

                for (uint iseg = 0; iseg < seg_count; iseg++) {
                    uint seg_start = ttf_u16(start_code + iseg * 2);
                    uint seg_end = ttf_u16(end_code + iseg * 2);
                    uint seg_offset = ttf_u16(offset + iseg * 2);
                    int seg_delta = ttf_i16(delta + iseg * 2);

                    if (seg_offset == 0) {
                        for (uint cp = seg_start; cp <= seg_end; ++cp) {
                            int idx = (ushort) (cp + seg_delta);
                            trueTypeFont.glyph_map.Add(cp, idx);
                        }
                    }
                    else {
                        for (uint cp = seg_start; cp <= seg_end; ++cp) {
                            uint item = cp - seg_start;
                            int idx = ttf_i16(offset + iseg * 2 + seg_offset + item * 2);
                            trueTypeFont.glyph_map.Add(cp, idx);
                        }
                    }
                }

                return true;

            }
            else if (format == 6) {
                uint first = ttf_u16(imap + 6);
                uint count = ttf_u16(imap + 8);
                byte* idx_data = imap + 10;

                for (uint i = 0; i < count; ++i) {
                    uint idx = ttf_u16(idx_data + i * 2);
                    trueTypeFont.glyph_map.Add(i + first, (int) idx);
                }

                return true;

            }
            else if (format == 10) {
                uint first_char = ttf_u32(imap + 12);
                uint num_chars = ttf_u32(imap + 16);
                byte* idx_data = imap + 20;

                for (uint i = 0; i < num_chars; ++i) {
                    uint idx = ttf_u16(idx_data + i * 2);
                    trueTypeFont.glyph_map.Add(i + first_char, (int) idx);
                }

                return true;

            }
            else if (format == 12) {
                uint ngroups = ttf_u32(imap + 12);
                byte* sm_group = imap + 16;

                for (uint i = 0; i < ngroups; ++i) {
                    uint start_code = ttf_u32(sm_group);
                    uint end_code = ttf_u32(sm_group + 4);
                    uint start_idx = ttf_u32(sm_group + 8);

                    for (uint icode = start_code; icode <= end_code; ++icode) {
                        uint idx = start_idx + icode - start_code;
                        trueTypeFont.glyph_map.Add(icode, (int) idx);
                    }

                    sm_group += 12;
                }

                return true;

            }
            else if (format == 13) {
                uint ngroups = ttf_u32(imap + 12);
                byte* sm_group = imap + 16;

                for (uint i = 0; i < ngroups; ++i) {
                    uint start_code = ttf_u32(sm_group);
                    uint end_code = ttf_u32(sm_group + 4);
                    uint glyph_idx = ttf_u32(sm_group + 8);

                    for (uint icode = start_code; icode <= end_code; ++icode) {
                        trueTypeFont.glyph_map.Add(icode, (int) glyph_idx);
                    }

                    sm_group += 12;
                }

                return true;
            }

            return false;
        }
        
        // Glyph offset in 'glyf' table

        private static int glyph_loc_offset( int glyph_idx, bool is_loc32, byte *loca ) {
            uint off0, off1;
            if ( is_loc32 ) {
                off0 = ttf_u32( loca + glyph_idx * 4 );
                off1 = ttf_u32( loca + glyph_idx * 4 + 4 );
            } else {
                off0 = ttf_u16( loca + glyph_idx * 2 ) * 2u;
                off1 = ttf_u16( loca + glyph_idx * 2 + 2 ) * 2u;
            }

            return off0 == off1 ? -1 : (int) off0;
        }

    }

}