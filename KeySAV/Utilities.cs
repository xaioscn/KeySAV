// -----------------------------------------------------------------------
// <copyright file="Utilities.cs" company="Project Pokemon">
// Copyright © Project Pokemon 2014.
// http://projectpokemon.org/forums/showthread.php?37221
// </copyright>
// -----------------------------------------------------------------------

namespace KeySAV
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class Utilities
    {
        public static string ConvertBytesToString(byte[] buff, int o)
        {
            string charstring;
            charstring = ((char)(buff[o] + buff[o + 1])).ToString();
            for (int i = 1; i <= 12; i++)
            {
                int val = buff[o + 2 * i] + 0x100 * buff[o + 2 * i + 1];
                if (val != 0)
                {
                    charstring += ((char)(val)).ToString();
                }
            }
            return charstring;
        }
    }
}
