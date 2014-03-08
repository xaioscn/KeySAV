// -----------------------------------------------------------------------
// <copyright file="Form1.cs" company="Project Pokemon">
// Copyright © Project Pokemon 2014.
// http://projectpokemon.org/forums/showthread.php?37221
// </copyright>
// -----------------------------------------------------------------------

namespace KeySAV
{
    using System;
    using System.IO;
    using System.Windows.Forms;

    public partial class KeySAV : Form
    {
        // Init
        public KeySAV()
        {
            if (COMPILEMODE != "Private")
            {
                this.Name = "KeySAV Public";
            }

            InitializeComponent();
            B_OpenKey.Enabled = false;
            B_OpenEKX.Enabled = false;
            B_DumpEKX.Enabled = false;
            B_DumpKey.Enabled = false;
            T_KeyOffset.Enabled = false;
            T_Key2Offset.Enabled = false;
            B_GetKey2.Enabled = false;
            B_DumpBoxKey.Enabled = false;
            B_DumpBoxEKXs.Enabled = false;
            T_BoxOffset.Enabled = false;
            B_FindOffset.Enabled = false;
            T_OutPath.Text = System.Windows.Forms.Application.StartupPath;
            CB_Box.SelectedIndex = 0;
            CHK_ALT.Checked = true;
            CHK_ALT.Enabled = false;
            CB_Box.Enabled = false;
            refreshoffset();
            C_Format.SelectedIndex = 0;
            if (COMPILEMODE != "Private")
            {
                // Public Mode
                // Hide Latter 2 Tabs
                TC.TabPages.Remove(Tab_Foreign2Key);
                TC.TabPages.Remove(Tab_RipEKX);

                // Hide Tab2's Advanced Functions
                CHK_ALT.Visible = false;
                label3.Visible = false;
                T_BoxOffset.Visible = false;
                B_DumpBoxKey.Visible = false;

                // Hide Tab1's Advanced Info
                T_Nick.Visible = false;
                label7.Visible = false;
                label8.Visible = false;
                label9.Visible = false;
                label10.Visible = false;
                label11.Visible = false;
                T_0xE0.Visible = false;
                T_0xE1.Visible = false;
                T_0xE2.Visible = false;
                T_0xE3.Visible = false;
                T_OutPath.Visible = false;
                B_ChangeOutputFolder.Visible = false;
            }
            else // Private Mode
            {
                // Allow Dumping
                C_Format.Items.Add("Dump");
            }

            CB_Box.Items.AddRange(new object[] {
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30",
            "31",
            "32"});
        }

        // Global Stuff
        public string COMPILEMODE = "Private";
        public byte[] savefile = new byte[0x10009C];
        public byte[] boxfile = new byte[0x10009C];
        public byte[] save1 = new byte[0x10009C];
        public byte[] save2 = new byte[0x10009C];
        public byte[] keystream = new byte[232];
        public byte[] boxkey = new byte[6960]; // 232*30
        public byte[] keystream1 = new byte[6960]; // 232*30
        public byte[] keystream2 = new byte[6960]; // 232*30
        public byte[] key2 = new byte[232];
        public byte[] ekx = new byte[232];
        public byte[] ekx1 = new byte[232];
        public byte[] ekx2 = new byte[232];
        public byte[] blankekx = new byte[232];
        public byte[] break1 = new byte[0x10009C];
        public byte[] break2 = new byte[0x10009C];
        public int[] offset = new int[] { 0, 0 };
        public string binsave = "Digital Save File|*.sav|DPS Save File|*.bin";
        public byte[] boxbreakblank = new byte[232];
        public string modestring = string.Empty;

        // Offset Prepopulation
        private void refreshoffset()
        {
            uint os = 0;
            uint zor = 0;
            if (CHK_ALT.Checked == false)
            {
                zor = 1;
            }
            else
            {
                zor = 0;
            }

            if (boxfile.Length == 0x100000)
            {
                // Headerless
                os += ((uint)(CB_Box.SelectedIndex) * (232 * 30)) + 0xA6A00 - zor * 0x7F000;
            }
            else
            {
                os += ((uint)(CB_Box.SelectedIndex) * (232 * 30)) + 0xA6A9C - zor * 0x7F000;
            }

            T_BoxOffset.Text = os.ToString("X");
        }

        private int getbox(int offset)
        {
            int box = 0;
            if (break1.Length == 0x100000)
            {
                // Digital Copy or Headerless Rip
                box = (offset - 0xA6A00) / (232 * 30);
            }
            else
            {
                // Powersaves
                box = (offset - 0xA6A9C) / (232 * 30);
            }

            return (box + 1);
        }

        // Data Manipulation

        /// <summary>
        /// Converts the string representation of a number in a specified base to an equivalent 32-bit unsigned integer. Values of <c>null</c> or Empty are converted to 0.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="b">The base of the input.</param>
        /// <returns>A 32-bit unsigned integer that is equivalent to the number in value, or 0 (zero) if value is empty or null.</returns>
        public static uint ToUInt32(string value, int b)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            return Convert.ToUInt32(value, b);
        }

        private int getCloc(uint ec)
        {
            // Define Shuffle Order Structure
            var aloc = new byte[] { 0, 0, 0, 0, 0, 0, 1, 1, 2, 3, 2, 3, 1, 1, 2, 3, 2, 3, 1, 1, 2, 3, 2, 3 };
            var bloc = new byte[] { 1, 1, 2, 3, 2, 3, 0, 0, 0, 0, 0, 0, 2, 3, 1, 1, 3, 2, 2, 3, 1, 1, 3, 2 };
            var cloc = new byte[] { 2, 3, 1, 1, 3, 2, 2, 3, 1, 1, 3, 2, 0, 0, 0, 0, 0, 0, 3, 2, 3, 2, 1, 1 };
            var dloc = new byte[] { 3, 2, 3, 2, 1, 1, 3, 2, 3, 2, 1, 1, 3, 2, 3, 2, 1, 1, 0, 0, 0, 0, 0, 0 };
            uint sv = (((ec & 0x3E000) >> 0xD) % 24);

            int clocation = cloc[sv];
            return clocation;
        }

        private int getDloc(uint ec)
        {
            // Define Shuffle Order Structure
            var aloc = new byte[] { 0, 0, 0, 0, 0, 0, 1, 1, 2, 3, 2, 3, 1, 1, 2, 3, 2, 3, 1, 1, 2, 3, 2, 3 };
            var bloc = new byte[] { 1, 1, 2, 3, 2, 3, 0, 0, 0, 0, 0, 0, 2, 3, 1, 1, 3, 2, 2, 3, 1, 1, 3, 2 };
            var cloc = new byte[] { 2, 3, 1, 1, 3, 2, 2, 3, 1, 1, 3, 2, 0, 0, 0, 0, 0, 0, 3, 2, 3, 2, 1, 1 };
            var dloc = new byte[] { 3, 2, 3, 2, 1, 1, 3, 2, 3, 2, 1, 1, 3, 2, 3, 2, 1, 1, 0, 0, 0, 0, 0, 0 };
            uint sv = (((ec & 0x3E000) >> 0xD) % 24);

            int dlocation = dloc[sv];
            return dlocation;
        }

