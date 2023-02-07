using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace vgmpcm_salvage
{
    class Program
    {
        /*  https://vgmrips.net/wiki/VGM_Specification
         * 
         *          00 	01 	02 	03 	    04 	05 	06 	07 	        08 	09 	0A 	0B 	        0C 	0D 	0E 	0F
            0x00 	"Vgm " ident 	    EoF offset 	            Version 	            SN76489 clock
            0x10 	YM2413 clock 	    GD3 offset 	            Total # samples 	    Loop offset
            0x20 	Loop # samples 	    Rate 	                SN FB  |SNW| SF 	    YM2612 clock
            0x30 	YM2151 clock 	    VGM data offset 	    Sega PCM clock 	        SPCM Interface
            0x40 	RF5C68 clock 	    YM2203 clock 	        YM2608 clock 	        YM2610/B clock
            0x50 	YM3812 clock 	    YM3526 clock 	        Y8950 clock 	        YMF262 clock
            0x60 	YMF278B clock 	    YMF271 clock 	        YMZ280B clock 	        RF5C164 clock
            0x70 	PWM clock 	        AY8910 clock 	        AYT| AY Flags 	        VM | * | LB| LM
            0x80 	GB DMG clock 	    NES APU clock 	        MultiPCM clock 	        uPD7759 clock
            0x90 	OKIM6258 clock 	    OF| KF| CF | *   	    OKIM6295 clock 	        K051649 clock
            0xA0 	K054539 clock 	    HuC6280 clock 	        C140 clock 	            K053260 clock
            0xB0 	Pokey clock 	    QSound clock 	        SCSP clock 	            Extra Hdr ofs
            0xC0 	WonderSwan clock 	VSU clock 	            SAA1099 clock 	        ES5503 clock
            0xD0 	ES5506 clock 	    ESchns| CD | * 	        X1-010 clock 	        C352 clock
            0xE0 	GA20 clock 	        * | * | *  | *          *  | * | * | *           * | * | * | *
            0xF0 	* | * | * | *       * | * | *  | *          *  | * | * | *           * | * | * | *
         */

        public struct VGMHeader
        {
            public Int32 indent;
            public Int32 EoF;
            public Int32 version;
            public Int32 sn76489Clock;
            public Int32 ym2413Clock;
            public Int32 gd3Offset;
            public Int32 totalSamples;
            public Int32 loopOffset;
            public Int32 loopNumSamples;
            public Int32 rate;
            public Int32 snX;
            public Int32 ym2612Clock;
            public Int32 ym2151Clock;
            public Int32 vgmDataOffset;
            public Int32 segaPCMClock;
            public Int32 spcmInterface;

            public void Reset()
            {
                indent = 0;
                EoF = 0;
                version = 0;
                sn76489Clock = 0;
                ym2413Clock = 0;
                gd3Offset = 0;
                totalSamples = 0;
                loopOffset = 0;
                loopNumSamples = 0;
                rate = 0;
                snX = 0;
                ym2612Clock = 0;
                ym2151Clock = 0;
                vgmDataOffset = 0;
                segaPCMClock = 0;
                spcmInterface = 0;
            }
        };

        public struct GD3
        {
            public Int32 version;
            public Int32 size;
            public String enTrackName;
            public String enGameName;
            public String enSystemName;
            public String enAuthor;
            public String releaseDate;

            public void Reset()
            {
                version = 0;
                size = 0;
                enTrackName = "";
                enGameName = "";
                enSystemName = "";
                enAuthor = "";
                releaseDate = "";
            }
        };
        static void Main(string[] args)
        {
            Console.WriteLine(" **********************************************************");
            Console.WriteLine(" * DPTP System, VGM PCM data salvage                      *");
            Console.WriteLine(" * Készítés dátuma: 2023.02.05, Érdliget                  *");
            Console.WriteLine(" * Készítő: Tóth Péter - don_peter@freemail.hu            *");
            Console.WriteLine(" **********************************************************");
            Console.WriteLine(" * Az alkalmazás a VGM fájlból kinyeri a PCM hangszereket *");
            Console.WriteLine(" * RAW vagy is nyers adatként és WAV audió fájlként is.   *");
            Console.WriteLine(" * Továbbá C programkódként és a PCM stream hangszer      *");
            Console.WriteLine(" * pointereket rendezett listában. Méret: (n+1)-n         *");
            Console.WriteLine(" *  Hz: megadható a PCM frekvenciája, alapesetben 2-es.   *");
            Console.WriteLine(" *  1 = 8000Hz                                            *");
            Console.WriteLine(" *  2 = 11025Hz                                           *");
            Console.WriteLine(" *  3 = 16000Hz                                           *");
            Console.WriteLine(" *  4 = 22050Hz                                           *");
            Console.WriteLine(" *  5 = 44100Hz                                           *");
            Console.WriteLine(" * Alkalmazás használata: vgmpcm_salvage filename.vgm ?   *");
            Console.WriteLine(" * Alapértelmezett beállítás 11025Hz                      *");
            Console.WriteLine(" **********************************************************");

/*#if DEBUG
            args = new[] { "m1.vgm" };
#endif*/
            if (args.Length != 0)
            {
                if (args.Length > 0)
                {
                    // Minden előzetes vizsgálaton áttment, jöhet a file betöltése és kommunikáció
                    Console.WriteLine(DateTime.Now.ToString() + " -> Fájl megnyitása...");
                    if (File.Exists(args[0]))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(DateTime.Now.ToString() + " -> Fájl megnyitása rendben...");
                        Console.ResetColor();

                        VGMHeader Header = new VGMHeader();
                        GD3 gd3 = new GD3();

                        byte[] buffer = new byte[1];
                        byte[] rows;
                        rows = System.IO.File.ReadAllBytes(args[0]);
                        byte[] rbuff = new byte[1];
                        int FileSize = rows.Length;
                        int offset = 0;
                        int NextEntry = 0;

                        Header.Reset();
                        gd3.Reset();

                        // 512 byte, VGM header adatok kinyerése
                        Header.indent = (rows[NextEntry] + (rows[NextEntry + 1] << 8) + (rows[NextEntry + 2] << 16) + (rows[NextEntry + 3] << 24));
                        NextEntry += 4;
                        Header.EoF = (rows[NextEntry] + (rows[NextEntry + 1] << 8) + (rows[NextEntry + 2] << 16) + (rows[NextEntry + 3] << 24));
                        NextEntry += 4;
                        Header.version = (rows[NextEntry] + (rows[NextEntry + 1] << 8) + (rows[NextEntry + 2] << 16) + (rows[NextEntry + 3] << 24));
                        NextEntry += 4;
                        Header.sn76489Clock = (rows[NextEntry] + (rows[NextEntry + 1] << 8) + (rows[NextEntry + 2] << 16) + (rows[NextEntry + 3] << 24));
                        NextEntry += 4;
                        Header.ym2413Clock = (rows[NextEntry] + (rows[NextEntry + 1] << 8) + (rows[NextEntry + 2] << 16) + (rows[NextEntry + 3] << 24));
                        NextEntry += 4;
                        Header.gd3Offset = (rows[NextEntry] + (rows[NextEntry + 1] << 8) + (rows[NextEntry + 2] << 16) + (rows[NextEntry + 3] << 24));
                        NextEntry += 4;
                        Header.totalSamples = (rows[NextEntry] + (rows[NextEntry + 1] << 8) + (rows[NextEntry + 2] << 16) + (rows[NextEntry + 3] << 24));
                        NextEntry += 4;
                        Header.loopOffset = (rows[NextEntry] + (rows[NextEntry + 1] << 8) + (rows[NextEntry + 2] << 16) + (rows[NextEntry + 3] << 24));
                        NextEntry += 4;
                        Header.loopNumSamples = (rows[NextEntry] + (rows[NextEntry + 1] << 8) + (rows[NextEntry + 2] << 16) + (rows[NextEntry + 3] << 24));
                        NextEntry += 4;
                        Header.rate = (rows[NextEntry] + (rows[NextEntry + 1] << 8) + (rows[NextEntry + 2] << 16) + (rows[NextEntry + 3] << 24));
                        NextEntry += 4;
                        Header.snX = (rows[NextEntry] + (rows[NextEntry + 1] << 8) + (rows[NextEntry + 2] << 16) + (rows[NextEntry + 3] << 24));
                        NextEntry += 4;
                        Header.ym2612Clock = (rows[NextEntry] + (rows[NextEntry + 1] << 8) + (rows[NextEntry + 2] << 16) + (rows[NextEntry + 3] << 24));
                        NextEntry += 4;
                        Header.ym2151Clock = (rows[NextEntry] + (rows[NextEntry + 1] << 8) + (rows[NextEntry + 2] << 16) + (rows[NextEntry + 3] << 24));
                        NextEntry += 4;
                        Header.vgmDataOffset = (rows[NextEntry] + (rows[NextEntry + 1] << 8) + (rows[NextEntry + 2] << 16) + (rows[NextEntry + 3] << 24));
                        NextEntry += 4;
                        Header.segaPCMClock = (rows[NextEntry] + (rows[NextEntry + 1] << 8) + (rows[NextEntry + 2] << 16) + (rows[NextEntry + 3] << 24));
                        NextEntry += 4;
                        Header.spcmInterface = (rows[NextEntry] + (rows[NextEntry + 1] << 8) + (rows[NextEntry + 2] << 16) + (rows[NextEntry + 3] << 24));
                        NextEntry += 4;

                        if (Header.vgmDataOffset < 0x40) Header.vgmDataOffset = 0x40;   // VGM kezdő címe


                        // Kiírás képernyőre is
                        /*Console.WriteLine("Fájl neve: {0}", args[0]);
                        Console.WriteLine("Header.indent: 0x{0} - {1}", Header.indent.ToString("X"), Convert.ToInt32(Header.indent));
                        Console.WriteLine("Header.EoF: 0x{0} - {1}", Header.EoF.ToString("X"), Convert.ToInt32(Header.EoF));
                        Console.WriteLine("Header.version: 0x{0} - {1}", Header.version.ToString("X"), Convert.ToInt32(Header.version));
                        Console.WriteLine("Header.sn76489Clock: 0x{0} - {1}", Header.sn76489Clock.ToString("X"), Convert.ToInt32(Header.sn76489Clock));
                        Console.WriteLine("Header.ym2413Clock: 0x{0} - {1}", Header.ym2413Clock.ToString("X"), Convert.ToInt32(Header.ym2413Clock));
                        Console.WriteLine("Header.gd3Offset: 0x{0} - {1}", Header.gd3Offset.ToString("X"), Convert.ToInt32(Header.gd3Offset));
                        Console.WriteLine("Header.totalSamples: 0x{0} - {1}", Header.totalSamples.ToString("X"), Convert.ToInt32(Header.totalSamples));
                        Console.WriteLine("Header.loopOffset: 0x{0} - {1}", Header.loopOffset.ToString("X"), Convert.ToInt32(Header.loopOffset));
                        Console.WriteLine("Header.loopNumSamples: 0x{0} - {1}", Header.loopNumSamples.ToString("X"), Convert.ToInt32(Header.loopNumSamples));
                        Console.WriteLine("Header.rate: 0x{0} - {1}", Header.rate.ToString("X"), Convert.ToInt32(Header.rate));
                        Console.WriteLine("Header.snX: 0x{0} - {1}", Header.snX.ToString("X"), Convert.ToInt32(Header.snX));
                        Console.WriteLine("Header.ym2612Clock: 0x{0} - {1}", Header.ym2612Clock.ToString("X"), Convert.ToInt32(Header.ym2612Clock));
                        Console.WriteLine("Header.ym2151Clock: 0x{0} - {1}", Header.ym2151Clock.ToString("X"), Convert.ToInt32(Header.ym2151Clock));
                        Console.WriteLine("Header.vgmDataOffset: 0x{0} - {1}", Header.vgmDataOffset.ToString("X"), Convert.ToInt32(Header.vgmDataOffset));
                        Console.WriteLine("Header.segaPCMClock: 0x{0} - {1}", Header.segaPCMClock.ToString("X"), Convert.ToInt32(Header.segaPCMClock));
                        Console.WriteLine("Header.spcmInterface: 0x{0} - {1}", Header.spcmInterface.ToString("X"), Convert.ToInt32(Header.spcmInterface));
                        */

                        Console.WriteLine("VGM version: 0x{0}", Header.version.ToString("X"));

                        // További adatok lekérdezése
                        int GD3Check = 0;
                        NextEntry = Header.gd3Offset + 0x14;    // 20 bájtot léptetünk a GD3 ellenőrzéséhez
                        for (int i = 0; i < 4; i++) GD3Check += rows[NextEntry++];

                        // VGM fájl ellenőrzése
                        if (GD3Check != 0xFE)
                        {
                            Console.WriteLine("Nincs GD3 információ!!");
                        }
                        gd3.version = (rows[NextEntry] + (rows[NextEntry + 1] << 8) + (rows[NextEntry + 2] << 16) + (rows[NextEntry + 3] << 24));
                        NextEntry += 4;
                        gd3.size = (rows[NextEntry] + (rows[NextEntry + 1] << 8) + (rows[NextEntry + 2] << 16) + (rows[NextEntry + 3] << 24));
                        NextEntry += 4;

                        char a, b;
                        int itemIndex = 0;
                        for (int i = 0; i < gd3.size && NextEntry < rows.Length - 1; i++)
                        {
                            a = Convert.ToChar(rows[NextEntry++]);
                            b = Convert.ToChar(rows[NextEntry++]);
                            if (a + b == 0) //Double 0 detected
                            {
                                itemIndex++;
                                continue;
                            }
                            switch (itemIndex)
                            {
                                case 0:
                                    gd3.enTrackName += a;
                                    break;
                                case 1:
                                    //JP TRACK NAME
                                    break;
                                case 2:
                                    gd3.enGameName += a;
                                    break;
                                case 3:
                                    //JP GAME NAME
                                    break;
                                case 4:
                                    gd3.enSystemName += a;
                                    break;
                                case 5:
                                    //JP SYSTEM NAME
                                    break;
                                case 6:
                                    gd3.enAuthor += a;
                                    break;
                                case 7:
                                    //JP AUTHOR
                                    break;
                                case 8:
                                    gd3.releaseDate += a;
                                    break;
                                default:
                                    //IGNORE CONVERTER NAME + NOTES
                                    break;
                            }
                        }

                        //Console.Write("\n\r");
                        Console.WriteLine("GD3 Információk:");
                        Console.WriteLine("GD3 Version: {0}", gd3.version);
                        Console.WriteLine("GD3 size: {0}", gd3.size);
                        Console.WriteLine("GD3 enTrackName: {0}", gd3.enTrackName);
                        Console.WriteLine("GD3 enGameName: {0}", gd3.enGameName);
                        Console.WriteLine("GD3 enSystemName: {0}", gd3.enSystemName);
                        Console.WriteLine("GD3 enAuthor: {0}", gd3.enAuthor);
                        Console.WriteLine("GD3 releaseDate: {0}", gd3.releaseDate);
                        int time = Header.totalSamples / 22050;
                        Console.WriteLine("GDM3 Time: {0:00}:{1:00}", (int)(time / 60), time % 60);

                        string path = @"SaveRAW";
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        string result;
                        string FileName = Path.GetFileNameWithoutExtension(args[0]);

                        UInt32 PCMDataSize = 0;
                        UInt32 PCMBuffIndex = 0;

                        // Első verzió: E0-nál írjuk ki az adatot
                        int hangszer = 1;
                        UInt32 CSi = 0, SDBi = 0;
                        //bool ChackSumFlag = false;
                        //UInt64 CheckSumNumber = 0;
                        //UInt64[] ChenckSum = new UInt64[100];
                        byte[] SampleDataBuff = new byte[(1024 * 1024)];   // 1MB minta tároló, mivel nem tudjuk mekkora lesz vagy lehet 1 minta mérete

                        // 2. verzió 0x52 0x2B 0x00-nál írjuk ki a PCM adatot
                        uint ShiftOffIndex = 0;
                        uint[] ShiftOffset = new uint[1000];
                        bool ShiftOffFlag = false;
                        uint OldOffset = 0, OldI = 0;

                        byte[] PCMDataBuff = new byte[10];
                        // PCM adatok, hangszerek kinyerése
                        for (int i = Header.vgmDataOffset; i < rows.Length; ++i)
                        {
                            switch (rows[i])
                            {
                                case 0x4F:  // GG
                                case 0x50:  // PSG
                                    i++;
                                    break;
                                case 0x52:  // YM2612
                                case 0x53:  // YM2612
                                    // Ha 52 2B 00 érkezik, vége a PCM adatnak                           
                                    i++;
                                    i++;
                                    break;
                                case 0x61:  // wait_n
                                    i++;
                                    i++;
                                    break;
                                case 0x62:  // wait 735
                                case 0x63:  // wait 882
                                    break;
                                case 0x66:  // vgm end
                                    i = rows.Length;
                                    break;
                                case 0x67:  // PCM data
                                    if(PCMDataSize!=0)
                                    {
                                        i = rows.Length;
                                        break;
                                    }
                                    i++; // Skip 0x66
                                    i++; // Skip data type
                                    for (int x = 0; x < 4; x++)
                                    {
                                        i++;
                                        PCMDataSize += ((UInt32)rows[i] << (8 * x));
                                    }
                                    Console.WriteLine("PCM mentése kész!!");
                                    Console.WriteLine("PCM data size: {0} byte", PCMDataSize);
                                    PCMDataBuff = new byte[PCMDataSize];
                                    for (UInt32 x = 0; x < PCMDataSize; x++)
                                    {
                                        i++;
                                        PCMDataBuff[x] = rows[i];
                                    }
                                    for (Int32 z = 0; z < ShiftOffset.Length; z++) ShiftOffset[z] = 0xFFFFFF;
                                    PCMBuffIndex = 0;
                                    break;
                                case 0x70:
                                case 0x71:
                                case 0x72:
                                case 0x73:
                                case 0x74:
                                case 0x75:
                                case 0x76:
                                case 0x77:
                                case 0x78:
                                case 0x79:
                                case 0x7A:
                                case 0x7B:
                                case 0x7C:
                                case 0x7D:
                                case 0x7E:
                                case 0x7F:  //wait = (0x7n & 0x0F) + 1
                                    break;
                                case 0x80:
                                case 0x81:
                                case 0x82:
                                case 0x83:
                                case 0x84:
                                case 0x85:
                                case 0x86:
                                case 0x87:
                                case 0x88:
                                case 0x89:
                                case 0x8A:
                                case 0x8B:
                                case 0x8C:
                                case 0x8D:
                                case 0x8E:
                                case 0x8F:  //wait = 0x8n & 0x0F
                                    PCMBuffIndex++;
                                    break;
                                case 0x90:
                                case 0x91:
                                case 0x92:
                                case 0x93:
                                    break;
                                case 0xE0:  // PCM Start Address
                                    PCMBuffIndex = 0;
                                    for (int x = 0; x < 4; x++)
                                    {
                                        i++;
                                        PCMBuffIndex += ((UInt32)rows[i] << (8 * x));
                                    }

                                    // Eltolások ellenörzése, hogy volt e már ilyen eltárolva
                                    for (int x = 0; x < 100; x++)
                                    {
                                        if ((ShiftOffset[x] == PCMBuffIndex))
                                        {
                                            ShiftOffFlag = true;
                                            if (ShiftOffIndex > OldI) ShiftOffIndex--;
                                            break;
                                        }

                                    }

                                    // Eltolások eltárolása
                                    if (OldOffset != PCMBuffIndex && !ShiftOffFlag)
                                    {
                                        ShiftOffset[ShiftOffIndex++] = PCMBuffIndex;
                                        OldI++;
                                    }
                                    OldOffset = PCMBuffIndex;
                                    ShiftOffFlag = false;

                                    break;
                                default:
                                    break;
                            }

                        }

                        uint Hz;
                        string Select;
                        Select = args.Length > 1 ? args[1] : "0";
                        switch (Select)
                        {
                            case "1":
                                Hz = 8000;
                                break;
                            case "2":
                                Hz = 11025;
                                break;
                            case "3":
                                Hz = 16000;
                                break;
                            case "4":
                                Hz = 22050;
                                break;
                            case "5":
                                Hz = 44100;
                                break;
                            default:
                                Hz = 11025;
                                break;
                        }
                        if (PCMDataSize > 0)
                        {
                            // Ha van PCM adat
                            uint WavIndex = 0;
                            Array.Sort(ShiftOffset);
                            uint q = 0;
                            for (q = 0; q < 100; q++) if (ShiftOffset[q] == 0x00FFFFFF) break;
                            for (uint e = 0; e < q; e++)
                            {
                                uint PCMSize = ShiftOffset[e + 1] > PCMDataSize ? PCMDataSize - ShiftOffset[e] : ShiftOffset[e + 1] - ShiftOffset[e];
                                byte[] PCMDataWav = new byte[PCMSize];
                                if (PCMSize > 200)
                                {
                                    FileStream pcmf = File.Create(path + "/" + FileName + "_" + hangszer + ".raw");
                                    for (uint w = ShiftOffset[e]; w < ShiftOffset[e] + PCMSize; w++)
                                    {
                                        pcmf.WriteByte(PCMDataBuff[w]);
                                        PCMDataWav[WavIndex++] = PCMDataBuff[w];
                                    }
                                    pcmf.Close();
                                    // WAV fájl létrehozása a teljes PCM adat stream-el.
                                    var wav = new WavePcmFormat(PCMDataWav, numChannels: 1, sampleRate: Hz, bitsPerSample: 8);
                                    var rawDataWithHeader = wav.ToBytesArray();
                                    pcmf = File.Create(path + "/" + FileName + "_" + hangszer + ".wav");
                                    for (Int32 y = 0; y < rawDataWithHeader.Length; y++)
                                    {
                                        pcmf.WriteByte(rawDataWithHeader[y]);
                                    }
                                    pcmf.Close();
                                    WavIndex = 0;
                                    hangszer++;
                                }
                            }


                            FileStream PCMOffPointer = new FileStream(path + "/" + FileName + "_offsets.txt", FileMode.Create);
                            StreamWriter wPCMOffPointer = new StreamWriter(PCMOffPointer);
                            for (Int32 y = 0; y < q; y++)
                            {
                                //Console.WriteLine(ShiftOffset[y].ToString());
                                wPCMOffPointer.Write(ShiftOffset[y].ToString() + "\n\r");
                            }
                            wPCMOffPointer.Close();

                            Console.WriteLine("Hangszerek száma: {0}", hangszer - 1);

                            // WAV fájl létrehozása a teljes PCM adat stream-el.
                            if ((hangszer - 1) == 0)
                            {
                                var wav = new WavePcmFormat(PCMDataBuff, numChannels: 1, sampleRate: Hz, bitsPerSample: 8);
                                var rawDataWithHeader = wav.ToBytesArray();

                                FileStream pcmf = File.Create(path + "/" + FileName + ".wav");
                                for (Int32 y = 0; y < rawDataWithHeader.Length; y++)
                                {
                                    pcmf.WriteByte(rawDataWithHeader[y]);
                                }
                                pcmf.Close();

                                // RAW kinyrése
                                FileStream pcmrawf = File.Create(path + "/" + FileName + "_" + hangszer + ".raw");
                                for (uint c = 0; c < PCMDataBuff.Length; c++)
                                {
                                    pcmrawf.WriteByte(PCMDataBuff[c]);
                                }
                                pcmrawf.Close();
                                Console.WriteLine("Teljes PCM adat kinyerése: {0}", path + "/" + FileName + ".wav/raw");
                            }


                            // PCM adat blok mérete: 0x67 0x66 tt ss ss ss ss (data)
                            for (int i = 0; i < rows.Length; ++i)
                            {
                                if (rows[i] == 0x67 && rows[(i + 1)] == 0x66)
                                {
                                    // van PCM adat, ki kell írni.
                                    FileStream fc = new FileStream(path + "/" + FileName + "_out.txt", FileMode.Create);
                                    StreamWriter wf = new StreamWriter(fc);
                                    PCMDataSize = rows[(i + 6)];
                                    PCMDataSize = (PCMDataSize << 8) | rows[(i + 5)];
                                    PCMDataSize = (PCMDataSize << 8) | rows[(i + 4)];
                                    PCMDataSize = (PCMDataSize << 8) | rows[(i + 3)];
                                    wf.Write("const rom unsigned char PCM[" + PCMDataSize + "] = {");
                                    for (int x = 0; x < PCMDataSize - 1; ++x)
                                    {
                                        wf.Write("0x" + rows[((i + 7) + x)].ToString("X") + ", ");
                                        if (x % 64 == 0) wf.Write("\n\r");
                                    }
                                    wf.Write("};//utolsó vesszőt töröld!\n\r");
                                    wf.Close();
                                    i = rows.Length;
                                    Console.WriteLine("PCM mentése kész!!");
                                    Console.WriteLine("PCM data size: {0} byte", PCMDataSize);
                                }
                            }

                            if (Header.version >= 0x160)
                            {
                                Console.WriteLine("PCM Stream start - 0x93-as parancsok...");
                                FileStream fcmd93 = new FileStream(path + "/" + FileName + "_CMD93_out.txt", FileMode.Create);
                                StreamWriter wfcmd93 = new StreamWriter(fcmd93);
                                bool Flag93 = false;
                                for (int i = 0; i < rows.Length; ++i)
                                {
                                    if ((rows[i] == 0x90 && rows[(i + 5)] == 0x91 && rows[(i + 10)] == 0x92) || Flag93)
                                    {
                                        Flag93 = true;
                                        if (rows[i] == 0x93)
                                        {
                                            for (int x = 0; x <= 10; x++)
                                            {
                                                if (x == 0) wfcmd93.Write("\n\r");
                                                wfcmd93.Write(rows[i++].ToString("X2") + "|");
                                            }
                                            i--;
                                        }

                                    }
                                }
                                wfcmd93.Close();
                                if (Flag93)
                                    Console.WriteLine("PCM Stream kész..");
                                else
                                    Console.WriteLine("Nincs PCM Stream. (VGM fájl veriója kisebb mint 1.60)");
                            }
                        }
                        else
                            Console.WriteLine("Nincs PCM adat!");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(DateTime.Now.ToString() + " -> Nem létezik a megadott fájl..(" + args[0] + ")");
                        Console.ResetColor();
                    }
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(DateTime.Now.ToString() + " -> A fájl nevét kötelező megadni, pl: könyvtár/fájl.bin");
                Console.ResetColor();
            }
                     
            Console.WriteLine("\n\nKilépéshez nyomj egy gombot...");
            Console.ReadKey();
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class WavePcmFormat
    {
        /* ChunkID          Contains the letters "RIFF" in ASCII form */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private char[] chunkID = new char[] { 'R', 'I', 'F', 'F' };

        /* ChunkSize        36 + SubChunk2Size */
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        private uint chunkSize = 0;

        /* Format           The "WAVE" format name */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private char[] format = new char[] { 'W', 'A', 'V', 'E' };

        /* Subchunk1ID      Contains the letters "fmt " */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private char[] subchunk1ID = new char[] { 'f', 'm', 't', ' ' };

        /* Subchunk1Size    16 for PCM */
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        private uint subchunk1Size = 16;

        /* AudioFormat      PCM = 1 (i.e. Linear quantization) */
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        private ushort audioFormat = 1;

        /* NumChannels      Mono = 1, Stereo = 2, etc. */
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        private ushort numChannels = 1;
        public ushort NumChannels { get => numChannels; set => numChannels = value; }

        /* SampleRate       8000, 44100, etc. */
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        private uint sampleRate = 44100;
        public uint SampleRate { get => sampleRate; set => sampleRate = value; }

        /* ByteRate         == SampleRate * NumChannels * BitsPerSample/8 */
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        private uint byteRate = 0;

        /* BlockAlign       == NumChannels * BitsPerSample/8 */
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        private ushort blockAlign = 0;

        /* BitsPerSample    8 bits = 8, 16 bits = 16, etc. */
        [MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        private ushort bitsPerSample = 16;
        public ushort BitsPerSample { get => bitsPerSample; set => bitsPerSample = value; }

        /* Subchunk2ID      Contains the letters "data" */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private char[] subchunk2ID = new char[] { 'd', 'a', 't', 'a' };

        /* Subchunk2Size    == NumSamples * NumChannels * BitsPerSample/8 */
        [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        private uint subchunk2Size = 0;

        /* Data             The actual sound data. */
        public byte[] Data { get; set; } = new byte[0];

        public WavePcmFormat(byte[] data, ushort numChannels = 2, uint sampleRate = 44100, ushort bitsPerSample = 16)
        {
            Data = data;
            NumChannels = numChannels;
            SampleRate = sampleRate;
            BitsPerSample = bitsPerSample;
        }

        private void CalculateSizes()
        {
            subchunk2Size = (uint)Data.Length;
            blockAlign = (ushort)(NumChannels * BitsPerSample / 8);
            byteRate = SampleRate * NumChannels * BitsPerSample / 8;
            chunkSize = 36 + subchunk2Size;
        }

        public byte[] ToBytesArray()
        {
            CalculateSizes();
            int headerSize = Marshal.SizeOf(this);
            IntPtr headerPtr = Marshal.AllocHGlobal(headerSize);
            Marshal.StructureToPtr(this, headerPtr, false);
            byte[] rawData = new byte[headerSize + Data.Length];
            Marshal.Copy(headerPtr, rawData, 0, headerSize);
            Marshal.FreeHGlobal(headerPtr);
            Array.Copy(Data, 0, rawData, 44, Data.Length);
            return rawData;
        }
    }
}
