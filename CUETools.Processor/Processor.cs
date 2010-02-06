// ****************************************************************************
// 
// CUE Tools
// Copyright (C) 2006-2007  Moitah (moitah@yahoo.com)
// 
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
// 
// ****************************************************************************

using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Security.Cryptography;
using System.Threading;
using System.Xml;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using HDCDDotNet;
using CUETools.Codecs;
using CUETools.Codecs.LossyWAV;
using CUETools.CDImage;
using CUETools.AccurateRip;
using CUETools.Ripper.SCSI;
using MusicBrainz;
using Freedb;
#if !MONO
using UnRarDotNet;
#endif
using ICSharpCode.SharpZipLib.Zip;
using CSScriptLibrary;

namespace CUETools.Processor
{

	public enum AudioEncoderType
	{
		Lossless,
		Hybrid,
		Lossy,
		/// <summary>
		/// No Audio
		/// </summary>
		NoAudio,
	}

	public enum CUEAction
	{
		Encode = 0,
		Verify = 1,
		CreateDummyCUE = 2,
		CorrectFilenames = 3
	}

	public enum CUEStyle
	{
		/// <summary>
		/// Single file with embedded CUE
		/// </summary>
		SingleFileWithCUE,
		/// <summary>
		/// Single file with external CUE
		/// </summary>
		SingleFile,
		/// <summary>
		/// Gaps prepended file-per-track
		/// </summary>
		GapsPrepended,
		/// <summary>
		/// Gaps appended (noncompliant) file-per-track
		/// </summary>
		GapsAppended,
		/// <summary>
		/// Gaps left out file-per-track
		/// </summary>
		GapsLeftOut
	}

	public static class General {
		public static CUELine FindCUELine(List<CUELine> list, string command) {
			command = command.ToUpper();
			foreach (CUELine line in list) {
				if (line.Params[0].ToUpper() == command) {
					return line;
				}
			}
			return null;
		}

		public static CUELine FindCUELine(List<CUELine> list, string command, string command2)
		{
			command = command.ToUpper();
			command2 = command2.ToUpper();
			foreach (CUELine line in list)
			{
				if (line.Params.Count > 1 && line.Params[0].ToUpper() == command && line.Params[1].ToUpper() == command2)
				{
					return line;
				}
			}
			return null;
		}

		//public static CUELine FindCUELine(List<CUELine> list, string [] commands)
		//{
		//    foreach (CUELine line in list)
		//    {
		//        if (line.Params.Count < commands.Length)
		//            continue;
		//        for (int i = 0; i < commands.Length; i++)
		//        {
		//            if (line.Params[i].ToUpper() != commands[i].ToUpper())
		//                break;
		//            if (i == commands.Length - 1)
		//                return line;
		//        }
		//    }
		//    return null;
		//}

		public static void SetCUELine(List<CUELine> list, string command, string value, bool quoted)
		{
			CUELine line = General.FindCUELine(list, command);
			if (line == null)
			{
				line = new CUELine();
				line.Params.Add(command); line.IsQuoted.Add(false);
				line.Params.Add(value); line.IsQuoted.Add(quoted);
				list.Add(line);
			}
			else
			{
				while (line.Params.Count > 1)
				{
					line.Params.RemoveAt(1);
					line.IsQuoted.RemoveAt(1);
				}
				line.Params.Add(value); line.IsQuoted.Add(quoted);
			}
		}

		public static void SetCUELine(List<CUELine> list, string command, string command2, string value, bool quoted)
		{
			CUELine line = General.FindCUELine(list, command, command2);
			if (line == null)
			{
				line = new CUELine();
				line.Params.Add(command); line.IsQuoted.Add(false);
				line.Params.Add(command2); line.IsQuoted.Add(false);
				line.Params.Add(value); line.IsQuoted.Add(quoted);
				list.Add(line);
			}
			else
			{
				while (line.Params.Count > 2)
				{
					line.Params.RemoveAt(2);
					line.IsQuoted.RemoveAt(2);
				}
				line.Params.Add(value); line.IsQuoted.Add(quoted);
			}
		}

		public static void DelCUELine(List<CUELine> list, string command, string command2)
		{
			CUELine line = General.FindCUELine(list, command, command2);
			if (line == null)
				return;
			list.Remove(line);
		}

		public static void DelCUELine(List<CUELine> list, string command)
		{
			CUELine line = General.FindCUELine(list, command);
			if (line == null)
				return;
			list.Remove(line);
		}

		class TitleFormatFunctionInfo
		{
			public string func;
			public List<int> positions;
			public List<bool> found;

			public TitleFormatFunctionInfo(string _func, int position)
			{
				func = _func;
				positions = new List<int>();
				found = new List<bool>();
				NextArg(position);
			}

			public void Found()
			{
				found[found.Count - 1] = true;
			}

			public void NextArg(int position)
			{
				positions.Add(position);
				found.Add(false);
			}

			public string GetArg(StringBuilder sb, int no)
			{
				return sb.ToString().Substring(positions[no], 
					((no == positions.Count - 1) ? sb.Length : positions[no + 1]) - positions[no]);
			}

			public int GetIntArg(StringBuilder sb, int no)
			{
				int res;
				return int.TryParse(GetArg(sb, no), out res) ? res : 0;
			}

			void Returns(StringBuilder sb, string res)
			{
				sb.Length = positions[0];
				sb.Append(res);
			}

			public bool Finalise(StringBuilder sb)
			{
				switch (func)
				{
					case "[":
						if (positions.Count != 1)
							return false;
						if (!found[0])
							sb.Length = positions[0];
						return true;
					case "if":
						if (positions.Count != 3)
							return false;
						Returns(sb, GetArg(sb, found[0] ? 1 : 2));
						return true;
					case "if2":
						if (positions.Count != 2)
							return false;
						Returns(sb, GetArg(sb, found[0] ? 0 : 1));
						return true;
					case "if3":
						if (positions.Count < 1)
							return false;
						for (int argno = 0; argno < positions.Count; argno++)
							if (found[argno] || argno == positions.Count - 1)
							{
								Returns(sb, GetArg(sb, argno));
								return true;
							}
						return false;
					case "ifgreater":
						if (positions.Count != 4)
							return false;
						Returns(sb, GetArg(sb, (GetIntArg(sb, 0) > GetIntArg(sb, 1)) ? 2 : 3));
						return true;
					case "iflonger":
						if (positions.Count != 4)
							return false;
						Returns(sb, GetArg(sb, (GetArg(sb, 0).Length > GetIntArg(sb, 1)) ? 2 : 3));
						return true;
					case "ifequal":
						if (positions.Count != 4)
							return false;
						Returns(sb, GetArg(sb, (GetIntArg(sb, 0) == GetIntArg(sb, 1)) ? 2 : 3));
						return true;
					case "len":
						if (positions.Count != 1)
							return false;
						Returns(sb, GetArg(sb, 0).Length.ToString());
						return true;
					case "max":
						if (positions.Count != 2)
							return false;
						Returns(sb, Math.Max(GetIntArg(sb, 0), GetIntArg(sb, 1)).ToString());
						return true;
					case "directory":
						if (positions.Count != 1 && positions.Count != 2 && positions.Count != 3)
							return false;
						try
						{
							int arg3 = positions.Count > 1 ? GetIntArg(sb, 1) : 1;
							int arg2 = positions.Count > 2 ? GetIntArg(sb, 2) : arg3;
							Returns(sb, General.GetDirectoryElements(Path.GetDirectoryName(GetArg(sb, 0)), -arg2, -arg3));
						}
						catch { return false; }
						return true;
					case "directory_path":
						if (positions.Count != 1)
							return false;
						try { Returns(sb, Path.GetDirectoryName(GetArg(sb, 0))); }
						catch { return false; }
						return true;
					case "ext":
						if (positions.Count != 1)
							return false;
						try { Returns(sb, Path.GetExtension(GetArg(sb, 0))); }
						catch { return false; }
						return true;
					case "filename":
						if (positions.Count != 1)
							return false;
						try { Returns(sb, Path.GetFileNameWithoutExtension(GetArg(sb, 0))); }
						catch { return false; }
						return true;
				}
				return false;
			}
		}

		public static string GetDirectoryElements(string dir, int first, int last)
		{
			if (dir == null)
				return "";

			string[] dirSplit = dir.Split(Path.DirectorySeparatorChar,
				Path.AltDirectorySeparatorChar);
			int count = dirSplit.Length;

			if ((first == 0) && (last == 0))
			{
				first = 1;
				last = count;
			}

			if (first < 0) first = (count + 1) + first;
			if (last < 0) last = (count + 1) + last;

			if ((first < 1) && (last < 1))
			{
				return String.Empty;
			}
			else if ((first > count) && (last > count))
			{
				return String.Empty;
			}
			else
			{
				int i;
				StringBuilder sb = new StringBuilder();

				if (first < 1) first = 1;
				if (first > count) first = count;
				if (last < 1) last = 1;
				if (last > count) last = count;

				if (last >= first)
				{
					for (i = first; i <= last; i++)
					{
						sb.Append(dirSplit[i - 1]);
						sb.Append(Path.DirectorySeparatorChar);
					}
				}
				else
				{
					for (i = first; i >= last; i--)
					{
						sb.Append(dirSplit[i - 1]);
						sb.Append(Path.DirectorySeparatorChar);
					}
				}

				return sb.ToString(0, sb.Length - 1);
			}
		}

		public static string ReplaceMultiple(string s, NameValueCollection tags)
		{
			List<string> find = new List<string>();
			List<string> replace = new List<string>();

			foreach (string tag in tags.AllKeys)
			{
				string key = '%' + tag.ToLower() + '%';
				string val = tags[tag];
				if (!find.Contains(key) && val != null && val != "")
				{
					find.Add(key);
					replace.Add(val);
				}
			}

			return ReplaceMultiple(s, find, replace);
		}

		public delegate bool CheckIfExists(string output);

		public static string ReplaceMultiple(string fmt, NameValueCollection tags, string unique_key, CheckIfExists exists)
		{
			string result = ReplaceMultiple(fmt, tags);
			if (result == String.Empty || result  == null)
				return result;
			int unique = 1;
			try
			{
				while (exists(result))
				{
					tags[unique_key] = unique.ToString();
					string new_result = ReplaceMultiple(fmt, tags);
					if (new_result == result || new_result == String.Empty || new_result == null)
						break;
					result = new_result;
					unique++;
				}
			}
			catch { }
			return result;
		}

		public static string ReplaceMultiple(string s, List<string> find, List<string> replace)
		{
			if (find.Count != replace.Count)
			{
				throw new ArgumentException();
			}
			StringBuilder sb;
			int iChar, iFind;
			string f;
			bool found;
			List<TitleFormatFunctionInfo> formatFunctions = new List<TitleFormatFunctionInfo>();
			bool quote = false;

			sb = new StringBuilder();

			for (iChar = 0; iChar < s.Length; iChar++)
			{
				found = false;

				if (quote)
				{
					if (s[iChar] == '\'')
					{
						if (iChar > 0 && s[iChar-1] == '\'')
							sb.Append(s[iChar]);
						quote = false;
						continue;
					}
					sb.Append(s[iChar]);
					continue;
				}

				if (s[iChar] == '\'')
				{
					quote = true;
					continue;
				}

				if (s[iChar] == '[')
				{
					formatFunctions.Add(new TitleFormatFunctionInfo("[", sb.Length));
					continue;
				}

				if (s[iChar] == '$')
				{
					int funcEnd = s.IndexOf('(', iChar + 1);
					if (funcEnd < 0)
						return null;
					formatFunctions.Add(new TitleFormatFunctionInfo(s.Substring(iChar + 1, funcEnd - iChar - 1), sb.Length));
					iChar = funcEnd;
					continue;
				}

				if (s[iChar] == ',')
				{
					if (formatFunctions.Count < 1)
						return null;
					formatFunctions[formatFunctions.Count - 1].NextArg(sb.Length);
					continue;
				}

				if (s[iChar] == ']')
				{
					if (formatFunctions.Count < 1 ||
						formatFunctions[formatFunctions.Count - 1].func != "[" 
						|| !formatFunctions[formatFunctions.Count - 1].Finalise(sb))
						return null;
					formatFunctions.RemoveAt(formatFunctions.Count - 1);
					continue;
				}

				if (s[iChar] == ')')
				{
					if (formatFunctions.Count < 1 ||
						formatFunctions[formatFunctions.Count - 1].func == "["
						|| !formatFunctions[formatFunctions.Count - 1].Finalise(sb))
						return null;
					formatFunctions.RemoveAt(formatFunctions.Count - 1);
					continue;
				}

				for (iFind = 0; iFind < find.Count; iFind++)
				{
					f = find[iFind];
					if ((f.Length <= (s.Length - iChar)) && (s.Substring(iChar, f.Length) == f))
					{
						if (formatFunctions.Count > 0)
						{
							if (replace[iFind] != null)
							{
								formatFunctions[formatFunctions.Count - 1].Found();
								sb.Append(replace[iFind]);
							}
						}
						else
						{
							if (replace[iFind] != null)
								sb.Append(replace[iFind]);
							else
								return null;
						}
						iChar += f.Length - 1;
						found = true;
						break;
					}
				}

				if (!found)
				{
					sb.Append(s[iChar]);
				}
			}

			return sb.ToString();
		}

		public static string EmptyStringToNull(string s)
		{
			return ((s != null) && (s.Length == 0)) ? null : s;
		}
	}

	public enum CUEToolsTagger
	{
		TagLibSharp = 0,
		APEv2 = 1,
		ID3v2 = 2,
	}

	public class CUEToolsFormat
	{
		public CUEToolsFormat(
			string _extension,
			CUEToolsTagger _tagger, 
			bool _allowLossless,
			bool _allowLossy,
			bool _allowLossyWAV,
			bool _allowEmbed,
			bool _builtin,
			CUEToolsUDC _encoderLossless,
			CUEToolsUDC _encoderLossy,
			string _decoder)
		{
			extension = _extension;
			tagger = _tagger;
			allowLossless = _allowLossless;
			allowLossy = _allowLossy;
			allowLossyWAV = _allowLossyWAV;
			allowEmbed = _allowEmbed;
			builtin = _builtin;
			encoderLossless = _encoderLossless;
			encoderLossy = _encoderLossy;
			decoder = _decoder;
		}
		public override string ToString()
		{
			return extension;
		}
		public string extension;
		public CUEToolsUDC encoderLossless;
		public CUEToolsUDC encoderLossy;
		public string decoder;
		public CUEToolsTagger tagger;
		public bool allowLossless, allowLossy, allowLossyWAV, allowEmbed, builtin;
	}

	public class CUEToolsUDC : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public CUEToolsUDC(
			string _name,
			string _extension,
			bool _lossless,
			string _supported_modes,
			string _default_mode,
			string _path,
			string _parameters
			)
		{
			name = _name;
			extension = _extension;
			lossless = _lossless;
			supported_modes = _supported_modes;
			default_mode = _default_mode;
			path = _path;
			parameters = _parameters;
			type = null;
		}
		public CUEToolsUDC(AudioEncoderClass enc, Type enctype)
		{
			name = enc.EncoderName;
			extension = enc.Extension;
			lossless = enc.Lossless;
			supported_modes = enc.SupportedModes;
			default_mode = enc.DefaultMode;
			priority = enc.Priority;
			path = null;
			parameters = null;
			type = enctype;
		}
		public CUEToolsUDC(AudioDecoderClass enc, Type dectype)
		{
			name = enc.DecoderName;
			extension = enc.Extension;
			lossless = true;
			supported_modes = "";
			default_mode = "";
			path = null;
			parameters = null;
			type = dectype;
		}
		public override string ToString()
		{
			return name;
		}
		public string name = "";
		public string extension = "wav";
		public string path = "";
		public string parameters = "";
		public Type type = null;
		public string supported_modes = "";
		public string default_mode = "";
		public bool lossless = false;
		public int priority = 0;