        private static uint LCRNG(uint seed)
        {
            uint a = 0x41C64E6D;
            uint c = 0x00006073;

            seed = (seed * a + c) & 0xFFFFFFFF;
            return seed;
        }

        private uint getchecksum(byte[] pkx)
        {
            uint chk = 0;
            for (int i = 8; i < 232; i += 2) // Loop through the entire PKX
            {
                chk += (uint)(pkx[i] + pkx[i + 1] * 0x100);
            }
            return chk & 0xFFFF;
        }

        private static uint CEXOR(uint seed)
        {
            uint a = 0xDEADBABE;
            uint c = 0x2B9A7B1E;

            seed = (seed * a + c) & 0xFFFFFFFF;
            return seed;
        }

        // Custom Encryption
        private byte[] da(byte[] array)
        {
            if (COMPILEMODE == "Private")
            {
                return array;
            }
            else
            {
                // Returns the Encrypted/Decrypted Array of Data
                int al = array.Length;

                // Set Encryption Seed
                uint eseed = (uint)(array[al - 4] + array[al - 3] * 0x100 + array[al - 2] * 0x10000 + array[al - 1] * 0x10000000);
                byte[] nca = new byte[al];

                // Get our XORCryptor
                uint xc = CEXOR(eseed);
                uint xc0 = (xc & 0xFF);
                uint xc1 = ((xc >> 8) & 0xFF);
                uint xc2 = ((xc >> 16) & 0xFF);
                uint xc3 = ((xc >> 24) & 0xFF);

                // Fill Our New Array
                for (int i = 0; i < (al - 4); i += 4)
                {
                    nca[i + 0] = (byte)(xc0 ^ array[i + 0]);
                    nca[i + 1] = (byte)(xc1 ^ array[i + 1]);
                    nca[i + 2] = (byte)(xc2 ^ array[i + 2]);
                    nca[i + 3] = (byte)(xc3 ^ array[i + 3]);
                }

                // Return the Seed
                nca[al - 4] = array[al - 4];
                nca[al - 3] = array[al - 3];
                nca[al - 2] = array[al - 2];
                nca[al - 1] = array[al - 1];

                return nca;
            }
        }

        // Array Manipulation
        private byte[] unshufflearray(byte[] pkx, uint sv)
        {
            byte[] ekx = new byte[260];
            for (int i = 0; i < 8; i++)
            {
                ekx[i] = pkx[i];
            }

            // Now to shuffle the blocks

            // Define Shuffle Order Structure
            var aloc = new byte[] { 0, 0, 0, 0, 0, 0, 1, 1, 2, 3, 2, 3, 1, 1, 2, 3, 2, 3, 1, 1, 2, 3, 2, 3 };
            var bloc = new byte[] { 1, 1, 2, 3, 2, 3, 0, 0, 0, 0, 0, 0, 2, 3, 1, 1, 3, 2, 2, 3, 1, 1, 3, 2 };
            var cloc = new byte[] { 2, 3, 1, 1, 3, 2, 2, 3, 1, 1, 3, 2, 0, 0, 0, 0, 0, 0, 3, 2, 3, 2, 1, 1 };
            var dloc = new byte[] { 3, 2, 3, 2, 1, 1, 3, 2, 3, 2, 1, 1, 3, 2, 3, 2, 1, 1, 0, 0, 0, 0, 0, 0 };

            // Get Shuffle Order
            var shlog = new byte[] { aloc[sv], bloc[sv], cloc[sv], dloc[sv] };

            // UnShuffle Away!
            for (int b = 0; b < 4; b++)
            {
                for (int i = 0; i < 56; i++)
                {
                    ekx[8 + 56 * b + i] = pkx[8 + 56 * shlog[b] + i];
                }
            }

            // Fill the Battle Stats back
            if (pkx.Length > 232)
            {
                for (int i = 232; i < 260; i++)
                {
                    ekx[i] = pkx[i];
                }
            }
            return ekx;
        }

        private byte[] shufflearray(byte[] pkx, uint sv)
        {
            byte[] ekx = new byte[260];
            for (int i = 0; i < 8; i++)
            {
                ekx[i] = pkx[i];
            }

            // Now to shuffle the blocks

            // Define Shuffle Order Structure
            var aloc = new byte[] { 0, 0, 0, 0, 0, 0, 1, 1, 2, 3, 2, 3, 1, 1, 2, 3, 2, 3, 1, 1, 2, 3, 2, 3 };
            var bloc = new byte[] { 1, 1, 2, 3, 2, 3, 0, 0, 0, 0, 0, 0, 2, 3, 1, 1, 3, 2, 2, 3, 1, 1, 3, 2 };
            var cloc = new byte[] { 2, 3, 1, 1, 3, 2, 2, 3, 1, 1, 3, 2, 0, 0, 0, 0, 0, 0, 3, 2, 3, 2, 1, 1 };
            var dloc = new byte[] { 3, 2, 3, 2, 1, 1, 3, 2, 3, 2, 1, 1, 3, 2, 3, 2, 1, 1, 0, 0, 0, 0, 0, 0 };

            // Get Shuffle Order
            var shlog = new byte[] { aloc[sv], bloc[sv], cloc[sv], dloc[sv] };

            // Shuffle Away!
            for (int b = 0; b < 4; b++)
            {
                for (int i = 0; i < 56; i++)
                {
                    ekx[8 + 56 * shlog[b] + i] = pkx[8 + 56 * b + i];
                }
            }

            // Fill the Battle Stats back
            if (pkx.Length > 232)
            {
                for (int i = 232; i < 260; i++)
                {
                    ekx[i] = pkx[i];
                }
            }
            return ekx;
        }

        private byte[] decryptarray(byte[] ekx)
        {
            byte[] pkx = ekx;
            uint pv = (uint)ekx[0] + (uint)((ekx[1] << 8)) + (uint)((ekx[2]) << 16) + (uint)((ekx[3]) << 24);
            uint sv = (((pv & 0x3E000) >> 0xD) % 24);

            uint seed = pv;

            // Decrypt Blocks with RNG Seed
            for (int i = 8; i < 232; i += 2)
            {
                int pre = pkx[i] + ((pkx[i + 1]) << 8);
                seed = LCRNG(seed);
                int seedxor = (int)((seed) >> 16);
                int post = (pre ^ seedxor);
                pkx[i] = (byte)((post) & 0xFF);
                pkx[i + 1] = (byte)(((post) >> 8) & 0xFF);
            }

            // Deshuffle
            pkx = unshufflearray(pkx, sv);

            // Decrypt the Party Stats
            seed = pv;
            for (int i = 232; i < 260; i += 2)
            {
                int pre = pkx[i] + ((pkx[i + 1]) << 8);
                seed = LCRNG(seed);
                int seedxor = (int)((seed) >> 16);
                int post = (pre ^ seedxor);
                pkx[i] = (byte)((post) & 0xFF);
                pkx[i + 1] = (byte)(((post) >> 8) & 0xFF);
            }

            return pkx;
        }

