# SEGA VGM PCM adat export RAW and WAV files
# DPTP System - 2023.02.07.
# [neo-geo.hu/SEGA VGM, PCM Sound Sample](http://neo-geo.hu/index.php?modul=news&news=43#nwe)

# Program indítása paraméterezése

- Program indítása

	`vgmpcm_salvage filename.vgm`
	
- a program alap állapotban indítva, annyit tesz, hogy 11KHz-es wav állományokat exportál

	`vgmpcm_salvage filename.vgm 1`
	
- a program 8KHz-es wav állományt exportál

	`vgmpcm_salvage filename.vgm 2`
	
- 11025Hz vagy is 11KHz, paraméter nélkül ez az alapértelmezett

	`vgmpcm_salvage filename.vgm 3`
	
- 16KHz

	`vgmpcm_salvage filename.vgm 4`
	
- 22050Hz vagy is 22KHz

	`vgmpcm_salvage filename.vgm 5`
	
- 44100Hz vagy is 44.1KHz

# Program hibái
- A program csak a SEGA VGM fájlokra lett optimalizálva, minden más esetben, ha nem kompatibilis a fájl, akkor a program programhibára futhat.
Az esetleges hiba kezelésére érdemes kivételkezelést beiktatni, főként a fájlkezeléseknél. Try Catch blokkok vagy IF() feltétel ágak.
Későbbiek során lehet, hogy ezeket pótolom, de addigis figyelni kell a kompatibilitásra. Neo Geo VGM fájlok esetén, csak az első digitális
hangot exportálja a program.

![alt text](http://neo-geo.hu/news/don_peter/new43/wav.png "Wav sample" center)

# Program működése
- Program indulásnál (lásd: feljebb az indítási paramétereket) ellenőrzi, hogy létezik e a fájl, ha igen, akkor megnyitja majd ezt követően
a VGM header és GD3 adatokat kiveszi és a program strukturába rendezi, majd ki írja a főb információkat. A program kiolvassa a VGM offset címét, 
majd ellenörzi, hogy helyes e. Ha igen akkor ennek megfelelően oda ugrik. (0x40 vagy 0x80)
Továbbiakban a VGM parancsértelmező kezd el futni, a program csak a 0x67-es és 0xE0 parancsokra helyez hangsúlyt.
- * 0x67-es parancs: Ez a parancs a PCM adat jelenlétét jelenti. Ha érkezik 0x67-es parancs akkor a program a teljes PCM adatot kimenti egy 
külön bufferbe. Későb ezt a buffert használja mikor az adatokat külön-külön meghatároza és exportálja.
- * 0xE0 parancsokra  aprogram egy táblát tölt fel a PCM adat kezdőcímével. Minden PCM mintának más és más címe van a memóriában, (jelen esetben a bufferben)
ezeket a címeket letárolja a program. Letárolás előtt kiszűi program, hogy 2 szer ugyan olyan címmel ne lehessen felvenni poziciót, mert az 
biztosan ugyan arra a hangmintára mutat mint ami már a táblákban van.
Ezt követően a program rendezi emelkedő sorrendbe a letárolt PCM pozició címeket, majd megkezdi egyesével a mintákat kigyűjteni majd exportálni 
RAW majd WAV fájlokba. Illetve elkészít egy C program kódot is a teljes PCM adattal és egy fájlt a pozició címekkel.

- Pozició címek esetében a PCM hossz a következő képen kapható vissza: PCMSize = (AddrOffset[(n+1)]-AddrOffset[(n)

# Program javításaival kapcsolatos történések
- Még nincs ilyen.