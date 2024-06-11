// Copyright © 2024 ema
//
// This file is part of QuickLook program.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace QuickLook.Plugin.HashViewer;

internal sealed class HeaderReader
{
    private static long OffDosHeader = 0;
    private static long OffFileHeader = 0;
    private static long OffOptHeader = 0;
    private static long OffSetHeader = 0;

    [StructLayout(LayoutKind.Sequential)]
    struct IMAGE_DOS_HEADER
    {
        public ushort e_magic;
        public ushort e_cblp;
        public ushort e_cp;
        public ushort e_crlc;
        public ushort e_cparhdr;
        public ushort e_minalloc;
        public ushort e_maxalloc;
        public ushort e_ss;
        public ushort e_sp;
        public ushort e_csum;
        public ushort e_ip;
        public ushort e_cs;
        public ushort e_lfarlc;
        public ushort e_ovno;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ushort[] e_res1;

        public ushort e_oemid;
        public ushort e_oeminfo;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public ushort[] e_res2;

        public int e_lfanew;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct IMAGE_FILE_HEADER
    {
        public ushort Machine;
        public ushort NumberOfSections;
        public uint TimeDateStamp;
        public uint PointerToSymbolTable;
        public uint NumberOfSymbols;
        public ushort SizeOfOptionalHeader;
        public ushort Characteristics;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct IMAGE_OPTIONAL_HEADER
    {
        public ushort Magic;
        public byte MajorLinkerVersion;
        public byte MinorLinkerVersion;
        public uint SizeOfCode;
        public uint SizeOfInitializedData;
        public uint SizeOfUninitializedData;
        public uint AddressOfEntryPoint;
        public uint BaseOfCode;
        public uint BaseOfData;
        public uint ImageBase;
        public uint SectionAlignment;
        public uint FileAlignment;
        public ushort MajorOperatingSystemVersion;
        public ushort MinorOperatingSystemVersion;
        public ushort MajorImageVersion;
        public ushort MinorImageVersion;
        public ushort MajorSubsystemVersion;
        public ushort MinorSubsystemVersion;
        public uint Reserved1;
        public uint SizeOfImage;
        public uint SizeOfHeaders;
        public uint CheckSum;
        public ushort Subsystem;
        public ushort DllCharacteristics;
        public uint SizeOfStackReserve;
        public uint SizeOfStackCommit;
        public uint SizeOfHeapReserve;
        public uint SizeOfHeapCommit;
        public uint LoaderFlags;
        public uint NumberOfRvaAndSizes;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public uint[] DataDirectory;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct IMAGE_SECTION_HEADER
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] Name;

        public uint VirtualAddress;
        public uint SizeOfRawData;
        public uint PointerToRawData;
        public uint PointerToRelocations;
        public uint PointerToLinenumbers;
        public ushort NumberOfRelocations;
        public ushort NumberOfLinenumbers;
        public uint Characteristics;
    }

    static void CalculateOffset(FileStream fs)
    {
        var dosheader = ReadStruct<IMAGE_DOS_HEADER>(fs);
        OffDosHeader = 0;
        OffFileHeader = dosheader.e_lfanew + 4;
        OffOptHeader = OffFileHeader + 0x14;
        OffSetHeader = OffOptHeader + Marshal.SizeOf<IMAGE_OPTIONAL_HEADER>();

        fs.Seek(0, SeekOrigin.Begin);
    }

    static T ReadStruct<T>(FileStream fs) where T : struct
    {
        var buffer = new byte[Marshal.SizeOf<T>()];
        fs.Read(buffer, 0, buffer.Length);
        var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        var result = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        handle.Free();
        return result;
    }

    class DosHeader
    {
        private IMAGE_DOS_HEADER dosheader;
        private FileStream fs;

        public DosHeader(FileStream f)
        {
            fs = f;
            fs.Seek(OffDosHeader, SeekOrigin.Begin);
            dosheader = ReadStruct<IMAGE_DOS_HEADER>(fs);
        }

