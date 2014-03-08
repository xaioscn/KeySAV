// -----------------------------------------------------------------------
// <copyright file="TrainerAttribute.cs" company="Project Pokemon">
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
    public class TrainerAttribute
    {
        /// <summary>
        /// Gets the Trainer Shiny Value (TSV)
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        public static string TrainerShinyValue(byte[] buff)
        {
            uint TID = (uint)(buff[0x0C] + buff[0x0D] * 0x100);
            uint SID = (uint)(buff[0x0E] + buff[0x0F] * 0x100);
            uint TSV = (TID ^ SID) >> 4;
            return TSV.ToString("0000");
        }
    }
}
