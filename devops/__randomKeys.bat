@echo OFF

REM This file is only needed to make sure we have a single SEED for creating RANDOM values. It avoids creating the same random numbers.
cmd /c "mkdir output" 2>NUL

(
	__random.bat 8 ./output/runid.temp

	__random.bat 16 ./output/passwordkey_16_1.temp
	__random.bat 16 ./output/passwordkey_16_2.temp
	__random.bat 16 ./output/passwordkey_16_3.temp
	
	__random.bat 24 ./output/passwordvalue_24_1.temp
	__random.bat 24 ./output/passwordvalue_24_2.temp
	__random.bat 24 ./output/passwordvalue_24_3.temp
)