        public string ShowHeader()
        {
            StringBuilder sb = new();

            sb.AppendLine("\n------------------DOS HEADER INFO--------------------");
            sb.AppendLine($"Magic number: {dosheader.e_magic:X}");
            sb.AppendLine($"Bytes on last page of file: {dosheader.e_cblp}");
            sb.AppendLine($"Pages in file: {dosheader.e_cp}");
            sb.AppendLine($"Relocation: {dosheader.e_crlc}");
            sb.AppendLine($"Size of header in paragraphs: {dosheader.e_cparhdr}");
            sb.AppendLine($"Minimum extra paragraph needed: {dosheader.e_minalloc}");
            sb.AppendLine($"Initial(relative) SS value: {dosheader.e_ss}");
            sb.AppendLine($"Initial SP value: {dosheader.e_sp}");
            sb.AppendLine($"Checksum: {dosheader.e_csum}");
            sb.AppendLine($"Initial IP value: {dosheader.e_ip}");
            sb.AppendLine($"Initial(relative) CS value: {dosheader.e_cs}");
            sb.AppendLine($"File address of relocation table: {dosheader.e_lfarlc}");
            sb.AppendLine($"Overlay number: {dosheader.e_ovno}");
            sb.AppendLine($"OEM identifier: {dosheader.e_oemid}");
            sb.AppendLine($"OEM information(e_oemid specific): {dosheader.e_oeminfo}");
            sb.AppendLine($"RVA address of PE header: {dosheader.e_lfanew}");

            return sb.ToString();
        }
    }

    class FileHeader
    {
        private IMAGE_FILE_HEADER fileHeader;
        private FileStream fs;

        public FileHeader(FileStream f)
        {
            fs = f;
            fs.Seek(OffFileHeader, SeekOrigin.Begin);
            fileHeader = ReadStruct<IMAGE_FILE_HEADER>(fs);
        }

        public string ShowHeader()
        {
            StringBuilder sb = new();

            sb.AppendLine("\n---------------FILE HEADER INFO------------");
            sb.AppendLine($"Machine: {ShowMachineType(fileHeader.Machine)}");
            sb.AppendLine($"Number Of Sections: {fileHeader.NumberOfSections}");
            sb.AppendLine($"Time Date Stamp: {fileHeader.TimeDateStamp}");
            sb.AppendLine($"Pointer to symbol table: {fileHeader.PointerToSymbolTable}");
            sb.AppendLine($"Number Of Symbols: {fileHeader.NumberOfSymbols}");
            sb.AppendLine($"Size Of Optional Header: {fileHeader.SizeOfOptionalHeader}");
            sb.AppendLine($"Characteristics: {fileHeader.Characteristics}");

            return sb.ToString();
        }

        private string ShowMachineType(ushort machine)
        {
            return machine switch
            {
                0x014c => "Architecture: x86 (32-bit)",
                0x8664 => ("Architecture: x64 (64-bit)"),
                0x0200 => ("Architecture: IA64 (Itanium)"),
                _ => ($"Unknown architecture: {machine:X}"),
            };
        }
    }

    class OptHeader
    {
        private IMAGE_OPTIONAL_HEADER optHeader;
        private FileStream fs;

        public OptHeader(FileStream f)
        {
            fs = f;
            fs.Seek(OffOptHeader, SeekOrigin.Begin);
            optHeader = ReadStruct<IMAGE_OPTIONAL_HEADER>(fs);
        }

