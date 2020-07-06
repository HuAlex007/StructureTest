using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ConsoleHasCode
{
    class Program
    {
        static void Main(string[] args)
        {
            //所有的服务器
            List<string> listServers = new List<string>() {
                "192.168.0.1","192.168.0.2","192.168.0.3",
                "192.168.0.4","192.168.0.5","192.168.0.6",
                "192.168.0.7","192.168.0.8","192.168.0.9","192.168.0.10"
            };

            //一致性hash算法类，实例类
            ConsHashWithVNode conHashClass = new ConsHashWithVNode(listServers);

            //string nodename = conHashClass.GetServer("4241681493");
            //Console.WriteLine("得到的节点为：" + nodename);

            //计算节点
            //String[] keys = { "太阳", "月亮", "星星", "木星" };
            //for (int i = 0; i < keys.Length; i++)
            //{
            //    byte[] digest = HashAlgorithm.computeMd5(keys[i]);
            //    long ll = HashAlgorithm.hash(digest, 0);
            //    Console.WriteLine("[" + keys[i] + "]的hash值为" + ll + ", 被路由到结点[" + conHashClass.GetServer(keys[i]) + "]");
            //}

            #region 字典存放，所有服务器上存放的cache值数据
            Dictionary<string, int> dicList = new Dictionary<string, int>();
            dicList.Add("192.168.0.1", 0);
            dicList.Add("192.168.0.2", 0);
            dicList.Add("192.168.0.3", 0);
            dicList.Add("192.168.0.4", 0);
            dicList.Add("192.168.0.5", 0);
            dicList.Add("192.168.0.6", 0);
            dicList.Add("192.168.0.7", 0);
            dicList.Add("192.168.0.8", 0);
            dicList.Add("192.168.0.9", 0);
            dicList.Add("192.168.0.10", 0);
            #endregion

            //测试100万kv数据
            for (int i = 0; i < 1000000; i++)
            {
                string testValue = "数据" + i;
                byte[] digest = HashAlgorithm.computeMd5(testValue);
                long longVal = HashAlgorithm.hash(digest, 0);//当前数据的hash值
                string serverName = conHashClass.GetServer(testValue);//匹配到的节点服务器名称
                //Console.WriteLine("[" + testValue + "]的hash值为" + longVal + ", 被路由到结点[" + serverName + "]");
                dicList[serverName] = dicList[serverName] + 1;//匹配到当前服务器时，缓存数据总量加1
            }

            //打印数据在服务器上的存放详情
            foreach (KeyValuePair<string, int> item in dicList) {
                Console.WriteLine("服务器["+ item.Key + "]存放的缓存数据总量为："+item.Value);
            }
            Console.ReadKey();
        }
    }

    #region 带虚拟节点的一致性hash算法类
    public class ConsHashWithVNode
    {
        // 存放服务器的Hash值(包含虚拟节点)
        private SortedList<long, string> serverHashList = new SortedList<long, string>();

        //虚拟节点数，每个真实节点指向为160个虚拟节点
        private int numReps = 160;

        //计算服务器的hash值，并放入serverHashList
        public ConsHashWithVNode(List<string> listServers/*,int nodeCopies*/)
        {
            serverHashList = new SortedList<long, string>();
            //numReps = nodeCopies;
            foreach (string node in listServers)
            {
                //每四个虚拟结点为一组
                for (int i = 0; i < numReps / 4; i++)
                {
                    //getKeyForNode方法为这组虚拟结点得到惟一名称 
                    byte[] digest = HashAlgorithm.computeMd5(node + i);
                    /** Md5是一个16字节长度的数组，将16字节的数组每四个字节一组，分别对应一个虚拟结点，这就是为什么上面把虚拟结点四个划分一组的原因*/
                    for (int h = 0; h < 4; h++)
                    {
                        long hashId = HashAlgorithm.hash(digest, h);
                        serverHashList[hashId] = node;
                        //Console.WriteLine("虚拟节点：" + node + "#" + i + h + " 加入集合中，其hash值为：" + hashId);
                    }
                }
            }
        }
        /// <summary>
        /// GetServer
        /// </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        public string GetServer(string k)
        {
            byte[] digest = HashAlgorithm.computeMd5(k);
            string rv = GetNodeForKey(HashAlgorithm.hash(digest, 0));
            return rv;
        }

        /// <summary>
        /// 获取当前Hashkey值匹配到的最近的服务器节点（正向循环）
        /// </summary>
        /// <param name="Hashkey"></param>
        /// <returns></returns>
        public string GetNodeForKey(long Hashkey)
        {
            //返回的真实的服务器节点名称
            string rv;
            //如果serverHashList，不包含key为Hashkey的键值对，则从hashList查询离它最近的服务器节点keyid
            if (!serverHashList.ContainsKey(Hashkey))
            {
                var tailMap = from coll in serverHashList
                              where coll.Key > Hashkey
                              select new { coll.Key };
                if (tailMap == null || tailMap.Count() == 0)
                {
                    Hashkey = serverHashList.FirstOrDefault().Key;//直接获取服务器列表的第一个node
                }
                else
                {
                    Hashkey = tailMap.FirstOrDefault().Key;//从匹配到的服务器节点列表tailMap,获取第一个node
                }
            }
            rv = serverHashList[Hashkey];
            return rv;
        }
    }
    #endregion

    #region  Hash算法类
    public class HashAlgorithm
    {
        //get hash
        public static long hash(byte[] digest, int nTime)
        {
            long rv = ((long)(digest[3 + nTime * 4] & 0xFF) << 24)
                    | ((long)(digest[2 + nTime * 4] & 0xFF) << 16)
                    | ((long)(digest[1 + nTime * 4] & 0xFF) << 8)
                    | ((long)digest[0 + nTime * 4] & 0xFF);
            return rv & 0xffffffffL; /* Truncate to 32-bits */
        }
        // Get the md5 of the given key.
        public static byte[] computeMd5(string k)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            byte[] keyBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(k));
            md5.Clear();
            //md5.update(keyBytes);
            //return md5.digest();
            return keyBytes;
        }
    }
    #endregion

}

