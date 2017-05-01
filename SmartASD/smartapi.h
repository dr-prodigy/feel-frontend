// PICERNO'S VERSION
// =================
#include <stdint.h>

#ifndef _DLL_H_
#define _DLL_H_

struct smart_device;
typedef struct smart_device *smart_dev;

struct smart_info
{
	smart_dev device;
	struct smart_info *next;
};

struct smart_input
{
	int32_t e1a:1, e1b:1, e2a:1, e2b:1, d1:1, d2:1, d3:1, d4:1, d5:1, d6:1, d7:1, d8:1, d9:1, d10:1, d11:1, d12:1;
	int32_t e3a:1, e3b:1, e4a:1, e4b:1, d13:1, d14:1, d15:1, d16:1, d17:1, d18:1, d19:1, d20:1, d21:1, d22:1, d23:1, d24:1;
	int16_t enc1, enc2, enc3, enc4;
	int16_t a1, a2, a3, a4, a5, a6, a7, a8;
};

struct smart_info* SmartScan();
wchar_t* SmartName(smart_dev device);
int32_t SmartOpen(smart_dev device);
void SmartClose(smart_dev device);
int32_t SmartSetAll(smart_dev device, uint32_t state, uint32_t mask);
int32_t SmartSetSingle(smart_dev device, int32_t channel, int32_t state);
int32_t SmartGetAll(smart_dev device, struct smart_input *state, int32_t timeout);

#endif // _DLL_H_

// MY VERSION
// ==========
#include <stdint.h>

struct smart_device;
typedef struct smart_device *smart_dev;

struct smart_info
{
	smart_dev device;
	struct smart_info *next;
};

struct smart_input
{
	int e1a:1, e1b:1, e2a:1, e2b:1, d1:1, d2:1, d3:1, d4:1, d5:1, d6:1, d7:1, d8:1, d9:1, d10:1, d11:1, d12:1;
	int e3a:1, e3b:1, e4a:1, e4b:1, d13:1, d14:1, d15:1, d16:1, d17:1, d18:1, d19:1, d20:1, d21:1, d22:1, d23:1, d24:1;
	short enc1, enc2, enc3, enc4;
	short a1, a2, a3, a4, a5, a6, a7, a8;
};

struct smart_info* SmartScan();
wchar_t* SmartName(smart_dev device);
int SmartOpen(smart_dev device);
void SmartClose(smart_dev device);
int SmartSetAll(smart_dev device, uint state, uint mask);
int SmartSetSingle(smart_dev device, int channel, int state);
int SmartGetAll(smart_dev device, struct smart_input *state, int timeout);