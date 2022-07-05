using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure;
using Azure.Storage.Blobs;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using System.IO.Compression;

namespace Company.Function
{
	//method for generating the storage table name prefix
	//In Logic App Standard, we need to map the Logic App Name to the storage table name (LAName -> flowxxxxxflows)
	//DO NOT change anything in MurmurHash64 method
    public static class StoragePrefixGenerator
    {
        public static string Generate(string logicAppName)
        {
            byte[] data = Encoding.UTF8.GetBytes(logicAppName.ToLower());

            string hashResult = MurmurHash64(data, 0U).ToString("X");

            return TrimStorageKeyPrefix(hashResult, 32).ToLower();
        }

        
        private static string TrimStorageKeyPrefix(string storageKeyPrefix, int limit)
		{
			if (limit < 17)
			{
				throw new ArgumentException(string.Format("The storage key limit should be at least {0} characters.", 17), "limit");
			}
			if (storageKeyPrefix.Length <= limit - 17)
			{
				return storageKeyPrefix;
			}
			return storageKeyPrefix.Substring(0, limit - 17);
		}

        #region Hash Algorithm for LA table
        private static ulong MurmurHash64(byte[] data, uint seed = 0U)
		{
			int num = data.Length;
			uint num2 = seed;
			uint num3 = seed;
			int num4 = 0;
			while (num4 + 7 < num)
			{
				uint num5 = (uint)((int)data[num4] | (int)data[num4 + 1] << 8 | (int)data[num4 + 2] << 16 | (int)data[num4 + 3] << 24);
				uint num6 = (uint)((int)data[num4 + 4] | (int)data[num4 + 5] << 8 | (int)data[num4 + 6] << 16 | (int)data[num4 + 7] << 24);
				num5 *= 597399067U;
				num5 = RotateLeft32(num5, 15);
				num5 *= 2869860233U;
				num2 ^= num5;
				num2 = RotateLeft32(num2, 19);
				num2 += num3;
				num2 = num2 * 5U + 1444728091U;
				num6 *= 2869860233U;
				num6 = RotateLeft32(num6,17);
				num6 *= 597399067U;
				num3 ^= num6;
				num3 = RotateLeft32(num3, 13);
				num3 += num2;
				num3 = num3 * 5U + 197830471U;
				num4 += 8;
			}
			int num7 = num - num4;
			if (num7 > 0)
			{
				uint num8 = (uint)((num7 >= 4) ? ((int)data[num4] | (int)data[num4 + 1] << 8 | (int)data[num4 + 2] << 16 | (int)data[num4 + 3] << 24) : ((num7 == 3) ? ((int)data[num4] | (int)data[num4 + 1] << 8 | (int)data[num4 + 2] << 16) : ((num7 == 2) ? ((int)data[num4] | (int)data[num4 + 1] << 8) : ((int)data[num4]))));
				num8 *= 597399067U;
				num8 = RotateLeft32(num8, 15);
				num8 *= 2869860233U;
				num2 ^= num8;
				if (num7 > 4)
				{
					uint num9 = (uint)((num7 == 7) ? ((int)data[num4 + 4] | (int)data[num4 + 5] << 8 | (int)data[num4 + 6] << 16) : ((num7 == 6) ? ((int)data[num4 + 4] | (int)data[num4 + 5] << 8) : ((int)data[num4 + 4])));
					num9 *= 2869860233U;
					num9 = RotateLeft32(num9,17);
					num9 *= 597399067U;
					num3 ^= num9;
				}
			}
			num2 ^= (uint)num;
			num3 ^= (uint)num;
			num2 += num3;
			num3 += num2;
			num2 ^= num2 >> 16;
			num2 *= 2246822507U;
			num2 ^= num2 >> 13;
			num2 *= 3266489909U;
			num2 ^= num2 >> 16;
			num3 ^= num3 >> 16;
			num3 *= 2246822507U;
			num3 ^= num3 >> 13;
			num3 *= 3266489909U;
			num3 ^= num3 >> 16;
			num2 += num3;
			num3 += num2;
			return (ulong)num3 << 32 | (ulong)num2;
		}

		private static uint RotateLeft32(this uint value, int count)
		{
			return value << count | value >> 32 - count;
		}
        #endregion
    }
}