		public string Name
		{
			get { return name; }
			set { name = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Name")); }
		}
		public string Path
		{
			get { return path; }
			set { path = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Path")); }
		}
		public string Parameters
		{
			get { return parameters; }
			set { parameters = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Parameters")); }
		}
		public bool Lossless
		{
			get { return lossless; }
			set { lossless = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Lossless")); }
		}
		public string Extension
		{
			get { return extension; }
			set { extension = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Extension")); }
		}
		public string DotExtension
		{
			get { return "." + extension; }
		}
		public string SupportedModesStr
		{
			get { return supported_modes; }
			set { supported_modes = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("SupportedModesStr")); }
		}
		public string[] SupportedModes
		{
			get
			{
				return supported_modes.Split(' ');
			}
		}
		public int DefaultModeIndex
		{
			get
			{
				string[] modes = supported_modes.Split(' ');
				if (modes == null || modes.Length < 2)
					return -1;
				for (int i = 0; i < modes.Length; i++)
					if (modes[i] == default_mode)
						return i;
				return -1;
			}
		}
		public bool CanBeDeleted
		{
			get
			{
				return path != null;
			}
		}
	}

	public class CUEToolsScript
	{
		public CUEToolsScript(string _name, bool _builtin, IEnumerable<CUEAction> _conditions, string _code)
		{
			name = _name;
			builtin = _builtin;
			conditions = new List<CUEAction>();
			foreach(CUEAction condition in _conditions)
				conditions.Add(condition);
			code = _code;
		}
		public override string ToString()
		{
			return name;
		}
		public string name;
		public bool builtin;
		public List<CUEAction> conditions;
		public string code;
	}

	public class CUEToolsUDCList : BindingList<CUEToolsUDC>
	{
		public CUEToolsUDCList() : base()
		{
			AddingNew += OnAddingNew;
		}

		private void OnAddingNew(object sender, AddingNewEventArgs e)
		{
			string name = "new";
			CUEToolsUDC temp;
			while (TryGetValue(name, out temp))
				name += "(1)";
			e.NewObject = new CUEToolsUDC(name, "wav", true, "", "", "", "");
		}

		public bool TryGetValue(string name, out CUEToolsUDC result)
		{
			foreach(CUEToolsUDC udc in this)
				if (udc.name == name)
				{
					result = udc;
					return true;
				}
			result = null;
			return false;
		}

		public CUEToolsUDC GetDefault(string extension, bool lossless)
		{
			CUEToolsUDC result = null;
			foreach (CUEToolsUDC udc in this)
				if (udc.extension == extension && udc.lossless == lossless && (result == null || result.priority < udc.priority))
					result = udc;
			return result;
		}

		public CUEToolsUDC this[string name]
		{
			get
			{
				CUEToolsUDC udc;
				if (!TryGetValue(name, out udc))
					throw new Exception("CUEToolsUDCList: member not found");
				return udc;
			}
		}
	}

	public class CUEConfig {
		public uint fixOffsetMinimumConfidence;
		public uint fixOffsetMinimumTracksPercent;
		public uint encodeWhenConfidence;
		public uint encodeWhenPercent;
		public bool encodeWhenZeroOffset;
		public bool writeArTagsOnVerify;
		public bool writeArLogOnVerify;
		public bool writeArTagsOnEncode;
		public bool writeArLogOnConvert;
		public bool fixOffset;
		public bool noUnverifiedOutput;
		public bool autoCorrectFilenames;
		public bool flacVerify;
		public bool flaCudaVerify;
		public bool flaCudaGPUOnly;
		public bool flaCudaThreads;
		public bool preserveHTOA;
		public int wvExtraMode;
		public bool wvStoreMD5;
		public bool keepOriginalFilenames;
		public string trackFilenameFormat;
		public string singleFilenameFormat;
		public bool removeSpecial;
		public string specialExceptions;
		public bool replaceSpaces;
		public bool embedLog;
		public bool extractLog;
		public bool fillUpCUE;
		public bool overwriteCUEData;
		public bool filenamesANSISafe;
		public bool bruteForceDTL;
		public bool createEACLOG;
		public bool detectHDCD;
		public bool decodeHDCD;
		public bool wait750FramesForHDCD;
		public bool createM3U;
		public bool createTOC;
		public bool createCUEFileWhenEmbedded;
		public bool truncate4608ExtraSamples;
		public int lossyWAVQuality;
		public bool decodeHDCDtoLW16;
		public bool decodeHDCDto24bit;
		public bool disableAsm;
		public bool oneInstance;
		public bool checkForUpdates;
		public string language;
		public Dictionary<string, CUEToolsFormat> formats;
		public CUEToolsUDCList encoders;
		public Dictionary<string, CUEToolsUDC> decoders;
		public Dictionary<string, CUEToolsScript> scripts;
		public string defaultVerifyScript;
		public string defaultEncodeScript;
		public bool writeBasicTagsFromCUEData;
		public bool copyBasicTags;
		public bool copyUnknownTags;
		public bool copyAlbumArt;
		public bool embedAlbumArt;
		public bool extractAlbumArt;
		public bool arLogToSourceFolder;
		public bool arLogVerbose;
		public bool fixOffsetToNearest;
		public int maxAlbumArtSize;
		public string arLogFilenameFormat, alArtFilenameFormat;
		public CUEStyle gapsHandling;

		public bool CopyAlbumArt { get { return copyAlbumArt; } set { copyAlbumArt = value; } }
		public bool FlaCudaThreads { get { return flaCudaThreads; } set { flaCudaThreads = value; } }
		public bool FlaCudaGPUOnly { get { return flaCudaGPUOnly; } set { flaCudaGPUOnly = value; } }
		public bool FlaCudaVerify { get { return flaCudaVerify; } set { flaCudaVerify = value; } }		
		public string ArLogFilenameFormat { get { return arLogFilenameFormat; } set { arLogFilenameFormat = value; }  }
		public string AlArtFilenameFormat { get { return alArtFilenameFormat; } set { alArtFilenameFormat = value; }  }
		public CUEToolsUDCList Encoders
		{
			get { return encoders; }
		}

		public CUEConfig()
		{
			fixOffsetMinimumConfidence = 2;
			fixOffsetMinimumTracksPercent = 51;
			encodeWhenConfidence = 2;
			encodeWhenPercent = 100;
			encodeWhenZeroOffset = false;
			fixOffset = false;
			noUnverifiedOutput = false;
			writeArTagsOnEncode = true;
			writeArLogOnConvert = true;
			writeArTagsOnVerify = false;
			writeArLogOnVerify = true;

			autoCorrectFilenames = true;
			flacVerify = false;
			flaCudaVerify = false;
			flaCudaGPUOnly = false;
			flaCudaThreads = true;
			preserveHTOA = true;
			wvExtraMode = 0;
			wvStoreMD5 = false;
			keepOriginalFilenames = false;
			trackFilenameFormat = "%tracknumber%. %title%";
			singleFilenameFormat = "%filename%";
			removeSpecial = false;
			specialExceptions = "-()";
			replaceSpaces = false;
			embedLog = true;
			extractLog = true;
			fillUpCUE = true;
			overwriteCUEData = false;
			filenamesANSISafe = true;
			bruteForceDTL = false;
			createEACLOG = false;
			detectHDCD = true;
			wait750FramesForHDCD = true;
			decodeHDCD = false;
			createM3U = false;
			createTOC = false;
			createCUEFileWhenEmbedded = true;
			truncate4608ExtraSamples = true;
			lossyWAVQuality = 5;
			decodeHDCDtoLW16 = false;
			decodeHDCDto24bit = true;

			disableAsm = false;
			oneInstance = true;
			checkForUpdates = true;

			writeBasicTagsFromCUEData = true;
			copyBasicTags = true;
			copyUnknownTags = true;
			copyAlbumArt = true;
			embedAlbumArt = true;
			extractAlbumArt = true;
			maxAlbumArtSize = 300;

			arLogToSourceFolder = false;
			arLogVerbose = true;
			fixOffsetToNearest = true;
			arLogFilenameFormat = "%filename%.accurip";
			alArtFilenameFormat = "folder.jpg";

			gapsHandling = CUEStyle.GapsAppended;

			language = Thread.CurrentThread.CurrentUICulture.Name;

			List<Type> encs = new List<Type>();
			List<Type> decs = new List<Type>();
			
			encs.Add(typeof(CUETools.Codecs.WAVWriter));
			encs.Add(typeof(CUETools.Codecs.FLAKE.FlakeWriter));
			encs.Add(typeof(CUETools.Codecs.FlaCuda.FlaCudaWriter));
			encs.Add(typeof(CUETools.Codecs.ALAC.ALACWriter));
			
			decs.Add(typeof(CUETools.Codecs.WAVReader));
			decs.Add(typeof(CUETools.Codecs.FLAKE.FlakeReader));
			decs.Add(typeof(CUETools.Codecs.ALAC.ALACReader));

			//ApplicationSecurityInfo asi = new ApplicationSecurityInfo(AppDomain.CurrentDomain.ActivationContext);
			//string arch = asi.ApplicationId.ProcessorArchitecture;
			//ActivationContext is null most of the time :(

			string arch = Marshal.SizeOf(typeof(IntPtr)) == 8 ? "x64" : "x86";
			string plugins_path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "plugins (" + arch + ")");
			if (Directory.Exists(plugins_path))
				foreach (string plugin_path in Directory.GetFiles(plugins_path, "*.dll", SearchOption.TopDirectoryOnly))
				{
					AssemblyName name = AssemblyName.GetAssemblyName(plugin_path);
					Assembly assembly = Assembly.Load(name);
					foreach (Type type in assembly.GetExportedTypes())
					{
						try
						{
							if (Attribute.GetCustomAttribute(type, typeof(AudioDecoderClass)) != null)
								decs.Add(type);
							//if (type.IsClass && !type.IsAbstract && typeof(IAudioDest).IsAssignableFrom(type))
							if (Attribute.GetCustomAttribute(type, typeof(AudioEncoderClass)) != null)
								encs.Add(type);
						}
						catch (Exception ex)
						{
							System.Diagnostics.Debug.WriteLine(ex.Message);
						}
					}
				}

			encoders = new CUEToolsUDCList();
			foreach (Type type in encs)
			{
				AudioEncoderClass enc = Attribute.GetCustomAttribute(type, typeof(AudioEncoderClass)) as AudioEncoderClass;
				//if (!encoders.TryGetValue(enc.EncoderName))
				encoders.Add(new CUEToolsUDC(enc, type));
			}
			encoders.Add(new CUEToolsUDC("flake", "flac", true, "0 1 2 3 4 5 6 7 8 9 10 11", "10", "flake.exe", "-%M - -o %O -p %P"));
			encoders.Add(new CUEToolsUDC("takc", "tak", true, "0 1 2 2e 2m 3 3e 3m 4 4e 4m", "2", "takc.exe", "-e -p%M -overwrite - %O"));
			encoders.Add(new CUEToolsUDC("ffmpeg alac", "m4a", true, "", "", "ffmpeg.exe", "-i - -f ipod -acodec alac -y %O"));
			encoders.Add(new CUEToolsUDC("lame vbr", "mp3", false, "V9 V8 V7 V6 V5 V4 V3 V2 V1 V0", "V2", "lame.exe", "--vbr-new -%M - %O"));
			encoders.Add(new CUEToolsUDC("lame cbr", "mp3", false, "96 128 192 256 320", "256", "lame.exe", "-m s -q 0 -b %M --noreplaygain - %O"));
			encoders.Add(new CUEToolsUDC("oggenc", "ogg", false, "-1 -0.5 0 0.5 1 1.5 2 2.5 3 3.5 4 4.5 5 5.5 6 6.5 7 7.5 8", "3", "oggenc.exe", "-q %M - -o %O"));
			encoders.Add(new CUEToolsUDC("nero aac", "m4a", false, "0.1 0.2 0.3 0.4 0.5 0.6 0.7 0.8 0.9", "0.4", "neroAacEnc.exe", "-q %M -if - -of %O"));

			decoders = new Dictionary<string, CUEToolsUDC>();
			foreach (Type type in decs)
			{
				AudioDecoderClass dec = Attribute.GetCustomAttribute(type, typeof(AudioDecoderClass)) as AudioDecoderClass;
				decoders.Add(dec.DecoderName, new CUEToolsUDC(dec, type));
			}
			decoders.Add("takc", new CUEToolsUDC("takc", "tak", true, "", "", "takc.exe", "-d %I -"));
			decoders.Add("ffmpeg alac", new CUEToolsUDC("ffmpeg alac", "m4a", true, "", "", "ffmpeg.exe", "%I -f wav -"));

			formats = new Dictionary<string, CUEToolsFormat>();
			formats.Add("flac", new CUEToolsFormat("flac", CUEToolsTagger.TagLibSharp, true, false, true, true, true, encoders.GetDefault("flac", true), null, "libFLAC"));
			formats.Add("wv", new CUEToolsFormat("wv", CUEToolsTagger.TagLibSharp, true, false, true, true, true, encoders.GetDefault("wv", true), null, "libwavpack"));
			formats.Add("ape", new CUEToolsFormat("ape", CUEToolsTagger.TagLibSharp, true, false, false, true, true, encoders.GetDefault("ape", true), null, "MAC_SDK"));
			formats.Add("tta", new CUEToolsFormat("tta", CUEToolsTagger.APEv2, true, false, false, false, true, encoders.GetDefault("tta", true), null, "ttalib"));
			formats.Add("wav", new CUEToolsFormat("wav", CUEToolsTagger.TagLibSharp, true, false, true, false, true, encoders.GetDefault("wav", true), null, "builtin wav"));
			formats.Add("tak", new CUEToolsFormat("tak", CUEToolsTagger.APEv2, true, false, true, true, true, encoders.GetDefault("tak", true), null, "takc"));
			formats.Add("m4a", new CUEToolsFormat("m4a", CUEToolsTagger.TagLibSharp, true, true, false, false, true, encoders.GetDefault("m4a", true), encoders.GetDefault("m4a", false), "builtin alac"));
			formats.Add("mp3", new CUEToolsFormat("mp3", CUEToolsTagger.TagLibSharp, false, true, false, false, true, null, encoders.GetDefault("mp3", false), null));
			formats.Add("ogg", new CUEToolsFormat("ogg", CUEToolsTagger.TagLibSharp, false, true, false, false, true, null, encoders.GetDefault("ogg", false), null));

			scripts = new Dictionary<string, CUEToolsScript>();
			scripts.Add("default", new CUEToolsScript("default", true,
				new CUEAction[] { CUEAction.Verify, CUEAction.Encode },
				"return processor.Go();"));
			scripts.Add("only if found", new CUEToolsScript("only if found", true,
				new CUEAction[] { CUEAction.Verify },
@"if (processor.ArVerify.AccResult != HttpStatusCode.OK)
	return processor.WriteReport(); 
return processor.Go();"));
			scripts.Add("fix offset", new CUEToolsScript("fix offset", true,
				new CUEAction[] { CUEAction.Encode },
@"if (processor.ArVerify.AccResult != HttpStatusCode.OK)
    return processor.WriteReport(); 
processor.WriteOffset = 0;
processor.Action = CUEAction.Verify;
string status = processor.Go();
uint tracksMatch;
int bestOffset;
processor.FindBestOffset(processor.Config.fixOffsetMinimumConfidence, !processor.Config.fixOffsetToNearest, out tracksMatch, out bestOffset);
if (tracksMatch * 100 < processor.Config.fixOffsetMinimumTracksPercent * processor.TrackCount)
    return status;
processor.WriteOffset = bestOffset;
processor.Action = CUEAction.Encode;
//MessageBox.Show(null, processor.AccurateRipLog, " + "\"Done\"" + @"MessageBoxButtons.OK, MessageBoxIcon.Information);
return processor.Go();
"));
			scripts.Add("encode if verified", new CUEToolsScript("encode if verified", true,
				new CUEAction[] { CUEAction.Encode },
@"if (processor.ArVerify.AccResult != HttpStatusCode.OK)
    return processor.WriteReport();
processor.Action = CUEAction.Verify;
string status = processor.Go();
uint tracksMatch;
int bestOffset;
processor.FindBestOffset(processor.Config.encodeWhenConfidence, false, out tracksMatch, out bestOffset);
if (tracksMatch * 100 < processor.Config.encodeWhenPercent * processor.TrackCount || (processor.Config.encodeWhenZeroOffset && bestOffset != 0))
    return status;
processor.Action = CUEAction.Encode;
return processor.Go();
"));
			defaultVerifyScript = "default";
			defaultEncodeScript = "default";
		}

		public void Save (SettingsWriter sw)
		{
			sw.Save("Version", 203);
			sw.Save("ArFixWhenConfidence", fixOffsetMinimumConfidence);
			sw.Save("ArFixWhenPercent", fixOffsetMinimumTracksPercent);
			sw.Save("ArEncodeWhenConfidence", encodeWhenConfidence);
			sw.Save("ArEncodeWhenPercent", encodeWhenPercent);
			sw.Save("ArEncodeWhenZeroOffset", encodeWhenZeroOffset);
			sw.Save("ArNoUnverifiedOutput", noUnverifiedOutput);
			sw.Save("ArFixOffset", fixOffset);
			sw.Save("ArWriteCRC", writeArTagsOnEncode);
			sw.Save("ArWriteLog", writeArLogOnConvert);
			sw.Save("ArWriteTagsOnVerify", writeArTagsOnVerify);
			sw.Save("ArWriteLogOnVerify", writeArLogOnVerify);

			sw.Save("PreserveHTOA", preserveHTOA);
			sw.Save("AutoCorrectFilenames", autoCorrectFilenames);
			sw.Save("FLACVerify", flacVerify);
			sw.Save("FlaCudaVerify", flaCudaVerify);
			sw.Save("FlaCudaGPUOnly", flaCudaGPUOnly);
			sw.Save("FlaCudaThreads", flaCudaThreads);
			sw.Save("WVExtraMode", wvExtraMode);
			sw.Save("WVStoreMD5", wvStoreMD5);
			sw.Save("KeepOriginalFilenames", keepOriginalFilenames);
			sw.Save("SingleFilenameFormat", singleFilenameFormat);
			sw.Save("TrackFilenameFormat", trackFilenameFormat);
			sw.Save("RemoveSpecialCharacters", removeSpecial);
			sw.Save("SpecialCharactersExceptions", specialExceptions);
			sw.Save("ReplaceSpaces", replaceSpaces);
			sw.Save("EmbedLog", embedLog);
			sw.Save("ExtractLog", extractLog);
			sw.Save("FillUpCUE", fillUpCUE);
			sw.Save("OverwriteCUEData", overwriteCUEData);			
			sw.Save("FilenamesANSISafe", filenamesANSISafe);
			if (bruteForceDTL) sw.Save("BruteForceDTL", bruteForceDTL);
			if (createEACLOG) sw.Save("CreateEACLOG", createEACLOG);
			sw.Save("DetectHDCD", detectHDCD);
			sw.Save("Wait750FramesForHDCD", wait750FramesForHDCD);
			sw.Save("DecodeHDCD", decodeHDCD);
			sw.Save("CreateM3U", createM3U);
			sw.Save("CreateTOC", createTOC);
			sw.Save("CreateCUEFileWhenEmbedded", createCUEFileWhenEmbedded);
			sw.Save("Truncate4608ExtraSamples", truncate4608ExtraSamples);
			sw.Save("LossyWAVQuality", lossyWAVQuality);
			sw.Save("DecodeHDCDToLossyWAV16", decodeHDCDtoLW16);
			sw.Save("DecodeHDCDTo24bit", decodeHDCDto24bit);
			sw.Save("DisableAsm", disableAsm);
			sw.Save("OneInstance", oneInstance);
			sw.Save("CheckForUpdates", checkForUpdates);
			sw.Save("Language", language);

			sw.Save("WriteBasicTagsFromCUEData", writeBasicTagsFromCUEData);
			sw.Save("CopyBasicTags", copyBasicTags);
			sw.Save("CopyUnknownTags", copyUnknownTags);
			sw.Save("CopyAlbumArt", copyAlbumArt);
			sw.Save("EmbedAlbumArt", embedAlbumArt);
			sw.Save("ExtractAlbumArt", extractAlbumArt);
			sw.Save("MaxAlbumArtSize", maxAlbumArtSize);

			sw.Save("ArLogToSourceFolder", arLogToSourceFolder);
			sw.Save("ArLogVerbose", arLogVerbose);
			sw.Save("FixOffsetToNearest", fixOffsetToNearest);

			sw.Save("ArLogFilenameFormat", arLogFilenameFormat);
			sw.Save("AlArtFilenameFormat", alArtFilenameFormat);

			int nEncoders = 0;
			foreach (CUEToolsUDC encoder in encoders)
			{
				sw.Save(string.Format("ExternalEncoder{0}Name", nEncoders), encoder.name);
				sw.Save(string.Format("ExternalEncoder{0}Modes", nEncoders), encoder.supported_modes);
				sw.Save(string.Format("ExternalEncoder{0}Mode", nEncoders), encoder.default_mode);
				if (encoder.path != null)
				{
					sw.Save(string.Format("ExternalEncoder{0}Extension", nEncoders), encoder.extension);
					sw.Save(string.Format("ExternalEncoder{0}Path", nEncoders), encoder.path);
					sw.Save(string.Format("ExternalEncoder{0}Parameters", nEncoders), encoder.parameters);
					sw.Save(string.Format("ExternalEncoder{0}Lossless", nEncoders), encoder.lossless);
				}
				nEncoders++;
			}
			sw.Save("ExternalEncoders", nEncoders);

			int nDecoders = 0;
			foreach (KeyValuePair<string, CUEToolsUDC> decoder in decoders)
				if (decoder.Value.path != null)
				{
					sw.Save(string.Format("ExternalDecoder{0}Name", nDecoders), decoder.Key);
					sw.Save(string.Format("ExternalDecoder{0}Extension", nDecoders), decoder.Value.extension);
					sw.Save(string.Format("ExternalDecoder{0}Path", nDecoders), decoder.Value.path);
					sw.Save(string.Format("ExternalDecoder{0}Parameters", nDecoders), decoder.Value.parameters);
					nDecoders++;
				}
			sw.Save("ExternalDecoders", nDecoders);

			int nFormats = 0;
			foreach (KeyValuePair<string, CUEToolsFormat> format in formats)
			{
				sw.Save(string.Format("CustomFormat{0}Name", nFormats), format.Key);
				sw.Save(string.Format("CustomFormat{0}EncoderLossless", nFormats), format.Value.encoderLossless == null ? "" : format.Value.encoderLossless.Name);
				sw.Save(string.Format("CustomFormat{0}EncoderLossy", nFormats), format.Value.encoderLossy == null ? "" : format.Value.encoderLossy.Name);
				sw.Save(string.Format("CustomFormat{0}Decoder", nFormats), format.Value.decoder);
				sw.Save(string.Format("CustomFormat{0}Tagger", nFormats), (int)format.Value.tagger);
				sw.Save(string.Format("CustomFormat{0}AllowLossless", nFormats), format.Value.allowLossless);
				sw.Save(string.Format("CustomFormat{0}AllowLossy", nFormats), format.Value.allowLossy);
				sw.Save(string.Format("CustomFormat{0}AllowLossyWAV", nFormats), format.Value.allowLossyWAV);
				sw.Save(string.Format("CustomFormat{0}AllowEmbed", nFormats), format.Value.allowEmbed);
				nFormats++;
			}
			sw.Save("CustomFormats", nFormats);

			int nScripts = 0;
			foreach (KeyValuePair<string, CUEToolsScript> script in scripts)
			{
				sw.Save(string.Format("CustomScript{0}Name", nScripts), script.Key);
				sw.SaveText(string.Format("CustomScript{0}Code", nScripts), script.Value.code);
				int nCondition = 0;
				foreach (CUEAction action in script.Value.conditions)
					sw.Save(string.Format("CustomScript{0}Condition{1}", nScripts, nCondition++), (int)action);
				sw.Save(string.Format("CustomScript{0}Conditions", nScripts), nCondition);
				nScripts++;
			}
			sw.Save("CustomScripts", nScripts);
			sw.Save("DefaultVerifyScript", defaultVerifyScript);
			sw.Save("DefaultVerifyAndConvertScript", defaultEncodeScript);

			sw.Save("GapsHandling", (int)gapsHandling);
		}

		public void Load(SettingsReader sr)
		{
			int version = sr.LoadInt32("Version", null, null) ?? 202;

			fixOffsetMinimumConfidence = sr.LoadUInt32("ArFixWhenConfidence", 1, 1000) ?? 2;
			fixOffsetMinimumTracksPercent = sr.LoadUInt32("ArFixWhenPercent", 1, 100) ?? 51;
			encodeWhenConfidence = sr.LoadUInt32("ArEncodeWhenConfidence", 1, 1000) ?? 2;
			encodeWhenPercent = sr.LoadUInt32("ArEncodeWhenPercent", 1, 100) ?? 100;
			encodeWhenZeroOffset = sr.LoadBoolean("ArEncodeWhenZeroOffset") ?? false;
			noUnverifiedOutput = sr.LoadBoolean("ArNoUnverifiedOutput") ?? false;
			fixOffset = sr.LoadBoolean("ArFixOffset") ?? false;
			writeArTagsOnEncode = sr.LoadBoolean("ArWriteCRC") ?? true;
			writeArLogOnConvert = sr.LoadBoolean("ArWriteLog") ?? true;
			writeArTagsOnVerify = sr.LoadBoolean("ArWriteTagsOnVerify") ?? false;
			writeArLogOnVerify = sr.LoadBoolean("ArWriteLogOnVerify") ?? true;

			preserveHTOA = sr.LoadBoolean("PreserveHTOA") ?? true;
			autoCorrectFilenames = sr.LoadBoolean("AutoCorrectFilenames") ?? true;
			flacVerify = sr.LoadBoolean("FLACVerify") ?? false;
			flaCudaVerify = sr.LoadBoolean("FlaCudaVerify") ?? false;
			flaCudaGPUOnly = sr.LoadBoolean("FlaCudaGPUOnly") ?? false;
			flaCudaThreads = sr.LoadBoolean("FlaCudaThreads") ?? true;
			wvExtraMode = sr.LoadInt32("WVExtraMode", 0, 6) ?? 0;
			wvStoreMD5 = sr.LoadBoolean("WVStoreMD5") ?? false;
			keepOriginalFilenames = sr.LoadBoolean("KeepOriginalFilenames") ?? false;
			singleFilenameFormat =  sr.Load("SingleFilenameFormat") ?? singleFilenameFormat;
			trackFilenameFormat = sr.Load("TrackFilenameFormat") ?? trackFilenameFormat;
			removeSpecial = sr.LoadBoolean("RemoveSpecialCharacters") ?? false;
			specialExceptions = sr.Load("SpecialCharactersExceptions") ?? "-()";
			replaceSpaces = sr.LoadBoolean("ReplaceSpaces") ?? false;
			embedLog = sr.LoadBoolean("EmbedLog") ?? true;
			extractLog = sr.LoadBoolean("ExtractLog") ?? true;
			fillUpCUE = sr.LoadBoolean("FillUpCUE") ?? true;
			overwriteCUEData = sr.LoadBoolean("OverwriteCUEData") ?? false;
			filenamesANSISafe = sr.LoadBoolean("FilenamesANSISafe") ?? true;
			bruteForceDTL = sr.LoadBoolean("BruteForceDTL") ?? false;
			createEACLOG = sr.LoadBoolean("createEACLOG") ?? false;
			detectHDCD = sr.LoadBoolean("DetectHDCD") ?? true;
			wait750FramesForHDCD = sr.LoadBoolean("Wait750FramesForHDCD") ?? true;
			decodeHDCD = sr.LoadBoolean("DecodeHDCD") ?? false;
			createM3U = sr.LoadBoolean("CreateM3U") ?? false;
			createTOC = sr.LoadBoolean("CreateTOC") ?? false;
			createCUEFileWhenEmbedded = sr.LoadBoolean("CreateCUEFileWhenEmbedded") ?? true;
			truncate4608ExtraSamples = sr.LoadBoolean("Truncate4608ExtraSamples") ?? true;
			lossyWAVQuality = sr.LoadInt32("LossyWAVQuality", 0, 10) ?? 5;
			decodeHDCDtoLW16 = sr.LoadBoolean("DecodeHDCDToLossyWAV16") ?? false;
			decodeHDCDto24bit = sr.LoadBoolean("DecodeHDCDTo24bit") ?? true;

			disableAsm = sr.LoadBoolean("DisableAsm") ?? false;
			oneInstance = sr.LoadBoolean("OneInstance") ?? true;
			checkForUpdates = sr.LoadBoolean("CheckForUpdates") ?? true;

			writeBasicTagsFromCUEData = sr.LoadBoolean("WriteBasicTagsFromCUEData") ?? true;
			copyBasicTags = sr.LoadBoolean("CopyBasicTags") ?? true;
			copyUnknownTags = sr.LoadBoolean("CopyUnknownTags") ?? true;
			copyAlbumArt = sr.LoadBoolean("CopyAlbumArt") ?? true;
			embedAlbumArt = sr.LoadBoolean("EmbedAlbumArt") ?? true;
			extractAlbumArt = sr.LoadBoolean("ExtractAlbumArt") ?? true;
			maxAlbumArtSize = sr.LoadInt32("MaxAlbumArtSize", 100, 10000) ?? maxAlbumArtSize;

			arLogToSourceFolder = sr.LoadBoolean("ArLogToSourceFolder") ?? arLogToSourceFolder;
			arLogVerbose = sr.LoadBoolean("ArLogVerbose") ?? arLogVerbose;
			fixOffsetToNearest = sr.LoadBoolean("FixOffsetToNearest") ?? fixOffsetToNearest;
			arLogFilenameFormat = sr.Load("ArLogFilenameFormat") ?? arLogFilenameFormat;
			alArtFilenameFormat = sr.Load("AlArtFilenameFormat") ?? alArtFilenameFormat;

			int totalEncoders = sr.LoadInt32("ExternalEncoders", 0, null) ?? 0;
			for (int nEncoders = 0; nEncoders < totalEncoders; nEncoders++)
			{
				string name = sr.Load(string.Format("ExternalEncoder{0}Name", nEncoders));
				string extension = sr.Load(string.Format("ExternalEncoder{0}Extension", nEncoders));
				string path = sr.Load(string.Format("ExternalEncoder{0}Path", nEncoders));
				string parameters = sr.Load(string.Format("ExternalEncoder{0}Parameters", nEncoders));
				bool lossless = sr.LoadBoolean(string.Format("ExternalEncoder{0}Lossless", nEncoders)) ?? true;
				string supported_modes = sr.Load(string.Format("ExternalEncoder{0}Modes", nEncoders)) ?? "";
				string default_mode = sr.Load(string.Format("ExternalEncoder{0}Mode", nEncoders)) ?? "";
				CUEToolsUDC encoder;
				if (name == null) continue;
				if (!encoders.TryGetValue(name, out encoder))
				{
					if (path == null || parameters == null || extension == null) continue;
					encoders.Add(new CUEToolsUDC(name, extension, lossless, supported_modes, default_mode, path, parameters));
				}
				else if (version == 203)
				{
					if (encoder.path != null)
					{
						if (path == null || parameters == null || extension == null) continue;
						encoder.extension = extension;
						encoder.path = path;
						encoder.parameters = parameters;
						encoder.lossless = lossless;
					}
					encoder.supported_modes = supported_modes;
					encoder.default_mode = default_mode;
				}
			}
			
			int totalDecoders = sr.LoadInt32("ExternalDecoders", 0, null) ?? 0;
			for (int nDecoders = 0; nDecoders < totalDecoders; nDecoders++)
			{
				string name = sr.Load(string.Format("ExternalDecoder{0}Name", nDecoders));
				string extension = sr.Load(string.Format("ExternalDecoder{0}Extension", nDecoders));
				string path = sr.Load(string.Format("ExternalDecoder{0}Path", nDecoders));
				string parameters = sr.Load(string.Format("ExternalDecoder{0}Parameters", nDecoders));
				CUEToolsUDC decoder;
				if (!decoders.TryGetValue(name, out decoder))
					decoders.Add(name, new CUEToolsUDC(name, extension, true, "", "", path, parameters));
				else
				{
					decoder.extension = extension;
					decoder.path = path;
					decoder.parameters = parameters;
				}
			}

			int totalFormats = sr.LoadInt32("CustomFormats", 0, null) ?? 0;
			for (int nFormats = 0; nFormats < totalFormats; nFormats++)
			{
				string extension = sr.Load(string.Format("CustomFormat{0}Name", nFormats));
				string encoderLossless = sr.Load(string.Format("CustomFormat{0}EncoderLossless", nFormats)) ?? "";
				string encoderLossy = sr.Load(string.Format("CustomFormat{0}EncoderLossy", nFormats)) ?? "";
				string decoder = sr.Load(string.Format("CustomFormat{0}Decoder", nFormats));
				CUEToolsTagger tagger = (CUEToolsTagger) (sr.LoadInt32(string.Format("CustomFormat{0}Tagger", nFormats), 0, 2) ?? 0);
				bool allowLossless = sr.LoadBoolean(string.Format("CustomFormat{0}AllowLossless", nFormats)) ?? false;
				bool allowLossy = sr.LoadBoolean(string.Format("CustomFormat{0}AllowLossy", nFormats)) ?? false;
				bool allowLossyWav = sr.LoadBoolean(string.Format("CustomFormat{0}AllowLossyWAV", nFormats)) ?? false;
				bool allowEmbed = sr.LoadBoolean(string.Format("CustomFormat{0}AllowEmbed", nFormats)) ?? false;
				CUEToolsFormat format;
				CUEToolsUDC udcLossless, udcLossy;
				if (encoderLossless == "" || !encoders.TryGetValue(encoderLossless, out udcLossless))
					udcLossless = null;
				if (encoderLossy == "" || !encoders.TryGetValue(encoderLossy, out udcLossy))
					udcLossy = null;
				if (!formats.TryGetValue(extension, out format))
					formats.Add(extension, new CUEToolsFormat(extension, tagger, allowLossless, allowLossy, allowLossyWav, allowEmbed, false, udcLossless, udcLossy, decoder));
				else
				{
					format.encoderLossless = udcLossless;
					format.encoderLossy = udcLossy;
					format.decoder = decoder;
					if (!format.builtin)
					{
						format.tagger = tagger;
						format.allowLossless = allowLossless;
						format.allowLossy = allowLossy;
						format.allowLossyWAV = allowLossyWav;
						format.allowEmbed = allowEmbed;
					}
				}
			}

			int totalScripts = sr.LoadInt32("CustomScripts", 0, null) ?? 0;
			for (int nScripts = 0; nScripts < totalScripts; nScripts++)
			{
				string name = sr.Load(string.Format("CustomScript{0}Name", nScripts));
				string code = sr.Load(string.Format("CustomScript{0}Code", nScripts));
				List<CUEAction> conditions = new List<CUEAction>();
				int totalConditions = sr.LoadInt32(string.Format("CustomScript{0}Conditions", nScripts), 0, null) ?? 0;
				for (int nCondition = 0; nCondition < totalConditions; nCondition++)
					conditions.Add((CUEAction)sr.LoadInt32(string.Format("CustomScript{0}Condition{1}", nScripts, nCondition), 0, null));
				CUEToolsScript script;
				if (!scripts.TryGetValue(name, out script))
					scripts.Add(name, new CUEToolsScript(name, false, conditions, code));
				else
				{
					if (!script.builtin)
					{
						script.code = code;
						script.conditions = conditions;
					}
				}
			}

			defaultVerifyScript = sr.Load("DefaultVerifyScript") ?? "default";
			defaultEncodeScript = sr.Load("DefaultVerifyAndConvertScript") ?? "default";

			gapsHandling = (CUEStyle?)sr.LoadInt32("GapsHandling", null, null) ?? gapsHandling;

			language = sr.Load("Language") ?? Thread.CurrentThread.CurrentUICulture.Name;

			if (arLogFilenameFormat.Contains("%F"))
				arLogFilenameFormat = "%filename%.accurip";
			if (singleFilenameFormat.Contains("%F"))
				singleFilenameFormat = "%filename%";
			if (trackFilenameFormat.Contains("%N"))
				trackFilenameFormat = "%tracknumber%. %title%";
		}

		public string CleanseString (string s)
		{
			StringBuilder sb = new StringBuilder();
			char[] invalid = Path.GetInvalidFileNameChars();

			if (filenamesANSISafe)
				s = Encoding.Default.GetString(Encoding.Default.GetBytes(s));

			for (int i = 0; i < s.Length; i++)
			{
				char ch = s[i];
				if (filenamesANSISafe && removeSpecial && specialExceptions.IndexOf(ch) < 0 && !(
					((ch >= 'a') && (ch <= 'z')) ||
					((ch >= 'A') && (ch <= 'Z')) ||
					((ch >= '0') && (ch <= '9')) ||
					(ch == ' ') || (ch == '_')))
					ch = '_';
				if ((Array.IndexOf(invalid, ch) >= 0) || (replaceSpaces && ch == ' '))
					sb.Append("_");
				else
					sb.Append(ch);
			}

			return sb.ToString();
		}
	}

	public class CUEToolsProfile
	{
		public CUEToolsProfile(string name)
		{
			_config = new CUEConfig();
			_name = name;
			switch (name)
			{
				case "verify":
					_action = CUEAction.Verify;
					_script = "only if found";
					break;
				case "convert":
					_action = CUEAction.Encode;
					break;
				case "fix":
					_action = CUEAction.Encode;
					_script = "fix offset";
					break;
			}
		}

		public void Load(SettingsReader sr)
		{
			_config.Load(sr);

			_useFreeDb = sr.LoadBoolean("FreedbLookup") ?? _useFreeDb;
			_useMusicBrainz = sr.LoadBoolean("MusicBrainzLookup") ?? _useMusicBrainz;
			_useAccurateRip = sr.LoadBoolean("AccurateRipLookup") ?? _useAccurateRip;
			_outputAudioType = (AudioEncoderType?)sr.LoadInt32("OutputAudioType", null, null) ?? _outputAudioType;
			_outputAudioFormat = sr.Load("OutputAudioFmt") ?? _outputAudioFormat;
			_action = (CUEAction?)sr.LoadInt32("AccurateRipMode", (int)CUEAction.Encode, (int)CUEAction.CorrectFilenames) ?? _action;
			_CUEStyle = (CUEStyle?)sr.LoadInt32("CUEStyle", null, null) ?? _CUEStyle;
			_writeOffset = sr.LoadInt32("WriteOffset", null, null) ?? 0;
			_outputTemplate = sr.Load("OutputPathTemplate") ?? _outputTemplate;
			_script = sr.Load("Script") ?? _script;
		}

		public void Save(SettingsWriter sw)
		{
			_config.Save(sw);

			sw.Save("FreedbLookup", _useFreeDb);
			sw.Save("MusicBrainzLookup", _useMusicBrainz);
			sw.Save("AccurateRipLookup", _useAccurateRip);
			sw.Save("OutputAudioType", (int)_outputAudioType);
			sw.Save("OutputAudioFmt", _outputAudioFormat);
			sw.Save("AccurateRipMode", (int)_action);
			sw.Save("CUEStyle", (int)_CUEStyle);
			sw.Save("WriteOffset", (int)_writeOffset);
			sw.Save("OutputPathTemplate", _outputTemplate);
			sw.Save("Script", _script);
		}

		public CUEConfig _config;
		public AudioEncoderType _outputAudioType = AudioEncoderType.Lossless;
		public string _outputAudioFormat = "flac", _outputTemplate = null, _script = "default";
		public CUEAction _action = CUEAction.Encode;
		public CUEStyle _CUEStyle = CUEStyle.SingleFileWithCUE;
		public int _writeOffset = 0;
		public bool _useFreeDb = true, _useMusicBrainz = true, _useAccurateRip = true;

		public string _name;
	}

	public class CUEToolsProgressEventArgs
	{
		public string status = string.Empty;
		public double percentTrck = 0;
		public double percentDisk = 0.0;
		public int offset = 0;
		public string input = string.Empty;
		public string output = string.Empty;
	}

	public class ArchivePasswordRequiredEventArgs
	{
		public string Password = string.Empty;
		public bool ContinueOperation = true;
	}

	public class CUEToolsSourceFile
	{
		public string path;
		public string contents;
		public bool isEAC;

		public CUEToolsSourceFile(string _path, StreamReader reader)
		{
			path = _path;
			contents = reader.ReadToEnd();
			reader.Close();
		}
	}

	public class CUEToolsSelectionEventArgs
	{
		public object[] choices;
		public int selection = -1;
	}

	public delegate void CUEToolsSelectionHandler(object sender, CUEToolsSelectionEventArgs e);
	public delegate void CUEToolsProgressHandler(object sender, CUEToolsProgressEventArgs e);
	public delegate void ArchivePasswordRequiredHandler(object sender, ArchivePasswordRequiredEventArgs e);

	public class CUESheet {
		private bool _stop, _pause;
		private List<CUELine> _attributes;
		private List<TrackInfo> _tracks;
		private List<SourceInfo> _sources;
		private List<string> _sourcePaths, _trackFilenames;
		private string _htoaFilename, _singleFilename;
		private bool _hasHTOAFilename = false, _hasTrackFilenames = false, _hasSingleFilename = false, _appliedWriteOffset;
		private bool _hasEmbeddedCUESheet;
		private bool _paddedToFrame, _truncated4608, _usePregapForFirstTrackInSingleFile;
		private int _writeOffset;
		private CUEAction _action;
		private bool _useAccurateRip = false;
		private uint? _minDataTrackLength;
		private string _accurateRipId;
		private string _eacLog;
		private string _inputPath, _inputDir;
		private string _outputPath;
		private string[] _destPaths;
		private TagLib.File _fileInfo;
		private const int _arOffsetRange = 5 * 588 - 1;
		private HDCDDotNet.HDCDDotNet hdcdDecoder;
		private AudioEncoderType _audioEncoderType = AudioEncoderType.Lossless;
		private bool _outputLossyWAV = false;
		private string _outputFormat = "wav";
		private CUEStyle _outputStyle = CUEStyle.SingleFile;
		private CUEConfig _config;
		private string _cddbDiscIdTag;
		private bool _isCD;
		private string _ripperLog;
		private CDDriveReader _ripper;
		private bool _isArchive;
		private List<string> _archiveContents;
		private string _archiveCUEpath;
		private string _archivePath;
		private string _archivePassword;
		private CUEToolsProgressEventArgs _progress;
		private AccurateRipVerify _arVerify;
		private CDImageLayout _toc;
		private string _arLogFileName, _alArtFileName;
		private TagLib.IPicture[] _albumArt;
		private int _padding = 8192;

		public event ArchivePasswordRequiredHandler PasswordRequired;
		public event CUEToolsProgressHandler CUEToolsProgress;
		public event CUEToolsSelectionHandler CUEToolsSelection;

		public CUESheet(CUEConfig config)
		{
			_config = config;
			_progress = new CUEToolsProgressEventArgs();
			_attributes = new List<CUELine>();
			_tracks = new List<TrackInfo>();
			_trackFilenames = new List<string>();
			_toc = new CDImageLayout();
			_sources = new List<SourceInfo>();
			_sourcePaths = new List<string>();
			_stop = false;
			_pause = false;
			_outputPath = null;
			_paddedToFrame = false;
			_truncated4608 = false;
			_usePregapForFirstTrackInSingleFile = false;
			_action = CUEAction.Encode;
			_appliedWriteOffset = false;
			_minDataTrackLength = null;
			hdcdDecoder = null;
			_hasEmbeddedCUESheet = false;
			_isArchive = false;
			_isCD = false;
			_albumArt = null;
		}

		public void OpenCD(CDDriveReader ripper)
		{
			_ripper = ripper;
			_toc = (CDImageLayout)_ripper.TOC.Clone();
			for (int iTrack = 0; iTrack < _toc.AudioTracks; iTrack++)
			{
				_trackFilenames.Add(string.Format("{0:00}.wav", iTrack + 1));
				_tracks.Add(new TrackInfo());
			}
			_arVerify = new AccurateRipVerify(_toc);
			_isCD = true;
			SourceInfo cdInfo;
			cdInfo.Path = _ripper.ARName;
			cdInfo.Offset = 0;
			cdInfo.Length = _toc.AudioLength * 588;
			_sources.Add(cdInfo);
			_ripper.ReadProgress += new EventHandler<ReadProgressArgs>(CDReadProgress);
		}

		public void Close()
		{
			if (_ripper != null)
				_ripper.Close();
			_ripper = null;
		}

		public AccurateRipVerify ArVerify
		{
			get
			{
				return _arVerify;
			}
		}

		public CDDriveReader CDRipper
		{
			get
			{
				return _ripper;
			}
			set
			{
				_ripper = value;
			}
		}

		public void CopyMetadata(CUESheet metadata)
		{
			TotalDiscs = metadata.TotalDiscs;
			DiscNumber = metadata.DiscNumber;
			Year = metadata.Year;
			Genre = metadata.Genre;
			Artist = metadata.Artist;
			Title = metadata.Title;
			for (int i = 0; i < Tracks.Count; i++)
			{
				Tracks[i].Title = metadata.Tracks[i].Title;
				Tracks[i].Artist = metadata.Tracks[i].Artist;
			}
		}

		public void FillFromMusicBrainz(MusicBrainz.Release release)
		{
			Year = release.GetEvents().Count > 0 ? release.GetEvents()[0].Date.Substring(0, 4) : "";
			Artist = release.GetArtist();
			Title = release.GetTitle();
			// How to get Genre: http://mm.musicbrainz.org/ws/1/release/6fe1e218-2aee-49ac-94f0-7910ba2151df.html?type=xml&inc=tags
			//Catalog = release.GetEvents().Count > 0 ? release.GetEvents()[0].Barcode : "";
			for (int i = 1; i <= _toc.AudioTracks; i++)
			{
				MusicBrainz.Track track = release.GetTracks()[(int)_toc[i].Number - 1]; // !!!!!! - _toc.FirstAudio
				Tracks[i - 1].Title = track.GetTitle();
				Tracks[i - 1].Artist = track.GetArtist();
			}
		}

		public void FillFromFreedb(Freedb.CDEntry cdEntry)
		{
			Year = cdEntry.Year;
			Genre = cdEntry.Genre;
			Artist = cdEntry.Artist;
			Title = cdEntry.Title;
			for (int i = 0; i < _toc.AudioTracks; i++)
			{
				Tracks[i].Title = cdEntry.Tracks[i].Title;
				Tracks[i].Artist = cdEntry.Artist;
			}
		}

		public List<object> LookupAlbumInfo(bool useFreedb, bool useMusicBrainz)
		{
			List<object> Releases = new List<object>();

			if (useFreedb)
			{
				ShowProgress("Looking up album via Freedb...", 0.0, 0.0, null, null);

				FreedbHelper m_freedb = new FreedbHelper();

				m_freedb.UserName = "gchudov";
				m_freedb.Hostname = "gmail.com";
				m_freedb.ClientName = "CUETools";
				m_freedb.Version = "1.9.5";
				m_freedb.SetDefaultSiteAddress("freedb.org");

				QueryResult queryResult;
				QueryResultCollection coll;
				string code = string.Empty;
				try
				{
					CDEntry cdEntry = null;
					code = m_freedb.Query(AccurateRipVerify.CalculateCDDBQuery(_toc), out queryResult, out coll);
					if (code == FreedbHelper.ResponseCodes.CODE_200)
					{
						code = m_freedb.Read(queryResult, out cdEntry);
						if (code == FreedbHelper.ResponseCodes.CODE_210)
							Releases.Add(cdEntry);
					}
					else
						if (code == FreedbHelper.ResponseCodes.CODE_210 ||
							code == FreedbHelper.ResponseCodes.CODE_211)
						{
							int i = 0;
							foreach (QueryResult qr in coll)
							{
								ShowProgress("Looking up album via freedb...", 0.0, (++i + 0.0) / coll.Count, null, null);
								CheckStop();
								code = m_freedb.Read(qr, out cdEntry);
								if (code == FreedbHelper.ResponseCodes.CODE_210)
									Releases.Add(cdEntry);
							}
						}
				}
				catch (Exception ex)
				{
					if (ex is StopException)
						throw ex;
				}
			}

			if (useMusicBrainz)
			{
				ShowProgress("Looking up album via MusicBrainz...", 0.0, 0.0, null, null);

				StringCollection DiscIds = new StringCollection();
				DiscIds.Add(_toc.MusicBrainzId);
				//if (_tocFromLog != null && !DiscIds.Contains(_tocFromLog.MusicBrainzId))
				//	DiscIds.Add(_tocFromLog.MusicBrainzId);
				foreach (CDEntry cdEntry in Releases)
				{
					CDImageLayout toc = TocFromCDEntry(cdEntry);
					if (!DiscIds.Contains(toc.MusicBrainzId))
						DiscIds.Add(toc.MusicBrainzId);
				}

				MusicBrainzService.XmlRequest += new EventHandler<XmlRequestEventArgs>(MusicBrainz_LookupProgress);
				foreach (string DiscId in DiscIds)
				{
					ReleaseQueryParameters p = new ReleaseQueryParameters();
					p.DiscId = DiscId;
					Query<Release> results = Release.Query(p);
					try
					{
						foreach (MusicBrainz.Release release in results)
						{
							release.GetEvents();
							release.GetTracks();
							try
							{
								foreach (MusicBrainz.Track track in release.GetTracks())
									;
							}
							catch { }
							try
							{
								foreach (MusicBrainz.Event ev in release.GetEvents())
									;
							}
							catch { }
							Releases.Add(release);
						}
					}
					catch { }
				}
				MusicBrainzService.XmlRequest -= new EventHandler<XmlRequestEventArgs>(MusicBrainz_LookupProgress);
				//if (release != null)
				//{
				//    FillFromMusicBrainz(release);
				//    return;
				//}
				//if (cdEntry != null)
				//    FillFromFreedb(cdEntry);
			}
			return Releases;
		}

		public CDImageLayout TocFromCDEntry(CDEntry cdEntry)
		{
			CDImageLayout tocFromCDEntry = new CDImageLayout();
			for (int i = 0; i < cdEntry.Tracks.Count; i++)
			{
				if (i >= _toc.TrackCount)
					break;
				tocFromCDEntry.AddTrack(new CDTrack((uint)i + 1,
					(uint) cdEntry.Tracks[i].FrameOffset - 150,
					(i + 1 < cdEntry.Tracks.Count) ? (uint) (cdEntry.Tracks[i + 1].FrameOffset - cdEntry.Tracks[i].FrameOffset) : _toc[i + 1].Length,
					_toc[i + 1].IsAudio,
					false/*preEmphasis*/));
			}
			if (tocFromCDEntry.TrackCount > 0 && tocFromCDEntry[1].IsAudio)
				tocFromCDEntry[1][0].Start = 0;
			return tocFromCDEntry;
		}

		public CDImageLayout TocFromLog(string eacLog)
		{
			CDImageLayout tocFromLog = new CDImageLayout();
			using (StringReader sr = new StringReader(eacLog))
			{
				bool isEACLog = false;
				bool iscdda2wavlog = false;
				string lineStr;
				int prevTrNo = 1, prevTrStart = 0;
				uint firstPreGap = 0;
				while ((lineStr = sr.ReadLine()) != null)
				{
					if (isEACLog)
					{
						string[] n = lineStr.Split('|');
						uint trNo, trStart, trEnd;
						if (n.Length == 5 && uint.TryParse(n[0], out trNo) && uint.TryParse(n[3], out trStart) && uint.TryParse(n[4], out trEnd) && trNo == tocFromLog.TrackCount + 1)
						{
							bool isAudio = true;
							if (tocFromLog.TrackCount >= _toc.TrackCount &&
								trStart == tocFromLog[tocFromLog.TrackCount].End + 1U + 152U * 75U
								)
								isAudio = false;
							if (tocFromLog.TrackCount < _toc.TrackCount &&
								!_toc[tocFromLog.TrackCount + 1].IsAudio
								)
								isAudio = false;
							tocFromLog.AddTrack(new CDTrack(trNo, trStart, trEnd + 1 - trStart, isAudio, false));
						}
						else
						{
							string[] sepTrack = { "Track" };
							string[] sepGap = { "Pre-gap length" };

							string[] partsTrack = lineStr.Split(sepTrack, StringSplitOptions.None);
							if (partsTrack.Length == 2 && uint.TryParse(partsTrack[1], out trNo))
							{
								prevTrNo = (int) trNo;
								continue;
							}

							string[] partsGap = lineStr.Split(sepGap, StringSplitOptions.None);
							if (partsGap.Length == 2)
							{
								string[] n1 = partsGap[1].Split(':', '.');
								int h, m, s, f;
								if (n1.Length == 4 && int.TryParse(n1[0], out h) && int.TryParse(n1[1], out m) && int.TryParse(n1[2], out s) && int.TryParse(n1[3], out f))
								{
									uint gap = (uint)((f * 3 + 2) / 4 + 75 * (s + 60 * (m + 60 * h)));
									if (prevTrNo == 1)
										gap -= 150;
									if (prevTrNo == 1)
										firstPreGap = gap - _toc[1].Start;
									//else
										//firstPreGap += gap;
									while (prevTrNo > tocFromLog.TrackCount && _toc.TrackCount > tocFromLog.TrackCount)
									{
										tocFromLog.AddTrack(new CDTrack((uint)tocFromLog.TrackCount + 1,
											_toc[tocFromLog.TrackCount + 1].Start + firstPreGap,
											_toc[tocFromLog.TrackCount + 1].Length,
											_toc[tocFromLog.TrackCount + 1].IsAudio, false));
									}
									if (prevTrNo <= tocFromLog.TrackCount)
										tocFromLog[prevTrNo].Pregap = gap;
								}
							}
						}
					}
					else if (iscdda2wavlog)
					{
						foreach (string entry in lineStr.Split(','))
						{
							string[] n = entry.Split('(');
							if (n.Length < 2) continue;
							// assert n.Length == 2;
							string key = n[0].Trim(' ', '.');
							int trStart = int.Parse(n[1].Trim(' ', ')'));
							bool isAudio = true; // !!!
							if (key != "1")
								tocFromLog.AddTrack(new CDTrack((uint)prevTrNo, (uint)prevTrStart, (uint)(trStart - prevTrStart), isAudio, false));
							if (key == "lead-out")
							{
								iscdda2wavlog = false;
								break;
							}
							prevTrNo = int.Parse(key);
							prevTrStart = trStart;
						}
					}
					else if (lineStr.StartsWith("TOC of the extracted CD")
						|| lineStr.StartsWith("Exact Audio Copy")
						|| lineStr.StartsWith("EAC extraction logfile")
						|| lineStr.StartsWith("CUERipper"))
						isEACLog = true;
					else if (lineStr.StartsWith("Table of Contents: starting sectors"))
						iscdda2wavlog = true;
				}
			}
			if (tocFromLog.TrackCount == 0)
				return null;
			tocFromLog[1][0].Start = 0;
			return tocFromLog;
		}

		public void Open(string pathIn)
		{
			_inputPath = pathIn;
			_inputDir = Path.GetDirectoryName(_inputPath) ?? _inputPath;
			if (_inputDir == "") _inputDir = ".";
#if !MONO
			if (_inputDir == pathIn)
			{
				CDDriveReader ripper = new CDDriveReader();
				try
				{
					ripper.Open(pathIn[0]);
					if (ripper.TOC.AudioTracks > 0)
					{
						OpenCD(ripper);
						int driveOffset;
						if (!AccurateRipVerify.FindDriveReadOffset(_ripper.ARName, out driveOffset))
							throw new Exception("Failed to find drive read offset for drive" + _ripper.ARName);
						_ripper.DriveOffset = driveOffset;
						//LookupAlbumInfo();
						return;
					}
				}
				catch
				{
					ripper.Dispose();
					_ripper = null;
					throw;
				}
			}
#endif

			TextReader sr;

			if (Directory.Exists(pathIn))
				throw new Exception("is a directory");
			//{
			//    if (cueDir + Path.DirectorySeparatorChar != pathIn && cueDir != pathIn)
			//        throw new Exception("Input directory must end on path separator character.");
			//    string cueSheet = null;
			//    string[] audioExts = new string[] { "*.wav", "*.flac", "*.wv", "*.ape", "*.m4a", "*.tta" };
			//    for (i = 0; i < audioExts.Length && cueSheet == null; i++)
			//        cueSheet = CUESheet.CreateDummyCUESheet(pathIn, audioExts[i]);
			//    if (_config.udc1Extension != null && cueSheet == null)
			//        cueSheet = CUESheet.CreateDummyCUESheet(pathIn, "*." + _config.udc1Extension);
			//    if (cueSheet == null)
			//        throw new Exception("Input directory doesn't contain supported audio files.");
			//    sr = new StringReader(cueSheet);

			//    List<CUEToolsSourceFile> logFiles = new List<CUEToolsSourceFile>();
			//    foreach (string logPath in Directory.GetFiles(pathIn, "*.log"))
			//        logFiles.Add(new CUEToolsSourceFile(logPath, new StreamReader(logPath, CUESheet.Encoding)));
			//    CUEToolsSourceFile selectedLogFile = ChooseFile(logFiles, null, false);
			//    _eacLog = selectedLogFile != null ? selectedLogFile.contents : null;
			//} 
			else if (Path.GetExtension(pathIn).ToLower() == ".zip" || Path.GetExtension(pathIn).ToLower() == ".rar")
			{
				_archiveContents = new List<string>();
				_isArchive = true;
				_archivePath = pathIn;

				if (Path.GetExtension(pathIn).ToLower() == ".rar")
				{
#if !MONO
					using (Unrar _unrar = new Unrar())
					{
						_unrar.PasswordRequired += new PasswordRequiredHandler(unrar_PasswordRequired);
						_unrar.Open(pathIn, Unrar.OpenMode.List);
						while (_unrar.ReadHeader())
						{
							if (!_unrar.CurrentFile.IsDirectory)
								_archiveContents.Add(_unrar.CurrentFile.FileName);
							_unrar.Skip();
						}
						_unrar.Close();
					}
#else
					throw new Exception("rar archives not supported on MONO.");
#endif
				}
				if (Path.GetExtension(pathIn).ToLower() == ".zip")
				{
					using (ZipFile _unzip = new ZipFile(pathIn))
					{
						foreach (ZipEntry e in _unzip)
						{
							if (e.IsFile)
								_archiveContents.Add(e.Name);
						}
						_unzip.Close();
					}
				}

				List<CUEToolsSourceFile> logFiles = new List<CUEToolsSourceFile>();
				List<CUEToolsSourceFile> cueFiles = new List<CUEToolsSourceFile>();
				foreach (string s in _archiveContents)
				{
					if (Path.GetExtension(s).ToLower() == ".cue" || Path.GetExtension(s).ToLower() == ".log")
					{
						Stream archiveStream = OpenArchive(s, false);
						CUEToolsSourceFile sourceFile = new CUEToolsSourceFile(s, new StreamReader(archiveStream, CUESheet.Encoding));
						archiveStream.Close();
						if (Path.GetExtension(s).ToLower() == ".cue")
							cueFiles.Add(sourceFile);
						else
							logFiles.Add(sourceFile);
					}
				}
				CUEToolsSourceFile selectedCUEFile = ChooseFile(cueFiles, null, true);
				if (selectedCUEFile == null || selectedCUEFile.contents == "")
					throw new Exception("Input archive doesn't contain a usable cue sheet.");
				CUEToolsSourceFile selectedLogFile = ChooseFile(logFiles, Path.GetFileNameWithoutExtension(selectedCUEFile.path), true);
				_eacLog = selectedLogFile != null ? selectedLogFile.contents : null;
				_archiveCUEpath = Path.GetDirectoryName(selectedCUEFile.path);
				string cueText = selectedCUEFile.contents;
				if (_config.autoCorrectFilenames)
				{
					string extension;
					cueText = CorrectAudioFilenames(_config, _archiveCUEpath, cueText, false, _archiveContents, out extension);
				}
				sr = new StringReader(cueText);
			}
			else if (Path.GetExtension(pathIn).ToLower() == ".cue")
			{
				if (_config.autoCorrectFilenames)
					sr = new StringReader(CorrectAudioFilenames(_config, pathIn, false));
				else
					sr = new StreamReader(pathIn, CUESheet.Encoding);

				List<CUEToolsSourceFile> logFiles = new List<CUEToolsSourceFile>();
				foreach (string logPath in Directory.GetFiles(_inputDir, "*.log"))
					try { logFiles.Add(new CUEToolsSourceFile(logPath, new StreamReader(logPath, CUESheet.Encoding))); }
					catch { }
				CUEToolsSourceFile selectedLogFile = ChooseFile(logFiles, Path.GetFileNameWithoutExtension(pathIn), false);
				_eacLog = selectedLogFile != null ? selectedLogFile.contents : null;
			}
			else
			{
				string extension = Path.GetExtension(pathIn).ToLower();
				sr = null;
				CUEToolsFormat fmt;
				if (!extension.StartsWith(".") || !_config.formats.TryGetValue(extension.Substring(1), out fmt) || !fmt.allowLossless)
					throw new Exception("Unknown input format.");
				if (fmt.allowEmbed)
				{
					string cuesheetTag = null;
					TagLib.File fileInfo;
					GetSampleLength(pathIn, out fileInfo);
					NameValueCollection tags = Tagging.Analyze(fileInfo);
					cuesheetTag = tags.Get("CUESHEET");
					_accurateRipId = tags.Get("ACCURATERIPID");
					_eacLog = tags.Get("LOG");
					if (_eacLog == null) _eacLog = tags.Get("LOGFILE");
					if (_eacLog == null) _eacLog = tags.Get("EACLOG");
					if (cuesheetTag != null)
					{
						sr = new StringReader(cuesheetTag);
						_hasEmbeddedCUESheet = true;
					}
				}
				if (!_hasEmbeddedCUESheet)
				{
					string cueSheet = CUESheet.CreateDummyCUESheet(_config, pathIn);
					if (cueSheet == null)
						throw new Exception("Input file doesn't seem to contain a cue sheet or be part of an album.");
					sr = new StringReader(cueSheet);
					List<CUEToolsSourceFile> logFiles = new List<CUEToolsSourceFile>();
					foreach (string logPath in Directory.GetFiles(_inputDir, "*.log"))
						try { logFiles.Add(new CUEToolsSourceFile(logPath, new StreamReader(logPath, CUESheet.Encoding))); }
						catch { }
					CUEToolsSourceFile selectedLogFile = ChooseFile(logFiles, null, false);
					_eacLog = selectedLogFile != null ? selectedLogFile.contents : null;
				}
			}

			OpenCUE(sr);
		}

		public void OpenCUE(TextReader sr)
		{
			string pathAudio = null;
			string lineStr, command, fileType;
			bool fileIsBinary = false;
			int timeRelativeToFileStart, absoluteFileStartTime = 0;
			int fileTimeLengthSamples = 0, fileTimeLengthFrames = 0, i;
			TagLib.File _trackFileInfo = null;
			bool seenFirstFileIndex = false;
			bool isAudioTrack = true;
			List<IndexInfo> indexes = new List<IndexInfo>();
			IndexInfo indexInfo;
			SourceInfo sourceInfo;
			TrackInfo trackInfo = null;
			int trackNumber = 0;

			using (sr)
			{
				while ((lineStr = sr.ReadLine()) != null) {
					CUELine line = new CUELine(lineStr);
					if (line.Params.Count > 0) {
						command = line.Params[0].ToUpper();

						if (command == "FILE") {
							fileType = line.Params[2].ToUpper();
							fileIsBinary = (fileType == "BINARY") || (fileType == "MOTOROLA");
							if (fileIsBinary)
							{
								if (!_hasEmbeddedCUESheet && _sourcePaths.Count == 0)
								{
									try
									{
										if (_isArchive)
											pathAudio = LocateFile(_archiveCUEpath, line.Params[1], _archiveContents);
										else
											pathAudio = LocateFile(_inputDir, line.Params[1], null);
										fileIsBinary = (pathAudio == null);
									}
									catch { }
								}
							}
							if (!fileIsBinary)
							{
								if (!_hasEmbeddedCUESheet)
								{
									if (_isArchive)
										pathAudio = LocateFile(_archiveCUEpath, line.Params[1], _archiveContents);
									else
										pathAudio = LocateFile(_inputDir, line.Params[1], null);
								}
								else
								{
									pathAudio = _inputPath;
									if (_sourcePaths.Count > 0)
										throw new Exception("Extra file in embedded CUE sheet: \"" + line.Params[1] + "\".");
								}
								_sourcePaths.Add(pathAudio);
								absoluteFileStartTime += fileTimeLengthFrames;
								if (pathAudio == null)
								{
									throw new Exception("Unable to locate file \"" + line.Params[1] + "\".");
									//fileTimeLengthFrames = 75 * 60 * 70;;
									//fileTimeLengthSamples = fileTimeLengthFrames * 588;
									//if (_hasEmbeddedCUESheet)
									//    _fileInfo = null;
									//else
									//    _trackFileInfo = null;
								}
								else
								{
									TagLib.File fileInfo;
									fileTimeLengthSamples = GetSampleLength(pathAudio, out fileInfo);
									if ((fileTimeLengthSamples % 588) == 492 && _config.truncate4608ExtraSamples)
									{
										_truncated4608 = true;
										fileTimeLengthSamples -= 4608;
									}
									fileTimeLengthFrames = (int)((fileTimeLengthSamples + 587) / 588);
									if (_hasEmbeddedCUESheet)
										_fileInfo = fileInfo;
									else
										_trackFileInfo = fileInfo;
								}
								seenFirstFileIndex = false;
							}
						}
						else if (command == "TRACK") 
						{
							isAudioTrack = line.Params[2].ToUpper() == "AUDIO";
							trackNumber = int.Parse(line.Params[1]);
							if (trackNumber != _toc.TrackCount + 1)
								throw new Exception("Invalid track number.");
							_toc.AddTrack(new CDTrack((uint)trackNumber, 0, 0, isAudioTrack, false));
							if (isAudioTrack)
							{
								trackInfo = new TrackInfo();
								_tracks.Add(trackInfo);
							}
						}
						else if (command == "INDEX") 
						{
							timeRelativeToFileStart = CDImageLayout.TimeFromString(line.Params[2]);
							if (!seenFirstFileIndex)
							{
								if (timeRelativeToFileStart != 0)
									throw new Exception("First index must start at file beginning.");
								seenFirstFileIndex = true;
								if (isAudioTrack)
								{
									if (_tracks.Count > 0 && _trackFileInfo != null)
										_tracks[_tracks.Count - 1]._fileInfo = _trackFileInfo;
									_trackFileInfo = null;
									sourceInfo.Path = pathAudio;
									sourceInfo.Offset = 0;
									sourceInfo.Length = (uint)fileTimeLengthSamples;
									_sources.Add(sourceInfo);
									if ((fileTimeLengthSamples % 588) != 0)
									{
										sourceInfo.Path = null;
										sourceInfo.Offset = 0;
										sourceInfo.Length = (uint)((fileTimeLengthFrames * 588) - fileTimeLengthSamples);
										_sources.Add(sourceInfo);
										_paddedToFrame = true;
									}
								}
							}
							else
							{
								if (fileIsBinary)
								{
									// THIS CODE NEVER EXECUTES!!!

									fileTimeLengthFrames = timeRelativeToFileStart + 150;
									sourceInfo.Path = null;
									sourceInfo.Offset = 0;
									sourceInfo.Length = 150 * 588;
									_sources.Add(sourceInfo);
									throw new Exception("unexpected BINARY directive");
								}
								else
								{
									if (timeRelativeToFileStart > fileTimeLengthFrames)
										throw new Exception(string.Format("TRACK {0} INDEX {1} is at {2}, which is past {3} - the end of source file {4}", trackNumber, line.Params[1], CDImageLayout.TimeToString((uint)timeRelativeToFileStart), CDImageLayout.TimeToString((uint)fileTimeLengthFrames), pathAudio));
								}
							}
							indexInfo.Track = trackNumber;
							indexInfo.Index = Int32.Parse(line.Params[1]);
							indexInfo.Time = absoluteFileStartTime + timeRelativeToFileStart;
							indexes.Add(indexInfo);
						}
						else if (!isAudioTrack)
						{
							// Ignore lines belonging to data tracks
						}
						else if (command == "PREGAP")
						{
							if (seenFirstFileIndex)
								throw new Exception("Pregap must occur at the beginning of a file.");
							int pregapLength = CDImageLayout.TimeFromString(line.Params[1]);
							indexInfo.Track = trackNumber;
							indexInfo.Index = 0;
							indexInfo.Time = absoluteFileStartTime;
							indexes.Add(indexInfo);
							sourceInfo.Path = null;
							sourceInfo.Offset = 0;
							sourceInfo.Length = (uint)pregapLength * 588;
							_sources.Add(sourceInfo);
							absoluteFileStartTime += pregapLength;
						}
						else if (command == "POSTGAP") {
							throw new Exception("POSTGAP command isn't supported.");
						}
						//else if ((command == "REM") &&
						//    (line.Params.Count >= 3) &&
						//    (line.Params[1].Length >= 10) &&
						//    (line.Params[1].Substring(0, 10).ToUpper() == "REPLAYGAIN"))
						//{
						//    // Remove ReplayGain lines
						//}
						else if ((command == "REM") &&
						   (line.Params.Count == 3) &&
						   (line.Params[1].ToUpper() == "ACCURATERIPID"))
						{
							_accurateRipId = line.Params[2];
						}
						//else if ((command == "REM") &&
						//   (line.Params.Count == 3) &&
						//   (line.Params[1].ToUpper() == "SHORTEN"))
						//{
						//    fileTimeLengthFrames -= General.TimeFromString(line.Params[2]);
						//}							
						//else if ((command == "REM") &&
						//   (line.Params.Count == 3) &&
						//   (line.Params[1].ToUpper() == "LENGTHEN"))
						//{
						//    fileTimeLengthFrames += General.TimeFromString(line.Params[2]);
						//}							
						else
						{
							if (trackInfo != null)
							{
								trackInfo.Attributes.Add(line);
							}
							else
							{
								if (line.Params.Count > 2 && !line.IsQuoted[1] &&
									(line.Params[0].ToUpper() == "TITLE" || line.Params[0].ToUpper() == "ARTIST" ||
									(line.Params[0].ToUpper() == "REM" && line.Params[1].ToUpper() == "GENRE" && line.Params.Count > 3 && !line.IsQuoted[2])))
								{
									CUELine modline = new CUELine();
									int nParams = line.Params[0].ToUpper() == "REM" ? 2 : 1;
									for (int iParam = 0; iParam < nParams; iParam++)
									{
										modline.Params.Add(line.Params[iParam]);
										modline.IsQuoted.Add(false);
									}
									string s = line.Params[nParams];
									for (int iParam = nParams + 1; iParam < line.Params.Count; iParam++)
										s += " " + line.Params[iParam];
									modline.Params.Add(s); 
									modline.IsQuoted.Add(true);
									line = modline;
								}
								_attributes.Add(line);
							}
						}
					}
				}
				sr.Close();
			}

			if (_tracks.Count == 0)
				throw new Exception("File must contain at least one audio track.");

			// Add dummy index 01 for data track
			if (!_toc[_toc.TrackCount].IsAudio && indexes[indexes.Count - 1].Index == 0)
			{
				fileTimeLengthFrames += 152 * 75;
				indexInfo.Track = trackNumber;
				indexInfo.Index = 1;
				indexInfo.Time = absoluteFileStartTime + fileTimeLengthFrames;
				indexes.Add(indexInfo);
			}

			// Add dummy track for calculation purposes
			indexInfo.Track = trackNumber + 1;
			indexInfo.Index = 1;
			indexInfo.Time = absoluteFileStartTime + fileTimeLengthFrames;
			indexes.Add(indexInfo);

			// Calculate the length of each index
			for (i = 0; i < indexes.Count - 1; i++) 
			{
				if (indexes[i + 1].Time - indexes[i].Time < 0)
					throw new Exception("Indexes must be in chronological order.");
				if ((indexes[i+1].Track != indexes[i].Track || indexes[i+1].Index != indexes[i].Index + 1) &&
					(indexes[i + 1].Track != indexes[i].Track + 1 || indexes[i].Index < 1 || indexes[i + 1].Index > 1))
					throw new Exception("Indexes must be in chronological order.");
				if (indexes[i].Index == 1 && (i == 0 || indexes[i - 1].Index != 0))
					_toc[indexes[i].Track].AddIndex(new CDTrackIndex(0U, (uint)indexes[i].Time));
				_toc[indexes[i].Track].AddIndex(new CDTrackIndex((uint)indexes[i].Index, (uint)indexes[i].Time));
			}

			// Calculate the length of each track
			for (int iTrack = 1; iTrack <= _toc.TrackCount; iTrack++)
			{
				_toc[iTrack].Start = _toc[iTrack][1].Start;
				_toc[iTrack].Length = iTrack == _toc.TrackCount
					? (uint)indexes[indexes.Count - 1].Time - _toc[iTrack].Start
					: _toc[iTrack + 1].IsAudio
						? _toc[iTrack + 1][1].Start - _toc[iTrack].Start
						: _toc[iTrack + 1][0].Start - _toc[iTrack].Start;

			}

			// Store the audio filenames, generating generic names if necessary
			_hasSingleFilename = _sourcePaths.Count == 1;
			_singleFilename = _hasSingleFilename ? Path.GetFileName(_sourcePaths[0]) :
				"Range.wav";

			_hasHTOAFilename = (_sourcePaths.Count == (TrackCount + 1));
			_htoaFilename = _hasHTOAFilename ? Path.GetFileName(_sourcePaths[0]) : "01.00.wav";

			_hasTrackFilenames = !_hasEmbeddedCUESheet && !_hasSingleFilename && (_sourcePaths.Count == TrackCount || _hasHTOAFilename);
			for (i = 0; i < TrackCount; i++) {
				_trackFilenames.Add( _hasTrackFilenames ? Path.GetFileName(
					_sourcePaths[i + (_hasHTOAFilename ? 1 : 0)]) : String.Format("{0:00}.wav", i + 1) );
			}
			if (!_hasEmbeddedCUESheet && _hasSingleFilename)
			{
				_fileInfo = _tracks[0]._fileInfo;
				_tracks[0]._fileInfo = null;
			}
			if (_config.fillUpCUE)
			{
				if (_config.overwriteCUEData || General.FindCUELine(_attributes, "PERFORMER") == null)
				{
					string value = GetCommonTag(delegate(TagLib.File file) { return file.Tag.JoinedAlbumArtists; });
					if (value == null)
						value = GetCommonTag(delegate(TagLib.File file) { return file.Tag.JoinedPerformers; });
					if (value != null && value != "")
						General.SetCUELine(_attributes, "PERFORMER", value, true);
				}
				if (_config.overwriteCUEData || General.FindCUELine(_attributes, "TITLE") == null)
				{
					string value = GetCommonTag(delegate(TagLib.File file) { return file.Tag.Album; });
					if (value != null)
						General.SetCUELine(_attributes, "TITLE", value, true);
				}
				if (_config.overwriteCUEData || General.FindCUELine(_attributes, "REM", "DATE") == null)
				{
					string value = GetCommonTag(delegate(TagLib.File file) { return file.Tag.Year != 0 ? file.Tag.Year.ToString() : null; });
					if (value != null)
						General.SetCUELine(_attributes, "REM", "DATE", value, false);
				}
				if (_config.overwriteCUEData || General.FindCUELine(_attributes, "REM", "GENRE") == null)
				{
					string value = GetCommonTag(delegate(TagLib.File file) { return file.Tag.JoinedGenres; });
					if (value != null)
						General.SetCUELine(_attributes, "REM", "GENRE", value, true);
				}
				if (_config.overwriteCUEData || TotalDiscs == "")
				{
					string value = GetCommonTag(delegate(TagLib.File file) { return file.Tag.DiscCount != 0 ? file.Tag.DiscCount.ToString() : null; });
					if (value != null)
						TotalDiscs = value;
				}
				if (_config.overwriteCUEData || DiscNumber == "")
				{
					string value = GetCommonTag(delegate(TagLib.File file) { return file.Tag.Disc != 0 ? file.Tag.Disc.ToString() : null; });
					if (value != null)
						DiscNumber = value;
				}
				for (i = 0; i < TrackCount; i++)
				{
					TrackInfo track = _tracks[i];
					string artist = _hasTrackFilenames && track._fileInfo  != null ? track._fileInfo.Tag.JoinedPerformers :
						_hasEmbeddedCUESheet && _fileInfo != null ? Tagging.TagListToSingleValue(Tagging.GetMiscTag(_fileInfo, String.Format("cue_track{0:00}_ARTIST", i + 1))) :
						null;
					string title = _hasTrackFilenames && track._fileInfo != null ? track._fileInfo.Tag.Title :
						_hasEmbeddedCUESheet && _fileInfo != null ? Tagging.TagListToSingleValue(Tagging.GetMiscTag(_fileInfo, String.Format("cue_track{0:00}_TITLE", i + 1))) :
						null;
					if ((_config.overwriteCUEData || track.Artist == "") && artist != null && artist != "")
						track.Artist = artist;
					if ((_config.overwriteCUEData || track.Title == "") && title != null)
						track.Title = title;
				}
			}

			CUELine cddbDiscIdLine = General.FindCUELine(_attributes, "REM", "DISCID");
			_cddbDiscIdTag = cddbDiscIdLine != null && cddbDiscIdLine.Params.Count == 3 ? cddbDiscIdLine.Params[2] : null;
			if (_cddbDiscIdTag == null)
				_cddbDiscIdTag = GetCommonMiscTag("DISCID");

			if (_accurateRipId == null)
				_accurateRipId = GetCommonMiscTag("ACCURATERIPID");

			CDImageLayout tocFromLog = _eacLog == null ? null : TocFromLog(_eacLog);

			// use pregaps from log
			if (tocFromLog != null)
			{
				//int srcNo = (int) _toc[_toc.FirstAudio].LastIndex - (PreGapLength == 0 ? 1 : 0);
				if (PreGapLength < tocFromLog.Pregap)
				{
					PreGapLength = tocFromLog.Pregap;
					//srcNo ++;
				}
				int trNo;
				for (trNo = 1; trNo < tocFromLog.AudioTracks && trNo < _toc.AudioTracks; trNo++)
				{
					if (_toc[_toc.FirstAudio + trNo].Pregap < tocFromLog[tocFromLog.FirstAudio + trNo].Pregap)
						_toc[_toc.FirstAudio + trNo].Pregap = tocFromLog[tocFromLog.FirstAudio + trNo].Pregap;
				}
				//if (_toc[_toc.FirstAudio].Length > tocFromLog[tocFromLog.FirstAudio].Length)
				//{
				//    uint offs = _toc[_toc.FirstAudio].Length - tocFromLog[tocFromLog.FirstAudio].Length;
				//    _toc[_toc.FirstAudio].Length -= offs;

				//    sourceInfo = _sources[srcNo];
				//    sourceInfo.Length -= offs * 588;
				//    _sources[srcNo] = sourceInfo;
				//    for (i = _toc.FirstAudio + 1; i <= _toc.TrackCount; i++)
				//    {
				//        _toc[i].Start -= offs;
				//        for (int j = 0; j <= _toc[i].LastIndex; j++)
				//            if (i != _toc.FirstAudio + 1 || j != 0 || _toc[i][0].Start == _toc[i][1].Start)
				//                _toc[i][j].Start -= offs;
				//    }
				//}
				//for (trNo = 1; trNo < tocFromLog.AudioTracks && trNo < _toc.AudioTracks; trNo++)
				//{
				//    srcNo ++;
				//    if (_toc[_toc.FirstAudio + trNo].Length > tocFromLog[tocFromLog.FirstAudio + trNo].Length)
				//    {
				//        uint offs = _toc[_toc.FirstAudio + trNo].Length - tocFromLog[tocFromLog.FirstAudio + trNo].Length;
				//        _toc[_toc.FirstAudio + trNo].Length -= offs;
				//        sourceInfo = _sources[srcNo];
				//        sourceInfo.Length -= offs * 588;
				//        _sources[srcNo] = sourceInfo;
				//        for (i = _toc.FirstAudio + trNo + 1; i <= _toc.TrackCount; i++)
				//        {
				//            _toc[i].Start -= offs;
				//            for (int j = 0; j <= _toc[i].LastIndex; j++)
				//                if (i != _toc.FirstAudio + trNo + 1 || j != 0 || _toc[i][0].Start == _toc[i][1].Start)
				//                    _toc[i][j].Start -= offs;
				//        }
				//    }
				//}
			}

			// use data track length from log
			if (tocFromLog != null && tocFromLog.AudioTracks == _toc.AudioTracks && tocFromLog.TrackCount == tocFromLog.AudioTracks + 1)
			{
				if (!tocFromLog[tocFromLog.TrackCount].IsAudio)
				{
					DataTrackLength = tocFromLog[tocFromLog.TrackCount].Length;
					_toc[_toc.TrackCount].Start = tocFromLog[_toc.TrackCount].Start;
					_toc[_toc.TrackCount][0].Start = tocFromLog[_toc.TrackCount].Start;
					_toc[_toc.TrackCount][1].Start = tocFromLog[_toc.TrackCount].Start;
				} else
					DataTrackLength = tocFromLog[1].Length;
			}

			// use data track length range from cddbId
			if (DataTrackLength == 0 && _cddbDiscIdTag != null)
			{
				uint cddbDiscIdNum;
				if (uint.TryParse(_cddbDiscIdTag, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out cddbDiscIdNum) && (cddbDiscIdNum & 0xff) == _toc.AudioTracks + 1)
				{
					if (_toc.TrackCount == _toc.AudioTracks)
						_toc.AddTrack(new CDTrack((uint)_toc.TrackCount, _toc.Length + 152 * 75, 0, false, false));
					uint lengthFromTag = ((cddbDiscIdNum >> 8) & 0xffff);
					_minDataTrackLength = (lengthFromTag + _toc[1].Start / 75) * 75 - _toc.Length;
				}
			}

			_arVerify = new AccurateRipVerify(_toc);

			if (_eacLog != null)
			{
				sr = new StringReader(_eacLog);
				bool isEACLog = false;
				int trNo = 1;
				while ((lineStr = sr.ReadLine()) != null)
				{
					if (isEACLog && trNo <= TrackCount)
					{
						string[] s = { "Copy CRC ", "CRC �����" };
						string[] s1 = { "CRC" };
						string[] n = lineStr.Split(s, StringSplitOptions.None);
						uint crc;
						if (n.Length == 2 && uint.TryParse(n[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out crc))
							_arVerify.CRCLOG(trNo++, crc);
						else if (n.Length == 1)
						{
							n = lineStr.Split(s1, StringSplitOptions.None);
							if (n.Length == 2 && n[0].Trim() == "" && uint.TryParse(n[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out crc))
								_arVerify.CRCLOG(trNo++, crc);
						}
					}
					else
						if (lineStr.StartsWith("Exact Audio Copy")
							|| lineStr.StartsWith("EAC extraction logfile"))
							isEACLog = true;
				}
				if (trNo == 2)
				{
					_arVerify.CRCLOG(0, _arVerify.CRCLOG(1));
					if (TrackCount > 1)
						_arVerify.CRCLOG(1, 0);
				}
			}

			LoadAlbumArt(_tracks[0]._fileInfo ?? _fileInfo);
			ResizeAlbumArt();
			if ((_config.embedAlbumArt || _config.copyAlbumArt) && _albumArt != null && _albumArt.Length > 0)
				_padding += _albumArt[0].Data.Count;
			if (_config.embedLog && _eacLog != null)
				_padding += _eacLog.Length;
		}

		public void UseAccurateRip()
		{
			ShowProgress((string)"Contacting AccurateRip database...", 0, 0, null, null);
			if (!_toc[_toc.TrackCount].IsAudio && DataTrackLength == 0 && _minDataTrackLength.HasValue && _accurateRipId == null && _config.bruteForceDTL)
			{
				uint minDTL = _minDataTrackLength.Value;
				CDImageLayout toc2 = new CDImageLayout(_toc);
				for (uint dtl = minDTL; dtl < minDTL + 75; dtl++)
				{
					toc2[toc2.TrackCount].Length = dtl;
					_arVerify.ContactAccurateRip(AccurateRipVerify.CalculateAccurateRipId(toc2));
					if (_arVerify.AccResult != HttpStatusCode.NotFound)
					{
						DataTrackLength = dtl;
						break;
					}
					ShowProgress((string)"Contacting AccurateRip database...", 0, (dtl - minDTL) / 75.0, null, null);
					CheckStop();
					lock (this)
					{
						Monitor.Wait(this, 500);
					}
				}
			}
			else
			{
				_arVerify.ContactAccurateRip(_accurateRipId ?? AccurateRipVerify.CalculateAccurateRipId(_toc));
			}
			_useAccurateRip = true;
		}

		public static Encoding Encoding {
			get {
				return Encoding.Default;
			}
		}

		internal CUEToolsSourceFile ChooseFile(List<CUEToolsSourceFile> sourceFiles, string defaultFileName, bool quietIfSingle)
		{
			if (sourceFiles.Count <= 0)
				return null;

			if (defaultFileName != null)
			{
				CUEToolsSourceFile defaultFile = null;
				foreach (CUEToolsSourceFile file in sourceFiles)
					if (Path.GetFileNameWithoutExtension(file.path).ToLower() == defaultFileName.ToLower())
					{
						if (defaultFile != null)
						{
							defaultFile = null;
							break;
						}
						defaultFile = file;
					}
				if (defaultFile != null)
					return defaultFile;
			}

			if (quietIfSingle && sourceFiles.Count == 1)
				return sourceFiles[0];

			if (CUEToolsSelection == null)
				return null;

			CUEToolsSelectionEventArgs e = new CUEToolsSelectionEventArgs();
			e.choices = sourceFiles.ToArray();
			CUEToolsSelection(this, e);
			if (e.selection == -1)
				return null;

			return sourceFiles[e.selection];
		}

		internal Stream OpenArchive(string fileName, bool showProgress)
		{
#if !MONO
			if (Path.GetExtension(_archivePath).ToLower() == ".rar")
			{
				RarStream rarStream = new RarStream(_archivePath, fileName);
				rarStream.PasswordRequired += new PasswordRequiredHandler(unrar_PasswordRequired);
				if (showProgress)
					rarStream.ExtractionProgress += new ExtractionProgressHandler(unrar_ExtractionProgress);
				return rarStream;
			}
#endif
			if (Path.GetExtension(_archivePath).ToLower() == ".zip")
			{
				SeekableZipStream zipStream = new SeekableZipStream(_archivePath, fileName);
				zipStream.PasswordRequired += new ZipPasswordRequiredHandler(unzip_PasswordRequired);
				if (showProgress)
					zipStream.ExtractionProgress += new ZipExtractionProgressHandler(unzip_ExtractionProgress);
				return zipStream;
			}
			throw new Exception("Unknown archive type.");
		}

		private void ShowProgress(string status, double percentTrack, double percentDisk, string input, string output)
		{
			if (this.CUEToolsProgress == null)
				return;
			_progress.status = status;
			_progress.percentTrck = percentTrack;
			_progress.percentDisk = percentDisk;
			_progress.offset = 0;
			_progress.input = input;
			_progress.output = output;
			this.CUEToolsProgress(this, _progress);
		}

		private void ShowProgress(string status, double percentTrack, int diskOffset, int diskLength, string input, string output)
		{
			if (this.CUEToolsProgress == null)
				return;
			_progress.status = status;
			_progress.percentTrck = percentTrack;
			_progress.percentDisk = (double)diskOffset / diskLength;
			_progress.offset = diskOffset;
			_progress.input = input;
			_progress.output = output;
			this.CUEToolsProgress(this, _progress);
		}

#if !MONO
		private void CDReadProgress(object sender, ReadProgressArgs e)
		{
			CheckStop();
			if (this.CUEToolsProgress == null)
				return;
			CDDriveReader audioSource = (CDDriveReader)sender;
			int processed = e.Position - e.PassStart;
			TimeSpan elapsed = DateTime.Now - e.PassTime;
			double speed = elapsed.TotalSeconds > 0 ? processed / elapsed.TotalSeconds / 75 : 1.0;
			_progress.percentDisk = (double)(e.PassStart + (processed + e.Pass * (e.PassEnd - e.PassStart)) / (audioSource.CorrectionQuality + 1)) / audioSource.TOC.AudioLength;
			_progress.percentTrck = (double) (e.Position - e.PassStart) / (e.PassEnd - e.PassStart);
			_progress.offset = 0;
			_progress.status = string.Format("Ripping @{0:00.00}x {1}", speed, e.Pass > 0 ? " (Retry " + e.Pass.ToString() + ")" : "");
			this.CUEToolsProgress(this, _progress);
		}

		private void MusicBrainz_LookupProgress(object sender, XmlRequestEventArgs e)
		{
			if (this.CUEToolsProgress == null)
				return;
			_progress.percentDisk = (1.0 + _progress.percentDisk) / 2;
			_progress.percentTrck = 0;
			_progress.offset = 0;
			_progress.input = e.Uri.ToString();
			_progress.output = null;
			_progress.status = "Looking up album via MusicBrainz";
			this.CUEToolsProgress(this, _progress);
		}

		private void unrar_ExtractionProgress(object sender, ExtractionProgressEventArgs e)
		{
			CheckStop();
			if (this.CUEToolsProgress == null)
				return;
			_progress.percentTrck = e.PercentComplete/100;
			this.CUEToolsProgress(this, _progress);
		}

		private void unrar_PasswordRequired(object sender, PasswordRequiredEventArgs e)
		{
			if (_archivePassword != null)
			{
				e.ContinueOperation = true;
				e.Password = _archivePassword;
				return;
			}
			if (this.PasswordRequired != null)
			{
				ArchivePasswordRequiredEventArgs e1 = new ArchivePasswordRequiredEventArgs();
				this.PasswordRequired(this, e1);
				if (e1.ContinueOperation && e1.Password != "")
				{
					_archivePassword = e1.Password;
					e.ContinueOperation = true;
					e.Password = e1.Password;
					return;
				} 
			}
			throw new IOException("Password is required for extraction.");
		}
#endif

		private void unzip_ExtractionProgress(object sender, ZipExtractionProgressEventArgs e)
		{
			CheckStop();
			if (this.CUEToolsProgress == null)
				return;
			_progress.percentTrck = e.PercentComplete / 100;
			this.CUEToolsProgress(this, _progress);
		}

		private void unzip_PasswordRequired(object sender, ZipPasswordRequiredEventArgs e)
		{
			if (_archivePassword != null)
			{
				e.ContinueOperation = true;
				e.Password = _archivePassword;
				return;
			}
			if (this.PasswordRequired != null)
			{
				ArchivePasswordRequiredEventArgs e1 = new ArchivePasswordRequiredEventArgs();
				this.PasswordRequired(this, e1);
				if (e1.ContinueOperation && e1.Password != "")
				{
					_archivePassword = e1.Password;
					e.ContinueOperation = true;
					e.Password = e1.Password;
					return;
				}
			}
			throw new IOException("Password is required for extraction.");
		}

		public delegate string GetStringTagProvider(TagLib.File file);
		
		public string GetCommonTag(GetStringTagProvider provider)
		{
			if (_hasEmbeddedCUESheet || _hasSingleFilename)
				return _fileInfo == null ? null : General.EmptyStringToNull(provider(_fileInfo));
			if (_hasTrackFilenames)
			{
				string tagValue = null;
				bool commonValue = true;
				for (int i = 0; i < TrackCount; i++)
				{
					TrackInfo track = _tracks[i];
					string newValue = track._fileInfo == null ? null:
						General.EmptyStringToNull(provider(track._fileInfo));
					if (tagValue == null)
						tagValue = newValue;
					else
						commonValue = (newValue == null || tagValue == newValue);
				}
				return commonValue ? tagValue : null;
			}
			return null;
		}

		public string GetCommonMiscTag(string tagName)
		{
			return GetCommonTag(delegate(TagLib.File file) { return Tagging.TagListToSingleValue(Tagging.GetMiscTag(file, tagName)); });
		}

		private static string LocateFile(string dir, string file, List<string> contents) 
		{
			List<string> dirList, fileList;
			string altDir;

			dirList = new List<string>();
			fileList = new List<string>();
			altDir = Path.GetDirectoryName(file);
			file = Path.GetFileName(file);

			dirList.Add(dir);
			if (altDir.Length != 0) {
				dirList.Add(Path.IsPathRooted(altDir) ? altDir : Path.Combine(dir, altDir));
			}

			fileList.Add(file);
			fileList.Add(file.Replace(' ', '_'));
			fileList.Add(file.Replace('_', ' '));

			for (int iDir = 0; iDir < dirList.Count; iDir++) {
				for (int iFile = 0; iFile < fileList.Count; iFile++) {
					string path = Path.Combine(dirList[iDir], fileList[iFile]);
					if ((contents == null && System.IO.File.Exists(path))
						|| (contents != null && contents.Contains(path)))
						return path;
					path = dirList[iDir] + '/' + fileList[iFile];
					if (contents != null && contents.Contains(path))
						return path;
				}
			}

			return null;
		}

		private static bool IsCDROM(string pathIn)
		{
			return pathIn.Length == 3 && pathIn.Substring(1) == ":\\" && new DriveInfo(pathIn).DriveType == DriveType.CDRom;
		}

		public string GenerateUniqueOutputPath(string format, string ext, CUEAction action, string pathIn)
		{
			return GenerateUniqueOutputPath(_config, format, ext, action, new NameValueCollection(), pathIn, this);
		}

		public static string GenerateUniqueOutputPath(CUEConfig _config, string format, string ext, CUEAction action, NameValueCollection vars, string pathIn, CUESheet cueSheet)
		{
			if (pathIn == "" || (pathIn == null && action != CUEAction.Encode) || (pathIn != null && !IsCDROM(pathIn) && !File.Exists(pathIn) && !Directory.Exists(pathIn)))
				return String.Empty;
			if (action == CUEAction.Verify && _config.arLogToSourceFolder)
				return Path.ChangeExtension(pathIn, ".cue");
			if (action == CUEAction.CreateDummyCUE)
				return Path.ChangeExtension(pathIn, ".cue");
			if (action == CUEAction.CorrectFilenames)
				return pathIn;

			if (_config.detectHDCD && _config.decodeHDCD && (!ext.StartsWith(".lossy.") || !_config.decodeHDCDtoLW16))
			{
				if (_config.decodeHDCDto24bit)
					ext = ".24bit" + ext;
				else
					ext = ".20bit" + ext;
			}

			if (pathIn != null)
			{
				vars.Add("path", pathIn);
				try
				{
					vars.Add("filename", Path.GetFileNameWithoutExtension(pathIn));
					vars.Add("filename_ext", Path.GetFileName(pathIn));
					vars.Add("directoryname", General.EmptyStringToNull(Path.GetDirectoryName(pathIn)));
				}
				catch { }
			}
			vars.Add("music", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
			string artist = cueSheet == null ? "Artist" : cueSheet.Artist == "" ? "Unknown Artist" : cueSheet.Artist;
			string album = cueSheet == null ? "Album" : cueSheet.Title == "" ? "Unknown Title" : cueSheet.Title;
			vars.Add("artist", General.EmptyStringToNull(_config.CleanseString(artist)));
			vars.Add("album", General.EmptyStringToNull(_config.CleanseString(album)));

			if (cueSheet != null)
			{
				vars.Add("year", General.EmptyStringToNull(cueSheet.Year));
				vars.Add("catalog", General.EmptyStringToNull(cueSheet.Catalog));
				vars.Add("discnumber", General.EmptyStringToNull(cueSheet.DiscNumber));
				vars.Add("totaldiscs", General.EmptyStringToNull(cueSheet.TotalDiscs));
				NameValueCollection tags = cueSheet.Tags;
				if (tags != null)
					foreach (string tag in tags.AllKeys)
					{
						string key = tag.ToLower();
						string val = tags[tag];
						if (vars.Get(key) == null && val != null && val != "")
							vars.Add(key, _config.CleanseString(val));
					}
			}

			vars.Add("unique", null);

			string outputPath = General.ReplaceMultiple(format, vars, "unique",
				(General.CheckIfExists)delegate(string pathOut) {
					return File.Exists(Path.ChangeExtension(pathOut, ext));
				});
			if (outputPath == "" || outputPath == null)
				return "";
			try { outputPath = Path.ChangeExtension(outputPath, ext); }
			catch { outputPath = ""; }
			return outputPath;
		}

		private bool CheckIfFileExists(string output) 
		{ 
			return File.Exists(Path.Combine(OutputDir, output)); 
		}

		public void GenerateFilenames(AudioEncoderType audioEncoderType, string format, string outputPath)
		{
			_audioEncoderType = audioEncoderType;
			_outputLossyWAV = format.StartsWith("lossy.");
			_outputFormat = format;
			_outputPath = outputPath;

			string extension = "." + format;
			string filename;
			int iTrack;

			NameValueCollection vars = new NameValueCollection();
			vars.Add("unique", null);
			vars.Add("album artist", General.EmptyStringToNull(_config.CleanseString(Artist)));
			vars.Add("artist", General.EmptyStringToNull(_config.CleanseString(Artist)));
			vars.Add("album", General.EmptyStringToNull(_config.CleanseString(Title)));
			vars.Add("year", General.EmptyStringToNull(_config.CleanseString(Year)));
			vars.Add("catalog", General.EmptyStringToNull(_config.CleanseString(Catalog)));
			vars.Add("discnumber", General.EmptyStringToNull(_config.CleanseString(DiscNumber)));
			vars.Add("totaldiscs", General.EmptyStringToNull(_config.CleanseString(TotalDiscs)));
			vars.Add("filename", Path.GetFileNameWithoutExtension(outputPath));
			vars.Add("tracknumber", null);
			vars.Add("title", null);
			
			if (_config.detectHDCD && _config.decodeHDCD && (!_outputLossyWAV || !_config.decodeHDCDtoLW16))
			{
				if (_config.decodeHDCDto24bit )
					extension = ".24bit" + extension;
				else
					extension = ".20bit" + extension;
			}

			ArLogFileName = General.ReplaceMultiple(_config.arLogFilenameFormat, vars, "unique", CheckIfFileExists) 
				?? vars["%filename%"] + ".accurip";
			AlArtFileName = General.ReplaceMultiple(_config.alArtFilenameFormat, vars, "unique", CheckIfFileExists) 
				?? "folder.jpg";

			if (OutputStyle == CUEStyle.SingleFileWithCUE)
				SingleFilename = Path.ChangeExtension(Path.GetFileName(outputPath), extension);
			else if (_config.keepOriginalFilenames && HasSingleFilename)
				SingleFilename = Path.ChangeExtension(SingleFilename, extension);
			else
				SingleFilename = (General.ReplaceMultiple(_config.singleFilenameFormat, vars) ?? "range") + extension;

			for (iTrack = -1; iTrack < TrackCount; iTrack++)
			{
				bool htoa = (iTrack == -1);

				if (_config.keepOriginalFilenames && htoa && HasHTOAFilename)
				{
					HTOAFilename = Path.ChangeExtension(HTOAFilename, extension);
				}
				else if (_config.keepOriginalFilenames && !htoa && HasTrackFilenames)
				{
					TrackFilenames[iTrack] = Path.ChangeExtension(
						TrackFilenames[iTrack], extension);
				}
				else
				{
					string trackStr = htoa ? "01.00" : String.Format("{0:00}", iTrack + 1);
					string artist = Tracks[htoa ? 0 : iTrack].Artist;
					string title = htoa ? "(HTOA)" : Tracks[iTrack].Title;

					vars["tracknumber"] = trackStr;
					vars["artist"] = General.EmptyStringToNull(_config.CleanseString(artist)) ?? vars["album artist"];
					vars["title"] = General.EmptyStringToNull(_config.CleanseString(title));

					filename = (General.ReplaceMultiple(_config.trackFilenameFormat, vars) ?? vars["tracknumber"]) + extension;

					if (htoa)
						HTOAFilename = filename;
					else
						TrackFilenames[iTrack] = filename;
				}
			}

			if (OutputStyle == CUEStyle.SingleFile || OutputStyle == CUEStyle.SingleFileWithCUE)
			{
				_destPaths = new string[1];
				_destPaths[0] = Path.Combine(OutputDir, _singleFilename);
			}
			else
			{
				bool htoaToFile = ((OutputStyle == CUEStyle.GapsAppended) && _config.preserveHTOA &&
					(_toc.Pregap != 0));
				_destPaths = new string[TrackCount + (htoaToFile ? 1 : 0)];
				if (htoaToFile)
					_destPaths[0] = Path.Combine(OutputDir, _htoaFilename);
				for (int i = 0; i < TrackCount; i++)
					_destPaths[i + (htoaToFile ? 1 : 0)] = Path.Combine(OutputDir, _trackFilenames[i]);
			}
		}

		public bool OutputExists()
		{
			bool outputExists = false;
			bool outputCUE = Action == CUEAction.Encode && (OutputStyle != CUEStyle.SingleFileWithCUE || _config.createCUEFileWhenEmbedded);
			bool outputAudio = Action == CUEAction.Encode && _audioEncoderType != AudioEncoderType.NoAudio;
			if (outputCUE)
				outputExists = File.Exists(_outputPath);
			if (_useAccurateRip && (
				(Action == CUEAction.Encode && _config.writeArLogOnConvert) ||
				(Action == CUEAction.Verify && _config.writeArLogOnVerify)))
				outputExists |= File.Exists(Path.Combine(OutputDir, ArLogFileName));
			if (outputAudio)
			{
				if (_config.extractAlbumArt && AlbumArt != null && AlbumArt.Length != 0)
					outputExists |= File.Exists(Path.Combine(OutputDir, AlArtFileName));
				if (OutputStyle == CUEStyle.SingleFile || OutputStyle == CUEStyle.SingleFileWithCUE)
					outputExists |= File.Exists(Path.Combine(OutputDir, SingleFilename));
				else
				{
					if (OutputStyle == CUEStyle.GapsAppended && _config.preserveHTOA)
						outputExists |= File.Exists(Path.Combine(OutputDir, HTOAFilename));
					for (int i = 0; i < TrackCount; i++)
						outputExists |= File.Exists(Path.Combine(OutputDir, TrackFilenames[i]));
				}
			}
			return outputExists;
		}

		private int GetSampleLength(string path, out TagLib.File fileInfo)
		{
			ShowProgress("Analyzing input file...", 0.0, 0.0, path, null);

			if (Path.GetExtension(path) == ".dummy" || Path.GetExtension(path) == ".bin")
			{
				fileInfo = null;
			} else
			{
				TagLib.UserDefined.AdditionalFileTypes.Config = _config;
				TagLib.File.IFileAbstraction file = _isArchive
					? (TagLib.File.IFileAbstraction)new ArchiveFileAbstraction(this, path)
					: (TagLib.File.IFileAbstraction)new TagLib.File.LocalFileAbstraction(path);
				fileInfo = TagLib.File.Create(file);
			}

			IAudioSource audioSource = AudioReadWrite.GetAudioSource(path, _isArchive ? OpenArchive(path, true) : null, _config);
			if (!audioSource.PCM.IsRedBook ||
				audioSource.Length <= 0 ||
				audioSource.Length >= Int32.MaxValue)
			{
				audioSource.Close();
				throw new Exception("Audio format is invalid.");
			}
			audioSource.Close();
			return (int)audioSource.Length;
		}

		public static void WriteText(string path, string text, Encoding encoding)
		{
			StreamWriter sw1 = new StreamWriter(path, false, encoding);
			sw1.Write(text);
			sw1.Close();
		}

		public static void WriteText(string path, string text)
		{
			bool utf8Required = CUESheet.Encoding.GetString(CUESheet.Encoding.GetBytes(text)) != text;
			WriteText(path, text, utf8Required ? Encoding.UTF8 : CUESheet.Encoding);
		}

		public string LOGContents
		{
			get
			{
				return _ripperLog;
			}
		}

#if !MONO
		public void CreateExactAudioCopyLOG()
		{
			StringWriter logWriter = new StringWriter(CultureInfo.InvariantCulture);
			string eacHeader = "Exact Audio Copy V0.99 prebeta 4 from 23. January 2008\r\n" +
				"\r\n" +
				"EAC extraction logfile from {0:d'.' MMMM yyyy', 'H':'mm}\r\n" +
				"\r\n" +
				"{1} / {2}\r\n" +
				"\r\n" +
				"Used drive  : {3}   Adapter: 1  ID: 0\r\n" +
				"\r\n" +
				"Read mode               : {4}\r\n" +
				"Utilize accurate stream : Yes\r\n" +
				"Defeat audio cache      : Yes\r\n" +
				"Make use of C2 pointers : No\r\n" +
				"\r\n" +
				"Read offset correction                      : {5}\r\n" +
				"Overread into Lead-In and Lead-Out          : No\r\n" +
				"Fill up missing offset samples with silence : Yes\r\n" +
				"Delete leading and trailing silent blocks   : No\r\n" +
				"Null samples used in CRC calculations       : Yes\r\n" +
				"Used interface                              : Native Win32 interface for Win NT & 2000\r\n" +
				"Gap handling                                : Appended to previous track\r\n" +
				"\r\n" +
				"Used output format : Internal WAV Routines\r\n" +
				"Sample format      : 44.100 Hz; 16 Bit; Stereo\r\n";

			logWriter.WriteLine(eacHeader, 
				DateTime.Now,
				Artist, Title, 
				_ripper.EACName, 
				_ripper.CorrectionQuality > 0 ? "Secure" : "Burst", 
				_ripper.DriveOffset);

			logWriter.WriteLine();
			logWriter.WriteLine("TOC of the extracted CD");
			logWriter.WriteLine();
			logWriter.WriteLine("     Track |   Start  |  Length  | Start sector | End sector ");
			logWriter.WriteLine("    ---------------------------------------------------------");
			for (int track = 1; track <= _toc.TrackCount; track++)
				logWriter.WriteLine("{0,9}  | {1,8} | {2,8} |  {3,8}    | {4,8}   ",
					_toc[track].Number,
					CDImageLayout.TimeToString("{0,2}:{1:00}.{2:00}", _toc[track].Start),
					CDImageLayout.TimeToString("{0,2}:{1:00}.{2:00}", _toc[track].Length),
					_toc[track].Start,
					_toc[track].End);
			logWriter.WriteLine();

			bool htoaToFile = ((OutputStyle == CUEStyle.GapsAppended) && _config.preserveHTOA &&
				(_toc.Pregap != 0));
			int accurateTracks = 0, knownTracks = 0;
			if (OutputStyle != CUEStyle.SingleFile && OutputStyle != CUEStyle.SingleFileWithCUE)
			{
				logWriter.WriteLine();
				for (int track = 0; track < _toc.AudioTracks; track++)
				{
					logWriter.WriteLine("Track {0,2}", track + 1);
					logWriter.WriteLine();
					logWriter.WriteLine("     Filename {0}", Path.ChangeExtension(Path.GetFullPath(_destPaths[track + (htoaToFile ? 1 : 0)]), ".wav"));
					if (_toc[track + _toc.FirstAudio].Pregap > 0 || track + _toc.FirstAudio == 1)
					{
						logWriter.WriteLine();
						logWriter.WriteLine("     Pre-gap length  0:{0}.{1:00}", CDImageLayout.TimeToString("{0:00}:{1:00}", _toc[track + _toc.FirstAudio].Pregap + (track + _toc.FirstAudio == 1 ? 150U : 0U)), (_toc[track + _toc.FirstAudio].Pregap % 75) * 100 / 75);
					}
					logWriter.WriteLine();
					logWriter.WriteLine("     Peak level {0:F1} %", (Tracks[track].PeakLevel * 1000 / 32768) * 0.1);
					logWriter.WriteLine("     Track quality 100.0 %");
					logWriter.WriteLine("     Test CRC {0:X8}", _arVerify.CRC32(track + 1));
					logWriter.WriteLine("     Copy CRC {0:X8}", _arVerify.CRC32(track + 1));
					if (_arVerify.Total(track) == 0)
						logWriter.WriteLine("     Track not present in AccurateRip database");
					else
					{
						knownTracks++;
						if (_arVerify.Confidence(track) == 0)
							logWriter.WriteLine("     Cannot be verified as accurate (confidence {0})  [{1:X8}], AccurateRip returned [{2:X8}]", _arVerify.Total(track), _arVerify.CRC(track), _arVerify.DBCRC(track));
						else
						{
							logWriter.WriteLine("     Accurately ripped (confidence {0})  [{1:X8}]", _arVerify.Confidence(track), _arVerify.CRC(track));
							accurateTracks++;
						}
					}
					logWriter.WriteLine("     Copy OK");
					logWriter.WriteLine();
				}
			}
			else
			{
				logWriter.WriteLine();
				logWriter.WriteLine("Range status and errors");
				logWriter.WriteLine();
				logWriter.WriteLine("Selected range");
				logWriter.WriteLine();
				logWriter.WriteLine("     Filename {0}", Path.ChangeExtension(Path.GetFullPath(_destPaths[0]), ".wav"));
				logWriter.WriteLine();
				int PeakLevel = 0;
				for (int track = 0; track < TrackCount; track++)
					if (PeakLevel < Tracks[track].PeakLevel)
						PeakLevel = Tracks[track].PeakLevel;
				logWriter.WriteLine("     Peak level {0:F1} %", (PeakLevel * 1000 / 32768) * 0.1);
				logWriter.WriteLine("     Range quality 100.0 %");
				logWriter.WriteLine("     Test CRC {0:X8}", _arVerify.CRC32(0));
				logWriter.WriteLine("     Copy CRC {0:X8}", _arVerify.CRC32(0));
				logWriter.WriteLine("     Copy OK");
				logWriter.WriteLine();
				logWriter.WriteLine("No errors occurred");
				logWriter.WriteLine();
				logWriter.WriteLine();
				logWriter.WriteLine("AccurateRip summary");
				logWriter.WriteLine();
				for (int track = 0; track < _toc.AudioTracks; track++)
				{
					if (_arVerify.Total(track) == 0)
						logWriter.WriteLine("Track {0,2}  not present in database", track + 1);
					else
					{
						knownTracks++;
						if (_arVerify.Confidence(track) == 0)
							logWriter.WriteLine("Track {3,2}  cannot be verified as accurate (confidence {0})  [{1:X8}], AccurateRip returned [{2:X8}]", _arVerify.Total(track), _arVerify.CRC(track), _arVerify.DBCRC(track), track + 1);
						else
						{
							logWriter.WriteLine("Track {2,2}  accurately ripped (confidence {0})  [{1:X8}]", _arVerify.Confidence(track), _arVerify.CRC(track), track + 1);
							accurateTracks++;
						}
					}
				}
			}
			logWriter.WriteLine();
			if (knownTracks == 0)
				logWriter.WriteLine("None of the tracks are present in the AccurateRip database");
			else if (accurateTracks == 0)
			{
				logWriter.WriteLine("No tracks could be verified as accurate");
				logWriter.WriteLine("You may have a different pressing from the one(s) in the database");
			}
			else if (accurateTracks == TrackCount)
				logWriter.WriteLine("All tracks accurately ripped");
			else
			{
				logWriter.WriteLine("{0,2} track(s) accurately ripped", accurateTracks);
				if (TrackCount - knownTracks > 0)
					logWriter.WriteLine("{0,2} track(s) not present in the AccurateRip database", TrackCount - knownTracks);
				logWriter.WriteLine();
				logWriter.WriteLine("Some tracks could not be verified as accurate");
			}
			logWriter.WriteLine();
			if (OutputStyle != CUEStyle.SingleFile && OutputStyle != CUEStyle.SingleFileWithCUE)
			{
				logWriter.WriteLine("No errors occurred");
				logWriter.WriteLine();
			}
			logWriter.WriteLine("End of status report");
			logWriter.Close();
			_ripperLog = logWriter.ToString();
		}
#endif

		public void CreateRipperLOG()
		{
			if (!_isCD || _ripper == null || _ripperLog != null)
				return;
#if !MONO
			if (_config.createEACLOG)
			{
				CreateExactAudioCopyLOG();
				return;
			}
			StringWriter logWriter = new StringWriter();
			logWriter.WriteLine("{0}", CDDriveReader.RipperVersion());
			logWriter.WriteLine("Extraction logfile from : {0}", DateTime.Now);
			logWriter.WriteLine("Used drive              : {0}", _ripper.ARName);
			logWriter.WriteLine("Read offset correction  : {0}", _ripper.DriveOffset);
			logWriter.WriteLine("Read command            : {0}", _ripper.CurrentReadCommand);
			logWriter.WriteLine("Secure mode             : {0}", _ripper.CorrectionQuality);
			logWriter.WriteLine("Disk length             : {0}", CDImageLayout.TimeToString(_toc.AudioLength));
			logWriter.WriteLine("AccurateRip             : {0}", _arVerify.ARStatus == null ? "ok" : _arVerify.ARStatus);
			if (hdcdDecoder != null && hdcdDecoder.Detected)
			{
				hdcd_decoder_statistics stats;
				hdcdDecoder.GetStatistics(out stats);
				logWriter.WriteLine("HDCD                    : peak extend: {0}, transient filter: {1}, gain: {2}",
					(stats.enabled_peak_extend ? (stats.disabled_peak_extend ? "some" : "yes") : "none"),
					(stats.enabled_transient_filter ? (stats.disabled_transient_filter ? "some" : "yes") : "none"),
					stats.min_gain_adjustment == stats.max_gain_adjustment ?
					(stats.min_gain_adjustment == 1.0 ? "none" : String.Format("{0:0.0}dB", (Math.Log10(stats.min_gain_adjustment) * 20))) :
					String.Format("{0:0.0}dB..{1:0.0}dB", (Math.Log10(stats.min_gain_adjustment) * 20), (Math.Log10(stats.max_gain_adjustment) * 20))
					);
				logWriter.WriteLine();
			}
			logWriter.WriteLine();
			logWriter.WriteLine("TOC of the extracted CD");
			logWriter.WriteLine();
			logWriter.WriteLine("     Track |   Start  |  Length  | Start sector | End sector");
			logWriter.WriteLine("    ---------------------------------------------------------");
			for (int track = 1; track <= _toc.TrackCount; track++)
				logWriter.WriteLine("{0,9}  | {1,8} | {2,8} |  {3,8}    | {4,8}",
					_toc[track].Number,
					_toc[track].StartMSF,
					_toc[track].LengthMSF,
					_toc[track].Start,
					_toc[track].End);
			logWriter.WriteLine();
			logWriter.WriteLine("     Track |   Pregap  | Indexes");
			logWriter.WriteLine("    ---------------------------------------------------------");
			for (int track = 1; track <= _toc.TrackCount; track++)
				logWriter.WriteLine("{0,9}  | {1,8} |    {2,2}",
					_toc[track].Number,
					CDImageLayout.TimeToString(_toc[track].Pregap + (track == 1 ? 150U : 0U)),
					_toc[track].LastIndex);
			logWriter.WriteLine();
			logWriter.WriteLine("Destination files");
			foreach (string path in _destPaths)
				logWriter.WriteLine("    {0}", path);
			bool wereErrors = false;
			for (int iTrack = 0; iTrack < _toc.AudioTracks; iTrack++)
			{
				int cdErrors = 0;
				for (uint iSector = _toc[iTrack + 1].Start; iSector <= _toc[iTrack + 1].End; iSector++)
					if (_ripper.Errors[(int)iSector])
						cdErrors++;
				if (cdErrors != 0)
				{
					if (!wereErrors)
					{
						logWriter.WriteLine();
						logWriter.WriteLine("Errors detected");
						logWriter.WriteLine();
					}
					wereErrors = true;
					logWriter.WriteLine("Track {0} contains {1} errors", iTrack + 1, cdErrors);
				}
			}
			if (_useAccurateRip)
			{
				logWriter.WriteLine();
				logWriter.WriteLine("AccurateRip summary");
				logWriter.WriteLine();
				_arVerify.GenerateFullLog(logWriter, true);
				logWriter.WriteLine();
			}
			logWriter.WriteLine();
			logWriter.WriteLine("End of status report");
			logWriter.Close();
			_ripperLog = logWriter.ToString();
#endif
		}

		public string M3UContents(CUEStyle style)
		{
			StringWriter sw = new StringWriter();
			if (style == CUEStyle.GapsAppended && _config.preserveHTOA && _toc.Pregap != 0)
				WriteLine(sw, 0, _htoaFilename);
			for (int iTrack = 0; iTrack < TrackCount; iTrack++)
				WriteLine(sw, 0, _trackFilenames[iTrack]);
			sw.Close();
			return sw.ToString();
		}

		public string TOCContents()
		{
			StringWriter sw = new StringWriter();
			for (int iTrack = 1; iTrack <= _toc.TrackCount; iTrack++)
				sw.WriteLine("\t{0}", _toc[iTrack].Start + 150);
			sw.Close();
			return sw.ToString();
		}

		public string CUESheetContents()
		{
			CUEStyle style = _hasEmbeddedCUESheet ? CUEStyle.SingleFile
				: _hasSingleFilename ? CUEStyle.SingleFileWithCUE
				: CUEStyle.GapsAppended;
			bool htoaToFile = _hasHTOAFilename;
			return CUESheetContents(style, htoaToFile);
		}

		public string CUESheetContents(CUEStyle style)
		{
			return CUESheetContents(style, (style == CUEStyle.GapsAppended && _config.preserveHTOA && _toc.Pregap != 0));
		}

		public string CUESheetContents(CUEStyle style, bool htoaToFile)
		{
			StringWriter sw = new StringWriter();
			int i, iTrack, iIndex;

			uint timeRelativeToFileStart = 0;

			using (sw) 
			{
				if (_config.writeArTagsOnEncode)
					WriteLine(sw, 0, "REM ACCURATERIPID " + (_accurateRipId ?? AccurateRipVerify.CalculateAccurateRipId(_toc)));

				for (i = 0; i < _attributes.Count; i++)
					WriteLine(sw, 0, _attributes[i]);

				if (style == CUEStyle.SingleFile || style == CUEStyle.SingleFileWithCUE)
					WriteLine(sw, 0, String.Format("FILE \"{0}\" WAVE", _singleFilename));

				if (htoaToFile)
					WriteLine(sw, 0, String.Format("FILE \"{0}\" WAVE", _htoaFilename));

				for (iTrack = 0; iTrack < TrackCount; iTrack++) 
				{
					if ((style == CUEStyle.GapsPrepended) ||
						(style == CUEStyle.GapsLeftOut) ||
						((style == CUEStyle.GapsAppended) &&
						((_toc[_toc.FirstAudio + iTrack].Pregap == 0) || ((iTrack == 0) && !htoaToFile))))
					{
						WriteLine(sw, 0, String.Format("FILE \"{0}\" WAVE", _trackFilenames[iTrack]));
						timeRelativeToFileStart = 0;
					}

					WriteLine(sw, 1, String.Format("TRACK {0:00} AUDIO", iTrack + 1));
					for (i = 0; i < _tracks[iTrack].Attributes.Count; i++)
						WriteLine(sw, 2, _tracks[iTrack].Attributes[i]);

					if (_toc[_toc.FirstAudio + iTrack].Pregap != 0)
					{
						if (((style == CUEStyle.GapsLeftOut) ||
							((style == CUEStyle.GapsAppended) && (iTrack == 0) && !htoaToFile) ||
							((style == CUEStyle.SingleFile || style == CUEStyle.SingleFileWithCUE) && (iTrack == 0) && _usePregapForFirstTrackInSingleFile)))
							WriteLine(sw, 2, "PREGAP " + CDImageLayout.TimeToString(_toc[_toc.FirstAudio + iTrack].Pregap));
						else
						{
							WriteLine(sw, 2, String.Format("INDEX 00 {0}", CDImageLayout.TimeToString(timeRelativeToFileStart)));
							timeRelativeToFileStart += _toc[_toc.FirstAudio + iTrack].Pregap;
							if (style == CUEStyle.GapsAppended)
							{
								WriteLine(sw, 0, String.Format("FILE \"{0}\" WAVE", _trackFilenames[iTrack]));
								timeRelativeToFileStart = 0;
							}
						}
					}
					for (iIndex = 1; iIndex <= _toc[_toc.FirstAudio + iTrack].LastIndex; iIndex++)
					{
						WriteLine(sw, 2, String.Format( "INDEX {0:00} {1}", iIndex, CDImageLayout.TimeToString(timeRelativeToFileStart)));
						timeRelativeToFileStart += _toc.IndexLength(_toc.FirstAudio + iTrack, iIndex);
					}
				}
			}
			sw.Close();
			return sw.ToString();
		}

		public void GenerateAccurateRipLog(TextWriter sw)
		{
			sw.WriteLine("[Verification date: {0}]", DateTime.Now);
			sw.WriteLine("[Disc ID: {0}]", _accurateRipId ?? AccurateRipVerify.CalculateAccurateRipId(_toc));
			if (PreGapLength != 0)
				sw.WriteLine("Pregap length {0}.", PreGapLengthMSF);
			if (!_toc[1].IsAudio)
				sw.WriteLine("Playstation type data track length {0}.", _toc[1].LengthMSF);
			if (!_toc[_toc.TrackCount].IsAudio)
				sw.WriteLine("CD-Extra data track length {0}.", 
					_toc[_toc.TrackCount].Length == 0 && _minDataTrackLength.HasValue ? 
						CDImageLayout.TimeToString(_minDataTrackLength.Value) + " - " + CDImageLayout.TimeToString(_minDataTrackLength.Value + 74) : 
						_toc[_toc.TrackCount].LengthMSF );
			if (_cddbDiscIdTag != null && AccurateRipVerify.CalculateCDDBId(_toc).ToUpper() != _cddbDiscIdTag.ToUpper() && !_minDataTrackLength.HasValue)
				sw.WriteLine("CDDBId mismatch: {0} vs {1}", _cddbDiscIdTag.ToUpper(), AccurateRipVerify.CalculateCDDBId(_toc).ToUpper());
			if (_accurateRipId != null && AccurateRipVerify.CalculateAccurateRipId(_toc) != _accurateRipId)
				sw.WriteLine("Using preserved id, actual id is {0}.", AccurateRipVerify.CalculateAccurateRipId(_toc));
			if (_truncated4608)
				sw.WriteLine("Truncated 4608 extra samples in some input files.");
			if (_paddedToFrame)
				sw.WriteLine("Padded some input files to a frame boundary.");

			if (hdcdDecoder != null && hdcdDecoder.Detected)
			{
				hdcd_decoder_statistics stats;
				hdcdDecoder.GetStatistics(out stats);
				sw.WriteLine("HDCD: peak extend: {0}, transient filter: {1}, gain: {2}",
					(stats.enabled_peak_extend ? (stats.disabled_peak_extend ? "some" : "yes") : "none"),
					(stats.enabled_transient_filter ? (stats.disabled_transient_filter ? "some" : "yes") : "none"),
					stats.min_gain_adjustment == stats.max_gain_adjustment ? 
					(stats.min_gain_adjustment == 1.0 ? "none" : String.Format ("{0:0.0}dB", (Math.Log10(stats.min_gain_adjustment) * 20))) :
					String.Format ("{0:0.0}dB..{1:0.0}dB", (Math.Log10(stats.min_gain_adjustment) * 20), (Math.Log10(stats.max_gain_adjustment) * 20))
					);
			}

			if (0 != _writeOffset)
				sw.WriteLine("Offset applied: {0}", _writeOffset);
			_arVerify.GenerateFullLog(sw, _config.arLogVerbose);
		}

		public string GenerateAccurateRipStatus()
		{
			string prefix = "";
			if (hdcdDecoder != null && hdcdDecoder.Detected)
				prefix += "hdcd detected, ";
			if (_useAccurateRip)
			{
				if (_arVerify.ARStatus != null)
					prefix += _arVerify.ARStatus;
				else
				{
					uint tracksMatch = 0;
					int bestOffset = 0;
					FindBestOffset(1, false, out tracksMatch, out bestOffset);
					if (bestOffset != 0)
						prefix += string.Format("offset {0}, ", bestOffset);
					if (tracksMatch == TrackCount)
						prefix += string.Format("rip accurate ({0}/{1})", _arVerify.WorstConfidence(), _arVerify.WorstTotal());
					else
						prefix += "rip not accurate";
				}
			} else
				prefix += "done";
			return prefix;
		}

		public void GenerateAccurateRipTagsForTrack(NameValueCollection tags, int bestOffset, int iTrack, string prefix)
		{
			tags.Add(String.Format("{0}ACCURATERIPCRC", prefix), String.Format("{0:x8}", _arVerify.CRC(iTrack, 0)));
			tags.Add(String.Format("{0}AccurateRipDiscId", prefix), String.Format("{0:000}-{1}-{2:00}", TrackCount, _accurateRipId ?? AccurateRipVerify.CalculateAccurateRipId(_toc), iTrack + 1));
			tags.Add(String.Format("{0}ACCURATERIPCOUNT", prefix), String.Format("{0}", _arVerify.Confidence(iTrack, 0)));
			tags.Add(String.Format("{0}ACCURATERIPCOUNTALLOFFSETS", prefix), String.Format("{0}", _arVerify.SumConfidence(iTrack)));
			tags.Add(String.Format("{0}ACCURATERIPTOTAL", prefix), String.Format("{0}", _arVerify.Total(iTrack)));
			if (bestOffset != 0)
				tags.Add(String.Format("{0}ACCURATERIPCOUNTWITHOFFSET", prefix), String.Format("{0}", _arVerify.Confidence(iTrack, bestOffset)));
		}

		public void GenerateAccurateRipTags(NameValueCollection tags, int bestOffset, int iTrack)
		{
			tags.Add("ACCURATERIPID", _accurateRipId ?? AccurateRipVerify.CalculateAccurateRipId(_toc));
			if (bestOffset != 0)
				tags.Add("ACCURATERIPOFFSET", String.Format("{1}{0}", bestOffset, bestOffset > 0 ? "+" : ""));
			if (iTrack != -1)
				GenerateAccurateRipTagsForTrack(tags, bestOffset, iTrack, "");
			else
			for (iTrack = 0; iTrack < TrackCount; iTrack++)
			{
				GenerateAccurateRipTagsForTrack(tags, bestOffset, iTrack,
					String.Format("cue_track{0:00}_", iTrack + 1));
			}
		}

		public void CleanupTags (NameValueCollection tags, string substring)
		{
			string [] keys = tags.AllKeys;
			for (int i = 0; i < keys.Length; i++)
				if (keys[i].ToUpper().Contains(substring))
					tags.Remove (keys[i]);
		}

		public void FindBestOffset(uint minConfidence, bool optimizeConfidence, out uint outTracksMatch, out int outBestOffset)
		{
			uint bestTracksMatch = 0;
			uint bestConfidence = 0;
			int bestOffset = 0;

			for (int offset = -_arOffsetRange; offset <= _arOffsetRange; offset++)
			{
				uint tracksMatch = 0;
				uint sumConfidence = 0;

				for (int iTrack = 0; iTrack < TrackCount; iTrack++)
				{
					uint confidence = 0;

					for (int di = 0; di < (int)_arVerify.AccDisks.Count; di++)
						if (_arVerify.CRC(iTrack, offset) == _arVerify.AccDisks[di].tracks[iTrack].CRC)
							confidence += _arVerify.AccDisks[di].tracks[iTrack].count;

					if (confidence >= minConfidence)
						tracksMatch++;

					sumConfidence += confidence;
				}

				if (tracksMatch > bestTracksMatch
					|| (tracksMatch == bestTracksMatch && optimizeConfidence && sumConfidence > bestConfidence)
					|| (tracksMatch == bestTracksMatch && optimizeConfidence && sumConfidence == bestConfidence && Math.Abs(offset) < Math.Abs(bestOffset))
					|| (tracksMatch == bestTracksMatch && !optimizeConfidence && Math.Abs(offset) < Math.Abs(bestOffset))
					)
				{
					bestTracksMatch = tracksMatch;
					bestConfidence = sumConfidence;
					bestOffset = offset;
				}
			}
			outBestOffset = bestOffset;
			outTracksMatch = bestTracksMatch;
		}

		public string Go()
		{
			int[] destLengths;
			bool htoaToFile = ((OutputStyle == CUEStyle.GapsAppended) && _config.preserveHTOA &&
				(_toc.Pregap != 0));

			if (_isCD && (OutputStyle == CUEStyle.GapsLeftOut || OutputStyle == CUEStyle.GapsPrepended))
				throw new Exception("Gaps Left Out/Gaps prepended modes cannot be used when ripping a CD");

			if (_usePregapForFirstTrackInSingleFile)
				throw new Exception("UsePregapForFirstTrackInSingleFile is not supported for writing audio files.");

			if (_action != CUEAction.Verify)
				for (int i = 0; i < _destPaths.Length; i++)
					for (int j = 0; j < _sourcePaths.Count; j++)
						if (_destPaths[i].ToLower() == _sourcePaths[j].ToLower())
							throw new Exception("Source and destination audio file paths cannot be the same.");

			destLengths = CalculateAudioFileLengths(OutputStyle);

			// TODO: if (_isCD) might need to recalc, might have changed after scanning the CD

			// Lookup();
			
			if (_action != CUEAction.Verify)
			{
				if (!Directory.Exists(OutputDir))
					Directory.CreateDirectory(OutputDir);
			}
			
			if (_audioEncoderType != AudioEncoderType.NoAudio || _action == CUEAction.Verify)
				WriteAudioFilesPass(OutputDir, OutputStyle, destLengths, htoaToFile, _action == CUEAction.Verify);

			CreateRipperLOG();

			if (_action == CUEAction.Encode)
			{
				string cueContents = CUESheetContents(OutputStyle);
				uint tracksMatch = 0;
				int bestOffset = 0;

				if (_useAccurateRip &&
					_config.writeArTagsOnEncode &&
					_arVerify.AccResult == HttpStatusCode.OK)
					FindBestOffset(1, true, out tracksMatch, out bestOffset);

				if (_config.createEACLOG)
				{
					if (_ripperLog != null)
						_ripperLog = CUESheet.Encoding.GetString(CUESheet.Encoding.GetBytes(_ripperLog));
					cueContents = CUESheet.Encoding.GetString(CUESheet.Encoding.GetBytes(cueContents));
				}

				if (_ripperLog != null)
					WriteText(Path.ChangeExtension(_outputPath, ".log"), _ripperLog);
				else
					if (_eacLog != null && _config.extractLog)
						WriteText(Path.ChangeExtension(_outputPath, ".log"), _eacLog);

				if (_audioEncoderType != AudioEncoderType.NoAudio && _config.extractAlbumArt)
					ExtractAlbumArt();

				if (OutputStyle == CUEStyle.SingleFileWithCUE || OutputStyle == CUEStyle.SingleFile)
				{
					if (OutputStyle == CUEStyle.SingleFileWithCUE && _config.createCUEFileWhenEmbedded)
						WriteText(Path.ChangeExtension(_outputPath, ".cue"), cueContents);
					if (OutputStyle == CUEStyle.SingleFile)
						WriteText(_outputPath, cueContents);
					if (_audioEncoderType != AudioEncoderType.NoAudio)
					{
						NameValueCollection tags = GenerateAlbumTags(bestOffset, OutputStyle == CUEStyle.SingleFileWithCUE, _ripperLog ?? _eacLog);
						TagLib.UserDefined.AdditionalFileTypes.Config = _config;
						TagLib.File fileInfo = TagLib.File.Create(new TagLib.File.LocalFileAbstraction(_destPaths[0]));
						if (Tagging.UpdateTags(fileInfo, tags, _config))
						{
							TagLib.File sourceFileInfo = _tracks[0]._fileInfo ?? _fileInfo;
							
							// first, use cue sheet information
							if (_config.writeBasicTagsFromCUEData)
							{
								uint temp;
								if (fileInfo.Tag.Album == null && Title != "") 
									fileInfo.Tag.Album = Title;
								if (fileInfo.Tag.Performers.Length == 0 && Artist != "")
									fileInfo.Tag.Performers = new string[] { Artist };
								//if (fileInfo.Tag.AlbumArtists.Length == 0 && Artist != "")
								//    fileInfo.Tag.AlbumArtists = new string[] { Artist };
								if (fileInfo.Tag.Genres.Length == 0 && Genre != "") 
									fileInfo.Tag.Genres = new string[] { Genre };
								if (fileInfo.Tag.DiscCount == 0 && TotalDiscs != "" && uint.TryParse(TotalDiscs, out temp))
									fileInfo.Tag.DiscCount = temp;
								if (fileInfo.Tag.Disc == 0 && DiscNumber != "" && uint.TryParse(DiscNumber, out temp)) 
									fileInfo.Tag.Disc = temp;
								if (fileInfo.Tag.Year == 0 && Year != "" && uint.TryParse(Year, out temp))
									fileInfo.Tag.Year = temp;
							}
							
							// fill up missing information from tags
							if (_config.copyBasicTags && sourceFileInfo != null)
							{
								if (fileInfo.Tag.DiscCount == 0)
									fileInfo.Tag.DiscCount = sourceFileInfo.Tag.DiscCount; // TODO: GetCommonTag?
								if (fileInfo.Tag.Disc == 0)
									fileInfo.Tag.Disc = sourceFileInfo.Tag.Disc;
								//fileInfo.Tag.Performers = sourceFileInfo.Tag.Performers;
								if (fileInfo.Tag.Album == null)
									fileInfo.Tag.Album = sourceFileInfo.Tag.Album;
								if (fileInfo.Tag.Performers.Length == 0)
									fileInfo.Tag.Performers = sourceFileInfo.Tag.Performers;
								if (fileInfo.Tag.AlbumArtists.Length == 0)
									fileInfo.Tag.AlbumArtists = sourceFileInfo.Tag.AlbumArtists;
								if (fileInfo.Tag.Genres.Length == 0)
									fileInfo.Tag.Genres = sourceFileInfo.Tag.Genres;
								if (fileInfo.Tag.Year == 0)
									fileInfo.Tag.Year = sourceFileInfo.Tag.Year;
							}

							if ((_config.embedAlbumArt || _config.copyAlbumArt) && _albumArt != null && _albumArt.Length > 0)
								fileInfo.Tag.Pictures = _albumArt;

							fileInfo.Save();
						}
					}
				}
				else
				{
					WriteText(_outputPath, cueContents);
					if (_config.createM3U)
						WriteText(Path.ChangeExtension(_outputPath, ".m3u"), M3UContents(OutputStyle));
					bool fNeedAlbumArtist = false;
					for (int iTrack = 1; iTrack < TrackCount; iTrack++)
						if (_tracks[iTrack].Artist != _tracks[0].Artist)
							fNeedAlbumArtist = true;
					if (_audioEncoderType != AudioEncoderType.NoAudio)
						for (int iTrack = 0; iTrack < TrackCount; iTrack++)
						{
							string path = _destPaths[iTrack + (htoaToFile ? 1 : 0)];
							NameValueCollection tags = GenerateTrackTags(iTrack, bestOffset);
							TagLib.UserDefined.AdditionalFileTypes.Config = _config;
							TagLib.File fileInfo = TagLib.File.Create(new TagLib.File.LocalFileAbstraction(path));
							if (Tagging.UpdateTags(fileInfo, tags, _config))
							{
								TagLib.File sourceFileInfo = _tracks[iTrack]._fileInfo ?? _fileInfo;

								if (_config.writeBasicTagsFromCUEData)
								{
									uint temp;
									fileInfo.Tag.TrackCount = (uint)TrackCount;
									fileInfo.Tag.Track = (uint)iTrack + 1;
									if (fileInfo.Tag.Title == null && _tracks[iTrack].Title != "")
										fileInfo.Tag.Title = _tracks[iTrack].Title;
									if (fileInfo.Tag.Album == null && Title != "") 
										fileInfo.Tag.Album = Title;
									if (fileInfo.Tag.Performers.Length == 0 && _tracks[iTrack].Artist != "") 
										fileInfo.Tag.Performers = new string[] { _tracks[iTrack].Artist };
									if (fileInfo.Tag.Performers.Length == 0 && Artist != "") 
										fileInfo.Tag.Performers = new string[] { Artist };
									if (fNeedAlbumArtist && fileInfo.Tag.AlbumArtists.Length == 0 && Artist != "") 
										fileInfo.Tag.AlbumArtists = new string[] { Artist };
									if (fileInfo.Tag.Genres.Length == 0 && Genre != "") 
										fileInfo.Tag.Genres = new string[] { Genre };
									if (fileInfo.Tag.DiscCount == 0 && TotalDiscs != "" && uint.TryParse(TotalDiscs, out temp))
										fileInfo.Tag.DiscCount = temp;
									if (fileInfo.Tag.Disc == 0 && DiscNumber != "" && uint.TryParse(DiscNumber, out temp))
										fileInfo.Tag.Disc = temp;
									if (fileInfo.Tag.Year == 0 && Year != "" && uint.TryParse(Year, out temp))
										fileInfo.Tag.Year = temp;
								}

								if (_config.copyBasicTags && sourceFileInfo != null)
								{
									if (fileInfo.Tag.Title == null && _tracks[iTrack]._fileInfo != null)
										fileInfo.Tag.Title = _tracks[iTrack]._fileInfo.Tag.Title;
									if (fileInfo.Tag.DiscCount == 0)
										fileInfo.Tag.DiscCount = sourceFileInfo.Tag.DiscCount;
									if (fileInfo.Tag.Disc == 0)
										fileInfo.Tag.Disc = sourceFileInfo.Tag.Disc;
									if (fileInfo.Tag.Performers.Length == 0)
										fileInfo.Tag.Performers = sourceFileInfo.Tag.Performers;
									if (fileInfo.Tag.AlbumArtists.Length == 0)
										fileInfo.Tag.AlbumArtists = sourceFileInfo.Tag.AlbumArtists;
									if (fileInfo.Tag.Album == null)
										fileInfo.Tag.Album = sourceFileInfo.Tag.Album;
									if (fileInfo.Tag.Year == 0)
										fileInfo.Tag.Year = sourceFileInfo.Tag.Year;
									if (fileInfo.Tag.Genres.Length == 0)
										fileInfo.Tag.Genres = sourceFileInfo.Tag.Genres;
								}

								if ((_config.embedAlbumArt || _config.copyAlbumArt) && _albumArt != null && _albumArt.Length > 0)
									fileInfo.Tag.Pictures = _albumArt;

								fileInfo.Save();
							}
						}
				}
			}

			return WriteReport();
		}

		private static Bitmap resizeImage(Image imgToResize, Size size)
		{
			int sourceWidth = imgToResize.Width;
			int sourceHeight = imgToResize.Height;

			float nPercent = 0;
			float nPercentW = 0;
			float nPercentH = 0;

			nPercentW = ((float)size.Width / (float)sourceWidth);
			nPercentH = ((float)size.Height / (float)sourceHeight);

			if (nPercentH < nPercentW)
				nPercent = nPercentH;
			else
				nPercent = nPercentW;

			int destWidth = (int)(sourceWidth * nPercent);
			int destHeight = (int)(sourceHeight * nPercent);

			Bitmap b = new Bitmap(destWidth, destHeight);
			Graphics g = Graphics.FromImage((Image)b);
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;

			g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
			g.Dispose();

			return b;
		}

		public void ExtractAlbumArt()
		{
			if (!_config.extractAlbumArt || _albumArt == null || _albumArt.Length == 0)
				return;

			string imgPath = Path.Combine(OutputDir, AlArtFileName);
			if (File.Exists(imgPath))
				return;

			foreach (TagLib.IPicture picture in _albumArt)
				using (FileStream file = new FileStream(imgPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
				{
					file.Write(picture.Data.Data, 0, picture.Data.Count);
					return;
				}
		}

		public void LoadAlbumArt(TagLib.File fileInfo)
		{
			if ((_config.extractAlbumArt || _config.copyAlbumArt) && fileInfo != null)
				foreach (TagLib.IPicture picture in fileInfo.Tag.Pictures)
					if (picture.Type == TagLib.PictureType.FrontCover)
						if (picture.MimeType == "image/jpeg")
						{
							_albumArt = new TagLib.IPicture[] { picture };
							return;
						}
			if ((_config.extractAlbumArt || _config.embedAlbumArt) && _inputDir != null)
			{
				string imgPath = Path.Combine(_inputDir, "folder.jpg");
				if (!File.Exists(imgPath))
					imgPath = Path.Combine(_inputDir, "cover.jpg");
				if (!File.Exists(imgPath))
					return;
				_albumArt = new TagLib.IPicture[] { new TagLib.Picture(imgPath) };
			}				
		}

		public void ResizeAlbumArt()
		{
			if (_albumArt == null)
				return;
			foreach (TagLib.IPicture picture in _albumArt)
				using (MemoryStream imageStream = new MemoryStream(picture.Data.Data, 0, picture.Data.Count))
				using (Image img = Image.FromStream(imageStream))
					if (img.Width > _config.maxAlbumArtSize || img.Height > _config.maxAlbumArtSize)
					{
						using (Bitmap small = resizeImage(img, new Size(_config.maxAlbumArtSize, _config.maxAlbumArtSize)))
						using (MemoryStream encoded = new MemoryStream())
						{
							//System.Drawing.Imaging.EncoderParameters encoderParams = new EncoderParameters(1);
							//encoderParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(Encoder.Quality, quality);
							small.Save(encoded, System.Drawing.Imaging.ImageFormat.Jpeg);
							picture.Data = new TagLib.ByteVector(encoded.ToArray());
							picture.MimeType = "image/jpeg";
						}
					}
		}

		public TagLib.IPicture[] AlbumArt
		{
			get
			{
				return _albumArt;
			}
		}

		public Image Cover
		{
			get
			{
				if (AlbumArt == null || AlbumArt.Length == 0)
					return null;
				TagLib.IPicture picture = AlbumArt[0];
				using (MemoryStream imageStream = new MemoryStream(picture.Data.Data, 0, picture.Data.Count))
					try { return Image.FromStream(imageStream); }
					catch { }
				return null;
			}
		}

		public string WriteReport()
		{
			if (_useAccurateRip)
			{
				ShowProgress((string)"Generating AccurateRip report...", 0, 0, null, null);
				if (_action == CUEAction.Verify && _config.writeArTagsOnVerify && _writeOffset == 0 && !_isArchive && !_isCD)
				{
					uint tracksMatch;
					int bestOffset;
					FindBestOffset(1, true, out tracksMatch, out bestOffset);

					if (_hasEmbeddedCUESheet)
					{
						if (_fileInfo is TagLib.Flac.File)
						{
							NameValueCollection tags = Tagging.Analyze(_fileInfo);
							CleanupTags(tags, "ACCURATERIP");
							GenerateAccurateRipTags(tags, bestOffset, -1);
							if (Tagging.UpdateTags(_fileInfo, tags, _config))
								_fileInfo.Save();
						}
					} else if (_hasTrackFilenames)
					{
						for (int iTrack = 0; iTrack < TrackCount; iTrack++)
							if (_tracks[iTrack]._fileInfo is TagLib.Flac.File)
							{
								NameValueCollection tags = Tagging.Analyze(_tracks[iTrack]._fileInfo);
								CleanupTags(tags, "ACCURATERIP");
								GenerateAccurateRipTags(tags, bestOffset, iTrack);
								if (Tagging.UpdateTags(_tracks[iTrack]._fileInfo, tags, _config))
									_tracks[iTrack]._fileInfo.Save();
							}
					}
				}

				if ((_action != CUEAction.Verify && _config.writeArLogOnConvert) ||
					(_action == CUEAction.Verify && _config.writeArLogOnVerify))
				{
					if (!Directory.Exists(OutputDir))
						Directory.CreateDirectory(OutputDir);
					StreamWriter sw = new StreamWriter(Path.Combine(OutputDir, ArLogFileName),
						false, CUESheet.Encoding);
					GenerateAccurateRipLog(sw);
					sw.Close();
				}
				if (_config.createTOC)
				{
					if (!Directory.Exists(OutputDir))
						Directory.CreateDirectory(OutputDir);
					WriteText(Path.ChangeExtension(_outputPath, ".toc"), TOCContents());
				}
			}
			return GenerateAccurateRipStatus();
		}

		private NameValueCollection GenerateTrackTags(int iTrack, int bestOffset)
		{
			NameValueCollection destTags = new NameValueCollection();

			if (_config.copyUnknownTags)
			{
				if (_hasEmbeddedCUESheet)
				{
					string trackPrefix = String.Format("cue_track{0:00}_", iTrack + 1);
					NameValueCollection albumTags = Tagging.Analyze(_fileInfo);
					foreach (string key in albumTags.AllKeys)
					{
						if (key.ToLower().StartsWith(trackPrefix)
							|| !key.ToLower().StartsWith("cue_track"))
						{
							string name = key.ToLower().StartsWith(trackPrefix) ?
								key.Substring(trackPrefix.Length) : key;
							string[] values = albumTags.GetValues(key);
							for (int j = 0; j < values.Length; j++)
								destTags.Add(name, values[j]);
						}
					}
				}
				else if (_hasTrackFilenames)
					destTags.Add(Tagging.Analyze(_tracks[iTrack]._fileInfo));
				else if (_hasSingleFilename)
				{
					// TODO?
				}

				// these will be set explicitely
				destTags.Remove("ARTIST");
				destTags.Remove("TITLE");
				destTags.Remove("ALBUM");
				destTags.Remove("ALBUMARTIST");
				destTags.Remove("DATE");
				destTags.Remove("GENRE");
				destTags.Remove("TRACKNUMBER");
				destTags.Remove("TRACKTOTAL");
				destTags.Remove("TOTALTRACKS");
				destTags.Remove("DISCNUMBER");
				destTags.Remove("DISCTOTAL");
				destTags.Remove("TOTALDISCS");

				destTags.Remove("LOG");
				destTags.Remove("LOGFILE");
				destTags.Remove("EACLOG");

				// these are not valid
				destTags.Remove("CUESHEET");
				CleanupTags(destTags, "ACCURATERIP");
				//CleanupTags(destTags, "REPLAYGAIN");
			}

			if (_config.writeArTagsOnEncode && _action == CUEAction.Encode && _useAccurateRip && _arVerify.AccResult == HttpStatusCode.OK)
				GenerateAccurateRipTags(destTags, bestOffset, iTrack);

			return destTags;
		}

		private NameValueCollection GenerateAlbumTags(int bestOffset, bool fWithCUE, string logContents)
		{
			NameValueCollection destTags = new NameValueCollection();

			if (_config.copyUnknownTags)
			{
				if (_hasEmbeddedCUESheet || _hasSingleFilename)
				{
					destTags.Add(Tagging.Analyze(_fileInfo));
					if (!fWithCUE)
						CleanupTags(destTags, "CUE_TRACK");
				}
				else if (_hasTrackFilenames)
				{
					for (int iTrack = 0; iTrack < TrackCount; iTrack++)
					{
						NameValueCollection trackTags = Tagging.Analyze(_tracks[iTrack]._fileInfo);
						foreach (string key in trackTags.AllKeys)
						{
							string singleValue = GetCommonMiscTag(key);
							if (singleValue != null)
							{
								if (destTags.Get(key) == null)
									destTags.Add(key, singleValue);
							}
							else if (fWithCUE && key.ToUpper() != "TRACKNUMBER" && key.ToUpper() != "TITLE" && key.ToUpper() != "ARTIST")
							{
								string[] values = trackTags.GetValues(key);
								for (int j = 0; j < values.Length; j++)
									destTags.Add(String.Format("cue_track{0:00}_{1}", iTrack + 1, key), values[j]);
							}
						}
					}
				}

				// these will be set explicitely
				destTags.Remove("ARTIST");
				destTags.Remove("TITLE");
				destTags.Remove("ALBUM");
				destTags.Remove("ALBUMARTIST");
				destTags.Remove("DATE");
				destTags.Remove("GENRE");
				destTags.Remove("TRACKNUMBER");
				destTags.Remove("TRACKTOTAL");
				destTags.Remove("TOTALTRACKS");
				destTags.Remove("DISCNUMBER");
				destTags.Remove("DISCTOTAL");
				destTags.Remove("TOTALDISCS");
				destTags.Remove("LOG");
				destTags.Remove("LOGFILE");
				destTags.Remove("EACLOG");

				// these are not valid
				CleanupTags(destTags, "ACCURATERIP");
				//CleanupTags(destTags, "REPLAYGAIN");

				destTags.Remove("CUESHEET");
			}

			if (fWithCUE)
				destTags.Add("CUESHEET", CUESheetContents(CUEStyle.SingleFileWithCUE));

			if (_config.embedLog && logContents != null)
				destTags.Add("LOG", logContents);

			if (fWithCUE && _config.writeArTagsOnEncode && _action == CUEAction.Encode && _useAccurateRip && _arVerify.AccResult == HttpStatusCode.OK)
				GenerateAccurateRipTags(destTags, bestOffset, -1);

			return destTags;
		}

		public void WriteAudioFilesPass(string dir, CUEStyle style, int[] destLengths, bool htoaToFile, bool noOutput)
		{
			int iTrack, iIndex;
			AudioBuffer sampleBuffer = new AudioBuffer(AudioPCMConfig.RedBook, 0x10000);
			TrackInfo track;
			IAudioSource audioSource = null;
			IAudioDest audioDest = null;
			bool discardOutput;
			int iSource = -1;
			int iDest = -1;
			int samplesRemSource = 0;
			//CDImageLayout updatedTOC = null;

			if (_writeOffset != 0)
			{
				int absOffset = Math.Abs(_writeOffset);
				SourceInfo sourceInfo;

				sourceInfo.Path = null;
				sourceInfo.Offset = 0;
				sourceInfo.Length = (uint)absOffset;

				if (_writeOffset < 0)
				{
					_sources.Insert(0, sourceInfo);

					int last = _sources.Count - 1;
					while (absOffset >= _sources[last].Length)
					{
						absOffset -= (int)_sources[last].Length;
						_sources.RemoveAt(last--);
					}
					sourceInfo = _sources[last];
					sourceInfo.Length -= (uint)absOffset;
					_sources[last] = sourceInfo;
				}
				else
				{
					_sources.Add(sourceInfo);

					while (absOffset >= _sources[0].Length)
					{
						absOffset -= (int)_sources[0].Length;
						_sources.RemoveAt(0);
					}
					sourceInfo = _sources[0];
					sourceInfo.Offset += (uint)absOffset;
					sourceInfo.Length -= (uint)absOffset;
					_sources[0] = sourceInfo;
				}

				_appliedWriteOffset = true;
			}

			if (_config.detectHDCD)
			{
				// currently broken verifyThenConvert on HDCD detection!!!! need to check for HDCD results higher
				try { hdcdDecoder = new HDCDDotNet.HDCDDotNet(2, 44100, ((_outputLossyWAV && _config.decodeHDCDtoLW16) || !_config.decodeHDCDto24bit) ? 20 : 24, _config.decodeHDCD); }
				catch { }
			}

			if (style == CUEStyle.SingleFile || style == CUEStyle.SingleFileWithCUE)
			{
				iDest++;
				audioDest = GetAudioDest(_destPaths[iDest], destLengths[iDest], hdcdDecoder != null && hdcdDecoder.Decoding ? hdcdDecoder.BitsPerSample : 16, _padding, noOutput);
			}

			int currentOffset = 0, previousOffset = 0;
			int trackLength = (int)_toc.Pregap * 588;
			int diskLength = 588 * (int)_toc.AudioLength;
			int diskOffset = 0;

			if (_useAccurateRip)
				_arVerify.Init();

			ShowProgress(String.Format("{2} track {0:00} ({1:00}%)...", 0, 0, noOutput ? "Verifying" : "Writing"), 0, 0.0, null, null);

#if !DEBUG
			try
#endif
			{
				for (iTrack = 0; iTrack < TrackCount; iTrack++)
				{
					track = _tracks[iTrack];

					if ((style == CUEStyle.GapsPrepended) || (style == CUEStyle.GapsLeftOut))
					{
						iDest++;
						if (hdcdDecoder != null)
							hdcdDecoder.AudioDest = null;
						if (audioDest != null)
							audioDest.Close();
						audioDest = GetAudioDest(_destPaths[iDest], destLengths[iDest], hdcdDecoder != null && hdcdDecoder.Decoding ? hdcdDecoder.BitsPerSample : 16, _padding, noOutput);
					}

					for (iIndex = 0; iIndex <= _toc[_toc.FirstAudio + iTrack].LastIndex; iIndex++)
					{
						int samplesRemIndex = (int)_toc.IndexLength(_toc.FirstAudio + iTrack, iIndex) * 588;

						if (iIndex == 1)
						{
							previousOffset = currentOffset;
							currentOffset = 0;
							trackLength = (int)_toc[_toc.FirstAudio + iTrack].Length * 588;
						}

						if ((style == CUEStyle.GapsAppended) && (iIndex == 1))
						{
							if (hdcdDecoder != null)
								hdcdDecoder.AudioDest = null;
							if (audioDest != null)
								audioDest.Close();
							iDest++;
							audioDest = GetAudioDest(_destPaths[iDest], destLengths[iDest], hdcdDecoder != null && hdcdDecoder.Decoding ? hdcdDecoder.BitsPerSample : 16, _padding, noOutput);
						}

						if ((style == CUEStyle.GapsAppended) && (iIndex == 0) && (iTrack == 0))
						{
							discardOutput = !htoaToFile;
							if (htoaToFile)
							{
								iDest++;
								audioDest = GetAudioDest(_destPaths[iDest], destLengths[iDest], hdcdDecoder != null && hdcdDecoder.Decoding ? hdcdDecoder.BitsPerSample : 16, _padding, noOutput);
							}
						}
						else if ((style == CUEStyle.GapsLeftOut) && (iIndex == 0))
						{
							discardOutput = true;
						}
						else
						{
							discardOutput = false;
						}

						while (samplesRemIndex != 0)
						{
							if (samplesRemSource == 0)
							{
//#if !MONO
//                                if (_isCD && audioSource != null && audioSource is CDDriveReader)
//                                    updatedTOC = ((CDDriveReader)audioSource).TOC;
//#endif
								if (audioSource != null && !_isCD) audioSource.Close();
								audioSource = GetAudioSource(++iSource);
								samplesRemSource = (int)_sources[iSource].Length;
							}

							int copyCount = Math.Min(samplesRemIndex, samplesRemSource);

							if (trackLength > 0 && !_isCD)
							{
								double trackPercent = (double)currentOffset / trackLength;
								ShowProgress(String.Format("{2} track {0:00} ({1:00}%)...", iIndex > 0 ? iTrack + 1 : iTrack, (uint)(100 * trackPercent),
									noOutput ? "Verifying" : "Writing"), trackPercent, (int)diskOffset, (int)diskLength,
									_isCD ? string.Format("{0}: {1:00} - {2}", audioSource.Path, iTrack + 1, _tracks[iTrack].Title) : audioSource.Path, discardOutput ? null : audioDest.Path);
							}

							copyCount = audioSource.Read(sampleBuffer, copyCount);
							if (!discardOutput)
							{
								if (!_config.detectHDCD || !_config.decodeHDCD)
									audioDest.Write(sampleBuffer);
								if (_config.detectHDCD && hdcdDecoder != null)
								{
									if (_config.wait750FramesForHDCD && diskOffset > 750 * 588 && !hdcdDecoder.Detected)
									{
										hdcdDecoder.AudioDest = null;
										hdcdDecoder = null;
										if (_config.decodeHDCD)
										{
											if (!_isCD) audioSource.Close();
											audioDest.Delete();
											throw new Exception("HDCD not detected.");
										}
									}
									else
									{
										if (_config.decodeHDCD)
											hdcdDecoder.AudioDest = (discardOutput || noOutput) ? null : audioDest;
										hdcdDecoder.Process(sampleBuffer.Samples, copyCount);
									}
								}
							}
							if (_useAccurateRip)
							{
								_arVerify.Write(sampleBuffer);
								if (iTrack > 0 || iIndex > 0)
									Tracks[iTrack + (iIndex == 0 ? -1 : 0)].MeasurePeakLevel(sampleBuffer.Samples, copyCount);
							}

							currentOffset += copyCount;
							diskOffset += copyCount;
							samplesRemIndex -= copyCount;
							samplesRemSource -= copyCount;

							CheckStop();
						}
					}
				}
			}
#if !DEBUG
			catch (Exception ex)
			{
				if (hdcdDecoder != null)
					hdcdDecoder.AudioDest = null;
				hdcdDecoder = null;
				try { if (audioSource != null && !_isCD) audioSource.Close(); }
				catch { }
				audioSource = null;
				try { if (audioDest != null) audioDest.Delete(); } 
				catch { }
				audioDest = null;
				throw ex;
			}
#endif

#if !MONO
			//if (_isCD && audioSource != null && audioSource is CDDriveReader)
			//    updatedTOC = ((CDDriveReader)audioSource).TOC;
			if (_isCD)
			{
				_toc = (CDImageLayout)_ripper.TOC.Clone();
				if (_toc.Catalog != null)
					Catalog = _toc.Catalog;
				for (iTrack = 0; iTrack < _toc.AudioTracks; iTrack++)
				{
					if (_toc[_toc.FirstAudio + iTrack].ISRC != null)
						General.SetCUELine(_tracks[iTrack].Attributes, "ISRC", _toc[_toc.FirstAudio + iTrack].ISRC, false);
					if (_toc[_toc.FirstAudio + iTrack].DCP || _toc[_toc.FirstAudio + iTrack].PreEmphasis)
						_tracks[iTrack].Attributes.Add(new CUELine("FLAGS" + (_toc[_toc.FirstAudio + iTrack].PreEmphasis ? " PRE" : "") + (_toc[_toc.FirstAudio + iTrack].DCP ? " DCP" : "")));
				}
			}
#endif

			if (hdcdDecoder != null)
				hdcdDecoder.AudioDest = null;
			if (audioSource != null && !_isCD)
				audioSource.Close();
			if (audioDest != null)
				audioDest.Close();
		}

		public static string CreateDummyCUESheet(CUEConfig _config, string pathIn)
		{
			pathIn = Path.GetFullPath(pathIn);
			List<FileGroupInfo> fileGroups = CUESheet.ScanFolder(_config, Path.GetDirectoryName(pathIn));
			FileGroupInfo fileGroup = FileGroupInfo.WhichContains(fileGroups, pathIn, FileGroupInfoType.TrackFiles)
				?? FileGroupInfo.WhichContains(fileGroups, pathIn, FileGroupInfoType.FileWithCUE);
			return fileGroup == null ? null : CreateDummyCUESheet(_config, fileGroup);
		}

		public static string CreateDummyCUESheet(CUEConfig _config, FileGroupInfo fileGroup)
		{
			if (fileGroup.type == FileGroupInfoType.FileWithCUE)
			{
				TagLib.UserDefined.AdditionalFileTypes.Config = _config;
				TagLib.File.IFileAbstraction fileAbsraction = new TagLib.File.LocalFileAbstraction(fileGroup.main.FullName);
				TagLib.File fileInfo = TagLib.File.Create(fileAbsraction);
				return Tagging.Analyze(fileInfo).Get("CUESHEET");
			}

			StringWriter sw = new StringWriter();
			sw.WriteLine(String.Format("REM COMMENT \"CUETools generated dummy CUE sheet\""));
			int trackNo = 0;
			foreach (FileSystemInfo file in fileGroup.files)
			{
				sw.WriteLine(String.Format("FILE \"{0}\" WAVE", file.Name));
				sw.WriteLine(String.Format("  TRACK {0:00} AUDIO", ++trackNo));
				sw.WriteLine(String.Format("    INDEX 01 00:00:00"));
			}
			sw.Close();
			return sw.ToString();
		}

		public static string CorrectAudioFilenames(CUEConfig _config, string path, bool always)
		{
			StreamReader sr = new StreamReader(path, CUESheet.Encoding);
			string cue = sr.ReadToEnd();
			sr.Close();
			string extension;
			return CorrectAudioFilenames(_config, Path.GetDirectoryName(path), cue, always, null, out extension);
		}

		public static string CorrectAudioFilenames(CUEConfig _config, string dir, string cue, bool always, List<string> files, out string extension)
		{
			List<string> lines = new List<string>();
			List<int> filePos = new List<int>();
			List<string> origFiles = new List<string>();
			bool foundAll = true;
			string[] audioFiles = null;
			string lineStr;
			CUELine line;
			int i;

			using (StringReader sr = new StringReader(cue))
			{
				while ((lineStr = sr.ReadLine()) != null)
				{
					lines.Add(lineStr);
					line = new CUELine(lineStr);
					if ((line.Params.Count == 3) && (line.Params[0].ToUpper() == "FILE"))
					{
						string fileType = line.Params[2].ToUpper();
						if ((fileType != "BINARY") && (fileType != "MOTOROLA"))
						{
							filePos.Add(lines.Count - 1);
							origFiles.Add(line.Params[1]);
							foundAll &= (LocateFile(dir, line.Params[1], files) != null);
						}
					}
				}
				sr.Close();
			}


			extension = null;
			if (foundAll && !always)
				return cue;

			foundAll = false;
			
			foreach (KeyValuePair<string, CUEToolsFormat> format in _config.formats)
			{
				List<string> newFiles = new List<string>();
				for (int j = 0; j < origFiles.Count; j++)
				{
					string newFilename = Path.ChangeExtension(Path.GetFileName(origFiles[j]), "." + format.Key);
					string locatedFilename = LocateFile(dir, newFilename, files);
					if (locatedFilename != null)
						newFiles.Add(locatedFilename);
				}
				if (newFiles.Count == origFiles.Count)
				{
					audioFiles = newFiles.ToArray();
					extension = format.Key;
					foundAll = true;
					break;
				}
			}

			if (!foundAll)
				foreach (KeyValuePair<string, CUEToolsFormat> format in _config.formats)
				{
					if (files == null)
						audioFiles = Directory.GetFiles(dir == "" ? "." : dir, "*." + format.Key);
					else
					{
						audioFiles = files.FindAll(delegate(string s)
						{
							return Path.GetDirectoryName(s) == dir && Path.GetExtension(s).ToLower() == "." + format.Key;
						}).ToArray();
					}
					if (audioFiles.Length == filePos.Count)
					{
						Array.Sort(audioFiles);
						extension = format.Key;
						foundAll = true;
						break;
					}
				}

			if (!foundAll)
				throw new Exception("unable to locate the audio files");

			for (i = 0; i < filePos.Count; i++)
				lines[filePos[i]] = "FILE \"" + Path.GetFileName(audioFiles[i]) + "\" WAVE";

			using (StringWriter sw = new StringWriter())
			{
				for (i = 0; i < lines.Count; i++)
				{
					sw.WriteLine(lines[i]);
				}
				return sw.ToString();
			}
		}

		private int[] CalculateAudioFileLengths(CUEStyle style) 
		{
			int iTrack, iIndex, iFile;
			TrackInfo track;
			int[] fileLengths;
			bool htoaToFile = (style == CUEStyle.GapsAppended && _config.preserveHTOA && _toc.Pregap != 0);
			bool discardOutput;

			if (style == CUEStyle.SingleFile || style == CUEStyle.SingleFileWithCUE) {
				fileLengths = new int[1];
				iFile = 0;
			}
			else {
				fileLengths = new int[TrackCount + (htoaToFile ? 1 : 0)];
				iFile = -1;
			}

			for (iTrack = 0; iTrack < TrackCount; iTrack++) {
				track = _tracks[iTrack];

				if (style == CUEStyle.GapsPrepended || style == CUEStyle.GapsLeftOut)
					iFile++;

				for (iIndex = 0; iIndex <= _toc[_toc.FirstAudio + iTrack].LastIndex; iIndex++)
				{
					if (style == CUEStyle.GapsAppended && (iIndex == 1 || (iIndex == 0 && iTrack == 0 && htoaToFile)))
						iFile++;

					if (style == CUEStyle.GapsAppended && iIndex == 0 && iTrack == 0) 
						discardOutput = !htoaToFile;
					else 
						discardOutput = (style == CUEStyle.GapsLeftOut && iIndex == 0);

					if (!discardOutput)
						fileLengths[iFile] += (int)_toc.IndexLength(_toc.FirstAudio + iTrack, iIndex) * 588;
				}
			}

			return fileLengths;
		}

		public void CheckStop()
		{
			lock (this)
			{
				if (_stop)
					throw new StopException();
				if (_pause)
				{
					ShowProgress("Paused...", 0, 0, null, null);
					Monitor.Wait(this);
				}
			}
		}

		public void Stop() {
			lock (this) {
				if (_pause)
				{
					_pause = false;
					Monitor.Pulse(this);
				}
				_stop = true;
			}
		}

		public void Pause()
		{
			lock (this)
			{
				if (_pause)
				{
					_pause = false;
					Monitor.Pulse(this);
				} else
				{
					_pause = true;
				}
			}
		}

		public int TrackCount {
			get {
				return _tracks.Count;
			}
		}

		public string OutputPath
		{
			get
			{
				return _outputPath;
			}
		}

		public string OutputDir
		{
			get
			{
				string outDir = Path.GetDirectoryName(_outputPath);
				return outDir == "" ? "." :  outDir;
			}
		}

		public CDImageLayout TOC
		{
			get
			{
				return _toc;
			}
			set
			{
				_toc = new CDImageLayout(value);
				if (Tracks.Count == 0)
				{
					for (int iTrack = 0; iTrack < _toc.AudioTracks; iTrack++)
					{
						//_trackFilenames.Add(string.Format("{0:00}.wav", iTrack + 1));
						_tracks.Add(new TrackInfo());
					}
				}
			}
		}

		private IAudioDest GetAudioDest(string path, int finalSampleCount, int bps, int padding, bool noOutput) 
		{
			if (noOutput)
				return new DummyWriter(path, new AudioPCMConfig(bps, 2, 44100));
			return AudioReadWrite.GetAudioDest(_audioEncoderType, path, finalSampleCount, bps, 44100, padding, _config);
		}

		private IAudioSource GetAudioSource(int sourceIndex) {
			SourceInfo sourceInfo = _sources[sourceIndex];
			IAudioSource audioSource;

			if (sourceInfo.Path == null) {
				audioSource = new SilenceGenerator(sourceInfo.Offset + sourceInfo.Length);
			}
			else {
#if !MONO
				if (_isCD)
				{
					_ripper.Position = 0;
					//audioSource = _ripper;
					audioSource = new AudioPipe(_ripper, 0x100000);
				} else
#endif
				if (_isArchive)
					audioSource = AudioReadWrite.GetAudioSource(sourceInfo.Path, OpenArchive(sourceInfo.Path, false), _config);
				else
					audioSource = AudioReadWrite.GetAudioSource(sourceInfo.Path, null, _config);
			}

			if (sourceInfo.Offset != 0)
				audioSource.Position = sourceInfo.Offset;

			//audioSource = new AudioPipe(audioSource, 0x10000);

			return audioSource;
		}

		private void WriteLine(TextWriter sw, int level, CUELine line) {
			WriteLine(sw, level, line.ToString());
		}

		private void WriteLine(TextWriter sw, int level, string line) {
			sw.Write(new string(' ', level * 2));
			sw.WriteLine(line);
		}

		public List<CUELine> Attributes {
			get {
				return _attributes;
			}
		}

		public List<TrackInfo> Tracks {
			get { 
				return _tracks;
			}
		}

		public bool HasHTOAFilename {
			get {
				return _hasHTOAFilename;
			}
		}

		public string HTOAFilename {
			get {
				return _htoaFilename;
			}
			set {
				_htoaFilename = value;
			}
		}

		public bool HasTrackFilenames {
			get {
				return _hasTrackFilenames;
			}
		}

		public List<string> TrackFilenames {
			get {
				return _trackFilenames;
			}
		}

		public bool HasSingleFilename {
			get {
				return _hasSingleFilename;
			}
		}

		public string SingleFilename {
			get {
				return _singleFilename;
			}
			set {
				_singleFilename = value;
			}
		}

		public string ArLogFileName
		{
			get
			{
				return _arLogFileName;
			}
			set
			{
				_arLogFileName = value;
			}
		}
		
		public string AlArtFileName
		{
			get
			{
				return _alArtFileName;
			}
			set
			{
				_alArtFileName = value;
			}
		}

		public NameValueCollection Tags
		{
			get
			{
				TagLib.File fileInfo = _tracks[0]._fileInfo ?? _fileInfo;
				return fileInfo != null ? Tagging.Analyze(fileInfo) : null;
			}
		}

		public string Artist {
			get {
				CUELine line = General.FindCUELine(_attributes, "PERFORMER");
				return (line == null || line.Params.Count < 2) ? String.Empty : line.Params[1];
			}
			set {
				General.SetCUELine(_attributes, "PERFORMER", value, true);
			}
		}

		public string Year
		{
			get
			{
				CUELine line = General.FindCUELine(_attributes, "REM", "DATE");
				return ( line == null || line.Params.Count < 3 ) ? String.Empty : line.Params[2];
			}
			set
			{
				if (value != "")
					General.SetCUELine(_attributes, "REM", "DATE", value, false);
				else
					General.DelCUELine(_attributes, "REM", "DATE");
			}
		}

		public string DiscNumber
		{
			get
			{
				CUELine line = General.FindCUELine(_attributes, "REM", "DISCNUMBER");
				return (line == null || line.Params.Count < 3) ? String.Empty : line.Params[2];
			}
			set
			{
				if (value != "")
					General.SetCUELine(_attributes, "REM", "DISCNUMBER", value, false);
				else
					General.DelCUELine(_attributes, "REM", "DISCNUMBER");
			}
		}

		public string TotalDiscs
		{
			get
			{
				CUELine line = General.FindCUELine(_attributes, "REM", "TOTALDISCS");
				return (line == null || line.Params.Count < 3) ? String.Empty : line.Params[2];
			}
			set
			{
				if (value != "")
					General.SetCUELine(_attributes, "REM", "TOTALDISCS", value, false);
				else
					General.DelCUELine(_attributes, "REM", "TOTALDISCS");
			}
		}

		public string Genre
		{
			get
			{
				CUELine line = General.FindCUELine(_attributes, "REM", "GENRE");
				return (line == null  || line.Params.Count < 3) ? String.Empty : line.Params[2];
			}
			set
			{
				if (value != "")
					General.SetCUELine(_attributes, "REM", "GENRE", value, true);
				else
					General.DelCUELine(_attributes, "REM", "GENRE");
			}
		}

		public string Catalog
		{
			get
			{
				CUELine line = General.FindCUELine(_attributes, "CATALOG");
				return (line == null || line.Params.Count < 2) ? String.Empty : line.Params[1];
			}
			set
			{
				if (value != "")
					General.SetCUELine(_attributes, "CATALOG", value, false);
				else
					General.DelCUELine(_attributes, "CATALOG");
			}
		}

		public string Title {
			get {
				CUELine line = General.FindCUELine(_attributes, "TITLE");
				return (line == null || line.Params.Count < 2) ? String.Empty : line.Params[1];
			}
			set {
				General.SetCUELine(_attributes, "TITLE", value, true);
			}
		}

		public int WriteOffset {
			get {
				return _writeOffset;
			}
			set {
				if (_appliedWriteOffset) {
					throw new Exception("Cannot change write offset after audio files have been written.");
				}
				_writeOffset = value;
			}
		}

		public bool PaddedToFrame {
			get {
				return _paddedToFrame;
			}
		}

		public uint DataTrackLength
		{
			get
			{
				if (!_toc[1].IsAudio)
					return _toc[1].Length;
				else if (!_toc[_toc.TrackCount].IsAudio)
					return _toc[_toc.TrackCount].Length;
				else
					return 0U;
			}
			set
			{
				if (value == 0)
					return;
				if (!_toc[1].IsAudio)
				{
					for (int i = 2; i <= _toc.TrackCount; i++)
					{
						_toc[i].Start += value - _toc[1].Length;
						for (int j = 0; j <= _toc[i].LastIndex; j++)
							_toc[i][j].Start += value - _toc[1].Length;
					}
					_toc[1].Length = value;
				}
				else if (!_toc[_toc.TrackCount].IsAudio)
				{
					//_toc[_toc.TrackCount].Start = tocFromLog[_toc.TrackCount].Start;
					_toc[_toc.TrackCount].Length = value;
					//_toc[_toc.TrackCount][0].Start = tocFromLog[_toc.TrackCount].Start;
					//_toc[_toc.TrackCount][1].Start = tocFromLog[_toc.TrackCount].Start;
				}
				else
					_toc.AddTrack(new CDTrack((uint)_toc.TrackCount, _toc.Length + 152U * 75U, value, false, false));
			}
		}

		public string DataTrackLengthMSF
		{
			get
			{
				return CDImageLayout.TimeToString(DataTrackLength);
			}
			set
			{
				DataTrackLength = (uint) CDImageLayout.TimeFromString(value);
			}
		}

		public string PreGapLengthMSF
		{
			get
			{
				return CDImageLayout.TimeToString(_toc.Pregap);
			}
			set
			{
				PreGapLength = (uint) CDImageLayout.TimeFromString(value);
			}
		}

		public uint PreGapLength
		{
			get
			{
				return _toc.Pregap;
			}
			set
			{
				if (value == _toc.Pregap || value == 0)
					return;
				if (!_toc[1].IsAudio)
					throw new Exception("can't set pregap to a data track");
				if (value < _toc.Pregap)
					throw new Exception("can't set negative pregap");
				uint offs = value - _toc.Pregap;
				for (int i = 1; i <= _toc.TrackCount; i++)
				{
					_toc[i].Start += offs;
					for (int j = 0; j <= _toc[i].LastIndex; j++)
						_toc[i][j].Start += offs;
				}
				_toc[1][0].Start = 0;

				SourceInfo sourceInfo;
				sourceInfo.Path = null;
				sourceInfo.Offset = 0;
				sourceInfo.Length = offs * 588;
				_sources.Insert(0, sourceInfo);
			}
		}

		public bool UsePregapForFirstTrackInSingleFile {
			get {
				return _usePregapForFirstTrackInSingleFile;
			}
			set{
				_usePregapForFirstTrackInSingleFile = value;
			}
		}

		public CUEConfig Config
		{
			get
			{
				return _config;
			}
		}

		public CUEAction Action
		{
			get
			{
				return _action;
			}
			set
			{
				_action = value;
			}
		}

		public CUEStyle OutputStyle
		{
			get
			{
				return _outputStyle;
			}
			set
			{
				_outputStyle = value;
			}
		}

		public bool IsCD
		{
			get
			{
				return _isCD;
			}
		}

		public static List<FileGroupInfo> ScanFolder(CUEConfig _config, string path)
		{
			DirectoryInfo dir = new DirectoryInfo(path);
			return ScanFolder(_config, dir.GetFileSystemInfos());
		}

		public static List<FileGroupInfo> ScanFolder(CUEConfig _config, IEnumerable<FileSystemInfo> files)
		{
			List<FileGroupInfo> fileGroups = new List<FileGroupInfo>();
			foreach (FileSystemInfo file in files)
			{
				if ((file.Attributes & FileAttributes.Hidden) != 0)
					continue;
				if ((file.Attributes & FileAttributes.Directory) != 0)
				{
					// foreach (FileSystemInfo subfile in ((DirectoryInfo)e.file).GetFileSystemInfos())
					// if (IsVisible(subfile))
					// {
					//     e.isExpandable = true;
					//  break;
					// }
					fileGroups.Add(new FileGroupInfo(file, FileGroupInfoType.Folder));
					continue;
				}
				string ext = file.Extension.ToLower();
				if (ext == ".cue")
				{
					fileGroups.Add(new FileGroupInfo(file, FileGroupInfoType.CUESheetFile));
					continue;
				}
				if (ext == ".zip")
				{
					fileGroups.Add(new FileGroupInfo(file, FileGroupInfoType.Archive));
					//try
					//{
					//    using (ICSharpCode.SharpZipLib.Zip.ZipFile unzip = new ICSharpCode.SharpZipLib.Zip.ZipFile(file.FullName))
					//    {
					//        foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry entry in unzip)
					//        {
					//            if (entry.IsFile && Path.GetExtension(entry.Name).ToLower() == ".cue")
					//            {
					//                e.node.Nodes.Add(fileSystemTreeView1.NewNode(file, false));
					//                break;
					//            }

					//        }
					//        unzip.Close();
					//    }
					//}
					//catch
					//{
					//}
					continue;
				}
				if (ext == ".rar")
				{
					fileGroups.Add(new FileGroupInfo(file, FileGroupInfoType.Archive));
					continue;
				}
				CUEToolsFormat fmt;
				if (ext.StartsWith(".") && _config.formats.TryGetValue(ext.Substring(1), out fmt) && fmt.allowLossless)
				{
					uint disc = 0;
					bool cueFound = false;
					TagLib.UserDefined.AdditionalFileTypes.Config = _config;
					TagLib.File.IFileAbstraction fileAbsraction = new TagLib.File.LocalFileAbstraction(file.FullName);
					try
					{
						TagLib.File fileInfo = TagLib.File.Create(fileAbsraction);
						disc = fileInfo.Tag.Disc;
						cueFound = fmt.allowEmbed && Tagging.Analyze(fileInfo).Get("CUESHEET") != null;
					}
					catch { }
					if (cueFound)
					{
						fileGroups.Add(new FileGroupInfo(file, FileGroupInfoType.FileWithCUE));
						continue;
					}
					disc = Math.Min(5, Math.Max(1, disc));
					FileGroupInfo groupFound = null;
					foreach (FileGroupInfo fileGroup in fileGroups)
					{
						if (fileGroup.type == FileGroupInfoType.TrackFiles && fileGroup.discNo == disc && fileGroup.main.Extension.ToLower() == ext)
						{
							groupFound = fileGroup;
							break;
						}
					}
					if (groupFound != null)
					{
						groupFound.files.Add(file);
					}
					else
					{
						groupFound = new FileGroupInfo(file, FileGroupInfoType.TrackFiles);
						groupFound.discNo = disc;
						groupFound.files.Add(file);
						fileGroups.Add(groupFound);
						// TODO: tracks must be sorted according to tracknumer (or filename if missing)
					}
				}
			}
			fileGroups.RemoveAll(new Predicate<FileGroupInfo>(FileGroupInfo.IsExcessive));
			return fileGroups;
		}

		public string AccurateRipLog
		{
			get
			{
				using (StringWriter logWriter = new StringWriter())
				{
					GenerateAccurateRipLog(logWriter);
					return logWriter.ToString();
				}
			}
		}

		public string ExecuteScript(CUEToolsScript script)
		{
			if (!script.builtin)
				return ExecuteScript(script.code);

			switch (script.name)
			{
				case "default":
					return Go();
				case "only if found":
					return ArVerify.AccResult != HttpStatusCode.OK ? WriteReport() : Go();
				case "fix offset":
					{
						if (ArVerify.AccResult != HttpStatusCode.OK)
							return WriteReport();

						WriteOffset = 0;
						Action = CUEAction.Verify;
						string status = Go();

						uint tracksMatch;
						int bestOffset;
						FindBestOffset(Config.fixOffsetMinimumConfidence, !Config.fixOffsetToNearest, out tracksMatch, out bestOffset);
						if (tracksMatch * 100 >= Config.fixOffsetMinimumTracksPercent * TrackCount)
						{
							WriteOffset = bestOffset;
							Action = CUEAction.Encode;
							status = Go();
						}
						return status;
					}

				case "encode if verified":
					{
						if (ArVerify.AccResult != HttpStatusCode.OK)
							return WriteReport();

						Action = CUEAction.Verify;
						string status = Go();

						uint tracksMatch;
						int bestOffset;
						FindBestOffset(Config.encodeWhenConfidence, false, out tracksMatch, out bestOffset);
						if (tracksMatch * 100 >= Config.encodeWhenPercent * TrackCount && (!_config.encodeWhenZeroOffset || bestOffset == 0))
						{
							Action = CUEAction.Encode;
							status = Go();
						}
						return status;
					}
			}

			return "internal error";
		}

		public string ExecuteScript(string script)
		{
			AsmHelper helper = CompileScript(script);
			return (string)helper.Invoke("*.Execute", this);
		}

		public static AsmHelper CompileScript(string script)
		{
			//CSScript.GlobalSettings.InMemoryAsssembly = true;
			//CSScript.GlobalSettings.HideAutoGeneratedFiles =
			//CSScript.CacheEnabled = false;
			return new AsmHelper(CSScript.LoadCode("using System; using System.Windows.Forms; using System.Net; using CUETools.Processor; using CUETools.Codecs; using CUETools.AccurateRip; public class Script { "
				+ "public static string Execute(CUESheet processor) { \r\n"
				+ script
				+ "\r\n } "
				+ " }", null, true));
		}

		public static bool TryCompileScript(string script)
		{
			AsmHelper helper = CompileScript(script);
			return helper != null;
		}
	}

	public enum FileGroupInfoType
	{
		Folder,
		Archive,
		CUESheetFile,
		FileWithCUE,
		TrackFiles
	}

	public class FileGroupInfo
	{
		public List<FileSystemInfo> files;
		public FileSystemInfo main;
		public FileGroupInfoType type;
		public uint discNo;

		public FileGroupInfo(FileSystemInfo _main, FileGroupInfoType _type)
		{
			main = _main;
			type = _type;
			files = new List<FileSystemInfo>();
		}

		public static bool IsExcessive(FileGroupInfo group)
		{
			return group.type == FileGroupInfoType.TrackFiles && group.files.Count < 2;
		}

		public bool Contains(string pathIn)
		{
			if (type != FileGroupInfoType.TrackFiles)
				return main.FullName.ToLower() == pathIn.ToLower();
			bool found = false;
			foreach (FileSystemInfo file in files)
				if (file.FullName.ToLower() == pathIn.ToLower())
					found = true;
			return found;
		}

		public static FileGroupInfo WhichContains(IEnumerable<FileGroupInfo> fileGroups, string pathIn, FileGroupInfoType type)
		{
			foreach (FileGroupInfo fileGroup in fileGroups)
			{
				if (fileGroup.type == type && fileGroup.Contains(pathIn))
					return fileGroup;
			}
			return null;
		}
	}

	public class ArchiveFileAbstraction : TagLib.File.IFileAbstraction
	{
		private string name;
		private CUESheet _cueSheet;

		public ArchiveFileAbstraction(CUESheet cueSheet, string file)
		{
			name = file;
			_cueSheet = cueSheet;
		}

		public string Name
		{
			get { return name; }
		}

		public System.IO.Stream ReadStream
		{
			get { return _cueSheet.OpenArchive(Name, true); }
		}

		public System.IO.Stream WriteStream
		{
			get { return null; }
		}

		public void CloseStream(System.IO.Stream stream)
		{
			stream.Close();
		}
	}

	public class CUELine {
		private List<String> _params;
		private List<bool> _quoted;

		public CUELine() {
			_params = new List<string>();
			_quoted = new List<bool>();
		}

		public CUELine(string line) {
			int start, end, lineLen;
			bool isQuoted;

			_params = new List<string>();
			_quoted = new List<bool>();

			start = 0;
			lineLen = line.Length;

			while (true) {
				while ((start < lineLen) && (line[start] == ' ')) {
					start++;
				}
				if (start >= lineLen) {
					break;
				}

				isQuoted = (line[start] == '"');
				if (isQuoted) {
					start++;
				}

				end = line.IndexOf(isQuoted ? '"' : ' ', start);
				if (end == -1) {
					end = lineLen;
				}

				_params.Add(line.Substring(start, end - start));
				_quoted.Add(isQuoted);

				start = isQuoted ? end + 1 : end;
			}
		}

		public List<string> Params {
			get {
				return _params;
			}
		}

		public List<bool> IsQuoted {
			get {
				return _quoted;
			}
		}

		public override string ToString() {
			if (_params.Count != _quoted.Count) {
				throw new Exception("Parameter and IsQuoted lists must match.");
			}

			StringBuilder sb = new StringBuilder();
			int last = _params.Count - 1;

			for (int i = 0; i <= last; i++) {
				if (_quoted[i] || _params[i].Contains(" ")) sb.Append('"');
				sb.Append(_params[i].Replace('"', '\''));
				if (_quoted[i] || _params[i].Contains(" ")) sb.Append('"');
				if (i < last) sb.Append(' ');
			}

			return sb.ToString();
		}
	}

	public class TrackInfo {
		private List<CUELine> _attributes;
		private int _peakLevel;
		public TagLib.File _fileInfo;

		public TrackInfo() {
			_attributes = new List<CUELine>();
			_fileInfo = null;
			_peakLevel = 0;
		}

		public unsafe void MeasurePeakLevel(int[,] samplesBuffer, int sampleCount)
		{
			fixed (int* s = samplesBuffer)
			{
				for (int i = 0; i < sampleCount * 2; i++)
					_peakLevel = Math.Max(_peakLevel, Math.Abs(s[i]));
			}
			//for (uint i = 0; i < sampleCount; i++)
			//    for (uint j = 0; j < 2; j++)
			//        if (_peakLevel < Math.Abs(samplesBuffer[i, j]))
			//            _peakLevel = Math.Abs(samplesBuffer[i, j]);
		}

		public int PeakLevel
		{
			get
			{
				return _peakLevel;
			}
		}

		public List<CUELine> Attributes {
			get {
				return _attributes;
			}
		}

		public string Artist {
			get {
				CUELine line = General.FindCUELine(_attributes, "PERFORMER");
				return (line == null || line.Params.Count < 2) ? String.Empty : line.Params[1];
			}
			set
			{
				General.SetCUELine(_attributes, "PERFORMER", value, true);
			}
		}

		public string Title {
			get {
				CUELine line = General.FindCUELine(_attributes, "TITLE");
				return (line == null || line.Params.Count < 2) ? String.Empty : line.Params[1];
			}
			set
			{
				General.SetCUELine(_attributes, "TITLE", value, true);
			}
		}
	}

	struct IndexInfo {
		public int Track;
		public int Index;
		public int Time;
	}

	struct SourceInfo {
		public string Path;
		public uint Offset;
		public uint Length;
	}

	public class StopException : Exception {
		public StopException() : base() {
		}
	}
}
