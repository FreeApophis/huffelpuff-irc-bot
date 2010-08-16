/*
 *  UT3GlobalStatsPlugin, Access to the GameSpy Stats for UT3
 * 
 *  Copyright (c) 2007-2010 Thomas Bruderer <apophis@apophis.ch> <http://www.apophis.ch>
 *  Copyright (c) 2005,2006 Luigi Auriemma <aluigi@autistici.org> <http://aluigi.org>
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>. 
 */

using System;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Huffelpuff.Utils;

namespace Plugin
{
    // Ported from gslist by Luigi Auriemma, therfore all of this is GPL
    // the code is completly rewritten to use the .NET framework and not doing
    // everything on hands and knees. Thats why the code looks much shorter.
    // If you want to look in the original code look at : http://aluigi.altervista.org/papers.htm (gslogincheck)
    public class GameSpyClient
    {
        private const string Host = "gpcm.gamespy.com";
        private const int Port = 29900;
        private readonly Random randomN = new Random();
        private const string Spaces48 = "                                                ";
        private const int BufferSize = 2048;
        private TcpClient client;
        private NetworkStream ns;
        private string sessionkey;
        public string GetTicket(string nick, string pass, bool verbose)
        {
            var buffer = new byte[BufferSize];
            try
            {
                client = new TcpClient(Host, Port);
            }
            catch (SocketException)
            {
                Log.Instance.Log("No connection: check internet connection and open port tcp 29900", Level.Fail);
                return null;
            }
            ns = client.GetStream();
            int len = ns.Read(buffer, 0, BufferSize);
            string receive1 = Encoding.ASCII.GetString(buffer, 0, len);
            string serverChallenge = GetParameterValue(receive1, "challenge");
            if (string.IsNullOrEmpty(serverChallenge)) { Log.Instance.Log("no challenge", Level.Trace); }
            string clientChallenge = CreateRandomString(32);
            string response = GetResponseValue(nick, pass, clientChallenge, serverChallenge);
            string login = "\\login\\" +
                           "\\challenge\\" + clientChallenge +
                           "\\uniquenick\\" + nick +
                           "\\partnerid\\0" +
                           "\\response\\" + response +
                           "\\port\\" + randomN.Next(32768) +
                           "\\productid\\11097" +
                           "\\gamename\\ut3pc" +
                           "\\namespaceid\\64" +
                           "\\sdkrevision\\3" +
                           "\\id\\1" +
                           "\\final\\";
            buffer = Encoding.ASCII.GetBytes(login);
            ns.Write(buffer, 0, login.Length);
            buffer = new byte[BufferSize];
            len = ns.Read(buffer, 0, BufferSize);
            string receive2 = Encoding.ASCII.GetString(buffer, 0, len);
            if (!string.IsNullOrEmpty(GetParameterValue(receive2, "errmsg")))
                Log.Instance.Log(GetParameterValue(receive2, "errmsg"));
            sessionkey = GetParameterValue(receive2, "sesskey");
            return GetParameterValue(receive2, "lt");
        }

        public void LogOut()
        {
            string logout = "\\logout\\" +
                            "\\sesskey\\" + sessionkey +
                            "\\final\\";
            byte[] buffer = Encoding.ASCII.GetBytes(logout);
            if (ns != null)
                ns.Write(buffer, 0, logout.Length);
        }

        static readonly char[] Alphanumeric = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 
                            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
                            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
                            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
                            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};

        private string CreateRandomString(int length)
        {
            string s = "";

            while (length > 0)
            {
                --length;
                s += Alphanumeric[randomN.Next(62)];
            }
            return s;
        }

        private static string GetParameterValue(string message, string parameter)
        {
            char[] backslash = { '\\' };
            string[] parts = message.Split(backslash);
            bool next = false;
            foreach (string part in parts)
            {
                if (next) { return part; }
                if (part == parameter) { next = true; }
            }
            return "";
        }
        static readonly string[] BtoH = { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "0a", "0b", "0c", "0d", "0e", "0f", 
                                  "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "1a", "1b", "1c", "1d", "1e", "1f", 
                                  "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "2a", "2b", "2c", "2d", "2e", "2f", 
                                  "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "3a", "3b", "3c", "3d", "3e", "3f", 
                                  "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "4a", "4b", "4c", "4d", "4e", "4f", 
                                  "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "5a", "5b", "5c", "5d", "5e", "5f", 
                                  "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "6a", "6b", "6c", "6d", "6e", "6f", 
                                  "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "7a", "7b", "7c", "7d", "7e", "7f", 
                                  "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "8a", "8b", "8c", "8d", "8e", "8f", 
                                  "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "9a", "9b", "9c", "9d", "9e", "9f", 
                                  "a0", "a1", "a2", "a3", "a4", "a5", "a6", "a7", "a8", "a9", "aa", "ab", "ac", "ad", "ae", "af", 
                                  "b0", "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8", "b9", "ba", "bb", "bc", "bd", "be", "bf", 
                                  "c0", "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "c9", "ca", "cb", "cc", "cd", "ce", "cf", 
                                  "d0", "d1", "d2", "d3", "d4", "d5", "d6", "d7", "d8", "d9", "da", "db", "dc", "dd", "de", "df", 
                                  "e0", "e1", "e2", "e3", "e4", "e5", "e6", "e7", "e8", "e9", "ea", "eb", "ec", "ed", "ee", "ef", 
                                  "f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8", "f9", "fa", "fb", "fc", "fd", "fe", "ff" };
        private static string GetResponseValue(string nickname, string password, string clientChallenge, string serverChallenge)
        {
            var createMd5 = MD5.Create();
            var passwordHash = createMd5.ComputeHash(Encoding.ASCII.GetBytes(password));
            var pwmd5 = passwordHash.Aggregate("", (current, b) => current + BtoH[b]);
            var finalHash = createMd5.ComputeHash(Encoding.ASCII.GetBytes(pwmd5 + Spaces48 + nickname + clientChallenge + serverChallenge + pwmd5));

            return finalHash.Aggregate("", (current, b) => current + BtoH[b]);
        }
    }
}
