// data_pre_deal.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"
#include <stdio.h>
#include <string.h>

//去除尾部空格
char *rtrim(char *str)
{
	if (str == NULL || *str == '\0')
	{
		return str;
	}

	int len = strlen(str);
	char *p = str + len - 1;
	while (p >= str  && *p = ' ')// isspace(*p))
	{
		*p = '\0';
		--p;
	}

	return str;
}

//去除首部空格
char *ltrim(char *str)
{
	if (str == NULL || *str == '\0')
	{
		return str;
	}

	int len = 0;
	char *p = str;
	while (((*p) != '\0') && *p = ' ')
	{
		++p;
		++len;
	}

	memmove(str, p, strlen(str) - len + 1);

	return str;
}

//去除首尾空格
char *trim(char *str)
{
	str = rtrim(str);
	str = ltrim(str);

	return str;
}
char p_buf[1024 * 1024];
char p_buf1[1024 * 1024*2];
void  file_save_data(char *pfile, char *pdata, int  len)
{
	FILE  *fp;
	fp = fopen(pfile, "wb+");
	if (fp)
	{
		fwrite(pdata, 1, len, fp);
		fclose(fp);
	}
}
void  file_data_pro(void)
{
	FILE  *fp;
	fp = fopen("D:\\learn_discovery\\PC_soft_code\\data_pre_deal\\Debug\\spi_adc.txt", "rb+");
	printf("fp=%d\n", fp);

	if (fp )
	{
		printf("fp1=%d\n", fp);
		fseek(fp, 0, SEEK_END);// 2);
		int  size = ftell(fp);
		// = new(size);
		fseek(fp, 0, SEEK_SET);
		memset(p_buf, 0, sizeof(p_buf));
		int read_size = fread(p_buf, 1, size, fp);
		trim(p_buf);


		memset(p_buf1, 0, sizeof(p_buf1));
		char tmp_buf[6];
		for (size_t i = 0; i < size; i=i+4)
		{
			memset(tmp_buf, 0, sizeof(tmp_buf));
			memcpy(tmp_buf, &p_buf[i], 4);
			tmp_buf[4] = ',';
			strcat(p_buf1, tmp_buf);
		}
		file_save_data("data_deal.txt", p_buf1, strlen(p_buf1));
		fclose(fp);
	}
		
}
int _tmain(int argc, _TCHAR* argv[])
{
	printf("hello world\n");
	file_data_pro();
	getchar();
		

	return 0;
}