        private byte[] encryptarray(byte[] pkx)
        {
            // Shuffle
            uint pv = (uint)pkx[0] + (uint)((pkx[1] << 8)) + (uint)((pkx[2]) << 16) + (uint)((pkx[3]) << 24);
            uint sv = (((pv & 0x3E000) >> 0xD) % 24);

            var encrypt_sv = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 12, 18, 13, 19, 8, 10, 14, 20, 16, 22, 9, 11, 15, 21, 17, 23 };

            sv = encrypt_sv[sv];

            byte[] ekx = shufflearray(pkx, sv);

            uint seed = pv;

            // Encrypt Blocks with RNG Seed
            for (int i = 8; i < 232; i += 2)
            {
                int pre = ekx[i] + ((ekx[i + 1]) << 8);
                seed = LCRNG(seed);
                int seedxor = (int)((seed) >> 16);
                int post = (pre ^ seedxor);
                ekx[i] = (byte)((post) & 0xFF);
                ekx[i + 1] = (byte)(((post) >> 8) & 0xFF);
            }

            // Encrypt the Party Stats
            seed = pv;
            for (int i = 232; i < 260; i += 2)
            {
                int pre = ekx[i] + ((ekx[i + 1]) << 8);
                seed = LCRNG(seed);
                int seedxor = (int)((seed) >> 16);
                int post = (pre ^ seedxor);
                ekx[i] = (byte)((post) & 0xFF);
                ekx[i + 1] = (byte)(((post) >> 8) & 0xFF);
            }

