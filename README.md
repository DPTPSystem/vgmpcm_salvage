# SEGA VGM PCM adat export RAW and WAV files
# DPTP System - 2023.02.07.
# don_peter@freemail.hu
# neo-geo.hu

# Program indítása paraméterezése
- vgmpcm_salvage filename.vgm
- * a program alap állapotban indítva, annyit tesz, hogy 11KHz-es wav állományokat exportál
- vgmpcm_salvage filename.vgm 1
- * a program 8KHz-es wav állományt exportál
- vgmpcm_salvage filename.vgm 2
- * 11025Hz vagy is 11KHz, paraméter nélkül ez az alapértelmezett
- vgmpcm_salvage filename.vgm 3
- * 16KHz
- vgmpcm_salvage filename.vgm 4
- * 22050Hz vagy is 22KHz
- vgmpcm_salvage filename.vgm 5
- * 44100Hz vagy is 44.1KHz

# Program hibái
- A program csak a SEGA VGM fájlokat ismeri, minden más esetben, ha nem kompatibilis a fájl, akkor a program programhibára futhat.

# Program működése
- Folyt köv...