        public string ShowHeader()
        {
            StringBuilder sb = new();

            sb.AppendLine("\n---------------OPTIONAL HEADER INFO------------");
            sb.AppendLine($"Magic: {optHeader.Magic}");
            sb.AppendLine($"Size Of Code: {optHeader.SizeOfCode}");
            sb.AppendLine($"Size Of Initialized Data: {optHeader.SizeOfInitializedData}");
            sb.AppendLine($"Size Of Uninitialized Data: {optHeader.SizeOfUninitializedData}");
            sb.AppendLine($"Address Of Entry Point: {optHeader.AddressOfEntryPoint}");
            sb.AppendLine($"Base Of Code: {optHeader.BaseOfCode}");
            sb.AppendLine($"Base Of Data: {optHeader.BaseOfData}");
            sb.AppendLine($"Image Base: {optHeader.ImageBase}");
            sb.AppendLine($"Section Alignment: {optHeader.SectionAlignment}");
            sb.AppendLine($"File Alignment: {optHeader.FileAlignment}");
            sb.AppendLine($"Major Operating System Version: {optHeader.MajorOperatingSystemVersion}");
            sb.AppendLine($"Minor Operating System Version: {optHeader.MinorOperatingSystemVersion}");
            sb.AppendLine($"Major Image Version: {optHeader.MajorImageVersion}");
            sb.AppendLine($"Minor Image Version: {optHeader.MinorImageVersion}");
            sb.AppendLine($"Major Subsystem Version: {optHeader.MajorSubsystemVersion}");
            sb.AppendLine($"Minor Subsystem Version: {optHeader.MinorSubsystemVersion}");
            sb.AppendLine($"Size Of Image: {optHeader.SizeOfImage}");
            sb.AppendLine($"Size Of Headers: {optHeader.SizeOfHeaders}");
            sb.AppendLine($"CheckSum: {optHeader.CheckSum}");
            sb.AppendLine($"Subsystem: {optHeader.Subsystem}");
            sb.AppendLine($"Dll Characteristics: {optHeader.DllCharacteristics}");
            sb.AppendLine($"Size Of Stack Reserve: {optHeader.SizeOfStackReserve}");
            sb.AppendLine($"Size Of Stack Commit: {optHeader.SizeOfStackCommit}");
            sb.AppendLine($"Size Of Heap Reserve: {optHeader.SizeOfHeapReserve}");
            sb.AppendLine($"Size Of Heap Commit: {optHeader.SizeOfHeapCommit}");
            sb.AppendLine($"Loader Flags: {optHeader.LoaderFlags}");
            sb.AppendLine($"Number Of Rva And Sizes: {optHeader.NumberOfRvaAndSizes}");

            return sb.ToString();
        }
    }

    class SecHeader
    {
        private IMAGE_SECTION_HEADER secHeader;
        private int NoOfSec;
        private FileStream fs;

        public SecHeader(FileStream f)
        {
            var fileHeader = new IMAGE_FILE_HEADER();
            fs = f;
            fs.Seek(OffFileHeader, SeekOrigin.Begin);
            fileHeader = ReadStruct<IMAGE_FILE_HEADER>(fs);
            NoOfSec = fileHeader.NumberOfSections;

            fs.Seek(OffSetHeader, SeekOrigin.Begin);
            secHeader = ReadStruct<IMAGE_SECTION_HEADER>(fs);
        }

        public string ShowHeader()
        {
            StringBuilder sb = new();

            sb.AppendLine("\n----------------SECTION HEADER INFO-----------");
            while (NoOfSec != 0)
            {
                sb.AppendLine($"Name: {System.Text.Encoding.ASCII.GetString(secHeader.Name)}");
                sb.AppendLine($"Virtual Address: {secHeader.VirtualAddress}");
                sb.AppendLine($"Size Of Raw Data: {secHeader.SizeOfRawData}");
                sb.AppendLine($"Pointer To Raw Data: {secHeader.PointerToRawData}");
                sb.AppendLine($"Pointer To Relocations: {secHeader.PointerToRelocations}");
                sb.AppendLine($"Pointer To Line numbers: {secHeader.PointerToLinenumbers}");
                sb.AppendLine($"Number Of Relocations: {secHeader.NumberOfRelocations}");
                sb.AppendLine($"Number Of Line numbers: {secHeader.NumberOfLinenumbers}");
                sb.AppendLine($"Characteristics: {secHeader.Characteristics}");
                NoOfSec--;
                sb.AppendLine("\n-----------------------------------------------------");
                secHeader = ReadStruct<IMAGE_SECTION_HEADER>(fs);
            }

            return sb.ToString();
        }
    }

    public static string GetHeaders(string fileName)
    {
        using FileStream fs = new(fileName, FileMode.Open, FileAccess.Read);

        CalculateOffset(fs);

        StringBuilder sb = new();

        {
            var dh = new DosHeader(fs);
            var header = dh.ShowHeader();
            sb.AppendLine(header);
        }
        {
            var dh = new FileHeader(fs);
            var header = dh.ShowHeader();
            sb.AppendLine(header);
        }
        {
            var dh = new OptHeader(fs);
            var header = dh.ShowHeader();
            sb.AppendLine(header);
        }
        {
            var dh = new SecHeader(fs);
            var header = dh.ShowHeader();
            sb.AppendLine(header);
        }

        return sb.ToString();
    }
}