            // Done
            return ekx;
        }



        // Toggle Enabled Stuff & Update
        private void toggle_main1(object sender, EventArgs e)
        {
            B_OpenEKX.Enabled = true;
            B_OpenKey.Enabled = true;
            T_KeyOffset.Enabled = true;
        }

        private void toggle_getekx(object sender, EventArgs e)
        {
            if ((T_key.Text != string.Empty) && (T_KeyOffset.Text != string.Empty))
            {
                B_DumpEKX.Enabled = true;
            }
            else
            {
                B_DumpEKX.Enabled = false;
            }

            if ((T_key.Text != string.Empty) && (T_KeyOffset.Text != string.Empty) && (T_ekx.Text != string.Empty))
            {
                T_FixKey.Enabled = true;
            }
            else
            {
                T_FixKey.Enabled = false;
            }
        }

        private void toggle_getkey(object sender, EventArgs e)
        {
            if ((T_ekx.Text != string.Empty) && (T_KeyOffset.Text != string.Empty))
            {
                B_DumpKey.Enabled = true;
            }
            else
            {
                B_DumpKey.Enabled = false;
            }

            if ((T_key.Text != string.Empty) && (T_KeyOffset.Text != string.Empty) && (T_ekx.Text != string.Empty))
            {
                T_FixKey.Enabled = true;
            }
            else
            {
                T_FixKey.Enabled = false;
            }

            if ((T_ekx.Text != string.Empty))
            {
                B_FixEKX.Enabled = true;
            }
            else
            {
                B_FixEKX.Enabled = false;
            }
        }

        private void toggle_secondary(object sender, EventArgs e)
        {
            if ((T_Open1.Text != string.Empty))
            {
                B_OpenKey.Enabled = true;
                B_OpenEKX.Enabled = true;
                T_KeyOffset.Enabled = true;
            }
            else
            {
                B_OpenKey.Enabled = false;
                B_OpenEKX.Enabled = false;
                T_KeyOffset.Enabled = false;
            }
        }

        private void toggle_key2(object sender, EventArgs e)
        {
            if ((T_S1.Text != string.Empty) && (T_E1.Text != string.Empty) && (T_S2.Text != string.Empty) && (T_E2.Text != string.Empty))
            {
                T_Key2Offset.Enabled = true;
                B_GetKey2.Enabled = true;
            }
            else
            {
                T_Key2Offset.Enabled = false;
                B_GetKey2.Enabled = false;
            }
            if ((T_S1.Text != string.Empty) && (T_S2.Text != string.Empty))
            {
                B_FindOffset.Enabled = true;
            }
        }

        private void togglebox(object sender, EventArgs e)
        {
            if ((T_Blank.Text != string.Empty) && (T_BoxSAV.Text != string.Empty))
            {
                // Enable dumping of Key
                B_DumpBoxKey.Enabled = true;
                CHK_ALT.Enabled = true;
                CB_Box.Enabled = true;
            }
            else
            {
                // Disable dumping of Key
                B_DumpBoxKey.Enabled = false;
                CHK_ALT.Enabled = false;
                CB_Box.Enabled = false;
            }

            if (((T_BoxKey.Text != string.Empty) && (T_BoxSAV.Text != string.Empty)) && ((T_Blank.Text != string.Empty) && (T_BoxSAV.Text != string.Empty)))
            {
                B_DumpBoxEKXs.Enabled = true;
                T_BoxOffset.Enabled = true;
                CHK_ALT.Enabled = true;
                CB_Box.Enabled = true;
            }
            else
            {
                B_DumpBoxEKXs.Enabled = false;
                CHK_ALT.Enabled = false;
                CB_Box.Enabled = false;
            }
            refreshoffset();
        }

        private void togglebreak(object sender, EventArgs e)
        {
            if ((T_OBreak1.Text != string.Empty) && (T_OBreak2.Text != string.Empty))
            {
                B_DoBreak.Enabled = true;
            }
        }

        private void toggle_fixkey(object sender, EventArgs e)
        {
            if (T_FixKey.Text != string.Empty)
            {
                B_FixKey.Enabled = true;
            }
            else
            {
                B_FixKey.Enabled = false;
            }
        }

        private void CHK_ALT_CheckedChanged(object sender, EventArgs e)
        {
            refreshoffset();
        }

        private void changebox(object sender, EventArgs e)
        {
            refreshoffset();
        }

        // Tab Page 1 I/O - Dump Box Keystream
        private void B_OBreak1_Click(object sender, EventArgs e)
        {
            // Open Save File 1
            OpenFileDialog opensave1 = new OpenFileDialog();
            opensave1.Filter = binsave;

            if (opensave1.ShowDialog() == DialogResult.OK)
            {
                string path = opensave1.FileName;
                break1 = File.ReadAllBytes(path);
                T_OBreak1.Text = path;
            }
        }

        private void B_OBreak2_Click(object sender, EventArgs e)
        {
            // Open Save File 2
            OpenFileDialog opensave2 = new OpenFileDialog();
            opensave2.Filter = binsave;

            if (opensave2.ShowDialog() == DialogResult.OK)
            {
                string path = opensave2.FileName;
                break2 = File.ReadAllBytes(path);
                T_OBreak2.Text = path;
            }
        }

        private void B_DoBreak_Click(object sender, EventArgs e)
        {
            // Do Break. Let's first do some sanity checking to find out the 2 offsets we're dumping from.
            // Loop through save file to find
            int fo = 0xA0000; // Initial Offset, can tweak later.
            int success = 0;
            string result = string.Empty;

            for (int d = 0; d < 2; d++)
            {
                // Do this twice to get both box offsets.
                for (int i = fo; i < 0xEE000; i++)
                {
                    int err = 0;

                    // Start at findoffset and see if it matches pattern
                    if ((break1[i + 4] == break2[i + 4]) && (break1[i + 4 + 232] == break2[i + 4 + 232]))
                    {
                        // Sanity Placeholders are the same
                        for (int j = 0; j < 4; j++)
                        {
                            if (break1[i + j] == break2[i + j])
                            {
                                err++;
                            }
                        }

                        if (err < 4)
                        {
                            // Keystream ^ PID doesn't match entirely. Keep checking.
                            for (int j = 8; j < 232; j++)
                            {
                                if (break1[i + j] == break2[i + j])
                                {
                                    err++;
                                }
                            }

                            if (err < 20)
                            {
                                // Tolerable amount of difference between offsets. We have a result.
                                offset[d] = i;
                                break;
                            }
                        }
                    }
                }

                fo = offset[d] + 232 * 30;  // Fast forward out of this box to find the next.
            }

            // Now that we have our two box offsets...
            // Check to see if we actually have them.
            if ((offset[0] == 0) || (offset[1] == 0))
            {
                // We have a problem. Don't continue.
                result = "Unable to Find Box.";
            }
            else
            {
                // Let's go deeper. We have the two box offsets.
                // Chunk up the base streams.
                byte[,] estream1 = new byte[30, 232];
                byte[,] estream2 = new byte[30, 232];

                // Stuff 'em.
                for (int i = 0; i < 30; i++)    // Times we're iterating
                {
                    for (int j = 0; j < 232; j++)   // Stuff the Data
                    {
                        estream1[i, j] = break1[offset[0] + 232 * i + j];
                        estream2[i, j] = break2[offset[1] + 232 * i + j];
                    }
                }

                // Okay, now that we have the encrypted streams, formulate our EKX.
                byte[] empty = new byte[232];
                string nick = T_Nick.Text;
                for (int i = 0; i < 24; i += 2) // Stuff in the nickname to our blank EKX.
                {
                    int val = 0;
                    try
                    {
                        val = (int)((char)nick[i / 2]);
                    }
                    catch
                    {
                    }

                    empty[0x40 + i] = (byte)(val & 0xFF);
                    empty[0x40 + i + 1] = (byte)((val >> 8) & 0xFF);
                }

                // Encrypt the Empty PKX to EKX.
                byte[] emptyekx = new byte[232];
                Array.Copy(empty, emptyekx, 232);
                emptyekx = encryptarray(emptyekx);

                // Sweet. Now we just have E0-E3 and the Checksum as unknown values. Let's get our polluted streams from each.
                // Save file 1 has empty box 1. Save file 2 has empty box 2.
                byte[,] pstream1 = new byte[30, 232];
                byte[,] pstream2 = new byte[30, 232];
                for (int i = 0; i < 30; i++)    // Times we're iterating
                {
                    for (int j = 0; j < 232; j++)   // Stuff the Data
                    {
                        pstream1[i, j] = (byte)(estream1[i, j] ^ emptyekx[j]);
                        pstream2[i, j] = (byte)(estream2[i, j] ^ emptyekx[j]);
                    }
                }

                // Cool. So we have a fairly decent keystream to roll with. We now need to find what the E0-E3 region is.
                // 0x00000000 Encryption Constant has the D block last. 
                // We need to make sure our Supplied Encryption Constant Pokemon have the D block somewhere else (Pref in 1 or 3).

                // First, let's get out our polluted EKX's.
                byte[,] polekx = new byte[6, 232];
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 232; j++)
                    {   // Save file 1 has them in the second box. XOR them out with the Box2 Polluted Stream
                        polekx[i, j] = (byte)(break1[offset[1] + 232 * i + j] ^ pstream2[i, j]);
                    }
                }

                uint[] encryptionconstants = new uint[6];
                int valid = 0;
                for (int i = 0; i < 6; i++)
                {
                    encryptionconstants[i] = (uint)polekx[i, 0];
                    encryptionconstants[i] += (uint)polekx[i, 1] * 0x100;
                    encryptionconstants[i] += (uint)polekx[i, 2] * 0x10000;
                    encryptionconstants[i] += (uint)polekx[i, 3] * 0x1000000;

                    // EC Obtained. Check to see if Block D is not last.
                    if (getDloc(encryptionconstants[i]) != 3)
                    {
                        valid++;

                        // Find the Origin/Region data.
                        byte[] encryptedekx = new byte[232];
                        byte[] decryptedpkx = new byte[232];
                        for (int z = 0; z < 232; z++)
                        {
                            encryptedekx[z] = polekx[i, z];
                        }
                        decryptedpkx = decryptarray(encryptedekx);

                        // Dump the relevant data to the Masked Textboxes.
                        T_0xE0.Text = decryptedpkx[0xE0].ToString();
                        T_0xE1.Text = decryptedpkx[0xE1].ToString();
                        T_0xE2.Text = decryptedpkx[0xE2].ToString();
                        T_0xE3.Text = decryptedpkx[0xE3].ToString();

                        // Dump it into our Blank EKX. We have won!
                        empty[0xE0] = decryptedpkx[0xE0];
                        empty[0xE1] = decryptedpkx[0xE1];
                        empty[0xE2] = decryptedpkx[0xE2];
                        empty[0xE3] = decryptedpkx[0xE3];
                        break;
                    }
                }

                if (valid == 0)
                {
                    // We didn't get any valid EC's where D was not in last. Tell the user to try again with different specimens.
                    result = "The 6 supplied Pokemon are not suitable. \r\nRip new saves with 6 different ones that originated from your save file.";
                }
                else
                {
                    // We can continue to get our actual keystream.
                    // Let's calculate the actual checksum of our empty pkx.
                    uint chk = 0;
                    for (int i = 8; i < 232; i += 2) // Loop through the entire PKX
                    {
                        chk += (uint)(empty[i] + empty[i + 1] * 0x100);
                    }

                    // Apply New Checksum
                    empty[0x06] = (byte)(chk & 0xFF);
                    empty[0x07] = (byte)((chk >> 8) & 0xFF);

                    // Okay. So we're now fixed with the proper blank PKX. Encrypt it!
                    Array.Copy(empty, emptyekx, 232);
                    emptyekx = encryptarray(emptyekx);

                    // Empty EKX obtained. Time to get our keystreams!
                    // Save file 1 is empty in box 1. Save file 2 is empty in box 2.
                    for (int i = 0; i < 30; i++)    // Times we're iterating
                    {
                        for (int j = 0; j < 232; j++)   // Stuff the Data
                        {
                            keystream1[i * 232 + j] = (byte)(estream1[i, j] ^ emptyekx[j]);
                            keystream2[i * 232 + j] = (byte)(estream2[i, j] ^ emptyekx[j]);
                        }
                    }

                    // We're done. Great job!
                    // Enable the Dump Buttons.
                    B_DumpBreakBox1.Enabled = true;
                    B_DumpBreakBox2.Enabled = true;
                    B_DumpBlank.Enabled = true;
                    success = 1;
                    Array.Copy(emptyekx, boxbreakblank, 232);
                }
            }

            if (success == 1)
            {
                // Success
                result = "Keystreams were successfully bruteforced!\r\n\r\n";
                if (COMPILEMODE == "Private")
                {
                    result += "First Box @ " + offset[0].ToString("X5") + " & \r\n" + "Second Box @ " + offset[1].ToString("X5") + "\r\n\r\n";
                }
                result += "Dumping Enabled:\r\n";
                result += "K1 button - dump the First Box's Keystream.\r\n";
                result += "K2 button - dump the Second Box's Keystream.\r\n";
                result += "Blank button - dump Blank EKX data.\r\n";
            }
            else
            {
                // Failed
                result = "Keystreams were NOT bruteforced!\r\n\r\nStart over and try again :(";
            }

            T_Dialog.Text = result;
        }

        private void B_DumpBreakBox1_Click(object sender, EventArgs e)
        {
            // Dumps the Keystream for Box 1
            // Keystream is already prepared. Prompt saving.
            SaveFileDialog saveboxkey = new SaveFileDialog();
            saveboxkey.Filter = "Keystream|*.bin";
            if (COMPILEMODE == "Private")
            {
                saveboxkey.FileName = offset[0].ToString("X") + " - Box" + getbox(offset[0]) + ".bin";
            }
            else
            {
                saveboxkey.FileName = "Key - Box" + getbox(offset[0]) + ".bin";
            }
            if (saveboxkey.ShowDialog() == DialogResult.OK)
            {
                string path = saveboxkey.FileName;
                File.WriteAllBytes(path, da(keystream1));
            }
        }

        private void B_DumpBreakBox2_Click(object sender, EventArgs e)
        {
            // Dumps the Keystream for Box 2
            // Keystream is already prepared. Prompt saving.
            SaveFileDialog saveboxkey = new SaveFileDialog();
            saveboxkey.Filter = "Keystream|*.bin";

            if (COMPILEMODE == "Private")
            {
                saveboxkey.FileName = offset[1].ToString("X") + " - Box" + getbox(offset[1]) + ".bin";
            }
            else
            {
                saveboxkey.FileName = "Key - Box" + getbox(offset[1]) + ".bin";
            }

            if (saveboxkey.ShowDialog() == DialogResult.OK)
            {
                string path = saveboxkey.FileName;
                File.WriteAllBytes(path, da(keystream2));
            }
        }

        private void B_DumpBlank_Click(object sender, EventArgs e)
        {
            // Dumps the Keystream for Box 2
            // Keystream is already prepared. Prompt saving.
            SaveFileDialog saveboxkey = new SaveFileDialog();
            saveboxkey.Filter = "Blank EKX|*.ekx";
            saveboxkey.FileName = "Blank.ekx";
            if (saveboxkey.ShowDialog() == DialogResult.OK)
            {
                string path = saveboxkey.FileName;
                File.WriteAllBytes(path, da(boxbreakblank));
            }
        }

        // Tab Page 2 I/O - Dump Box Contents
        private void B_OpenBoxSave_Click(object sender, EventArgs e)
        {
            // Open Save File
            OpenFileDialog boxsave = new OpenFileDialog();
            boxsave.Filter = binsave;

            if (boxsave.ShowDialog() == DialogResult.OK)
            {
                string path = boxsave.FileName;
                boxfile = File.ReadAllBytes(path);
                T_BoxSAV.Text = path;
            }
        }

        private void B_OpenBoxKey_Click(object sender, EventArgs e)
        {
            // Open Key File
            OpenFileDialog boxkeyfile = new OpenFileDialog();
            boxkeyfile.Filter = "Keystream|*.bin";

            if (boxkeyfile.ShowDialog() == DialogResult.OK)
            {
                string path = boxkeyfile.FileName;
                boxkey = da(File.ReadAllBytes(path));
                T_BoxKey.Text = path;
            }
        }

        private void B_OpenBlank_Click(object sender, EventArgs e)
        {
            // Open Key File
            OpenFileDialog openblankekx = new OpenFileDialog();
            openblankekx.Filter = "EKX|*.ekx";

            if (openblankekx.ShowDialog() == DialogResult.OK)
            {
                string path = openblankekx.FileName;
                blankekx = da(File.ReadAllBytes(path));
                T_Blank.Text = path;
            }
        }

        private void B_ChangeOutputFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                T_OutPath.Text = fbd.SelectedPath;
            }
        }

        private void B_DumpBoxKey_Click(object sender, EventArgs e)
        {
            // Create new Keystream
            byte[] newboxkey = new byte[232 * 30];

            // Fill Key
            uint offset = ToUInt32(T_BoxOffset.Text, 16);
            for (int i = 0; i < (30 * 232); i++)
            {
                newboxkey[i] = (byte)(boxfile[offset + i] ^ blankekx[i % 232]);
            }

            // Keystream is prepared. Prompt saving.
            SaveFileDialog saveboxkey = new SaveFileDialog();
            saveboxkey.Filter = "Keystream|*.bin";
            saveboxkey.FileName = T_BoxOffset.Text + " - Box" + (CB_Box.SelectedIndex + 1) + ".bin";
            if (saveboxkey.ShowDialog() == DialogResult.OK)
            {
                string path = saveboxkey.FileName;
                File.WriteAllBytes(path, newboxkey);
            }
        }

        private void B_DumpBoxEKXs_Click(object sender, EventArgs e)
        {
            string result = string.Empty;
            int valid = 0;
            int errors = 0;
            string errstr = string.Empty;
            string corruptedindex = string.Empty;
            if (T_BoxOffset.Text == string.Empty)
            {
                // Need an offset.
                ////MessageBox.Show("No offset entered.", "Error");
                T_Dialog.Text = "Error: No offset entered. Stopping.";
            }
            else
            {
                // Dump Data
                ////try
                {
                    string dumppath = T_OutPath.Text;
                    uint offset = ToUInt32(T_BoxOffset.Text, 16);
                    if (boxkey.Length < (232 * 30))
                    {
                        ////MessageBox.Show("Incorrect Box Keystream Length.", "Error");
                        T_Dialog.Text = "Error: Incorrect Box Keystream Length. Stopping.";
                    }
                    else
                    {
                        // Loop through all 30 to dump
                        byte[] boxekx = new byte[232];
                        byte[] oldboxkey = new byte[232 * 30];
                        for (int i = 0; i < (232 * 30); i++)
                        {
                            oldboxkey[i] = boxkey[i];
                        }
                        byte[] blankpkx = new byte[232];
                        for (int i = 0; i < (232); i++)
                        {
                            blankpkx[i] = blankekx[i];
                        }
                        blankpkx = decryptarray(blankpkx);
                        for (int i = 0; i < 30; i++)
                        {
                            for (int j = 0; j < 232; j++)
                            {
                                boxekx[j] = (byte)(boxfile[offset + i * 232 + j] ^ oldboxkey[i * 232 + j]);
                            }

                            // Okay, we have the data. Let's get some data out for a proper filename.
                            // Decrypt the data
                            byte[] esave = new byte[232];
                            for (int j = 0; j < 232; j++)
                            {
                                esave[j] = boxekx[j];
                            }

                            byte[] pkxdata = decryptarray(boxekx);
                            uint checksum = getchecksum(pkxdata);
                            uint actualsum = (uint)(pkxdata[0x06] + pkxdata[0x07] * 0x100);
                            if (checksum != actualsum)
                            {
                                ////MessageBox.Show("Keystream Corruption detected for Index " + i + ". Fixing keystream.", "Error");
                                corruptedindex += (i + 1) + " - Keystream Corruption Detected\r\n";
                                ////File.WriteAllBytes(dumppath + "\\error"+i+".bin", esave);
                                for (int c = i * 232; c < (i + 1) * 232; c++)
                                {
                                    boxkey[c] = (byte)(oldboxkey[c] ^ blankpkx[c % 232]);
                                }

                                byte[] fixedekx = new byte[232];

                                // Get actual data now
                                for (int j = 0; j < 232; j++)
                                {
                                    fixedekx[j] = (byte)(boxkey[i * 232 + j] ^ boxfile[offset + i * 232 + j]);
                                }
                                for (int z = 0; z < 232; z++)
                                {
                                    pkxdata[z] = fixedekx[z];
                                    esave[z] = fixedekx[z];
                                }
                                pkxdata = decryptarray(pkxdata);
                                checksum = getchecksum(pkxdata);
                                actualsum = (uint)(pkxdata[0x06] + pkxdata[0x07] * 0x100);
                                if (checksum != actualsum)
                                {
                                    ////MessageBox.Show("Keystream correction failed for " + i + ". :(");
                                    errors++;

                                    errstr += "@" + (i + 1) + " - CHK Key Invalid" + "\r\n";

                                    // Undo our changes
                                    for (int z = 0; z < (232 * 30); z++)
                                    {
                                        boxkey[z] = (byte)(oldboxkey[z]);
                                    }
                                    continue;
                                }
                                else
                                {   // Save our changes
                                    ////MessageBox.Show("Keystream correction passed.");
                                    corruptedindex += (i + 1) + " - Keystream Corruption Fixed!\r\n";
                                    if (!File.Exists(T_BoxKey.Text + ".bak"))
                                    {
                                        File.WriteAllBytes(T_BoxKey.Text + ".bak", oldboxkey);
                                    }
                                    File.WriteAllBytes(T_BoxKey.Text, da(boxkey));
                                }
                            }

                            // Get PID, ShinyValue and Species Name
                            uint PID = (uint)(pkxdata[0x18] + pkxdata[0x19] * 0x100 + pkxdata[0x1A] * 0x10000 + pkxdata[0x1B] * 0x1000000);
                            uint ShinyValue = (((PID & 0xFFFF) ^ (PID >> 16)) >> 4);
                            int species = pkxdata[0x08] + pkxdata[0x09] * 0x100;
                            if (species > 0)
                            {
                                string specname = PokemonAttribute.LookupSpecies(species);
                                if (specname == "Error")
                                {
                                    ////MessageBox.Show("Error on index " + i, "Error");
                                    errors++;
                                    errstr += "@" + (i + 1).ToString("0000") + " - Species Index: " + species + "\r\n";
                                }

                                {
                                    string boxLocation = (i / 6 + 1) + "," + (i % 6 + 1);

                                    if (C_Format.SelectedIndex == 0)
                                    {
                                        // Default
                                        string filename =
                                        boxLocation
                                        + " - "
                                        + specname
                                        + PokemonAttribute.GetGender(pkxdata)
                                        + " - "
                                        + PokemonAttribute.GetNature(pkxdata)
                                        + " - "
                                        + PokemonAttribute.GetAbility(pkxdata)
                                        + " - "
                                        + PokemonAttribute.getivs(pkxdata, ShinyValue);
                                        result += "\r\n" + filename;
                                    }
                                    else if (C_Format.Text == "Reddit")
                                    {
                                        // Reddit
                                        modestring = "\r\n| Box | Name | Nature | Ability | Spread | SV\r\n|:--|:--|:--|:--|:--|:--";
                                        string resultline =
                                            "| " + boxLocation +
                                            " | " + specname + PokemonAttribute.GetGender(pkxdata) +
                                            " | " + PokemonAttribute.GetNature(pkxdata) +
                                            " | " + PokemonAttribute.GetAbility(pkxdata) +
                                            " | " + PokemonAttribute.getivs2(pkxdata, ShinyValue) +
                                            " |"
                                            ;
                                        result += "\r\n" + resultline;
                                    }
                                    else if (C_Format.Text == "TSV")
                                    {
                                        // TSV Checking Mode
                                        modestring = "\r\n|Slot | Species | OT | TID | TSV\r\n|:--|:--|:--|:--|:--";
                                        string resultline =
                                            "| " + boxLocation + // Slot
                                            " | " + specname + PokemonAttribute.GetGender(pkxdata) + // Species
                                            " | " + Utilities.ConvertBytesToString(pkxdata, 0xB0) + // OT
                                            " | " + ((uint)(pkxdata[0x0C] + pkxdata[0x0D] * 0x100)).ToString("00000") + // TID
                                            " | " + TrainerAttribute.TrainerShinyValue(pkxdata) +
                                            " |"
                                            ;
                                        result += "\r\n" + resultline;
                                    }
                                    else if (C_Format.Text == "Dump")
                                    {
                                        // Private Dumper
                                        string filename =
                                        boxLocation
                                        + " - "
                                        + specname
                                        + PokemonAttribute.GetGender(pkxdata)
                                        + " - "
                                        + PokemonAttribute.GetNature(pkxdata)
                                        + " - "
                                        + PokemonAttribute.GetAbility(pkxdata)
                                        + " - "
                                        + PokemonAttribute.getivs(pkxdata, ShinyValue);
                                        string path = dumppath + "\\" + filename + ".ekx";
                                        result += "\r\n" + filename;
                                        File.WriteAllBytes(path, esave);
                                    }
                                    valid++;
                                }
                            }
                        }

                        // Load the old boxkey as the new one, in case we made any new alterations.
                        for (int i = 0; i < (232 * 30); i++)
                        {
                            oldboxkey[i] = boxkey[i];
                        }

                        if (result == string.Empty)
                        {
                            result = "Nothing was dumped.";
                        }

                        if (valid > 0)
                        {
                            if (errors > 0)
                            {
                                MessageBox.Show("Partial Dump :|", "Alert");
                            }
                            MessageBox.Show("Successful Dump!", "Alert");
                        }

                        try
                        {
                            Clipboard.SetText(modestring + result);
                        }
                        catch
                        {
                        }

                        T_Dialog.Text = string.Empty;
                        if (C_Format.Text == "Dump")
                        {
                            T_Dialog.Text += "All EKX's dumped to:\n" + dumppath + "\r\n\r\n";
                        }
                        T_Dialog.Text += "Dumped info copied to Clipboard!\r\n";
                        T_Dialog.Text += "Total Dumped: " + valid + "\r\n";
                        T_Dialog.Text += "Empty Slots: " + (30 - valid - errors) + "\r\n";

                        if ((corruptedindex != string.Empty) && (COMPILEMODE == "Private"))
                        {
                            T_Dialog.Text += corruptedindex;
                        }

                        if (errstr != string.Empty)
                        {
                            T_Dialog.Text += errstr;
                        }

                        if (errors > 0)
                        {
                            T_Dialog.Text += "Errors: " + errors + "\r\n";
                        }

                        T_Dialog.Text += "\r\nData Dumped: ";
                        T_Dialog.Text += modestring;
                        T_Dialog.Text += result;
                        valid = 0;
                    }
                }
                ////catch (Exception ex)
                ////{
                ////    string message = "Error while dumping:\n\n" + ex + "\n\nDid you enter everything properly? If not, fix it!";
                ////    string caption = "Error";
                ////    MessageBox.Show(message, caption);
                ////}
            }
        }

        // Tab Page 3 I/O - Native EKX
        private void B_OpenSave_Click(object sender, EventArgs e)
        {
            // Open Save File
            OpenFileDialog opensave = new OpenFileDialog();
            opensave.Filter = binsave;

            if (opensave.ShowDialog() == DialogResult.OK)
            {
                string path = opensave.FileName;
                savefile = File.ReadAllBytes(path);
                T_Open1.Text = path;
            }
        }

        private void B_OpenEKX_Click(object sender, EventArgs e)
        {
            // Open EKX
            OpenFileDialog openekx = new OpenFileDialog();
            openekx.Filter = "EKX|*.ekx";

            if (openekx.ShowDialog() == DialogResult.OK)
            {
                string path = openekx.FileName;
                ekx = File.ReadAllBytes(path);
                T_ekx.Text = path;
            }
        }

        private void B_OpenKey_Click(object sender, EventArgs e)
        {
            // Open Keystream
            OpenFileDialog openkey = new OpenFileDialog();
            openkey.Filter = "Keystream|*.bin";

            if (openkey.ShowDialog() == DialogResult.OK)
            {
                string path = openkey.FileName;
                keystream = File.ReadAllBytes(path);
                T_key.Text = path;
            }
        }

        private void B_DumpEKX_Click(object sender, EventArgs e)
        {
            // Save Data
            if ((T_Open1.Text != string.Empty) && (T_key.Text != string.Empty))
            {
                uint offset = ToUInt32(T_KeyOffset.Text, 16);
                byte[] ekxdata = new byte[232];
                for (uint i = offset; i < (offset + 232); i++)
                {
                    ekxdata[i - offset] = (byte)(savefile[i] ^ keystream[i - offset]);
                }
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "EKX|*.ekx";
                byte[] esave = new byte[232];
                Array.Copy(ekxdata, esave, 232);
                byte[] pkxdata = decryptarray(ekxdata);

                // Get PID, ShinyValue and Species Name
                uint PID = (uint)(pkxdata[0x18] + pkxdata[0x19] * 0x100 + pkxdata[0x1A] * 0x10000 + pkxdata[0x1B] * 0x1000000);
                uint ShinyValue = (((PID & 0xFFFF) ^ (PID >> 16)) >> 4);
                int species = pkxdata[0x08] + pkxdata[0x09] * 0x100;
                string specname = PokemonAttribute.LookupSpecies(species);
                string dumppath = ShinyValue + " - " + specname + ".ekx";
                save.FileName = dumppath;

                if (save.ShowDialog() == DialogResult.OK)
                {
                    string path = save.FileName;
                    File.WriteAllBytes(path, esave);
                }
            }
            else
            {
                string message = "Did not load Save1/Save2/Keystream. Try again!";
                string caption = "Error - LoadedData";
                MessageBox.Show(message, caption);
            }
        }

        private void B_DumpKey_Click(object sender, EventArgs e)
        {
            // Save Data
            if ((T_Open1.Text != string.Empty) && (T_ekx.Text != string.Empty))
            {
                uint offset = ToUInt32(T_KeyOffset.Text, 16);
                byte[] data = new byte[232];
                for (uint i = offset; i < (offset + 232); i++)
                {
                    data[i - offset] = (byte)(savefile[i] ^ ekx[i - offset]);
                }
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Keystream|*.bin";
                save.FileName = T_KeyOffset.Text + ".bin";

                if (save.ShowDialog() == DialogResult.OK)
                {
                    string path = save.FileName;
                    File.WriteAllBytes(path, data);
                }
            }
            else
            {
                string message = "Did not load Save1/Save2/EKX. Try again!";
                string caption = "Error - LoadedData";
                MessageBox.Show(message, caption);
            }
        }

        private void B_FixKey_Click(object sender, EventArgs e)
        {
            // savefile stores our save
            // keystream stores our key
            // ekx stores our ekx

            uint fixindex = ToUInt32(T_FixKey.Text, 16);
            uint saveoffset = ToUInt32(T_KeyOffset.Text, 16);

            // Copy over keystream to new var
            byte[] newstream = new byte[6960];
            Array.Copy(keystream, newstream, 6960);

            for (int i = 0; i < 232; i++)
            {
                newstream[fixindex * 232 + i] = (byte)(ekx[i] ^ savefile[i + saveoffset + fixindex * 232]);
            }

            SaveFileDialog savenewstream = new SaveFileDialog();
            string fn = T_KeyOffset.Text + " - BoxFixed@" + fixindex + ".bin";
            savenewstream.FileName = fn;
            if (savenewstream.ShowDialog() == DialogResult.OK)
            {
                // Save Keystream
                string path = savenewstream.FileName;
                File.WriteAllBytes(path, newstream);
            }
        }

        private void B_FixEKX_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Loaded EKX is the EKX you want to fix, yes? If so, press OK and when prompted load the Blank EKX", "Prompt");
            OpenFileDialog openblankfix = new OpenFileDialog();
            openblankfix.FileName = "blank.ekx";
            openblankfix.Filter = "Blank EKX|*.ekx";

            if (openblankfix.ShowDialog() == DialogResult.OK)
            {
                byte[] fixingekx = new byte[232];
                byte[] newekx = new byte[232];
                string path = openblankfix.FileName;
                fixingekx = File.ReadAllBytes(path);
                byte[] fixingpkx = decryptarray(fixingekx);
                for (int i = 0; i < 232; i++)
                {
                    newekx[i] = (byte)(ekx[i] ^ fixingpkx[i]);
                }

                // New EKX is prepared. Prompt saving.
                SaveFileDialog savenewekx = new SaveFileDialog();
                savenewekx.Filter = "EKX|*.ekx";
                savenewekx.FileName = "fixed.ekx";
                if (savenewekx.ShowDialog() == DialogResult.OK)
                {
                    path = savenewekx.FileName;
                    File.WriteAllBytes(path, newekx);
                }
            }
        }

        // Tab Page 4 I/O - Foreign EKX
        private void B_S1_Click(object sender, EventArgs e)
        {
            // Open Save File 1
            OpenFileDialog opensave1 = new OpenFileDialog();
            opensave1.Filter = binsave;

            if (opensave1.ShowDialog() == DialogResult.OK)
            {
                string path = opensave1.FileName;
                save1 = File.ReadAllBytes(path);
                T_S1.Text = path;
            }
        }

        private void B_E1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openekx1 = new OpenFileDialog();
            openekx1.Filter = "EKX|*.ekx";

            if (openekx1.ShowDialog() == DialogResult.OK)
            {
                string path = openekx1.FileName;
                ekx1 = File.ReadAllBytes(path);
                T_E1.Text = path;
            }
        }

        private void B_S2_Click(object sender, EventArgs e)
        {
            // Open Save File 2
            OpenFileDialog opensave2 = new OpenFileDialog();
            opensave2.Filter = binsave;

            if (opensave2.ShowDialog() == DialogResult.OK)
            {
                string path = opensave2.FileName;
                save2 = File.ReadAllBytes(path);
                T_S2.Text = path;
            }
        }

        private void B_E2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openekx2 = new OpenFileDialog();
            openekx2.Filter = "EKX|*.ekx";

            if (openekx2.ShowDialog() == DialogResult.OK)
            {
                string path = openekx2.FileName;
                ekx2 = File.ReadAllBytes(path);
                T_E2.Text = path;
            }
        }

        private void B_FindOffset_Click(object sender, EventArgs e)
        {
            // Find the offset of swapped EKX's
            // The data should be as follows:
            // *6 different
            // *2 same
            // *0xE0 different
            // *2 pattern

            // Loop through save file to find
            int fo = 0xA0; // Initial Offset, can tweak later.
            string res = string.Empty;
            for (int i = fo; i < 0xEB000; i++)
            {
                int err = 0;

                // Start at findoffset and see if it matches pattern
                if ((save1[i + 4] == save2[i + 4]) && (save1[i + 4 + 232] == save2[i + 4 + 232]))
                {
                    // Unused Pads are the same
                    for (int j = 0; j < 4; j++)
                    {
                        if (save1[i + j] == save2[i + j])
                        {
                            err++;
                        }
                    }

                    if (err < 4)
                    {
                        // Keystream ^ PID doesn't match entirely. Keep checking.
                        for (int j = 8; j < 232; j++)
                        {
                            if (save1[i + j] == save2[i + j])
                            {
                                err++;
                            }
                        }

                        if (err < 20)
                        {
                            // Tolerable amount of difference between offsets. We have a result.
                            if (res != string.Empty)
                            {
                                res += ", ";
                            }
                            res += i.ToString("X5");
                            i++;
                        }
                    }
                }
            }

            if (res == string.Empty)
            {
                res = "No result found";
            }

            T_Dialog.Text = res;
        }

        private void B_DumpKey2_Click(object sender, EventArgs e)
        {
            // Logic to get the key!
            // Need 2 EKX's to have the C block in different positions. Let's check!
            // Get the Encryption Constants of Each PID
            uint ec1 = (uint)(ekx1[0x0] + ekx1[0x1] * 0x100 + ekx1[0x2] * 0x10000 + ekx1[0x3] * 0x1000000);
            uint ec2 = (uint)(ekx2[0x0] + ekx2[0x1] * 0x100 + ekx2[0x2] * 0x10000 + ekx2[0x3] * 0x1000000);

            if (getCloc(ec1) != getCloc(ec2))
            {
                // Blocks aren't in the same position. Great! Let's continue.
                uint saveoffset = ToUInt32(T_Key2Offset.Text, 16);

                // Get the two keystreams.
                byte[] data1 = new byte[232];
                for (uint i = saveoffset; i < (saveoffset + 232); i++)
                {
                    data1[i - saveoffset] = (byte)(save1[i] ^ ekx1[i - saveoffset]);
                }

                byte[] data2 = new byte[232];
                for (uint i = saveoffset; i < (saveoffset + 232); i++)
                {
                    data2[i - saveoffset] = (byte)(save2[i] ^ ekx2[i - saveoffset]);
                }

                // Sanity check time. Check to see if the keystream matches for the first part.
                int fails = 0;
                for (int i = 0; i < 8; i++)
                {
                    if (data1[i] != data2[i])
                    {
                        fails++;
                    }
                }
                if (fails != 0)
                {
                    // Keystream doesn't match. User error.
                    MessageBox.Show("Calculated keystream doesn't match. EKXs/SAVs/Offset are likely incorrect.");
                }
                else
                {
                    // Keystream Matches the first part. Let's continue!
                    for (int i = 0; i < 8; i++)
                    {
                        key2[i] = data1[i];
                    }

                    // Copy over the first keystream, skipping the C block.
                    for (int i = 0; i < 4; i++)
                    {
                        if (getCloc(ec1) != i)
                        {
                            for (int j = 0; j < 56; j++)
                            {
                                key2[0x8 + i * 56 + j] = data1[0x8 + i * 56 + j];
                            }
                        }
                    }

                    // Copy over the second keystream, skipping the C block.
                    for (int i = 0; i < 4; i++)
                    {
                        if (getCloc(ec2) != i)
                        {
                            for (int j = 0; j < 56; j++)
                            {
                                key2[0x8 + i * 56 + j] = data2[0x8 + i * 56 + j];
                            }
                        }
                    }

                    // Keystream is prepared. Prompt saving.
                    SaveFileDialog savekey2 = new SaveFileDialog();
                    savekey2.Filter = "Keystream|*.bin";
                    savekey2.FileName = T_Key2Offset.Text + ".bin";
                    if (savekey2.ShowDialog() == DialogResult.OK)
                    {
                        string path = savekey2.FileName;
                        File.WriteAllBytes(path, key2);
                    }
                }
            }
            else
            {
                // They are in the same position. They can't be used together or else we'll get a corrupted keystream.
                string message = "The two EKX's supplied have the same C block shuffled offset. Find different PKX's.";
                string caption = "Error";
                MessageBox.Show(message, caption);
            }
        }

        // About Button
        private void B_About_Click(object sender, EventArgs e)
        {
            string message = "KeySAV - By Kaphotics and OmegaDonut\n\nhttp://projectpokemon.org/";
            message += "\r\n\r\nContact @ Forums or IRC.";
            if (COMPILEMODE == "Public")
            {
                message += "\r\n\r\nPublic Version.";
            }

            string caption = "About";
            MessageBox.Show(message, caption);
        }
    }
}
